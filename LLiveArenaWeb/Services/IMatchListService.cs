using LLiveArenaWeb.Models;

namespace LLiveArenaWeb.Services;

public interface IMatchListService
{
    Task<MatchListResponse?> GetMatchListAsync();
    Task<MatchListCategories> GetMatchCategoriesAsync();
    Task<List<MatchListItem>> GetMatchesByCompetitionAsync(long competitionId);
    Task<List<MatchListItem>> GetLiveMatchesAsync();
    Task<MatchListItem?> GetMatchByGmidAsync(long gmid);
}
