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
            MainLabel.Content = main;
            SubLabel.Content = sub;
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
            switch (type)
            {
                case MediaType.Music:
                    MainIcon.Icon = IconType.musnote;
                    break;
                case MediaType.Video:
                    MainIcon.Icon = IconType.ondemand_video;
                    break;
                case MediaType.Online:
                    MainIcon.Icon = IconType.cloud;
                    break;
                default: break;
            }
            DefaultIcon = MainIcon.Icon;
            Manip = new string[] { main, sub };
        }
        public void Revoke(InfoExchangeArgs e)
        {
            Revoke(e.Integer, e.Media.Title, e.Media.Artist, e.Media.MediaType);
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
            DownloadButton.Opacity = 0;

            //Load SubContextMenuItems
            if (DefaultIcon == IconType.cloud)
            {
                MenuItem[] OnlineMenuItems = new MenuItem[]
                {
                    GetMenu("Download"),
                    GetMenu("Play After"),
                    GetMenu("Repeat", "2 times", "3 times", "5 times", "10 times", "Forever"),
                    GetMenu("Remove"),
                    GetMenu("Properties")
                };
                OnlineMenuItems[0].Click += delegate { DownloadButton_Click(this, null); };
                OnlineMenuItems[1].Click += delegate { PlayAfterRequested?.Invoke(this, new InfoExchangeArgs(MediaIndex)); };
                OnlineMenuItems[2].Click += delegate { };
                OnlineMenuItems[3].Click += delegate { RemoveRequested?.Invoke(this, new InfoExchangeArgs(MediaIndex)); };
                OnlineMenuItems[4].Click += delegate { PropertiesRequested?.Invoke(this, new InfoExchangeArgs(MediaIndex)); };
                ContextMenu = new ContextMenu() { ItemsSource = OnlineMenuItems };
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
        private void OutputCanvas_MouseDoubleClick(object sender, MouseButtonEventArgs e) => DoubleClicked?.Invoke(this, new InfoExchangeArgs(MediaIndex));

        private void Play_Click(object sender, EventArgs e) => PlayClicked?.Invoke(this, new InfoExchangeArgs(MediaIndex));
        private void PlayAfter_Click(object sender, RoutedEventArgs e) => PlayAfterRequested?.Invoke(this, new InfoExchangeArgs(MediaIndex));
        private void Remove_Click(object sender, RoutedEventArgs e) => RemoveRequested?.Invoke(this, new InfoExchangeArgs(MediaIndex));
        private void Delete_Click(object sender, RoutedEventArgs e) => DeleteRequested?.Invoke(this, new InfoExchangeArgs(MediaIndex));
        private void Location_Click(object sender, RoutedEventArgs e) => LocationRequested?.Invoke(this, new InfoExchangeArgs(MediaIndex));
        private void Properties_Click(object sender, RoutedEventArgs e) => PropertiesRequested?.Invoke(this, new InfoExchangeArgs(MediaIndex));

        WebClient Client = null;
        public void Download(Media media)
        {
            var SavePath = $"{App.Path}Downloads\\{media.Title}";
            DownloadButton.Icon = IconType.cancel;
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
                    DownloadButton.Icon = IconType.cloud_download;
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
    }
}
