﻿using System.Diagnostics;
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

            webView.CoreWebView2.AddWebResourceRequestedFilter("https://primagi.jp/mypage/login/", CoreWebView2WebResourceContext.All);
            webView.CoreWebView2.AddWebResourceRequestedFilter("https://primagi.jp/mypage/api/*", CoreWebView2WebResourceContext.All);
            webView.CoreWebView2.WebResourceResponseReceived += CoreWebView2_WebResourceResponseReceived;

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

    private async void CoreWebView2_WebResourceResponseReceived(object? sender, CoreWebView2WebResourceResponseReceivedEventArgs e)
    {
        try
        {
            if (e.Request.Method == "POST"
                && e.Request.Uri == "https://primagi.jp/mypage/login/")
            {
                ViewModel?.HandleLoginResponse(e.Response.Headers);
            }
            else if (e.Request.Uri.StartsWith("https://primagi.jp/mypage/api/"))
            {
                string? pc = null;
                //var ps = e.Request.Content;
                //if (ps != null)
                //{
                //    var pr = new StreamReader(ps, Encoding.UTF8, leaveOpen: true);
                //    pc = await pr.ReadToEndAsync();
                //}

                var rs = await e.Response.GetContentAsync();
                var rr = new StreamReader(rs, Encoding.UTF8, leaveOpen: true);
                var rc = await rr.ReadToEndAsync();

                ViewModel?.HandleApiResponse(e.Request.Uri, e.Request.Headers, pc, rc);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }
}