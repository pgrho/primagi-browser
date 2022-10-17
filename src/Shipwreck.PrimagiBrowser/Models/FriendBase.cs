namespace Shipwreck.PrimagiBrowser.Models;

public abstract class FriendBase
{
    public string? PlayerName { get; set; }
    public string? ImageUrl { get; set; }
    public string? ThumbUrl { get; set; }
    public int FriendLevel { get; set; }
}
