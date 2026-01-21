using LLiveArenaWeb.Models;
using System.Text.Json;

namespace LLiveArenaWeb.Services;

public class MatchListBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MatchListBackgroundService> _logger;
    private readonly TimeSpan _refreshInterval = TimeSpan.FromSeconds(30); // Refresh every 30 seconds
    
    private const string RapidApiHost = "all-sport-live-stream.p.rapidapi.com";
    private const string RapidApiKey = "49eb2c2a31mshb8ed05c07896df9p120e09jsn67fc0221f12d";
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
            _logger.LogError(ex, "Error in initial match list fetch");
        }

        // Then continue with periodic refreshes
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_refreshInterval, stoppingToken);
                await RefreshMatchListAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MatchListBackgroundService");
                // Wait a bit longer before retrying on error
                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
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
                        service.UpdateCache(matchListResponse);
                    }
                    
                    var matchCount = matchListResponse.Data?.T1?.Count ?? 0;
                    var championsLeagueCount = matchListResponse.Data?.T1?
                        .Count(m => m.Cid == 7846996 || (m.Cname != null && m.Cname.Contains("CHAMPIONS LEAGUE", StringComparison.OrdinalIgnoreCase))) ?? 0;
                    
                    _logger.LogInformation("Match list refreshed successfully. Total matches: {Total}, Champions League: {CL}", 
                        matchCount, championsLeagueCount);
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Failed to refresh match list. Status: {Status}, Response: {Response}", 
                    response.StatusCode, errorContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing match list in background service");
        }
    }
}
