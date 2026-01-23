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
