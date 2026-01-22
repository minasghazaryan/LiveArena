using System.Text.Json;
using Microsoft.Extensions.Options;

namespace LLiveArenaWeb.Services;

public class SportscoreOptions
{
    public const string Section = "Sportscore";
    public string BaseUrl { get; set; } = "https://sportscore1.p.rapidapi.com";
    public string Host { get; set; } = "sportscore1.p.rapidapi.com";
    public string ApiKey { get; set; } = "";
}

public class LiveEventsService : ILiveEventsService
{
    private readonly SportscoreOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<LiveEventsService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public LiveEventsService(IOptions<SportscoreOptions> options, IHttpClientFactory httpClientFactory, ILogger<LiveEventsService> logger)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<SportscoreLiveEventsResult> GetLiveEventsAsync(int sportId, int page = 1, CancellationToken cancellationToken = default)
    {
        var baseUrl = _options.BaseUrl?.Trim().TrimEnd('/');
        var host = _options.Host?.Trim();
        var apiKey = _options.ApiKey?.Trim();

        if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(host) || string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("Sportscore API not configured (BaseUrl, Host, ApiKey)");
            return new SportscoreLiveEventsResult { Success = false, Error = "Sportscore API not configured." };
        }

        var url = $"{baseUrl}/sports/{sportId}/events/live?page={page}";

        try
        {
            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("x-rapidapi-host", host);
            request.Headers.Add("x-rapidapi-key", apiKey);

            var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogWarning("Sportscore live events API returned {StatusCode}: {Body}", response.StatusCode, body);
                return new SportscoreLiveEventsResult { Success = false, Error = $"API returned {response.StatusCode}." };
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var events = new List<JsonElement>();
            if (root.TryGetProperty("data", out var dataArr) && dataArr.ValueKind == JsonValueKind.Array)
            {
                foreach (var e in dataArr.EnumerateArray())
                    events.Add(e.Clone());
            }
            else if (root.TryGetProperty("events", out var eventsArr) && eventsArr.ValueKind == JsonValueKind.Array)
            {
                foreach (var e in eventsArr.EnumerateArray())
                    events.Add(e.Clone());
            }

            JsonElement? meta = null;
            if (root.TryGetProperty("meta", out var metaEl))
                meta = metaEl.Clone();

            _logger.LogDebug("Sportscore live events: sport_id={SportId}, page={Page}, count={Count}", sportId, page, events.Count);
            return new SportscoreLiveEventsResult { Success = true, Events = events, Meta = meta };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sportscore live events request failed");
            return new SportscoreLiveEventsResult { Success = false, Error = ex.Message };
        }
    }

    public async Task<SportscoreEventDetailsResult> GetEventDetailsAsync(int eventId, CancellationToken cancellationToken = default)
    {
        var baseUrl = _options.BaseUrl?.Trim().TrimEnd('/');
        var host = _options.Host?.Trim();
        var apiKey = _options.ApiKey?.Trim();

        if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(host) || string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("Sportscore API not configured (BaseUrl, Host, ApiKey)");
            return new SportscoreEventDetailsResult { Success = false, Error = "Sportscore API not configured." };
        }

        var url = $"{baseUrl}/events/{eventId}";

        try
        {
            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("x-rapidapi-host", host);
            request.Headers.Add("x-rapidapi-key", apiKey);

            var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogWarning("Sportscore event details API returned {StatusCode}: {Body}", response.StatusCode, body);
                return new SportscoreEventDetailsResult { Success = false, Error = $"API returned {response.StatusCode}." };
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Try to find the event data - could be in "data" property or root itself
            JsonElement? eventData = null;
            if (root.TryGetProperty("data", out var dataEl))
            {
                eventData = dataEl.Clone();
            }
            else
            {
                eventData = root.Clone();
            }

            _logger.LogDebug("Sportscore event details: event_id={EventId}", eventId);
            return new SportscoreEventDetailsResult { Success = true, Event = eventData };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sportscore event details request failed");
            return new SportscoreEventDetailsResult { Success = false, Error = ex.Message };
        }
    }
}
