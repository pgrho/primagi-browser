using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shipwreck.PrimagiBrowser.Models;

public class PhotoRecord
{
    public int CharacterId { get; set; }

    [Required, StringLength(8)]
    public string Seq { get; set; } = string.Empty;

    public DateTime PlayDate { get; set; }

    [Required, StringLength(255)]
    public string ImageUrl { get; set; } = string.Empty;

    [Required, StringLength(255)]
    public string ThumbUrl { get; set; } = string.Empty;

    [StringLength(255)]
    public string? ImagePath { get; set; }

    [StringLength(255)]
    public string? ThumbPath { get; set; }

    [ForeignKey(nameof(CharacterId))]
    public virtual CharacterRecord? Character { get; set; }
}