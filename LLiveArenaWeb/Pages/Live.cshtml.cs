using Microsoft.AspNetCore.Mvc.RazorPages;
using LLiveArenaWeb.Services;
using LLiveArenaWeb.Models;

namespace LLiveArenaWeb.Pages;

public class LiveModel : PageModel
{
    private readonly IMatchListService _matchListService;
    private readonly IStreamService _streamService;
    private readonly ISportsDataService _sportsDataService;
    private readonly ILogger<LiveModel> _logger;

    public LiveModel(IMatchListService matchListService, IStreamService streamService, ISportsDataService sportsDataService, ILogger<LiveModel> logger)
    {
        _matchListService = matchListService;
        _streamService = streamService;
        _sportsDataService = sportsDataService;
        _logger = logger;
    }

    public List<MatchListItem> LiveMatches { get; set; } = new();
    public Dictionary<string, List<MatchListItem>> LiveMatchesByLeague { get; set; } = new();
    public MatchListItem? SelectedMatch { get; set; }
    public StreamResponse? StreamResponse { get; set; }
    public Dictionary<string, int> LeaguePriorityByName { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public async Task OnGetAsync(long? gmid = null)
    {
        var sportsData = await _sportsDataService.GetSportsDataAsync();
        LeaguePriorityByName = sportsData.Leagues?
            .Where(l => !string.IsNullOrWhiteSpace(l.Name))
            .ToDictionary(l => l.Name, l => l.Priority ?? int.MaxValue, StringComparer.OrdinalIgnoreCase)
            ?? new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        // Use GetLiveMatchesAsync which gets unfiltered data - we want to show ALL live matches
        // The filtering by sports-data.json is for Sportscore API, not MatchListService API
        LiveMatches = await _matchListService.GetLiveMatchesAsync();
        LiveMatches = LiveMatches
            .OrderBy(m => m.Stime)
            .ToList();
        
        _logger.LogInformation("Live page: Found {Count} live matches (unfiltered)", LiveMatches.Count);
        
        // Group by competition
        LiveMatchesByLeague = LiveMatches
            .GroupBy(m => m.Cname ?? "Unknown Competition")
            .OrderBy(g => GetLeaguePriority(g.Key))
            .ThenBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.OrderBy(m => m.Stime).ToList());
        
        // If a specific match is selected, get its stream
        if (gmid.HasValue)
        {
            SelectedMatch = await _matchListService.GetMatchByGmidAsync(gmid.Value);
            if (SelectedMatch != null)
            {
                _logger.LogInformation("Loading stream for selected match gmid: {Gmid}", gmid.Value);
                StreamResponse = await _streamService.GetStreamSourceAsync(gmid.Value);
                if (StreamResponse?.Success == true && !string.IsNullOrEmpty(StreamResponse.Data?.StreamUrl))
                {
                    _logger.LogInformation("Stream loaded successfully for gmid: {Gmid}", gmid.Value);
                }
                else
                {
                    _logger.LogWarning("Stream not available for gmid: {Gmid}. Success: {Success}, Message: {Message}", 
                        gmid.Value, StreamResponse?.Success, StreamResponse?.Message);
                }
            }
            else
            {
                _logger.LogWarning("Match not found for gmid: {Gmid}", gmid.Value);
            }
        }
        else if (LiveMatches.Any())
        {
            // Default to first live match
            SelectedMatch = LiveMatches.First();
            if (SelectedMatch != null)
            {
                _logger.LogInformation("Auto-selecting first live match gmid: {Gmid}", SelectedMatch.Gmid);
                StreamResponse = await _streamService.GetStreamSourceAsync(SelectedMatch.Gmid);
                if (StreamResponse?.Success == true && !string.IsNullOrEmpty(StreamResponse.Data?.StreamUrl))
                {
                    _logger.LogInformation("Stream loaded successfully for auto-selected match gmid: {Gmid}", SelectedMatch.Gmid);
                }
                else
                {
                    _logger.LogWarning("Stream not available for auto-selected match gmid: {Gmid}. Success: {Success}, Message: {Message}", 
                        SelectedMatch.Gmid, StreamResponse?.Success, StreamResponse?.Message);
                }
            }
        }
        else
        {
            _logger.LogWarning("No live matches available to display streams");
        }
    }

    private int GetLeaguePriority(string? leagueName)
    {
        if (string.IsNullOrWhiteSpace(leagueName))
            return int.MaxValue;

        return LeaguePriorityByName.TryGetValue(leagueName, out var priority)
            ? priority
            : int.MaxValue;
    }
}
