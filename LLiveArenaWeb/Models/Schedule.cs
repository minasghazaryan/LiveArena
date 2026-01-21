namespace LLiveArenaWeb.Models;

public class ScheduleData
{
    public FetchScheduleData? FetchScheduleData { get; set; }
}

public class FetchScheduleData
{
    public List<ScheduleEdge> Edges { get; set; } = new();
}

public class ScheduleEdge
{
    public string Date { get; set; } = string.Empty;
    public PageInfo Page { get; set; } = new();
    public List<TourWithMatches> Tours { get; set; } = new();
}

public class PageInfo
{
    public string? Prev { get; set; }
    public string? Cur { get; set; }
    public string? Next { get; set; }
}

public class TourWithMatches
{
    public Tour Tour { get; set; } = new();
    public List<Match> Matches { get; set; } = new();
}

public class Tour
{
    public int Id { get; set; }
    public int? CollectionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DeeplinkUrl { get; set; }
    public string? TourUrl { get; set; }
    public Sport? Sport { get; set; }
    public string? CollectionSlug { get; set; }
}

public class Match
{
    public int Id { get; set; }
    public string MatchDesc { get; set; } = string.Empty;
    public string TeamType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool RthAvailable { get; set; }
    public string StreamingStatus { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Venue { get; set; } = string.Empty;
    public string? City { get; set; }
    public string MatchSlug { get; set; } = string.Empty;
    public Tour MatchTour { get; set; } = new();
    public MatchCategory MatchCategory { get; set; } = new();
    public DateTime? DayStartTime { get; set; }
    public bool IsScorecardAvailable { get; set; }
    public string Format { get; set; } = string.Empty;
    public List<Squad> Squads { get; set; } = new();
    public Sport Sport { get; set; } = new();
    public Scorecard Scorecard { get; set; } = new();
}

public class MatchCategory
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}

public class Squad
{
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public int? SquadNo { get; set; }
    public int SquadId { get; set; }
    public Flag Flag { get; set; } = new();
    public bool? IsWinner { get; set; }
    public string Color { get; set; } = string.Empty;
    public CricketScore? CricketScore { get; set; }
    public KabaddiScore? KabaddiScore { get; set; }
    public FootballScore? FootballScore { get; set; }
    public BasketballScore? BasketBallScore { get; set; }
    public HockeyScore? HockeyScore { get; set; }
    public SquadStatus? Status { get; set; }
    public TennisScore? TennisScore { get; set; }
}

public class Flag
{
    public string Src { get; set; } = string.Empty;
}

public class SquadStatus
{
    public CricketStatus? Cricket { get; set; }
}

public class CricketStatus
{
    public bool IsBatting { get; set; }
}

public class FootballScore
{
    public int? Points { get; set; }
    public List<Goal> Goals { get; set; } = new();
}

public class Goal
{
    public string Id { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public bool IsOwnGoal { get; set; }
    public string GoalTime { get; set; } = string.Empty;
}

public class CricketScore
{
    // Add cricket-specific score properties as needed
}

public class KabaddiScore
{
    // Add kabaddi-specific score properties as needed
}

public class BasketballScore
{
    // Add basketball-specific score properties as needed
}

public class HockeyScore
{
    // Add hockey-specific score properties as needed
}

public class TennisScore
{
    // Add tennis-specific score properties as needed
}

public class Scorecard
{
    public CricketScore? CricketScore { get; set; }
    public KabaddiScore? KabaddiScore { get; set; }
    public FootballScorecard? FootballScore { get; set; }
    public BasketballScore? BasketballScore { get; set; }
    public HockeyScore? HockeyScore { get; set; }
    public TennisScore? TennisScore { get; set; }
}

public class FootballScorecard
{
    public string? MatchClock { get; set; }
    public string? Description { get; set; }
    public string? NewDescription { get; set; }
    public string? MatchState { get; set; }
}
