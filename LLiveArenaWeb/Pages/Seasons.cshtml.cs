using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LLiveArenaWeb.Services;
using System.Text.Json;

namespace LLiveArenaWeb.Pages;

public class SeasonsModel : PageModel
{
    private readonly ILiveEventsService _liveEventsService;
    private readonly ISportscoreDataService _sportscoreDataService;
    private readonly ISportsDataService _sportsDataService;
    private readonly ILogger<SeasonsModel> _logger;

    public SeasonsModel(ILiveEventsService liveEventsService, ISportscoreDataService sportscoreDataService, ISportsDataService sportsDataService, ILogger<SeasonsModel> logger)
    {
        _liveEventsService = liveEventsService;
        _sportscoreDataService = sportscoreDataService;
        _sportsDataService = sportsDataService;
        _logger = logger;
    }

    public int LeagueId { get; private set; }
    public string? LeagueName { get; private set; }
    public string? LeagueLogo { get; private set; }
    public List<JsonElement> Seasons { get; private set; } = new();
    public JsonElement? CupTree { get; private set; }
    public int? SelectedSeasonId { get; private set; }
    public string? Error { get; private set; }

    public async Task<IActionResult> OnGetAsync(int leagueId, int? seasonId = null)
    {
        try
        {
            LeagueId = leagueId;
            SelectedSeasonId = seasonId;

            // Get league info from sports-data.json
            var sportsData = await _sportsDataService.GetSportsDataAsync();
            var league = sportsData?.Leagues?.FirstOrDefault(l => l.Id == leagueId);
            if (league != null)
            {
                LeagueName = league.Name;
                LeagueLogo = league.Logo;
            }

            // Fetch seasons
            var seasonsResult = await _liveEventsService.GetLeagueSeasonsAsync(leagueId);
            if (seasonsResult.Success)
            {
                // Filter to current seasons only
                var currentYear = DateTime.Now.Year;
                var currentDate = DateTime.Now;
                Seasons = seasonsResult.Seasons
                    .Where(s => IsCurrentSeason(s, currentDate))
                    .OrderByDescending(s => {
                        var yearStart = GetIntProperty(s, "year_start") ?? 0;
                        return yearStart;
                    })
                    .ToList();
                
                // If a season is selected, fetch cup tree (optional - 404 is expected for some seasons)
                // Don't block page loading if this fails
                if (seasonId.HasValue)
                {
                    try
                    {
                        // Use a timeout to prevent hanging
                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                        var cupTreeResult = await _sportscoreDataService.GetSeasonCupTreeAsync(seasonId.Value, cts.Token);
                        if (cupTreeResult.Success && cupTreeResult.CupTree.HasValue)
                        {
                            CupTree = cupTreeResult.CupTree;
                            _logger.LogDebug("Cup tree loaded for season {SeasonId}", seasonId.Value);
                        }
                        else
                        {
                            _logger.LogDebug("Cup tree not available for season {SeasonId}: {Error}", seasonId.Value, cupTreeResult.Error ?? "Not found");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogWarning("Cup tree request timed out for season {SeasonId}", seasonId.Value);
                        // Continue without cup tree - it's optional
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error fetching cup tree for season {SeasonId}", seasonId.Value);
                        // Continue without cup tree - it's optional
                    }
                }
            }
            else
            {
                Error = seasonsResult.Error ?? "Failed to load seasons";
                _logger.LogWarning("Failed to load seasons for league {LeagueId}: {Error}", leagueId, Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading seasons page for league {LeagueId}", leagueId);
            Error = "An error occurred while loading the page. Please try again.";
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

    private bool IsCurrentSeason(JsonElement season, DateTime currentDate)
    {
        var yearStart = GetIntProperty(season, "year_start");
        var yearEnd = GetIntProperty(season, "year_end");

        if (!yearStart.HasValue || !yearEnd.HasValue)
        {
            // If no year info, assume it's current if it exists
            return true;
        }

        // Season is current if current date falls within the season year range
        // For example, 2024-2025 season is current if we're in 2024 or 2025
        var currentYear = currentDate.Year;
        return currentYear >= yearStart.Value && currentYear <= yearEnd.Value;
    }
}
