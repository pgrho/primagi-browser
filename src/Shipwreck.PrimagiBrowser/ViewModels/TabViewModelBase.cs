using Shipwreck.ViewModelUtils;

namespace Shipwreck.PrimagiBrowser.ViewModels;

public abstract class TabViewModelBase : ObservableModel
{
    protected TabViewModelBase(MainWindowViewModel window, string title)
    {
        Window = window;
        _Title = title;
    }

    public MainWindowViewModel Window { get; }

    #region Title

    private string _Title = string.Empty;

    public string Title
    {
        get => _Title;
        set => SetProperty(ref _Title, value ?? string.Empty);
    }

    #endregion Title
}
