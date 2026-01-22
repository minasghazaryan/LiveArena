using LLiveArenaWeb.Models;

namespace LLiveArenaWeb.Services;

public interface ISportsDataService
{
    /// <summary>Gets the current sports data (from memory cache or file).</summary>
    Task<SportsDataStore> GetSportsDataAsync(CancellationToken cancellationToken = default);

    /// <summary>Loads data from the configured JSON file.</summary>
    Task<SportsDataStore> LoadFromFileAsync(CancellationToken cancellationToken = default);

    /// <summary>Persists data to the configured JSON file.</summary>
    Task SaveToFileAsync(SportsDataStore data, CancellationToken cancellationToken = default);

    /// <summary>Fetches from configured API (if any) and saves to file.</summary>
    Task<bool> RefreshFromApiAsync(CancellationToken cancellationToken = default);
}
