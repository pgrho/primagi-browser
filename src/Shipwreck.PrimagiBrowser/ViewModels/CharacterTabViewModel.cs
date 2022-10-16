using Shipwreck.PrimagiBrowser.Models;

namespace Shipwreck.PrimagiBrowser.ViewModels;

public sealed class CharacterTabViewModel : TabViewModelBase
{
    public CharacterTabViewModel(MainWindowViewModel window, CharacterInfo character)
        : base(window, $"{character.CharacterName}")
    {
        CharacterName = character.CharacterName ?? throw new ArgumentException();
        BirthMonth = character.BirthMonth;
        BirthDate = character.BirthDate;
        CardId = character.CardId ?? throw new ArgumentException();
    }

    public string CharacterName { get; }
    public byte BirthMonth { get; }
    public byte BirthDate { get; }
    internal readonly string CardId;
}
