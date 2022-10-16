using Shipwreck.PrimagiBrowser.Properties;
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
                _Tabs = new BulkUpdateableCollection<TabViewModelBase>();

                foreach (var c in Settings.Default.GetCharacterInfo())
                {
                    _Tabs.Add(new CharacterTabViewModel(this, c));
                }

                _Tabs.Add(new AddNewTabViewModel(this));
            }
            return _Tabs;
        }
    }

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