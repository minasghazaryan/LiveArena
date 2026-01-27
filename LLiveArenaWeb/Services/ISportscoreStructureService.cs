using System.Text.Json;

namespace LLiveArenaWeb.Services;

/// <summary>Fetches structural data (sports, sections, leagues, challenges) from Sportscore API.</summary>
public interface ISportscoreStructureService
{
    /// <summary>GET /sports - Get a list of all available sports.</summary>
    Task<SportscoreSportsResult> GetSportsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>GET /sports/{sport_id}/sections - Get all sections by sport ID.</summary>
    Task<SportscoreSectionsResult> GetSectionsBySportAsync(int sportId, CancellationToken cancellationToken = default);
    
    /// <summary>GET /sections/{section_id}/leagues - Get leagues by section ID.</summary>
    Task<SportscoreLeaguesResult> GetLeaguesBySectionAsync(int sectionId, int page = 1, CancellationToken cancellationToken = default);
    
    /// <summary>GET /sections/{section_id}/challenges - Get challenges by section ID.</summary>
    Task<SportscoreChallengesResult> GetChallengesBySectionAsync(int sectionId, int page = 1, CancellationToken cancellationToken = default);
}

/// <summary>Sports response with sports data.</summary>
public class SportscoreSportsResult
{
    public List<JsonElement> Sports { get; init; } = new();
    public JsonElement? Meta { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
}

/// <summary>Sections response with sections data.</summary>
public class SportscoreSectionsResult
{
    public List<JsonElement> Sections { get; init; } = new();
    public JsonElement? Meta { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
}

/// <summary>Leagues response with leagues data.</summary>
public class SportscoreLeaguesResult
{
    public List<JsonElement> Leagues { get; init; } = new();
    public JsonElement? Meta { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
}

// Note: SportscoreChallengesResult is already defined in ILiveEventsService.cs and is reused here
