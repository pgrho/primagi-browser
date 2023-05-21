using Shipwreck.PrimagiBrowser.Models;
using Shipwreck.ViewModelUtils;

namespace Shipwreck.PrimagiBrowser.ViewModels;

public class CharacterViewModel : ObservableModel
{
    internal CharacterViewModel(CharacterRecord record)
    {
        Id = record.Id;
        Name = record.CharacterName;
        DisplayName = record.DisplayName?.Length > 0 ? record.DisplayName : Name;
    }

    public int Id { get; }
    public string Name { get; }
    public string DisplayName { get; }
}