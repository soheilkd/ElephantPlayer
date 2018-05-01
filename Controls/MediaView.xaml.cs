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

        public event EventHandler<InfoExchangeArgs> DoubleClicked;
        public event EventHandler<InfoExchangeArgs> PlayClicked;

        public event EventHandler<InfoExchangeArgs> DeleteRequested;
        public event EventHandler<InfoExchangeArgs> RemoveRequested;
        public event EventHandler<InfoExchangeArgs> PlayAfterRequested;
        public event EventHandler<InfoExchangeArgs> PropertiesRequested;
        public event EventHandler<InfoExchangeArgs> RepeatRequested;
        public event EventHandler<InfoExchangeArgs> LocationRequested;
        public event EventHandler<InfoExchangeArgs> DownloadRequested;
        public event EventHandler<InfoExchangeArgs> Downloaded;
        public event EventHandler<InfoExchangeArgs> ZipDownloaded;

        string[] Manip = new string[2];

        public int MediaIndex { get; set; }
        public Preferences Settings;
        private bool isPlaying = false;
        public bool IsPlaying
        {
            get => isPlaying; set
            {
                isPlaying = value;
                MainIcon.Icon = value ? IconType.Equalizer : DefaultIcon;
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
        public MediaView(int index, string main, string sub, string time, MediaType type = MediaType.Music)
        {
            InitializeComponent();
            Revoke(index, main, sub,time, type);
        }
        public MediaView(int index, Media media) : this(index, media.Title, media.Artist, MainUI.CastTime(media.Length), media.MediaType) { }

        public void Revoke(int index, string main, string sub, string time, MediaType type = MediaType.Music)
        {
            MainLabel.Content = main;
            SubLabel.Content = sub;
            MediaIndex = index;
            switch (type)
            {
                case MediaType.Music:
                    DefaultIcon = IconType.MusicNote;
                    break;
                case MediaType.Video:
                    DefaultIcon = IconType.OndemandVideo;
                    break;
                case MediaType.OnlineMusic:
                case MediaType.OnlineVideo:
                case MediaType.OnlineFile:
                    DefaultIcon = IconType.Cloud;
                    break;
                default:
                    DefaultIcon = IconType.None;
                    break;
            }
            MainIcon.Icon = DefaultIcon;
            Manip = new string[] { main, sub };
            TimeLabel.Content = time;
            if ((int)type < 3)
                DownloadButton.Visibility = Visibility.Hidden;
        }
        public void Revoke(int index, Media media) => Revoke(index, media.Title, media.Artist, MainUI.CastTime(media.Length), media.MediaType);
        public void Revoke(InfoExchangeArgs e) => Revoke(e.Integer, e.Media.Title, e.Media.Artist, MainUI.CastTime(e.Media.Length), e.Media.MediaType);
        public void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            PlayButton.Opacity = 0;
            DownloadButton.Opacity = 0;

            //Load SubContextMenuItems
            if (DefaultIcon == IconType.Cloud)
            {
                MenuItem[] OnlineMediaMenu = new MenuItem[]
                {
                    GetMenu("Download"),
                    GetMenu("Play After"),
                    GetMenu("Repeat", "2 times", "3 times", "5 times", "10 times", "Forever"),
                    GetMenu("Remove"),
                    GetMenu("Properties")
                };
                OnlineMediaMenu[0].Click += delegate { DownloadButton_Click(this, null); };
                OnlineMediaMenu[1].Click += delegate { PlayAfterRequested?.Invoke(this, new InfoExchangeArgs(MediaIndex)); };
                OnlineMediaMenu[2].Click += delegate { };
                OnlineMediaMenu[3].Click += delegate { RemoveRequested?.Invoke(this, new InfoExchangeArgs(MediaIndex)); };
                OnlineMediaMenu[4].Click += delegate { PropertiesRequested?.Invoke(this, new InfoExchangeArgs(MediaIndex)); };
                (OnlineMediaMenu[2].Items[0] as MenuItem).Click += delegate
                {
                    RepeatRequested?.Invoke(this, new InfoExchangeArgs()
                    {
                        Integer = MediaIndex,
                        Object = 2
                    });
                };
                (OnlineMediaMenu[2].Items[1] as MenuItem).Click += delegate
                {
                    RepeatRequested?.Invoke(this, new InfoExchangeArgs()
                    {
                        Integer = MediaIndex,
                        Object = 3
                    });
                };
                (OnlineMediaMenu[2].Items[2] as MenuItem).Click += delegate
                {
                    RepeatRequested?.Invoke(this, new InfoExchangeArgs()
                    {
                        Integer = MediaIndex,
                        Object = 5
                    });
                };
                (OnlineMediaMenu[2].Items[3] as MenuItem).Click += delegate
                {
                    RepeatRequested?.Invoke(this, new InfoExchangeArgs()
                    {
                        Integer = MediaIndex,
                        Object = 10
                    });
                };
                (OnlineMediaMenu[2].Items[4] as MenuItem).Click += delegate
                {
                    RepeatRequested?.Invoke(this, new InfoExchangeArgs()
                    {
                        Integer = MediaIndex,
                        Object = 999
                    });
                };
                ContextMenu = new ContextMenu() { ItemsSource = OnlineMediaMenu };
            }
            else
            {
                MenuItem[] OfflineMediaMenu = new MenuItem[]
                    {
                     GetMenu("Play After"),
                     GetMenu("Repeat", "2 times", "3 times", "5 times", "10 times", "Forever"),
                     GetMenu("Remove"),
                     GetMenu("Delete"),
                     GetMenu("Move...", "To Default Loc", "Browse"),
                     GetMenu("Copy...", "To Default Loc", "Browse"),
                     GetMenu("Open Location"),
                     GetMenu("Properties")
                    };
                OfflineMediaMenu[0].Click += delegate { PlayAfterRequested?.Invoke(this, new InfoExchangeArgs(MediaIndex)); };
                OfflineMediaMenu[1].Click += delegate { };
                OfflineMediaMenu[2].Click += delegate { RemoveRequested?.Invoke(this, new InfoExchangeArgs(MediaIndex)); };
                OfflineMediaMenu[3].Click += delegate { DeleteRequested?.Invoke(this, new InfoExchangeArgs(MediaIndex)); };
                OfflineMediaMenu[4].Click += delegate { };
                OfflineMediaMenu[5].Click += delegate { };
                OfflineMediaMenu[6].Click += delegate { LocationRequested?.Invoke(this, new InfoExchangeArgs(MediaIndex)); };
                OfflineMediaMenu[7].Click += delegate { PropertiesRequested?.Invoke(this, new InfoExchangeArgs(MediaIndex)); };
                (OfflineMediaMenu[1].Items[0] as MenuItem).Click += delegate
                {
                    RepeatRequested?.Invoke(this, new InfoExchangeArgs()
                    {
                        Integer = MediaIndex,
                        Object = 2
                    });
                };
                (OfflineMediaMenu[1].Items[1] as MenuItem).Click += delegate
                {
                    RepeatRequested?.Invoke(this, new InfoExchangeArgs()
                    {
                        Integer = MediaIndex,
                        Object = 3
                    });
                };
                (OfflineMediaMenu[1].Items[2] as MenuItem).Click += delegate
                {
                    RepeatRequested?.Invoke(this, new InfoExchangeArgs()
                    {
                        Integer = MediaIndex,
                        Object = 5
                    });
                };
                (OfflineMediaMenu[1].Items[3] as MenuItem).Click += delegate
                {
                    RepeatRequested?.Invoke(this, new InfoExchangeArgs()
                    {
                        Integer = MediaIndex,
                        Object = 10
                    });
                };
                (OfflineMediaMenu[1].Items[4] as MenuItem).Click += delegate
                {
                    RepeatRequested?.Invoke(this, new InfoExchangeArgs()
                    {
                        Integer = MediaIndex,
                        Object = 999
                    });
                };
                ContextMenu = new ContextMenu() { ItemsSource = OfflineMediaMenu };
            }
        }

        private static MenuItem GetMenu(string header) => new MenuItem() { Header = header };
        private static MenuItem GetMenu(string header, params string[] subitems)
        {
            var output = new MenuItem() { Header = header };
            foreach (var item in subitems)
                output.Items.Add(new MenuItem() { Header = item });
            return output;
        }

        
        WebClient Client = null;
        public void Download(Media media)
        {
            var SavePath = $"{App.Path}Downloads\\{media.Title}";
            DownloadButton.Icon = IconType.Cancel;
            Client = new WebClient();
            Client.DownloadProgressChanged += (o, f) =>
            {
                MainLabel.Content = $"Downloading... - {f.ProgressPercentage}% ";
                SubLabel.Content = $"{f.BytesReceived / 1024} KB \\ {f.TotalBytesToReceive / 1024} KB";
            };
            Client.DownloadFileCompleted += (o, f) =>
            {
                if (downloadCanceled)
                {
                    DownloadButton.Icon = IconType.CloudLoad;
                    MainLabel.Content = Manip[0];
                    SubLabel.Content = Manip[1];
                    System.IO.File.Delete(SavePath);
                    return;
                }
                MainLabel.Content = "Downloaded, Processing...";
                DownloadButton.Visibility = Visibility.Hidden;
                if (Manip[0].EndsWith(".zip"))
                {
                    MainLabel.Content = "Extracting...";
                    SubLabel.Content = "";
                    using (var zip = new Ionic.Zip.ZipFile(SavePath))
                    {
                        var folderToExtract = SavePath.Substring(0, SavePath.IndexOf(".zip")) + "\\";
                        zip.ExtractAll(folderToExtract, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
                        ZipDownloaded?.Invoke(this, new InfoExchangeArgs(InfoType.Management)
                        {
                            ObjectArray = System.IO.Directory.GetFiles(folderToExtract, "*.*", System.IO.SearchOption.AllDirectories),
                            Type = InfoType.StringArray
                        });
                    }
                    System.IO.File.Delete(SavePath);
                }
                else
                    Downloaded?.Invoke(this, new InfoExchangeArgs()
                    {
                        Integer = MediaIndex,
                        Media = new Media(SavePath)
                    });
            };
            Client.DownloadFileAsync(new Uri(media.Path, UriKind.Absolute), SavePath);
        }

        private void DownloadButton_Click(object sender, EventArgs e)
        {
            if (Client == null)
                Client = new WebClient();
            if (Client.IsBusy)
            {
                var res = MessageBox.Show("Sure to cancel?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    downloadCanceled = true;
                    Client.CancelAsync();
                    return;
                }
            }
            else
            {

                DownloadRequested?.Invoke(this, new InfoExchangeArgs() { Integer = MediaIndex, Object = this });
                downloadCanceled = false;
            }
        }
        bool downloadCanceled = true;

        private void Play_Click(object sender, EventArgs e) => PlayClicked?.Invoke(this, new InfoExchangeArgs(MediaIndex));
        private void OutputCanvas_MouseDoubleClick(object sender, MouseButtonEventArgs e) => DoubleClicked?.Invoke(this, new InfoExchangeArgs(MediaIndex));
    }
}
