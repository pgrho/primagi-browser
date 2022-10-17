namespace Shipwreck.PrimagiBrowser.Models;

public sealed class FriendListData
{
    public int MaxPageIndex { get; set; }
    public int CurrentPageIndex { get; set; }
    public FriendItem[]? FriendList { get; set; }
}
