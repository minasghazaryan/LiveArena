using LLiveArenaWeb.Models;
using System.Text.Json;

namespace LLiveArenaWeb.Services;

public class ScheduleService : IScheduleService
{
    private readonly ScheduleData _scheduleData;

    public ScheduleService()
    {
        // Initialize with sample data from the provided schedule endpoint
        _scheduleData = new ScheduleData
        {
            FetchScheduleData = new FetchScheduleData
            {
                Edges = new List<ScheduleEdge>
                {
                    new ScheduleEdge
                    {
                       
                    }
                }
            }
        };
    }

    public Task<ScheduleData?> GetScheduleAsync(string? date = null)
    {
        // This will be replaced with actual API call
        return Task.FromResult<ScheduleData?>(_scheduleData);
    }

    public Task<List<Match>> GetMatchesByDateAsync(string date)
    {
        var matches = _scheduleData.FetchScheduleData?.Edges
            .Where(e => e.Date == date)
            .SelectMany(e => e.Tours)
            .SelectMany(t => t.Matches)
            .ToList() ?? new List<Match>();

        return Task.FromResult(matches);
    }

    public Task<List<Match>> GetMatchesBySportAsync(string sportSlug)
    {
        var matches = _scheduleData.FetchScheduleData?.Edges
            .SelectMany(e => e.Tours)
            .SelectMany(t => t.Matches)
            .Where(m => m.Sport.Slug.Equals(sportSlug, StringComparison.OrdinalIgnoreCase))
            .ToList() ?? new List<Match>();

        return Task.FromResult(matches);
    }

    public Task<Match?> GetMatchByIdAsync(int matchId)
    {
        var match = _scheduleData.FetchScheduleData?.Edges
            .SelectMany(e => e.Tours)
            .SelectMany(t => t.Matches)
            .FirstOrDefault(m => m.Id == matchId);

        return Task.FromResult(match);
    }

    public Task<List<Match>> GetLiveMatchesAsync()
    {
        var matches = _scheduleData.FetchScheduleData?.Edges
            .SelectMany(e => e.Tours)
            .SelectMany(t => t.Matches)
            .Where(m => m.Status == "LIVE" || m.StreamingStatus == "LIVE")
            .ToList() ?? new List<Match>();

        return Task.FromResult(matches);
    }
}
