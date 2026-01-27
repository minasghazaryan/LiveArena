using LLiveArenaWeb.Models;
using System.Text.Json;

namespace LLiveArenaWeb.Services;

public class MatchListBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MatchListBackgroundService> _logger;
    private readonly TimeSpan _refreshInterval = TimeSpan.FromMinutes(5); // Refresh every 5 minutes
    private const string DefaultRapidApiHost = "all-sport-live-stream.p.rapidapi.com";
    private const string MatchListUrl = "https://all-sport-live-stream.p.rapidapi.com/api/d/match_list?sportId=1";

    private readonly string _rapidApiHost;
    private readonly string _rapidApiKey;

    public MatchListBackgroundService(IServiceProvider serviceProvider, ILogger<MatchListBackgroundService> logger, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _rapidApiHost = configuration["RapidApi:AllSportLiveStream:Host"] ?? DefaultRapidApiHost;
        _rapidApiKey = configuration["RapidApi:AllSportLiveStream:ApiKey"] ?? string.Empty;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Do an initial fetch immediately on startup
        try
        {
            await RefreshMatchListAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            try
            {
                _logger.LogError(ex, "Error in initial match list fetch");
            }
            catch
            {
                // Ignore logging errors (e.g., if logger is disposed)
            }
        }

        // Then continue with periodic refreshes
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_refreshInterval, stoppingToken);
                await RefreshMatchListAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                try
                {
                    _logger.LogError(ex, "Error in MatchListBackgroundService");
                }
                catch
                {
                    // Ignore logging errors (e.g., if logger is disposed)
                }
                
                // Wait a bit longer before retrying on error
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }

    private async Task RefreshMatchListAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_rapidApiKey))
            {
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            var matchListService = scope.ServiceProvider.GetRequiredService<IMatchListService>();
            var sportsDataService = scope.ServiceProvider.GetRequiredService<ISportsDataService>();

            var httpClient = httpClientFactory.CreateClient();
            
            // Use HttpRequestMessage to avoid header conflicts
            var request = new HttpRequestMessage(HttpMethod.Get, MatchListUrl);
            request.Headers.Add("x-rapidapi-host", _rapidApiHost);
            request.Headers.Add("x-rapidapi-key", _rapidApiKey);
            
            var response = await httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var matchListResponse = JsonSerializer.Deserialize<MatchListResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (matchListResponse != null)
                {
                    matchListResponse.LastUpdatedAt = DateTime.UtcNow;
                    
                    // Apply filtering by sports-data.json leagues (same as MatchListService does)
                    await FilterBySportsDataLeaguesAsync(matchListResponse, sportsDataService);
                    
                    // Update the cache in MatchListService
                    if (matchListService is MatchListService service)
                    {
                        service.ClearCache();
                        service.UpdateCache(matchListResponse);
                    }
                    
                    var matchCount = matchListResponse.Data?.T1?.Count ?? 0;
                    var championsLeagueCount = matchListResponse.Data?.T1?
                        .Count(m => m.Cid == 7846996 || (m.Cname != null && m.Cname.Contains("CHAMPIONS LEAGUE", StringComparison.OrdinalIgnoreCase))) ?? 0;
                    
                    try
                    {
                        _logger.LogInformation("Match list refreshed successfully. Total matches: {Total}, Champions League: {CL}", 
                            matchCount, championsLeagueCount);
                    }
                    catch
                    {
                        // Ignore logging errors (e.g., if logger is disposed)
                    }
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                try
                {
                    _logger.LogWarning("Failed to refresh match list. Status: {Status}, Response: {Response}", 
                        response.StatusCode, errorContent);
                }
                catch
                {
                    // Ignore logging errors (e.g., if logger is disposed)
                }
            }
        }
        catch (Exception ex)
        {
            try
            {
                _logger.LogError(ex, "Error refreshing match list in background service");
            }
            catch
            {
                // Ignore logging errors (e.g., if logger is disposed)
            }
        }
    }

    private static async Task FilterBySportsDataLeaguesAsync(MatchListResponse matchListResponse, ISportsDataService sportsDataService)
    {
        if (matchListResponse.Data.T1 == null)
        {
            return;
        }

        try
        {
            // Get league IDs from sports-data.json
            var sportsData = await sportsDataService.GetSportsDataAsync();
            var allowedLeagueIds = sportsData.Leagues?.Select(l => (long)l.Id).ToHashSet() ?? new HashSet<long>();

            if (!allowedLeagueIds.Any())
            {
                // If no leagues configured, don't filter (show all matches)
                return;
            }

            // Filter matches to only include those with Cid matching league IDs from sports-data.json
            matchListResponse.Data.T1 = matchListResponse.Data.T1
                .Where(match => allowedLeagueIds.Contains(match.Cid))
                .ToList();
        }
        catch (Exception)
        {
            // If there's an error getting sports data, don't filter (show all matches)
            // This ensures the service continues to work even if sports data is unavailable
        }
    }
}
