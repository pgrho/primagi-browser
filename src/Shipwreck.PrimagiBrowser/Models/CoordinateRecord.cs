using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shipwreck.PrimagiBrowser.Models;

public class CoordinateRecord
{
    public int CharacterId { get; set; }

    [Required, StringLength(8)]
    public string SealId { get; set; } = string.Empty;

    public byte Level { get; set; }

    [ForeignKey(nameof(CharacterId))]
    public virtual CharacterRecord? Character { get; set; }
}