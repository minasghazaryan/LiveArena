namespace LLiveArenaWeb.Models;

public class StreamResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public StreamData? Data { get; set; }
}

public class StreamData
{
    public string? StreamUrl { get; set; }
    public string? StreamType { get; set; }
    public string? Quality { get; set; }
    public Dictionary<string, string>? StreamSources { get; set; }
}
