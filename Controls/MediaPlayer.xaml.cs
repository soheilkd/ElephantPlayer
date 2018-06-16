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
		public double Volume
		{
			get => element.Volume; set
			{
				element.Volume = value;
				switch (element.Volume)
				{
					case double n when (n <= 0.1): VolumeIcon.Glyph = Glyph.Volume0; break;
					case double n when (n <= 0.4): VolumeIcon.Glyph = Glyph.Volume1; break;
					case double n when (n <= 0.8): VolumeIcon.Glyph = Glyph.Volume2; break;
					default: VolumeIcon.Glyph = Glyph.Volume3; break;
				}
				App.Settings.Volume = element.Volume;
			}
		}
		private Media _Media = new Media();
		public event EventHandler<InfoExchangeArgs> SomethingHappened;
		private Timer DraggerTimer = new Timer(250) { AutoReset = false };
		private Timer MouseMoveTimer = new Timer(App.Settings.MouseOverTimeout) { AutoReset = false };
		private Timer PlayCountTimer = new Timer(120000) { AutoReset = false };
		private TimeSpan timeSpan;
		private TimeSpan TimeSpan
		{
			get => timeSpan;
			set
			{
				timeSpan = value;
				PositionSlider.Maximum = TimeSpan.TotalMilliseconds;
				PositionSlider.SmallChange = 1 * PositionSlider.Maximum / 100;
				PositionSlider.LargeChange = 5 * PositionSlider.Maximum / 100;
				TimeLabel_Full.Content = TimeSpan.ToNewString();
				Invoke(InfoType.LengthFound, TimeSpan);
				SmallChange = new TimeSpan(0, 0, 0, 0, (int)PositionSlider.SmallChange);
				BackwardSmallChange = new TimeSpan(0, 0, 0, 0, -1 * (int)PositionSlider.SmallChange);
			}
		}
		private bool IsFullScreen, WasMaximized;
		public Window ParentWindow;
		public Taskbar.Thumb Thumb = new Taskbar.Thumb();
		private Storyboard MagnifyBoard, MinifyBoard, FullOnBoard, FullOffBoard;
		private ThicknessAnimation MagnifyAnimation, MinifyAnimation;
		public TimeSpan SmallChange, BackwardSmallChange;
		private bool isTopMost;
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
		private bool Magnified = false;

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
			MouseMoveTimer.Elapsed += (_, __) => HideControls();
			PlayCountTimer.Elapsed += PlayCountTimer_Elapsed;
			FullOnBoard.Completed += (_, __) => Cursor = Cursors.None;
			SizeChanged += (_,__) => elementCanvas.Height = Magnified ? Double.NaN : 0;
			FullOffBoard.CurrentStateInvalidated += (_, __) => Cursor = Cursors.Arrow;
			PlayModeButton.Glyph = (Glyph)Enum.Parse(typeof(Glyph), App.Settings.PlayMode.ToString());
			element.MediaEnded += (_, __) => PlayNext();
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			RunUX();
			Volume = App.Settings.Volume;
			App.Settings.Changed += (_, __) => MouseMoveTimer = new Timer(App.Settings.MouseOverTimeout) { AutoReset = false };
			VolumeSlider.Value = Volume * 100;
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
					IsTopMost = !IsTopMost;
			}
			catch (Exception) { }
		}
		private async void Element_MouseMove(object sender, MouseEventArgs e)
		{
			var y = ControlsTranslation.Y;
			await Task.Delay(50);
			if (ControlsTranslation.Y < y)
				return;
			ShowControls();
			MouseMoveTimer.Start();
		}

		bool IsUXChangingPosition = false;
		private async void RunUX()
		{
			UX:
			await Task.Delay(250);
			if (element.NaturalDuration.HasTimeSpan && element.NaturalDuration.TimeSpan != TimeSpan)
				TimeSpan = element.NaturalDuration.TimeSpan;
			TimeLabel_Current.Content = Position.ToNewString();
			IsUXChangingPosition = true;
			PositionSlider.Value = Position.TotalMilliseconds;
			IsUXChangingPosition = false;
			goto UX;
		}

		private void Position_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (!IsUXChangingPosition)
				Position = new TimeSpan(0, 0, 0, 0, (int)PositionSlider.Value);
		}
		private async void Position_Holding(object sender, MouseButtonEventArgs e)
		{
			var but = PlayPauseButton.Glyph;
			element.Pause();
			while (e.ButtonState == MouseButtonState.Pressed)
				await Task.Delay(50);
			if (but == Glyph.Pause)
				element.Play();
			else if (App.Settings.PlayOnPositionChange)
				Play();
		}
		private void PlayPauseButton_Clicked(object sender, MouseButtonEventArgs e)
		{
			if (PlayPauseButton.Glyph == Glyph.Pause)
				Pause();
			else
				Play();
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
			if (Magnified) Minify();
			else Magnify();
		}
		private void FullScreenButton_Clicked(object sender, MouseButtonEventArgs e)
		{
			IsFullScreen = !IsFullScreen;
			ParentWindow.ResizeMode = IsFullScreen ? ResizeMode.NoResize : ResizeMode.CanResize;
			FullScreenButton.Glyph = IsFullScreen ? Glyph.ExitFullScreen : Glyph.FullScreen;
			VisionButton.Visibility = IsFullScreen ? Visibility.Hidden : Visibility.Visible;
			ParentWindow.WindowStyle = IsFullScreen ? WindowStyle.None : WindowStyle.SingleBorderWindow;
			if (IsFullScreen)
			{
				WasMaximized = ParentWindow.WindowState == WindowState.Maximized;
				if (WasMaximized)
					ParentWindow.WindowState = WindowState.Normal;
				ParentWindow.WindowState = WindowState.Maximized;
			}
			else
			{
				ParentWindow.WindowState = WasMaximized ? WindowState.Maximized : WindowState.Normal;
			}
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

		public void Play(Media media)
		{
			media.Load();
			_Media = media;
			VisionButton.Visibility = media.IsVideo ? Visibility.Visible : Visibility.Hidden;
			FullScreenButton.Visibility = VisionButton.Visibility;
			if (IsFullScreen && !media.IsVideo)
				FullScreenButton.EmulateClick();
			if (media.IsVideo && App.Settings.VisionOrientation)
				Magnify();
			else if (Magnified && !media.IsVideo)
				Minify();
			element.Source = media.Url;
			PlayCountTimer.Stop();
			PlayCountTimer.Start();
			TitleLabel.Content = media.ToString();
			Play();
		}

		private void Invoke(InfoType type, object obj = null) => SomethingHappened?.Invoke(this, new InfoExchangeArgs() { Type = type, Object = obj });

		public void Play(bool emulateClick = false)
		{
			if (emulateClick)
			{
				PlayPauseButton.Glyph = Glyph.Play;
				PlayPauseButton.EmulateClick();
			}
			else
			{
				element.Play();
				PlayPauseButton.Glyph = Glyph.Pause;
				Thumb.SetPlayingState(true);
			}
		}
		public void Pause(bool emulateClick = false)
		{
			if (emulateClick)
			{
				PlayPauseButton.Glyph = Glyph.Pause;
				PlayPauseButton.EmulateClick();
			}
			else
			{
				element.Pause();
				PlayPauseButton.Glyph = Glyph.Play;
				Thumb.SetPlayingState(false);
			}
		}
		public void Stop()
		{
			element.Source = null;
		}

		public void Magnify()
		{
			ControlsGrid.VerticalAlignment = VerticalAlignment.Bottom;
			MagnifyAnimation.From = new Thickness(ActualWidth / 2, ActualHeight, ActualWidth / 2, 0);
			elementCanvas.Height = Double.NaN;
			MagnifyBoard.Begin();
			MouseMoveTimer.Start();
			Resources["Foreground"] = Brushes.White;
			Invoke(InfoType.Magnifiement, true);
			Magnified = true;
		}
		public void Minify()
		{
			ControlsGrid.VerticalAlignment = VerticalAlignment.Top;
			MinifyAnimation.To = new Thickness(ActualWidth / 2, ActualHeight, ActualWidth / 2, 0);
			MinifyBoard.Begin();
			Resources["Foreground"] = Brushes.Black;
			Invoke(InfoType.Magnifiement, false);
			Magnified = false;
			IsTopMost = false;
		}
		public void HideControls()
		{
			if (!Magnified)
				return;
			FullOffBoard.Stop();
			Dispatcher.Invoke(() => FullOnBoard.Begin());
		}
		public void ShowControls()
		{
			FullOnBoard.Stop();
			Dispatcher.Invoke(() => FullOffBoard.Begin());
		}
	}
}
