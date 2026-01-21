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
    public Dictionary<string, List<MatchListItem>> LiveMatchesByLeague { get; set; } = new();
    public MatchListItem? SelectedMatch { get; set; }
    public StreamResponse? StreamResponse { get; set; }

    public async Task OnGetAsync(long? gmid = null)
    {
        // First ensure we have match list data
        var matchListResponse = await _matchListService.GetMatchListAsync();
        
        if (matchListResponse?.Success == true && matchListResponse.Data?.T1 != null)
        {
            var allMatches = matchListResponse.Data.T1;
            
            // Get all live matches
            LiveMatches = allMatches
                .Where(m => m.Iplay == true)
                .OrderBy(m => m.Stime) // Sort by start time
                .ToList();
            
            // Group live matches by competition/league
            LiveMatchesByLeague = LiveMatches
                .GroupBy(m => m.Cname ?? "Unknown Competition")
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.OrderBy(m => m.Stime).ToList());
        }
        else
        {
            // Fallback: try the service method
            LiveMatches = await _matchListService.GetLiveMatchesAsync();
            LiveMatches = LiveMatches
                .OrderBy(m => m.Stime)
                .ToList();
            
            // Group by competition
            LiveMatchesByLeague = LiveMatches
                .GroupBy(m => m.Cname ?? "Unknown Competition")
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.OrderBy(m => m.Stime).ToList());
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
        else if (LiveMatches.Any())
        {
            // Default to first live match
            SelectedMatch = LiveMatches.First();
            if (SelectedMatch != null)
            {
                StreamResponse = await _streamService.GetStreamSourceAsync(SelectedMatch.Gmid);
            }
        }
    }
}
