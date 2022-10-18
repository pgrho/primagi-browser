using Shipwreck.PrimagiBrowser.Models;
using Shipwreck.ViewModelUtils;

namespace Shipwreck.PrimagiBrowser.ViewModels;

public sealed class AddNewTabViewModel : TabViewModelBase
{
    public AddNewTabViewModel(MainWindowViewModel window)
        : base(window, "追加")
    {
    }

    #region CharacterName

    private string _CharacterName = string.Empty;

    public string CharacterName
    {
        get => _CharacterName;
        set => SetProperty(ref _CharacterName, value?.Trim() ?? string.Empty);
    }

    #endregion CharacterName

    #region BirthMonth

    private byte _BirthMonth = 1;

    public byte BirthMonth
    {
        get => _BirthMonth;
        set => SetProperty(ref _BirthMonth, value);
    }

    #endregion BirthMonth

    #region BirthDate

    private byte _BirthDate = 1;

    public byte BirthDate
    {
        get => _BirthDate;
        set => SetProperty(ref _BirthDate, value);
    }

    #endregion BirthDate

    #region CardId

    private string _CardId = string.Empty;

    public string CardId
    {
        get => _CardId;
        set => SetProperty(ref _CardId, value?.Trim().ToUpperInvariant() ?? string.Empty);
    }

    #endregion CardId

    #region AddCommand

    private CommandViewModelBase? _AddCommand;

    public CommandViewModelBase AddCommand
        => _AddCommand ??= CommandViewModel.Create(Add);

    public async void Add()
    {
        var c = new CharacterRecord
        {
            CharacterName = CharacterName,
            BirthMonth = BirthMonth,
            BirthDate = BirthDate,
            CardId = CardId
        };

        if (!c.IsValid())
        {
            await Window.ShowErrorToastAsync("Invalid Parameter");
            return;
        }

        if (Window.Tabs.OfType<CharacterTabViewModel>().Any(e => e.CardId == c.CardId))
        {
            await Window.ShowErrorToastAsync("Duplicate CardId");
            return;
        }

        using (var db = await BrowserDbContext.CreateDbAsync())
        {
            db.Characters!.Add(c);
            await db.SaveChangesAsync();
        }

        var t = new CharacterTabViewModel(Window, c);
        Window.Tabs.Insert(Window.Tabs.Count - 1, t);
        Window.SelectedTab = t;
    }

    #endregion AddCommand
}