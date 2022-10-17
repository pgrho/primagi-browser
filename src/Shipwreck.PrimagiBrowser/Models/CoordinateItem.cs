namespace Shipwreck.PrimagiBrowser.Models;

public sealed class CoordinateItem
{
    public int ID { get; set; }

    public byte Possession { get; set; }

    public string? ImageUrl { get; set; }

    public byte Level { get; set; }

    public string? ItemName { get; set; }

    public string? GenreID { get; set; }

    public string? BrandID { get; set; }

    public string? ColorID { get; set; }

    public string? RarityID { get; set; }

    public string? RarityName { get; set; }

    public int Point { get; set; }

    public string? CategoryID { get; set; }

    public string? CategoryName { get; set; }

    public string? SubCategoryID { get; set; }

    public string? SealID { get; set; }

    public string? CollectionID { get; set; }

    public string? LimitedIconID { get; set; }
}