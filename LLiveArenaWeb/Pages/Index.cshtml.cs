using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LLiveArenaWeb.Services;
using LLiveArenaWeb.Models;

namespace LLiveArenaWeb.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IMatchListService _matchListService;

    private const long ChampionsLeagueId = 7846996; // EUROPE CHAMPIONS LEAGUE

    public IndexModel(ILogger<IndexModel> logger, IMatchListService matchListService)
    {
        _logger = logger;
        _matchListService = matchListService;
    }

    public List<MatchListItem> ChampionsLeagueMatches { get; set; } = new();

    public async Task OnGetAsync(bool refresh = false)
    {
        try
        {
            // First, ensure we have match list data by calling GetMatchListAsync
            // This will fetch from API if cache is empty
            var matchListResponse = await _matchListService.GetMatchListAsync();
            
            if (matchListResponse?.Success == true && matchListResponse.Data?.T1 != null)
            {
                var allMatches = matchListResponse.Data.T1;
                _logger.LogInformation("Total matches from API: {Count}", allMatches.Count);
                
                // Try to find Champions League matches by ID first
                var championsLeagueMatches = allMatches
                    .Where(m => m.Cid == ChampionsLeagueId)
                    .ToList();
                
                // If no exact match, try by competition name
                if (!championsLeagueMatches.Any())
                {
                    championsLeagueMatches = allMatches
                        .Where(m => m.Cname != null && m.Cname.Contains("CHAMPIONS LEAGUE", StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    _logger.LogInformation("Found {Count} Champions League matches by name", championsLeagueMatches.Count);
                }
                else
                {
                    _logger.LogInformation("Found {Count} Champions League matches by ID", championsLeagueMatches.Count);
                }
                
                // If still no Champions League matches, show any available matches
                if (!championsLeagueMatches.Any() && allMatches.Any())
                {
                    _logger.LogInformation("No Champions League matches found, showing {Count} general matches", Math.Min(3, allMatches.Count));
                    ChampionsLeagueMatches = allMatches
                        .OrderByDescending(m => m.Iplay)
                        .ThenBy(m => m.Stime)
                        .Take(3)
                        .ToList();
                }
                else
                {
                    // Take top 3 Champions League matches, prioritize live matches
                    ChampionsLeagueMatches = championsLeagueMatches
                        .OrderByDescending(m => m.Iplay) // Live matches first
                        .ThenBy(m => m.Stime) // Then by start time
                        .Take(3)
                        .ToList();
                }
            }
            else
            {
                _logger.LogWarning("Match list API returned no data or failed. Success: {Success}, Message: {Msg}", 
                    matchListResponse?.Success ?? false, matchListResponse?.Msg ?? "Unknown error");
                ChampionsLeagueMatches = new List<MatchListItem>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Champions League matches");
            ChampionsLeagueMatches = new List<MatchListItem>();
        }
    }
}
