using Shipwreck.ViewModelUtils;

namespace Shipwreck.PrimagiBrowser.ViewModels;

public sealed class PhotoListMonthViewModel : ObservableModel
{
    internal PhotoListMonthViewModel(PhotoListTabViewModel tab, DateOnly startDate)
    {
        Tab = tab;
        StartDate = startDate;
    }

    public PhotoListTabViewModel Tab { get; }
    public DateOnly StartDate { get; }
}
