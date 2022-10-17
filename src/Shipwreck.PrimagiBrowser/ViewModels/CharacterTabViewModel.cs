using System.Diagnostics;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Shipwreck.PrimagiBrowser.Models;
using Shipwreck.PrimagiBrowser.Properties;

namespace Shipwreck.PrimagiBrowser.ViewModels;

public sealed class CharacterTabViewModel : TabViewModelBase
{
    public CharacterTabViewModel(MainWindowViewModel window, CharacterInfo character)
        : base(window, $"{character.CharacterName}")
    {
        CharacterName = character.CharacterName ?? throw new ArgumentException();
        BirthMonth = character.BirthMonth;
        BirthDate = character.BirthDate;
        CardId = character.CardId ?? throw new ArgumentException();
    }

    public string CharacterName { get; }
    public byte BirthMonth { get; }
    public byte BirthDate { get; }
    internal readonly string CardId;
    private string? _LoginUserKey;

    public void HandleLoginResponse(IEnumerable<KeyValuePair<string, string>> responseHeaders)
    {
        var luk = GetLoginUserKey(responseHeaders);

        if (luk != null)
        {
            _LoginUserKey = luk;
            var sd = Settings.Default;
            var cs = sd.GetCharacterInfo().ToList();
            var c = cs.FirstOrDefault(e => e.CardId == CardId);
            if (c != null && c.LoginUserKey != luk)
            {
                c.LoginUserKey = luk;
                sd.SetCharacterInfo(cs);
                sd.Save();
            }
        }
    }

    private static string? GetLoginUserKey(IEnumerable<KeyValuePair<string, string>> responseHeaders)
    {
        return responseHeaders.Select(
                    e => Regex.IsMatch(e.Key, "^(set-)?cookie$", RegexOptions.IgnoreCase)
                    && Regex.Match(e.Value, "(?:^|;|\\s)LOGIN_USER_KEY=([a-f0-9]+(?:-[a-f0-9]+)+)(?:;|$)", RegexOptions.IgnoreCase) is var m
                    && m.Success ? m.Groups[1].Value : null).FirstOrDefault(e => e != null);
    }

    public void HandleApiResponse(string uri, IEnumerable<KeyValuePair<string, string>> requestHeaders, string? requestContent, string responseContent)
    {
        var luk = GetLoginUserKey(requestHeaders);
        if (luk != _LoginUserKey)
        {
            return;
        }

        Debug.WriteLine(uri);
        if (uri == "https://primagi.jp/mypage/api/myphotolist/")
        {
            var res = JsonConvert.DeserializeObject<ApiResponse<PhotoDataListData>>(responseContent);

            if (res?.IsSuccessful() == true
                && res.Data?.PhotoDataList?.Length >= 0)
            {
                Debug.WriteLine("Got {0} photo", res.Data.PhotoDataList.Length);
            }
        }
        else if (uri == "https://primagi.jp/mypage/api/itemlist/")
        {
            var res = JsonConvert.DeserializeObject<ApiResponse<ItemListData>>(responseContent);

            if (res?.IsSuccessful() == true
                && res.Data?.ItemList?.Length >= 0)
            {
                Debug.WriteLine("Got {0} items", res.Data.ItemList.Length);
            }
        }
        else if (uri == "https://primagi.jp/mypage/api/friendlist/")
        {
            var res = JsonConvert.DeserializeObject<ApiResponse<FriendListData>>(responseContent);

            if (res?.IsSuccessful() == true
                && res.Data?.FriendList?.Length >= 0)
            {
                Debug.WriteLine("Got {0} friends", res.Data.FriendList.Length);
            }
        }
        else if (uri == "https://primagi.jp/mypage/api/promofriend/")
        {
            var res = JsonConvert.DeserializeObject<ApiResponse<PromoFriendListData>>(responseContent);

            if (res?.IsSuccessful() == true
                && res.Data?.PromoFriendList?.Length >= 0)
            {
                Debug.WriteLine("Got {0} promo friends", res.Data.PromoFriendList.Length);
            }
        }
    }
}