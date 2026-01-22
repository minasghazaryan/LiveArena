using System.Text.Json.Serialization;

namespace LLiveArenaWeb.Models;

public class MatchListResponse
{
    public bool Success { get; set; }
    public string Msg { get; set; } = string.Empty;
    public int Status { get; set; }
    public MatchListData Data { get; set; } = new();
    public DateTime LastUpdatedAt { get; set; }
}

public class MatchListData
{
    public List<MatchListItem>? T1 { get; set; }
    public List<MatchListItem>? T2 { get; set; }
}

public class MatchListCategories
{
    public List<MatchListItem> Live { get; } = new();
    public List<MatchListItem> Prematch { get; } = new();
    public List<MatchListItem> Finished { get; } = new();
}

public class MatchListItem
{
    public long Gmid { get; set; }
    public string Ename { get; set; } = string.Empty;
    public int Etid { get; set; }
    public long Cid { get; set; }
    public string Cname { get; set; } = string.Empty;
    public bool Iplay { get; set; }
    public string Stime { get; set; } = string.Empty;
    public bool Tv { get; set; }
    public bool Bm { get; set; }
    public bool F { get; set; }
    public bool F1 { get; set; }
    public int Iscc { get; set; }
    public long Mid { get; set; }
    public string Mname { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Rc { get; set; }
    public int Gscode { get; set; }
    public int M { get; set; }
    public int Oid { get; set; }
    public string Gtype { get; set; } = string.Empty;
    public List<MatchSection> Section { get; set; } = new();
}

public class MatchSection
{
    public long Sid { get; set; }
    public int Sno { get; set; }
    public string Gstatus { get; set; } = string.Empty;
    public int Gscode { get; set; }
    public string Nat { get; set; } = string.Empty;
    public List<Odds> Odds { get; set; } = new();
}

public class Odds
{
    public long Sid { get; set; }
    public int Psid { get; set; }
    
    [JsonPropertyName("odds")]
    public double OddsValue { get; set; }
    
    public string Otype { get; set; } = string.Empty;
    public string Oname { get; set; } = string.Empty;
    public int Tno { get; set; }
    public double Size { get; set; }
}
