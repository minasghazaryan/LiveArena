using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LLiveArenaWeb.Services;
using System.Text.Json;

namespace LLiveArenaWeb.Pages;

public class ChallengesModel : PageModel
{
    private readonly ILiveEventsService _liveEventsService;
    private readonly ISportsDataService _sportsDataService;
    private readonly ILogger<ChallengesModel> _logger;

    public ChallengesModel(ILiveEventsService liveEventsService, ISportsDataService sportsDataService, ILogger<ChallengesModel> logger)
    {
        _liveEventsService = liveEventsService;
        _sportsDataService = sportsDataService;
        _logger = logger;
    }

    public int LeagueId { get; private set; }
    public string? LeagueName { get; private set; }
    public string? LeagueLogo { get; private set; }
    public List<JsonElement> Challenges { get; private set; } = new();
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

        // Fetch challenges
        var challengesResult = await _liveEventsService.GetLeagueChallengesAsync(leagueId);
        if (challengesResult.Success)
        {
            Challenges = challengesResult.Challenges;
        }
        else
        {
            Error = challengesResult.Error ?? "Failed to load challenges";
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
