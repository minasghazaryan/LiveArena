using System.Text.Json.Serialization;

namespace LLiveArenaWeb.Models;

/// <summary>Root store for all sports data (sports, leagues, teams, managers, players) persisted to JSON.</summary>
public class SportsDataStore
{
    [JsonPropertyName("lastUpdatedUtc")]
    public DateTime LastUpdatedUtc { get; set; }

    [JsonPropertyName("sports")]
    public List<SportInfo> Sports { get; set; } = new();

    [JsonPropertyName("sections")]
    public List<SectionInfo> Sections { get; set; } = new();

    [JsonPropertyName("leagues")]
    public List<LeagueInfo> Leagues { get; set; } = new();

    [JsonPropertyName("teams")]
    public List<TeamInfo> Teams { get; set; } = new();

    [JsonPropertyName("managers")]
    public List<ManagerInfo> Managers { get; set; } = new();

    [JsonPropertyName("players")]
    public List<PlayerInfo> Players { get; set; } = new();
}

/// <summary>Sport stored in sports-data.json (id, sport_id, slug, name only).</summary>
public class SportInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("sport_id")]
    public int SportId { get; set; }

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>API response for the sport-list endpoint.</summary>
public class SportsListApiResponse
{
    [JsonPropertyName("data")]
    public List<SportApiItem> Data { get; set; } = new();

    [JsonPropertyName("meta")]
    public object? Meta { get; set; }
}

/// <summary>Section stored in sports-data.json (id, sport_id, slug, name, flag). Linked to sport via sport_id.</summary>
public class SectionInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("sport_id")]
    public int SportId { get; set; }

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("flag")]
    public string Flag { get; set; } = string.Empty;
}

/// <summary>API response for the sections-list endpoint.</summary>
public class SectionsListApiResponse
{
    [JsonPropertyName("data")]
    public List<SectionApiItem> Data { get; set; } = new();

    [JsonPropertyName("meta")]
    public object? Meta { get; set; }
}

/// <summary>Section item as returned by the API (snake_case).</summary>
public class SectionApiItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("sport_id")]
    public int SportId { get; set; }

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("flag")]
    public string Flag { get; set; } = string.Empty;
}

/// <summary>Sport item as returned by the API (snake_case).</summary>
public class SportApiItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("name_translations")]
    public Dictionary<string, string> NameTranslations { get; set; } = new();
}

public class LeagueInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("sport_id")]
    public int SportId { get; set; }

    [JsonPropertyName("section_id")]
    public int SectionId { get; set; }

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("logo")]
    public string? Logo { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("flag")]
    public string? Flag { get; set; }
}

public class TeamInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("logoUrl")]
    public string? LogoUrl { get; set; }

    [JsonPropertyName("leagueId")]
    public int LeagueId { get; set; }
}

public class ManagerInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("photoUrl")]
    public string? PhotoUrl { get; set; }

    [JsonPropertyName("teamId")]
    public int? TeamId { get; set; }

    [JsonPropertyName("leagueId")]
    public int? LeagueId { get; set; }

    [JsonPropertyName("nationality")]
    public string? Nationality { get; set; }
}

public class PlayerInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("photoUrl")]
    public string? PhotoUrl { get; set; }

    [JsonPropertyName("teamId")]
    public int? TeamId { get; set; }

    [JsonPropertyName("leagueId")]
    public int? LeagueId { get; set; }

    [JsonPropertyName("position")]
    public string? Position { get; set; }

    [JsonPropertyName("nationality")]
    public string? Nationality { get; set; }

    [JsonPropertyName("number")]
    public int? Number { get; set; }
}
