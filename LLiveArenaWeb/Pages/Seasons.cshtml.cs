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
            // Filter to current seasons only
            var currentYear = DateTime.Now.Year;
            var currentDate = DateTime.Now;
            Seasons = seasonsResult.Seasons
                .Where(s => IsCurrentSeason(s, currentDate))
                .OrderByDescending(s => {
                    var yearStart = GetIntProperty(s, "year_start") ?? 0;
                    return yearStart;
                })
                .ToList();
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

    private bool IsCurrentSeason(JsonElement season, DateTime currentDate)
    {
        var yearStart = GetIntProperty(season, "year_start");
        var yearEnd = GetIntProperty(season, "year_end");

        if (!yearStart.HasValue || !yearEnd.HasValue)
        {
            // If no year info, assume it's current if it exists
            return true;
        }

        // Season is current if current date falls within the season year range
        // For example, 2024-2025 season is current if we're in 2024 or 2025
        var currentYear = currentDate.Year;
        return currentYear >= yearStart.Value && currentYear <= yearEnd.Value;
    }
}
