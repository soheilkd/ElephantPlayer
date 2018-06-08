using Player.Events;
using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Player.Controls
{
    public partial class MediaPlayer : UserControl
    {
        public TimeSpan Position
        {
            get => element.Position;
            set
            {

                element.Position = value;
                if (value.TotalSeconds <= 20)
                {
                    PlayCountTimer.Stop();
                    PlayCountTimer.Start();
                }
            }
        }
        public double Volume { get => element.Volume; set { element.Volume = value; ProcessVolume(); } }
        private Media _Media = new Media();
        public event EventHandler<InfoExchangeArgs> EventHappened;
        private Timer DraggerTimer = new Timer(250) { AutoReset = false };
        private Timer MouseMoveTimer = new Timer(App.Settings.MouseOverTimeout) { AutoReset = false };
        private Timer PlayCountTimer = new Timer(120000) { AutoReset = false };
        private TimeSpan TimeSpan;
        private bool IsUserSeeking, IsFullScreen, WasMaximized;
        public Window ParentWindow;
        public Taskbar.Thumb Thumb = new Taskbar.Thumb();
        private Storyboard MagnifyBoard, MinifyBoard, FullOnBoard, FullOffBoard;
        private ThicknessAnimation MagnifyAnimation, MinifyAnimation;
        private TimeSpan SmallChange, BackwardSmallChange;
        private bool controlsVisibile;
        private bool ControlsVisible
        {
            get => controlsVisibile;
            set
            {
                if (!Magnified)
                    value = true;
                controlsVisibile = value;
                if (value)
                {
                    FullOnBoard.Stop();
                    Dispatcher.Invoke(() => FullOffBoard.Begin());
                }
                else
                {
                    FullOffBoard.Stop();
                    Dispatcher.Invoke(() => FullOnBoard.Begin());
                }
            }
        }
        private bool magnified;
        public bool Magnified
        {
            get => magnified;
            set
            {
                magnified = value;
                ControlsGrid.VerticalAlignment = value ? VerticalAlignment.Bottom : VerticalAlignment.Top;
                MinifyAnimation.To = new Thickness(ActualWidth / 2, ActualHeight, ActualWidth / 2, 0);
                if (value)
                    MagnifyBoard.Begin();
                else
                    MinifyBoard.Begin();
                MouseMoveTimer.Start();
                if (!Magnified && false)
                {
                    ParentWindow.Height--;
                    ParentWindow.Height++;
                }
                Resources["Foreground"] = value ? Brushes.White : Brushes.Black;
                EventHappened?.Invoke(this, new InfoExchangeArgs() { Type = InfoType.Magnifiement, Object = value });
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
            MouseMoveTimer.Elapsed += (_, __) => ControlsVisible = false;
            PlayCountTimer.Elapsed += PlayCountTimer_Elapsed;
            FullOnBoard.Completed += (_, __) => Cursor = Cursors.None;
            FullOffBoard.CurrentStateInvalidated += (_, __) => Cursor = Cursors.Arrow;
        }

        private void PlayCountTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _Media.PlayCount++;
            PlayCountTimer.Stop();
        }

        private void Element_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DraggerTimer.Start();
            try
            {
                if (ParentWindow.WindowState != WindowState.Maximized)
                    ParentWindow.DragMove();
                if (DraggerTimer.Enabled && !IsFullScreen)
                {
                    ParentWindow.Topmost = !ParentWindow.Topmost;
                    ParentWindow.WindowStyle = ParentWindow.Topmost ? WindowStyle.None : WindowStyle.SingleBorderWindow;
                }
            }
            catch (Exception) { }
        }
        private async void Element_MouseMove(object sender, MouseEventArgs e)
        {
            var y = ControlsTranslation.Y;
            await Task.Delay(50);
            if (ControlsTranslation.Y < y)
                return;
            ControlsVisible = true;
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
                    TimeLabel_Full.Content = TimeSpan.ToNewString();
                    Invoke(InfoType.LengthFound, TimeSpan);
                    SmallChange = new TimeSpan(0, 0, 0, 0, (int)PositionSlider.SmallChange);
                    BackwardSmallChange = new TimeSpan(0, 0, 0, 0, -1 * (int)PositionSlider.SmallChange);
                }
            TimeLabel_Current.Content = Position.ToNewString();
            PositionSlider.Value = Position.TotalMilliseconds;
            goto UX;
        }

        private void Position_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsUserSeeking)
            {
                Position = new TimeSpan(0, 0, 0, 0, (int)PositionSlider.Value);
                element.Play();
            }
        }
        private async void Position_Holding(object sender, MouseButtonEventArgs e)
        {
            IsUserSeeking = true;
            while (e.ButtonState == MouseButtonState.Pressed)
            {
                await Task.Delay(50);
                element.Pause();
            }
            PlayPauseButton.Glyph = Glyph.Pause;
            element.Play();
            IsUserSeeking = false;
        }
        private void PlayPauseButton_Clicked(object sender, MouseButtonEventArgs e)
        {
            if (PlayPauseButton.Glyph == Glyph.Pause)
            {
                element.Pause();
                PlayPauseButton.Glyph = Glyph.Play;
                Thumb.SetPlayingState(false);
            }
            else
            {
                element.Play();
                PlayPauseButton.Glyph = Glyph.Pause;
                Thumb.SetPlayingState(true);
            }
        }
        private void NextButton_Clicked(object sender, MouseButtonEventArgs e) => Invoke(InfoType.NextRequest);
        private void PreviousButton_Clicked(object sender, MouseButtonEventArgs e)
        {
            if (PositionSlider.Value > PositionSlider.Maximum / 100 * 10)
            {
                element.Stop();
                element.Play();
            }
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
                FullScreenButton.Glyph = Glyph.ExitFullScreen;
                VisionButton.Visibility = Visibility.Hidden;
            }
            else
            {
                ParentWindow.ResizeMode = ResizeMode.CanResize;
                ParentWindow.WindowStyle = WindowStyle.ThreeDBorderWindow;
                ParentWindow.WindowState = WasMaximized ? WindowState.Maximized : WindowState.Normal;
                FullScreenButton.Glyph = Glyph.FullScreen;
                VisionButton.Visibility = Visibility.Visible;
            }
        }

        private void ProcessVolume()
        {
            switch (element.Volume)
            {
                case double n when (n < 0.1): VolumeIcon.Glyph = Glyph.Volume0; break;
                case double n when (n < 0.4): VolumeIcon.Glyph = Glyph.Volume1; break;
                case double n when (n < 0.8): VolumeIcon.Glyph = Glyph.Volume2; break;
                default: VolumeIcon.Glyph = Glyph.Volume3; break;
            }
            App.Settings.Volume = element.Volume;
        }

        public void PlayNext() => NextButton.EmulateClick();
        public void PlayPrevious() => PreviousButton.EmulateClick();
        public void PlayPause() => PlayPauseButton.EmulateClick();

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Volume = VolumeSlider.Value / 100;
        }

        private void PlayMode_Click(object sender, MouseButtonEventArgs e)
        {
            switch (PlayModeButton.Glyph)
            {
                case Glyph.RepeatAll:
                    PlayModeButton.Glyph = Glyph.RepeatOne;
                    App.Settings.PlayMode = PlayMode.RepeatOne;
                    break;
                case Glyph.RepeatOne:
                    PlayModeButton.Glyph = Glyph.Shuffle;
                    App.Settings.PlayMode = PlayMode.Shuffle;
                    break;
                case Glyph.Shuffle:
                    PlayModeButton.Glyph = Glyph.RepeatAll;
                    App.Settings.PlayMode = PlayMode.RepeatAll;
                    break;
                default:
                    break;
            }
        }

        public void SmallSlideLeft() => Position = Position.Subtract(SmallChange);
        public void SmallSlideRight() => Position = Position.Add(SmallChange);

        private void Element_MediaEnded(object sender, RoutedEventArgs e)
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

        public void Play(Media media)
        {
            media.Load();
            _Media = media;
            VisionButton.Visibility = media.IsVideo ? Visibility.Visible : Visibility.Hidden;
            FullScreenButton.Visibility = VisionButton.Visibility;
            if (IsFullScreen && !media.IsVideo)
                FullScreenButton.EmulateClick();
            Magnified = media.IsVideo;
            PlayPauseButton.Glyph = Glyph.Pause;
            element.Source = media.Url;
            element.Play();
            PlayCountTimer.Stop();
            PlayCountTimer.Start();
            TitleLabel.Content = media.ToString();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            RunUX();
            Volume = App.Settings.Volume;
            App.Settings.Changed += (_, __) => MouseMoveTimer = new Timer(App.Settings.MouseOverTimeout) { AutoReset = false };
            VolumeSlider.Value = Volume * 100;
        }
    }
}
