using Player.Controls;
using Player.Events;
using Player.Extensions;
using Player.Management;
using Player.User;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Routed = System.Windows.RoutedPropertyChangedEventArgs<double>;
using Forms = System.Windows.Forms;
using System.Timers;

namespace Player
{
    /// <summary>
    /// Interaction logic for MainUI.xaml
    /// </summary>
    public partial class MainUI : Window
    {
        Preferences P = Preferences.Load();
        MediaManager Manager = new MediaManager();
        List<MediaView> MediaViews = new List<MediaView>();
        Taskbar.Thumb Thumb = new Taskbar.Thumb();
        Boolean IsVisionOn;
        Timer PlayCountTimer = new Timer(100000) { AutoReset = false };
        new string Title
        {
            get => (string)Resources["Res_Title"];
            set => Resources["Res_Title"] = value;
        }

        public MainUI()
        {
            InitializeComponent(); 
            Manager.Change += Manager_Change;
            App.NewInstanceRequested += App_NewInstanceRequested;
            
            Pref_Latency.IsChecked = P.HighLatency;
            Pref_IPC.IsChecked = P.IPC;
            Pref_MassLib.IsChecked = P.MassiveLibrary;
            Pref_DoubleValid.IsChecked = P.LibraryValidation;
            Pref_LightWeight.IsChecked = P.LightWeight;
            Pref_GC.IsChecked = P.ManualGarbageCollector;
            Pref_WM.IsChecked = P.WMDebug;

        }

        private void App_NewInstanceRequested(object sender, InstanceEventArgs e)
        {
            Manager.Add(e.Args, true);
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
                    TimeLabel_Full.Content = ConvertTime(TimeSpan);
                    PlayCountTimer.Stop();
                    PlayCountTimer.Interval = PositionSlider.Maximum / 3;
                    PlayCountTimer.Start();
                }
            PositionSlider.Value = Player.Position.TotalMilliseconds;
            TimeLabel_Current.Content = ConvertTime(Player.Position);
            goto UX;
        }
        private async void Manager_Change(object sender, ManagementChangeEventArgs e)
        {
            switch (e.Change)
            {
                case ManagementChange.NewMedia:
                   MediaViews.Add(new MediaView(e.Changes.Index, Manager[e.Changes.Index].Title, Manager[e.Changes.Index].Artist, Manager[e.Changes.Index].MediaType));
                    var p = MediaViews.Count - 1;
                    QueueListView.Items.Add(MediaViews[p]);
                    MediaViews[p].DoubleClicked += MainUI_DoubleClicked;
                    MediaViews[p].PlayClicked += (n, f) => Play(Manager.Next(f.Index));
                    MediaViews[p].DeleteRequested += (n, f) => Manager.RequestDelete(f.Index);
                    MediaViews[p].LocationRequested += (n, f) => Manager.RequestLocation(f.Index);
                    MediaViews[p].RemoveRequested += (n, f) => Manager.Remove(f.Index);
                    MediaViews[p].PropertiesRequested += (n, f) => Manager.ShowProperties(f.Index);
                    MediaViews[p].RepeatRequested += (n, f) => Manager.Repeat(f.Index, f.Para);
                    MediaViews[p].DownloadRequested += (n, f) => f.Sender.Download(Manager[f.Index]);
                    MediaViews[p].Downloaded += (n, f) => Manager[f.Index] = f.Media;
                    MediaViews[p].ZipDownloaded += (n, f) =>
                    {
                        Manager.Add((string[])f.ObjectArray);
                        Manager.Remove(((MediaView)n).MediaIndex);
                    };
                    Window_SizeChanged(this, null);
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
                    }
                    else
                        e.Changes.File.Save();
                    break;
                case ManagementChange.MediaRemoved:
                    int index = MediaViews.FindIndex(item => item.MediaIndex == e.Changes.Index);
                    MediaViews.RemoveAt(index);

                    QueueListView.Items.Clear();
                    for (int i = 0; i < MediaViews.Count; i++)
                    {
                        QueueListView.Items.Add(MediaViews[i]);
                    }
                    break;
                case ManagementChange.MediaRequested:
                    Play(Manager.Next(e.Changes.Index));
                    break;
                case ManagementChange.MediaUpdate:
                    for (int i = 0; i < MediaViews.Count; i++)
                        if (MediaViews[i].MediaIndex == e.Changes.Index)
                            MediaViews[i].Revoke(e.Changes.Index, e.Changes.Media.Title, e.Changes.Media.Artist);
                    MediaViews[MediaViews.FindIndex(item => item.MediaIndex == e.Changes.Index)].Revoke(e.Changes);
                    if (e.Changes.Index == Manager.CurrentlyPlayingIndex)
                    {
                        var q = Player.Position;
                        Play(e.Changes.Media);
                        ForcePositionChange(q.TotalMilliseconds, true);
                    }
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var lib = MassiveLibrary.Load();
            for (int i = 0; i < lib.Medias.Length; i++)
            {
                Manager.Add(lib.Medias[i]);
            }
            Left = P.LastLoc.X;
            Top = P.LastLoc.Y;
            Width = P.LastSize.Width;
            Height = P.LastSize.Height;
            var cml = Environment.GetCommandLineArgs();
            if (cml.Length > 1)
            {
                Manager.Add(cml, true);
            }
            DraggerTimer.Elapsed += DraggerTimer_Elapsed;
            TaskbarItemInfo = Thumb.Info;
            Thumb.NextPressed += (obj, f) => NextButton_Click(obj, f);
            Thumb.PausePressed += (obj, f) => PlayPauseButtonClick(obj, f);
            Thumb.PlayPressed += (obj, f) => PlayPauseButtonClick(obj, f);
            Thumb.PrevPressed += (obj, f) => PreviousButton_Click(obj, f);

            User.Keyboard.KeyDown += Keyboard_KeyDown;
            User.Keyboard.KeyUp += Keyboard_KeyUp;
            UserExperience();
            PlayCountTimer.Elapsed += PlayCountTimer_Elapsed;
            AddUrlButton.Click += delegate
            {
                UrlPopup.IsOpen = true;
                UrlTextBox.Text = Clipboard.GetText() ?? "http://URL";
                UrlTextBox.Focus();
               
            };
        }

