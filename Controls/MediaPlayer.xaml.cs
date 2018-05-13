using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace Player.Controls
{
    /// <summary>
    /// Interaction logic for MediaControl.xaml
    /// </summary>
    public partial class MediaPlayer : UserControl
    {
        private Timer DraggerTimer = new Timer(250) { AutoReset = false };
        private Timer MouseMoveTimer = new Timer(5000) { AutoReset = false };
        public MediaControl()
        {
            InitializeComponent();
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

        private void UI_PlayPauseButton_Click(object sender, MouseButtonEventArgs e)
        {
            if (PlayPauseButton.Glyph == Glyph.Pause)
            {
                Player.Pause();
                PlayPauseButton.Icon = Glyph.Play;
                Thumb.Refresh(false);
            }
            else
            {
                Player.Play();
                PlayPauseButton.Icon = Glyph.Pause;
                Thumb.Refresh(true);
            }
        }
        private void UI_NextButton_Click(object sender, MouseButtonEventArgs e) => Play(Manager.Next());
        private void UI_PreviousButton_Click(object sender, MouseButtonEventArgs e)
        {
            if (PositionSlider.Value > PositionSlider.Maximum / 100 * 10)
                ChangePosition(0, true);
            else
                Play(Manager.Previous());
        }
        private void UI_VisionButton_Click(object sender, MouseButtonEventArgs e)
        {
            WindowSizes[IsVisionOn[0] ? 1 : 0].Width = ActualWidth;
            IsVisionOn[0] = !IsVisionOn[0];
            BeginStoryboard(Resources[IsVisionOn[0] ? "VisionOnBoard" : "VisionOffBoard"] as Storyboard);
            if (!IsVisionOn[0]) { Height--; Height++; }
            (WindowWidthBoard.Children[0] as DoubleAnimation).From = ActualWidth;
            (WindowWidthBoard.Children[0] as DoubleAnimation).To = WindowSizes[IsVisionOn[0] ? 1 : 0].Width;
            WindowWidthBoard.Begin();
        }
        private void UI_PlayModeButton_Click(object sender, MouseButtonEventArgs e)
        {
            switch (Manager.ActivePlayMode)
            {
                case PlayMode.Shuffle:
                    PlayModeButton.Glyph = Glyph.RepeatOne;
                    Manager.ActivePlayMode = PlayMode.RepeatOne;
                    break;
                case PlayMode.RepeatOne:
                    PlayModeButton.Icon = Glyph.RepeatAll;
                    Manager.ActivePlayMode = PlayMode.RepeatAll;
                    break;
                case PlayMode.RepeatAll:
                    PlayModeButton.Icon = Glyph.GroupList;
                    Manager.ActivePlayMode = PlayMode.Queue;
                    break;
                case PlayMode.Queue:
                    PlayModeButton.Icon = Glyph.Shuffle;
                    Manager.ActivePlayMode = PlayMode.Shuffle;
                    break;
                default:
                    break;
            }
            App.Preferences.PlayMode = (int)Manager.ActivePlayMode;
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
            while (e.RightButton == MouseButtonState.Pressed)
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
                case double n when (n < 0.1): VolumeButton.Icon = Glyph.Volume0; break;
                case double n when (n < 0.4): VolumeButton.Icon = Glyph.Volume1; break;
                case double n when (n < 0.8): VolumeButton.Icon = Glyph.Volume2; break;
                default: VolumeButton.Icon = Glyph.Volume3; break;
            }
        }
    }
}
