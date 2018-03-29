using Player.Events;
using Player.Extensions;
using Player.Management;
using Player.Types;
using Player.User;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Routed = System.Windows.RoutedPropertyChangedEventArgs<double>;

namespace Player
{
    /// <summary>
    /// Interaction logic for MainUI.xaml
    /// </summary>
    public partial class MainUI : Window
    {
        enum Tabs {Minimal, NowPlaying, Songs, Explore, Meta, Vision, SpaceBuilder, Settings }
        Preferences P = Preferences.Load();
        MediaManager Manager = new MediaManager();
        List<MediaView> MediaViews = new List<MediaView>();
        Taskbar.Thumb Thumb = new Taskbar.Thumb();
        public MainUI()
        {
            InitializeComponent(); 
            Manager.Change += Manager_Change;
            App.NewInstanceRequested += App_NewInstanceRequested;

            Pref_BackOpt.IsChecked = P.BackgroundOptimization;
            Pref_Latency.IsChecked = P.HighLatency;
            Pref_IPC.IsChecked = P.IPC;
            Pref_MassLib.IsChecked = P.MassiveLibrary;
            Pref_DoubleValid.IsChecked = P.LibraryValidation;
            Pref_LightWeight.IsChecked = P.LightWeight;
            Pref_LyrInit.IsChecked = P.LyricsProvider;
            Pref_GC.IsChecked = P.ManualGarbageCollector;
            Pref_MetaInit.IsChecked = P.MetaInit;
            Pref_MediaRestr.IsChecked = P.Restrict;
            Pref_SideColl.IsChecked = P.SideBarColl;
            
            Pref_LiveStr.IsChecked = P.Stream;
            Pref_VisionOrient.IsChecked = P.VisionOrientation;
            Pref_VolBal.IsChecked = P.VolumeSetter;
            Pref_WM.IsChecked = P.WMDebug;
            Pref_DefaultPathBox.Text = P.DefaultPath;
        }

        private void App_NewInstanceRequested(object sender, InstanceEventArgs e)
        {
            Manager.Add(e.Args);
            Play(Manager.Next(Manager.Count - e.ArgsCount + 1));
            if (Manager.CurrentlyPlaying.IsVideo && P.VisionOrientation)
                OrinateVideoUI(true);
        }

        private async Task UserExperience()
        {
            UX:
            await Task.Delay(250);
            if (Player.NaturalDuration.HasTimeSpan)
                if (Player.NaturalDuration.TimeSpan != TimeSpan)
                {
                    //Update TimeSpan
                    TimeSpan = Player.NaturalDuration.TimeSpan;
                    PositionSlider.Maximum = TimeSpan.TotalMilliseconds;
                    PositionSlider.SmallChange = 1 * PositionSlider.Maximum / 100;
                    PositionSlider.LargeChange = 5 * PositionSlider.Maximum / 100;
                }
            PositionSlider.Value = Player.Position.TotalMilliseconds;
            NP_Label5.Content = $"{ConvertTime(Player.Position)}   -    {Manager.CurrentlyPlayingIndex + 1} \\ {Manager.Count} ";
            if (PositionSlider.Value >= PositionSlider.Maximum - 250)
                Play(Manager.Next());
               
            goto UX;
        }
        private async void Manager_Change(object sender, ManagementChangeEventArgs e)
        {
            switch (e.Change)
            {
                case ManagementChange.NewMedia:
                    MediaViews.Add(new MediaView(e.Changes.Index, e.Changes.Media.Title, e.Changes.Media.Artist, e.Changes.Media.MediaType));
                    QueueListView.Items.Add(MediaViews[MediaViews.Count - 1]);
                    MediaViews[MediaViews.Count - 1].DoubleClicked += MainUI_DoubleClicked;
                    break;
                case ManagementChange.EditingTag:
                    if (e.Changes.File.Name == Manager.CurrentlyPlaying.Path)
                    {
                        var pos = Player.Position;
                        Player.Stop();
                        Player.Source = null;
                        await Task.Delay(500);
                        e.Changes.File.Save();
                        Play(Manager[Manager.Find(e.Changes.File.Name)]);
                        Player.Position = pos;
                        Manager_Change(this, new ManagementChangeEventArgs()
                        {
                            Change = ManagementChange.MediaUpdate,
                            Changes = new MediaEventArgs()
                            {
                                Index = Manager.Find(e.Changes.File.Name),
                                Media = Manager[Manager.Find(e.Changes.File.Name)]
                            }
                        });
                    }
                    else
                        e.Changes.File.Save();
                    break;
                case ManagementChange.InterfaceUpdate:
                case ManagementChange.MediaUpdate:
                    for (int i = 0; i < MediaViews.Count; i++)
                        if (MediaViews[i].MediaIndex == e.Changes.Index)
                            MediaViews[i].Revoke(e.Changes.Index, e.Changes.Media.Title, e.Changes.Media.Artist);
                    MediaViews[MediaViews.FindIndex(item => item.MediaIndex == e.Changes.Index)].Revoke(e.Changes);

                    break;
                case ManagementChange.Crash:
                    break;
                case ManagementChange.PopupRequest:
                    break;
                case ManagementChange.ArtworkClick:
                    break;
                case ManagementChange.SomethingHappened:
                    break;
                default:
                    break;
            }
        }

