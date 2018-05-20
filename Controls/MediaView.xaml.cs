using Player.Controls;
using Player.Events;
using System;
using System.Windows.Media;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using static Player.Global;

#pragma warning disable 1591
namespace Player
{
    public partial class MediaView : UserControl
    {
        public Media Media;

        public event EventHandler<InfoExchangeArgs> DoubleClicked, PlayClicked;

        public event EventHandler<InfoExchangeArgs>
            TagSaveRequested,
            RemoveRequested,
            PlayAfterRequested,
            RepeatRequested,
            ZipDownloaded;

        string[] OriginalStringsOfLabels = new string[2];

        public Glyph DefaultIcon { get; set; }
        bool downloadCanceled = true;
        private bool isPlaying = false;
        public bool IsPlaying
        {
            get => isPlaying;
            set
            {
                isPlaying = value;
                MainIcon.Glyph = value ? Glyph.Speakers : DefaultIcon;
                MainIcon.Foreground = value ? Brushes.DeepSkyBlue : Brushes.White;
                MainLabel.Foreground = value ? Brushes.DeepSkyBlue : Brushes.White;
                SubLabel.Foreground = value ? Brushes.DeepSkyBlue : Brushes.White;
            }
        }

        WebClient Client = null;

        public MediaView() => InitializeComponent();
        public MediaView(Media media) : this()
        {
            Revoke(media);
        }


        public void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            PlayButton.Opacity = 0;
            DownloadButton.Opacity = 0;

            RoutedEventHandler RepeatInvokation(int count) => (_, __) => RepeatRequested?.Invoke(this, new InfoExchangeArgs(count));
            //Load SubContextMenuItems
            if (DefaultIcon == Glyph.Cloud)
            {
                MenuItem[] OnlineMediaMenu = new MenuItem[]
                {
                    GetMenu("Download", (_,__) => DownloadButton_Clicked(this, null)),
                    GetMenu("Play After", (_,__) => PlayAfterRequested?.Invoke(this, null)),
                    GetMenu("Repeat",
                    new (string, RoutedEventHandler)[]
                    {
                        ("2 times", (_,__) => RepeatInvokation(2)),
                        ("3 times", (_,__) => RepeatInvokation(3)),
                        ("5 times", (_,__) => RepeatInvokation(5)),
                        ("10 times", (_,__) => RepeatInvokation(10)),
                        ("50 times", (_,__) => RepeatInvokation(50))
                    }),
                    GetMenu("Remove", (_,__) => RemoveRequested?.Invoke(this, null)),
                    GetMenu("Properties", new RoutedEventHandler(PropertiesRequest))
                };
                ContextMenu = new ContextMenu() { ItemsSource = OnlineMediaMenu };
            }
            else
            {
                MenuItem[] OfflineMediaMenu = new MenuItem[]
                    {
                     GetMenu("Play After", (_,__) => PlayAfterRequested?.Invoke(this, null)),
                     GetMenu("Repeat",
                     new (string, RoutedEventHandler)[]
                     {
                         ("2 times", (_,__) => RepeatInvokation(2)),
                         ("3 times", (_,__) => RepeatInvokation(3)),
                         ("5 times", (_,__) => RepeatInvokation(5)),
                         ("10 times", (_,__) => RepeatInvokation(10)),
                         ("50 times", (_,__) => RepeatInvokation(50))
                     }),
                     GetMenu("Remove", new RoutedEventHandler(RemoveRequest)),
                     GetMenu("Delete", new RoutedEventHandler(DeleteRequest)),
                     GetMenu("Move...",
                     new (string, RoutedEventHandler)[]
                     {
                         ("To Last Location", (_,__) => MoveTo(App.Preferences.LastPath + Media.Name)),
                         ("Browse...", (_,__) =>
                         {

                         })
                     }),
                     GetMenu("Copy...",
                     new (string, RoutedEventHandler)[]
                     {
                         ("To Last Location", (_,__) => File.Copy(Media.Path, App.Preferences.LastPath + Media.Name)),
                         ("Browse...", (_,__) =>
                         {

                         })
                     }),
                     GetMenu("Open Location", new RoutedEventHandler(LocationRequest)),
                     GetMenu("Properties", new RoutedEventHandler(PropertiesRequest))
                    };
                ContextMenu = new ContextMenu() { ItemsSource = OfflineMediaMenu };
            }
        }

