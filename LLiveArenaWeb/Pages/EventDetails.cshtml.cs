using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LLiveArenaWeb.Services;
using LLiveArenaWeb.Models;
using System.Text.Json;

namespace LLiveArenaWeb.Pages;

public class EventDetailsModel : PageModel
{
    private readonly ILiveEventsService _liveEventsService;
    private readonly IStreamService _streamService;
    private readonly IMatchListService _matchListService;
    private readonly ILogger<EventDetailsModel> _logger;

    public EventDetailsModel(ILiveEventsService liveEventsService, IStreamService streamService, IMatchListService matchListService, ILogger<EventDetailsModel> logger)
    {
        _liveEventsService = liveEventsService;
        _streamService = streamService;
        _matchListService = matchListService;
        _logger = logger;
    }

    public JsonElement? Event { get; private set; }
    public string? Error { get; private set; }
    public StreamResponse? StreamResponse { get; private set; }
    public string? StreamUrl { get; private set; }

    public async Task<IActionResult> OnGetAsync(int eventId)
    {
        var result = await _liveEventsService.GetEventDetailsAsync(eventId);
        
        if (!result.Success)
        {
            Error = result.Error ?? "Failed to load event details";
            return Page();
        }

        Event = result.Event;
        
        // Find gmid and fetch stream - same approach as Live page
        long? gmid = null;
        
        if (Event.HasValue)
        {
            var evt = Event.Value;
            var evtNullable = (JsonElement?)evt;
            
            // Try 1: Get gmid directly from event data
            gmid = GetLongProperty(evtNullable, "gmid", "gmid_id", "match_id", "matchId");
            
            // Try 2: If no gmid, find matching match from MatchListService by team names
            if (!gmid.HasValue)
            {
                try
                {
                    // Extract team names from event
                    var homeTeam = default(JsonElement?);
                    if (evt.TryGetProperty("home_team", out var ht1)) homeTeam = ht1;
                    else if (evt.TryGetProperty("homeTeam", out var ht2)) homeTeam = ht2;
                    else if (evt.TryGetProperty("home", out var ht3)) homeTeam = ht3;
                    
                    var awayTeam = default(JsonElement?);
                    if (evt.TryGetProperty("away_team", out var at1)) awayTeam = at1;
                    else if (evt.TryGetProperty("awayTeam", out var at2)) awayTeam = at2;
                    else if (evt.TryGetProperty("away", out var at3)) awayTeam = at3;
                    
                    var homeName = GetStringProperty(homeTeam, "name", "short_name", "title");
                    var awayName = GetStringProperty(awayTeam, "name", "short_name", "title");
                    
                    if (!string.IsNullOrEmpty(homeName) && !string.IsNullOrEmpty(awayName))
                    {
                        // Get live matches from MatchListService
                        var liveMatches = await _matchListService.GetLiveMatchesAsync();
                        
                        // Find matching match by team names
                        var matchingMatch = liveMatches.FirstOrDefault(m =>
                        {
                            var homeTeamFromMatch = m.Section?.FirstOrDefault(s => s.Sno == 1)?.Nat ?? "";
                            var awayTeamFromMatch = m.Section?.FirstOrDefault(s => s.Sno == 3)?.Nat ?? "";
                            
                            // Exact match (normal or reversed)
                            if ((homeTeamFromMatch.Equals(homeName, StringComparison.OrdinalIgnoreCase) &&
                                 awayTeamFromMatch.Equals(awayName, StringComparison.OrdinalIgnoreCase)) ||
                                (homeTeamFromMatch.Equals(awayName, StringComparison.OrdinalIgnoreCase) &&
                                 awayTeamFromMatch.Equals(homeName, StringComparison.OrdinalIgnoreCase)))
                            {
                                return true;
                            }
                            
                            // Partial match
                            if ((homeTeamFromMatch.Contains(homeName, StringComparison.OrdinalIgnoreCase) ||
                                 homeName.Contains(homeTeamFromMatch, StringComparison.OrdinalIgnoreCase)) &&
                                (awayTeamFromMatch.Contains(awayName, StringComparison.OrdinalIgnoreCase) ||
                                 awayName.Contains(awayTeamFromMatch, StringComparison.OrdinalIgnoreCase)))
                            {
                                return true;
                            }
                            
                            return false;
                        });
                        
                        if (matchingMatch != null)
                        {
                            gmid = matchingMatch.Gmid;
                            _logger.LogDebug("Found matching match with gmid: {Gmid}", gmid.Value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to find matching match from MatchListService");
                }
            }
        }
        
        // Fetch stream using gmid - same as Live page
        if (gmid.HasValue)
        {
            StreamResponse = await _streamService.GetStreamSourceAsync(gmid.Value);
            if (StreamResponse?.Success == true && !string.IsNullOrEmpty(StreamResponse.Data?.StreamUrl))
            {
                StreamUrl = StreamResponse.Data.StreamUrl;
            }
        }
        
        return Page();
    }

    // Helper methods for extracting data from JsonElement
    public string? GetStringProperty(JsonElement? element, params string[] propertyNames)
    {
        if (element == null) return null;
        var el = element.Value;
        
        foreach (var propName in propertyNames)
        {
            if (el.TryGetProperty(propName, out var prop) && prop.ValueKind == JsonValueKind.String)
                return prop.GetString();
        }
        return null;
    }

    public int? GetIntProperty(JsonElement? element, params string[] propertyNames)
    {
        if (element == null) return null;
        var el = element.Value;
        
        foreach (var propName in propertyNames)
        {
            if (el.TryGetProperty(propName, out var prop) && prop.ValueKind == JsonValueKind.Number)
                return prop.GetInt32();
        }
        return null;
    }

    public long? GetLongProperty(JsonElement? element, params string[] propertyNames)
    {
        if (element == null) return null;
        var el = element.Value;
        
        foreach (var propName in propertyNames)
        {
            if (el.TryGetProperty(propName, out var prop) && prop.ValueKind == JsonValueKind.Number)
            {
                if (prop.TryGetInt64(out var longValue))
                    return longValue;
                if (prop.TryGetInt32(out var intValue))
                    return intValue;
            }
        }
        return null;
    }

    public JsonElement? GetObjectProperty(JsonElement? element, params string[] propertyNames)
    {
        if (element == null) return null;
        var el = element.Value;
        
        foreach (var propName in propertyNames)
        {
            if (el.TryGetProperty(propName, out var prop) && prop.ValueKind == JsonValueKind.Object)
                return prop;
        }
        return null;
    }

    public List<JsonElement> GetArrayProperty(JsonElement? element, params string[] propertyNames)
    {
        var list = new List<JsonElement>();
        if (element == null) return list;
        var el = element.Value;
        
        foreach (var propName in propertyNames)
        {
            if (el.TryGetProperty(propName, out var prop) && prop.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in prop.EnumerateArray())
                    list.Add(item.Clone());
                break;
            }
        }
        return list;
    }
}
