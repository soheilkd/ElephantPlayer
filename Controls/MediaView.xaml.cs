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
        public MenuItem[] OriginMenuItems;
        public event EventHandler<InfoExchangeArgs>
            DoubleClicked,
            PlayClicked,
            TagSaveRequested,
            RemoveRequested,
            PlayAfterRequested,
            RepeatRequested,
            ZipDownloaded;

        public Glyph ProperGlyph
        {
            get
            {
                switch (Media.Type)
                {
                    case MediaType.Music: return Glyph.MusicNote;
                    case MediaType.Video: return Glyph.Video;
                    default: return Controls.Glyph.Cloud;
                }
            }
        }
        private bool isPlaying = false;
        public bool IsPlaying
        {
            get => isPlaying;
            set
            {
                isPlaying = value;
                MainIcon.Glyph = value ? Glyph.Speakers : ProperGlyph;
                MainIcon.Foreground = value ? Brushes.DeepSkyBlue : Brushes.White;
                MainLabel.Foreground = value ? Brushes.DeepSkyBlue : Brushes.White;
                SubLabel.Foreground = value ? Brushes.DeepSkyBlue : Brushes.White;
            }
        }
        
        public MediaView(Media media)
        {
            InitializeComponent();
            Media = media;
            Sync();
        }
        
        public void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            PlayButton.Opacity = 0;
            DownloadButton.Opacity = 0;

            if (Media.Type == MediaType.None)
                RemoveRequested?.Invoke(this, null);
            RoutedEventHandler RepeatInvokation(int count) => (_, __) => RepeatRequested?.Invoke(this, new InfoExchangeArgs(count));
            //Load SubContextMenuItems
            if (ProperGlyph == Glyph.Cloud)
            {
                OriginMenuItems = new MenuItem[]
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
                    GetMenu("Remove", (_,__) => RemoveRequested?.Invoke(this, null))
                };
            }
            else
            {
                OriginMenuItems = new MenuItem[]
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
                     GetMenu("Remove", (_, __) => RemoveRequested?.Invoke(this, null)),
                     GetMenu("Delete", (_, __) =>{
                         var res = MessageBox.Show($"Sure? this file will be deleted:\r\n{Media}", " ", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                         if (res == MessageBoxResult.OK)
                         {
                             File.Delete(Media.Path);
                             RemoveRequested?.Invoke(this, null);
                         } }),
                     GetMenu("Move...",
                     new (string, RoutedEventHandler)[]
                     {
                         ("To Last Location", (_,__) => Media.MoveTo(App.Settings.LastPath)),
                         ("Browse...", (_,__) =>
                         {

                         })
                     }),
                     GetMenu("Copy...",
                     new (string, RoutedEventHandler)[]
                     {
                         ("To Last Location", (_,__) => Media.CopyTo(App.Settings.LastPath)),
                         ("Browse...", (_,__) =>
                         {

                         })
                     }),
                     GetMenu("Open Location", (_,__) => System.Diagnostics.Process.Start("explorer.exe", "/select," + Media.Path)),
                     GetMenu("Properties", (_, __) => { })
                    };
            }
            ContextMenu = new ContextMenu() { ItemsSource = OriginMenuItems };
        }

        private void Play_Clicked(object sender, MouseButtonEventArgs e) => PlayClicked?.Invoke(this, null);
        private void Canvas_DoubleClicked(object sender, MouseButtonEventArgs e) => DoubleClicked?.Invoke(this, null);

        private void Size_Changed(object sender, SizeChangedEventArgs e)
        {
            if (ActualWidth > 500)
            {
                SubLabel.Visibility = Visibility.Hidden;
                SubLabel.Margin = new Thickness(30, 0, 40, 0);
                MainLabel.Content = $"{Media.Artist} - {Media.Title}";
                PlayButton.Width = 20;
                PlayButton.Height = 20;
                DownloadButton.Width = 20;
                DownloadButton.Height = 20;
            }
            else if (SubLabel.Visibility == Visibility.Hidden)
            {
                SubLabel.Visibility = Visibility.Visible;
                SubLabel.Margin = new Thickness(30, 20, 40, 0);
                MainLabel.Content = Media.Title;
                PlayButton.Width = 26;
                PlayButton.Height = 26;
                DownloadButton.Width = 26;
                DownloadButton.Height = 26;
            }
        }
        
        public void Sync()
        {
            MainLabel.Content = Media.Title;
            SubLabel.Content = Media.Artist;
            MainIcon.Glyph = ProperGlyph;
            TimeLabel.Content = CastTime(Media.Length);
            if ((int)Media.Type < 3)
                DownloadButton.Visibility = Visibility.Hidden;
            Resources["TimeLabelTargetMargin"] = new Thickness(0, 0, Media.IsOffline ? 35 : 75, 0);
            Size_Changed(this, null);
            IsPlaying = IsPlaying;
        }

        public new MediaView MemberwiseClone() => MemberwiseClone() as MediaView;

        public void UpdateLength(TimeSpan length)
        {
            Media.Length = length;
            TimeLabel.Content = CastTime(length);
        }
        #region Online File Handling
        private WebClient Client = null;
        private bool downloadCanceled = true;
        
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
                        File.Delete(SavePath);
                        return;
                    }
                    MainLabel.Content = "Downloaded, Processing...";
                    DownloadButton.Visibility = Visibility.Hidden;
                    if (Media.Title.EndsWith(".zip"))
                    {
                        MainLabel.Content = "Extracting...";
                        SubLabel.Content = "";
                        using (var zip = new Ionic.Zip.ZipFile(SavePath))
                        {
                            var folderToExtract = SavePath.Substring(0, SavePath.IndexOf(".zip")) + "\\";
                            zip.ExtractAll(folderToExtract, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
                            ZipDownloaded?.Invoke(this, new InfoExchangeArgs(InfoType.MediaUpdate)
                            {
                                Object = Directory.GetFiles(folderToExtract, "*.*", SearchOption.AllDirectories),
                                Type = InfoType.StringArray
                            });
                        }
                        File.Delete(SavePath);
                    }
                    else
                    {
                        Media = Media.FromString(SavePath);
                        Sync();
                        UserControl_Loaded(this, null);
                    }
                };
                Client.DownloadFileAsync(new Uri(Media.Path, UriKind.Absolute), SavePath);
                downloadCanceled = false;
            }
        }


        #endregion Online File Handling
    }
}