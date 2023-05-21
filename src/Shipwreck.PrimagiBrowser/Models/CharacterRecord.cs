using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace Shipwreck.PrimagiBrowser.Models;

public class CharacterRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(6)]
    public string CharacterName { get; set; } = string.Empty;

    [StringLength(12)]
    public string? DisplayName { get; set; }

    public byte BirthMonth { get; set; }

    public byte BirthDate { get; set; }

    [Required]
    [StringLength(15)]
    public string CardId { get; set; } = string.Empty;

    public string? LoginUserKey { get; set; }

    public bool IsValid()
        => CharacterName != null
        && CardId != null
        && 1 <= BirthMonth && BirthMonth <= 12
        && !(DisplayName?.Length > 12)
        && 1 <= BirthDate && BirthDate <= DateTime.DaysInMonth(2020, BirthMonth)
        && Regex.IsMatch(CharacterName, "^.{1,6}$")
        && Regex.IsMatch(CardId, "^[A-Z0-9]{7}-[A-Z0-9]{7}$");
}