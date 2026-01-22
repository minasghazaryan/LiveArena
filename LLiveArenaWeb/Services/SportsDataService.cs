using System.Text.Json;
using LLiveArenaWeb.Models;
using Microsoft.Extensions.Options;

namespace LLiveArenaWeb.Services;

public class SportsDataOptions
{
    public const string Section = "SportsData";
    public string FilePath { get; set; } = "Data/sports-data.json";
    public SportsDataApiOptions Api { get; set; } = new();
    public int RefreshIntervalHours { get; set; } = 24;
}

public class SportsDataApiOptions
{
    public string BaseUrl { get; set; } = "";
    /// <summary>Sport-list endpoint URL (returns { "data": [ { "id", "slug", "name", "name_translations" }, ... ] }).</summary>
    public string SportsListUrl { get; set; } = "";
    /// <summary>Sections-list endpoint URL (returns { "data": [ { "id", "sport_id", "slug", "name", "flag" }, ... ] }).</summary>
    public string SectionsListUrl { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string ApiKeyHeader { get; set; } = "x-apisports-key";
}

public class SportsDataService : ISportsDataService
{
    private readonly SportsDataOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<SportsDataService> _logger;
    private SportsDataStore? _cache;
    private readonly SemaphoreSlim _cacheLock = new(1, 1);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public SportsDataService(
        IOptions<SportsDataOptions> options,
        IHttpClientFactory httpClientFactory,
        IHostEnvironment hostEnvironment,
        ILogger<SportsDataService> logger)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _hostEnvironment = hostEnvironment;
        _logger = logger;
    }

    private string GetFullFilePath()
    {
        var path = Path.Combine(_hostEnvironment.ContentRootPath, _options.FilePath);
        return Path.GetFullPath(path);
    }

    public async Task<SportsDataStore> GetSportsDataAsync(CancellationToken cancellationToken = default)
    {
        await _cacheLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_cache != null)
                return _cache;
            var store = await LoadFromFileCoreAsync(path: null, cancellationToken).ConfigureAwait(false);
            _cache = store;
            return _cache;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public async Task<SportsDataStore> LoadFromFileAsync(CancellationToken cancellationToken = default)
    {
        await _cacheLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var store = await LoadFromFileCoreAsync(path: null, cancellationToken).ConfigureAwait(false);
            _cache = store;
            return _cache;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    private async Task<SportsDataStore> LoadFromFileCoreAsync(string? path, CancellationToken cancellationToken)
    {
        path ??= GetFullFilePath();
        if (!File.Exists(path))
        {
            _logger.LogInformation("Sports data file not found at {Path}, using empty store", path);
            return CreateEmptyStore();
        }

        try
        {
            var json = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
            var store = JsonSerializer.Deserialize<SportsDataStore>(json, JsonOptions)
                ?? CreateEmptyStore();
            _logger.LogInformation("Loaded sports data from {Path}: {Sports} sports, {Sections} sections, {Leagues} leagues, {Teams} teams, {Managers} managers, {Players} players",
                path, store.Sports.Count, store.Sections.Count, store.Leagues.Count, store.Teams.Count, store.Managers.Count, store.Players.Count);
            return store;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load sports data from {Path}, using empty store", path);
            return CreateEmptyStore();
        }
    }

    public async Task SaveToFileAsync(SportsDataStore data, CancellationToken cancellationToken = default)
    {
        var path = GetFullFilePath();
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        data.LastUpdatedUtc = DateTime.UtcNow;
        var json = JsonSerializer.Serialize(data, JsonOptions);
        await File.WriteAllTextAsync(path, json, cancellationToken).ConfigureAwait(false);

        await _cacheLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _cache = data;
        }
        finally
        {
            _cacheLock.Release();
        }

        _logger.LogInformation("Saved sports data to {Path}", path);
    }

