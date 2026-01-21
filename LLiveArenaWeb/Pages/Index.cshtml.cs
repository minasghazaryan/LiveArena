using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LLiveArenaWeb.Services;
using LLiveArenaWeb.Models;

namespace LLiveArenaWeb.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IScheduleService _scheduleService;

    public IndexModel(ILogger<IndexModel> logger, IScheduleService scheduleService)
    {
        _logger = logger;
        _scheduleService = scheduleService;
    }

    public List<Match> TopMatches { get; set; } = new();

    public async Task OnGetAsync()
    {
        // Get matches for today (using the date from sample data)
        var today = "2026-01-20";
        var matches = await _scheduleService.GetMatchesByDateAsync(today);
        
        // Take top 3 matches, or all if less than 3
        TopMatches = matches.Take(3).ToList();
    }
}
