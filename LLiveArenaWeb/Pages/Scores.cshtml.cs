using Microsoft.AspNetCore.Mvc.RazorPages;
using LLiveArenaWeb.Services;
using LLiveArenaWeb.Models;

namespace LLiveArenaWeb.Pages;

public class ScoresModel : PageModel
{
    private readonly IMatchListService _matchListService;

    public ScoresModel(IMatchListService matchListService)
    {
        _matchListService = matchListService;
    }

    public List<MatchListItem> Matches { get; set; } = new();
    public List<MatchListItem> LiveMatches { get; set; } = new();

    public async Task OnGetAsync()
    {
        var matchListResponse = await _matchListService.GetMatchListAsync();
        Matches = matchListResponse?.Data.T1?.ToList() ?? new List<MatchListItem>();
        LiveMatches = await _matchListService.GetLiveMatchesAsync();
    }
}