        private void Downloader_DownloadingDone(object sender, MediaEventArgs e)
        {
            Manager[e.Index] = e.Media;
            if (Manager.CurrentlyPlayingIndex == e.Index)
            {
                var pos = Player.Position;
                Play(e.Media);
                ForcePositionChange(Player.Position.TotalMilliseconds, true);
            }
        }

        private void PlayCountTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Manager.AddCount();
        }

        MassiveLibrary Library = new MassiveLibrary();
        private void Keyboard_KeyUp(object sender, Forms::KeyEventArgs e)
        {
            
        }

        private async void Keyboard_KeyDown(object sender, Forms::KeyEventArgs e)
        {
            if (IsFocused || e.Alt)
            {
                switch (e.KeyCode)
                {
                    case Forms::Keys.Left:
                        ForcePositionChange(PositionSlider.SmallChange * -1);
                        await Task.Delay(200);
                        break;
                    case Forms::Keys.Right:
                        ForcePositionChange(PositionSlider.SmallChange);
                        await Task.Delay(200);
                        break;
                    case Forms::Keys.Up:
                        if (Player.Volume >= 1)
                            break;
                        Player.Volume += 0.01;
                        await Task.Delay(50);
                        break;
                    case Forms::Keys.Down:
                        if (Player.Volume <= 0)
                            break;
                        Player.Volume -= 0.01;
                        await Task.Delay(50);
                        break;
                    default: break;
                }
            }
            switch (e.KeyCode)
            {
                case Forms::Keys.MediaNextTrack:
                    NextButton_Click(this, null);
                    break;
                case Forms::Keys.MediaPreviousTrack:
                    PreviousButton_Click(this, null);
                    break;
                case Forms::Keys.MediaPlayPause:
                    PlayPauseButtonClick(this, null);
                    break;
                case Forms::Keys.MediaStop:
                    break;
                default:
                    break;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            P.LastSize = new Size()
            {
                Width = Width,
                Height = Height
            };
            P.LastLoc = new Point()
            {
                X = Left,
                Y = Top
            };
            P.Save();
            Manager.Close();
            Application.Current.Shutdown();
        }

        private void Play(Media media)
        {
            PositionSlider.Value = 0;
            PlayPauseButton.Icon = IconType.pause;
            Player.Source = new Uri(media.Path);
            Player.Play();
            MiniArtworkImage.Source = media.Artwork;
            Title = media.Title;
            for (int i = 0; i < MediaViews.Count; i++)
                MediaViews[i].IsPlaying = false;
            int index = MediaViews.FindIndex(item => item.MediaIndex == Manager.CurrentlyPlayingIndex);
            MediaViews[index].IsPlaying = true; 

        }

        private void Window_Drop(object sender, DragEventArgs e) => Manager.Add((string[])e.Data.GetData(DataFormats.FileDrop));

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            for (int i = 0; i < MediaViews.Count; i++)
                MediaViews[i].Width = QueueListView.ActualWidth > 25 ? QueueListView.ActualWidth - 25 : 25;
            Resources["Height"] = Height;
            Resources["Width"] = Width;
            if (IsVisionOn)
            {
                Player.Width = Width;
                Player.Height = Height;
            }
        }
        
        private void PlayPauseButtonClick(object sender, EventArgs e)
        {
            if (PlayPauseButton.Icon == IconType.pause)
            {
                Player.Pause();
                PlayPauseButton.Icon = IconType.play_arrow;
                Thumb.Refresh(false);
            }
            else
            {
                Player.Play();
                PlayPauseButton.Icon = IconType.pause;
                Thumb.Refresh(true);
            }
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
        private async void PositionMouseDown(object sender, MouseButtonEventArgs e)
        {
            UserChangingPosition = true;
            while (e.ButtonState == MouseButtonState.Pressed)
            {
                Player.Pause();
                await Task.Delay(300);
                Player.Play();
                await Task.Delay(50);
            }
            PlayPauseButton.Icon = IconType.pause;
            Player.Play();
            UserChangingPosition = false;
        }
        private void PositionChanged(object sender, Routed e)
        {
            if (UserChangingPosition)
            {
                Player.Position = new TimeSpan(0, 0, 0, 0, PositionSlider.Value.ToInt());
                PositionSlider.Value = ((Slider)sender).Value;
            }
        }
        private (int mins, int secs) TimeConvertion;
        #region VideoUI

        private double LastWidth, LastHeight, LastLeft;
        private Timer DraggerTimer = new Timer(250) { AutoReset = false };
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

        private void NextButton_Click(object sender, EventArgs e) => Play(Manager.Next());

        private void PreviousButton_Click(object sender, EventArgs e)
        {
            if (PositionSlider.Value > PositionSlider.Maximum / 100 * 10)
                ForcePositionChange(0, true);
            else
                Play(Manager.Previous());
        }
        
        private void Position_RepeatBackwardClick(object sender, RoutedEventArgs e) => PositionSlider.Value -= PositionSlider.LargeChange;
        private void Position_RepeatForwardClick(object sender, RoutedEventArgs e) => PositionSlider.Value += PositionSlider.LargeChange;

        private void VisionButtonClick(object sender, EventArgs e)
        {
            IsVisionOn = !IsVisionOn;
            VisionButton.Icon = IsVisionOn ? IconType.expand_less : IconType.ondemand_video;
            Player.BeginStoryboard(Player.Resources[IsVisionOn ? "VisionOnBoard": "VisionOffBoard"] as Storyboard);
        }

        private void Player_MediaEnded(object sender, RoutedEventArgs e)
        {
            Play(Manager.Next());
        }

        private void PlayModeButton_Click(object sender, EventArgs e)
        {
            switch (Manager.ActivePlayMode)
            {
                case PlayMode.Shuffle:
                    PlayModeButton.Icon = IconType.repeat_one;
                    Manager.ActivePlayMode = PlayMode.RepeatOne;
                    break;
                case PlayMode.RepeatOne:
                    PlayModeButton.Icon = IconType.repeat;
                    Manager.ActivePlayMode = PlayMode.RepeatAll;
                    break;
                case PlayMode.RepeatAll:
                    PlayModeButton.Icon = IconType.queue_music;
                    Manager.ActivePlayMode = PlayMode.Queue;
                    break;
                case PlayMode.Queue:
                    PlayModeButton.Icon = IconType.shuffle;
                    Manager.ActivePlayMode = PlayMode.Shuffle;
                    break;
                default:
                    break;
            }
            P.PlayMode = (int)Manager.ActivePlayMode;
        }
        
        private void UrlTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Manager.Add(new Uri(UrlTextBox.Text, UriKind.Absolute));
                QueueListView.ScrollIntoView(QueueListView.Items[QueueListView.Items.Count - 1]);
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
            P.HighLatency = Pref_Latency.IsChecked.Value;
            P.IPC = Pref_IPC.IsChecked.Value;
            P.LibraryValidation = Pref_DoubleValid.IsChecked.Value;
            P.LightWeight = Pref_LightWeight.IsChecked.Value;
            P.ManualGarbageCollector = Pref_GC.IsChecked.Value;
            P.MassiveLibrary = Pref_MassLib.IsChecked.Value;
            P.WMDebug = Pref_WM.IsChecked.Value;
            P.Save();
        }
    }
}
