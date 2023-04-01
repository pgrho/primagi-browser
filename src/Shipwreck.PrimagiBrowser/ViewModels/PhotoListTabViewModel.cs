using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using Shipwreck.PrimagiBrowser.Models;
using Shipwreck.ViewModelUtils;

namespace Shipwreck.PrimagiBrowser.ViewModels;

public sealed class PhotoListTabViewModel : TabViewModelBase
{
    public PhotoListTabViewModel(MainWindowViewModel window)
        : base(window, "画像")
    {
    }

    #region Months

    private ReadOnlyCollection<PhotoListMonthViewModel>? _Months;

    public ReadOnlyCollection<PhotoListMonthViewModel> Months
        => _Months ??= Array.AsReadOnly(
            Enumerable.Range(0, int.MaxValue)
            .Select(e =>
            {
                var td = DateTime.Today;
                td = td.AddDays(1 - td.Day);
                return DateOnly.FromDateTime(td.AddMonths(-e));
            })
            .TakeWhile(e => e >= new DateOnly(2021, 10, 1))
            .Select(e => new PhotoListMonthViewModel(this, e))
            .ToArray());

    #endregion Months

    #region SelectedMonth

    private DateOnly _SelectedMonth;

    public DateOnly SelectedMonth
    {
        get
        {
            if (!Months.Any(e => e.StartDate == _SelectedMonth))
            {
                _SelectedMonth = Months[0].StartDate;
            }
            return _SelectedMonth;
        }
        set
        {
            if (SetProperty(ref _SelectedMonth, value))
            {
                BeginLoadPhoto();
            }
        }
    }

    #endregion SelectedMonth

    #region Characters

    private readonly Dictionary<int, PhotoListCharacterViewModel> _Characters = new();
    private BulkUpdateableCollection<PhotoListCharacterViewModel>? _CharacterList;

    public BulkUpdateableCollection<PhotoListCharacterViewModel> Characters
    {
        get
        {
            if (_CharacterList == null)
            {
                lock (_Characters)
                {
                    _CharacterList = new(_Characters.Values.OrderBy(e => e.Id));
                    BindingOperations.EnableCollectionSynchronization(_CharacterList, _Characters);
                }
            }
            return _CharacterList;
        }
    }

    internal PhotoListCharacterViewModel GetOrCreate(CharacterRecord character)
    {
        lock (_Characters)
        {
            if (!_Characters.TryGetValue(character.Id, out var vm))
            {
                _Characters[character.Id] = vm = new PhotoListCharacterViewModel(this, character);
                if (_CharacterList != null)
                {
                    _CharacterList.Set(_CharacterList.Append(vm).OrderBy(e => e.Id).ToList());
                }
            }
            return vm;
        }
    }

    internal PhotoListCharacterViewModel? GetById(int characterId)
    {
        lock (_Characters)
        {
            _Characters.TryGetValue(characterId, out var vm);

            return vm;
        }
    }

    #endregion Characters

    #region PhotoList

    private BulkUpdateableCollection<PhotoViewModel>? _PhotoList;

    public BulkUpdateableCollection<PhotoViewModel> PhotoList
    {
        get
        {
            if (_PhotoList == null)
            {
                _PhotoList = new();
                BindingOperations.EnableCollectionSynchronization(_PhotoList, _PhotoList);
                BeginLoadPhoto();
            }
            return _PhotoList;
        }
    }

    internal async void BeginLoadPhoto()
    {
        var min = SelectedMonth.ToDateTime(default);
        var max = min.AddMonths(1);

        var cids = Characters.Select(e => e.Id).ToList();

        using var db = await BrowserDbContext.CreateDbAsync();

        var cs = await db.Photo!.Where(e => min <= e.PlayDate && e.PlayDate < max && !cids.Contains(e.CharacterId)).Select(e => e.Character).Distinct().ToListAsync();

        foreach(var c in cs)
        {
            GetOrCreate(c);
        }

        cids = Characters.Any() ? Characters.Where(e => e.IsSelected).Select(e => e.Id).ToList() : null;

        var ps = await db.Photo!.Include(e => e.Character)
                        .Where(e => min <= e.PlayDate && e.PlayDate < max && (cids == null || cids.Contains(e.CharacterId)))
                        .ToListAsync();

        lock (PhotoList)
        {
            PhotoList.Set(Order(ps.Select(e => new PhotoViewModel(this, e))));
            InvalidateLeftText();
        }

        Enqueue(ps);
    }

    private void InvalidateLeftText()
    {
        LeftText = $"画像: {PhotoList.Count}件";
    }

    private void InvalidateRightText()
    {
        RightText = _DownloadQueue.Any() ? $"ダウンロード中: {_DownloadQueue.Count}件" : " ";
    }

    private static IOrderedEnumerable<PhotoViewModel> Order(IEnumerable<PhotoViewModel> source)
        => source.OrderByDescending(e => e.PlayDate).ThenByDescending(e => e.Seq);

    #endregion PhotoList

    #region CopyCommand

    private CommandViewModelBase? _CopyCommand;

