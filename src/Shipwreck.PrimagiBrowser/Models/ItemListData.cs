namespace Shipwreck.PrimagiBrowser.Models;

public sealed class ItemListData
{
    public int ItemCount { get; set; }
    public int PosessionCount { get; set; }
    public CoordinateItem[]? ItemList { get; set; }
}
