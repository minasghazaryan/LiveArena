using System.Text.Json;

namespace LLiveArenaWeb.Services;

/// <summary>Search teams via Sportscore POST /teams/search. All parameters optional.</summary>
public interface ITeamSearchService
{
    Task<SportscoreTeamSearchResult> SearchTeamsAsync(TeamSearchParams? parameters = null, CancellationToken cancellationToken = default);
}

public class TeamSearchParams
{
    public string? Name { get; set; }
    public int? SportId { get; set; }
    public int? SectionId { get; set; }
    public string? Country { get; set; }
    public bool? IsNational { get; set; }
    public string? Locale { get; set; }
    public int Page { get; set; } = 1;
}

public class SportscoreTeamSearchResult
{
    public List<JsonElement> Teams { get; init; } = new();
    public JsonElement? Meta { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
}
