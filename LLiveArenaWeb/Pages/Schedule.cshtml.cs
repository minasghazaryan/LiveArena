using Microsoft.AspNetCore.Mvc.RazorPages;
using LLiveArenaWeb.Models;
using LLiveArenaWeb.Services;

namespace LLiveArenaWeb.Pages;

public class ScheduleModel : PageModel
{
    private readonly IMatchListService _matchListService;

    public ScheduleModel(IMatchListService matchListService)
    {
        _matchListService = matchListService;
    }

    public Dictionary<DateOnly, List<MatchListItem>> PrematchByDate { get; private set; } = new();

    public async Task OnGetAsync()
    {
        var categories = await _matchListService.GetMatchCategoriesAsync();
        var prematch = categories.Prematch ?? new List<MatchListItem>();

        // Parse times and group by date
        var groups = new Dictionary<DateOnly, List<(DateTime Time, MatchListItem Match)>>();

        foreach (var match in prematch)
        {
            if (!DateTime.TryParse(match.Stime, out var dt))
            {
                // If time cannot be parsed, skip from schedule
                continue;
            }

            var dateKey = DateOnly.FromDateTime(dt.Date);
            if (!groups.TryGetValue(dateKey, out var list))
            {
                list = new List<(DateTime, MatchListItem)>();
                groups[dateKey] = list;
            }
            list.Add((dt, match));
        }

        PrematchByDate = groups
            .OrderBy(g => g.Key)
            .ToDictionary(
                g => g.Key,
                g => g.Value
                    .OrderBy(x => x.Time)
                    .Select(x => x.Match)
                    .ToList());
    }

    public static string GetTime(MatchListItem match)
    {
        if (DateTime.TryParse(match.Stime, out var dt))
        {
            return dt.ToString("HH:mm");
        }
        return match.Stime;
    }

    public static string GetHomeTeam(MatchListItem match)
    {
        return match.Section?.FirstOrDefault(s => s.Sno == 1)?.Nat ?? string.Empty;
    }

    public static string GetAwayTeam(MatchListItem match)
    {
        return match.Section?.FirstOrDefault(s => s.Sno == 3)?.Nat ?? string.Empty;
    }
}

