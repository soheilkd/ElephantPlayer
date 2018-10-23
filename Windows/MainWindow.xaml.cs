using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Library.Controls;
using Library.Extensions;
using Library.Hook;
using MahApps.Metro.Controls;
using Player.Controllers;
using Player.Models;
using Player.Views;

namespace Player
{
	public partial class MainWindow : MetroWindow
	{
		private const int HeightOnMinimal = 112;
		private const int WidthOnMinimal = 300;
		private double TempHeight
		{
			get => (double)Resources["MinimalHeightTemp"];
			set => Resources["MinimalHeightTemp"] = value;
		}
		private double TempWidth
		{
			get => (double)Resources["MinimalWidthTemp"];
			set => Resources["MinimalWidthTemp"] = value;
		}

		private Visibility ControlsNotNeededOnVisionVisibility
		{
			set
			{
				SearchButton.Visibility = value;
				SearchLabel.Visibility = value;
				MinimalViewButton.Visibility = value;
			}
		}
		private bool WasMaximized, WasMinimal;

		public MainWindow()
		{
			InitializeComponent();
			#region Initialization
			App.NewInstanceRequested += (_, e) => e.Args.For(each => LibraryManager.AddFromPath(each, true));
			Events.KeyDown += KeyboardListener_KeyDown;

			LibraryManager.MediaRequested += (_, e) => Player.Play(LibraryManager.Data, e.Parameter);
			Player.FullScreenToggled += Player_FullScreenClicked;
			Player.UpdateLayout();

			DataGrid.ItemsSource = LibraryManager.Data;

			TaskbarItemInfo = Player.Thumb.Info;
			Resources["LastPath"] = Settings.LastPath;

			Player.VisionChanged += (_, e) => ControlsNotNeededOnVisionVisibility = e.Parameter ? Visibility.Hidden : Visibility.Visible;
			Player.MediaChanged += (_, e) => Title = $"{(Topmost ? "" : "Elephant Player | ")}{e.Parameter.Artist} - {e.Parameter.Title}";
			Player.AutoOrinateVision = Settings.VisionOrientation;
			Player.PlayOnPositionChange = Settings.PlayOnPositionChange;

			Player.BorderBack = Background;
			Player.ChangeVolumeBySlider(Settings.Volume * 100);
			Player.Volume = Settings.Volume;

			foreach (MenuItem item in this.FindChildren<MenuItem>())
				item.Background = Menu.Background;
			foreach (MenuItem item in DataGrid.ContextMenu.Items)
				item.Background = Menu.Background;
			ArtistsView.PlayRequested += (_, e) => Player.Play(e.Queue, e.Media);
			AlbumsView.PlayRequested += (_, e) => Player.Play(e.Queue, e.Media);
			#endregion

			Left = Settings.LastLocation.X;
			Top = Settings.LastLocation.Y;
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

		private void MinimalButton_Clicked(object sender, MouseButtonEventArgs e)
		{
			Hide();
			Topmost = !Topmost;
			Player.IsMinimal = Topmost;
			Title = $"{(Topmost ? "" : "Elephant Player | ")}{Player.Current.Artist} - {Player.Current.Title}";
			LeftWindowCommands.Visibility = Topmost ? Visibility.Collapsed : Visibility.Visible;
			MinimalViewButton.Icon = Topmost ? IconType.ExpandPane : IconType.CollapsePane;
			Menu.Visibility = Topmost ? Visibility.Hidden : Visibility.Visible;
			SearchButton.Visibility = Menu.Visibility;
			WindowStyle = Topmost ? WindowStyle.ToolWindow : WindowStyle.ThreeDBorderWindow;
			ResizeMode = Topmost ? ResizeMode.CanMinimize : ResizeMode.CanResize;
			if (Topmost)
			{
				WasMaximized = WindowState == WindowState.Maximized;
				WindowState = WindowState.Normal;
				TempHeight = ActualHeight;
				TempWidth = ActualWidth;
				Height = HeightOnMinimal;
				Width = WidthOnMinimal;
			}
			else
			{
				if (WasMaximized)
					WindowState = WindowState.Maximized;
				Height = TempHeight;
				Width = TempWidth;
				if (Left + Width > 1366)
					Left = 1366 - Width;
				if (Top + Height > 720)
					Top = 720 - Height;
			}
			Show();
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
				Player.Play(LibraryManager.Data, med);
		}

		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
			TempHeight = Settings.LastSize.Height;
			TempWidth = Settings.LastSize.Width;
			if (Settings.RememberMinimal && Settings.WasMinimal)
			{
				MinimalViewButton.EmulateClick();
				TempHeight = Settings.LastSize.Height;
				TempWidth = Settings.LastSize.Width;
			}
			else
			{
				Height = TempHeight;
				Width = TempWidth;
			}
			while (!Player.IsFullyLoaded)
				await Task.Delay(10);
			Environment.GetCommandLineArgs().For(each => LibraryManager.AddFromPath(each, true));
		}
		private void Window_Closing(object sender, CancelEventArgs e)
		{
			Settings.LastSize = new Size(Width <= 310 ? TempWidth : Width, Height <= 130 ? TempHeight : Height);
			Settings.LastLocation = new Point(Left, Top);
			Settings.WasMinimal = Height <= 131;
			Settings.Volume = Player.Volume;
			Settings.Save();
			Resource.Save();
			LibraryManager.Save();
			Application.Current.Shutdown();
		}
		private void Window_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Space && !SearchBox.IsFocused)
				Player.PlayPause();
		}
		private void Window_Drop(object sender, DragEventArgs e)
		{
			((string[])e.Data.GetData(DataFormats.FileDrop)).For(each => LibraryManager.AddFromPath(each));
		}

		private void Play(MediaQueue queue, Media media, bool inc)
		{
			Player.Play(queue, media);

			MinimalViewButton.Visibility = media.IsVideo ? Visibility.Hidden : Visibility.Visible;
			if (media.IsVideo)
			{
				if (WasMinimal)
					return;
				if (ActualHeight <= 131)
				{
					MinimalViewButton.EmulateClick();
					WasMinimal = true;
				}
				else
					WasMinimal = false;
			}
			else if (WasMinimal && ActualHeight > 131)
			{
				MinimalViewButton.EmulateClick();
				WasMinimal = false;
			}
		}

		private bool IsQueried = false;
		private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			IsQueried = !string.IsNullOrWhiteSpace(SearchBox.Text);
			LibraryManager.Filter(SearchBox.Text);

		}
		private void SearchIcon_Click(object sender, MouseButtonEventArgs e)
		{
			SearchPopup.IsOpen = true;
			SearchBox.Focus();
		}

		private void Menu_RevalidateClick(object sender, RoutedEventArgs e)
		{
			Hide();
			Close();
			Process.Start("Elephant Player.exe");
		}

		private void Data_Sorting(object sender, DataGridSortingEventArgs e)
		{
			var asc = (e.Column.SortDirection ?? ListSortDirection.Ascending) == ListSortDirection.Descending;
			switch (e.Column.DisplayIndex)
			{
				case 0:
					LibraryManager.SortBy(each => each.Title, asc);
					break;
				case 1:
					LibraryManager.SortBy(each => each.Artist, asc);
					break;
				case 2:
					LibraryManager.SortBy(each => each.Album, asc);
					break;
				case 3:
					LibraryManager.SortBy(each => each.PlayCount, asc);
					break;
				case 4:
					LibraryManager.SortBy(each => each.AdditionDate, asc);
					break;
				default:
					break;
			}
		}

		private bool Loaded1 = false, Loaded2 = false;

		private void DataGrid_MediaRequested(object sender, QueueEventArgs e)
		{
			Player.Play(e.Queue, e.Media);
		}

		private void Grid_Loaded(object sender, RoutedEventArgs e)
		{
			if (Loaded1)
				return;
			Loaded1 = true;
		}

		private void ArtistGrid_Loaded(object sender, RoutedEventArgs e)
		{
			if (Loaded2)
				return;
			Loaded2 = true;
		}
	}
}
