using LLiveArenaWeb.Models;

namespace LLiveArenaWeb.Services;

public interface IStreamService
{
    Task<StreamResponse?> GetStreamSourceAsync(long gmid);
}
