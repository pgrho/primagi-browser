namespace Shipwreck.PrimagiBrowser.Models;

public sealed class PromoFriendListData
{
    public int MaxPageIndex { get; set; }
    public int CurrentPageIndex { get; set; }
    public PromoFriendItem[]? PromoFriendList { get; set; }
}
