using System.Text.Json;

namespace LLiveArenaWeb.Services;

/// <summary>Fetches additional data (teams, players, managers, venues, referees, markets) from Sportscore API.</summary>
public interface ISportscoreDataService
{
    // Teams endpoints
    /// <summary>GET /teams/{teamId} - Get team details.</summary>
    Task<SportscoreTeamResult> GetTeamAsync(int teamId, CancellationToken cancellationToken = default);
    
    /// <summary>GET /teams/{teamId}/players - Get players by team.</summary>
    Task<SportscorePlayersResult> GetTeamPlayersAsync(int teamId, int page = 1, CancellationToken cancellationToken = default);
    
    /// <summary>GET /teams/{teamId}/managers - Get managers by team.</summary>
    Task<SportscoreManagersResult> GetTeamManagersAsync(int teamId, int page = 1, CancellationToken cancellationToken = default);
    
    /// <summary>GET /leagues/{leagueId}/teams - Get teams by league.</summary>
    Task<SportscoreTeamsResult> GetTeamsByLeagueAsync(int leagueId, int page = 1, CancellationToken cancellationToken = default);
    
    /// <summary>GET /seasons/{seasonId}/teams - Get teams by season.</summary>
    Task<SportscoreTeamsResult> GetTeamsBySeasonAsync(int seasonId, int page = 1, CancellationToken cancellationToken = default);

    // Players endpoints
    /// <summary>GET /players/{playerId} - Get player details.</summary>
    Task<SportscorePlayerResult> GetPlayerAsync(int playerId, CancellationToken cancellationToken = default);
    
    /// <summary>GET /players/{playerId}/statistics - Get player statistics.</summary>
    Task<SportscorePlayerStatisticsResult> GetPlayerStatisticsAsync(int playerId, CancellationToken cancellationToken = default);

    // Managers endpoints
    /// <summary>GET /managers/{managerId} - Get manager details.</summary>
    Task<SportscoreManagerResult> GetManagerAsync(int managerId, CancellationToken cancellationToken = default);

    // Venues endpoints
    /// <summary>GET /venues/{venueId} - Get venue details.</summary>
    Task<SportscoreVenueResult> GetVenueAsync(int venueId, CancellationToken cancellationToken = default);
    
    /// <summary>GET /events/{eventId}/venue - Get venue for event.</summary>
    Task<SportscoreVenueResult> GetEventVenueAsync(int eventId, CancellationToken cancellationToken = default);

    // Referees endpoints
    /// <summary>GET /referees/{refereeId} - Get referee details.</summary>
    Task<SportscoreRefereeResult> GetRefereeAsync(int refereeId, CancellationToken cancellationToken = default);
    
    /// <summary>GET /events/{eventId}/referee - Get referee for event.</summary>
    Task<SportscoreRefereeResult> GetEventRefereeAsync(int eventId, CancellationToken cancellationToken = default);

    // Markets/Odds endpoints
    /// <summary>GET /events/{eventId}/markets - Get markets for event.</summary>
    Task<SportscoreMarketsResult> GetEventMarketsAsync(int eventId, int page = 1, CancellationToken cancellationToken = default);
    
    /// <summary>GET /markets/{marketId}/outcomes - Get outcomes for market.</summary>
    Task<SportscoreOutcomesResult> GetMarketOutcomesAsync(int marketId, CancellationToken cancellationToken = default);

    // Cup tree endpoints
    /// <summary>GET /seasons/{seasonId}/cup-tree - Get cup tree for season.</summary>
    Task<SportscoreCupTreeResult> GetSeasonCupTreeAsync(int seasonId, CancellationToken cancellationToken = default);

    // Other event endpoints
    /// <summary>GET /events/{eventId}/h2h - Get head-to-head comparison.</summary>
    Task<SportscoreH2HResult> GetEventH2HAsync(int eventId, CancellationToken cancellationToken = default);
    
    /// <summary>GET /events/{eventId}/trends - Get event trends.</summary>
    Task<SportscoreTrendsResult> GetEventTrendsAsync(int eventId, CancellationToken cancellationToken = default);
}

// Result classes
public class SportscoreTeamResult
{
    public JsonElement? Team { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
}

public class SportscoreTeamsResult
{
    public List<JsonElement> Teams { get; init; } = new();
    public JsonElement? Meta { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
}

public class SportscorePlayersResult
{
    public List<JsonElement> Players { get; init; } = new();
    public JsonElement? Meta { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
}

public class SportscorePlayerResult
{
    public JsonElement? Player { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
}

public class SportscorePlayerStatisticsResult
{
    public List<JsonElement> Statistics { get; init; } = new();
    public JsonElement? Meta { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
}

public class SportscoreManagersResult
{
    public List<JsonElement> Managers { get; init; } = new();
    public JsonElement? Meta { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
}

public class SportscoreManagerResult
{
    public JsonElement? Manager { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
}

public class SportscoreVenueResult
{
    public JsonElement? Venue { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
}

public class SportscoreRefereeResult
{
    public JsonElement? Referee { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
}

public class SportscoreMarketsResult
{
    public List<JsonElement> Markets { get; init; } = new();
    public JsonElement? Meta { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
}

public class SportscoreOutcomesResult
{
    public List<JsonElement> Outcomes { get; init; } = new();
    public JsonElement? Meta { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
}

public class SportscoreCupTreeResult
{
    public JsonElement? CupTree { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
}

public class SportscoreH2HResult
{
    public JsonElement? H2H { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
}

public class SportscoreTrendsResult
{
    public JsonElement? Trends { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
}
