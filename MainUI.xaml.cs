using Player.Controls;
using Player.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Linq;
using Forms = System.Windows.Forms;
using Routed = System.Windows.RoutedPropertyChangedEventArgs<double>;
namespace Player
{
    public partial class MainUI : Window
    {
        private MediaManager Manager = new MediaManager();
        private List<MediaView> MediaViews = new List<MediaView>();
        private MassiveLibrary Library = new MassiveLibrary();
        private Timer PlayCountTimer = new Timer(100000) { AutoReset = false };
        private Timer DraggerTimer = new Timer(250) { AutoReset = false };
        private Timer MouseMoveTimer = new Timer(5000);
        private Timer SizeChangeTimer = new Timer(50) { AutoReset = true };
        private TimeSpan TimeSpan;
        private Taskbar.Thumb Thumb = new Taskbar.Thumb();
        private bool IsUserSeeking = false;
        private bool[] IsVisionOn = new bool[] { false, false, false };
        private Size[] WindowSizes = new Size[2];
        private Storyboard WindowWidthBoard => Resources["WindowWidthBoard"] as Storyboard;
        private bool IsMouseOnControls
        {
            get
            {
                return PreviousButton.IsMouseOver
                    || PlayPauseButton.IsMouseOver
                    || NextButton.IsMouseOver
                    || PositionSlider.IsMouseOver
                    || VisionButton.IsMouseOver;
            }
        }
        public MainUI()
        {
            InitializeComponent();
            Manager.Change += Manager_Change;
            App.NewInstanceRequested += (_, e) => Manager.Add(e.Args);

            Pref_DoubleValid.IsChecked = App.Preferences.LibraryValidation;
            var lib = MassiveLibrary.Load();
            for (int i = 0; i < lib.Medias.Length; i++)
                Manager.Add(lib.Medias[i]);
            WindowSizes = App.Preferences.LastSize;
            Width = WindowSizes[0].Width;
            Height = WindowSizes[0].Height;
            Left = App.Preferences.LastLoc.X;
            Top = App.Preferences.LastLoc.Y;
            Player.Volume = 1;
            ProcessVolume();
        }
        private void BindUI()
        {
            TaskbarItemInfo = Thumb.Info;
            Thumb.NextPressed += (obj, f) => NextButton_Click(obj, null);
            Thumb.PausePressed += (obj, f) => PlayPauseButton_Click(obj, null);
            Thumb.PlayPressed += (obj, f) => PlayPauseButton_Click(obj, null);
            Thumb.PrevPressed += (obj, f) => PreviousButton_Click(obj, null);
            PlayCountTimer.Elapsed += (_, __) => Manager.AddCount();
            (Resources["VisionOnBoard"] as Storyboard).CurrentStateInvalidated += (_, __) => QueueListView.Visibility = Visibility.Hidden;
            (Resources["VisionOffBoard"] as Storyboard).Completed += (_, __) => QueueListView.Visibility = Visibility.Visible;
            (Resources["SettingsOnBoard"] as Storyboard).CurrentStateInvalidated += (_, __) => SettingsGrid.Visibility = Visibility.Visible;
            (Resources["SettingsOffBoard"] as Storyboard).Completed += (_, __) => SettingsGrid.Visibility = Visibility.Hidden;
            SettingsGrid.Visibility = Visibility.Hidden;
            MouseMoveTimer.Elapsed += delegate
            {
                Dispatcher.Invoke(() =>
                {
                    if (IsMouseOnControls || !IsVisionOn[0])
                        return;
                    OrinateFullVision(true);
                });
            };
            SizeChangeTimer.Elapsed += delegate
            {
                Dispatcher.Invoke(
                    delegate
                    {
                        if (!QueueListView.IsVisible)
                            return;
                        for (int i = 0; i < MediaViews.Count; i++)
                            MediaViews[i].Width = QueueListView.ActualWidth > 25 ? QueueListView.ActualWidth - 25 : 25;
                    });
                SizeChangeTimer.Stop();
            };
            User.Keyboard.Events.KeyDown += Keyboard_KeyDown;
            Player.MediaEnded += (_, __) => Play(Manager.Next());
            switch ((PlayMode)App.Preferences.PlayMode)
            {
                case PlayMode.Shuffle: PlayModeButton.Icon = IconType.Shuffle; break;
                case PlayMode.RepeatOne: PlayModeButton.Icon = IconType.RepeatOne; break;
                case PlayMode.RepeatAll: PlayModeButton.Icon = IconType.Repeat; break;
                case PlayMode.Queue: PlayModeButton.Icon = IconType.MusicQueue; break;
                default: PlayModeButton.Icon = IconType.Repeat; break;
            }

            Settings_AncestorCombo.SelectedIndex = App.Preferences.MainKey;
            Settings_OrinateCheck.IsChecked = App.Preferences.VisionOrientation;
            Settings_TimeoutCombo.SelectedIndex = App.Preferences.MouseOverTimeout;
            Settings_AncestorCombo.SelectionChanged += (_, __) => App.Preferences.MainKey = Settings_AncestorCombo.SelectedIndex;
            Settings_TimeoutCombo.SelectionChanged += delegate
            {
                App.Preferences.MouseOverTimeout = Settings_TimeoutCombo.SelectedIndex;
                switch (App.Preferences.MouseOverTimeout)
                {
                    case 0: MouseMoveTimer.Interval = 500; break;
                    case 1: MouseMoveTimer.Interval = 1000; break;
                    case 2: MouseMoveTimer.Interval = 2000; break;
                    case 3: MouseMoveTimer.Interval = 3000; break;
                    case 4: MouseMoveTimer.Interval = 4000; break;
                    case 5: MouseMoveTimer.Interval = 5000; break;
                    case 6: MouseMoveTimer.Interval = 10000; break;
                    case 7: MouseMoveTimer.Interval = 60000; break;
                    default: MouseMoveTimer.Interval = 5000; break;
                }

            };
            Settings_OrinateCheck.Checked += (_, __) => App.Preferences.VisionOrientation = Settings_OrinateCheck.IsChecked.Value;
            Settings_OrinateCheck.Unchecked += (_, __) => App.Preferences.VisionOrientation = Settings_OrinateCheck.IsChecked.Value;
            switch (App.Preferences.MouseOverTimeout)
            {
                case 0: MouseMoveTimer.Interval = 500; break;
                case 1: MouseMoveTimer.Interval = 1000; break;
                case 2: MouseMoveTimer.Interval = 2000; break;
                case 3: MouseMoveTimer.Interval = 3000; break;
                case 4: MouseMoveTimer.Interval = 4000; break;
                case 5: MouseMoveTimer.Interval = 5000; break;
                case 6: MouseMoveTimer.Interval = 10000; break;
                case 7: MouseMoveTimer.Interval = 60000; break;
                default: MouseMoveTimer.Interval = 5000; break;
            }
            SettingsButton.MouseUp += delegate
            {
                if (!IsVisionOn[2])
                {
                    (Resources["SettingsOnBoard"] as Storyboard).Begin();
                }
                else
                {
                    (Resources["SettingsOffBoard"] as Storyboard).Begin();
                }
                IsVisionOn[2] = !IsVisionOn[2];
            };
            RunUX();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BindUI();
            var cml = Environment.GetCommandLineArgs();
            if (cml.Length > 1)
                Manager.Add(cml, true);

        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WindowSizes[IsVisionOn[0] ? 1: 0].Width = ActualWidth;
            App.Preferences.LastSize = WindowSizes;
            App.Preferences.LastLoc = new Point(Left, Top);
            App.Preferences.Volume = Player.Volume;
            App.Preferences.Save();
            Manager.Close();
            Application.Current.Shutdown();
        }
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    PlayPauseButton_Click(this, null);
                    break;
                default: break;
            }
        }
        private void Window_Drop(object sender, DragEventArgs e) => Manager.Add((string[])e.Data.GetData(DataFormats.FileDrop));
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            WindowSizes[IsVisionOn[0] ? 1 : 0].Height = Height;
            SizeChangeTimer.Start();
        }

        private async void Manager_Change(object sender, InfoExchangeArgs e)
        {
            switch (e.Type)
            {
                case InfoType.NewMedia:
                    MediaViews.Add(new MediaView(e.Integer, Manager[e.Integer]));
                    var p = MediaViews.Count - 1;
                    QueueListView.Items.Add(MediaViews[p]);
                    MediaViews[p].DoubleClicked += (n, f) => Play(Manager.Next(f.Integer));
                    MediaViews[p].PlayClicked += (n, f) => Play(Manager.Next(f.Integer));
                    MediaViews[p].DeleteRequested += (n, f) => Manager.RequestDelete(f.Integer);
                    MediaViews[p].LocationRequested += (n, f) => Manager.RequestLocation(f.Integer);
                    MediaViews[p].RemoveRequested += (n, f) => Manager.Remove(f.Integer);
                    MediaViews[p].PropertiesRequested += (n, f) => Manager.ShowProperties(f.Integer);
                    MediaViews[p].RepeatRequested += (n, f) => Manager.Repeat(f.Integer, (int)f.Object);
                    MediaViews[p].DownloadRequested += (n, f) => (f.Object as MediaView).Download(Manager[f.Integer]);
                    MediaViews[p].Downloaded += (n, f) =>
                    {
                        Manager[f.Integer] = f.Object as Media;
                        if (Manager.CurrentlyPlayingIndex == e.Integer)
                        {
                            var posit = Player.Position;
                            Play(Manager[f.Integer]);
                            ChangePosition(posit.TotalMilliseconds, true);
                        }
                    };
                    MediaViews[p].ZipDownloaded += (n, f) =>
                    {
                        Manager.Add((string[])f.Object);
                        Manager.Remove(((MediaView)n).MediaIndex);
                    };
                    Height--; Height++;
                    break;
                case InfoType.EditingTag:
                    var pos = Player.Position;
                    Player.Stop();
                    Player.Source = null;
                    await Task.Delay(500);
                    (e.Object as TagLib.File).Save();
                    Play(Manager[Manager.Find((e.Object as TagLib.File).Name)]);
                    Player.Position = pos;
                    break;
                case InfoType.MediaRemoved:
                    MediaViews.RemoveAll(item => item.MediaIndex == e.Integer);
                    QueueListView.Items.Clear();
                    for (int i = 0; i < MediaViews.Count; i++)
                        QueueListView.Items.Add(MediaViews[i]);
                    break;
                case InfoType.MediaRequested:
                    Play(Manager.Next(e.Integer));
                    break;
                case InfoType.MediaUpdate:
                    MediaViews.Find(item => item.MediaIndex == e.Integer).Revoke(e);
                    if (e.Integer == Manager.CurrentlyPlayingIndex)
                    {
                        var q = Player.Position;
                        Play(e.Object as Media);
                        ChangePosition(q.TotalMilliseconds, true);
                    }
                    break;
                default:
                    break;
            }
        }
        private async void Keyboard_KeyDown(object sender, Forms::KeyEventArgs e)
        {
            if (IsActive || IsAncestorKeyDown(e))
            {
                switch (e.KeyCode)
                {
                    case Forms::Keys.Left:
                        ChangePosition(PositionSlider.SmallChange * -1);
                        await Task.Delay(200);
                        break;
                    case Forms::Keys.Right:
                        ChangePosition(PositionSlider.SmallChange);
                        await Task.Delay(200);
                        break;
                    case Forms::Keys.A:
                        var cb = Clipboard.GetText() ?? String.Empty;
                        if (Uri.TryCreate(cb, UriKind.Absolute, out var uri))
                            Manager.Add(uri);
                        else
                            return;
                        QueueListView.ScrollIntoView(QueueListView.Items[QueueListView.Items.Count - 1]);
                        break;
                    default: break;
                }
            }
            switch (e.KeyCode)
            {
                case Forms::Keys.MediaNextTrack: NextButton_Click(this, null); break;
                case Forms::Keys.MediaPreviousTrack: PreviousButton_Click(this, null); break;
                case Forms::Keys.MediaPlayPause: PlayPauseButton_Click(this, null); break; 
                default: break;
            }
        }

        private void Player_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DraggerTimer.Start();
            try
            {
                if (WindowState != WindowState.Maximized)
                    DragMove();
                if (DraggerTimer.Enabled && !IsVisionOn[1])
                {
                    Topmost = !Topmost;
                    WindowStyle = Topmost ? WindowStyle.None : WindowStyle.SingleBorderWindow;
                }
            }
            catch (Exception) { }
        }
        private void Player_ContextFullScreen(object sender, RoutedEventArgs e)
        {
            if (ResizeMode != ResizeMode.NoResize)
            {
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                WindowState = WindowState.Maximized;
            }
            else
            {
                ResizeMode = ResizeMode.CanResize;
                WindowStyle = WindowStyle.ThreeDBorderWindow;
                WindowState = WindowState.Normal;
            }
        }
        private void Player_MouseMove(object sender, MouseEventArgs e)
        {
            OrinateFullVision(false);
            Player.Cursor = Cursors.Arrow;
            MouseMoveTimer.Stop();
            MouseMoveTimer.Start();
        }

        private void PlayPauseButton_Click(object sender, MouseButtonEventArgs e)
        {
            if (PlayPauseButton.Icon == IconType.Pause)
            {
                Player.Pause();
                PlayPauseButton.Icon = IconType.Play;
                Thumb.Refresh(false);
            }
            else
            {
                Player.Play();
                PlayPauseButton.Icon = IconType.Pause;
                Thumb.Refresh(true);
            }
        }
        private void NextButton_Click(object sender, MouseButtonEventArgs e) => Play(Manager.Next());
        private void PreviousButton_Click(object sender, MouseButtonEventArgs e)
        {
            if (PositionSlider.Value > PositionSlider.Maximum / 100 * 10)
                ChangePosition(0, true);
            else
                Play(Manager.Previous());
        }
        private void VisionButton_Click(object sender, MouseButtonEventArgs e)
        {
            WindowSizes[IsVisionOn[0] ? 1 : 0].Width = ActualWidth;
            IsVisionOn[0] = !IsVisionOn[0];
            BeginStoryboard(Resources[IsVisionOn[0] ? "VisionOnBoard" : "VisionOffBoard"] as Storyboard);
            if (!IsVisionOn[0]) { Height--; Height++; }
            (WindowWidthBoard.Children[0] as DoubleAnimation).From = ActualWidth;
            (WindowWidthBoard.Children[0] as DoubleAnimation).To = WindowSizes[IsVisionOn[0] ? 1: 0].Width;
            WindowWidthBoard.Begin();
        }
        private void PlayModeButton_Click(object sender, MouseButtonEventArgs e)
        {
            switch (Manager.ActivePlayMode)
            {
                case PlayMode.Shuffle:
                    PlayModeButton.Icon = IconType.RepeatOne;
                    Manager.ActivePlayMode = PlayMode.RepeatOne;
                    break;
                case PlayMode.RepeatOne:
                    PlayModeButton.Icon = IconType.Repeat;
                    Manager.ActivePlayMode = PlayMode.RepeatAll;
                    break;
                case PlayMode.RepeatAll:
                    PlayModeButton.Icon = IconType.MusicQueue;
                    Manager.ActivePlayMode = PlayMode.Queue;
                    break;
                case PlayMode.Queue:
                    PlayModeButton.Icon = IconType.Shuffle;
                    Manager.ActivePlayMode = PlayMode.Shuffle;
                    break;
                default:
                    break;
            }
            App.Preferences.PlayMode = (int)Manager.ActivePlayMode;
        }

        public static string CastTime(TimeSpan time)
        {
            //Absolutely unreadable, btw just works...
            return $"{(time.TotalSeconds - (time.TotalSeconds % 60)).ToInt() / 60}:" +
                $"{((time.TotalSeconds.ToInt() % 60).ToString().Length == 1 ? $"0{time.TotalSeconds.ToInt() % 60}" : (time.TotalSeconds.ToInt() % 60).ToString())}";
        }
        public static string CastTime(int ms) => CastTime(new TimeSpan(0, 0, 0, 0, ms));
        private void AnySettingChanged(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
                return;
            App.Preferences.LibraryValidation = Pref_DoubleValid.IsChecked.Value;
            App.Preferences.Save();
        }
        private void Play(Media media)
        {
            if (!media.IsValid)
            {
                Manager.Remove(media);
                return;
            }
            PositionSlider.Value = 0;
            PlayPauseButton.Icon = IconType.Pause;
            Player.Source = new Uri(media.Path);
            Player.Play();
            MiniArtworkImage.Source = media.Artwork;
            TitleLabel.Content = media.Title;
            for (int i = 0; i < MediaViews.Count; i++)
                MediaViews[i].IsPlaying = false;
            //int index = MediaViews.FindIndex(item => item.MediaIndex == Manager.CurrentlyPlayingIndex);
            //MediaViews[index].IsPlaying = true;
            MediaViews.Find(item => item.MediaIndex == Manager.CurrentlyPlayingIndex).IsPlaying = true;
            VisionButton.Visibility = MediaManager.GetType(media.Path) == MediaType.Video ? Visibility.Visible : Visibility.Collapsed;
            if (IsVisionOn[0] && media.MediaType == MediaType.Music)
            {
                VisionButton_Click(this, null);
                OrinateFullVision(false);
                WindowStyle = WindowStyle.SingleBorderWindow;
                Topmost = false;
            }
        }
        private void OrinateFullVision(bool Enabled)
        {
            IsVisionOn[1] = Enabled;
            if (!IsVisionOn[0] && Enabled)
                BeginStoryboard(Resources["VisionOnBoard"] as Storyboard);
            else if (IsVisionOn[0])
                BeginStoryboard(Resources[Enabled ? "FullVisionOnBoard" : "FullVisionOffBoard"] as Storyboard);
            else
                BeginStoryboard(Resources["VisionOffBoard"] as Storyboard);
            Player.Cursor = Cursors.None;
        }
        private bool IsAncestorKeyDown(Forms::KeyEventArgs e)
        {
            switch (App.Preferences.MainKey)
            {
                case 0: return e.Control;
                case 1: return e.Alt;
                case 2: return e.Shift;
                case 3: return e.Control && e.Alt;
                case 4: return e.Control && e.Shift;
                case 5: return e.Shift && e.Alt;
                case 6: return e.Control && e.Shift && e.Alt;
                default: return false;
            }
        }

        private void ChangePosition(double ms, bool Seek = false)
        {
            IsUserSeeking = true;
            if (Seek) PositionSlider.Value = ms;
            else PositionSlider.Value += ms;
            IsUserSeeking = false;
        }
        private async void RunUX()
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
                    TimeLabel_Full.Content = CastTime(TimeSpan);
                    PlayCountTimer.Stop();
                    PlayCountTimer.Interval = PositionSlider.Maximum / 3;
                    PlayCountTimer.Start();
                    Manager.CurrentlyPlaying.Length = TimeSpan.TotalMilliseconds.ToInt();
                    MediaViews.Find(item => item.MediaIndex == Manager.CurrentlyPlayingIndex).TimeLabel.Content = TimeLabel_Full.Content;
                }
            PositionSlider.Value = Player.Position.TotalMilliseconds;
            TimeLabel_Current.Content = CastTime(Player.Position);
            if (!IsVisionOn[0] && PlayModeButton.Opacity <= 0.5)
                BeginStoryboard(Resources["VisionOffBoard"] as Storyboard);
            goto UX;
        }

        private void Position_Changed(object sender, Routed e)
        {
            if (IsUserSeeking)
            {
                Player.Position = new TimeSpan(0, 0, 0, 0, PositionSlider.Value.ToInt());
                PositionSlider.Value = ((Slider)sender).Value;
            }
        }
        private async void Position_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IsUserSeeking = true;
            while (e.ButtonState == MouseButtonState.Pressed)
            {
                Player.Pause();
                await Task.Delay(50);
                Player.Play();
                await Task.Delay(50);
            }
            PlayPauseButton.Icon = IconType.Pause;
            Player.Play();
            IsUserSeeking = false;
        }

        private async void VolumeButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            VolumePopup.IsOpen = true;
            while (e.LeftButton == MouseButtonState.Pressed)
            {
                if (Player.Volume < 1)
                    Player.Volume += 0.01;
                ProcessVolume();
                await Task.Delay(50);
            }
            while(e.RightButton == MouseButtonState.Pressed)
            {
                if (Player.Volume > 0)
                    Player.Volume -= 0.01;
                ProcessVolume();
                await Task.Delay(50);
            }
            VolumePopup.IsOpen = false;
        }
        private void ProcessVolume()
        {
            VolumeLabel.Content = (Player.Volume * 100).ToInt();
            switch (Player.Volume)
            {
                case double n when (n < 0.1): VolumeButton.Icon = IconType.Volume0; break;
                case double n when (n < 0.4): VolumeButton.Icon = IconType.Volume1; break;
                case double n when (n < 0.8): VolumeButton.Icon = IconType.Volume2; break;
                default: VolumeButton.Icon = IconType.Volume3; break;
            }
        }
    }
}
