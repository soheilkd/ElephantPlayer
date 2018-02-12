using Player.User;
using Player.Events;
using Player.Getters;
using Player.Types;
using Player.User;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
#pragma warning disable 1591
namespace Player
{
    public partial class MetaEditor : Window
    {
        Media media;
        Media Media { get => media; set => media = value; }
        string NewArtworkSource = "nile";
        public event EventHandler<MediaEventArgs> SaveRequested;
        public event EventHandler<MediaEventArgs> PreviousTagRequested;
        public event EventHandler<MediaEventArgs> NextTagRequested;
        new public MediaView Parent { get; set; }
        bool IsDigitsOnly(string str)
        {
            for (int i = 0; i < str.Length; i++)
                if (Char.IsDigit(str[i])) return false;
            return true;
        }
        public MetaEditor()
        {
            InitializeComponent();
            YearBox.TextChanged += YearBox_TextChanged;
            TrackBox.TextChanged += YearBox_TextChanged;
        }

        private void YearBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            YearBox.Background = Theming.ToBrush( Colors.White);
            TrackBox.Background = Theming.ToBrush(Colors.White);
        }

        TagLib.File MainFile;
        public void Load(ref MediaView RefToParent)
        {
            media = RefToParent.Media;
            MainFile = TagLib.File.Create(media.Path);
            var t = MainFile.Tag;
            TitleBox.Text = t.Title;
            AlbumBox.Text = t.Album;
            AlbumArtistBox.Text = t.FirstAlbumArtist;
            ArtistBox.Text = t.FirstPerformer;
            ComposerBox.Text = t.FirstComposer;
            ConductorBox.Text = t.Conductor;
            CopyrightBox.Text = t.Copyright;
            TrackBox.Text = t.Track.ToString();
            GenreBox.Text = t.FirstGenre;
            LyricsBox.Text = t.Lyrics;
            LyricsBox.AppendText(Lyrics.Get(media.Path));
            YearBox.Text = t.Year.ToString();
            CommentBox.Text = t.Comment;
            ArtworkImage.Source = media.Artwork;
            _Artwork = Image.ToImage(media.Artwork);
            Parent = RefToParent;
            Show();
        }
        BitmapImage _Artwork;
        private void SaveClick(object sender, RoutedEventArgs e)
        {
            bool er = false;
            if (!uint.TryParse(YearBox.Text, out _))
            {
                YearBox.Background = Theming.ToBrush(Colors.Red);
                er = true;
            }
            if (!uint.TryParse(TrackBox.Text, out _))
            {
                TrackBox.Background = Theming.ToBrush(Colors.Red);
                er = true;
            }
            if (er) return;
            MainFile.Tag.Title = TitleBox.Text;
            MainFile.Tag.Performers = ArtistBox.Text.Split(';');
            MainFile.Tag.Album = AlbumBox.Text;
            MainFile.Tag.AlbumArtists = AlbumArtistBox.Text.Split(';');
            MainFile.Tag.Track = uint.Parse(TrackBox.Text);
            MainFile.Tag.Comment = CommentBox.Text;
            if (LyricsProviderBox.IsChecked.Value)
                Lyrics.Set(MainFile.Name, LyricsBox.Text);
            else
                MainFile.Tag.Lyrics = LyricsBox.Text;
            MainFile.Tag.Copyright = CopyrightBox.Text;
            MainFile.Tag.Conductor = ConductorBox.Text;
            MainFile.Tag.Composers = ComposerBox.Text.Split(';');
            MainFile.Tag.Genres = GenreBox.Text.Split(';');
            MainFile.Tag.Year = uint.Parse(YearBox.Text);
            if (NewArtworkSource != "nile")
            {
                MainFile.Tag.Pictures = new TagLib.IPicture[]
                {
                    new TagLib.Picture(
                        new TagLib.ByteVector((byte[])new System.Drawing.ImageConverter().ConvertTo(System.Drawing.Image.FromFile(NewArtworkSource), typeof(byte[]))))

                };
            }
            SaveRequested.Invoke(this, new MediaEventArgs() { File = MainFile, Sender = Parent });
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();

        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try { DragMove(); } catch (Exception) { }
        }

        private void ArtworkImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var (ok, files) = Dialogs.RequestFiles("Images | *.jpg; *.png;", false);
            if (ok)
            {
                ArtworkImage.Source = new BitmapImage(new Uri(files[0], UriKind.Absolute));
                NewArtworkSource = files[0];
            }
        }

        private void MaterialButton_MouseUp(object sender, EventArgs e)
        {
            SaveClick(this, null);
            PreviousTagRequested?.Invoke(this, new MediaEventArgs() { Sender = Parent });
        }

        private void MaterialButton_MouseUp_1(object sender, EventArgs e)
        {
            SaveClick(this, null);
            NextTagRequested?.Invoke(this, new MediaEventArgs() { Sender = Parent });
        }

        private void LyricsProviderBox_Checked(object sender, RoutedEventArgs e)
        {
            FileLyrics = LyricsBox.Text;
            LyricsBox.Text = ProviderLyrics;
        }
        string ProviderLyrics = "";
        string FileLyrics = "";
        private void LyricsProviderBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ProviderLyrics = LyricsBox.Text;
            LyricsBox.Text = FileLyrics;
        }
    }
}
