using System.Diagnostics;
using Shipwreck.PrimagiBrowser.Models;
using Shipwreck.ViewModelUtils;

namespace Shipwreck.PrimagiBrowser.ViewModels;

public class PhotoViewModel : ObservableModel
{
    private readonly PhotoRecord _Record;

    internal PhotoViewModel(PhotoListTabViewModel tab, PhotoRecord record)
    {
        Tab = tab;
        _Record = record;
        Set(record);
    }

    public PhotoListTabViewModel Tab { get; }

    #region Character

    private CharacterViewModel? _Character;

    public CharacterViewModel? Character
    {
        get => _Character;
        private set => SetProperty(ref _Character, value);
    }

    #endregion Character

    public int CharacterId => _Record.CharacterId;
    public string Seq => _Record.Seq;

    #region PlayDate

    private DateTime _PlayDate;

    public DateTime PlayDate
    {
        get => _PlayDate;
        private set => SetProperty(ref _PlayDate, value);
    }

    #endregion PlayDate

    #region Image

    private ImageSource? _Image;

    public ImageSource? Image
    {
        get => _Image;
        private set => SetProperty(ref _Image, value);
    }

    #endregion Image

    #region Thumbnail

    private ImageSource? _Thumbnail;

    public ImageSource? Thumbnail
    {
        get => _Thumbnail;
        private set => SetProperty(ref _Thumbnail, value);
    }

    #endregion Thumbnail

    #region IsSelected

    private bool _IsSelected;

    public bool IsSelected
    {
        get => _IsSelected;
        set => SetProperty(ref _IsSelected, value);
    }

    #endregion IsSelected

    internal void Set(PhotoRecord record)
    {
        static ImageSource? toImageSource(string? path)
        {
            if (path == null)
            {
                return null;
            }
            var b = new BitmapImage();
            b.BeginInit();
            b.UriSource = new Uri(path);
            b.EndInit();
            return b;
        }

        Character = record.Character != null ? Tab.GetOrCreate(record.Character) : Tab.GetById(record.CharacterId);
        PlayDate = record.PlayDate;
        Image = toImageSource(record.ImagePath);
        Thumbnail = toImageSource(record.ThumbPath);
    }

    public void Open()
    {
        if (Image is BitmapImage bmp && bmp.UriSource?.Scheme == Uri.UriSchemeFile)
        {
            try
            {
                Process.Start(new ProcessStartInfo(bmp.UriSource.LocalPath)
                {
                    UseShellExecute = true
                });
            }
            catch { }
        }
    }
}