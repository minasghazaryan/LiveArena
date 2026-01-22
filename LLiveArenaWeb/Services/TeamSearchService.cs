using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace LLiveArenaWeb.Services;

public class TeamSearchService : ITeamSearchService
{
    private readonly SportscoreOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TeamSearchService> _logger;

    public TeamSearchService(IOptions<SportscoreOptions> options, IHttpClientFactory httpClientFactory, ILogger<TeamSearchService> logger)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<SportscoreTeamSearchResult> SearchTeamsAsync(TeamSearchParams? parameters = null, CancellationToken cancellationToken = default)
    {
        var baseUrl = _options.BaseUrl?.Trim().TrimEnd('/');
        var host = _options.Host?.Trim();
        var apiKey = _options.ApiKey?.Trim();

        if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(host) || string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("Sportscore API not configured (BaseUrl, Host, ApiKey)");
            return new SportscoreTeamSearchResult { Success = false, Error = "Sportscore API not configured." };
        }

        var qs = BuildQueryString(parameters);
        var url = $"{baseUrl}/teams/search{qs}";

        try
        {
            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("x-rapidapi-host", host);
            request.Headers.Add("x-rapidapi-key", apiKey);
            request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogWarning("Sportscore team search API returned {StatusCode}: {Body}", response.StatusCode, body);
                return new SportscoreTeamSearchResult { Success = false, Error = $"API returned {response.StatusCode}." };
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var teams = new List<JsonElement>();
            if (root.TryGetProperty("data", out var dataArr) && dataArr.ValueKind == JsonValueKind.Array)
            {
                foreach (var e in dataArr.EnumerateArray())
                    teams.Add(e.Clone());
            }
            else if (root.TryGetProperty("teams", out var teamsArr) && teamsArr.ValueKind == JsonValueKind.Array)
            {
                foreach (var e in teamsArr.EnumerateArray())
                    teams.Add(e.Clone());
            }

            JsonElement? meta = null;
            if (root.TryGetProperty("meta", out var metaEl))
                meta = metaEl.Clone();

            _logger.LogDebug("Sportscore team search: count={Count}", teams.Count);
            return new SportscoreTeamSearchResult { Success = true, Teams = teams, Meta = meta };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sportscore team search request failed");
            return new SportscoreTeamSearchResult { Success = false, Error = ex.Message };
        }
    }

    private static string BuildQueryString(TeamSearchParams? p)
    {
        if (p == null)
            return "?page=1";

        var q = new List<string>();
        if (p.SportId.HasValue) q.Add($"sport_id={p.SportId.Value}");
        if (p.SectionId.HasValue) q.Add($"section_id={p.SectionId.Value}");
        if (p.IsNational.HasValue) q.Add($"is_national={p.IsNational.Value.ToString().ToLowerInvariant()}");
        if (!string.IsNullOrWhiteSpace(p.Locale)) q.Add($"locale={Uri.EscapeDataString(p.Locale.Trim())}");
        if (!string.IsNullOrWhiteSpace(p.Name)) q.Add($"name={Uri.EscapeDataString(p.Name.Trim())}");
        if (p.Page > 0) q.Add($"page={p.Page}");
        if (!string.IsNullOrWhiteSpace(p.Country)) q.Add($"country={Uri.EscapeDataString(p.Country.Trim())}");

        if (q.Count == 0)
            return "?page=1";
        return "?" + string.Join("&", q);
    }
}
