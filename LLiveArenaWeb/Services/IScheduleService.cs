using LLiveArenaWeb.Models;

namespace LLiveArenaWeb.Services;

public interface IScheduleService
{
    Task<ScheduleData?> GetScheduleAsync(string? date = null);
    Task<List<Match>> GetMatchesByDateAsync(string date);
    Task<List<Match>> GetMatchesBySportAsync(string sportSlug);
    Task<Match?> GetMatchByIdAsync(int matchId);
    Task<List<Match>> GetLiveMatchesAsync();
}
