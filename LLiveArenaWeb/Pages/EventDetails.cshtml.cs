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
    public List<JsonElement> MediaItems { get; private set; } = new();
    public string? HighlightEmbedUrl { get; private set; }
    public string? HighlightUrl { get; private set; }
    public string? HighlightTitle { get; private set; }
    public string? HighlightThumbnailUrl { get; private set; }
    public bool HighlightIsYouTube { get; private set; }
    public List<JsonElement> Statistics { get; private set; } = new();
    public List<JsonElement> Lineups { get; private set; } = new();
    public List<JsonElement> Incidents { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(int eventId)
    {
        var result = await _liveEventsService.GetEventDetailsAsync(eventId);
        
        if (!result.Success)
        {
            Error = result.Error ?? "Failed to load event details";
            return Page();
        }

        Event = result.Event;
        var isFinishedEvent = false;
        
        // Find gmid and fetch stream - same approach as Live page
        long? gmid = null;
        
        if (Event.HasValue)
        {
            var evt = Event.Value;
            var evtNullable = (JsonElement?)evt;

            var status = GetStringProperty(evtNullable, "status", "state", "stage") ?? string.Empty;
            var statusText = status.ToUpperInvariant();
            isFinishedEvent = statusText.Contains("FINISHED") ||
                              statusText.Contains("COMPLETED") ||
                              statusText.Contains("FT") ||
                              statusText.Contains("FULL") ||
                              statusText.Contains("ENDED");
            
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

        if (isFinishedEvent)
        {
            var mediaResult = await _liveEventsService.GetEventMediasAsync(eventId, page: 1);
            if (mediaResult.Success)
            {
                MediaItems = mediaResult.Medias;
                var highlight = SelectHighlightMedia(MediaItems);
                if (highlight.HasValue)
                {
                    HighlightUrl = GetStringProperty(highlight, "url", "source_url");
                    HighlightEmbedUrl = BuildEmbedUrl(HighlightUrl);
                    HighlightTitle = GetMediaTitle(highlight);
                    HighlightThumbnailUrl = GetStringProperty(highlight, "thumbnail_url", "thumbnail");
                    HighlightIsYouTube = IsYouTubeUrl(HighlightUrl) || IsYouTubeUrl(HighlightEmbedUrl);
                }
            }
        }
        
        // Fetch statistics
        var statisticsResult = await _liveEventsService.GetEventStatisticsAsync(eventId);
        if (statisticsResult.Success)
        {
            // Filter to only show "all" period statistics
            Statistics = statisticsResult.Statistics
                .Where(s => s.TryGetProperty("period", out var period) && 
                           period.ValueKind == JsonValueKind.String && 
                           period.GetString()?.Equals("all", StringComparison.OrdinalIgnoreCase) == true)
                .ToList();
        }
        
        // Fetch lineups
        var lineupsResult = await _liveEventsService.GetEventLineupsAsync(eventId);
        if (lineupsResult.Success)
        {
            // If there are 4 items, take only the first 2 (one for each team)
            if (lineupsResult.Lineups.Count == 4)
            {
                Lineups = lineupsResult.Lineups.Take(2).ToList();
            }
            else
            {
                Lineups = lineupsResult.Lineups;
            }
        }
        
        // Fetch incidents
        var incidentsResult = await _liveEventsService.GetEventIncidentsAsync(eventId);
        if (incidentsResult.Success)
        {
            Incidents = incidentsResult.Incidents.OrderBy(i => {
                var order = GetIntProperty(i, "order") ?? 999;
                return order;
            }).ToList();
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

    private JsonElement? SelectHighlightMedia(List<JsonElement> medias)
    {
        if (medias.Count == 0)
            return null;

        JsonElement? fallback = null;
        foreach (var media in medias)
        {
            var type = GetIntProperty(media, "type") ?? 0;
            var subTitle = GetStringProperty(media, "sub_title") ?? string.Empty;
            var title = GetMediaTitle(media) ?? string.Empty;
            var isHighlight = type == 6 ||
                              subTitle.Contains("highlight", StringComparison.OrdinalIgnoreCase) ||
                              title.Contains("highlight", StringComparison.OrdinalIgnoreCase);

            if (isHighlight)
                return media;

            if (!fallback.HasValue)
                fallback = media;
        }

        return fallback;
    }

    private string? GetMediaTitle(JsonElement? media)
    {
        if (media == null)
            return null;

        var el = media.Value;
        if (el.TryGetProperty("title", out var titleEl))
        {
            if (titleEl.ValueKind == JsonValueKind.String)
                return titleEl.GetString();
            if (titleEl.ValueKind == JsonValueKind.Object &&
                titleEl.TryGetProperty("en", out var enEl) &&
                enEl.ValueKind == JsonValueKind.String)
                return enEl.GetString();
        }

        return GetStringProperty(media, "sub_title");
    }

    private static string? BuildEmbedUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            if (uri.Host.Contains("youtu.be", StringComparison.OrdinalIgnoreCase))
            {
                var videoId = uri.AbsolutePath.Trim('/').Split('/').FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(videoId))
                    return $"https://www.youtube.com/embed/{videoId}";
            }

            if (uri.Host.Contains("youtube.com", StringComparison.OrdinalIgnoreCase))
            {
                var query = uri.Query.TrimStart('?')
                    .Split('&', StringSplitOptions.RemoveEmptyEntries);
                foreach (var pair in query)
                {
                    var parts = pair.Split('=', 2);
                    if (parts.Length == 2 && parts[0].Equals("v", StringComparison.OrdinalIgnoreCase))
                    {
                        var videoId = Uri.UnescapeDataString(parts[1]);
                        if (!string.IsNullOrWhiteSpace(videoId))
                            return $"https://www.youtube.com/embed/{videoId}";
                    }
                }
            }
        }

        return url;
    }

    private static bool IsYouTubeUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        return uri.Host.Contains("youtube.com", StringComparison.OrdinalIgnoreCase) ||
               uri.Host.Contains("youtu.be", StringComparison.OrdinalIgnoreCase);
    }
}
