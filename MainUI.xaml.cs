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
namespace Player
{
    /// <summary>
    /// Interaction logic for MainUI.xaml
    /// </summary>
    public partial class MainUI : Window
    {
        enum Tabs { NowPlaying, Songs, Vision, SpaceBuilder, Settings }

        Preferences P = Preferences.Load();
        MediaManager Manager = new MediaManager();
        List<MediaView> MediaViews = new List<MediaView>();
        Taskbar.Thumb Thumb = new Taskbar.Thumb();

        double Position_Value
        {
            get => (double)Resources["Position_Value"];
            set => Resources["Position_Value"] = value;
        }
        double Position_Max
        {
            get => (double)Resources["Position_Max"];
            set => Resources["Position_Max"] = value;
        }
        double Position_SmallChange
        {
            get => (double)Resources["Position_ChangeS"];
            set => Resources["Position_ChangeS"] = value;
        }
        double Position_LargeChange
        {
            get => (double)Resources["Position_ChangeL"];
            set => Resources["Position_ChangeL"] = value;
        }
        System.Windows.Media.ImageSource Art
        {
            get => NP_ArtworkImage.Source;
            set
            {
                NP_ArtworkImage.Source = value;
                MiniArtworkImage.Source = value;
            }
        }
        new string Title
        {
            get => (string)Resources["Res_Title"];
            set => Resources["Res_Title"] = value;
        }
        IconType PPIcon
        {
            get => PlayPauseButton1.Icon;
            set
            {
                PlayPauseButton1.Icon = value;
                PlayPauseButton2.Icon = value;
            }
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
            Pref_MediaRestr.IsChecked = P.Restrict;
            
            Pref_VisionOrient.IsChecked = P.VisionOrientation;
            Pref_VolBal.IsChecked = P.VolumeSetter;
            Pref_WM.IsChecked = P.WMDebug;

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
                    Position_Max = TimeSpan.TotalMilliseconds;
                    Position_SmallChange = 1 * Position_Max / 100;
                    Position_LargeChange = 5 * Position_Max / 100;
                }
            Position_Value = Player.Position.TotalMilliseconds;
            NP_Label5.Content = $"{ConvertTime(Player.Position)}   -    {Manager.CurrentlyPlayingIndex + 1} \\ {Manager.Count} ";
            if (Position_Value >= Position_Max - 250)
                Play(Manager.Next());
            goto UX;
        }
        private async void Manager_Change(object sender, ManagementChangeEventArgs e)
        {
            switch (e.Change)
            {
                case ManagementChange.NewMedia:
                   MediaViews.Add(new MediaView(e.Changes.Index, Manager[e.Changes.Index].Title, Manager[e.Changes.Index].Artist, Manager[e.Changes.Index].MediaType));
                    QueueListView.Items.Add(MediaViews[MediaViews.Count - 1]);
                    MediaViews[MediaViews.Count - 1].DoubleClicked += MainUI_DoubleClicked;
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
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Left = P.LastLoc.X;
            Top = P.LastLoc.Y;
            Sizes = P.Sizes;
            Height = Sizes[0].H;
            Width = Sizes[0].W;
            if (Environment.GetCommandLineArgs().Length != 0)
                if (Manager.Add(Environment.GetCommandLineArgs()))
                {
                    Play(Manager.Next(Manager.Count - 1));
                    if (Manager.CurrentlyPlaying.IsVideo)
                        OrinateVideoUI(true);
                }
            DraggerTimer.Elapsed += DraggerTimer_Elapsed;
            TaskbarItemInfo = Thumb.Info;
            Thumb.NextPressed += (obj, f) => NextButton_Click(obj, f);
            Thumb.PausePressed += (obj, f) => PlayPauseButtonClick(obj, f);
            Thumb.PlayPressed += (obj, f) => PlayPauseButtonClick(obj, f);
            Thumb.PrevPressed += (obj, f) => PreviousButton_Click(obj, f);

            User.Keyboard.KeyDown += Keyboard_KeyDown;
            User.Keyboard.KeyUp += Keyboard_KeyUp;
            User.Keyboard.KeyPress += Keyboard_KeyPress;
            UserExperience();
        }

        private void Keyboard_KeyPress(object sender, Forms::KeyPressEventArgs e)
        {

        }

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
                        ForcePositionChange(Position_SmallChange * -1);
                        await Task.Delay(200);
                        break;
                    case Forms::Keys.Right:
                        ForcePositionChange(Position_SmallChange);
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
            P.Sizes = Sizes;
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
            LyricsBox.Text = media.Lyrics;
            Position_Value = 0;
            PPIcon = IconType.ic_pause;
            Player.Source = new Uri(media.Path);
            Player.Play();
            Art = media.Artwork;
            Title = media.Title;
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
            for (int i = 0; i < MediaViews.Count; i++)
                MediaViews[i].Width = QueueListView.ActualWidth > 25 ? QueueListView.ActualWidth - 25 : 25;
            Expander_CollapsAnim.From = Height + 50;
            Expander_ExpandAnim.To = Height + 50;
            if (PlayingExpander.IsExpanded)
                Height = Height + 50;
        }
        
        private void PlayPauseButtonClick(object sender, EventArgs e)
        {
            if (PPIcon == IconType.ic_pause)
            {
                Player.Pause();
                PPIcon = IconType.ic_play_arrow;
                Thumb.Refresh(false);
            }
            else
            {
                Player.Play();
                PPIcon = IconType.ic_pause;
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
            if (Seek) Position_Value = ms;
            else Position_Value += ms;
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
            PPIcon = IconType.ic_pause;
            Player.Play();
            UserChangingPosition = false;
        }
        private void PositionChanged(object sender, Routed e)
        {
            if (UserChangingPosition)
            {
                Player.Position = new TimeSpan(0, 0, 0, 0, Position_Value.ToInt());
                Position_Value = ((Slider)sender).Value;
                PositionSlider1.SetResourceReference(Slider.ValueProperty, "Position_Value");
                PositionSlider2.SetResourceReference(Slider.ValueProperty, "Position_Value");
            }
        }
        private (int mins, int secs) TimeConvertion;
        private (double W, double H)[] Sizes = new(double, double)[]
        {
            (0, 0), (0, 0)
        };
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
            if (Position_Value > Position_Max / 100 * 10)
                ForcePositionChange(0, true);
            else
                Play(Manager.Previous());
        }

        private void LyricsButton_Click(object sender, EventArgs e)
        {
            LyricsPopup.IsOpen = !LyricsPopup.IsOpen;
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            Position_Value -= Position_LargeChange;
        }

        private void RepeatButton_Click_1(object sender, RoutedEventArgs e)
        {

            Position_Value += Position_LargeChange;
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
            P.Restrict = Pref_MediaRestr.IsChecked.Value;
            P.VisionOrientation = Pref_VisionOrient.IsChecked.Value;
            P.VolumeSetter = Pref_VolBal.IsChecked.Value;
            P.WMDebug = Pref_WM.IsChecked.Value;
            P.Save();
        }
    }
}