    private static readonly HashSet<string> AllowedSectionNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "England", "Spain", "Germany", "Italy", "Netherlands", "Armenia", "Russia"
    };

    public async Task<bool> RefreshFromApiAsync(CancellationToken cancellationToken = default)
    {
        var sportsListUrl = _options.Api?.SportsListUrl?.Trim();
        var sectionsListUrl = _options.Api?.SectionsListUrl?.Trim();
        var baseUrl = _options.Api?.BaseUrl?.Trim();
        if (string.IsNullOrEmpty(sportsListUrl) && string.IsNullOrEmpty(sectionsListUrl) && string.IsNullOrEmpty(baseUrl))
        {
            _logger.LogDebug("Sports data API URLs not configured, skipping refresh");
            return false;
        }

        var apiKey = _options.Api?.ApiKey?.Trim();
        var apiKeyHeader = _options.Api?.ApiKeyHeader?.Trim();
        if (string.IsNullOrEmpty(apiKeyHeader))
            apiKeyHeader = "x-apisports-key";

        try
        {
            var store = await GetSportsDataAsync(cancellationToken).ConfigureAwait(false);
            var anyUpdated = false;

            if (!string.IsNullOrEmpty(sportsListUrl))
            {
                var updated = await FetchAndMergeSportsListAsync(sportsListUrl, apiKey, apiKeyHeader, store, cancellationToken).ConfigureAwait(false);
                if (updated) anyUpdated = true;
            }

            if (!string.IsNullOrEmpty(sectionsListUrl))
            {
                var updated = await FetchAndMergeSectionsListAsync(sectionsListUrl, apiKey, apiKeyHeader, store, cancellationToken).ConfigureAwait(false);
                if (updated) anyUpdated = true;
            }

            if (anyUpdated)
            {
                await SaveToFileAsync(store, cancellationToken).ConfigureAwait(false);
                _logger.LogInformation("Refreshed sports data from API: {Sports} sports, {Sections} sections", store.Sports.Count, store.Sections.Count);
                return true;
            }

            if (!string.IsNullOrEmpty(baseUrl))
            {
                var client = _httpClientFactory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Get, baseUrl);
                if (!string.IsNullOrEmpty(apiKey))
                    request.Headers.TryAddWithoutValidation(apiKeyHeader, apiKey);

                var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Sports data API returned {StatusCode}", response.StatusCode);
                    return false;
                }

                var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                var fullStore = JsonSerializer.Deserialize<SportsDataStore>(json, JsonOptions);
                if (fullStore == null)
                {
                    _logger.LogWarning("Sports data API returned invalid JSON");
                    return false;
                }

                await SaveToFileAsync(fullStore, cancellationToken).ConfigureAwait(false);
                _logger.LogInformation("Refreshed sports data from API: {Sports} sports, {Sections} sections, {Leagues} leagues, {Teams} teams, {Managers} managers, {Players} players",
                    fullStore.Sports.Count, fullStore.Sections.Count, fullStore.Leagues.Count, fullStore.Teams.Count, fullStore.Managers.Count, fullStore.Players.Count);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh sports data from API");
            return false;
        }
    }

    private async Task<bool> FetchAndMergeSportsListAsync(string url, string? apiKey, string apiKeyHeader, SportsDataStore store, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (!string.IsNullOrEmpty(apiKey))
            request.Headers.TryAddWithoutValidation(apiKeyHeader, apiKey);

        var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Sports list API returned {StatusCode}", response.StatusCode);
            return false;
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var apiResponse = JsonSerializer.Deserialize<SportsListApiResponse>(json, JsonOptions);
        if (apiResponse?.Data == null)
        {
            _logger.LogWarning("Sports list API returned invalid JSON");
            return false;
        }

        store.Sports = apiResponse.Data
            .Select(s => new SportInfo
            {
                Id = s.Id,
                SportId = s.Id,
                Slug = s.Slug ?? "",
                Name = s.Name ?? ""
            })
            .ToList();
        return true;
    }

    private async Task<bool> FetchAndMergeSectionsListAsync(string url, string? apiKey, string apiKeyHeader, SportsDataStore store, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (!string.IsNullOrEmpty(apiKey))
            request.Headers.TryAddWithoutValidation(apiKeyHeader, apiKey);

        var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Sections list API returned {StatusCode}", response.StatusCode);
            return false;
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var apiResponse = JsonSerializer.Deserialize<SectionsListApiResponse>(json, JsonOptions);
        if (apiResponse?.Data == null)
        {
            _logger.LogWarning("Sections list API returned invalid JSON");
            return false;
        }

        store.Sections = apiResponse.Data
            .Where(s => AllowedSectionNames.Contains(s.Name ?? ""))
            .Select(s => new SectionInfo
            {
                Id = s.Id,
                SportId = s.SportId,
                Slug = s.Slug ?? "",
                Name = s.Name ?? "",
                Flag = s.Flag ?? ""
            })
            .ToList();
        return true;
    }

    private static SportsDataStore CreateEmptyStore()
    {
        return new SportsDataStore
        {
            LastUpdatedUtc = DateTime.UtcNow,
            Sports = new List<SportInfo>(),
            Sections = new List<SectionInfo>(),
            Leagues = new List<LeagueInfo>(),
            Teams = new List<TeamInfo>(),
            Managers = new List<ManagerInfo>(),
            Players = new List<PlayerInfo>()
        };
    }
}
