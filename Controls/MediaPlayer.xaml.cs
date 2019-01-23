using Library;
using Library.Controls;
using Library.Extensions;
using Player.Models;
using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using static Player.Controller;

namespace Player.Controls
{
	public partial class MediaPlayer : UserControl
	{
		public event InfoExchangeHandler<Media> MediaChanged;
		public event InfoExchangeHandler<bool> VisionChanged;
		public event EventHandler FullScreenToggled;

		public ThumbController Thumb { get; set; } = default;

		public Media Current => Queue.Current;

		public double Volume
		{
			get => Element.Volume;
			set
			{
				Element.Volume = value;
				switch (Element.Volume)
				{
					case double n when (n <= 0.1): VolumeIcon.Type = IconType.Volume0; break;
					case double n when (n <= 0.6): VolumeIcon.Type = IconType.Volume1; break;
					default: VolumeIcon.Type = IconType.Volume2; break;
				}
			}
		}

		private Timer PlayCountTimer = new Timer(60000) { AutoReset = false };
		private Timer MouseMoveTimer = new Timer(5000);

		private TimeSpan _MediaTimeSpan;
		private TimeSpan MediaTimeSpan
		{
			get => _MediaTimeSpan;
			set
			{
				_MediaTimeSpan = value;
				PositionSlider.Maximum = value.TotalMilliseconds;
				TimeLabel_Full.Content = value.ToNewString();
				PositionSlider.SmallChange = 1 * PositionSlider.Maximum / 100;
				PositionSlider.LargeChange = 5 * PositionSlider.Maximum / 100;
				Queue.Current.Length = value;
			}
		}
		private bool IsUXChangingPosition;
		public bool IsFullyLoaded;
		public bool IsFullScreen => FullScreenButton.Icon == IconType.FullScreenExit;
		public bool IsVisionOn { get; set; }
		private bool IsControlBarVisible
		{
			set
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
			}
		}
		private Storyboard VisionOnBoard, VisionOffBoard, FullOnBoard, FullOffBoard;

		public MediaPlayer()
		{
			InitializeComponent();
			PlayRequest += (_, e) => Play(e.Parameter.Item1, e.Parameter.Item2);
			IsVisionOn = false;
			VisionOnBoard = Resources["VisionOnBoard"] as Storyboard;
			VisionOffBoard = Resources["VisionOffBoard"] as Storyboard;
			FullOnBoard = Resources["FullOnBoard"] as Storyboard;
			FullOffBoard = Resources["FullOffBoard"] as Storyboard;

			MouseMoveTimer.Elapsed += (_, __) => IsControlBarVisible = false;
			PlayCountTimer.Elapsed += PlayCountTimer_Elapsed;
			FullOnBoard.Completed += (_, __) => Cursor = Cursors.None;
			FullOffBoard.CurrentStateInvalidated += (_, __) => Cursor = Cursors.Arrow;
			Element.MediaEnded += (_, __) => Next();
			VisionOffBoard.Completed += delegate { ElementCanvas.Visibility = Visibility.Hidden; };
			VisionOnBoard.CurrentStateInvalidated += delegate { ElementCanvas.Visibility = Visibility.Visible; };
		}

		private void Element_MediaOpened(object sender, RoutedEventArgs e)
		{
			ResetCountTimer();
			var isVideo = Current.IsVideo;
			FullScreenButton.Visibility = isVideo ? Visibility.Visible : Visibility.Hidden;
			VisionButton.Visibility = isVideo ? Visibility.Visible : Visibility.Hidden;
			ArtworkImage.Visibility = isVideo ? Visibility.Hidden : Visibility.Visible;
			if (IsFullScreen && !isVideo)
				FullScreenButton.EmulateClick();
			if (isVideo != IsVisionOn)
				VisionButton.EmulateClick();
		}

