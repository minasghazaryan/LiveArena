using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LLiveArenaWeb.Services;
using System.Text.Json;

namespace LLiveArenaWeb.Pages;

public class StandingsModel : PageModel
{
    private readonly ILiveEventsService _liveEventsService;
    private readonly ISportsDataService _sportsDataService;
    private readonly ILogger<StandingsModel> _logger;

    public StandingsModel(ILiveEventsService liveEventsService, ISportsDataService sportsDataService, ILogger<StandingsModel> logger)
    {
        _liveEventsService = liveEventsService;
        _sportsDataService = sportsDataService;
        _logger = logger;
    }

    public int SeasonId { get; private set; }
    public string? SeasonName { get; private set; }
    public int? LeagueId { get; private set; }
    public string? LeagueName { get; private set; }
    public string? LeagueLogo { get; private set; }
    public List<JsonElement> Standings { get; private set; } = new();
    public string? Error { get; private set; }

    public async Task<IActionResult> OnGetAsync(int seasonId, int? leagueId = null)
    {
        SeasonId = seasonId;
        LeagueId = leagueId;

        // If leagueId provided, get league info from sports-data.json
        if (LeagueId.HasValue)
        {
            var sportsData = await _sportsDataService.GetSportsDataAsync();
            var league = sportsData?.Leagues?.FirstOrDefault(l => l.Id == LeagueId.Value);
            if (league != null)
            {
                LeagueName = league.Name;
                LeagueLogo = league.Logo;
            }
        }

        // Fetch season info to get season name
        if (LeagueId.HasValue)
        {
            var seasonsResult = await _liveEventsService.GetLeagueSeasonsAsync(LeagueId.Value);
            if (seasonsResult.Success)
            {
                var season = seasonsResult.Seasons.FirstOrDefault(s => GetIntProperty(s, "id") == seasonId);
                if (season.ValueKind != JsonValueKind.Undefined && season.ValueKind != JsonValueKind.Null)
                {
                    SeasonName = GetStringProperty(season, "name", "slug");
                }
            }
        }

        // Fetch standings
        var standingsResult = await _liveEventsService.GetSeasonStandingsAsync(seasonId);
        if (standingsResult.Success)
        {
            Standings = standingsResult.Standings;
        }
        else
        {
            Error = standingsResult.Error ?? "Failed to load standings";
        }

        return Page();
    }

    // Helper methods for extracting data from JsonElement
    public string? GetStringProperty(JsonElement? element, params string[] propertyNames)
    {
        if (element == null) return null;
        var el = element.Value;
        
        foreach (var propName in propertyNames)
        {
            if (el.TryGetProperty(propName, out var prop) && prop.ValueKind == JsonValueKind.String)
                return prop.GetString();
        }
        return null;
    }

    public int? GetIntProperty(JsonElement? element, params string[] propertyNames)
    {
        if (element == null) return null;
        var el = element.Value;
        
        foreach (var propName in propertyNames)
        {
            if (el.TryGetProperty(propName, out var prop) && prop.ValueKind == JsonValueKind.Number)
                return prop.GetInt32();
        }
        return null;
    }

    public JsonElement? GetObjectProperty(JsonElement? element, params string[] propertyNames)
    {
        if (element == null) return null;
        var el = element.Value;
        
        foreach (var propName in propertyNames)
        {
            if (el.TryGetProperty(propName, out var prop) && prop.ValueKind == JsonValueKind.Object)
                return prop;
        }
        return null;
    }

    public List<JsonElement> GetArrayProperty(JsonElement? element, params string[] propertyNames)
    {
        var list = new List<JsonElement>();
        if (element == null) return list;
        var el = element.Value;
        
        foreach (var propName in propertyNames)
        {
            if (el.TryGetProperty(propName, out var prop) && prop.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in prop.EnumerateArray())
                    list.Add(item.Clone());
                break;
            }
        }
        return list;
    }
}
