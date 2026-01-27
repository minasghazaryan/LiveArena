using System.Text.Json;
using Microsoft.Extensions.Options;

namespace LLiveArenaWeb.Services;

public class SportscoreStructureService : ISportscoreStructureService
{
    private readonly SportscoreOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SportscoreStructureService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public SportscoreStructureService(IOptions<SportscoreOptions> options, IHttpClientFactory httpClientFactory, ILogger<SportscoreStructureService> logger)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<SportscoreSportsResult> GetSportsAsync(CancellationToken cancellationToken = default)
    {
        var baseUrl = _options.BaseUrl?.Trim().TrimEnd('/');
        var host = _options.Host?.Trim();
        var apiKey = _options.ApiKey?.Trim();

        if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(host) || string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("Sportscore API not configured (BaseUrl, Host, ApiKey)");
            return new SportscoreSportsResult { Success = false, Error = "Sportscore API not configured." };
        }

        var url = $"{baseUrl}/sports";

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
                _logger.LogWarning("Sportscore sports API returned {StatusCode}: {Body}", response.StatusCode, body);
                return new SportscoreSportsResult { Success = false, Error = $"API returned {response.StatusCode}." };
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var sports = new List<JsonElement>();
            if (root.TryGetProperty("data", out var dataArr) && dataArr.ValueKind == JsonValueKind.Array)
            {
                foreach (var sport in dataArr.EnumerateArray())
                    sports.Add(sport.Clone());
            }
            else if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var sport in root.EnumerateArray())
                    sports.Add(sport.Clone());
            }

            JsonElement? meta = null;
            if (root.TryGetProperty("meta", out var metaEl))
                meta = metaEl.Clone();

            _logger.LogDebug("Sportscore sports: count={Count}", sports.Count);
            return new SportscoreSportsResult { Success = true, Sports = sports, Meta = meta };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sportscore sports request failed");
            return new SportscoreSportsResult { Success = false, Error = ex.Message };
        }
    }

    public async Task<SportscoreSectionsResult> GetSectionsBySportAsync(int sportId, CancellationToken cancellationToken = default)
    {
        var baseUrl = _options.BaseUrl?.Trim().TrimEnd('/');
        var host = _options.Host?.Trim();
        var apiKey = _options.ApiKey?.Trim();

        if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(host) || string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("Sportscore API not configured (BaseUrl, Host, ApiKey)");
            return new SportscoreSectionsResult { Success = false, Error = "Sportscore API not configured." };
        }

        var url = $"{baseUrl}/sports/{sportId}/sections";

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
                _logger.LogWarning("Sportscore sections by sport API returned {StatusCode}: {Body}", response.StatusCode, body);
                return new SportscoreSectionsResult { Success = false, Error = $"API returned {response.StatusCode}." };
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var sections = new List<JsonElement>();
            if (root.TryGetProperty("data", out var dataArr) && dataArr.ValueKind == JsonValueKind.Array)
            {
                foreach (var section in dataArr.EnumerateArray())
                    sections.Add(section.Clone());
            }
            else if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var section in root.EnumerateArray())
                    sections.Add(section.Clone());
            }

            JsonElement? meta = null;
            if (root.TryGetProperty("meta", out var metaEl))
                meta = metaEl.Clone();

            _logger.LogDebug("Sportscore sections by sport: sport_id={SportId}, count={Count}", sportId, sections.Count);
            return new SportscoreSectionsResult { Success = true, Sections = sections, Meta = meta };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sportscore sections by sport request failed");
            return new SportscoreSectionsResult { Success = false, Error = ex.Message };
        }
    }

    public async Task<SportscoreLeaguesResult> GetLeaguesBySectionAsync(int sectionId, int page = 1, CancellationToken cancellationToken = default)
    {
        var baseUrl = _options.BaseUrl?.Trim().TrimEnd('/');
        var host = _options.Host?.Trim();
        var apiKey = _options.ApiKey?.Trim();

        if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(host) || string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("Sportscore API not configured (BaseUrl, Host, ApiKey)");
            return new SportscoreLeaguesResult { Success = false, Error = "Sportscore API not configured." };
        }

        var url = $"{baseUrl}/sections/{sectionId}/leagues?page={page}";

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
                _logger.LogWarning("Sportscore leagues by section API returned {StatusCode}: {Body}", response.StatusCode, body);
                return new SportscoreLeaguesResult { Success = false, Error = $"API returned {response.StatusCode}." };
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var leagues = new List<JsonElement>();
            if (root.TryGetProperty("data", out var dataArr) && dataArr.ValueKind == JsonValueKind.Array)
            {
                foreach (var league in dataArr.EnumerateArray())
                    leagues.Add(league.Clone());
            }
            else if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var league in root.EnumerateArray())
                    leagues.Add(league.Clone());
            }

            JsonElement? meta = null;
            if (root.TryGetProperty("meta", out var metaEl))
                meta = metaEl.Clone();

            _logger.LogDebug("Sportscore leagues by section: section_id={SectionId}, page={Page}, count={Count}", sectionId, page, leagues.Count);
            return new SportscoreLeaguesResult { Success = true, Leagues = leagues, Meta = meta };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sportscore leagues by section request failed");
            return new SportscoreLeaguesResult { Success = false, Error = ex.Message };
        }
    }

    public async Task<SportscoreChallengesResult> GetChallengesBySectionAsync(int sectionId, int page = 1, CancellationToken cancellationToken = default)
    {
        var baseUrl = _options.BaseUrl?.Trim().TrimEnd('/');
        var host = _options.Host?.Trim();
        var apiKey = _options.ApiKey?.Trim();

        if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(host) || string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("Sportscore API not configured (BaseUrl, Host, ApiKey)");
            return new SportscoreChallengesResult { Success = false, Error = "Sportscore API not configured." };
        }

        var url = $"{baseUrl}/sections/{sectionId}/challenges?page={page}";

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
                _logger.LogWarning("Sportscore challenges by section API returned {StatusCode}: {Body}", response.StatusCode, body);
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
            else if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var challenge in root.EnumerateArray())
                    challenges.Add(challenge.Clone());
            }

            JsonElement? meta = null;
            if (root.TryGetProperty("meta", out var metaEl))
                meta = metaEl.Clone();

            _logger.LogDebug("Sportscore challenges by section: section_id={SectionId}, page={Page}, count={Count}", sectionId, page, challenges.Count);
            return new SportscoreChallengesResult { Success = true, Challenges = challenges, Meta = meta };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sportscore challenges by section request failed");
            return new SportscoreChallengesResult { Success = false, Error = ex.Message };
        }
    }
}
