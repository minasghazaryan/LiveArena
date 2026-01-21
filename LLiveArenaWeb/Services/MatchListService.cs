using LLiveArenaWeb.Models;
using System.Text.Json;

namespace LLiveArenaWeb.Services;

public class MatchListService : IMatchListService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private MatchListResponse? _cachedMatchListData;
    private DateTime _cacheExpiry = DateTime.MinValue;
    private readonly object _cacheLock = new object();
    private const int CacheDurationMinutes = 5; // Cache for 5 minutes (background service refreshes every 30 seconds)
    
    private const string RapidApiHost = "all-sport-live-stream.p.rapidapi.com";
    private const string RapidApiKey = "49eb2c2a31mshb8ed05c07896df9p120e09jsn67fc0221f12d";
    private const string MatchListUrl = "https://all-sport-live-stream.p.rapidapi.com/api/d/match_list?sportId=1";

    public MatchListService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private async Task<MatchListResponse?> FetchMatchListFromApiAsync()
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            
            // Use HttpRequestMessage to avoid header conflicts
            var request = new HttpRequestMessage(HttpMethod.Get, MatchListUrl);
            request.Headers.Add("x-rapidapi-host", RapidApiHost);
            request.Headers.Add("x-rapidapi-key", RapidApiKey);
            
            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var matchListResponse = JsonSerializer.Deserialize<MatchListResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (matchListResponse != null)
                {
                    matchListResponse.LastUpdatedAt = DateTime.UtcNow;
                    // Log for debugging
                    System.Diagnostics.Debug.WriteLine($"Match list fetched: {matchListResponse.Data?.T1?.Count ?? 0} matches");
                }
                
                return matchListResponse;
            }

            return new MatchListResponse
            {
                Success = false,
                Msg = $"Failed to fetch match list: {response.StatusCode}",
                Status = (int)response.StatusCode,
                Data = new MatchListData { T1 = new List<MatchListItem>() },
                LastUpdatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            return new MatchListResponse
            {
                Success = false,
                Msg = $"Error fetching match list: {ex.Message}",
                Status = 500,
                Data = new MatchListData { T1 = new List<MatchListItem>() },
                LastUpdatedAt = DateTime.UtcNow
            };
        }
    }

    private async Task<MatchListResponse> GetMatchListDataAsync(bool forceRefresh = false)
    {
        // Check if cache is still valid (unless force refresh)
        if (!forceRefresh)
        {
            lock (_cacheLock)
            {
                if (_cachedMatchListData != null && DateTime.UtcNow < _cacheExpiry)
                {
                    return _cachedMatchListData;
                }
            }
        }

        // Fetch fresh data from API
        var matchListData = await FetchMatchListFromApiAsync();
        
        if (matchListData != null)
        {
            lock (_cacheLock)
            {
                _cachedMatchListData = matchListData;
                _cacheExpiry = DateTime.UtcNow.AddMinutes(CacheDurationMinutes);
            }
            return matchListData;
        }

        // Return cached data if available, even if expired, otherwise return empty
        lock (_cacheLock)
        {
            if (_cachedMatchListData != null)
            {
                return _cachedMatchListData;
            }
        }

        // Return empty data if API call failed and no cache
        return new MatchListResponse
        {
            Success = false,
            Msg = "Failed to fetch match list",
            Status = 500,
            Data = new MatchListData { T1 = new List<MatchListItem>() },
            LastUpdatedAt = DateTime.UtcNow
        };
    }

    // Public method for background service to update cache
    public void UpdateCache(MatchListResponse matchListData)
    {
        lock (_cacheLock)
        {
            _cachedMatchListData = matchListData;
            _cacheExpiry = DateTime.UtcNow.AddMinutes(CacheDurationMinutes);
        }
    }

    public async Task<MatchListResponse?> GetMatchListAsync()
    {
        var matchListData = await GetMatchListDataAsync();
        return matchListData;
    }

    public async Task<List<MatchListItem>> GetMatchesByCompetitionAsync(long competitionId)
    {
        var matchListData = await GetMatchListDataAsync();
        
        // Debug: Log what we're getting
        var allMatches = matchListData.Data.T1 ?? new List<MatchListItem>();
        var championsLeagueMatches = allMatches
            .Where(m => m.Cid == competitionId)
            .ToList();
        
        // If no exact match, try to find by competition name
        if (!championsLeagueMatches.Any())
        {
            championsLeagueMatches = allMatches
                .Where(m => m.Cname != null && m.Cname.Contains("CHAMPIONS LEAGUE", StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return championsLeagueMatches;
    }

    public async Task<List<MatchListItem>> GetLiveMatchesAsync()
    {
        var matchListData = await GetMatchListDataAsync();
        var matches = matchListData.Data.T1?
            .Where(m => m.Iplay == true)
            .ToList() ?? new List<MatchListItem>();

        return matches;
    }

    public async Task<MatchListItem?> GetMatchByGmidAsync(long gmid)
    {
        var matchListData = await GetMatchListDataAsync();
        var match = matchListData.Data.T1?
            .FirstOrDefault(m => m.Gmid == gmid);

        return match;
    }
}
