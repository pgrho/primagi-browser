using System.Text.Json;
using Shipwreck.PrimagiBrowser.Models;

namespace Shipwreck.PrimagiBrowser.Properties;

internal sealed partial class Settings
{
    public IEnumerable<CharacterInfo> GetCharacterInfo()
    {
        try
        {
            var s = Characters;
            if (s != null
                && JsonSerializer.Deserialize<List<CharacterInfo>>(s) is var cs
                && cs != null)
            {
                return cs.Where(e => e?.IsValid() == true).GroupBy(e => e.CardId).Select(e => e.First());
            }
        }
        catch { }

        return Enumerable.Empty<CharacterInfo>();
    }

    public void SetCharacterInfo(IEnumerable<CharacterInfo> characters)
        => Characters = JsonSerializer.Serialize(characters?.ToList() ?? new List<CharacterInfo>());
}