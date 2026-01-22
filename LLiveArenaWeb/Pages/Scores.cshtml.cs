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
        var categories = await _matchListService.GetMatchCategoriesAsync();
        Matches = categories.Finished;
        LiveMatches = categories.Live;
    }
}