        private void MainUI_DoubleClicked(object sender, MediaEventArgs e)
        {
            Play(Manager.Next(e.Index));
        }
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Left = P.LastLoc.X;
            Top = P.LastLoc.Y;
            Width = P.LastSize.Width;
            Height = P.LastSize.Height;
            if (Environment.GetCommandLineArgs().Length != 0)
                if (Manager.Add(Environment.GetCommandLineArgs()))
                {
                    Play(Manager.Next(Manager.Count - 1));
                    if (Manager.CurrentlyPlaying.IsVideo)
                        OrinateVideoUI(true);
                }
            DraggerTimer.Elapsed += DraggerTimer_Elapsed;
            UserExperience();
            Grid_MouseUp(this, null);
            TaskbarItemInfo = Thumb.Info;
            Thumb.NextPressed += (obj, f) => NextButton_Click(obj, f);
            Thumb.PausePressed += (obj, f) => PlayPauseButtonClick(obj, f);
            Thumb.PlayPressed += (obj, f) => PlayPauseButtonClick(obj, f);
            Thumb.PrevPressed += (obj, f) => PreviousButton_Click(obj, f);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            P.LastSize = new Size()
            {
                Height = Height,
                Width = Width
            };
            P.LastLoc = new Point()
            {
                X = Left,
                Y = Top
            };
            P.Save();
            Application.Current.Shutdown();
            
        }

        private void Play(Media media)
        {
            if (P.MetaInit)
                Meta_LoadClick(this, null);
            if (P.LyricsProvider)
                Lyrics_LoadClick(this, null);
            PositionSlider.Value = 0;
            PlayPauseButton.Icon = MaterialIcons.MaterialIconType.ic_pause;
            Player.Source = new Uri(media.Path);
            Player.Play();
            Title = "Player - " + media.Name;
            NP_ArtworkImage.Source = media.Artwork;
            NP_Label1.Content = media.Title;
            NP_Label2.Content = media.Artist;
            NP_Label3.Content = media.Name;
            NP_Label4.Content = media.MediaType.ToString();
            NP_Label5.Content = "0:00";
            for (int i = 0; i < MediaViews.Count; i++)
                MediaViews[i].IsPlaying = false;
            MediaViews[MediaViews.FindIndex(item => item.MediaIndex == Manager.CurrentlyPlayingIndex)].IsPlaying = true;

        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            Manager.Add((string[])e.Data.GetData(DataFormats.FileDrop));
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TabsSpaceBuilderGrid.Width = Width > 365 ? Width - 365 : 1;
        }
        
