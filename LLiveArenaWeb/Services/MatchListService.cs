using LLiveArenaWeb.Models;

namespace LLiveArenaWeb.Services;

public class MatchListService : IMatchListService
{
    private readonly MatchListResponse _matchListData;

    public MatchListService()
    {
        // Initialize with sample match list data
        _matchListData = new MatchListResponse
        {
            Success = true,
            Msg = "Success",
            Status = 200,
            Data = new MatchListData
            {
                T1 = new List<MatchListItem>
                {
                    new MatchListItem
                    {
                        Gmid = 509853657,
                        Ename = "Newcastle v PSV",
                        Etid = 1,
                        Cid = 7846996,
                        Cname = "EUROPE CHAMPIONS LEAGUE",
                        Iplay = true,
                        Stime = "1/22/2026 12:30:00 AM",
                        Tv = true,
                        Bm = true,
                        Status = "OPEN",
                        Gscode = 1,
                        Gtype = "match",
                        Section = new List<MatchSection>
                        {
                            new MatchSection
                            {
                                Sid = 781680,
                                Sno = 1,
                                Gstatus = "ACTIVE",
                                Gscode = 1,
                                Nat = "Newcastle",
                                Odds = new List<Odds>
                                {
                                    new Odds { Sid = 781680, OddsValue = 1.35, Otype = "back", Oname = "back1", Size = 17608.52 },
                                    new Odds { Sid = 781680, OddsValue = 1.36, Otype = "lay", Oname = "lay1", Size = 8031.87 }
                                }
                            },
                            new MatchSection
                            {
                                Sid = 50622,
                                Sno = 2,
                                Gstatus = "ACTIVE",
                                Gscode = 1,
                                Nat = "The Draw",
                                Odds = new List<Odds>
                                {
                                    new Odds { Sid = 50622, OddsValue = 6.4, Otype = "back", Oname = "back1", Size = 1628.89 },
                                    new Odds { Sid = 50622, OddsValue = 6.6, Otype = "lay", Oname = "lay1", Size = 615.93 }
                                }
                            },
                            new MatchSection
                            {
                                Sid = 55081,
                                Sno = 3,
                                Gstatus = "ACTIVE",
                                Gscode = 1,
                                Nat = "PSV",
                                Odds = new List<Odds>
                                {
                                    new Odds { Sid = 55081, OddsValue = 9.4, Otype = "back", Oname = "back1", Size = 249 },
                                    new Odds { Sid = 55081, OddsValue = 9.6, Otype = "lay", Oname = "lay1", Size = 196.07 }
                                }
                            }
                        }
                    },
                    new MatchListItem
                    {
                        Gmid = 500137929,
                        Ename = "Chelsea U21 v Real Sociedad U21",
                        Etid = 1,
                        Cid = 4697205,
                        Cname = "EUROPE Premier League International Cup",
                        Iplay = true,
                        Stime = "1/22/2026 2:20:00 AM",
                        Tv = true,
                        Bm = false,
                        Status = "OPEN",
                        Gscode = 1,
                        Gtype = "match",
                        Section = new List<MatchSection>
                        {
                            new MatchSection
                            {
                                Sid = 63420,
                                Sno = 1,
                                Gstatus = "ACTIVE",
                                Gscode = 1,
                                Nat = "Chelsea U21",
                                Odds = new List<Odds>
                                {
                                    new Odds { Sid = 63420, OddsValue = 4.4, Otype = "back", Oname = "back1", Size = 26.21 },
                                    new Odds { Sid = 63420, OddsValue = 4.7, Otype = "lay", Oname = "lay1", Size = 12.54 }
                                }
                            },
                            new MatchSection
                            {
                                Sid = 898622,
                                Sno = 2,
                                Gstatus = "ACTIVE",
                                Gscode = 1,
                                Nat = "The Draw",
                                Odds = new List<Odds>
                                {
                                    new Odds { Sid = 898622, OddsValue = 3.05, Otype = "back", Oname = "back1", Size = 144.6 },
                                    new Odds { Sid = 898622, OddsValue = 3.2, Otype = "lay", Oname = "lay1", Size = 110.16 }
                                }
                            },
                            new MatchSection
                            {
                                Sid = 71061,
                                Sno = 3,
                                Gstatus = "ACTIVE",
                                Gscode = 1,
                                Nat = "Real Sociedad U21",
                                Odds = new List<Odds>
                                {
                                    new Odds { Sid = 71061, OddsValue = 2.18, Otype = "back", Oname = "back1", Size = 11.95 },
                                    new Odds { Sid = 71061, OddsValue = 2.22, Otype = "lay", Oname = "lay1", Size = 16.28 }
                                }
                            }
                        }
                    },
                    new MatchListItem
                    {
                        Gmid = 508252079,
                        Ename = "Man City (W) v Chelsea (W)",
                        Etid = 1,
                        Cid = 8897085,
                        Cname = "ENENGLAND WOMENS LEAGUE CUP - SEMI-FINALS",
                        Iplay = true,
                        Stime = "1/22/2026 2:00:00 AM",
                        Tv = true,
                        Bm = false,
                        Status = "OPEN",
                        Gscode = 1,
                        Gtype = "match",
                        Section = new List<MatchSection>
                        {
                            new MatchSection
                            {
                                Sid = 812490,
                                Sno = 1,
                                Gstatus = "ACTIVE",
                                Gscode = 1,
                                Nat = "Man City (W)",
                                Odds = new List<Odds>
                                {
                                    new Odds { Sid = 812490, OddsValue = 9.8, Otype = "back", Oname = "back1", Size = 12 },
                                    new Odds { Sid = 812490, OddsValue = 11, Otype = "lay", Oname = "lay1", Size = 49.04 }
                                }
                            },
                            new MatchSection
                            {
                                Sid = 520552,
                                Sno = 2,
                                Gstatus = "ACTIVE",
                                Gscode = 1,
                                Nat = "The Draw",
                                Odds = new List<Odds>
                                {
                                    new Odds { Sid = 520552, OddsValue = 4.6, Otype = "back", Oname = "back1", Size = 27.2 },
                                    new Odds { Sid = 520552, OddsValue = 4.8, Otype = "lay", Oname = "lay1", Size = 199.27 }
                                }
                            },
                            new MatchSection
                            {
                                Sid = 47881,
                                Sno = 3,
                                Gstatus = "ACTIVE",
                                Gscode = 1,
                                Nat = "Chelsea (W)",
                                Odds = new List<Odds>
                                {
                                    new Odds { Sid = 47881, OddsValue = 1.43, Otype = "back", Oname = "back1", Size = 14.63 },
                                    new Odds { Sid = 47881, OddsValue = 1.46, Otype = "lay", Oname = "lay1", Size = 296.9 }
                                }
                            }
                        }
                    },
                    new MatchListItem
                    {
                        Gmid = 824237451,
                        Ename = "Southampton v Sheff Utd",
                        Etid = 1,
                        Cid = 7161054,
                        Cname = "ENGLAND Championship",
                        Iplay = true,
                        Stime = "1/22/2026 1:15:00 AM",
                        Tv = true,
                        Bm = false,
                        Status = "OPEN",
                        Gscode = 1,
                        Gtype = "match",
                        Section = new List<MatchSection>
                        {
                            new MatchSection
                            {
                                Sid = 539610,
                                Sno = 1,
                                Gstatus = "ACTIVE",
                                Gscode = 1,
                                Nat = "Southampton",
                                Odds = new List<Odds>
                                {
                                    new Odds { Sid = 539610, OddsValue = 1.53, Otype = "back", Oname = "back1", Size = 37.79 },
                                    new Odds { Sid = 539610, OddsValue = 1.55, Otype = "lay", Oname = "lay1", Size = 60.17 }
                                }
                            },
                            new MatchSection
                            {
                                Sid = 602212,
                                Sno = 2,
                                Gstatus = "ACTIVE",
                                Gscode = 1,
                                Nat = "The Draw",
                                Odds = new List<Odds>
                                {
                                    new Odds { Sid = 602212, OddsValue = 4.8, Otype = "back", Oname = "back1", Size = 169.71 },
                                    new Odds { Sid = 602212, OddsValue = 4.9, Otype = "lay", Oname = "lay1", Size = 11.8 }
                                }
                            },
                            new MatchSection
                            {
                                Sid = 854991,
                                Sno = 3,
                                Gstatus = "ACTIVE",
                                Gscode = 1,
                                Nat = "Sheff Utd",
                                Odds = new List<Odds>
                                {
                                    new Odds { Sid = 854991, OddsValue = 6.8, Otype = "back", Oname = "back1", Size = 32.4 },
                                    new Odds { Sid = 854991, OddsValue = 7, Otype = "lay", Oname = "lay1", Size = 12 }
                                }
                            }
                        }
                    },
                    new MatchListItem
                    {
                        Gmid = 621791490,
                        Ename = "Slavia Prague v Barcelona",
                        Etid = 1,
                        Cid = 7846996,
                        Cname = "EUROPE CHAMPIONS LEAGUE",
                        Iplay = true,
                        Stime = "1/22/2026 12:35:00 AM",
                        Tv = true,
                        Bm = true,
                        Status = "OPEN",
                        Gscode = 1,
                        Gtype = "match",
                        Section = new List<MatchSection>
                        {
                            new MatchSection
                            {
                                Sid = 509540,
                                Sno = 1,
                                Gstatus = "ACTIVE",
                                Gscode = 1,
                                Nat = "Slavia Prague",
                                Odds = new List<Odds>
                                {
                                    new Odds { Sid = 509540, OddsValue = 8.8, Otype = "back", Oname = "back1", Size = 645.4 },
                                    new Odds { Sid = 509540, OddsValue = 9, Otype = "lay", Oname = "lay1", Size = 510.91 }
                                }
                            },
                            new MatchSection
                            {
                                Sid = 92232,
                                Sno = 2,
                                Gstatus = "ACTIVE",
                                Gscode = 1,
                                Nat = "The Draw",
                                Odds = new List<Odds>
                                {
                                    new Odds { Sid = 92232, OddsValue = 6.2, Otype = "back", Oname = "back1", Size = 622.94 },
                                    new Odds { Sid = 92232, OddsValue = 6.4, Otype = "lay", Oname = "lay1", Size = 2815.05 }
                                }
                            },
                            new MatchSection
                            {
                                Sid = 69541,
                                Sno = 3,
                                Gstatus = "ACTIVE",
                                Gscode = 1,
                                Nat = "Barcelona",
                                Odds = new List<Odds>
                                {
                                    new Odds { Sid = 69541, OddsValue = 1.37, Otype = "back", Oname = "back1", Size = 14312.7 },
                                    new Odds { Sid = 69541, OddsValue = 1.38, Otype = "lay", Oname = "lay1", Size = 9779.68 }
                                }
                            }
                        }
                    },
                    new MatchListItem
                    {
                        Gmid = 615918672,
                        Ename = "Bologna v Celtic",
                        Etid = 1,
                        Cid = 7846996,
                        Cname = "EUROPE CHAMPIONS LEAGUE",
                        Iplay = true,
                        Stime = "1/22/2026 11:15:00 PM",
                        Tv = true,
                        Bm = true,
                        Status = "OPEN",
                        Gscode = 1,
                        Gtype = "match",
                        Section = new List<MatchSection>
                        {
                            new MatchSection
                            {
                                Sid = 65790,
                                Sno = 1,
                                Gstatus = "ACTIVE",
                                Gscode = 1,
                                Nat = "Bologna",
                                Odds = new List<Odds>
                                {
                                    new Odds { Sid = 65790, OddsValue = 1.71, Otype = "back", Oname = "back1", Size = 690.78 },
                                    new Odds { Sid = 65790, OddsValue = 1.72, Otype = "lay", Oname = "lay1", Size = 15.12 }
                                }
                            },
                            new MatchSection
                            {
                                Sid = 77082,
                                Sno = 2,
                                Gstatus = "ACTIVE",
                                Gscode = 1,
                                Nat = "The Draw",
                                Odds = new List<Odds>
                                {
                                    new Odds { Sid = 77082, OddsValue = 4.2, Otype = "back", Oname = "back1", Size = 259.69 },
                                    new Odds { Sid = 77082, OddsValue = 4.3, Otype = "lay", Oname = "lay1", Size = 240.98 }
                                }
                            },
                            new MatchSection
                            {
                                Sid = 82931,
                                Sno = 3,
                                Gstatus = "ACTIVE",
                                Gscode = 1,
                                Nat = "Celtic",
                                Odds = new List<Odds>
                                {
                                    new Odds { Sid = 82931, OddsValue = 5.4, Otype = "back", Oname = "back1", Size = 157.51 },
                                    new Odds { Sid = 82931, OddsValue = 5.5, Otype = "lay", Oname = "lay1", Size = 44.18 }
                                }
                            }
                        }
                    },
                    new MatchListItem
                    {
                        Gmid = 532710544,
                        Ename = "Leiria v Vizela",
                        Etid = 1,
                        Cid = 6538041,
                        Cname = "PORTUGAL Liga Portugal 2",
                        Iplay = true,
                        Stime = "1/22/2026 1:40:00 AM",
                        Tv = true,
                        Bm = true,
                        Status = "OPEN",
                        Gscode = 1,
                        Gtype = "match",
                        Section = new List<MatchSection>
                        {
                            new MatchSection
                            {
                                Sid = 59660,
                                Sno = 1,
                                Gstatus = "ACTIVE",
                                Gscode = 1,
                                Nat = "Leiria",
                                Odds = new List<Odds>
                                {
                                    new Odds { Sid = 59660, OddsValue = 15, Otype = "back", Oname = "back1", Size = 1 },
                                    new Odds { Sid = 59660, OddsValue = 16, Otype = "lay", Oname = "lay1", Size = 8.54 }
                                }
                            },
                            new MatchSection
                            {
                                Sid = 89002,
                                Sno = 2,
                                Gstatus = "ACTIVE",
                                Gscode = 1,
                                Nat = "The Draw",
                                Odds = new List<Odds>
                                {
                                    new Odds { Sid = 89002, OddsValue = 1.19, Otype = "back", Oname = "back1", Size = 114.81 },
                                    new Odds { Sid = 89002, OddsValue = 1.2, Otype = "lay", Oname = "lay1", Size = 1612.75 }
                                }
                            },
                            new MatchSection
                            {
                                Sid = 70271,
                                Sno = 3,
                                Gstatus = "ACTIVE",
                                Gscode = 1,
                                Nat = "Vizela",
                                Odds = new List<Odds>
                                {
                                    new Odds { Sid = 70271, OddsValue = 10.5, Otype = "back", Oname = "back1", Size = 13.04 },
                                    new Odds { Sid = 70271, OddsValue = 11, Otype = "lay", Oname = "lay1", Size = 1.36 }
                                }
                            }
                        }
                    },
                    new MatchListItem
                    {
                        Gmid = 773453229,
                        Ename = "Smouha v Pharco FC",
                        Etid = 1,
                        Cid = 6952584,
                        Cname = "EGYPT Premier League",
                        Iplay = true,
                        Stime = "1/22/2026 1:40:00 AM",
                        Tv = true,
                        Bm = false,
                        Status = "OPEN",
                        Gscode = 1,
                        Gtype = "match",
                        Section = new List<MatchSection>
                        {
                            new MatchSection
                            {
                                Sid = 869800,
                                Sno = 1,
                                Gstatus = "ACTIVE",
                                Gscode = 1,
                                Nat = "Smouha",
                                Odds = new List<Odds>
                                {
                                    new Odds { Sid = 869800, OddsValue = 0, Otype = "back", Oname = "back1", Size = 0 },
                                    new Odds { Sid = 869800, OddsValue = 1.01, Otype = "lay", Oname = "lay1", Size = 625.32 }
                                }
                            },
                            new MatchSection
                            {
                                Sid = 659872,
                                Sno = 2,
                                Gstatus = "ACTIVE",
                                Gscode = 1,
                                Nat = "The Draw",
                                Odds = new List<Odds>
                                {
                                    new Odds { Sid = 659872, OddsValue = 100, Otype = "back", Oname = "back1", Size = 6.32 },
                                    new Odds { Sid = 659872, OddsValue = 950, Otype = "lay", Oname = "lay1", Size = 0.04 }
                                }
                            },
                            new MatchSection
                            {
                                Sid = 606511,
                                Sno = 3,
                                Gstatus = "ACTIVE",
                                Gscode = 1,
                                Nat = "Pharco FC",
                                Odds = new List<Odds>
                                {
                                    new Odds { Sid = 606511, OddsValue = 1000, Otype = "back", Oname = "back1", Size = 12.22 },
                                    new Odds { Sid = 606511, OddsValue = 0, Otype = "lay", Oname = "lay1", Size = 0 }
                                }
                            }
                        }
                    },
                    new MatchListItem
                    {
                        Gmid = 700301453,
                        Ename = "Barcelona (W) v Athletic Bilbao (W)",
                        Etid = 1,
                        Cid = 9183899,
                        Cname = "SPAIN SUPER CUP WOMEN - SEMI-FINALS",
                        Iplay = true,
                        Stime = "1/22/2026 1:55:00 AM",
                        Tv = true,
                        Bm = false,
                        Status = "OPEN",
                        Gscode = 1,
                        Gtype = "match",
                        Section = new List<MatchSection>
                        {
                            new MatchSection
                            {
                                Sid = 498170,
                                Sno = 1,
                                Gstatus = "ACTIVE",
                                Gscode = 1,
                                Nat = "Barcelona (W)",
                                Odds = new List<Odds>
                                {
                                    new Odds { Sid = 498170, OddsValue = 0, Otype = "back", Oname = "back1", Size = 0 },
                                    new Odds { Sid = 498170, OddsValue = 1.01, Otype = "lay", Oname = "lay1", Size = 3327.35 }
                                }
                            },
                            new MatchSection
                            {
                                Sid = 52712,
                                Sno = 2,
                                Gstatus = "ACTIVE",
                                Gscode = 1,
                                Nat = "The Draw",
                                Odds = new List<Odds>
                                {
                                    new Odds { Sid = 52712, OddsValue = 400, Otype = "back", Oname = "back1", Size = 2.8 },
                                    new Odds { Sid = 52712, OddsValue = 950, Otype = "lay", Oname = "lay1", Size = 0.01 }
                                }
                            },
                            new MatchSection
                            {
                                Sid = 561511,
                                Sno = 3,
                                Gstatus = "ACTIVE",
                                Gscode = 1,
                                Nat = "Athletic Bilbao (W)",
                                Odds = new List<Odds>
                                {
                                    new Odds { Sid = 561511, OddsValue = 1000, Otype = "back", Oname = "back1", Size = 5.15 },
                                    new Odds { Sid = 561511, OddsValue = 0, Otype = "lay", Oname = "lay1", Size = 0 }
                                }
                            }
                        }
                    }
                }
            },
            LastUpdatedAt = DateTime.Parse("2026-01-22T01:23:36.059Z")
        };
    }

    public Task<MatchListResponse?> GetMatchListAsync()
    {
        // This will be replaced with actual API call
        return Task.FromResult<MatchListResponse?>(_matchListData);
    }

    public Task<List<MatchListItem>> GetMatchesByCompetitionAsync(long competitionId)
    {
        var matches = _matchListData.Data.T1?
            .Where(m => m.Cid == competitionId)
            .ToList() ?? new List<MatchListItem>();

        return Task.FromResult(matches);
    }

    public Task<List<MatchListItem>> GetLiveMatchesAsync()
    {
        var matches = _matchListData.Data.T1?
            .Where(m => m.Iplay == true)
            .ToList() ?? new List<MatchListItem>();

        return Task.FromResult(matches);
    }

    public Task<MatchListItem?> GetMatchByGmidAsync(long gmid)
    {
        var match = _matchListData.Data.T1?
            .FirstOrDefault(m => m.Gmid == gmid);

        return Task.FromResult(match);
    }
}
