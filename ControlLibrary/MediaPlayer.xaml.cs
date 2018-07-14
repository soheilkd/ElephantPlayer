using MaterialDesignThemes.Wpf;
using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Player.Controls
{
	public partial class MediaPlayer : UserControl
	{
		public static DependencyProperty IsMagnifiedProperty =
			DependencyProperty.Register(nameof(IsMagnified), typeof(bool), typeof(MediaPlayer), new PropertyMetadata(false));
		public static DependencyProperty AreControlsVisibleProperty =
			DependencyProperty.Register(nameof(AreControlsVisible), typeof(bool), typeof(MediaPlayer), new PropertyMetadata(true));
		public static DependencyProperty IsFullScreenProperty =
			DependencyProperty.Register(nameof(IsFullScreen), typeof(bool), typeof(MediaPlayer), new PropertyMetadata(false));

		public event EventHandler LengthFound;
		public event EventHandler PlayCounterElapsed;
		public event EventHandler NextRequest;
		public event EventHandler PreviousRequest;
		public event EventHandler<DependencyPropertyChangedEventArgs> IsMagnifiedChange;

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
					case double n when (n <= 0.1): VolumeIcon.Kind = PackIconKind.VolumeOff;  break;
					case double n when (n <= 0.4): VolumeIcon.Kind = PackIconKind.VolumeLow; break;
					case double n when (n <= 0.7): VolumeIcon.Kind = PackIconKind.VolumeMedium; break;
					default: VolumeIcon.Kind = PackIconKind.VolumeHigh; break;
				}
			}
		}

		public void ChangeMouseMoveTimer(double interval) => MouseMoveTimer = new Timer(interval) { AutoReset = false };
		private Timer DraggerTimer = new Timer(250) { AutoReset = false };
		private Timer MouseMoveTimer = new Timer(5000);
		private Timer PlayCountTimer = new Timer(120000) { AutoReset = false };
		private TimeSpan timeSpan;
		public TimeSpan Length
		{
			get => timeSpan;
			set
			{
				timeSpan = value;
				PositionSlider.Maximum = Length.TotalMilliseconds;
				PositionSlider.SmallChange = 1 * PositionSlider.Maximum / 100;
				PositionSlider.LargeChange = 5 * PositionSlider.Maximum / 100;
				TimeLabel_Full.Content = Length.ToNewString();
				LengthFound?.Invoke(this, null);
			}
		}
		private bool WasMaximized, isTopMost, IsUXChangingPosition;
		public bool IsFullyLoaded;
		public bool IsFullScreen
		{
			get => (bool)GetValue(IsFullScreenProperty);
			set
			{
				SetValue(IsFullScreenProperty, value);
				ParentWindow.ResizeMode = value ? ResizeMode.NoResize : ResizeMode.CanResize;
				FullScreenButton.Icon = value ? IconKind.FullscreenExit : IconKind.Fullscreen;
				VisionButton.Visibility = value ? Visibility.Hidden : Visibility.Visible;
				ParentWindow.WindowStyle = value ? WindowStyle.None : WindowStyle.SingleBorderWindow;
				if (value)
				{
					WasMaximized = ParentWindow.WindowState == WindowState.Maximized;
					if (WasMaximized)
						ParentWindow.WindowState = WindowState.Normal;
					ParentWindow.WindowState = WindowState.Maximized;
				}
				else
					ParentWindow.WindowState = WasMaximized ? WindowState.Maximized : WindowState.Normal;
			}
		}
		public bool IsTopMost
		{
			get => isTopMost;
			set
			{
				isTopMost = value;
				ParentWindow.Topmost = value;
				ParentWindow.WindowStyle = value ? WindowStyle.None : WindowStyle.SingleBorderWindow;
			}
		}
		public bool IsMagnified
		{
			get => (bool)GetValue(IsMagnifiedProperty);
			set
			{
				IsMagnifiedChange?.Invoke(this, new DependencyPropertyChangedEventArgs(IsMagnifiedProperty, IsMagnified, value));
				SetValue(IsMagnifiedProperty, value);
				if (value)
				{
					MagnifyAnimation.From = new Thickness(ActualWidth / 2, ActualHeight, ActualWidth / 2, 0);
					elementCanvas.Height = Double.NaN;
					MagnifyBoard.Begin();
					MouseMoveTimer.Start();
				}
				else
				{
					MinifyAnimation.To = new Thickness(ActualWidth / 2, ActualHeight, ActualWidth / 2, 0);
					MinifyBoard.Begin();
					IsTopMost = false;
				}
			}
		}
		public bool AreControlsVisible
		{
			get => (bool)GetValue(AreControlsVisibleProperty);
			set
			{
				Dispatcher.Invoke(() =>
				{
					SetValue(AreControlsVisibleProperty, value);
					if (value)
					{
						FullOnBoard.Stop();
						FullOffBoard.Begin();
					}
					else
					{
						if (!IsMagnified || ControlsGrid.IsMouseOver)
							return;
						FullOffBoard.Stop();
						FullOnBoard.Begin();
					}
				});
			}
		}
		public bool PlayOnPositionChange { get; set; }
		public bool AutoOrinateVision { get; set; }
		public Window ParentWindow;
		public Taskbar.Thumb Thumb = new Taskbar.Thumb();
		private Storyboard MagnifyBoard, MinifyBoard, FullOnBoard, FullOffBoard;
		private ThicknessAnimation MagnifyAnimation, MinifyAnimation;

		public MediaPlayer()
		{
			InitializeComponent();
			MagnifyBoard = Resources["MagnifyBoard"] as Storyboard;
			MinifyBoard = Resources["MinifyBoard"] as Storyboard;
			FullOnBoard = Resources["FullOnBoard"] as Storyboard;
			FullOffBoard = Resources["FullOffBoard"] as Storyboard;
			MagnifyAnimation = MagnifyBoard.Children[0] as ThicknessAnimation;
			MinifyAnimation = MinifyBoard.Children[0] as ThicknessAnimation;

			Thumb.NextPressed += (obj, f) => Next();
			Thumb.PausePressed += (obj, f) => PlayPause();
			Thumb.PlayPressed += (obj, f) => PlayPause();
			Thumb.PrevPressed += (obj, f) => Previous();
			MouseMoveTimer.Elapsed += (_, __) => AreControlsVisible = false;
			PlayCountTimer.Elapsed += PlayCountTimer_Elapsed;
			FullOnBoard.Completed += (_, __) => Cursor = Cursors.None;
			SizeChanged += (_,__) => elementCanvas.Height = IsMagnified ? Double.NaN : 0;
			FullOffBoard.CurrentStateInvalidated += (_, __) => Cursor = Cursors.Arrow;
			element.MediaEnded += (_, __) => Next();
			element.MediaOpened += Element_MediaOpened;
		}

		private void Element_MediaOpened(object sender, RoutedEventArgs e)
		{
			bool isVideo = element.HasVideo;
			VisionButton.Visibility = isVideo ? Visibility.Visible : Visibility.Hidden;
			FullScreenButton.Visibility = VisionButton.Visibility;
			if (IsFullScreen && !isVideo)
				FullScreenButton.EmulateClick();
			IsMagnified = isVideo && AutoOrinateVision;
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

		private void Element_MouseDown(object sender, MouseButtonEventArgs e)
		{
			DraggerTimer.Start();
			try
			{
				if (ParentWindow.WindowState != WindowState.Maximized)
					ParentWindow.DragMove();
				if (DraggerTimer.Enabled && !IsFullScreen)
					IsTopMost = !IsTopMost;
			}
			catch { }
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
			if (element.NaturalDuration.HasTimeSpan && element.NaturalDuration.TimeSpan != Length)
				Length = element.NaturalDuration.TimeSpan;
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
			if (but == IconKind.Pause)
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
			if (PlayPauseButton.Icon == IconKind.Pause)
				Pause();
			else
				Play();
		}
		private void NextButton_Clicked(object sender, MouseButtonEventArgs e)
		{
			NextRequest?.Invoke(this, null);
		}
		private void PreviousButton_Clicked(object sender, MouseButtonEventArgs e)
		{
			if (PositionSlider.Value > PositionSlider.Maximum / 100 * 10)
			{
				element.Stop();
				element.Play();
			}
			else
				PreviousRequest?.Invoke(this, null);
		}
		private void FullScreenButton_Clicked(object sender, MouseButtonEventArgs e)
		{
			IsFullScreen = !IsFullScreen;
		}
		private void VisionButton_Clicked(object sender, MouseButtonEventArgs e)
		{
			IsMagnified = !IsMagnified;
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
				PlayPauseButton.Icon = IconKind.Play;
				PlayPauseButton.EmulateClick();
			}
			else
			{
				element.Play();
				PlayPauseButton.Icon = IconKind.Pause;
				Thumb.SetPlayingState(true);
			}
		}
		public void Pause(bool emulateClick = false)
		{
			if (emulateClick)
			{
				PlayPauseButton.Icon = IconKind.Pause;
				PlayPauseButton.EmulateClick();
			}
			else
			{
				element.Pause();
				PlayPauseButton.Icon = IconKind.Play;
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
