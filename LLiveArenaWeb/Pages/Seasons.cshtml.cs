using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LLiveArenaWeb.Services;
using System.Text.Json;

namespace LLiveArenaWeb.Pages;

public class SeasonsModel : PageModel
{
    private readonly ILiveEventsService _liveEventsService;
    private readonly ISportsDataService _sportsDataService;
    private readonly ILogger<SeasonsModel> _logger;

    public SeasonsModel(ILiveEventsService liveEventsService, ISportsDataService sportsDataService, ILogger<SeasonsModel> logger)
    {
        _liveEventsService = liveEventsService;
        _sportsDataService = sportsDataService;
        _logger = logger;
    }

    public int LeagueId { get; private set; }
    public string? LeagueName { get; private set; }
    public string? LeagueLogo { get; private set; }
    public List<JsonElement> Seasons { get; private set; } = new();
    public string? Error { get; private set; }

    public async Task<IActionResult> OnGetAsync(int leagueId)
    {
        LeagueId = leagueId;

        // Get league info from sports-data.json
        var sportsData = await _sportsDataService.GetSportsDataAsync();
        var league = sportsData?.Leagues?.FirstOrDefault(l => l.Id == leagueId);
        if (league != null)
        {
            LeagueName = league.Name;
            LeagueLogo = league.Logo;
        }

        // Fetch seasons
        var seasonsResult = await _liveEventsService.GetLeagueSeasonsAsync(leagueId);
        if (seasonsResult.Success)
        {
            Seasons = seasonsResult.Seasons.OrderByDescending(s => {
                var yearStart = GetIntProperty(s, "year_start") ?? 0;
                return yearStart;
            }).ToList();
        }
        else
        {
            Error = seasonsResult.Error ?? "Failed to load seasons";
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
}
