﻿using System;
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
		public event EventHandler<InfoExchangeArgs<TimeSpan>> LengthFound;
		public event EventHandler PlayCounterElapsed;
		public event EventHandler NextClicked;
		public event EventHandler PreviousClicked;
		public event EventHandler FullScreenClicked;
		public event EventHandler<InfoExchangeArgs<bool>> VisionChanged;

		private Brush _BorderBack = Brushes.White;
		public Brush BorderBack
		{
			get => _BorderBack;
			set
			{
				var r = (SolidColorBrush)value;
				_BorderBack = new SolidColorBrush(new Color()
				{
					R = r.Color.R,
					G = r.Color.G,
					B = r.Color.B,
					A = 255
				});
			}
		}

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
		public double Volume
		{
			get => element.Volume; set
			{
				element.Volume = value;
				switch (element.Volume)
				{
					case double n when (n <= 0.1): VolumeIcon.Type = IconType.Volume0; break;
					case double n when (n <= 0.4): VolumeIcon.Type = IconType.Volume1; break;
					case double n when (n <= 0.7): VolumeIcon.Type = IconType.Volume2; break;
					default: VolumeIcon.Type = IconType.Volume3; break;
				}
			}
		}

		public Taskbar.Thumb Thumb = new Taskbar.Thumb();
		private Timer MouseMoveTimer = new Timer(5000);
		public double MouseMoveInterval
		{
			get => MouseMoveTimer.Interval;
			set => MouseMoveTimer.Interval = value;
		}
		private Timer PlayCountTimer = new Timer(60000) { AutoReset = false };
		private TimeSpan _MediaTimeSpan;
		private TimeSpan MediaTimeSpan
		{
			get => _MediaTimeSpan;
			set
			{
				_MediaTimeSpan = value;
				PositionSlider.Maximum = MediaTimeSpan.TotalMilliseconds;
				PositionSlider.SmallChange = 1 * PositionSlider.Maximum / 100;
				PositionSlider.LargeChange = 5 * PositionSlider.Maximum / 100;
				TimeLabel_Full.Content = MediaTimeSpan.ToNewString();
				LengthFound?.Invoke(this, new InfoExchangeArgs<TimeSpan>(value));
			}
		}
		private bool IsUXChangingPosition;
		public bool IsFullyLoaded;
		public bool IsFullScreen => FullScreenButton.Icon == IconType.BackToWindow;
		public bool IsVisionOn { get; set; }
		private bool AreControlsVisible
		{
			set
			{
				Dispatcher.Invoke(() =>
				{
					if (value)
					{
						FullOnBoard.Stop();
						FullOffBoard.Begin();
					}
					else
					{
						if (!IsVisionOn || ControlsGrid.IsMouseOver)
							return;
						FullOffBoard.Stop();
						FullOnBoard.Begin();
					}
				});
			}
		}
		public bool PlayOnPositionChange { get; set; }
		public bool AutoOrinateVision { get; set; } = true;
		private Storyboard MagnifyBoard, MinifyBoard, FullOnBoard, FullOffBoard;

		public MediaPlayer()
		{
			InitializeComponent();
			MagnifyBoard = Resources["MagnifyBoard"] as Storyboard;
			MinifyBoard = Resources["MinifyBoard"] as Storyboard;
			FullOnBoard = Resources["FullOnBoard"] as Storyboard;
			FullOffBoard = Resources["FullOffBoard"] as Storyboard;

			Thumb.NextClicked += (obj, f) => Next();
			Thumb.PauseClicked += (obj, f) => PlayPause();
			Thumb.PlayClicked += (obj, f) => PlayPause();
			Thumb.PrevClicked += (obj, f) => Previous();
			MouseMoveTimer.Elapsed += (_, __) => AreControlsVisible = false;
			PlayCountTimer.Elapsed += PlayCountTimer_Elapsed;
			FullOnBoard.Completed += (_, __) => Cursor = Cursors.None;
			FullOffBoard.CurrentStateInvalidated += (_, __) => Cursor = Cursors.Arrow;
			element.MediaEnded += (_, __) => Next();
			element.MediaOpened += Element_MediaOpened;
			MagnifyBoard.CurrentStateInvalidated += (_, __) => elementCanvas.Visibility = Visibility.Visible;
			MinifyBoard.Completed += (_, __) => elementCanvas.Visibility = Visibility.Hidden;
			elementCanvas.Visibility = Visibility.Hidden;
			elementCanvas.Opacity = 0;
			Resources["BorderBack"] = Brushes.Transparent;
		}

		private void Element_MediaOpened(object sender, RoutedEventArgs e)
		{
			bool isVideo = element.HasVideo;
			VisionButton.Visibility = isVideo ? Visibility.Visible : Visibility.Hidden;
			FullScreenButton.Visibility = VisionButton.Visibility;
			if (IsFullScreen && !isVideo)
				FullScreenButton.EmulateClick();
			//Next seems not so readable, it just checks if AutoOrientation is on, check proper conditions where operation is needed
			if ((AutoOrinateVision && (isVideo && !IsVisionOn)) || (!isVideo && IsVisionOn))
				VisionButton.EmulateClick();
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			RunUX();
			IsFullyLoaded = true;
		}

		private void PlayCountTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			PlayCounterElapsed?.Invoke(this, null);
			PlayCountTimer.Stop();
		}
		
		private async void Element_MouseMove(object sender, MouseEventArgs e)
		{
			var y = ControlsTranslation.Y;
			await Task.Delay(50);
			if (ControlsTranslation.Y < y)
				return;
			AreControlsVisible = true;
			MouseMoveTimer.Start();
		}
		
		private async void RunUX()
		{
			UX:
			await Task.Delay(250);
			if (element.NaturalDuration.HasTimeSpan && element.NaturalDuration.TimeSpan != MediaTimeSpan)
				MediaTimeSpan = element.NaturalDuration.TimeSpan;
			TimeLabel_Current.Content = Position.ToNewString();
			IsUXChangingPosition = true;
			PositionSlider.Value = Position.TotalMilliseconds;
			IsUXChangingPosition = false;
			goto UX;
		}

		private async void Position_Holding(object sender, MouseButtonEventArgs e)
		{
			var but = PlayPauseButton.Icon;
			element.Pause();
			while (e.ButtonState == MouseButtonState.Pressed)
				await Task.Delay(50);
			if (but == IconType.Pause)
				element.Play();
			else if (PlayOnPositionChange)
				Play();
		}
		private void Position_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!IsUXChangingPosition)
				Position = new TimeSpan(0, 0, 0, 0, (int)PositionSlider.Value);
		}
		private void PlayPauseButton_Clicked(object sender, MouseButtonEventArgs e)
		{
			if (PlayPauseButton.Icon == IconType.Pause)
				Pause();
			else
				Play();
		}
		private void NextButton_Clicked(object sender, MouseButtonEventArgs e)
		{
			NextClicked?.Invoke(this, null);
		}
		private void PreviousButton_Clicked(object sender, MouseButtonEventArgs e)
		{
			if (PositionSlider.Value > PositionSlider.Maximum / 100 * 10)
			{
				element.Stop();
				element.Play();
			}
			else
				PreviousClicked?.Invoke(this, null);
		}
		private void FullScreenButton_Clicked(object sender, MouseButtonEventArgs e)
		{
			FullScreenButton.Icon = FullScreenButton.Icon == IconType.FullScreen ? IconType.BackToWindow : IconType.FullScreen;
			VisionButton.Visibility = IsFullScreen ? Visibility.Hidden : Visibility.Visible;
			FullScreenClicked?.Invoke(this, null);
		}
		private void VisionButton_Clicked(object sender, MouseButtonEventArgs e)
		{
			IsVisionOn = !IsVisionOn;
			VisionButton.Icon = IsVisionOn ? IconType.PresenceChickletVideo : IconType.Video;
			if (IsVisionOn) MagnifyBoard.Begin();
			else MinifyBoard.Begin();
			VisionChanged?.Invoke(this, new InfoExchangeArgs<bool>(IsVisionOn));
			Resources["BorderBack"] = IsVisionOn ? BorderBack : Brushes.Transparent;
			AreControlsVisible = true;
		}
		private void VolumeSlider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			Volume = VolumeSlider.Value / 100;
		}

		public void Play(Uri source)
		{
			element.Source = source;
			PlayCountTimer.Stop();
			PlayCountTimer.Start();
			Play();
		}

		public void Play(bool emulateClick = false)
		{
			if (emulateClick)
			{
				PlayPauseButton.Icon = IconType.Play;
				PlayPauseButton.EmulateClick();
			}
			else
			{
				element.Play();
				PlayPauseButton.Icon = IconType.Pause;
				Thumb.SetPlayingState(true);
			}
		}
		public void Pause(bool emulateClick = false)
		{
			if (emulateClick)
			{
				PlayPauseButton.Icon = IconType.Pause;
				PlayPauseButton.EmulateClick();
			}
			else
			{
				element.Pause();
				PlayPauseButton.Icon = IconType.Play;
				Thumb.SetPlayingState(false);
			}
		}
		public void SlidePosition(bool toRight, bool small = true)
		{
			if (toRight) PositionSlider.Value += small ? PositionSlider.SmallChange : PositionSlider.LargeChange;
			else PositionSlider.Value -= small ? PositionSlider.SmallChange : PositionSlider.LargeChange;
		}
		public void Stop()
		{
			element.Stop();
			element.Source = null;
		}
		public void Next() => NextButton.EmulateClick();
		public void Previous() => PreviousButton.EmulateClick();
		public void PlayPause() => PlayPauseButton.EmulateClick();
	}
}