using Microsoft.EntityFrameworkCore;
using Shipwreck.PrimagiBrowser.Models;
using Shipwreck.ViewModelUtils;

namespace Shipwreck.PrimagiBrowser.ViewModels;

public sealed class MainWindowViewModel : WindowViewModel
{
    #region Tabs

    private BulkUpdateableCollection<TabViewModelBase>? _Tabs;

    public BulkUpdateableCollection<TabViewModelBase> Tabs
    {
        get
        {
            if (_Tabs == null)
            {
                _Tabs = new() { AddNewTab, PhotoList };

                BeginLoadCharacters();
            }
            return _Tabs;
        }
    }

    private async void BeginLoadCharacters()
    {
        using var db = await BrowserDbContext.CreateDbAsync();
        var i = 0;
        foreach (var c in await db.Characters!.OrderBy(e => e.Id).ToListAsync())
        {
            _Tabs?.Insert(i++, new CharacterTabViewModel(this, c));
        }
    }

    private AddNewTabViewModel? _AddNewTab;

    public AddNewTabViewModel AddNewTab
        => _AddNewTab ??= new AddNewTabViewModel(this);

    private PhotoListTabViewModel? _PhotoList;

    public PhotoListTabViewModel PhotoList
        => _PhotoList ??= new PhotoListTabViewModel(this);

    #endregion Tabs

    #region SelectedTab

    private TabViewModelBase? _SelectedTab;

    public TabViewModelBase SelectedTab
    {
        get => _SelectedTab ??= Tabs[0];
        set => SetProperty(ref _SelectedTab, value);
    }

    #endregion SelectedTab
}