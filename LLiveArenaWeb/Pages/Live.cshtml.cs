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
        LiveMatches = await _matchListService.GetLiveMatchesAsync();
        
        // Filter Champions League matches - limit to 3 for display
        ChampionsLeagueMatches = LiveMatches
            .Where(m => m.Cid == ChampionsLeagueId) // Champions League
            .Take(3)
            .ToList();
        
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
            // Default to Barcelona game if available, otherwise first match
            SelectedMatch = ChampionsLeagueMatches.FirstOrDefault(m => m.Ename.Contains("Barcelona")) 
                ?? ChampionsLeagueMatches.First();
            if (SelectedMatch != null)
            {
                StreamResponse = await _streamService.GetStreamSourceAsync(SelectedMatch.Gmid);
            }
        }
    }
}
