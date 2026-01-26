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
    private readonly ISportsDataService _sportsDataService;
    private readonly ITeamSearchService _teamSearchService;

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

    public IndexModel(ILogger<IndexModel> logger, IMatchListService matchListService, ILiveEventsService liveEventsService, ISportsDataService sportsDataService, ITeamSearchService teamSearchService)
    {
        _logger = logger;
        _matchListService = matchListService;
        _liveEventsService = liveEventsService;
        _sportsDataService = sportsDataService;
        _teamSearchService = teamSearchService;
    }

    public List<MatchListItem> PrematchMatches { get; set; } = new();
    public List<MatchListItem> LeagueMatches { get; set; } = new();
    public List<MatchListItem> TopMatches { get; set; } = new();
    public Dictionary<string, string> TeamLogos { get; set; } = new(); // team name -> logo URL
    public Dictionary<string, string> LeagueLogos { get; set; } = new(); // league name -> logo URL
    public Dictionary<int, string> LeagueNamesById { get; set; } = new(); // league id -> name
    public Dictionary<int, string> LeagueLogosById { get; set; } = new(); // league id -> logo URL
    public Dictionary<int, int> LeaguePriorityById { get; set; } = new();
    public List<LeagueInfo> AllLeagues { get; set; } = new();
    public string? SelectedLeagueKey { get; set; }
    public string? SelectedLeagueDisplay { get; set; }
    public int? SelectedLeagueId { get; set; }
    public System.Text.Json.JsonElement[] LiveEvents { get; set; } = Array.Empty<System.Text.Json.JsonElement>();
    public string ActiveTab { get; set; } = "leagues";

    public async Task OnGetAsync(string? league = null, bool refresh = false, string? tab = null, int? leagueId = null)
    {
        try
        {
            // If leagueId is provided, switch to live-events tab
            if (leagueId.HasValue)
            {
                ActiveTab = "live-events";
                SelectedLeagueId = leagueId;
            }
            else
            {
                ActiveTab = tab ?? "leagues";
            }
            
            // Fetch top matches from leagues in sports-data.json
            await LoadTopMatchesAsync();
            
            if (ActiveTab == "live-events")
            {
                var liveEventsResult = await _liveEventsService.GetLiveEventsAsync(sportId: 1, page: 1);
                if (liveEventsResult.Success)
                {
                    // Get league ids from sports-data.json to filter events
                    var sportsData = await _sportsDataService.GetSportsDataAsync();
                    var allowedLeagueIds = sportsData.Leagues?.Select(l => l.Id).ToHashSet() ?? new HashSet<int>();
                    
                    // Filter events to only include leagues from sports-data.json
                    var filteredEvents = new List<System.Text.Json.JsonElement>();
                    foreach (var evt in liveEventsResult.Events)
                    {
                        // Extract league id from event
                        System.Text.Json.JsonElement leagueElement = default;
                        if (evt.TryGetProperty("league", out var lg1)) leagueElement = lg1;
                        else if (evt.TryGetProperty("tournament", out var lg2)) leagueElement = lg2;
                        else if (evt.TryGetProperty("competition", out var lg3)) leagueElement = lg3;
                        
                        int? eventLeagueId = null;
                        if (leagueElement.ValueKind != System.Text.Json.JsonValueKind.Null && leagueElement.ValueKind != System.Text.Json.JsonValueKind.Undefined)
                        {
                            if (leagueElement.TryGetProperty("id", out var lid) && lid.ValueKind == System.Text.Json.JsonValueKind.Number)
                                eventLeagueId = lid.GetInt32();
                            else if (leagueElement.TryGetProperty("league_id", out var lid2) && lid2.ValueKind == System.Text.Json.JsonValueKind.Number)
                                eventLeagueId = lid2.GetInt32();
                            else if (leagueElement.TryGetProperty("tournament_id", out var tid) && tid.ValueKind == System.Text.Json.JsonValueKind.Number)
                                eventLeagueId = tid.GetInt32();
                        }

                        if (!eventLeagueId.HasValue)
                        {
                            if (evt.TryGetProperty("league_id", out var evtLid) && evtLid.ValueKind == System.Text.Json.JsonValueKind.Number)
                                eventLeagueId = evtLid.GetInt32();
                            else if (evt.TryGetProperty("tournament_id", out var evtTid) && evtTid.ValueKind == System.Text.Json.JsonValueKind.Number)
                                eventLeagueId = evtTid.GetInt32();
                        }
                        
                        // If a specific leagueId is selected, only show events from that league
                        if (SelectedLeagueId.HasValue)
                        {
                            if (eventLeagueId.HasValue && eventLeagueId.Value == SelectedLeagueId.Value)
                            {
                                filteredEvents.Add(evt);
                            }
                        }
                        // Otherwise, show all leagues from sports-data.json
                        else if (eventLeagueId.HasValue && allowedLeagueIds.Contains(eventLeagueId.Value))
                        {
                            filteredEvents.Add(evt);
                        }
                    }
                    
                    LiveEvents = filteredEvents.ToArray();
                    // Load team logos for live events
                    await LoadLiveEventsTeamLogosAsync(LiveEvents);
                    // Load league names/logos from sports-data.json
                    await LoadLeagueLogosAsync();
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

    private async Task LoadTopMatchesAsync()
    {
        try
        {
            // Get league names and logos from sports-data.json
            var sportsData = await _sportsDataService.GetSportsDataAsync();
            var leagues = sportsData.Leagues ?? new List<LeagueInfo>();
            AllLeagues = leagues
                .OrderBy(l => l.Priority ?? int.MaxValue)
                .ThenBy(l => l.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var leagueNames = leagues.Select(l => l.Name).ToList();
            var premierLeagueId = leagues.FirstOrDefault(l => l.Name.Equals("Premier League", StringComparison.OrdinalIgnoreCase))?.Id;
            var laLigaId = leagues.FirstOrDefault(l => l.Name.Equals("LaLiga", StringComparison.OrdinalIgnoreCase))?.Id;
            
            // Build league logo dictionary
            foreach (var league in leagues)
            {
                if (!string.IsNullOrWhiteSpace(league.Logo))
                {
                    LeagueLogos[league.Name] = league.Logo;
                }

                if (!LeaguePriorityById.ContainsKey(league.Id))
                {
                    LeaguePriorityById[league.Id] = league.Priority ?? int.MaxValue;
                }
            }
            
            if (!leagueNames.Any())
            {
                return;
            }

            // Get live matches
            var liveMatches = await _matchListService.GetLiveMatchesAsync();
            
            // Pick one live match from Premier League and LaLiga if available
            var topMatches = new List<MatchListItem>();
            if (premierLeagueId.HasValue)
            {
                var premierMatch = liveMatches
                    .Where(m => m.Iplay && m.Cid == premierLeagueId.Value)
                    .OrderBy(m => m.Stime)
                    .FirstOrDefault();
                if (premierMatch != null)
                    topMatches.Add(premierMatch);
            }
            if (laLigaId.HasValue)
            {
                var laLigaMatch = liveMatches
                    .Where(m => m.Iplay && m.Cid == laLigaId.Value)
                    .OrderBy(m => m.Stime)
                    .FirstOrDefault();
                if (laLigaMatch != null)
                    topMatches.Add(laLigaMatch);
            }

            // Fallback: fill remaining slots with other live matches from sports-data leagues
            if (topMatches.Count < 2)
            {
                var fallbackMatches = liveMatches
                    .Where(m => m.Iplay && !string.IsNullOrWhiteSpace(m.Cname) &&
                               leagueNames.Any(leagueName =>
                                   m.Cname.Contains(leagueName, StringComparison.OrdinalIgnoreCase) ||
                                   leagueName.Contains(m.Cname, StringComparison.OrdinalIgnoreCase)))
                    .OrderBy(m => m.Stime)
                    .Where(m => topMatches.All(existing => existing.Gmid != m.Gmid))
                    .Take(2 - topMatches.Count);

                topMatches.AddRange(fallbackMatches);
            }

            TopMatches = topMatches;
            
            // Fetch team logos for top matches
            await LoadTeamLogosAsync(topMatches);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading top matches");
            TopMatches = new List<MatchListItem>();
        }
    }

    private async Task LoadTeamLogosAsync(List<MatchListItem> matches)
    {
        try
        {
            var teamNames = new HashSet<string>();
            
            foreach (var match in matches)
            {
                var homeTeam = match.Section?.FirstOrDefault(s => s.Sno == 1);
                var awayTeam = match.Section?.FirstOrDefault(s => s.Sno == 3);
                
                if (!string.IsNullOrWhiteSpace(homeTeam?.Nat))
                    teamNames.Add(homeTeam.Nat);
                if (!string.IsNullOrWhiteSpace(awayTeam?.Nat))
                    teamNames.Add(awayTeam.Nat);
            }

            // Search for each team to get their logo
            foreach (var teamName in teamNames)
            {
                if (TeamLogos.ContainsKey(teamName))
                    continue; // Already fetched
                    
                try
                {
                    var searchParams = new TeamSearchParams
                    {
                        Name = teamName,
                        SportId = 1, // Football
                        Page = 1
                    };
                    
                    var result = await _teamSearchService.SearchTeamsAsync(searchParams);
                    
                    if (result.Success && result.Teams.Any())
                    {
                        // Try to find matching team and extract logo
                        foreach (var team in result.Teams)
                        {
                            var name = team.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : null;
                            var shortName = team.TryGetProperty("short_name", out var shortNameEl) ? shortNameEl.GetString() : null;
                            
                            if ((name != null && name.Contains(teamName, StringComparison.OrdinalIgnoreCase)) ||
                                (shortName != null && shortName.Contains(teamName, StringComparison.OrdinalIgnoreCase)) ||
                                (teamName.Contains(name ?? "", StringComparison.OrdinalIgnoreCase)) ||
                                (teamName.Contains(shortName ?? "", StringComparison.OrdinalIgnoreCase)))
                            {
                                // Extract logo
                                string? logo = null;
                                if (team.TryGetProperty("logo", out var logoEl) && logoEl.ValueKind == System.Text.Json.JsonValueKind.String)
                                    logo = logoEl.GetString();
                                else if (team.TryGetProperty("image", out var imgEl) && imgEl.ValueKind == System.Text.Json.JsonValueKind.String)
                                    logo = imgEl.GetString();
                                else if (team.TryGetProperty("logo_url", out var logoUrlEl) && logoUrlEl.ValueKind == System.Text.Json.JsonValueKind.String)
                                    logo = logoUrlEl.GetString();
                                
                                if (!string.IsNullOrWhiteSpace(logo))
                                {
                                    TeamLogos[teamName] = logo;
                                    break; // Found match, move to next team
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch logo for team {TeamName}", teamName);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading team logos");
        }
    }

    private async Task LoadLiveEventsTeamLogosAsync(System.Text.Json.JsonElement[] events)
    {
        try
        {
            var teamNames = new HashSet<string>();
            
            foreach (var evt in events)
            {
                // Extract home team name
                System.Text.Json.JsonElement homeTeam = default;
                if (evt.TryGetProperty("home_team", out var ht1)) homeTeam = ht1;
                else if (evt.TryGetProperty("homeTeam", out var ht2)) homeTeam = ht2;
                else if (evt.TryGetProperty("home", out var ht3)) homeTeam = ht3;
                
                System.Text.Json.JsonElement awayTeam = default;
                if (evt.TryGetProperty("away_team", out var at1)) awayTeam = at1;
                else if (evt.TryGetProperty("awayTeam", out var at2)) awayTeam = at2;
                else if (evt.TryGetProperty("away", out var at3)) awayTeam = at3;
                
                string? homeName = null;
                if (homeTeam.ValueKind != System.Text.Json.JsonValueKind.Null && homeTeam.ValueKind != System.Text.Json.JsonValueKind.Undefined)
                {
                    if (homeTeam.TryGetProperty("name", out var hn) && hn.ValueKind == System.Text.Json.JsonValueKind.String)
                        homeName = hn.GetString();
                    else if (homeTeam.ValueKind == System.Text.Json.JsonValueKind.String)
                        homeName = homeTeam.GetString();
                }
                
                string? awayName = null;
                if (awayTeam.ValueKind != System.Text.Json.JsonValueKind.Null && awayTeam.ValueKind != System.Text.Json.JsonValueKind.Undefined)
                {
                    if (awayTeam.TryGetProperty("name", out var an) && an.ValueKind == System.Text.Json.JsonValueKind.String)
                        awayName = an.GetString();
                    else if (awayTeam.ValueKind == System.Text.Json.JsonValueKind.String)
                        awayName = awayTeam.GetString();
                }
                
                // Also try to get logo directly from team object
                if (!string.IsNullOrWhiteSpace(homeName))
                {
                    if (!TeamLogos.ContainsKey(homeName))
                    {
                        // Try to get logo from team object first
                        string? logo = null;
                        if (homeTeam.TryGetProperty("logo", out var logoEl) && logoEl.ValueKind == System.Text.Json.JsonValueKind.String)
                            logo = logoEl.GetString();
                        else if (homeTeam.TryGetProperty("image", out var imgEl) && imgEl.ValueKind == System.Text.Json.JsonValueKind.String)
                            logo = imgEl.GetString();
                        else if (homeTeam.TryGetProperty("logo_url", out var logoUrlEl) && logoUrlEl.ValueKind == System.Text.Json.JsonValueKind.String)
                            logo = logoUrlEl.GetString();
                        
                        if (!string.IsNullOrWhiteSpace(logo))
                        {
                            TeamLogos[homeName] = logo;
                        }
                        else
                        {
                            teamNames.Add(homeName);
                        }
                    }
                }
                
                if (!string.IsNullOrWhiteSpace(awayName))
                {
                    if (!TeamLogos.ContainsKey(awayName))
                    {
                        // Try to get logo from team object first
                        string? logo = null;
                        if (awayTeam.TryGetProperty("logo", out var logoEl) && logoEl.ValueKind == System.Text.Json.JsonValueKind.String)
                            logo = logoEl.GetString();
                        else if (awayTeam.TryGetProperty("image", out var imgEl) && imgEl.ValueKind == System.Text.Json.JsonValueKind.String)
                            logo = imgEl.GetString();
                        else if (awayTeam.TryGetProperty("logo_url", out var logoUrlEl) && logoUrlEl.ValueKind == System.Text.Json.JsonValueKind.String)
                            logo = logoUrlEl.GetString();
                        
                        if (!string.IsNullOrWhiteSpace(logo))
                        {
                            TeamLogos[awayName] = logo;
                        }
                        else
                        {
                            teamNames.Add(awayName);
                        }
                    }
                }
            }

            // Search for teams that don't have logos yet
            foreach (var teamName in teamNames)
            {
                if (TeamLogos.ContainsKey(teamName))
                    continue;
                    
                try
                {
                    var searchParams = new TeamSearchParams
                    {
                        Name = teamName,
                        SportId = 1, // Football
                        Page = 1
                    };
                    
                    var result = await _teamSearchService.SearchTeamsAsync(searchParams);
                    
                    if (result.Success && result.Teams.Any())
                    {
                        foreach (var team in result.Teams)
                        {
                            var name = team.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : null;
                            var shortName = team.TryGetProperty("short_name", out var shortNameEl) ? shortNameEl.GetString() : null;
                            
                            if ((name != null && name.Contains(teamName, StringComparison.OrdinalIgnoreCase)) ||
                                (shortName != null && shortName.Contains(teamName, StringComparison.OrdinalIgnoreCase)) ||
                                (teamName.Contains(name ?? "", StringComparison.OrdinalIgnoreCase)) ||
                                (teamName.Contains(shortName ?? "", StringComparison.OrdinalIgnoreCase)))
                            {
                                string? logo = null;
                                if (team.TryGetProperty("logo", out var logoEl) && logoEl.ValueKind == System.Text.Json.JsonValueKind.String)
                                    logo = logoEl.GetString();
                                else if (team.TryGetProperty("image", out var imgEl) && imgEl.ValueKind == System.Text.Json.JsonValueKind.String)
                                    logo = imgEl.GetString();
                                else if (team.TryGetProperty("logo_url", out var logoUrlEl) && logoUrlEl.ValueKind == System.Text.Json.JsonValueKind.String)
                                    logo = logoUrlEl.GetString();
                                
                                if (!string.IsNullOrWhiteSpace(logo))
                                {
                                    TeamLogos[teamName] = logo;
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch logo for team {TeamName}", teamName);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading live events team logos");
        }
    }

    private async Task LoadLeagueLogosAsync()
    {
        try
        {
            var sportsData = await _sportsDataService.GetSportsDataAsync();
            var leagues = sportsData.Leagues ?? new List<LeagueInfo>();
            
            foreach (var league in leagues)
            {
                if (!string.IsNullOrWhiteSpace(league.Name) && !LeagueNamesById.ContainsKey(league.Id))
                    LeagueNamesById[league.Id] = league.Name;

                if (!string.IsNullOrWhiteSpace(league.Logo))
                {
                    if (!string.IsNullOrWhiteSpace(league.Name) && !LeagueLogos.ContainsKey(league.Name))
                        LeagueLogos[league.Name] = league.Logo;
                    if (!LeagueLogosById.ContainsKey(league.Id))
                        LeagueLogosById[league.Id] = league.Logo;
                }

                if (!LeaguePriorityById.ContainsKey(league.Id))
                    LeaguePriorityById[league.Id] = league.Priority ?? int.MaxValue;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading league logos");
        }
    }

    public int GetLeaguePriority(int leagueId)
    {
        return LeaguePriorityById.TryGetValue(leagueId, out var priority)
            ? priority
            : int.MaxValue;
    }
}
