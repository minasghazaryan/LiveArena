using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LLiveArenaWeb.Services;
using LLiveArenaWeb.Models;

namespace LLiveArenaWeb.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IMatchListService _matchListService;
    private readonly ILiveEventsService _liveEventsService;

    private const long ChampionsLeagueId = 7846996; // EUROPE CHAMPIONS LEAGUE
    private sealed record LeagueFilter(string Display, string[] Includes, string[] Excludes);

    private static readonly Dictionary<string, LeagueFilter> LeagueFilters = new(StringComparer.OrdinalIgnoreCase)
    {
        ["champions-league"] = new LeagueFilter("Champions League", new[] { "CHAMPIONS LEAGUE" }, Array.Empty<string>()),
        ["europa-league"] = new LeagueFilter("Europa League", new[] { "EUROPA" }, new[] { "CONFERENCE" }),
        ["conference-league"] = new LeagueFilter("Conference League", new[] { "CONFERENCE" }, Array.Empty<string>()),
        ["world-cup"] = new LeagueFilter("World Cup", new[] { "WORLD CUP" }, Array.Empty<string>()),
        ["euro"] = new LeagueFilter("Euro Cup", new[] { "EURO CUP", "EUROPEAN CHAMPIONSHIP", "EURO 20", "EURO 202" }, new[] { "EUROPA" }),
        ["copa-america"] = new LeagueFilter("Copa America", new[] { "COPA AMERICA" }, Array.Empty<string>())
    };

    public IndexModel(ILogger<IndexModel> logger, IMatchListService matchListService, ILiveEventsService liveEventsService)
    {
        _logger = logger;
        _matchListService = matchListService;
        _liveEventsService = liveEventsService;
    }

    public List<MatchListItem> PrematchMatches { get; set; } = new();
    public List<MatchListItem> LeagueMatches { get; set; } = new();
    public string? SelectedLeagueKey { get; set; }
    public string? SelectedLeagueDisplay { get; set; }
    public System.Text.Json.JsonElement[] LiveEvents { get; set; } = Array.Empty<System.Text.Json.JsonElement>();
    public string ActiveTab { get; set; } = "leagues";

    public async Task OnGetAsync(string? league = null, bool refresh = false, string? tab = null)
    {
        try
        {
            ActiveTab = tab ?? "leagues";
            
            if (ActiveTab == "live-events")
            {
                var liveEventsResult = await _liveEventsService.GetLiveEventsAsync(sportId: 1, page: 1);
                if (liveEventsResult.Success)
                {
                    LiveEvents = liveEventsResult.Events.ToArray();
                }
                else
                {
                    _logger.LogWarning("Failed to fetch live events: {Error}", liveEventsResult.Error);
                }
            }
            else
            {
                var categories = await _matchListService.GetMatchCategoriesAsync();
                SelectedLeagueKey = league;
                SelectedLeagueDisplay = league != null && LeagueFilters.TryGetValue(league, out var displayFilter)
                    ? displayFilter.Display
                    : null;

                var liveMatches = categories.Live;
                var prematchMatches = categories.Prematch;

                if (!string.IsNullOrWhiteSpace(league) && LeagueFilters.TryGetValue(league, out var leagueFilter))
                {
                    LeagueMatches = liveMatches
                        .Concat(prematchMatches)
                        .Where(m => IsMatchInLeague(m, leagueFilter))
                        .OrderByDescending(m => m.Iplay)
                        .ThenBy(m => m.Stime)
                        .ToList();
                }
                else
                {
                    PrematchMatches = prematchMatches
                        .OrderBy(m => m.Stime)
                        .Take(2)
                        .ToList();
                }

                if (!LeagueMatches.Any() && !PrematchMatches.Any())
                {
                    var matchListResponse = await _matchListService.GetMatchListAsync();
                    if (matchListResponse?.Success == true && matchListResponse.Data?.T1 != null)
                    {
                        var allMatches = matchListResponse.Data.T1;
                        _logger.LogInformation("No categorized matches found, showing general matches");
                        var fallbackMatches = allMatches
                            .OrderByDescending(m => m.Iplay)
                            .ThenBy(m => m.Stime)
                            .ToList();

                        if (!string.IsNullOrWhiteSpace(league) && LeagueFilters.TryGetValue(league, out var fallbackFilter))
                        {
                            LeagueMatches = fallbackMatches
                                .Where(m => IsMatchInLeague(m, fallbackFilter))
                                .ToList();
                        }
                        else
                        {
                            PrematchMatches = fallbackMatches.Take(2).ToList();
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching prematch matches");
            PrematchMatches = new List<MatchListItem>();
            LeagueMatches = new List<MatchListItem>();
        }
    }

    private static bool IsMatchInLeague(MatchListItem match, LeagueFilter filter)
    {
        if (string.IsNullOrWhiteSpace(match.Cname))
        {
            return false;
        }

        var competition = match.Cname.ToUpperInvariant();

        if (!filter.Includes.Any(include => competition.Contains(include, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        if (filter.Excludes.Any(exclude => competition.Contains(exclude, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        return true;
    }
}
