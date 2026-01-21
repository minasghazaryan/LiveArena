using LLiveArenaWeb.Models;

namespace LLiveArenaWeb.Services;

public interface ISportsService
{
    Task<IEnumerable<Sport>> GetAllSportsAsync();
    Task<Sport?> GetSportByIdAsync(string id);
    Task<Sport?> GetSportByNameAsync(string name);
}
