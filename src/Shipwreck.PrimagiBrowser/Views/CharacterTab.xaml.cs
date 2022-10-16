using System.IO;
using Microsoft.Web.WebView2.Core;
using Shipwreck.PrimagiBrowser.ViewModels;

namespace Shipwreck.PrimagiBrowser.Views;

/// <summary>
/// CharacterTab.xaml の相互作用ロジック
/// </summary>
public partial class CharacterTab
{
    public CharacterTab()
    {
        InitializeComponent();
    }

    private CharacterTabViewModel? ViewModel => DataContext as CharacterTabViewModel;

    private async void webView_Loaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel is CharacterTabViewModel vm
            && webView.Source == null)
        {
            var udc = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), typeof(App)!.Namespace, "UserData", vm.CardId);

            if (!Directory.Exists(udc))
            {
                Directory.CreateDirectory(udc);
            }

            var env = await CoreWebView2Environment.CreateAsync(null, udc);
            await webView.EnsureCoreWebView2Async(env);
            webView.Source = new Uri("https://primagi.jp/mypage/login/");
        }
    }

    private bool _IsExecuted;

    private void webView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
    {
        if (ViewModel is CharacterTabViewModel vm
            && webView.Source?.ToString() == "https://primagi.jp/mypage/login/"
            && !_IsExecuted)
        {
            _IsExecuted = true;
            webView.ExecuteScriptAsync(
                @$"$('input[name=""val[ProfileCardID]""').val('{vm.CardId}');
$('input[name=""val[PlayerName]""').val('{vm.CharacterName}');
$('input[name=""val[Birthday]""').val('{vm.BirthMonth}/{vm.BirthDate}');
$('form').submit();");
        }
    }
}