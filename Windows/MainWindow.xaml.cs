using Library.Extensions;
using Library.Hook;
using MahApps.Metro.Controls;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static Player.Controller;

namespace Player
{
	public partial class MainWindow : MetroWindow
	{
		private bool WasMaximized;

		public MainWindow()
		{
			InitializeComponent();
			#region Initialization
			App.NewInstanceRequested += (_, e) => Controller.Library.Add(e.Args);
			Events.KeyDown += KeyboardListener_KeyDown;

			Player.FullScreenToggled += Player_FullScreenClicked;

			Player.MediaChanged += (_, e) => Title = $"Elephant Player | {e.Parameter.Artist} - {e.Parameter.Title}";

			Player.Volume = Settings.Volume;

			#endregion
		}

		private void Player_FullScreenClicked(object sender, EventArgs e)
		{
			IgnoreTaskbarOnMaximize = Player.IsFullScreen;
			ShowTitleBar = !Player.IsFullScreen;
			ShowCloseButton = !Player.IsFullScreen;
			ResizeMode = Player.IsFullScreen ? ResizeMode.NoResize : ResizeMode.CanResize;
			WindowStyle = Player.IsFullScreen ? WindowStyle.None : WindowStyle.SingleBorderWindow;
			UpdateLayout();
			if (Player.IsFullScreen)
			{
				WasMaximized = WindowState == WindowState.Maximized;
				if (WasMaximized)
					WindowState = WindowState.Normal;
				WindowState = WindowState.Maximized;
			}
			else
				WindowState = WasMaximized ? WindowState.Maximized : WindowState.Normal;
		}

		private void KeyboardListener_KeyDown(object sender, RawKeyEventArgs e)
		{
			//Key shortcuts whether window is active or main key is down
			if (IsActive || e.Key.HasFlag(Key.LeftShift))
			{
				if (e.Key == Key.Left) Player.SlidePosition(FlowDirection.RightToLeft);
				if (e.Key == Key.Right) Player.SlidePosition(FlowDirection.LeftToRight);
			}
			//Key shortcuts always invokable
			if (e.Key == Key.MediaPlayPause) Player.PlayPause();
			if (e.Key == Key.MediaNextTrack) Player.Next();
			if (e.Key == Key.MediaPreviousTrack) Player.Previous();
		}

		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
			TaskbarItemInfo = new System.Windows.Shell.TaskbarItemInfo();
			Player.Thumb = new ThumbController(TaskbarItemInfo, Player);
			while (!Player.IsFullyLoaded)
				await Task.Delay(10);
			Play(Environment.GetCommandLineArgs());
		}
		private void Window_Closing(object sender, CancelEventArgs e)
		{
			Settings.LastSize = new Size(Width, Height);
			Settings.Volume = Player.Volume;
			SaveAll();
			Application.Current.Shutdown();
		}
		private void Window_Drop(object sender, DragEventArgs e)
		{
			Controller.Library.Add(e.Data.GetData(DataFormats.FileDrop).As<string[]>());
		}
	}
}
