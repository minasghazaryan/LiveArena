using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LLiveArenaWeb.Models;
using LLiveArenaWeb.Services;

namespace LLiveArenaWeb.Pages;

public class MatchModel : PageModel
{
    private readonly IMatchListService _matchListService;

    public MatchModel(IMatchListService matchListService)
    {
        _matchListService = matchListService;
    }

    public MatchListItem? Match { get; private set; }
    public DateTime? StartTime { get; private set; }
    public string MatchTimeDisplay { get; private set; } = "--:--";
    public string MatchDateDisplay { get; private set; } = "--";
    public string RoundDisplay { get; private set; } = "TBD";
    public string StadiumDisplay { get; private set; } = "TBD";

    public async Task<IActionResult> OnGetAsync(long gmid)
    {
        Match = await _matchListService.GetMatchByGmidAsync(gmid);
        if (Match == null)
        {
            return Page();
        }

        if (Match.Iplay)
        {
            return RedirectToPage("/Live", new { gmid });
        }

        if (DateTime.TryParse(Match.Stime, out var parsed))
        {
            StartTime = parsed;
            MatchTimeDisplay = parsed.ToString("HH:mm");
            MatchDateDisplay = parsed.ToString("dd MMMM");
        }

        if (!string.IsNullOrWhiteSpace(Match.Mname))
        {
            RoundDisplay = Match.Mname;
        }

        return Page();
    }
}
