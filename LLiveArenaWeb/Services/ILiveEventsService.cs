using System.Text.Json;

namespace LLiveArenaWeb.Services;

/// <summary>Fetches live events by sport_id from Sportscore (sportscore1.p.rapidapi.com) API.</summary>
public interface ILiveEventsService
{
    /// <summary>GET /sports/{sportId}/events/live?page={page}</summary>
    Task<SportscoreLiveEventsResult> GetLiveEventsAsync(int sportId, int page = 1, CancellationToken cancellationToken = default);
    
    /// <summary>GET /sports/{sportId}/events/date/{date}?page={page} - Get events by sport_id and date.</summary>
    Task<SportscoreLiveEventsResult> GetEventsByDateAsync(int sportId, DateTime date, int page = 1, CancellationToken cancellationToken = default);
    
    /// <summary>GET /events/{eventId} - Get detailed event information including teams, odds, statistics.</summary>
    Task<SportscoreEventDetailsResult> GetEventDetailsAsync(int eventId, CancellationToken cancellationToken = default);
    
    /// <summary>GET /events/{eventId}/statistics - Get event statistics.</summary>
    Task<SportscoreStatisticsResult> GetEventStatisticsAsync(int eventId, CancellationToken cancellationToken = default);
    
    /// <summary>GET /events/{eventId}/lineups - Get event lineups.</summary>
    Task<SportscoreLineupsResult> GetEventLineupsAsync(int eventId, CancellationToken cancellationToken = default);
    
    /// <summary>GET /events/{eventId}/incidents - Get event incidents (goals, cards, substitutions, etc.).</summary>
    Task<SportscoreIncidentsResult> GetEventIncidentsAsync(int eventId, CancellationToken cancellationToken = default);

    /// <summary>GET /events/{eventId}/medias?page={page} - Get event media items (highlights).</summary>
    Task<SportscoreMediasResult> GetEventMediasAsync(int eventId, int page = 1, CancellationToken cancellationToken = default);
    
    /// <summary>GET /leagues/{leagueId}/challenges - Get challenges for a league.</summary>
    Task<SportscoreChallengesResult> GetLeagueChallengesAsync(int leagueId, int page = 1, CancellationToken cancellationToken = default);
    
    /// <summary>GET /leagues/{leagueId}/seasons - Get seasons for a league.</summary>
    Task<SportscoreSeasonsResult> GetLeagueSeasonsAsync(int leagueId, int page = 1, CancellationToken cancellationToken = default);
    
    /// <summary>GET /seasons/{seasonId}/standings-tables - Get standings tables for a season.</summary>
    Task<SportscoreStandingsResult> GetSeasonStandingsAsync(int seasonId, CancellationToken cancellationToken = default);
    
    /// <summary>POST /events/search - Search events with filters (sport_id, league_id, team_id, date range, status, etc.).</summary>
    Task<SportscoreLiveEventsResult> SearchEventsAsync(EventSearchParams searchParams, CancellationToken cancellationToken = default);
    
    /// <summary>POST /events/search-similar-name - Search events by name and date (more user-friendly).</summary>
    Task<SportscoreLiveEventsResult> SearchEventsBySimilarNameAsync(string name, DateTime? date = null, int sportId = 1, int page = 1, string locale = "en", CancellationToken cancellationToken = default);
}

/// <summary>Parameters for event search.</summary>
public class EventSearchParams
{
    public int? SportId { get; set; }
    public int? LeagueId { get; set; }
    public int? ChallengeId { get; set; }
    public int? SeasonId { get; set; }
    public int? HomeTeamId { get; set; }
    public int? AwayTeamId { get; set; }
    public int? VenueId { get; set; }
    public int? RefereeId { get; set; }
    public string? Status { get; set; } // e.g., "live", "finished", "postponed", "scheduled"
    public DateTime? DateStart { get; set; }
    public DateTime? DateEnd { get; set; }
    public int Page { get; set; } = 1;
}

/// <summary>Statistics response with statistics data.</summary>
public class SportscoreStatisticsResult
{
    public List<JsonElement> Statistics { get; init; } = new();
    public JsonElement? Meta { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
}

/// <summary>Lineups response with lineup data.</summary>
public class SportscoreLineupsResult
{
    public List<JsonElement> Lineups { get; init; } = new();
    public JsonElement? Meta { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
}

/// <summary>Incidents response with incident data.</summary>
public class SportscoreIncidentsResult
{
    public List<JsonElement> Incidents { get; init; } = new();
    public JsonElement? Meta { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
}

/// <summary>Media response with highlights or related video data.</summary>
public class SportscoreMediasResult
{
    public List<JsonElement> Medias { get; init; } = new();
    public JsonElement? Meta { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
}

/// <summary>Challenges response with challenge data.</summary>
public class SportscoreChallengesResult
{
    public List<JsonElement> Challenges { get; init; } = new();
    public JsonElement? Meta { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
}

/// <summary>Seasons response with season data.</summary>
public class SportscoreSeasonsResult
{
    public List<JsonElement> Seasons { get; init; } = new();
    public JsonElement? Meta { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
}

/// <summary>Standings response with standings table data.</summary>
public class SportscoreStandingsResult
{
    public List<JsonElement> Standings { get; init; } = new();
    public JsonElement? Meta { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
}

/// <summary>Event details response with full event data.</summary>
public class SportscoreEventDetailsResult
{
    public System.Text.Json.JsonElement? Event { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
}

/// <summary>Raw API response; use Data or Events for the list, Meta for pagination if present.</summary>
public class SportscoreLiveEventsResult
{
    public List<JsonElement> Events { get; init; } = new();
    public JsonElement? Meta { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
}
