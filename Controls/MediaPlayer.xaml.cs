using Player.Events;
using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using static Player.Global;

namespace Player.Controls
{
    public partial class MediaPlayer : UserControl
    {
        public TimeSpan Position { get => element.Position; set => element.Position = value; }
        public double Volume { get => element.Volume; set => element.Volume = value; }
        public event EventHandler<InfoExchangeArgs> EventHappened;
        private Timer DraggerTimer = new Timer(250) { AutoReset = false };
        private Timer MouseMoveTimer = new Timer(5000) { AutoReset = false };
        private Timer PlayCountTimer;
        private TimeSpan TimeSpan;
        private bool IsUserSeeking, IsFullScreen, WasMaximized;
        private Window ParentWindow;
        private Taskbar.Thumb Thumb = new Taskbar.Thumb();
        private Storyboard MagnifyBoard, MinifyBoard, FullOnBoard, FullOffBoard;
        private ThicknessAnimation MagnifyAnimation, MinifyAnimation;
        private bool controlsVisibile;
        private bool ControlsVisible
        {
            get => controlsVisibile;
            set
            {
                if (!Magnified)
                    value = true;
                controlsVisibile = value;
                FullOnBoard.Stop();
                FullOffBoard.Stop();
                if (value)
                    Dispatcher.Invoke(() => FullOffBoard.Begin());
                else
                    Dispatcher.Invoke(() => FullOnBoard.Begin());
            }
        }
        private bool magnified;
        public bool Magnified
        {
            get => magnified;
            set
            {
                magnified = value;
                MinifyAnimation.To = new Thickness(ActualWidth / 2, ActualHeight, ActualWidth / 2, 0);
                if (value)
                    MagnifyBoard.Begin();
                else
                    MinifyBoard.Begin();
                MouseMoveTimer.Start();
                if (!Magnified)
                {
                    ParentWindow.Height--;
                    ParentWindow.Height++;
                }
            }
        }
        public MediaPlayer()
        {
            InitializeComponent();
            MagnifyBoard = Resources["MagnifyBoard"] as Storyboard;
            MinifyBoard = Resources["MinifyBoard"] as Storyboard;
            FullOnBoard = Resources["FullOnBoard"] as Storyboard;
            FullOffBoard = Resources["FullOffBoard"] as Storyboard;
            MagnifyAnimation = MagnifyBoard.Children[0] as ThicknessAnimation;
            MinifyAnimation = MinifyBoard.Children[0] as ThicknessAnimation;

            Thumb.NextPressed += (obj, f) => PlayNext();
            Thumb.PausePressed += (obj, f) => PlayPause();
            Thumb.PlayPressed += (obj, f) => PlayPause();
            Thumb.PrevPressed += (obj, f) => PlayPrevious();
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
            switch ((PlayMode)App.Preferences.PlayMode)
            {
                case PlayMode.Shuffle: PlayModeButton.Glyph = Glyph.Shuffle; break;
                case PlayMode.RepeatOne: PlayModeButton.Glyph = Glyph.RepeatOne; break;
                case PlayMode.RepeatAll: PlayModeButton.Glyph = Glyph.RepeatAll; break;
                case PlayMode.Queue: PlayModeButton.Glyph = Glyph.GroupList; break;
                default: PlayModeButton.Glyph = Glyph.RepeatAll; break;
            }
            MouseMoveTimer.Elapsed += (_, __) => ControlsVisible = false;
        }

        private void Element_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DraggerTimer.Start();
            try
            {
                if (ParentWindow.WindowState != WindowState.Maximized)
                    ParentWindow.DragMove();
                if (DraggerTimer.Enabled)
                {
                    ParentWindow.Topmost = !ParentWindow.Topmost;
                    ParentWindow.WindowStyle = ParentWindow.Topmost ? WindowStyle.None : WindowStyle.SingleBorderWindow;
                }
            }
            catch (Exception) { }
        }
        private void Element_MouseMove(object sender, MouseEventArgs e)
        {
            element.Cursor = Cursors.Arrow;
            ControlsVisible = true;
            MouseMoveTimer.Stop();
            MouseMoveTimer.Start();
        }
        
        private void Invoke(InfoType type, object obj = null) => EventHappened?.Invoke(this, new InfoExchangeArgs() { Type = type, Object = obj });
        
        private async void RunUX()
        {
            UX:
            await Task.Delay(250);
            if (element.NaturalDuration.HasTimeSpan)
                if (element.NaturalDuration.TimeSpan != TimeSpan)
                {
                    //Update TimeSpan
                    TimeSpan = element.NaturalDuration.TimeSpan;
                    PositionSlider.Maximum = TimeSpan.TotalMilliseconds;
                    PositionSlider.SmallChange = 1 * PositionSlider.Maximum / 100;
                    PositionSlider.LargeChange = 5 * PositionSlider.Maximum / 100;
                    TimeLabel_Full.Content = CastTime(TimeSpan);
                    Invoke(InfoType.LengthFound, TimeSpan);
                }
            PositionSlider.Value = element.Position.TotalMilliseconds;
            TimeLabel_Current.Content = CastTime(Position);
            goto UX;
        }

