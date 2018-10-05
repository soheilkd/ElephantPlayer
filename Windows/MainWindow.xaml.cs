using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using Player.Controls.Navigation;
using Player.Extensions;
using Player.Hook;
using Player.Library;
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

		private Controller Library = new Controller();
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
			App.NewInstanceRequested += (_, e) => e.Args.For(each => Library.AddFromPath(each, true));
			Events.KeyDown += KeyboardListener_KeyDown;

			Library.MediaRequested += (_, e) => Play(Library, e.Parameter);
			Player.FullScreenToggled += Player_FullScreenClicked;
			Player.UpdateLayout();

			DataGrid.ItemsSource = Library;

			TaskbarItemInfo = Player.Thumb.Info;
			Resources["LastPath"] = Settings.LastPath;
			
			Player.VisionChanged += (_, e) => ControlsNotNeededOnVisionVisibility = e.Parameter ? Visibility.Hidden : Visibility.Visible;
			Player.MediaChanged += (_, e) => Title = $"{(Topmost ? "" : "Elephant Player | ")}{e.Parameter.Artist} - {e.Parameter.Title}";
			Player.AutoOrinateVision = Settings.VisionOrientation;
			Player.PlayOnPositionChange = Settings.PlayOnPositionChange;
			
			Player.BorderBack = Background;
			Player.ChangeVolumeBySlider(Settings.Volume * 100);
			Player.Volume = Settings.Volume;

			foreach (var item in this.FindChildren<MenuItem>())
				item.Background = Menu.Background;
			foreach (MenuItem item in DataGrid.ContextMenu.Items)
				item.Background = Menu.Background;
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
			Title = $"{(Topmost ?  "": "Elephant Player | ")}{Player.Current.Artist} - {Player.Current.Title}";
			LeftWindowCommands.Visibility = Topmost ? Visibility.Collapsed : Visibility.Visible;
			MinimalViewButton.Icon = Topmost ? Controls.IconType.ExpandPane : Controls.IconType.CollapsePane;
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
				Play(Library, med);
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
			Environment.GetCommandLineArgs().For(each => Library.AddFromPath(each, true));

			var artists = Library.OrderBy(each => each.Artist).GroupBy(each => each.Artist);
			var grid = ArtistNavigation.GetChildContent(1) as Grid;
			artists.ForEach(each =>
			grid.Children.Add(
				new NavigationTile()
				{
					Tag = each.Key,
					TileStyle = Controls.TileStyle.Singular,
					Navigation = new NavigationControl()
					{
						Tag = each.Key,
						Content = new ArtistView(new MediaQueue(each), (queue, media) => Play(queue, media))
					}
				}));

			var albums = Library.OrderBy(each => each.Artist).GroupBy(each => each.Album);
			var grid2 = AlbumNavigation.GetChildContent(1) as Grid;
			albums.ForEach(each =>
			grid2.Children.Add(
				new NavigationTile()
				{
					Tag = each.Key,
					TileStyle = Controls.TileStyle.Singular,
					Navigation = new NavigationControl()
					{
						Tag = each.Key,
						Content = new AlbumView(new MediaQueue(each), (queue, media) => Play(queue, media))
					}
				}));
			grid.SizeChanged += (_, __) => grid.AlignItems(Controls.Tile.StandardSize);
			grid2.SizeChanged += (_, __) => grid2.AlignItems(Controls.Tile.StandardSize);
		}
		private void Window_Closing(object sender, CancelEventArgs e)
		{
			Settings.LastSize = new Size(Width <= 310 ? TempWidth: Width, Height <= 130 ? TempHeight : Height);
			Settings.LastLocation = new Point(Left, Top);
			Settings.WasMinimal = Height <= 131;
			Settings.Volume = Player.Volume;
			Settings.Save();
			Library.CloseSeason();
			Application.Current.Shutdown();
		}
		private void Window_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Space && !SearchBox.IsFocused)
				Player.PlayPause();
		}
		private void Window_Drop(object sender, DragEventArgs e)
		{
			((string[])e.Data.GetData(DataFormats.FileDrop)).For(each => Library.AddFromPath(each));
		}

		private void Play(MediaQueue queue, Media media)
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
			Library.Filter(SearchBox.Text);
		}
		private void SearchIcon_Click(object sender, MouseButtonEventArgs e)
		{
			SearchPopup.IsOpen = true;
			SearchBox.Focus();
		}

		private void Menu_LibraryImportClick(object sender, RoutedEventArgs e)
		{
			if (Dialogs.RequestFile(out var file, Dialogs.LibraryFilter))
				if (Controller.TryLoad(file[0], out var lib))
				{
					Settings.LibraryLocation = file[0];
					Library.Clear();
					lib.For(each => Library.Add(each));
				}
		}
		private void Menu_LibraryExportClick(object sender, RoutedEventArgs e)
		{
			if (Dialogs.RequestSave(out var file, Dialogs.LibraryFilter))
			{
				if (!file.EndsWith(".bin"))
					file += ".bin";
				Settings.LibraryLocation = file;
				Controller.Save(Library);
			}
		}
		private void Menu_HeavyLoadClick(object sender, RoutedEventArgs e)
		{
			Library.For(each => each.Load());
		}
		private void Menu_RevalidateClick(object sender, RoutedEventArgs e)
		{
			Hide();
			Library.Revalidate();
			Close();
			Process.Start("Elephant Player.exe");
		}

		private void Data_Sorting(object sender, DataGridSortingEventArgs e)
		{
			var asc = (e.Column.SortDirection ?? ListSortDirection.Ascending) == ListSortDirection.Descending;
			switch (e.Column.DisplayIndex)
			{
				case 0:
					Library.SortBy(each => each.Title, asc);
					break;
				case 1:
					Library.SortBy(each => each.Artist, asc);
					break;
				case 2:
					Library.SortBy(each => each.Album, asc);
					break;
				case 3:
					Library.SortBy(each => each.PlayCount, asc);
					break;
				case 4:
					Library.SortBy(each => each.AdditionDate, asc);
					break;
				default:
					break;
			}
		}

		bool Loaded1 = false, Loaded2 = false;
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
