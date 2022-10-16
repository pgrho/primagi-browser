using Shipwreck.PrimagiBrowser.ViewModels;

namespace Shipwreck.PrimagiBrowser.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}