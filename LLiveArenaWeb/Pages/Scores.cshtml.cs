using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LLiveArenaWeb.Services;
using LLiveArenaWeb.Models;
using System.Text.Json;

namespace LLiveArenaWeb.Pages;

public class ScoresModel : PageModel
{
    private readonly IMatchListService _matchListService;
    private readonly ILiveEventsService _liveEventsService;
    private readonly ISportsDataService _sportsDataService;
    private readonly ITeamSearchService _teamSearchService;
    private readonly ILogger<ScoresModel> _logger;

    public ScoresModel(IMatchListService matchListService, ILiveEventsService liveEventsService, ISportsDataService sportsDataService, ITeamSearchService teamSearchService, ILogger<ScoresModel> logger)
    {
        _matchListService = matchListService;
        _liveEventsService = liveEventsService;
        _sportsDataService = sportsDataService;
        _teamSearchService = teamSearchService;
        _logger = logger;
    }

    public Dictionary<string, Dictionary<string, List<JsonElement>>> MatchesByDateAndLeague { get; set; } = new(); // date -> league -> matches
    public Dictionary<string, string> TeamLogos { get; set; } = new();
    public Dictionary<string, string> LeagueLogos { get; set; } = new();
    public Dictionary<string, int> LeaguePriorityByName { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public DateTime SelectedDate { get; set; } = DateTime.Now;
    public string Filter { get; set; } = "all"; // all, live, finished, scheduled
    public List<DateTime> DateRange { get; set; } = new();
    public DateTime Today { get; set; }
    public DateTime Tomorrow { get; set; }
    public DateTime AfterTomorrow { get; set; }

    public async Task OnGetAsync(DateTime? date = null, string? filter = null)
    {
        SelectedDate = date ?? DateTime.Now;
        Filter = filter ?? "all";
        
        Today = DateTime.Now.Date;
        Tomorrow = Today.AddDays(1);
        AfterTomorrow = Today.AddDays(2);
        
        // Generate date range: 5 days before today, today, tomorrow, and day after tomorrow
        DateRange = new List<DateTime>();
        for (int i = -5; i <= 2; i++)
        {
            DateRange.Add(Today.AddDays(i));
        }
        
        // Load league logos
        await LoadLeagueLogosAsync();
        
        // Get allowed league IDs from sports-data.json
        // Only show leagues that have IDs in sports-data.json: LaLiga (251), Bundesliga (512), Premier League (317), Serie A (592)
        var sportsData = await _sportsDataService.GetSportsDataAsync();
        var allowedLeagueIds = sportsData.Leagues?
            .Select(l => l.Id)
            .ToList() ?? new List<int>();
        
        // Create a mapping of league ID to league name for display
        var leagueIdToName = sportsData.Leagues?
            .ToDictionary(l => l.Id, l => l.Name) ?? new Dictionary<int, string>();

        LeaguePriorityByName = sportsData.Leagues?
            .Where(l => !string.IsNullOrWhiteSpace(l.Name))
            .ToDictionary(l => l.Name, l => l.Priority ?? int.MaxValue, StringComparer.OrdinalIgnoreCase)
            ?? new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        
        // Fetch events only for the selected date
        var allEvents = new List<(DateTime date, JsonElement evt)>();
        var selectedDateOnly = SelectedDate.Date;
        var result = await _liveEventsService.GetEventsByDateAsync(sportId: 1, selectedDateOnly, page: 1);
        
        if (result.Success)
        {
            foreach (var evt in result.Events)
            {
                // Extract league ID from event
                int? eventLeagueId = null;
                System.Text.Json.JsonElement leagueElement = default;
                
                if (evt.TryGetProperty("league", out var lg1)) leagueElement = lg1;
                else if (evt.TryGetProperty("tournament", out var lg2)) leagueElement = lg2;
                else if (evt.TryGetProperty("competition", out var lg3)) leagueElement = lg3;
                
                if (leagueElement.ValueKind != System.Text.Json.JsonValueKind.Null && leagueElement.ValueKind != System.Text.Json.JsonValueKind.Undefined)
                {
                    // Try to get league ID from the league object
                    if (leagueElement.TryGetProperty("id", out var lid) && lid.ValueKind == JsonValueKind.Number)
                        eventLeagueId = lid.GetInt32();
                    else if (leagueElement.TryGetProperty("league_id", out var lid2) && lid2.ValueKind == JsonValueKind.Number)
                        eventLeagueId = lid2.GetInt32();
                    else if (leagueElement.TryGetProperty("tournament_id", out var tid) && tid.ValueKind == JsonValueKind.Number)
                        eventLeagueId = tid.GetInt32();
                }
                
                // Also check if league_id is directly on the event
                if (!eventLeagueId.HasValue)
                {
                    if (evt.TryGetProperty("league_id", out var evtLid) && evtLid.ValueKind == JsonValueKind.Number)
                        eventLeagueId = evtLid.GetInt32();
                    else if (evt.TryGetProperty("tournament_id", out var evtTid) && evtTid.ValueKind == JsonValueKind.Number)
                        eventLeagueId = evtTid.GetInt32();
                }
                
                // Check if event league ID matches any allowed league ID
                if (eventLeagueId.HasValue && allowedLeagueIds.Contains(eventLeagueId.Value))
                {
                    allEvents.Add((selectedDateOnly, evt));
                }
            }
        }
        
        // Group by date and league
        foreach (var (eventDate, evt) in allEvents)
        {
            // Apply status filter
            var status = GetEventStatus(evt);
            if (Filter != "all" && 
                !((Filter == "live" && status == "live") ||
                  (Filter == "finished" && status == "finished") ||
                  (Filter == "scheduled" && status == "scheduled")))
            {
                continue;
            }
            
            // Get league ID and map to name from sports-data.json
            int? eventLeagueId = null;
            System.Text.Json.JsonElement leagueElement = default;
            if (evt.TryGetProperty("league", out var lg1)) leagueElement = lg1;
            else if (evt.TryGetProperty("tournament", out var lg2)) leagueElement = lg2;
            else if (evt.TryGetProperty("competition", out var lg3)) leagueElement = lg3;
            
            if (leagueElement.ValueKind != System.Text.Json.JsonValueKind.Null && leagueElement.ValueKind != System.Text.Json.JsonValueKind.Undefined)
            {
                if (leagueElement.TryGetProperty("id", out var lid) && lid.ValueKind == JsonValueKind.Number)
                    eventLeagueId = lid.GetInt32();
                else if (leagueElement.TryGetProperty("league_id", out var lid2) && lid2.ValueKind == JsonValueKind.Number)
                    eventLeagueId = lid2.GetInt32();
                else if (leagueElement.TryGetProperty("tournament_id", out var tid) && tid.ValueKind == JsonValueKind.Number)
                    eventLeagueId = tid.GetInt32();
            }
            
            if (!eventLeagueId.HasValue)
            {
                if (evt.TryGetProperty("league_id", out var evtLid) && evtLid.ValueKind == JsonValueKind.Number)
                    eventLeagueId = evtLid.GetInt32();
                else if (evt.TryGetProperty("tournament_id", out var evtTid) && evtTid.ValueKind == JsonValueKind.Number)
                    eventLeagueId = evtTid.GetInt32();
            }
            
            // Use league name from mapping if available, otherwise fallback to API name
            string? leagueName = "Other";
            if (eventLeagueId.HasValue)
            {
                var leagueId = eventLeagueId.Value;
                if (leagueIdToName.ContainsKey(leagueId))
                {
                    leagueName = leagueIdToName[leagueId];
                }
            }
            else
            {
                // Fallback to extracting name from event
                if (leagueElement.ValueKind != System.Text.Json.JsonValueKind.Null && leagueElement.ValueKind != System.Text.Json.JsonValueKind.Undefined)
                {
                    if (leagueElement.TryGetProperty("name", out var ln) && ln.ValueKind == System.Text.Json.JsonValueKind.String)
                        leagueName = ln.GetString() ?? "Other";
                    else if (leagueElement.ValueKind == System.Text.Json.JsonValueKind.String)
                        leagueName = leagueElement.GetString() ?? "Other";
                }
            }
            
            var dateKey = eventDate.ToString("yyyy-MM-dd");
            if (!MatchesByDateAndLeague.ContainsKey(dateKey))
                MatchesByDateAndLeague[dateKey] = new Dictionary<string, List<JsonElement>>();
            
            if (!MatchesByDateAndLeague[dateKey].ContainsKey(leagueName))
                MatchesByDateAndLeague[dateKey][leagueName] = new List<JsonElement>();
            
            MatchesByDateAndLeague[dateKey][leagueName].Add(evt);
        }
        
        // Load team logos for all events
        var allEventElements = allEvents.Select(e => e.evt).ToList();
        await LoadTeamLogosAsync(allEventElements);
    }

    public int GetLeaguePriority(string? leagueName)
    {
        if (string.IsNullOrWhiteSpace(leagueName))
            return int.MaxValue;

        return LeaguePriorityByName.TryGetValue(leagueName, out var priority)
            ? priority
            : int.MaxValue;
    }

    private string GetEventStatus(JsonElement evt)
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
            if (status.Equals("scheduled", StringComparison.OrdinalIgnoreCase) || 
                status.Equals("not_started", StringComparison.OrdinalIgnoreCase))
                return "scheduled";
        }
        
