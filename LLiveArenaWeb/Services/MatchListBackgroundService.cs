using LLiveArenaWeb.Models;
using System.Text.Json;

namespace LLiveArenaWeb.Services;

public class MatchListBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MatchListBackgroundService> _logger;
    private readonly TimeSpan _refreshInterval = TimeSpan.FromMinutes(5); // Refresh every 5 minutes
    
    private const string RapidApiHost = "all-sport-live-stream.p.rapidapi.com";
    private const string RapidApiKey = "46effbe6bcmshf54dd907f3cd18ap127fd8jsn9d2ba3ddee14";
    private const string MatchListUrl = "https://all-sport-live-stream.p.rapidapi.com/api/d/match_list?sportId=1";

    public MatchListBackgroundService(IServiceProvider serviceProvider, ILogger<MatchListBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
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
            using var scope = _serviceProvider.CreateScope();
            var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            var matchListService = scope.ServiceProvider.GetRequiredService<IMatchListService>();

            var httpClient = httpClientFactory.CreateClient();
            
            // Use HttpRequestMessage to avoid header conflicts
            var request = new HttpRequestMessage(HttpMethod.Get, MatchListUrl);
            request.Headers.Add("x-rapidapi-host", RapidApiHost);
            request.Headers.Add("x-rapidapi-key", RapidApiKey);
            
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
}
