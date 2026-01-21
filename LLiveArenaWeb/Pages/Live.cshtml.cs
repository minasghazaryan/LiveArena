using Microsoft.AspNetCore.Mvc.RazorPages;
using LLiveArenaWeb.Services;
using LLiveArenaWeb.Models;

namespace LLiveArenaWeb.Pages;

public class LiveModel : PageModel
{
    private readonly IMatchListService _matchListService;
    private readonly IStreamService _streamService;

    public LiveModel(IMatchListService matchListService, IStreamService streamService)
    {
        _matchListService = matchListService;
        _streamService = streamService;
    }

    public List<MatchListItem> LiveMatches { get; set; } = new();
    public List<MatchListItem> ChampionsLeagueMatches { get; set; } = new();
    public MatchListItem? SelectedMatch { get; set; }
    public StreamResponse? StreamResponse { get; set; }

    private const long ChampionsLeagueId = 7846996; // EUROPE CHAMPIONS LEAGUE

    public async Task OnGetAsync(long? gmid = null)
    {
        // First ensure we have match list data
        var matchListResponse = await _matchListService.GetMatchListAsync();
        
        if (matchListResponse?.Success == true && matchListResponse.Data?.T1 != null)
        {
            var allMatches = matchListResponse.Data.T1;
            
            // Try to find Champions League matches by ID first
            ChampionsLeagueMatches = allMatches
                .Where(m => m.Cid == ChampionsLeagueId)
                .ToList();
            
            // If no exact match, try by competition name
            if (!ChampionsLeagueMatches.Any())
            {
                ChampionsLeagueMatches = allMatches
                    .Where(m => m.Cname != null && m.Cname.Contains("CHAMPIONS LEAGUE", StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            
            // Filter to show only LIVE Champions League matches, sorted by start time
            ChampionsLeagueMatches = ChampionsLeagueMatches
                .Where(m => m.Iplay == true) // Only live matches
                .OrderBy(m => m.Stime) // Sort by start time
                .ToList();
            
            // Also get live matches for reference
            LiveMatches = allMatches
                .Where(m => m.Iplay == true)
                .ToList();
        }
        else
        {
            // Fallback: try the service method and filter for live matches
            var allChampionsLeagueMatches = await _matchListService.GetMatchesByCompetitionAsync(ChampionsLeagueId);
            ChampionsLeagueMatches = allChampionsLeagueMatches
                .Where(m => m.Iplay == true) // Only live matches
                .OrderBy(m => m.Stime) // Sort by start time
                .ToList();
            LiveMatches = await _matchListService.GetLiveMatchesAsync();
        }
        
        // If a specific match is selected, get its stream
        if (gmid.HasValue)
        {
            SelectedMatch = await _matchListService.GetMatchByGmidAsync(gmid.Value);
            if (SelectedMatch != null)
            {
                StreamResponse = await _streamService.GetStreamSourceAsync(gmid.Value);
            }
        }
        else if (ChampionsLeagueMatches.Any())
        {
            // Default to Barcelona game if available, otherwise first match (all are already live)
            SelectedMatch = ChampionsLeagueMatches.FirstOrDefault(m => m.Ename.Contains("Barcelona")) 
                ?? ChampionsLeagueMatches.First();
            if (SelectedMatch != null)
            {
                StreamResponse = await _streamService.GetStreamSourceAsync(SelectedMatch.Gmid);
            }
        }
    }
}
