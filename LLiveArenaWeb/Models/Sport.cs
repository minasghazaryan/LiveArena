namespace LLiveArenaWeb.Models;

public class Sport
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ChildNode { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty; // Used for schedule/match data compatibility
}
