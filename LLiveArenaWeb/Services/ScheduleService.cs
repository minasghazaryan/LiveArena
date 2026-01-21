using LLiveArenaWeb.Models;
using System.Text.Json;

namespace LLiveArenaWeb.Services;

public class ScheduleService : IScheduleService
{
    private readonly ScheduleData _scheduleData;

    public ScheduleService()
    {
        // Initialize with sample data from the provided schedule endpoint
        _scheduleData = new ScheduleData
        {
            FetchScheduleData = new FetchScheduleData
            {
                Edges = new List<ScheduleEdge>
                {
                    new ScheduleEdge
                    {
                        Date = "2026-01-20",
                        Page = new PageInfo
                        {
                            Prev = "2026-01-19",
                            Cur = "2026-01-20",
                            Next = "2026-01-21"
                        },
                        Tours = new List<TourWithMatches>
                        {
                            new TourWithMatches
                            {
                                Tour = new Tour
                                {
                                    Id = 5427,
                                    CollectionId = 18801700,
                                    Name = "LALIGA 2025-26",
                                    DeeplinkUrl = "https://www.fancode.com/football/tour/spanish-la-liga-season-2025-2026-18801700/matches",
                                    TourUrl = "https://www.fancode.com/football/tour/laliga-2025-26-18801700/matches",
                                    CollectionSlug = "spanish-la-liga-season-2025-2026"
                                },
                                Matches = new List<Match>
                                {
                                    new Match
                                    {
                                        Id = 131028,
                                        MatchDesc = "Match 192",
                                        TeamType = "DUAL_TEAM",
                                        Name = "Elche CF vs Sevilla FC",
                                        RthAvailable = true,
                                        StreamingStatus = "COMPLETED",
                                        StartTime = DateTime.Parse("2026-01-19T20:00:00.000Z"),
                                        Status = "COMPLETED",
                                        Venue = "Martínez Valero, Elche",
                                        MatchSlug = "elche-cf-vs-sevilla-fc",
                                        MatchTour = new Tour
                                        {
                                            Id = 5427,
                                            Name = "LALIGA 2025-26",
                                            CollectionId = 18801700,
                                            CollectionSlug = "spanish-la-liga-season-2025-2026"
                                        },
                                        MatchCategory = new MatchCategory
                                        {
                                            Title = "",
                                            Slug = "NONE_FOOTBALL"
                                        },
                                        IsScorecardAvailable = true,
                                        Format = "TEST",
                                        Sport = new Sport
                                        {
                                            Id = "1",
                                            Name = "Soccer",
                                            Slug = "football"
                                        },
                                        Squads = new List<Squad>
                                        {
                                            new Squad
                                            {
                                                Name = "Elche CF",
                                                ShortName = "ELC",
                                                SquadId = 2740,
                                                Flag = new Flag
                                                {
                                                    Src = "https://d13ir53smqqeyp.cloudfront.net/flags/ft-flags/ELC-FT1@2x.png"
                                                },
                                                Color = "#253611",
                                                FootballScore = new FootballScore
                                                {
                                                    Points = 2,
                                                    Goals = new List<Goal>
                                                    {
                                                        new Goal
                                                        {
                                                            Id = "69883",
                                                            ShortName = "G Valera",
                                                            IsOwnGoal = false,
                                                            GoalTime = "55'"
                                                        },
                                                        new Goal
                                                        {
                                                            Id = "30204",
                                                            ShortName = "A Febas",
                                                            IsOwnGoal = false,
                                                            GoalTime = "14'"
                                                        }
                                                    }
                                                },
                                                Status = new SquadStatus
                                                {
                                                    Cricket = new CricketStatus
                                                    {
                                                        IsBatting = false
                                                    }
                                                }
                                            },
                                            new Squad
                                            {
                                                Name = "Sevilla FC",
                                                ShortName = "SEV",
                                                SquadId = 534,
                                                Flag = new Flag
                                                {
                                                    Src = "https://d13ir53smqqeyp.cloudfront.net/flags/ft-flags/FC-SVI@2x.png"
                                                },
                                                Color = "#daa335",
                                                FootballScore = new FootballScore
                                                {
                                                    Points = 2,
                                                    Goals = new List<Goal>
                                                    {
                                                        new Goal
                                                        {
                                                            Id = "159480",
                                                            ShortName = "A Adams",
                                                            IsOwnGoal = false,
                                                            GoalTime = "90'"
                                                        },
                                                        new Goal
                                                        {
                                                            Id = "159480",
                                                            ShortName = "A Adams",
                                                            IsOwnGoal = false,
                                                            GoalTime = "75'"
                                                        }
                                                    }
                                                },
                                                Status = new SquadStatus
                                                {
                                                    Cricket = new CricketStatus
                                                    {
                                                        IsBatting = false
                                                    }
                                                }
                                            }
                                        },
                                        Scorecard = new Scorecard
                                        {
                                            FootballScore = new FootballScorecard
                                            {
                                                MatchClock = "Full Time",
                                                Description = null,
                                                NewDescription = "",
                                                MatchState = "ENDED"
                                            }
                                        }
                                    }
                                }
                            },
                            new TourWithMatches
                            {
                                Tour = new Tour
                                {
                                    Id = 5572,
                                    CollectionId = 18884118,
                                    Name = "LALIGA Hypermotion 2025-26",
                                    DeeplinkUrl = "https://www.fancode.com/football/tour/laliga-hypermotion-2025-26-18884118/matches",
                                    TourUrl = "https://www.fancode.com/football/tour/laliga-hypermotion-2025-26-18884118/matches",
                                    CollectionSlug = "laliga-hypermotion-2025-26"
                                },
                                Matches = new List<Match>
                                {
                                    new Match
                                    {
                                        Id = 139790,
                                        MatchDesc = "",
                                        TeamType = "DUAL_TEAM",
                                        Name = "Granada vs Eibar",
                                        RthAvailable = false,
                                        StreamingStatus = "COMPLETED",
                                        StartTime = DateTime.Parse("2026-01-19T19:30:00.000Z"),
                                        Status = "COMPLETED",
                                        Venue = "Nuevo Estadio de Los Cármenes",
                                        MatchSlug = "granada-vs-eibar",
                                        MatchTour = new Tour
                                        {
                                            Id = 5572,
                                            Name = "LALIGA Hypermotion 2025-26",
                                            CollectionId = 18884118,
                                            CollectionSlug = "laliga-hypermotion-2025-26"
                                        },
                                        MatchCategory = new MatchCategory
                                        {
                                            Title = "",
                                            Slug = "NONE_FOOTBALL"
                                        },
                                        IsScorecardAvailable = false,
                                        Format = "TEST",
                                        Sport = new Sport
                                        {
                                            Id = "1",
                                            Name = "Soccer",
                                            Slug = "football"
                                        },
                                        Squads = new List<Squad>
                                        {
                                            new Squad
                                            {
                                                Name = "Granada",
                                                ShortName = "GRD",
                                                SquadId = 1285,
                                                Flag = new Flag
                                                {
                                                    Src = "https://d13ir53smqqeyp.cloudfront.net/flags/ft-flags/FC-GRD@2x.png"
                                                },
                                                Color = "#FFDFA7"
                                            },
                                            new Squad
                                            {
                                                Name = "Eibar",
                                                ShortName = "EIB",
                                                SquadId = 868,
                                                Flag = new Flag
                                                {
                                                    Src = "https://d13ir53smqqeyp.cloudfront.net/flags/ft-flags/FC-EIB@2x.png"
                                                },
                                                Color = "#F5D577"
                                            }
                                        },
                                        Scorecard = new Scorecard()
                                    }
                                }
                            },
                            new TourWithMatches
                            {
                                Tour = new Tour
                                {
                                    Id = 5822,
                                    CollectionId = 19284266,
                                    Name = "AFC U23 Asian Cup, 2026",
                                    DeeplinkUrl = "https://www.fancode.com/football/tour/afc-u23-asian-cup-2026-19284266/matches",
                                    TourUrl = "https://www.fancode.com/football/tour/afc-u23-asian-cup-2026-19284266/matches",
                                    CollectionSlug = "afc-u23-asian-cup-2026"
                                },
                                Matches = new List<Match>
                                {
                                    new Match
                                    {
                                        Id = 139279,
                                        MatchDesc = "Match 29",
                                        TeamType = "DUAL_TEAM",
                                        Name = "Japan U23 vs Korea Republic U23",
                                        RthAvailable = false,
                                        StreamingStatus = "COMPLETED",
                                        StartTime = DateTime.Parse("2026-01-20T11:30:00.000Z"),
                                        Status = "COMPLETED",
                                        Venue = "King Abdullah Sports City Hall Stadium, Jeddah",
                                        MatchSlug = "japan-u23-vs-korea-republic-u23",
                                        MatchTour = new Tour
                                        {
                                            Id = 5822,
                                            Name = "AFC U23 Asian Cup, 2026",
                                            CollectionId = 19284266,
                                            CollectionSlug = "afc-u23-asian-cup-2026"
                                        },
                                        MatchCategory = new MatchCategory
                                        {
                                            Title = "",
                                            Slug = "NONE_FOOTBALL"
                                        },
                                        IsScorecardAvailable = false,
                                        Format = "TEST",
                                        Sport = new Sport
                                        {
                                            Id = "1",
                                            Name = "Soccer",
                                            Slug = "football"
                                        },
                                        Squads = new List<Squad>
                                        {
                                            new Squad
                                            {
                                                Name = "Japan U23",
                                                ShortName = "JP-U23",
                                                SquadId = 3714,
                                                Flag = new Flag
                                                {
                                                    Src = "https://d13ir53smqqeyp.cloudfront.net/flags/ft-flags/JPN-FT1@2x.png"
                                                },
                                                Color = "#d04c6c"
                                            },
                                            new Squad
                                            {
                                                Name = "Korea Republic U23",
                                                ShortName = "KR-U23",
                                                SquadId = 3715,
                                                Flag = new Flag
                                                {
                                                    Src = "https://d13ir53smqqeyp.cloudfront.net/flags/ft-flags/KOR-FT1@2x.png"
                                                },
                                                Color = "#dd717a"
                                            }
                                        },
                                        Scorecard = new Scorecard()
                                    },
                                    new Match
                                    {
                                        Id = 139280,
                                        MatchDesc = "Match 30",
                                        TeamType = "DUAL_TEAM",
                                        Name = "Vietnam U23 vs China PR U23",
                                        RthAvailable = false,
                                        StreamingStatus = "COMPLETED",
                                        StartTime = DateTime.Parse("2026-01-20T15:30:00.000Z"),
                                        Status = "COMPLETED",
                                        Venue = "Prince Abdullah Al Faisal Sports City Stadium, Jed",
                                        MatchSlug = "vietnam-u23-vs-china-pr-u23",
                                        MatchTour = new Tour
                                        {
                                            Id = 5822,
                                            Name = "AFC U23 Asian Cup, 2026",
                                            CollectionId = 19284266,
                                            CollectionSlug = "afc-u23-asian-cup-2026"
                                        },
                                        MatchCategory = new MatchCategory
                                        {
                                            Title = "",
                                            Slug = "NONE_FOOTBALL"
                                        },
                                        IsScorecardAvailable = false,
                                        Format = "TEST",
                                        Sport = new Sport
                                        {
                                            Id = "1",
                                            Name = "Soccer",
                                            Slug = "football"
                                        },
                                        Squads = new List<Squad>
                                        {
                                            new Squad
                                            {
                                                Name = "Vietnam U23",
                                                ShortName = "VIE-U23",
                                                SquadId = 8507,
                                                Flag = new Flag
                                                {
                                                    Src = "https://d13ir53smqqeyp.cloudfront.net/flags/ft-flags/FC-VIET@2x.png"
                                                },
                                                Color = "#a10d00"
                                            },
                                            new Squad
                                            {
                                                Name = "China PR U23",
                                                ShortName = "CHN-U23",
                                                SquadId = 8502,
                                                Flag = new Flag
                                                {
                                                    Src = "https://d13ir53smqqeyp.cloudfront.net/flags/ft-flags/CHN-FT1@2x.png"
                                                },
                                                Color = "#E96E5D"
                                            }
                                        },
                                        Scorecard = new Scorecard()
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    public Task<ScheduleData?> GetScheduleAsync(string? date = null)
    {
        // This will be replaced with actual API call
        return Task.FromResult<ScheduleData?>(_scheduleData);
    }

    public Task<List<Match>> GetMatchesByDateAsync(string date)
    {
        var matches = _scheduleData.FetchScheduleData?.Edges
            .Where(e => e.Date == date)
            .SelectMany(e => e.Tours)
            .SelectMany(t => t.Matches)
            .ToList() ?? new List<Match>();

        return Task.FromResult(matches);
    }

    public Task<List<Match>> GetMatchesBySportAsync(string sportSlug)
    {
        var matches = _scheduleData.FetchScheduleData?.Edges
            .SelectMany(e => e.Tours)
            .SelectMany(t => t.Matches)
            .Where(m => m.Sport.Slug.Equals(sportSlug, StringComparison.OrdinalIgnoreCase))
            .ToList() ?? new List<Match>();

        return Task.FromResult(matches);
    }

    public Task<Match?> GetMatchByIdAsync(int matchId)
    {
        var match = _scheduleData.FetchScheduleData?.Edges
            .SelectMany(e => e.Tours)
            .SelectMany(t => t.Matches)
            .FirstOrDefault(m => m.Id == matchId);

        return Task.FromResult(match);
    }

    public Task<List<Match>> GetLiveMatchesAsync()
    {
        var matches = _scheduleData.FetchScheduleData?.Edges
            .SelectMany(e => e.Tours)
            .SelectMany(t => t.Matches)
            .Where(m => m.Status == "LIVE" || m.StreamingStatus == "LIVE")
            .ToList() ?? new List<Match>();

        return Task.FromResult(matches);
    }
}
