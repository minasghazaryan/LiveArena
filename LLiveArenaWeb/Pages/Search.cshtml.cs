using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LLiveArenaWeb.Services;
using LLiveArenaWeb.Models;
using System.Text.Json;

namespace LLiveArenaWeb.Pages;

public class SearchModel : PageModel
{
    private readonly ILiveEventsService _liveEventsService;
    private readonly ISportsDataService _sportsDataService;
    private readonly ILogger<SearchModel> _logger;

    public SearchModel(ILiveEventsService liveEventsService, ISportsDataService sportsDataService, ILogger<SearchModel> logger)
    {
        _liveEventsService = liveEventsService;
        _sportsDataService = sportsDataService;
        _logger = logger;
    }

    public List<JsonElement> SearchResults { get; set; } = new();
    public string? Error { get; set; }
    public EventSearchParams SearchParams { get; set; } = new();
    public Dictionary<string, string> TeamLogos { get; set; } = new();
    public Dictionary<string, string> LeagueLogos { get; set; } = new();
    public bool HasSearched { get; set; }
    public string? SearchName { get; set; }
    public DateTime? SearchDate { get; set; }
    public int SearchSportId { get; set; } = 1; // Default to football

    public async Task OnGetAsync(
        string? name = null,
        DateTime? date = null,
        int? sportId = null,
        int? leagueId = null,
        int? homeTeamId = null,
        int? awayTeamId = null,
        string? status = null,
        DateTime? dateStart = null,
        DateTime? dateEnd = null,
        int page = 1)
    {
        SearchName = name;
        SearchDate = date;
        SearchSportId = sportId ?? 1;

        // Priority: If name is provided, use the name-based search (more user-friendly)
        if (!string.IsNullOrWhiteSpace(name))
        {
            HasSearched = true;
            var result = await _liveEventsService.SearchEventsBySimilarNameAsync(name, date, SearchSportId, page);
            if (result.Success)
            {
                SearchResults = result.Events;
                // Filter by configured leagues
                await FilterBySportsDataLeaguesAsync();
            }
            else
            {
                Error = result.Error ?? "Failed to search events";
            }
        }
        // Otherwise, use the advanced search with IDs
        else if (sportId.HasValue || leagueId.HasValue || homeTeamId.HasValue || awayTeamId.HasValue || 
                 !string.IsNullOrEmpty(status) || dateStart.HasValue || dateEnd.HasValue)
        {
            HasSearched = true;
            SearchParams = new EventSearchParams
            {
                SportId = sportId,
                LeagueId = leagueId,
                HomeTeamId = homeTeamId,
                AwayTeamId = awayTeamId,
                Status = status,
                DateStart = dateStart,
                DateEnd = dateEnd,
                Page = page
            };

            var result = await _liveEventsService.SearchEventsAsync(SearchParams);
            if (result.Success)
            {
                SearchResults = result.Events;
                // Filter by configured leagues
                await FilterBySportsDataLeaguesAsync();
            }
            else
            {
                Error = result.Error ?? "Failed to search events";
            }
        }

        // Load logos for display
        await LoadLogosAsync();
    }

    private async Task FilterBySportsDataLeaguesAsync()
    {
        try
        {
            var sportsData = await _sportsDataService.GetSportsDataAsync();
            var allowedLeagueIds = sportsData.Leagues?.Select(l => l.Id).ToHashSet() ?? new HashSet<int>();

            if (allowedLeagueIds.Any())
            {
                // Filter events to only include those with league_id matching configured leagues
                SearchResults = SearchResults
                    .Where(evt =>
                    {
                        var evtLeagueId = GetIntProperty(evt, "league_id");
                        if (!evtLeagueId.HasValue)
                        {
                            // Try to get from nested league object
                            var league = GetObjectProperty(evt, "league", "tournament", "competition");
                            evtLeagueId = GetIntProperty(league, "id", "league_id");
                        }
                        return evtLeagueId.HasValue && allowedLeagueIds.Contains(evtLeagueId.Value);
                    })
                    .ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error filtering search results by sports data leagues");
            // Continue without filtering if there's an error
        }
    }

    private async Task LoadLogosAsync()
    {
        try
        {
            var sportsData = await _sportsDataService.GetSportsDataAsync();
            var leagues = sportsData.Leagues ?? new List<LeagueInfo>();
            
            foreach (var league in leagues)
            {
                if (!string.IsNullOrEmpty(league.Name) && !string.IsNullOrEmpty(league.Logo))
                {
                    LeagueLogos[league.Name] = league.Logo;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load league logos");
        }
    }

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

    public string GetEventStatus(JsonElement evt)
    {
        if (evt.TryGetProperty("status", out var st) && st.ValueKind == JsonValueKind.String)
        {
            var status = st.GetString() ?? "";
            if (status.Equals("inprogress", StringComparison.OrdinalIgnoreCase) || 
                status.Equals("live", StringComparison.OrdinalIgnoreCase))
                return "live";
            if (status.Equals("finished", StringComparison.OrdinalIgnoreCase) || 
                status.Equals("completed", StringComparison.OrdinalIgnoreCase))
                return "finished";
        }
        
        // Check if there's a score (indicates live or finished)
        if (evt.TryGetProperty("home_score", out var hs) && hs.ValueKind == JsonValueKind.Number &&
            evt.TryGetProperty("away_score", out var aws) && aws.ValueKind == JsonValueKind.Number)
        {
            return "live";
        }
        
        return "scheduled";
    }
}