    public CommandViewModelBase CopyCommand
        => _CopyCommand ??= CommandViewModel.Create(() =>
        {
            List<string> ps;
            lock (PhotoList)
            {
#pragma warning disable CS8619 // 値における参照型の Null 許容性が、対象の型と一致しません。
                ps = PhotoList.Select(e => e.IsSelected && e.Image is BitmapImage bmp ? bmp.UriSource?.LocalPath : null).Where(e => e != null).ToList();
#pragma warning restore CS8619 // 値における参照型の Null 許容性が、対象の型と一致しません。
            }

            if (ps.Any())
            {
                var sc = new StringCollection();
                foreach (var p in ps)
                {
                    sc.Add(p);
                }
                Clipboard.SetFileDropList(sc);
            }
        });

    #endregion CopyCommand

    private readonly Queue<PhotoRecord> _DownloadQueue = new();
    private Task? _DownloadTask;
    private readonly HttpClient _Http = new();

    internal void Enqueue(IList<PhotoRecord> records)
    {
        lock (_DownloadQueue)
        {
            _DownloadQueue.EnsureCapacity(_DownloadQueue.Count + records.Count);
            foreach (var p in records)
            {
                _DownloadQueue.Enqueue(p);
            }

            lock (PhotoList)
            {
                var m = SelectedMonth;
                var ns = records.GroupBy(e => new { e.Character, e.Seq })
                                .Select(e => e.First())
                                .Where(p => p.PlayDate.Year == m.Year
                                        && p.PlayDate.Month == m.Month
                                        && GetById(p.CharacterId)?.IsSelected == true
                                        && !PhotoList.Any(e => e.CharacterId == p.CharacterId && e.Seq == p.Seq))
                                .ToList();

                if (ns.Any())
                {
                    PhotoList.Set(Order(PhotoList.Concat(ns.Select(p => new PhotoViewModel(this, p)))));
                    InvalidateLeftText();
                }
            }

            InvalidateRightText();

            var t = _DownloadTask;

            switch (t?.Status)
            {
                case TaskStatus.WaitingForActivation:
                case TaskStatus.WaitingToRun:
                case TaskStatus.Running:
                    return;
            }

            _DownloadTask = DownloadAsync();
        }
    }

    private async Task DownloadAsync()
    {
        var bp = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), typeof(App).Namespace!, "photo");

        for (; ; )
        {
            PhotoRecord? p;
            lock (_DownloadQueue)
            {
                InvalidateRightText();
                if (!_DownloadQueue.Any())
                {
                    return;
                }
                p = _DownloadQueue.Dequeue();
            }

            try
            {
                var pd = p.PlayDate;
                var ip = p.ImagePath;
                var tp = p.ThumbPath;

                var cvm = GetById(p.CharacterId);

                if (cvm == null)
                {
                    using var db = await BrowserDbContext.CreateDbAsync();

                    var c = await db.Characters!.FindAsync(p.CharacterId);
                    if (c != null)
                    {
                        cvm = GetOrCreate(c);
                    }
                }

                if (ip == null
                    || !File.Exists(ip))
                {
                    using var res = await _Http.GetAsync(p.ImageUrl);

                    res.EnsureSuccessStatusCode();

                    var ldt = res.Content.Headers.LastModified?.LocalDateTime;
                    if (ldt?.Date == pd)
                    {
                        pd = ldt ?? pd;
                    }
                    var fi = new FileInfo(System.IO.Path.Combine(bp, string.Format("{1:yyyy-MM-dd}/{1:yyyyMMddHHmmss}-{0}-{2}.jpg", p.Seq, pd, cvm?.Name ?? p.CharacterId.ToString())));

                    if (!fi.Directory!.Exists)
                    {
                        fi.Directory.Create();
                    }

                    using var s = await res.Content.ReadAsStreamAsync();
                    using var fs = fi.Open(FileMode.Create);

                    await s.CopyToAsync(fs);

                    ip = fi.FullName;
                }
                if (tp == null
                    || !File.Exists(tp))
                {
                    using var res = await _Http.GetAsync(p.ThumbUrl);

                    res.EnsureSuccessStatusCode();

                    var fi = new FileInfo(System.IO.Path.Combine(bp, string.Format("{1:yyyy-MM-dd}/{1:yyyyMMddHHmmss}-{0}-{2}_thumb.jpg", p.Seq, pd, cvm?.Name ?? p.CharacterId.ToString())));

                    if (!fi.Directory!.Exists)
                    {
                        fi.Directory.Create();
                    }

                    using var s = await res.Content.ReadAsStreamAsync();
                    using var fs = fi.Open(FileMode.Create);

                    await s.CopyToAsync(fs);

                    tp = fi.FullName;
                }

                if (pd != p.PlayDate || ip != p.ImagePath || tp != p.ThumbPath)
                {
                    using var db = await BrowserDbContext.CreateDbAsync();

                    var ep = await db.Photo!.FirstOrDefaultAsync(e => e.CharacterId == p.CharacterId && e.Seq == p.Seq);

                    if (ep != null)
                    {
                        ep.PlayDate = pd;
                        ep.ImagePath = ip;
                        ep.ThumbPath = tp;

                        await db.SaveChangesAsync();

                        lock (PhotoList)
                        {
                            if (PhotoList.FirstOrDefault(e => e.CharacterId == p.CharacterId && e.Seq == p.Seq) is PhotoViewModel pvm)
                            {
                                pvm.Set(ep);

                                var newList = Order(PhotoList).ToList();
                                if (!newList.SequenceEqual(PhotoList))
                                {
                                    PhotoList.Set(newList);
                                }
                            }
                        }
                    }
                }
                else
                {
                    continue;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            await Task.Delay(250);
        }
    }
}