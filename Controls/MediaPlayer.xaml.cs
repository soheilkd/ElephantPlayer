using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using static Player.Global;
using System.Windows.Input;
using Player.Events;
using System.Windows.Media.Animation;

namespace Player.Controls
{
    /// <summary>
    /// Interaction logic for MediaControl.xaml
    /// </summary>
    public partial class MediaPlayer : UserControl
    {
        public TimeSpan Position { get => element.Position; set => element.Position = value; }
        public double Volume { get => element.Volume; set => element.Volume = value; }
        public event EventHandler<InfoExchangeArgs> EventHappened;
        private Timer DraggerTimer = new Timer(250) { AutoReset = false };
        private Timer MouseMoveTimer = new Timer(5000) { AutoReset = false };
        bool IsUserSeeking;
        private Window ParentWindow;
        private Taskbar.Thumb Thumb = new Taskbar.Thumb();
        private Storyboard MagnifyBoard, MinifyBoard;
        private DoubleAnimation[] MagnifyAnimations, MinifyAnimations;
        public MediaPlayer()
        {
            InitializeComponent();
            MagnifyBoard = Resources["MagnifyBoard"] as Storyboard;
            MinifyBoard = Resources["MinifyBoard"] as Storyboard;
            MagnifyAnimations = new DoubleAnimation[]
            {
                MagnifyBoard.Children[0] as DoubleAnimation,
                MagnifyBoard.Children[1] as DoubleAnimation
            };
            MinifyAnimations = new DoubleAnimation[]
            {
                MinifyBoard.Children[0] as DoubleAnimation,
                MinifyBoard.Children[1] as DoubleAnimation
            };
            MagnifyBoard.CurrentStateInvalidated += MagnifyBoard_CurrentStateInvalidated;
            MagnifyBoard.Completed += MagnifyBoard_Completed;
            MinifyBoard.CurrentStateInvalidated += MinifyBoard_CurrentStateInvalidated;

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
        }

        private void MagnifyBoard_CurrentStateInvalidated(object sender, EventArgs e)
        {
        }

        private void MinifyBoard_CurrentStateInvalidated(object sender, EventArgs e)
        {
        }

        private void MagnifyBoard_Completed(object sender, EventArgs e)
        {
            element.HorizontalAlignment = HorizontalAlignment.Stretch;
            element.VerticalAlignment = VerticalAlignment.Stretch;
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
        private void Element_ContextFullScreen(object sender, RoutedEventArgs e)
        {
            if (ParentWindow.ResizeMode != ResizeMode.NoResize)
            {
                ParentWindow.WindowStyle = WindowStyle.None;
                ParentWindow.ResizeMode = ResizeMode.NoResize;
                ParentWindow.WindowState = WindowState.Maximized;
            }
            else
            {
                ParentWindow.ResizeMode = ResizeMode.CanResize;
                ParentWindow.WindowStyle = WindowStyle.ThreeDBorderWindow;
                ParentWindow.WindowState = WindowState.Normal;
            }
        }
        private void Element_MouseMove(object sender, MouseEventArgs e)
        {
            element.Cursor = Cursors.Arrow;
            MouseMoveTimer.Stop();
            MouseMoveTimer.Start();
        }

        bool IsVisionMinified = false;
        private void ChangeVisionState(bool magnified)
        {
            element.Cursor = Cursors.None;
            if (magnified)
            {
                MagnifyAnimations[0].To = ParentWindow.ActualWidth;
                MagnifyAnimations[1].To = ParentWindow.ActualHeight;
                MagnifyBoard.Begin();
            }
            else
            {
                element.HorizontalAlignment = HorizontalAlignment.Left;
                element.VerticalAlignment = VerticalAlignment.Bottom;
                MinifyAnimations[0].From = ParentWindow.ActualWidth;
                MinifyAnimations[1].From = ParentWindow.ActualHeight;
                MinifyBoard.Begin();
            }
        }

        private void Invoke(InfoType type, object obj = null) => EventHappened?.Invoke(this, new InfoExchangeArgs() { Type = type, Object = obj });
        
        TimeSpan TimeSpan;
        Timer PlayCountTimer;
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
        private async void Position_MouseDown(object sender, MouseButtonEventArgs e)
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
        private void PlayModeButton_Click(object sender, MouseButtonEventArgs e)
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
        private async void VolumeButton_MouseDown(object sender, MouseButtonEventArgs e)
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
        private void PlayPauseButton_Click(object sender, MouseButtonEventArgs e)
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
        private void NextButton_Click(object sender, MouseButtonEventArgs e) => Invoke(InfoType.NextRequest);
        private void PreviousButton_Click(object sender, MouseButtonEventArgs e)
        {
            if (PositionSlider.Value > PositionSlider.Maximum / 100 * 10)
                Seek(TimeSpan.Zero);
            else
                Invoke(InfoType.PrevRequest);
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

        public void PlayNext() => NextButton_Click(this, null);
        public void PlayPrevious() => PreviousButton_Click(this, null);
        public void PlayPause() => PlayPauseButton_Click(this, null);
        public void SmallSlideLeft() => Seek((int)PositionSlider.SmallChange * -1);
        public void SmallSlideRight() => Seek((int)PositionSlider.SmallChange);
        public void FullStop()
        {
            element.Stop();
            element.Source = null;
        }
        public void Seek(TimeSpan timeSpan, bool sliding = false)
        {
            if (!sliding) element.Position = timeSpan;
            else element.Position.Add(timeSpan);
        }
        public void Seek(int ms, bool sliding = false) => Seek(new TimeSpan(0, 0, 0, 0, ms), sliding);

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ChangeVisionState(IsVisionMinified = !IsVisionMinified);
        }

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
