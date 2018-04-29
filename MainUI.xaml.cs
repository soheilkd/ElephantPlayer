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
        private Timer SizeChangeTimer = new Timer(200) { AutoReset = true };
        private TimeSpan TimeSpan;
        private Taskbar.Thumb Thumb = new Taskbar.Thumb();
        private bool IsUserSeeking = false;
        private bool[] IsVisionOn = new bool[] { false, false };
        
        public MainUI()
        {
            InitializeComponent();
            Manager.Change += Manager_Change;
            App.NewInstanceRequested += (_, e) => Manager.Add(e.Args);

            Pref_DoubleValid.IsChecked = App.Preferences.LibraryValidation;
            var lib = MassiveLibrary.Load();
            for (int i = 0; i < lib.Medias.Length; i++)
                Manager.Add(lib.Medias[i]);
            Width = App.Preferences.LastSize.Width;
            Height = App.Preferences.LastSize.Height;
            Left = App.Preferences.LastLoc.X;
            Top = App.Preferences.LastLoc.Y;
            Player.Volume = 1;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SizeChangeTimer.Elapsed += delegate
            {
                Dispatcher.Invoke(
                    delegate
                    {
                        for (int i = 0; i < MediaViews.Count; i++)
                            MediaViews[i].Width = QueueListView.ActualWidth > 25 ? QueueListView.ActualWidth - 25 : 25;
                    });
                SizeChangeTimer.Stop();
            };
            var cml = Environment.GetCommandLineArgs();
            if (cml.Length > 1)
                Manager.Add(cml, true);
            TaskbarItemInfo = Thumb.Info;
            Thumb.NextPressed += (obj, f) => NextButton_Click(obj, f);
            Thumb.PausePressed += (obj, f) => PlayPauseButton_Click(obj, f);
            Thumb.PlayPressed += (obj, f) => PlayPauseButton_Click(obj, f);
            Thumb.PrevPressed += (obj, f) => PreviousButton_Click(obj, f);

            User.Keyboard.KeyDown += Keyboard_KeyDown;
            RunUX();
            PlayCountTimer.Elapsed += (_, __) => Manager.AddCount();
            AddUrlButton.Click += (_, p) =>
            {
                Manager.Add(new Uri(Clipboard.GetText() ?? String.Empty));
                QueueListView.ScrollIntoView(QueueListView.Items[QueueListView.Items.Count - 1]);
            };
            (Resources["VisionOnBoard"] as Storyboard).Completed += (_, __) => QueueListView.Visibility = Visibility.Collapsed;
            (Resources["VisionOffBoard"] as Storyboard).CurrentStateInvalidated += (_, __) => QueueListView.Visibility = Visibility.Visible;
            MouseMoveTimer.Elapsed += delegate
            {
                Dispatcher.Invoke(() =>
                {
                    if (!IsVisionOn[0])
                        return;
                    OrinateFullVision(true);
                    Player.Cursor = Cursors.None;
                });
            };
            switch ((PlayMode)App.Preferences.PlayMode)
            {
                case PlayMode.Shuffle: PlayModeButton.Icon = IconType.shuffle; break;
                case PlayMode.RepeatOne: PlayModeButton.Icon = IconType.repeat_one; break;
                case PlayMode.RepeatAll: PlayModeButton.Icon = IconType.repeat; break;
                case PlayMode.Queue: PlayModeButton.Icon = IconType.queue_music; break;
                default: PlayModeButton.Icon = IconType.repeat; break;
            }
            SizeChanged += (_, __) => SizeChangeTimer.Start();
            Width--; Width++; //For invoking SizeChanged
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App.Preferences.LastSize = new Size(Width, Height);
            App.Preferences.LastLoc = new Point(Left, Top);
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

        private async void Manager_Change(object sender, InfoExchangeArgs e)
        {
            switch (e.Type)
            {
                case InfoType.NewMedia:
                    MediaViews.Add(new MediaView(e.Integer, Manager[e.Integer].Title, Manager[e.Integer].Artist, Manager[e.Integer].MediaType));
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
                        Manager[f.Integer] = f.Media;
                        if (Manager.CurrentlyPlayingIndex == e.Integer)
                        {
                            var posit = Player.Position;
                            Play(e.Media);
                            ChangePosition(posit.TotalMilliseconds, true);
                        }
                    };
                    MediaViews[p].ZipDownloaded += (n, f) =>
                    {
                        Manager.Add((string[])f.ObjectArray);
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
                    int index = MediaViews.FindIndex(item => item.MediaIndex == e.Integer);
                    MediaViews.RemoveAt(index);
                    QueueListView.Items.Clear();
                    for (int i = 0; i < MediaViews.Count; i++)
                        QueueListView.Items.Add(MediaViews[i]);
                    break;
                case InfoType.MediaRequested:
                    Play(Manager.Next(e.Integer));
                    break;
                case InfoType.MediaUpdate:
                    for (int i = 0; i < MediaViews.Count; i++)
                        if (MediaViews[i].MediaIndex == e.Integer)
                            MediaViews[i].Revoke(e.Integer, e.Media.Title, e.Media.Artist);
                    MediaViews[MediaViews.FindIndex(item => item.MediaIndex == e.Integer)].Revoke(e);
                    if (e.Integer == Manager.CurrentlyPlayingIndex)
                    {
                        var q = Player.Position;
                        Play(e.Media);
                        ChangePosition(q.TotalMilliseconds, true);
                    }
                    break;
                case InfoType.Crash:
                    break;
                case InfoType.PopupRequest:
                    break;
                case InfoType.ArtworkClick:
                    break;
                case InfoType.SomethingHappened:
                    break;
                default:
                    break;
            }
        }
        private async void Keyboard_KeyDown(object sender, Forms::KeyEventArgs e)
        {
            if (IsActive || e.Alt)
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
                    case Forms::Keys.Up:
                        VolumeButton_MouseDown(this, new MouseButtonEventArgs(Mouse.PrimaryDevice, 1, MouseButton.Left));
                        break;
                    case Forms::Keys.Down:
                        VolumeButton_MouseDown(this, new MouseButtonEventArgs(Mouse.PrimaryDevice, 1, MouseButton.Right));
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
                    PlayPauseButton_Click(this, null);
                    break;
                case Forms::Keys.MediaStop:
                    break;
                default:
                    break;
            }
        }

        private void Player_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DraggerTimer.Start();
            try
            {
                if (WindowState != WindowState.Maximized)
                    DragMove();
                if (DraggerTimer.Enabled)
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
            MouseMoveTimer.Start();
        }

        private void PlayPauseButton_Click(object sender, EventArgs e)
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
        private void NextButton_Click(object sender, EventArgs e) => Play(Manager.Next());
        private void PreviousButton_Click(object sender, EventArgs e)
        {
            if (PositionSlider.Value > PositionSlider.Maximum / 100 * 10)
                ChangePosition(0, true);
            else
                Play(Manager.Previous());
        }
        private void VisionButton_Click(object sender, EventArgs e)
        {
            IsVisionOn[0] = !IsVisionOn[0];
            BeginStoryboard(Resources[IsVisionOn[0] ? "VisionOnBoard" : "VisionOffBoard"] as Storyboard);
            if (!IsVisionOn[0]) { Height--; Width++; }
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
            App.Preferences.PlayMode = (int)Manager.ActivePlayMode;
        }

        private string CastTime(TimeSpan time)
        {
            //Absolutely unreadable, btw just works...
            return $"{(time.TotalSeconds - (time.TotalSeconds % 60)).ToInt() / 60}:" +
                $"{((time.TotalSeconds.ToInt() % 60).ToString().Length == 1 ? $"0{time.TotalSeconds.ToInt() % 60}" : (time.TotalSeconds.ToInt() % 60).ToString())}";
        }
        private void AnySettingChanged(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
                return;
            App.Preferences.LibraryValidation = Pref_DoubleValid.IsChecked.Value;
            App.Preferences.Save();
        }
        private void Play(Media media)
        {
            if (!media.Exists)
            {
                var res = MessageBox.Show("Couldn't reach target, remove?", "IOException", MessageBoxButton.YesNo, MessageBoxImage.Error);
                if (res == MessageBoxResult.Yes)
                    Manager.Remove(media);
                return;
            }
            PositionSlider.Value = 0;
            PlayPauseButton.Icon = IconType.pause;
            Player.Source = new Uri(media.Path);
            Player.Play();
            MiniArtworkImage.Source = media.Artwork;
            TitleLabel.Content = media.Title;
            for (int i = 0; i < MediaViews.Count; i++)
                MediaViews[i].IsPlaying = false;
            int index = MediaViews.FindIndex(item => item.MediaIndex == Manager.CurrentlyPlayingIndex);
            MediaViews[index].IsPlaying = true;
            VisionButton.Visibility = MediaManager.GetType(media.Path) == MediaType.Video ? Visibility.Visible : Visibility.Collapsed;
            if (IsVisionOn[0] && media.MediaType == MediaType.Music)
            {
                VisionButton_Click(this, null);
                OrinateFullVision(false);
                Topmost = false;
                WindowStyle = WindowStyle.SingleBorderWindow;
            }
        }
        private void OrinateFullVision(bool Enabled)
        {
            IsVisionOn[1] = Enabled;
            BeginStoryboard(Resources[Enabled ? "FullVisionOnBoard" : "FullVisionOffBoard"] as Storyboard);
            if (Enabled && !IsVisionOn[0])
                BeginStoryboard(Resources["VisionOnBoard"] as Storyboard);
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
                }
            PositionSlider.Value = Player.Position.TotalMilliseconds;
            TimeLabel_Current.Content = CastTime(Player.Position);
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
            PlayPauseButton.Icon = IconType.pause;
            Player.Play();
            IsUserSeeking = false;
        }
        private void Position_RepeatBackwardClick(object sender, RoutedEventArgs e) => PositionSlider.Value -= PositionSlider.LargeChange;
        private void Position_RepeatForwardClick(object sender, RoutedEventArgs e) => PositionSlider.Value += PositionSlider.LargeChange;

        private async void VolumeButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            VolumePopup.IsOpen = true;
            while (e.LeftButton == MouseButtonState.Pressed)
            {
                if (Player.Volume < 1)
                    Player.Volume += 0.01;
                VolumeLabel.Content = (Player.Volume * 100).ToInt();
                switch (Player.Volume)
                {
                    case double n when (n < 0.1): VolumeButton.Icon = IconType.volume_0; break;
                    case double n when (n < 0.4): VolumeButton.Icon = IconType.volume_1; break;
                    case double n when (n < 0.8): VolumeButton.Icon = IconType.volume_2; break;
                    default: VolumeButton.Icon = IconType.volume_3; break;
                }
                await Task.Delay(50);
            }
            while(e.RightButton == MouseButtonState.Pressed)
            {
                if (Player.Volume > 0)
                    Player.Volume -= 0.01;
                VolumeLabel.Content = (Player.Volume * 100).ToInt();
                switch (Player.Volume)
                {
                    case double n when (n < 0.1): VolumeButton.Icon = IconType.volume_0; break;
                    case double n when (n < 0.4): VolumeButton.Icon = IconType.volume_1; break;
                    case double n when (n < 0.8): VolumeButton.Icon = IconType.volume_2; break;
                    default: VolumeButton.Icon = IconType.volume_3; break;
                }
                await Task.Delay(50);
            }
            VolumePopup.IsOpen = false;
        }
    }
}
