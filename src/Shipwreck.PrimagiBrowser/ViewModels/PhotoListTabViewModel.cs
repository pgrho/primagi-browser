using System.Collections.ObjectModel;
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

    private ReadOnlyCollection<DateOnly>? _Months;

    public ReadOnlyCollection<DateOnly> Months
        => _Months ??= Array.AsReadOnly(
            Enumerable.Range(0, int.MaxValue)
            .Select(e =>
            {
                var td = DateTime.Today;
                td = td.AddDays(1 - td.Day);
                return DateOnly.FromDateTime(td.AddMonths(-e));
            })
            .TakeWhile(e => e >= new DateOnly(2021, 10, 1))
            .ToArray());

    #endregion Months

    #region SelectedMonth

    private DateOnly _SelectedMonth;

    public DateOnly SelectedMonth
    {
        get
        {
            if (!Months.Contains(_SelectedMonth))
            {
                _SelectedMonth = Months[0];
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

    #region _Characters

    private readonly Dictionary<int, CharacterViewModel> _Characters = new();

    internal CharacterViewModel GetOrCreate(CharacterRecord character)
    {
        lock (_Characters)
        {
            if (!_Characters.TryGetValue(character.Id, out var vm))
            {
                _Characters[character.Id] = vm = new CharacterViewModel(character);
            }
            return vm;
        }
    }

    internal CharacterViewModel? GetById(int characterId)
    {
        lock (_Characters)
        {
            _Characters.TryGetValue(characterId, out var vm);

            return vm;
        }
    }

    #endregion _Characters

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

    private async void BeginLoadPhoto()
    {
        var min = SelectedMonth.ToDateTime(default);
        var max = min.AddMonths(1);

        using var db = await BrowserDbContext.CreateDbAsync();

        var ps = await db.Photo!.Include(e => e.Character)
                        .Where(e => min <= e.PlayDate && e.PlayDate < max)
                        .OrderByDescending(e => e.PlayDate)
                        .ThenByDescending(e => e.Seq)
                        .ToListAsync();

        lock (PhotoList)
        {
            PhotoList.Set(ps.Select(e => new PhotoViewModel(this, e)));
        }

        Enqueue(ps);
    }

    #endregion PhotoList

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

                var m = SelectedMonth;

                if (p.PlayDate.Year == m.Year
                    && p.PlayDate.Month == m.Month)
                {
                    lock (PhotoList)
                    {
                        if (!PhotoList.Any(e => e.CharacterId == p.CharacterId && e.Seq == p.Seq))
                        {
                            PhotoList.Add(new PhotoViewModel(this, p));
                        }
                    }
                }
            }

            var t = _DownloadTask;

            switch (t?.Status)
            {
                case TaskStatus.WaitingForActivation:
                case TaskStatus.WaitingToRun:
                case TaskStatus.Running:
                    break;
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
                    var fi = new FileInfo(System.IO.Path.Combine(bp, string.Format("{1:yyyy-MM-dd}/{1:HHmmss}-{0}-{2}.jpg", p.Seq, pd, cvm?.Name ?? p.CharacterId.ToString())));

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

                    var fi = new FileInfo(System.IO.Path.Combine(bp, string.Format("{1:yyyy-MM-dd}/{1:HHmmss}-{0}-{2}_thumb.jpg", p.Seq, pd, cvm?.Name ?? p.CharacterId.ToString())));

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
                            PhotoList.FirstOrDefault(e => e.CharacterId == p.CharacterId && e.Seq == p.Seq)?.Set(ep);
                        }
                    }
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