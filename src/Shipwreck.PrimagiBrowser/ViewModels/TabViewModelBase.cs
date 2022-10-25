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

    #region LeftText

    private string _LeftText = " ";

    public string LeftText
    {
        get => _LeftText;
        protected set => SetProperty(ref _LeftText, string.IsNullOrWhiteSpace(value) ? " " : value);
    }

    #endregion LeftText

    #region RightText

    private string _RightText = " ";

    public string RightText
    {
        get => _RightText;
        protected set => SetProperty(ref _RightText, string.IsNullOrWhiteSpace(value) ? " " : value);
    }

    #endregion RightText
}