using Player.Controls;
using Player.Events;
using System;
using System.Windows.Media;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.ComponentModel;
#pragma warning disable 1591
namespace Player
{
    public partial class MediaView : UserControl
    {
        public event EventHandler<MediaEventArgs> DoubleClicked;
        public event EventHandler<MediaEventArgs> PlayClicked;

        public event EventHandler<MediaEventArgs> DeleteRequested;
        public event EventHandler<MediaEventArgs> RemoveRequested;
        public event EventHandler<MediaEventArgs> PlayAfterRequested;
        public event EventHandler<MediaEventArgs> PropertiesRequested;
        public event EventHandler<MediaEventArgs> RepeatRequested;
        public event EventHandler<MediaEventArgs> LocationRequested;
        public event EventHandler<MediaEventArgs> DownloadRequested;
        public event EventHandler<MediaEventArgs> Downloaded;

        string[] Manip = new string[2];

        public int MediaIndex { get; set; }
        public Preferences Settings;
        private bool isPlaying = false;
        public bool IsPlaying
        {
            get => isPlaying; set
            {
                isPlaying = value;
                MainIcon.Icon = value ? IconType.equalizer : DefaultIcon;
                MainIcon.Foreground = value ? Brushes.DeepSkyBlue : Brushes.White;
                MainLabel.Foreground = value ? Brushes.DeepSkyBlue : Brushes.White;
                SubLabel.Foreground = value ? Brushes.DeepSkyBlue : Brushes.White;
            }
        }
        public IconType DefaultIcon { get; set; }
        public MediaView()
        {
            InitializeComponent();
        }
        public MediaView(int index, string main, string sub, MediaType type = MediaType.Music)
        {
            InitializeComponent();
            string manip = "";
            if (main == null)
                main = "Unknown";
            string full = main;
            for (int i = 0; i < full.Length; i++)
            {
                manip += full[i];
                LoadAnim.KeyFrames.Add(new DiscreteStringKeyFrame(manip));
            }
            manip = "";
            full = sub;
            for (int i = 0; i < full.Length; i++)
            {
                manip += full[i];
                LoadAnim2.KeyFrames.Add(new DiscreteStringKeyFrame(manip));
            }
            MediaIndex = index;
            switch (type)
            {
                case MediaType.Music:
                    DefaultIcon = IconType.musnote;
                    break;
                case MediaType.Video:
                    DefaultIcon = IconType.ondemand_video;
                    break;
                case MediaType.Online:
                    DefaultIcon = IconType.cloud;
                    break;
                case MediaType.NotMedia:
                    DefaultIcon = IconType.none;
                    break;
                default:
                    DefaultIcon = IconType.none;
                    break;
            }
            MainIcon.Icon = DefaultIcon;
            Manip = new string[] { main, sub };
            if (type != MediaType.Online)
                DownloadButton.Visibility = Visibility.Hidden;
        }
        public void Revoke(int index, string main, string sub, MediaType type = MediaType.Music)
        {
            MediaIndex = index;
            MainLabel.Content = main;
            SubLabel.Content = sub;
            MainIcon.Icon = type == MediaType.Music ? IconType.musnote : IconType.musvideo;
            DefaultIcon = MainIcon.Icon;
            Manip = new string[] { main, sub };
        }
        public void Revoke(MediaEventArgs e)
        {
            Revoke(e.Index, e.Media.Title, e.Media.Artist, e.Media.MediaType);
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
            PlayButton.Opacity = 0;
            OtherButton.Opacity = 0;
        }

        private void OutputCanvas_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DoubleClicked?.Invoke(this, new MediaEventArgs(MediaIndex));
        }

        private void Play_Click(object sender, EventArgs e)
        {
            
            PlayClicked?.Invoke(this, new MediaEventArgs(MediaIndex));
            OtherPopup.IsOpen = false;
        }
        private void PlayAfter_Click(object sender, RoutedEventArgs e)
        {
            PlayAfterRequested?.Invoke(this, new MediaEventArgs(MediaIndex));
            OtherPopup.IsOpen = false;
        }
        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            RemoveRequested?.Invoke(this, new MediaEventArgs(MediaIndex));
            OtherPopup.IsOpen = false;
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            DeleteRequested?.Invoke(this, new MediaEventArgs(MediaIndex));
            OtherPopup.IsOpen = false;
        }
        private void Location_Click(object sender, RoutedEventArgs e)
        {
            LocationRequested?.Invoke(this, new MediaEventArgs(MediaIndex));
            OtherPopup.IsOpen = false;
        }
        private void Properties_Click(object sender, RoutedEventArgs e)
        {
            PropertiesRequested?.Invoke(this, new MediaEventArgs(MediaIndex));
            OtherPopup.IsOpen = false;
        }
        private void Other_Click(object sender, EventArgs e)
        {
            OtherPopup.IsOpen = true;
        }

        WebClient Client = null;
        public void Download(Media media)
        {
            var SavePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)}\\{media.Title}";
            DownloadButton.Icon = IconType.cloud;
            Client = new WebClient();
            Client.DownloadProgressChanged += (o, f) =>
            {
                MainLabel.Content = $"Downloading... - {f.ProgressPercentage}% ";
                SubLabel.Content = $"{f.BytesReceived}KB \\ {f.TotalBytesToReceive}KB";
            };
            Client.DownloadFileCompleted += (o, f) =>
            {
                MainLabel.Content = "Downloaded, Processing...";
                Downloaded?.Invoke(this, new MediaEventArgs()
                {
                    Index = MediaIndex,
                    Media = new Media(SavePath)
                });
            };
            Client.DownloadFileAsync(new Uri(media.Path, UriKind.Absolute), SavePath);
        }
        private void DownloadButton_Click(object sender, EventArgs e)
        {
            if (Client.IsBusy)
            {
                var res = MessageBox.Show("Sure to cancel?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    Client.CancelAsync();
                    DownloadButton.Icon = IconType.cloud_download;
                    MainLabel.Content = Manip[0];
                    SubLabel.Content = Manip[1];
                    return;
                }
            }
            else
                DownloadRequested?.Invoke(this, new MediaEventArgs() { Index = MediaIndex, Sender = this });
        }
    }
}