        private void DownloadButton_Clicked(object sender, MouseButtonEventArgs e)
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
                var SavePath = $"{App.Path}Downloads\\{Media.Title}";
                DownloadButton.Glyph = Glyph.Cancel;
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
                        DownloadButton.Glyph = Glyph.Cloud;
                        MainLabel.Content = OriginalStringsOfLabels[0];
                        SubLabel.Content = OriginalStringsOfLabels[1];
                        System.IO.File.Delete(SavePath);
                        return;
                    }
                    MainLabel.Content = "Downloaded, Processing...";
                    DownloadButton.Visibility = Visibility.Hidden;
                    if (OriginalStringsOfLabels[0].EndsWith(".zip"))
                    {
                        MainLabel.Content = "Extracting...";
                        SubLabel.Content = "";
                        using (var zip = new Ionic.Zip.ZipFile(SavePath))
                        {
                            var folderToExtract = SavePath.Substring(0, SavePath.IndexOf(".zip")) + "\\";
                            zip.ExtractAll(folderToExtract, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
                            ZipDownloaded?.Invoke(this, new InfoExchangeArgs(InfoType.MediaUpdate)
                            {
                                Object = System.IO.Directory.GetFiles(folderToExtract, "*.*", System.IO.SearchOption.AllDirectories),
                                Type = InfoType.StringArray
                            });
                        }
                        File.Delete(SavePath);
                    }
                    else
                    {
                        Revoke(new Media(SavePath));
                    }
                };
                Client.DownloadFileAsync(new Uri(Media.Path, UriKind.Absolute), SavePath);
                downloadCanceled = false;
            }
        }
        public void RenderWidth(double newWidth) => Width = newWidth;
        private void Play_Clicked(object sender, MouseButtonEventArgs e) =>
            PlayClicked?.Invoke(this, null);
        private void Canvas_DoubleClicked(object sender, MouseButtonEventArgs e) => DoubleClicked?.Invoke(this, null);

        private void Size_Changed(object sender, SizeChangedEventArgs e)
        {
            if (ActualWidth > 500)
            {
                SubLabel.Visibility = Visibility.Hidden;
                SubLabel.Margin = new Thickness(30, 0, 40, 0);
                MainLabel.Content = $"{OriginalStringsOfLabels[1]} - {OriginalStringsOfLabels[0]}";
                PlayButton.Width = 20;
                PlayButton.Height = 20;
                DownloadButton.Width = 20;
                DownloadButton.Height = 20;
            }
            else if (SubLabel.Visibility == Visibility.Hidden)
            {
                SubLabel.Visibility = Visibility.Visible;
                SubLabel.Margin = new Thickness(30, 20, 40, 0);
                MainLabel.Content = OriginalStringsOfLabels[0];
                PlayButton.Width = 26;
                PlayButton.Height = 26;
                DownloadButton.Width = 26;
                DownloadButton.Height = 26;
            }
        }

        private static MenuItem GetMenu(string header, RoutedEventHandler onClick)
        {
            var menu = new MenuItem() { Header = header };
            menu.Click += onClick;
            return menu;
        }
        private static MenuItem GetMenu(string header, (string subItem, RoutedEventHandler onClick)[] subItems)
        {
            var output = new MenuItem() { Header = header };
            for (int i = 0; i < subItems.Length; i++)
                output.Items.Add(GetMenu(subItems[i].subItem, subItems[i].onClick));
            return output;
        }
        
        public void Revoke(string main, string sub, string time, MediaType type = MediaType.Music)
        {
            MainLabel.Content = main;
            SubLabel.Content = sub;
            switch (type)
            {
                case MediaType.Music: DefaultIcon = Glyph.MusicNote; break;
                case MediaType.Video: DefaultIcon = Glyph.Video; break;
                case MediaType.OnlineMusic:
                case MediaType.OnlineVideo:
                case MediaType.OnlineFile: DefaultIcon = Glyph.Cloud; break;
                default: DefaultIcon = default; break;
            }
            MainIcon.Glyph = DefaultIcon;
            OriginalStringsOfLabels = new string[] { main, sub };
            TimeLabel.Content = time;
            if ((int)type < 3)
                DownloadButton.Visibility = Visibility.Hidden;
            Resources["TimeLabelTargetMargin"] = new Thickness(0, 0, (int)type > 2 ? 75 : 35, 0);
            Size_Changed(this, null);
            IsPlaying = IsPlaying;
        }
        public void Revoke(Media media)
        {
            Media = media;
            Revoke(media.Title, media.Artist, CastTime(media.Length), media.MediaType);
        }
        public void Revoke(InfoExchangeArgs e) => Revoke(e.Object as Media);
        public void Revoke() => Revoke(new Media(Media.Path));

        private void MoveTo(string path)
        {
            File.Move(Media.Path, path);
            Media.Path = path;
        }

        public void RemoveRequest(object sender, RoutedEventArgs e)
        {
            RemoveRequested?.Invoke(this, null);
        }
        public void PropertiesRequest(object sender, RoutedEventArgs e)
        {
            PropertiesUI.OpenFor(Media, (s, f) => { if (!isPlaying) Revoke(f); else TagSaveRequested?.Invoke(this, new InfoExchangeArgs(f)); } );
        }
        public void LocationRequest(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "/select," + Media.Path);
        }
        public new MediaView MemberwiseClone() => MemberwiseClone() as MediaView;
        public void DeleteRequest(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show($"Sure? this file will be deleted:\r\n{Media}", " ", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (res == MessageBoxResult.OK)
            {
                File.Delete(Media.Path);
                RemoveRequested?.Invoke(this, null);
            }
        }

        public void UpdateLength(TimeSpan length) { }
    }
}