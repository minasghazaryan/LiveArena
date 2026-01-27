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

    public async Task<SportscoreLiveEventsResult> GetEventsByDateAsync(int sportId, DateTime date, int page = 1, CancellationToken cancellationToken = default)
    {
        var baseUrl = _options.BaseUrl?.Trim().TrimEnd('/');
        var host = _options.Host?.Trim();
        var apiKey = _options.ApiKey?.Trim();

        if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(host) || string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("Sportscore API not configured (BaseUrl, Host, ApiKey)");
            return new SportscoreLiveEventsResult { Success = false, Error = "Sportscore API not configured." };
        }

        // Format date as YYYY-MM-DD
        var dateStr = date.ToString("yyyy-MM-dd");
        var url = $"{baseUrl}/sports/{sportId}/events/date/{dateStr}?page={page}";

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
                _logger.LogWarning("Sportscore events by date API returned {StatusCode}: {Body}", response.StatusCode, body);
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

            _logger.LogDebug("Sportscore events by date: sport_id={SportId}, date={Date}, page={Page}, count={Count}", sportId, dateStr, page, events.Count);
            return new SportscoreLiveEventsResult { Success = true, Events = events, Meta = meta };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sportscore events by date request failed");
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

    public async Task<SportscoreStatisticsResult> GetEventStatisticsAsync(int eventId, CancellationToken cancellationToken = default)
    {
        var baseUrl = _options.BaseUrl?.Trim().TrimEnd('/');
        var host = _options.Host?.Trim();
        var apiKey = _options.ApiKey?.Trim();

        if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(host) || string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("Sportscore API not configured (BaseUrl, Host, ApiKey)");
            return new SportscoreStatisticsResult { Success = false, Error = "Sportscore API not configured." };
        }

        var url = $"{baseUrl}/events/{eventId}/statistics";

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
                _logger.LogWarning("Sportscore event statistics API returned {StatusCode}: {Body}", response.StatusCode, body);
                return new SportscoreStatisticsResult { Success = false, Error = $"API returned {response.StatusCode}." };
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var statistics = new List<JsonElement>();
            if (root.TryGetProperty("data", out var dataArr) && dataArr.ValueKind == JsonValueKind.Array)
            {
                foreach (var stat in dataArr.EnumerateArray())
                    statistics.Add(stat.Clone());
            }

            JsonElement? meta = null;
            if (root.TryGetProperty("meta", out var metaEl))
                meta = metaEl.Clone();

            _logger.LogDebug("Sportscore event statistics: event_id={EventId}, count={Count}", eventId, statistics.Count);
            return new SportscoreStatisticsResult { Success = true, Statistics = statistics, Meta = meta };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sportscore event statistics request failed");
            return new SportscoreStatisticsResult { Success = false, Error = ex.Message };
        }
    }

    public async Task<SportscoreLineupsResult> GetEventLineupsAsync(int eventId, CancellationToken cancellationToken = default)
    {
        var baseUrl = _options.BaseUrl?.Trim().TrimEnd('/');
        var host = _options.Host?.Trim();
        var apiKey = _options.ApiKey?.Trim();

        if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(host) || string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("Sportscore API not configured (BaseUrl, Host, ApiKey)");
            return new SportscoreLineupsResult { Success = false, Error = "Sportscore API not configured." };
        }

        var url = $"{baseUrl}/events/{eventId}/lineups";

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
                _logger.LogWarning("Sportscore event lineups API returned {StatusCode}: {Body}", response.StatusCode, body);
                return new SportscoreLineupsResult { Success = false, Error = $"API returned {response.StatusCode}." };
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var lineups = new List<JsonElement>();
            if (root.TryGetProperty("data", out var dataArr) && dataArr.ValueKind == JsonValueKind.Array)
            {
                foreach (var lineup in dataArr.EnumerateArray())
                    lineups.Add(lineup.Clone());
            }

            JsonElement? meta = null;
            if (root.TryGetProperty("meta", out var metaEl))
                meta = metaEl.Clone();

            _logger.LogDebug("Sportscore event lineups: event_id={EventId}, count={Count}", eventId, lineups.Count);
            return new SportscoreLineupsResult { Success = true, Lineups = lineups, Meta = meta };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sportscore event lineups request failed");
            return new SportscoreLineupsResult { Success = false, Error = ex.Message };
        }
    }

    public async Task<SportscoreIncidentsResult> GetEventIncidentsAsync(int eventId, CancellationToken cancellationToken = default)
    {
        var baseUrl = _options.BaseUrl?.Trim().TrimEnd('/');
        var host = _options.Host?.Trim();
        var apiKey = _options.ApiKey?.Trim();

        if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(host) || string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("Sportscore API not configured (BaseUrl, Host, ApiKey)");
            return new SportscoreIncidentsResult { Success = false, Error = "Sportscore API not configured." };
        }

        var url = $"{baseUrl}/events/{eventId}/incidents";

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
                _logger.LogWarning("Sportscore event incidents API returned {StatusCode}: {Body}", response.StatusCode, body);
                return new SportscoreIncidentsResult { Success = false, Error = $"API returned {response.StatusCode}." };
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var incidents = new List<JsonElement>();
            if (root.TryGetProperty("data", out var dataArr) && dataArr.ValueKind == JsonValueKind.Array)
            {
                foreach (var incident in dataArr.EnumerateArray())
                    incidents.Add(incident.Clone());
            }

            JsonElement? meta = null;
            if (root.TryGetProperty("meta", out var metaEl))
                meta = metaEl.Clone();

            _logger.LogDebug("Sportscore event incidents: event_id={EventId}, count={Count}", eventId, incidents.Count);
            return new SportscoreIncidentsResult { Success = true, Incidents = incidents, Meta = meta };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sportscore event incidents request failed");
            return new SportscoreIncidentsResult { Success = false, Error = ex.Message };
        }
    }

    public async Task<SportscoreMediasResult> GetEventMediasAsync(int eventId, int page = 1, CancellationToken cancellationToken = default)
    {
        var baseUrl = _options.BaseUrl?.Trim().TrimEnd('/');
        var host = _options.Host?.Trim();
        var apiKey = _options.ApiKey?.Trim();

        if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(host) || string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("Sportscore API not configured (BaseUrl, Host, ApiKey)");
            return new SportscoreMediasResult { Success = false, Error = "Sportscore API not configured." };
        }

        var url = $"{baseUrl}/events/{eventId}/medias?page={page}";

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
                _logger.LogWarning("Sportscore event medias API returned {StatusCode}: {Body}", response.StatusCode, body);
                return new SportscoreMediasResult { Success = false, Error = $"API returned {response.StatusCode}." };
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var medias = new List<JsonElement>();
            if (root.TryGetProperty("data", out var dataArr) && dataArr.ValueKind == JsonValueKind.Array)
            {
                foreach (var media in dataArr.EnumerateArray())
                    medias.Add(media.Clone());
            }

            JsonElement? meta = null;
            if (root.TryGetProperty("meta", out var metaEl))
                meta = metaEl.Clone();

            _logger.LogDebug("Sportscore event medias: event_id={EventId}, count={Count}", eventId, medias.Count);
            return new SportscoreMediasResult { Success = true, Medias = medias, Meta = meta };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sportscore event medias request failed");
            return new SportscoreMediasResult { Success = false, Error = ex.Message };
        }
    }

    public async Task<SportscoreChallengesResult> GetLeagueChallengesAsync(int leagueId, int page = 1, CancellationToken cancellationToken = default)
    {
        var baseUrl = _options.BaseUrl?.Trim().TrimEnd('/');
        var host = _options.Host?.Trim();
        var apiKey = _options.ApiKey?.Trim();

        if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(host) || string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("Sportscore API not configured (BaseUrl, Host, ApiKey)");
            return new SportscoreChallengesResult { Success = false, Error = "Sportscore API not configured." };
        }

        var url = $"{baseUrl}/leagues/{leagueId}/challenges?page={page}";

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
                _logger.LogWarning("Sportscore league challenges API returned {StatusCode}: {Body}", response.StatusCode, body);
                return new SportscoreChallengesResult { Success = false, Error = $"API returned {response.StatusCode}." };
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var challenges = new List<JsonElement>();
            if (root.TryGetProperty("data", out var dataArr) && dataArr.ValueKind == JsonValueKind.Array)
            {
                foreach (var challenge in dataArr.EnumerateArray())
                    challenges.Add(challenge.Clone());
            }

            JsonElement? meta = null;
            if (root.TryGetProperty("meta", out var metaEl))
                meta = metaEl.Clone();

            _logger.LogDebug("Sportscore league challenges: league_id={LeagueId}, count={Count}", leagueId, challenges.Count);
            return new SportscoreChallengesResult { Success = true, Challenges = challenges, Meta = meta };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sportscore league challenges request failed");
            return new SportscoreChallengesResult { Success = false, Error = ex.Message };
        }
    }

    public async Task<SportscoreSeasonsResult> GetLeagueSeasonsAsync(int leagueId, int page = 1, CancellationToken cancellationToken = default)
    {
        var baseUrl = _options.BaseUrl?.Trim().TrimEnd('/');
        var host = _options.Host?.Trim();
        var apiKey = _options.ApiKey?.Trim();

        if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(host) || string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("Sportscore API not configured (BaseUrl, Host, ApiKey)");
            return new SportscoreSeasonsResult { Success = false, Error = "Sportscore API not configured." };
        }

        var url = $"{baseUrl}/leagues/{leagueId}/seasons?page={page}";

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
                _logger.LogWarning("Sportscore league seasons API returned {StatusCode}: {Body}", response.StatusCode, body);
                return new SportscoreSeasonsResult { Success = false, Error = $"API returned {response.StatusCode}." };
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var seasons = new List<JsonElement>();
            if (root.TryGetProperty("data", out var dataArr) && dataArr.ValueKind == JsonValueKind.Array)
            {
                foreach (var season in dataArr.EnumerateArray())
                    seasons.Add(season.Clone());
            }

            JsonElement? meta = null;
            if (root.TryGetProperty("meta", out var metaEl))
                meta = metaEl.Clone();

            _logger.LogDebug("Sportscore league seasons: league_id={LeagueId}, count={Count}", leagueId, seasons.Count);
            return new SportscoreSeasonsResult { Success = true, Seasons = seasons, Meta = meta };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sportscore league seasons request failed");
            return new SportscoreSeasonsResult { Success = false, Error = ex.Message };
        }
    }

    public async Task<SportscoreStandingsResult> GetSeasonStandingsAsync(int seasonId, CancellationToken cancellationToken = default)
    {
        var baseUrl = _options.BaseUrl?.Trim().TrimEnd('/');
        var host = _options.Host?.Trim();
        var apiKey = _options.ApiKey?.Trim();

        if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(host) || string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("Sportscore API not configured (BaseUrl, Host, ApiKey)");
            return new SportscoreStandingsResult { Success = false, Error = "Sportscore API not configured." };
        }

        var url = $"{baseUrl}/seasons/{seasonId}/standings-tables";

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
                _logger.LogWarning("Sportscore season standings API returned {StatusCode}: {Body}", response.StatusCode, body);
                return new SportscoreStandingsResult { Success = false, Error = $"API returned {response.StatusCode}." };
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var standings = new List<JsonElement>();
            if (root.TryGetProperty("data", out var dataArr) && dataArr.ValueKind == JsonValueKind.Array)
            {
                foreach (var standing in dataArr.EnumerateArray())
                    standings.Add(standing.Clone());
            }
            else if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var standing in root.EnumerateArray())
                    standings.Add(standing.Clone());
            }

            JsonElement? meta = null;
            if (root.TryGetProperty("meta", out var metaEl))
                meta = metaEl.Clone();

            _logger.LogDebug("Sportscore season standings: season_id={SeasonId}, count={Count}", seasonId, standings.Count);
            return new SportscoreStandingsResult { Success = true, Standings = standings, Meta = meta };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sportscore season standings request failed");
            return new SportscoreStandingsResult { Success = false, Error = ex.Message };
        }
    }

    public async Task<SportscoreLiveEventsResult> SearchEventsAsync(EventSearchParams searchParams, CancellationToken cancellationToken = default)
    {
        var baseUrl = _options.BaseUrl?.Trim().TrimEnd('/');
        var host = _options.Host?.Trim();
        var apiKey = _options.ApiKey?.Trim();

        if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(host) || string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("Sportscore API not configured (BaseUrl, Host, ApiKey)");
            return new SportscoreLiveEventsResult { Success = false, Error = "Sportscore API not configured." };
        }

        // Build query string from search parameters
        var queryParams = new List<string>();
        if (searchParams.SportId.HasValue)
            queryParams.Add($"sport_id={searchParams.SportId.Value}");
        if (searchParams.LeagueId.HasValue)
            queryParams.Add($"league_id={searchParams.LeagueId.Value}");
        if (searchParams.ChallengeId.HasValue)
            queryParams.Add($"challenge_id={searchParams.ChallengeId.Value}");
        if (searchParams.SeasonId.HasValue)
            queryParams.Add($"season_id={searchParams.SeasonId.Value}");
        if (searchParams.HomeTeamId.HasValue)
            queryParams.Add($"home_team_id={searchParams.HomeTeamId.Value}");
        if (searchParams.AwayTeamId.HasValue)
            queryParams.Add($"away_team_id={searchParams.AwayTeamId.Value}");
        if (searchParams.VenueId.HasValue)
            queryParams.Add($"venue_id={searchParams.VenueId.Value}");
        if (searchParams.RefereeId.HasValue)
            queryParams.Add($"referee_id={searchParams.RefereeId.Value}");
        if (!string.IsNullOrEmpty(searchParams.Status))
            queryParams.Add($"status={Uri.EscapeDataString(searchParams.Status)}");
        if (searchParams.DateStart.HasValue)
            queryParams.Add($"date_start={searchParams.DateStart.Value:yyyy-MM-dd}");
        if (searchParams.DateEnd.HasValue)
            queryParams.Add($"date_end={searchParams.DateEnd.Value:yyyy-MM-dd}");
        queryParams.Add($"page={searchParams.Page}");

        var queryString = string.Join("&", queryParams);
        var url = $"{baseUrl}/events/search?{queryString}";

        try
        {
            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("x-rapidapi-host", host);
            request.Headers.Add("x-rapidapi-key", apiKey);
            request.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogWarning("Sportscore search events API returned {StatusCode}: {Body}", response.StatusCode, body);
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

            _logger.LogDebug("Sportscore search events: count={Count}, page={Page}", events.Count, searchParams.Page);
            return new SportscoreLiveEventsResult { Success = true, Events = events, Meta = meta };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sportscore search events request failed");
            return new SportscoreLiveEventsResult { Success = false, Error = ex.Message };
        }
    }

    public async Task<SportscoreLiveEventsResult> SearchEventsBySimilarNameAsync(string name, DateTime? date = null, int sportId = 1, int page = 1, string locale = "en", CancellationToken cancellationToken = default)
    {
        var baseUrl = _options.BaseUrl?.Trim().TrimEnd('/');
        var host = _options.Host?.Trim();
        var apiKey = _options.ApiKey?.Trim();

        if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(host) || string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("Sportscore API not configured (BaseUrl, Host, ApiKey)");
            return new SportscoreLiveEventsResult { Success = false, Error = "Sportscore API not configured." };
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return new SportscoreLiveEventsResult { Success = false, Error = "Event name is required." };
        }

        // Build query string
        var queryParams = new List<string>
        {
            $"sport_id={sportId}",
            $"page={page}",
            $"locale={Uri.EscapeDataString(locale)}",
            $"name={Uri.EscapeDataString(name)}"
        };

        if (date.HasValue)
        {
            queryParams.Add($"date={date.Value:yyyy-MM-dd}");
        }

        var queryString = string.Join("&", queryParams);
        var url = $"{baseUrl}/events/search-similar-name?{queryString}";

        try
        {
            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("x-rapidapi-host", host);
            request.Headers.Add("x-rapidapi-key", apiKey);
            request.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogWarning("Sportscore search similar name API returned {StatusCode}: {Body}", response.StatusCode, body);
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

            _logger.LogDebug("Sportscore search similar name: name={Name}, date={Date}, count={Count}, page={Page}", name, date, events.Count, page);
            return new SportscoreLiveEventsResult { Success = true, Events = events, Meta = meta };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sportscore search similar name request failed");
            return new SportscoreLiveEventsResult { Success = false, Error = ex.Message };
        }
    }
}
