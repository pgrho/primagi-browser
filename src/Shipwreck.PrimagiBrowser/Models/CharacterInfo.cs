using System.Text.RegularExpressions;

namespace Shipwreck.PrimagiBrowser.Models;

public class CharacterInfo
{
    public string? CharacterName { get; set; }
    public byte BirthMonth { get; set; }
    public byte BirthDate { get; set; }
    public string? CardId { get; set; }
    public string? LoginUserKey { get; set; }

    public bool IsValid()
        => CharacterName != null
        && CardId != null
        && 1 <= BirthMonth && BirthMonth <= 12
        && 1 <= BirthDate && BirthDate <= DateTime.DaysInMonth(2020, BirthMonth)
        && Regex.IsMatch(CharacterName, "^.{1,6}$")
        && Regex.IsMatch(CardId, "^[A-Z0-9]{7}-[A-Z0-9]{7}$");
}