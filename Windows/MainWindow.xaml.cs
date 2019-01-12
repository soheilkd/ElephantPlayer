using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Library.Extensions;
using Library.Hook;
using MahApps.Metro.Controls;
using Player.Models;
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

			DataGrid.ItemsSource = Controller.Library;

			Resources["LastPath"] = Settings.LastPath;

			Player.VisionChanged += Player_VisionChanged;
			Player.MediaChanged += (_, e) => Title = $"{(Topmost ? "" : "Elephant Player | ")}{e.Parameter.Artist} - {e.Parameter.Title}";

			Player.BorderBack = Background;
			Player.ChangeVolumeBySlider(Settings.Volume * 100);
			Player.Volume = Settings.Volume;

			foreach (MenuItem item in this.FindChildren<MenuItem>())
				item.Background = Menu.Background;
			foreach (MenuItem item in DataGrid.ContextMenu.Items)
				item.Background = Menu.Background;

			#endregion

			Left = Settings.LastLocation.X;
			Top = Settings.LastLocation.Y;
		}

		private void Player_VisionChanged(object sender, Library.InfoExchangeArgs<bool> e)
		{
			//Hide the controls which is not needed when vision is on, or make them visible if vision is going to hide
			SearchButton.Visibility = e.Parameter ? Visibility.Hidden : Visibility.Visible;
			SearchLabel.Visibility = e.Parameter ? Visibility.Hidden : Visibility.Visible;
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

		private async void KeyboardListener_KeyDown(object sender, RawKeyEventArgs e)
		{
			if (IsActive && SearchBox.IsFocused)
				return;
			//Key shortcuts when window is active and main key is down
			if (IsActive && e.Key.HasFlag(Key.LeftShift))
			{
				if (e.Key == Key.F)
				{
					SearchBox.IsEnabled = false;
					SearchBox.Text = "";
					SearchButton.EmulateClick();
					await Task.Delay(100);
					SearchBox.IsEnabled = true;
					SearchBox.Focus();
				}
			}
			//Key shortcuts whether window is active or main key is down
			if (IsActive || e.Key.HasFlag(Key.LeftShift))
			{
				if (e.Key == Key.Left) Player.SlidePosition(false);
				if (e.Key == Key.Right) Player.SlidePosition(true);
			}
			//Key shortcuts always invokable
			if (e.Key == Key.MediaPlayPause) Player.PlayPause();
			if (e.Key == Key.MediaNextTrack) Player.Next();
			if (e.Key == Key.MediaPreviousTrack) Player.Previous();
		}

		private void List_DoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (DataGrid.SelectedItem is Media med)
				Player.Play(Controller.Library, med);
		}

		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
			TaskbarItemInfo = new System.Windows.Shell.TaskbarItemInfo();
			Player.Thumb = new ThumbController(TaskbarItemInfo, Player);
			while (!Player.IsFullyLoaded)
				await Task.Delay(10);
			
			Controller.Play(Environment.GetCommandLineArgs());
		}
		private void Window_Closing(object sender, CancelEventArgs e)
		{
            Settings.LastSize = new Size(Width, Height);
			Settings.LastLocation = new Point(Left, Top);
			Settings.Volume = Player.Volume;
			SaveAll();
			Application.Current.Shutdown();
		}
		private void Window_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Space && !SearchBox.IsFocused)
				Player.PlayPause();
		}
		private void Window_Drop(object sender, DragEventArgs e)
		{
			((string[])e.Data.GetData(DataFormats.FileDrop)).For(each => Controller.Library.Add(each));
		}

		private void Play(MediaQueue queue, Media media, bool inc)
		{
			Player.Play(queue, media);
		}
		
		private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
		{
            DataGrid.ItemsSource = Controller.Library.Search(SearchBox.Text);
		}
		private void SearchIcon_Click(object sender, MouseButtonEventArgs e)
		{
			SearchPopup.IsOpen = true;
			SearchBox.Focus();
		}
	}
}
