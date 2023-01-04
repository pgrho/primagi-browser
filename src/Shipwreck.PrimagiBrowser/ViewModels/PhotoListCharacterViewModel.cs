using Shipwreck.PrimagiBrowser.Models;

namespace Shipwreck.PrimagiBrowser.ViewModels;

public sealed class PhotoListCharacterViewModel : CharacterViewModel
{
    internal PhotoListCharacterViewModel(PhotoListTabViewModel tab, CharacterRecord record)
            : base(record)
    {
        Tab = tab;
    }

    public PhotoListTabViewModel Tab { get; }

    #region IsSelected

    private bool _IsSelected = true;

    public bool IsSelected
    {
        get => _IsSelected;
        set
        {
            if (SetProperty(ref _IsSelected, value))
            {
                Tab.BeginLoadPhoto();
            }
        }
    }

    #endregion IsSelected
}