        private void PlayPauseButtonClick(object sender, EventArgs e)
        {
            if (PlayPauseButton.Icon == MaterialIcons.MaterialIconType.ic_pause)
            {
                Player.Pause();
                PlayPauseButton.Icon = MaterialIcons.MaterialIconType.ic_play_arrow;
                Thumb.Refresh(false);
            }
            else
            {
                Player.Play();
                PlayPauseButton.Icon = MaterialIcons.MaterialIconType.ic_pause;
                Thumb.Refresh(true);
            }
        }
        
        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }
        private string ConvertTime(TimeSpan time)
        {
            TimeConvertion.mins = (time.TotalSeconds - (time.TotalSeconds % 60)).ToInt() / 60;
            TimeConvertion.secs = time.TotalSeconds.ToInt() % 60;
            return $"{TimeConvertion.mins}:{(TimeConvertion.secs.ToString().Length == 1 ? $"0{TimeConvertion.secs}" : TimeConvertion.secs.ToString())}";
        }
        private void ForcePositionChange(double ms, bool Seek = false)
        {
            UserChangingPosition = true;
            if (Seek) PositionSlider.Value = ms;
            else PositionSlider.Value += ms;
            UserChangingPosition = false;
        }
        async void PositionMouseDown(object sender, MouseButtonEventArgs e)
        {
            UserChangingPosition = true;
            while (e.ButtonState == MouseButtonState.Pressed)
            {
                Player.Pause();
                await Task.Delay(300);
                Player.Play();
                await Task.Delay(50);
            }
            PlayPauseButton.Icon = MaterialIcons.MaterialIconType.ic_pause;
            Player.Play();
            UserChangingPosition = false;
        }
        void PositionChanged(object sender, Routed e)
        {
            if (UserChangingPosition) Player.Position = new TimeSpan(0, 0, 0, 0, PositionSlider.Value.ToInt());
        }
        private (int mins, int secs) TimeConvertion;

        #region VideoUI

        private double LastWidth, LastHeight, LastLeft;
        private System.Timers.Timer DraggerTimer = new System.Timers.Timer(250) { AutoReset = false };
        private double LastTop;
        private bool FullScreen = false;
        private TimeSpan TimeSpan;
        private Boolean UserChangingPosition = false;
        private WindowState _State;

        private void DraggerTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            DraggerTimer.Stop();
            DraggerTimer.Enabled = false;
        }

        public void OrinateVideoUI(bool Enabled)
        {
            MainTabControl.Margin = Enabled ? new Thickness(0) : new Thickness(0, 0, 0, 18);
            Topmost = Enabled;
            WindowStyle = Enabled ? WindowStyle.None : WindowStyle.ThreeDBorderWindow;
            foreach (TabItem item in MainTabControl.Items)
                    item.Style = (Style)Resources[Enabled ? "InvisibleTabStyle": "DefaultTabStyle"];
            if (Enabled && MainTabControl.SelectedIndex != (int)Tabs.Vision)
                MainTabControl.SelectedIndex = (int)Tabs.Vision;
            PositionSlider.Visibility = Enabled ? Visibility.Hidden : Visibility.Visible;
        }
        private void Player_MouseUp(object sender, MouseButtonEventArgs e) { }
        private void Player_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DraggerTimer.Enabled = true;
            DraggerTimer.Start();
            try
            {
                if (!FullScreen) DragMove();
                if (DraggerTimer.Enabled)
                {
                    VideoOriented = !VideoOriented;
                    OrinateVideoUI(VideoOriented);
                }
            }
            catch (Exception)
            {
            }
        }
        bool VideoOriented = false;

        private void PlayerCFullScreen(object sender, RoutedEventArgs e)
        {
            if (Height != Screen.FullHeight)
            {
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                FullScreen = true;
                LastWidth = Width;
                LastHeight = Height;
                LastLeft = Left;
                LastTop = Top;
                Width = Screen.FullWidth;
                Height = Screen.FullHeight;
                Left = 0;
                Top = 0;
                _State = WindowState;
                WindowState = WindowState.Normal;
            }
            else
            {
                FullScreen = false;
                ResizeMode = ResizeMode.CanResize;
                WindowState = _State;
                WindowStyle = WindowStyle.ThreeDBorderWindow;
                Width = LastWidth;
                Height = LastHeight;
                Left = LastLeft;
                Top = LastTop;
            }
            PlayerC_FullScreen.Header = FullScreen ? "Exit Full Screen" : "Enter Full Screen";
        }
       private void PlayerCSubtitle(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        #region MetaUI

        TagLib.File Meta_CurrentFile;
        bool IsLoadingMedia = false;
        private void Meta_LoadClick(object sender, EventArgs e)
        {
            try
            {
                //var pos = Player.Position;
                //Player.Stop();
                //Player.Source = null;
                Meta_CurrentFile = TagLib.File.Create(Manager.CurrentlyPlaying.Path, TagLib.ReadStyle.None);
                Meta_AlbumArtistBox.Text = Meta_CurrentFile.Tag.FirstAlbumArtist;
                Meta_AlbumBox.Text = Meta_CurrentFile.Tag.Album;
                Meta_ArtistBox.Text = Meta_CurrentFile.Tag.FirstPerformer;
                Meta_ArtworkImage.Source = Manager.CurrentlyPlaying.Artwork;
                Meta_CommentBox.Text = Meta_CurrentFile.Tag.Comment;
                Meta_ComposerBox.Text = Meta_CurrentFile.Tag.FirstComposer;
                Meta_ConductorBox.Text = Meta_CurrentFile.Tag.Conductor;
                Meta_CopyrightBox.Text = Meta_CurrentFile.Tag.Copyright;
                Meta_GenreBox.Text = Meta_CurrentFile.Tag.FirstGenre;
                Meta_TitleBox.Text = Meta_CurrentFile.Tag.Title;
                Meta_TrackBox.Text = Meta_CurrentFile.Tag.Track.ToString();
                Meta_YearBox.Text = Meta_CurrentFile.Tag.Year.ToString();

            }
            catch(Exception ex)
            {
                Debug.Display(ex.Message, ex.Source);
                throw;
            }
        }
        private void Meta_SaveClick(object sender, EventArgs e)
        {
            try
            {
                Meta_CurrentFile.Tag.AlbumArtists = new string[] { Meta_AlbumArtistBox.Text };
                Meta_CurrentFile.Tag.Album = Meta_AlbumBox.Text;
                Meta_CurrentFile.Tag.Performers = new string[] { Meta_ArtistBox.Text };
                Meta_CurrentFile.Tag.Pictures = new TagLib.IPicture[] { };
                Meta_CurrentFile.Tag.Comment = Meta_CommentBox.Text;
                Meta_CurrentFile.Tag.Composers = new string[] { Meta_ComposerBox.Text };
                Meta_CurrentFile.Tag.Copyright = Meta_CopyrightBox.Text;
                Meta_CurrentFile.Tag.Genres = Meta_GenreBox.Text.Split(',');
                Meta_CurrentFile.Tag.Title = Meta_TitleBox.Text;
                Meta_CurrentFile.Tag.Track = UInt32.Parse(Meta_TrackBox.Text);
                Meta_CurrentFile.Tag.Year = UInt32.Parse(Meta_YearBox.Text);
                Manager_Change(this, new ManagementChangeEventArgs()
                {
                    Change = ManagementChange.EditingTag,
                    Changes = new MediaEventArgs()
                    {
                        File = Meta_CurrentFile
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.Display(ex.Message, ex.Source);
            }
        }

        #endregion

        private void NextButton_Click(object sender, EventArgs e) => Play(Manager.Next());

        private void PreviousButton_Click(object sender, EventArgs e)
        {
            if (PositionSlider.Value > PositionSlider.Maximum / 100 * 10)
                ForcePositionChange(0, true);
            else
                Play(Manager.Previous());
        }
        #region LyricsUI
        private void Lyrics_LoadClick(object sender, EventArgs e) => Lyrics_FloatBox.Text = Manager.CurrentlyPlaying.Lyrics;
        #endregion

        async void window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    while(e.KeyStates == KeyStates.Down)
                    {
                        ForcePositionChange(PositionSlider.SmallChange * -1);
                        await Task.Delay(200);
                    }
                    break;
                case Key.Right:
                    while(e.KeyStates == KeyStates.Down)
                    {
                        ForcePositionChange(PositionSlider.SmallChange);
                        await Task.Delay(200);
                    }
                    break;
                case Key.Up:
                    while (e.KeyStates == KeyStates.Down)
                    {
                        if (Player.Volume >= 1)
                            break;
                        Player.Volume += 0.01;
                        await Task.Delay(50);
                    }
                    break;
                case Key.Down:
                    while (e.KeyStates == KeyStates.Down)
                    {
                        if (Player.Volume <= 0)
                            break;
                        Player.Volume -= 0.01;
                        await Task.Delay(50);
                    }
                    break;
                case Key.MediaNextTrack:
                    NextButton_Click(this, null);
                    break;
                case Key.MediaPreviousTrack:
                    PreviousButton_Click(this, null);
                    break;
                case Key.MediaPlayPause:
                    PlayPauseButtonClick(this, null);
                    break;
                case Key.MediaStop:
                    break;
                default:
                    break;
            }
        }

        private void CopyButton_Click(object sender, EventArgs e)
        {
            try
            {
                System.IO.File.Copy(Manager.CurrentlyPlaying.Path, P.DefaultPath + Manager.CurrentlyPlaying.Name);
            }
            catch(Exception f)
            {
                System.Windows.MessageBox.Show(f.Message);
            }
        }

        private void window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    PlayPauseButtonClick(this, null);
                    break;
                default:
                    break;
            }
        }
        private void AnySettingChanged(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
                return;
            P.BackgroundOptimization = Pref_BackOpt.IsChecked.Value;
            P.HighLatency = Pref_Latency.IsChecked.Value;
            P.IPC = Pref_IPC.IsChecked.Value;
            P.LibraryValidation = Pref_DoubleValid.IsChecked.Value;
            P.LightWeight = Pref_LightWeight.IsChecked.Value;
            P.LyricsProvider = Pref_LyrInit.IsChecked.Value;
            P.ManualGarbageCollector = Pref_GC.IsChecked.Value;
            P.MassiveLibrary = Pref_MassLib.IsChecked.Value;
            P.MetaInit = Pref_MetaInit.IsChecked.Value;
            P.Restrict = Pref_MediaRestr.IsChecked.Value;
            P.SideBarColl = Pref_SideColl.IsChecked.Value;
            P.Stream = Pref_LiveStr.IsChecked.Value;
            P.VisionOrientation = Pref_VisionOrient.IsChecked.Value;
            P.VolumeSetter = Pref_VolBal.IsChecked.Value;
            P.WMDebug = Pref_WM.IsChecked.Value;
            P.DefaultPath = Pref_DefaultPathBox.Text;
            P.Save();
        }
    }
}