        // Check if there's a score (indicates live or finished)
        if (evt.TryGetProperty("home_score", out var hs) && hs.ValueKind == JsonValueKind.Number &&
            evt.TryGetProperty("away_score", out var aws) && aws.ValueKind == JsonValueKind.Number)
        {
            return "live"; // Assume live if has scores
        }
        
        return "scheduled";
    }

    private async Task LoadLeagueLogosAsync()
    {
        try
        {
            var sportsData = await _sportsDataService.GetSportsDataAsync();
            var leagues = sportsData.Leagues ?? new List<LeagueInfo>();
            
            foreach (var league in leagues)
            {
                if (!string.IsNullOrWhiteSpace(league.Logo) && !LeagueLogos.ContainsKey(league.Name))
                {
                    LeagueLogos[league.Name] = league.Logo;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading league logos");
        }
    }

    private async Task LoadTeamLogosAsync(List<JsonElement> events)
    {
        try
        {
            var teamNames = new HashSet<string>();
            
            foreach (var evt in events)
            {
                // Extract team names
                System.Text.Json.JsonElement homeTeam = default;
                if (evt.TryGetProperty("home_team", out var ht1)) homeTeam = ht1;
                else if (evt.TryGetProperty("homeTeam", out var ht2)) homeTeam = ht2;
                
                System.Text.Json.JsonElement awayTeam = default;
                if (evt.TryGetProperty("away_team", out var at1)) awayTeam = at1;
                else if (evt.TryGetProperty("awayTeam", out var at2)) awayTeam = at2;
                
                string? homeName = null;
                if (homeTeam.ValueKind != System.Text.Json.JsonValueKind.Null)
                {
                    if (homeTeam.TryGetProperty("name", out var hn) && hn.ValueKind == System.Text.Json.JsonValueKind.String)
                        homeName = hn.GetString();
                }
                
                string? awayName = null;
                if (awayTeam.ValueKind != System.Text.Json.JsonValueKind.Null)
                {
                    if (awayTeam.TryGetProperty("name", out var an) && an.ValueKind == System.Text.Json.JsonValueKind.String)
                        awayName = an.GetString();
                }
                
                // Try to get logo directly from team object
                if (!string.IsNullOrWhiteSpace(homeName) && !TeamLogos.ContainsKey(homeName))
                {
                    string? logo = null;
                    if (homeTeam.TryGetProperty("logo", out var htLogo) && htLogo.ValueKind == System.Text.Json.JsonValueKind.String)
                        logo = htLogo.GetString();
                    else if (homeTeam.TryGetProperty("image", out var htImg) && htImg.ValueKind == System.Text.Json.JsonValueKind.String)
                        logo = htImg.GetString();
                    
                    if (!string.IsNullOrWhiteSpace(logo))
                        TeamLogos[homeName] = logo;
                    else
                        teamNames.Add(homeName);
                }
                
                if (!string.IsNullOrWhiteSpace(awayName) && !TeamLogos.ContainsKey(awayName))
                {
                    string? logo = null;
                    if (awayTeam.TryGetProperty("logo", out var atLogo) && atLogo.ValueKind == System.Text.Json.JsonValueKind.String)
                        logo = atLogo.GetString();
                    else if (awayTeam.TryGetProperty("image", out var atImg) && atImg.ValueKind == System.Text.Json.JsonValueKind.String)
                        logo = atImg.GetString();
                    
                    if (!string.IsNullOrWhiteSpace(logo))
                        TeamLogos[awayName] = logo;
                    else
                        teamNames.Add(awayName);
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
                        SportId = 1,
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
            _logger.LogError(ex, "Error loading team logos");
        }
    }
}
