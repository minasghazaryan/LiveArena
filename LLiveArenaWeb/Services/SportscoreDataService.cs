using System.Text.Json;
using Microsoft.Extensions.Options;

namespace LLiveArenaWeb.Services;

public class SportscoreDataService : ISportscoreDataService
{
    private readonly SportscoreOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SportscoreDataService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public SportscoreDataService(IOptions<SportscoreOptions> options, IHttpClientFactory httpClientFactory, ILogger<SportscoreDataService> logger)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    private async Task<(bool Success, string? Error, JsonElement? Data)> ExecuteRequestAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = _options.BaseUrl?.Trim().TrimEnd('/');
        var host = _options.Host?.Trim();
        var apiKey = _options.ApiKey?.Trim();

        if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(host) || string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("Sportscore API not configured (BaseUrl, Host, ApiKey)");
            return (false, "Sportscore API not configured.", null);
        }

        var fullUrl = $"{baseUrl}/{url.TrimStart('/')}";

        try
        {
            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, fullUrl);
            request.Headers.Add("x-rapidapi-host", host);
            request.Headers.Add("x-rapidapi-key", apiKey);

            var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);

            // Handle rate limiting (429) with retry logic
            if ((int)response.StatusCode == 429)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogWarning("Sportscore API rate limit exceeded (429) for {Url}. Waiting before retry...", fullUrl);
                
                // Wait 2 seconds before retrying once
                await Task.Delay(2000, cancellationToken).ConfigureAwait(false);
                
                // Retry the request once
                response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
                
                if ((int)response.StatusCode == 429)
                {
                    _logger.LogWarning("Sportscore API rate limit still exceeded after retry for {Url}", fullUrl);
                    return (false, "API rate limit exceeded. Please try again later.", null);
                }
            }

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                
                // 404 (Not Found) is expected for some endpoints that may not exist for all events
                // Don't log as warning, just return failure
                if ((int)response.StatusCode == 404)
                {
                    _logger.LogDebug("Sportscore API endpoint not found (404) for {Url}", fullUrl);
                    return (false, "Endpoint not found.", null);
                }
                
                // Log other errors as warnings
                _logger.LogWarning("Sportscore API returned {StatusCode}: {Body} for {Url}", response.StatusCode, body, fullUrl);
                return (false, $"API returned {response.StatusCode}.", null);
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement.Clone(); // Clone to make it independent of the disposed JsonDocument

            return (true, null, root);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sportscore API request failed for {Url}", fullUrl);
            return (false, ex.Message, null);
        }
    }

    // Teams endpoints
    public async Task<SportscoreTeamResult> GetTeamAsync(int teamId, CancellationToken cancellationToken = default)
    {
        var (success, error, root) = await ExecuteRequestAsync($"teams/{teamId}", cancellationToken);
        if (!success || root == null)
        {
            return new SportscoreTeamResult { Success = false, Error = error };
        }

        JsonElement? team = null;
        if (root.Value.TryGetProperty("data", out var dataEl))
            team = dataEl.Clone();
        else
            team = root.Value.Clone();

        _logger.LogDebug("Sportscore team: team_id={TeamId}", teamId);
        return new SportscoreTeamResult { Success = true, Team = team };
    }

    public async Task<SportscorePlayersResult> GetTeamPlayersAsync(int teamId, int page = 1, CancellationToken cancellationToken = default)
    {
        var (success, error, root) = await ExecuteRequestAsync($"teams/{teamId}/players?page={page}", cancellationToken);
        if (!success || root == null)
        {
            return new SportscorePlayersResult { Success = false, Error = error };
        }

        var players = new List<JsonElement>();
        if (root.Value.TryGetProperty("data", out var dataArr) && dataArr.ValueKind == JsonValueKind.Array)
        {
            foreach (var player in dataArr.EnumerateArray())
                players.Add(player.Clone());
        }
        else if (root.Value.ValueKind == JsonValueKind.Array)
        {
            foreach (var player in root.Value.EnumerateArray())
                players.Add(player.Clone());
        }

        JsonElement? meta = null;
        if (root.Value.TryGetProperty("meta", out var metaEl))
            meta = metaEl.Clone();

        _logger.LogDebug("Sportscore team players: team_id={TeamId}, page={Page}, count={Count}", teamId, page, players.Count);
        return new SportscorePlayersResult { Success = true, Players = players, Meta = meta };
    }

    public async Task<SportscoreManagersResult> GetTeamManagersAsync(int teamId, int page = 1, CancellationToken cancellationToken = default)
    {
        var (success, error, root) = await ExecuteRequestAsync($"teams/{teamId}/managers?page={page}", cancellationToken);
        if (!success || root == null)
        {
            return new SportscoreManagersResult { Success = false, Error = error };
        }

        var managers = new List<JsonElement>();
        if (root.Value.TryGetProperty("data", out var dataArr) && dataArr.ValueKind == JsonValueKind.Array)
        {
            foreach (var manager in dataArr.EnumerateArray())
                managers.Add(manager.Clone());
        }
        else if (root.Value.ValueKind == JsonValueKind.Array)
        {
            foreach (var manager in root.Value.EnumerateArray())
                managers.Add(manager.Clone());
        }

        JsonElement? meta = null;
        if (root.Value.TryGetProperty("meta", out var metaEl))
            meta = metaEl.Clone();

        _logger.LogDebug("Sportscore team managers: team_id={TeamId}, page={Page}, count={Count}", teamId, page, managers.Count);
        return new SportscoreManagersResult { Success = true, Managers = managers, Meta = meta };
    }

    public async Task<SportscoreTeamsResult> GetTeamsByLeagueAsync(int leagueId, int page = 1, CancellationToken cancellationToken = default)
    {
        var (success, error, root) = await ExecuteRequestAsync($"leagues/{leagueId}/teams?page={page}", cancellationToken);
        if (!success || root == null)
        {
            return new SportscoreTeamsResult { Success = false, Error = error };
        }

        var teams = new List<JsonElement>();
        if (root.Value.TryGetProperty("data", out var dataArr) && dataArr.ValueKind == JsonValueKind.Array)
        {
            foreach (var team in dataArr.EnumerateArray())
                teams.Add(team.Clone());
        }
        else if (root.Value.ValueKind == JsonValueKind.Array)
        {
            foreach (var team in root.Value.EnumerateArray())
                teams.Add(team.Clone());
        }

        JsonElement? meta = null;
        if (root.Value.TryGetProperty("meta", out var metaEl))
            meta = metaEl.Clone();

        _logger.LogDebug("Sportscore teams by league: league_id={LeagueId}, page={Page}, count={Count}", leagueId, page, teams.Count);
        return new SportscoreTeamsResult { Success = true, Teams = teams, Meta = meta };
    }

    public async Task<SportscoreTeamsResult> GetTeamsBySeasonAsync(int seasonId, int page = 1, CancellationToken cancellationToken = default)
    {
        var (success, error, root) = await ExecuteRequestAsync($"seasons/{seasonId}/teams?page={page}", cancellationToken);
        if (!success || root == null)
        {
            return new SportscoreTeamsResult { Success = false, Error = error };
        }

        var teams = new List<JsonElement>();
        if (root.Value.TryGetProperty("data", out var dataArr) && dataArr.ValueKind == JsonValueKind.Array)
        {
            foreach (var team in dataArr.EnumerateArray())
                teams.Add(team.Clone());
        }
        else if (root.Value.ValueKind == JsonValueKind.Array)
        {
            foreach (var team in root.Value.EnumerateArray())
                teams.Add(team.Clone());
        }

        JsonElement? meta = null;
        if (root.Value.TryGetProperty("meta", out var metaEl))
            meta = metaEl.Clone();

        _logger.LogDebug("Sportscore teams by season: season_id={SeasonId}, page={Page}, count={Count}", seasonId, page, teams.Count);
        return new SportscoreTeamsResult { Success = true, Teams = teams, Meta = meta };
    }

    // Players endpoints
    public async Task<SportscorePlayerResult> GetPlayerAsync(int playerId, CancellationToken cancellationToken = default)
    {
        var (success, error, root) = await ExecuteRequestAsync($"players/{playerId}", cancellationToken);
        if (!success || root == null)
        {
            return new SportscorePlayerResult { Success = false, Error = error };
        }

        JsonElement? player = null;
        if (root.Value.TryGetProperty("data", out var dataEl))
            player = dataEl.Clone();
        else
            player = root.Value.Clone();

        _logger.LogDebug("Sportscore player: player_id={PlayerId}", playerId);
        return new SportscorePlayerResult { Success = true, Player = player };
    }

    public async Task<SportscorePlayerStatisticsResult> GetPlayerStatisticsAsync(int playerId, CancellationToken cancellationToken = default)
    {
        var (success, error, root) = await ExecuteRequestAsync($"players/{playerId}/statistics", cancellationToken);
        if (!success || root == null)
        {
            return new SportscorePlayerStatisticsResult { Success = false, Error = error };
        }

        var statistics = new List<JsonElement>();
        if (root.Value.TryGetProperty("data", out var dataArr) && dataArr.ValueKind == JsonValueKind.Array)
        {
            foreach (var stat in dataArr.EnumerateArray())
                statistics.Add(stat.Clone());
        }
        else if (root.Value.ValueKind == JsonValueKind.Array)
        {
            foreach (var stat in root.Value.EnumerateArray())
                statistics.Add(stat.Clone());
        }

        JsonElement? meta = null;
        if (root.Value.TryGetProperty("meta", out var metaEl))
            meta = metaEl.Clone();

        _logger.LogDebug("Sportscore player statistics: player_id={PlayerId}, count={Count}", playerId, statistics.Count);
        return new SportscorePlayerStatisticsResult { Success = true, Statistics = statistics, Meta = meta };
    }

    // Managers endpoints
    public async Task<SportscoreManagerResult> GetManagerAsync(int managerId, CancellationToken cancellationToken = default)
    {
        var (success, error, root) = await ExecuteRequestAsync($"managers/{managerId}", cancellationToken);
        if (!success || root == null)
        {
            return new SportscoreManagerResult { Success = false, Error = error };
        }

        JsonElement? manager = null;
        if (root.Value.TryGetProperty("data", out var dataEl))
            manager = dataEl.Clone();
        else
            manager = root.Value.Clone();

        _logger.LogDebug("Sportscore manager: manager_id={ManagerId}", managerId);
        return new SportscoreManagerResult { Success = true, Manager = manager };
    }

    // Venues endpoints
    public async Task<SportscoreVenueResult> GetVenueAsync(int venueId, CancellationToken cancellationToken = default)
    {
        var (success, error, root) = await ExecuteRequestAsync($"venues/{venueId}", cancellationToken);
        if (!success || root == null)
        {
            return new SportscoreVenueResult { Success = false, Error = error };
        }

        JsonElement? venue = null;
        if (root.Value.TryGetProperty("data", out var dataEl))
            venue = dataEl.Clone();
        else
            venue = root.Value.Clone();

        _logger.LogDebug("Sportscore venue: venue_id={VenueId}", venueId);
        return new SportscoreVenueResult { Success = true, Venue = venue };
    }

    public async Task<SportscoreVenueResult> GetEventVenueAsync(int eventId, CancellationToken cancellationToken = default)
    {
        var (success, error, root) = await ExecuteRequestAsync($"events/{eventId}/venue", cancellationToken);
        if (!success || root == null)
        {
            return new SportscoreVenueResult { Success = false, Error = error };
        }

        JsonElement? venue = null;
        if (root.Value.TryGetProperty("data", out var dataEl))
            venue = dataEl.Clone();
        else
            venue = root.Value.Clone();

        _logger.LogDebug("Sportscore event venue: event_id={EventId}", eventId);
        return new SportscoreVenueResult { Success = true, Venue = venue };
    }

    // Referees endpoints
    public async Task<SportscoreRefereeResult> GetRefereeAsync(int refereeId, CancellationToken cancellationToken = default)
    {
        var (success, error, root) = await ExecuteRequestAsync($"referees/{refereeId}", cancellationToken);
        if (!success || root == null)
        {
            return new SportscoreRefereeResult { Success = false, Error = error };
        }

        JsonElement? referee = null;
        if (root.Value.TryGetProperty("data", out var dataEl))
            referee = dataEl.Clone();
        else
            referee = root.Value.Clone();

        _logger.LogDebug("Sportscore referee: referee_id={RefereeId}", refereeId);
        return new SportscoreRefereeResult { Success = true, Referee = referee };
    }

    public async Task<SportscoreRefereeResult> GetEventRefereeAsync(int eventId, CancellationToken cancellationToken = default)
    {
        var (success, error, root) = await ExecuteRequestAsync($"events/{eventId}/referee", cancellationToken);
        if (!success || root == null)
        {
            return new SportscoreRefereeResult { Success = false, Error = error };
        }

        JsonElement? referee = null;
        if (root.Value.TryGetProperty("data", out var dataEl))
            referee = dataEl.Clone();
        else
            referee = root.Value.Clone();

        _logger.LogDebug("Sportscore event referee: event_id={EventId}", eventId);
        return new SportscoreRefereeResult { Success = true, Referee = referee };
    }

    // Markets/Odds endpoints
    public async Task<SportscoreMarketsResult> GetEventMarketsAsync(int eventId, int page = 1, CancellationToken cancellationToken = default)
    {
        var (success, error, root) = await ExecuteRequestAsync($"events/{eventId}/markets?page={page}", cancellationToken);
        if (!success || root == null)
        {
            return new SportscoreMarketsResult { Success = false, Error = error };
        }

        var markets = new List<JsonElement>();
        if (root.Value.TryGetProperty("data", out var dataArr) && dataArr.ValueKind == JsonValueKind.Array)
        {
            foreach (var market in dataArr.EnumerateArray())
                markets.Add(market.Clone());
        }
        else if (root.Value.ValueKind == JsonValueKind.Array)
        {
            foreach (var market in root.Value.EnumerateArray())
                markets.Add(market.Clone());
        }

        JsonElement? meta = null;
        if (root.Value.TryGetProperty("meta", out var metaEl))
            meta = metaEl.Clone();

        _logger.LogDebug("Sportscore event markets: event_id={EventId}, page={Page}, count={Count}", eventId, page, markets.Count);
        return new SportscoreMarketsResult { Success = true, Markets = markets, Meta = meta };
    }

    public async Task<SportscoreOutcomesResult> GetMarketOutcomesAsync(int marketId, CancellationToken cancellationToken = default)
    {
        var (success, error, root) = await ExecuteRequestAsync($"markets/{marketId}/outcomes", cancellationToken);
        if (!success || root == null)
        {
            return new SportscoreOutcomesResult { Success = false, Error = error };
        }

        var outcomes = new List<JsonElement>();
        if (root.Value.TryGetProperty("data", out var dataArr) && dataArr.ValueKind == JsonValueKind.Array)
        {
            foreach (var outcome in dataArr.EnumerateArray())
                outcomes.Add(outcome.Clone());
        }
        else if (root.Value.ValueKind == JsonValueKind.Array)
        {
            foreach (var outcome in root.Value.EnumerateArray())
                outcomes.Add(outcome.Clone());
        }

        JsonElement? meta = null;
        if (root.Value.TryGetProperty("meta", out var metaEl))
            meta = metaEl.Clone();

        _logger.LogDebug("Sportscore market outcomes: market_id={MarketId}, count={Count}", marketId, outcomes.Count);
        return new SportscoreOutcomesResult { Success = true, Outcomes = outcomes, Meta = meta };
    }

    // Cup tree endpoints
    public async Task<SportscoreCupTreeResult> GetSeasonCupTreeAsync(int seasonId, CancellationToken cancellationToken = default)
    {
        var (success, error, root) = await ExecuteRequestAsync($"seasons/{seasonId}/cup-tree", cancellationToken);
        if (!success || root == null)
        {
            return new SportscoreCupTreeResult { Success = false, Error = error };
        }

        JsonElement? cupTree = null;
        if (root.Value.TryGetProperty("data", out var dataEl))
            cupTree = dataEl.Clone();
        else
            cupTree = root.Value.Clone();

        _logger.LogDebug("Sportscore season cup tree: season_id={SeasonId}", seasonId);
        return new SportscoreCupTreeResult { Success = true, CupTree = cupTree };
    }

    // Other event endpoints
    public async Task<SportscoreH2HResult> GetEventH2HAsync(int eventId, CancellationToken cancellationToken = default)
    {
        var (success, error, root) = await ExecuteRequestAsync($"events/{eventId}/h2h", cancellationToken);
        if (!success || root == null)
        {
            return new SportscoreH2HResult { Success = false, Error = error };
        }

        JsonElement? h2h = null;
        if (root.Value.TryGetProperty("data", out var dataEl))
            h2h = dataEl.Clone();
        else
            h2h = root.Value.Clone();

        _logger.LogDebug("Sportscore event h2h: event_id={EventId}", eventId);
        return new SportscoreH2HResult { Success = true, H2H = h2h };
    }

    public async Task<SportscoreTrendsResult> GetEventTrendsAsync(int eventId, CancellationToken cancellationToken = default)
    {
        var (success, error, root) = await ExecuteRequestAsync($"events/{eventId}/trends", cancellationToken);
        if (!success || root == null)
        {
            return new SportscoreTrendsResult { Success = false, Error = error };
        }

        JsonElement? trends = null;
        if (root.Value.TryGetProperty("data", out var dataEl))
            trends = dataEl.Clone();
        else
            trends = root.Value.Clone();

        _logger.LogDebug("Sportscore event trends: event_id={EventId}", eventId);
        return new SportscoreTrendsResult { Success = true, Trends = trends };
    }
}
