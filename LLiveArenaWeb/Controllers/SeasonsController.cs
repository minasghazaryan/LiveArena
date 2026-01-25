using Microsoft.AspNetCore.Mvc;
using LLiveArenaWeb.Services;
using System.Text.Json;

namespace LLiveArenaWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeasonsController : ControllerBase
{
    private readonly ILiveEventsService _liveEventsService;
    private readonly ISportsDataService _sportsDataService;
    private readonly ILogger<SeasonsController> _logger;

    public SeasonsController(
        ILiveEventsService liveEventsService,
        ISportsDataService sportsDataService,
        ILogger<SeasonsController> logger)
    {
        _liveEventsService = liveEventsService;
        _sportsDataService = sportsDataService;
        _logger = logger;
    }

    [HttpGet("{leagueId}")]
    public async Task<IActionResult> GetSeasons(int leagueId)
    {
        try
        {
            // Fetch seasons
            var seasonsResult = await _liveEventsService.GetLeagueSeasonsAsync(leagueId);
            if (!seasonsResult.Success)
            {
                return BadRequest(new { error = seasonsResult.Error ?? "Failed to load seasons" });
            }

            // Get league info
            var sportsData = await _sportsDataService.GetSportsDataAsync();
            var league = sportsData?.Leagues?.FirstOrDefault(l => l.Id == leagueId);
            var leagueName = league?.Name;
            var leagueLogo = league?.Logo;

            // Filter to current seasons only
            var currentYear = DateTime.Now.Year;
            var currentDate = DateTime.Now;
            var currentSeasons = seasonsResult.Seasons
                .Where(s => IsCurrentSeason(s, currentDate))
                .OrderByDescending(s =>
                {
                    var yearStart = GetIntProperty(s, "year_start") ?? 0;
                    return yearStart;
                })
                .Select(s => new
                {
                    id = GetIntProperty(s, "id") ?? 0,
                    name = GetStringProperty(s, "name") ?? "Season",
                    slug = GetStringProperty(s, "slug") ?? "",
                    yearStart = GetIntProperty(s, "year_start"),
                    yearEnd = GetIntProperty(s, "year_end")
                })
                .ToList();

            return Ok(new
            {
                leagueId,
                leagueName,
                leagueLogo,
                seasons = currentSeasons
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching seasons for league {LeagueId}", leagueId);
            return StatusCode(500, new { error = "Internal server error" });
        }
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

    private string? GetStringProperty(JsonElement element, params string[] propertyNames)
    {
        foreach (var propName in propertyNames)
        {
            if (element.TryGetProperty(propName, out var prop) && prop.ValueKind == JsonValueKind.String)
                return prop.GetString();
        }
        return null;
    }

    private int? GetIntProperty(JsonElement element, params string[] propertyNames)
    {
        foreach (var propName in propertyNames)
        {
            if (element.TryGetProperty(propName, out var prop) && prop.ValueKind == JsonValueKind.Number)
                return prop.GetInt32();
        }
        return null;
    }
}
