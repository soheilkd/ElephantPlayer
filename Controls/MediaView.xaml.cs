using Player.Controls;
using Player.Events;
using Player.Types;
using System;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
#pragma warning disable 1591
namespace Player
{
    public partial class MediaView : UserControl
    {
        public event EventHandler<MediaEventArgs> DoubleClicked;
        public event EventHandler<MediaEventArgs> PlayClicked;

        public int MediaIndex { get; set; }
        public Preferences Settings;
        private bool isPlaying = false;
        public bool IsPlaying { get => isPlaying; set
            {
                isPlaying = value;
                MainIcon.Icon = value ? IconType.ic_equalizer : DefaultIcon;
                MainIcon.Foreground = value ? Brushes.DeepSkyBlue : Brushes.White;
            } }
        public IconType DefaultIcon { get; set; }
        public MediaView()
        {
            InitializeComponent();
        }
        public MediaView(int index, string main, string sub, MediaType type = MediaType.Music)
        {
            InitializeComponent();
            MediaIndex = index;
            MainLabel.Content = main;
            SubLabel.Content = sub;
            
            MainIcon.Icon = type == MediaType.Music ? IconType.ic_music_note : IconType.ic_ondemand_video;
            DefaultIcon = MainIcon.Icon;
        }
        public void Revoke(int index, string main, string sub, MediaType type = MediaType.Music)
        {
            MediaIndex = index;
            MainLabel.Content = main;
            SubLabel.Content = sub;
            MainIcon.Icon = type == MediaType.Music ? IconType.ic_music_note : IconType.ic_music_video;
            DefaultIcon = MainIcon.Icon;
        }
        public void Revoke(MediaEventArgs e)
        {
            Revoke(e.Index, e.Media.Title, e.Media.Artist);
        }
        public long MediaLength { get; set; }
        public MediaView(Preferences pref)
        {
            InitializeComponent();
            IsPlaying = false;
            Settings = pref;
        }
        public void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void OutputCanvas_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DoubleClicked?.Invoke(this, new MediaEventArgs(MediaIndex));
        }

        private void MaterialButton_Click(object sender, EventArgs e)
        {
            PlayClicked?.Invoke(this, new MediaEventArgs(MediaIndex));
        }
    }
}
