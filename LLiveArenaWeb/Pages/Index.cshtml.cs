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

    public List<MatchListItem> PrematchMatches { get; set; } = new();

    public async Task OnGetAsync(bool refresh = false)
    {
        try
        {
            var categories = await _matchListService.GetMatchCategoriesAsync();
            PrematchMatches = categories.Prematch
                .OrderBy(m => m.Stime)
                .Take(3)
                .ToList();

            if (!PrematchMatches.Any())
            {
                var matchListResponse = await _matchListService.GetMatchListAsync();
                if (matchListResponse?.Success == true && matchListResponse.Data?.T1 != null)
                {
                    var allMatches = matchListResponse.Data.T1;
                    _logger.LogInformation("No prematch data, showing {Count} general matches", Math.Min(3, allMatches.Count));
                    PrematchMatches = allMatches
                        .OrderByDescending(m => m.Iplay)
                        .ThenBy(m => m.Stime)
                        .Take(3)
                        .ToList();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching prematch matches");
            PrematchMatches = new List<MatchListItem>();
        }
    }
}