        private void Position_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsUserSeeking)
            {
                Position = new TimeSpan(0, 0, 0, 0, PositionSlider.Value.ToInt());
                PositionSlider.Value = ((Slider)sender).Value;
            }
        }
        private async void Position_Holding(object sender, MouseButtonEventArgs e)
        {
            IsUserSeeking = true;
            while (e.ButtonState == MouseButtonState.Pressed)
            {
                element.Pause();
                await Task.Delay(50);
                element.Play();
                await Task.Delay(50);
            }
            PlayPauseButton.Glyph = Glyph.Pause;
            element.Play();
            IsUserSeeking = false;
        }
        private void PlayModeButton_Clicked(object sender, MouseButtonEventArgs e)
        {
            switch (PlayModeButton.Glyph)
            {
                case Glyph.Shuffle:
                    PlayModeButton.Glyph = Glyph.RepeatOne;
                    Invoke(InfoType.PlayModeChange, PlayMode.RepeatOne);
                    break;
                case Glyph.RepeatOne:
                    PlayModeButton.Glyph = Glyph.RepeatAll;
                    Invoke(InfoType.PlayModeChange, PlayMode.RepeatAll);
                    break;
                case Glyph.RepeatAll:
                    PlayModeButton.Glyph = Glyph.GroupList;
                    Invoke(InfoType.PlayModeChange, PlayMode.Queue);
                    break;
                case Glyph.GroupList:
                    PlayModeButton.Glyph = Glyph.Shuffle;
                    Invoke(InfoType.PlayModeChange, PlayMode.Shuffle);
                    break;
                default:
                    break;
            }
        }
        private async void VolumeButton_Holding(object sender, MouseButtonEventArgs e)
        {
            VolumePopup.IsOpen = true;
            while (e.LeftButton == MouseButtonState.Pressed)
            {
                if (element.Volume < 1)
                    element.Volume += 0.01;
                ProcessVolume();
                await Task.Delay(50);
            }
            while (e.RightButton == MouseButtonState.Pressed)
            {
                if (element.Volume > 0)
                    element.Volume -= 0.01;
                ProcessVolume();
                await Task.Delay(50);
            }
            VolumePopup.IsOpen = false;
        }
        private void PlayPauseButton_Clicked(object sender, MouseButtonEventArgs e)
        {
            if (PlayPauseButton.Glyph == Glyph.Pause)
            {
                element.Pause();
                PlayPauseButton.Glyph = Glyph.Play;
                Thumb.Refresh(false);
            }
            else
            {
                element.Play();
                PlayPauseButton.Glyph = Glyph.Pause;
                Thumb.Refresh(true);
            }
        }
        private void NextButton_Clicked(object sender, MouseButtonEventArgs e) => Invoke(InfoType.NextRequest);
        private void PreviousButton_Clicked(object sender, MouseButtonEventArgs e)
        {
            if (PositionSlider.Value > PositionSlider.Maximum / 100 * 10)
                Seek(TimeSpan.Zero);
            else
                Invoke(InfoType.PrevRequest);
        }
        private void VisionButton_Clicked(object sender, MouseButtonEventArgs e)
        {
            Magnified = !Magnified;
        }
        private void FullScreenButton_Clicked(object sender, MouseButtonEventArgs e)
        {
            IsFullScreen = !IsFullScreen;
            if (IsFullScreen)
            {
                WasMaximized = ParentWindow.WindowState == WindowState.Maximized;
                if (WasMaximized)
                    ParentWindow.WindowState = WindowState.Normal;
                ParentWindow.WindowStyle = WindowStyle.None;
                ParentWindow.ResizeMode = ResizeMode.NoResize;
                ParentWindow.WindowState = WindowState.Maximized;
                FullScreenButton.Glyph = Glyph.BackToWindow;
            }
            else
            {
                ParentWindow.ResizeMode = ResizeMode.CanResize;
                ParentWindow.WindowStyle = WindowStyle.ThreeDBorderWindow;
                ParentWindow.WindowState = WasMaximized ? WindowState.Maximized : WindowState.Normal;
                FullScreenButton.Glyph = Glyph.FullScreen;
            }
        }

        private void ProcessVolume()
        {
            VolumeLabel.Content = (element.Volume * 100).ToInt();
            switch (element.Volume)
            {
                case double n when (n < 0.1): VolumeButton.Glyph = Glyph.Volume0; break;
                case double n when (n < 0.4): VolumeButton.Glyph = Glyph.Volume1; break;
                case double n when (n < 0.8): VolumeButton.Glyph = Glyph.Volume2; break;
                default: VolumeButton.Glyph = Glyph.Volume3; break;
            }
        }

        public void PlayNext() => NextButton_Clicked(this, null);
        public void PlayPrevious() => PreviousButton_Clicked(this, null);
        public void PlayPause() => PlayPauseButton_Clicked(this, null);
        public void SmallSlideLeft() => Seek((int)PositionSlider.SmallChange * -1, true);
        public void SmallSlideRight() => Seek((int)PositionSlider.SmallChange, true);

        private void element_MediaEnded(object sender, RoutedEventArgs e)
        {
            PlayNext();
        }

        public void FullStop()
        {
            element.Stop();
            element.Source = null;
        }

        public void Size_Changed(object sender, SizeChangedEventArgs e)
        {
            if (!Magnified)
            {
                elementCanvas.SetValue(MarginProperty, new Thickness(ActualWidth / 2, ActualHeight, ActualWidth / 2, 0));
            }
        }


        public void Seek(TimeSpan timeSpan, bool sliding = false)
        {
            if (!sliding) element.Position = timeSpan;
            else element.Position.Add(timeSpan);
        }
        public void Seek(int ms, bool sliding = false) => Seek(new TimeSpan(0, 0, 0, 0, ms), sliding);
      
        public void Play(Media media)
        {
            PositionSlider.Value = 0;
            PlayPauseButton.Glyph = Glyph.Pause;
            element.Source = new Uri(media.Path);
            element.Play();
            Invoke(media.IsVideo ? InfoType.OrinateToVision : InfoType.OrinateToDefault);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ParentWindow = Window.GetWindow(this);
            ParentWindow.TaskbarItemInfo = Thumb.Info;
            RunUX();
        }
    }
}
