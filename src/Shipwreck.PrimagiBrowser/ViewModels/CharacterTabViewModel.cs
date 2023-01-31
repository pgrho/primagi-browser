using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shipwreck.PrimagiBrowser.Models;

namespace Shipwreck.PrimagiBrowser.ViewModels;

public sealed class CharacterTabViewModel : TabViewModelBase
{
    public CharacterTabViewModel(MainWindowViewModel window, CharacterRecord character)
        : base(window, $"{character.CharacterName}")
    {
        _Id = character.Id;
        CharacterName = character.CharacterName ?? throw new ArgumentException();
        BirthMonth = character.BirthMonth;
        BirthDate = character.BirthDate;
        CardId = character.CardId ?? throw new ArgumentException();
    }

    private readonly int _Id;
    public string CharacterName { get; }
    public byte BirthMonth { get; }
    public byte BirthDate { get; }
    internal readonly string CardId;
    private string? _LoginUserKey;

    public async void HandleLoginResponse(IEnumerable<KeyValuePair<string, string>> responseHeaders)
    {
        var luk = GetLoginUserKey(responseHeaders);

        if (luk != null)
        {
            _LoginUserKey = luk;

            using (var db = await BrowserDbContext.CreateDbAsync())
            {
                var c = await db.Characters!.FirstOrDefaultAsync(e => e.Id == _Id && e.LoginUserKey != luk);
                if (c != null)
                {
                    c.LoginUserKey = luk;
                    await db.SaveChangesAsync();
                }
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

    public async void HandleApiResponse(string uri, IEnumerable<KeyValuePair<string, string>> requestHeaders, string? requestContent, string responseContent)
    {
        var luk = GetLoginUserKey(requestHeaders);
        if (luk == null)
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

                await HandlePhotoDataListAsync(requestHeaders, res);
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

    internal async Task<List<PhotoRecord>> HandlePhotoDataListAsync(IEnumerable<KeyValuePair<string, string>> requestHeaders, ApiResponse<PhotoDataListData> res)
    {
        var luk = GetLoginUserKey(requestHeaders);
        if (luk == null)
        {
            return new(0);
        }

        var seqs = res.Data!.PhotoDataList!.Select(e => e.PhotoSeq);
        using var db = await BrowserDbContext.CreateDbAsync();

        var es = await db.Photo!.Where(e => e.CharacterId == _Id && seqs.Contains(e.Seq)).Select(e => e.Seq).ToListAsync();

        var newItems = new List<PhotoRecord>();

        foreach (var p in res.Data.PhotoDataList!)
        {
            if (p.PhotoSeq != null
                && p.ImageUrl != null
                && p.ThumbUrl != null
                && !es.Contains(p.PhotoSeq))
            {
                newItems.Add(new PhotoRecord
                {
                    CharacterId = _Id,
                    Seq = p.PhotoSeq,
                    PlayDate = p.PlayDate,
                    ImageUrl = p.ImageUrl,
                    ThumbUrl = p.ThumbUrl,
                });
            }
        }

        if (newItems.Any())
        {
            db.Photo!.AddRange(newItems);
            await db.SaveChangesAsync();

            Window.PhotoList.Enqueue(newItems);
        }

        return newItems;
    }

    #region DeleteAsync

    public override bool CanDelete => true;

    public override async Task<bool> DeleteAsync()
    {
        using var db = await BrowserDbContext.CreateDbAsync();

        var ch = await db.Characters!.FindAsync(_Id);

        if (ch == null)
        {
            return false;
        }

        db.Coordinates!.RemoveRange(await db.Coordinates.Where(e => e.CharacterId == _Id).ToListAsync());
        var ps = await db.Photo!.Where(e => e.CharacterId == _Id).ToListAsync();
        db.Photo!.RemoveRange(ps);

        db.Characters.Remove(ch);

        await db.SaveChangesAsync();

        return true;
    }

    #endregion DeleteAsync
}