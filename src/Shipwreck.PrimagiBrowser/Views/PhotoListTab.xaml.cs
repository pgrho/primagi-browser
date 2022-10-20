using MahApps.Metro.Controls;
using Shipwreck.PrimagiBrowser.ViewModels;

namespace Shipwreck.PrimagiBrowser.Views;

/// <summary>
/// PhotoListTab.xaml の相互作用ロジック
/// </summary>
public partial class PhotoListTab
{
    public PhotoListTab()
    {
        InitializeComponent();
    }

    private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is DependencyObject d
            && d.TryFindParent<ListViewItem>() is ListViewItem item
            && item.DataContext is PhotoViewModel pvm)
        {
            pvm.Open();
            e.Handled = true;
        }
    }
}