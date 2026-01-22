using Microsoft.Extensions.Options;

namespace LLiveArenaWeb.Services;

public class SportsDataBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SportsDataBackgroundService> _logger;
    private readonly SportsDataOptions _options;

    public SportsDataBackgroundService(
        IServiceProvider serviceProvider,
        IOptions<SportsDataOptions> options,
        ILogger<SportsDataBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await LoadFromFileOnStartupAsync(stoppingToken).ConfigureAwait(false);

        var interval = TimeSpan.FromHours(Math.Max(1, _options.RefreshIntervalHours));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(interval, stoppingToken).ConfigureAwait(false);
                await RefreshFromApiAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SportsDataBackgroundService");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken).ConfigureAwait(false);
            }
        }
    }

    private async Task LoadFromFileOnStartupAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ISportsDataService>();
            await service.LoadFromFileAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load sports data from file on startup");
        }
    }

    private async Task RefreshFromApiAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ISportsDataService>();
            var ok = await service.RefreshFromApiAsync(cancellationToken).ConfigureAwait(false);
            if (!ok)
                _logger.LogDebug("Sports data API refresh skipped (no URL configured or API unavailable)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh sports data from API");
        }
    }
}