		private void ResetCountTimer()
		{
			PlayCountTimer.Stop();
			PlayCountTimer.Start();
		}
		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			RunUX();
			IsFullyLoaded = true;
		}

		private void PlayCountTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			Queue.Current.PlayTimes.Add(DateTime.Now);
			PlayCountTimer.Stop();
		}

		private async void Element_MouseMove(object sender, MouseEventArgs e)
		{
			var y = ControlsTranslation.Y;
			await Task.Delay(50);
			if (ControlsTranslation.Y < y)
				return;
			IsControlBarVisible = true;
			MouseMoveTimer.Start();
		}

		private async void RunUX()
		{
			while (true)
			{
				await Task.Delay(250);
				if (Element.NaturalDuration.HasTimeSpan && Element.NaturalDuration.TimeSpan != MediaTimeSpan)
					MediaTimeSpan = Element.NaturalDuration.TimeSpan;
				TimeLabel_Current.Content = Element.Position.ToNewString();
				IsUXChangingPosition = true;
				PositionSlider.Value = Element.Position.TotalMilliseconds;
				IsUXChangingPosition = false;
			}
		}

		private async void Position_Holding(object sender, MouseButtonEventArgs e)
		{
			IconType but = PlayPauseButton.Icon;
			Element.Pause();
			while (e.ButtonState == MouseButtonState.Pressed)
				await Task.Delay(50);
			if (but == IconType.Pause)
				Element.Play();
			else if (Settings.PlayOnPositionChange)
				Play();
		}
		private void Position_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!IsUXChangingPosition)
				Element.Position = new TimeSpan(0, 0, 0, 0, (int)PositionSlider.Value);
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
			Play(Queue.Next());
		}
		private void Play(Media media)
		{
			Queue.ClearIsPlayings(except: media);
			Element.Source = new Uri(media.Path);
			MediaChanged.Invoke(media);
			Play();
			if (!media.IsVideo)
				ArtworkImage.Source = media.Artwork;
		}
		public void Play(MediaQueue queue, Media media)
		{
			Queue.ClearIsPlayings();
			Queue = queue;
			Play(media);
		}
		private void PreviousButton_Clicked(object sender, MouseButtonEventArgs e)
		{
			if (PositionSlider.Value > PositionSlider.Maximum / 100 * 10)
			{
				Element.Position = TimeSpan.Zero;
				ResetCountTimer();
			}
			else
				Play(Queue.Previous());
		}
		private void FullScreenButton_Clicked(object sender, MouseButtonEventArgs e)
		{
			FullScreenButton.Icon = FullScreenButton.Icon == IconType.FullScreen ? IconType.FullScreenExit : IconType.FullScreen;
			FullScreenToggled?.Invoke(this, default);
		}
		private void VolumeSlider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			Volume = VolumeSlider.Value / 100;
		}

		public void Play()
		{
			Element.Play();
			PlayPauseButton.Icon = IconType.Pause;
			Thumb.SetPlayingStateOnThumb(true);
		}
		public void Pause()
		{
			Element.Pause();
			PlayPauseButton.Icon = IconType.Play;
			Thumb.SetPlayingStateOnThumb(false);
		}

		public void SlidePosition(FlowDirection direction, bool small = true)
		{
			if (direction == FlowDirection.LeftToRight) PositionSlider.Value += small ? PositionSlider.SmallChange : PositionSlider.LargeChange;
			else PositionSlider.Value -= small ? PositionSlider.SmallChange : PositionSlider.LargeChange;
		}

		private void VisionButton_Clicked(object sender, MouseButtonEventArgs e)
		{
			IsVisionOn = !IsVisionOn;
			if (IsVisionOn)
				VisionOnBoard.Begin();
			else
				VisionOffBoard.Begin();
			VisionChanged.Invoke(IsVisionOn);
			IsControlBarVisible = true;
		}

		public void Next() => NextButton.EmulateClick();
		public void Previous() => PreviousButton.EmulateClick();

		private void Vision_Click(object sender, MouseButtonEventArgs e)
		{
			if (IsVisionOn = !IsVisionOn)
				VisionOnBoard.Begin();
			else
				VisionOffBoard.Begin();
		}

		public void PlayPause() => PlayPauseButton.EmulateClick();
	}
}
