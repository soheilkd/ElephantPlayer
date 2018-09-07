using MahApps.Metro.Controls;
using Microsoft.Win32;
using Player.Extensions;
using Player.Hook;
using Player.Library;
using Player.Models;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
		private Visibility SelectiveBoxesVisibility
		{
			get => (Visibility)Resources["SelectiveBoxesVisibility"];
			set => Resources["SelectiveBoxesVisibility"] = value;
		}
		private SaveFileDialog MediaTransferDialog = new SaveFileDialog()
		{
			AddExtension = false,
			CheckPathExists = true,
			CreatePrompt = false,
			DereferenceLinks = true,
			InitialDirectory = Settings.Current.LastPath
		};

		private MediaManager Manager = new MediaManager();
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
			App.NewInstanceRequested += (_, e) => e.Args.For(each => Manager.AddFromPath(each, true));
			Events.KeyDown += KeyboardListener_KeyDown;

			Manager.RequestReceived += (_, e) => Play(e.Parameter);
			Player.LengthFound += (_, e) => Manager.Current.Length = e.Parameter;
			Player.FullScreenToggled += Player_FullScreenClicked;
			Player.UpdateLayout();

			DataGrid.ItemsSource = Manager.QueueEnumerator;

			TaskbarItemInfo = Player.Thumb.Info;
			Resources["LastPath"] = Settings.Current.LastPath;

			Player.NextClicked += (_, __) => Play(Manager.Next());
			Player.PreviousClicked += (_, __) => Play(Manager.Previous());
			Player.VisionChanged += (_, e) => ControlsNotNeededOnVisionVisibility = e.Parameter ? Visibility.Hidden : Visibility.Visible;
			Player.AutoOrinateVision = Settings.Current.VisionOrientation;
			Player.PlayOnPositionChange = Settings.Current.PlayOnPositionChange;
			Player.PlayCounterElapsed += (_, __) => Manager.Current.PlayCount++;
			
			OrinateCheck.IsChecked = Settings.Current.VisionOrientation;
			LiveLibraryCheck.IsChecked = Settings.Current.LiveLibrary;
			ExplicitCheck.IsChecked = Settings.Current.ExplicitContent;
			PlayOnPosCheck.IsChecked = Settings.Current.PlayOnPositionChange;
			RevalidOnExitCheck.IsChecked = Settings.Current.RevalidateOnExit;
			TimeoutCombo.SelectedIndex = Settings.Current.MouseTimeoutIndex;

			TimeoutCombo.SelectionChanged += (_, __) => Settings.Current.MouseTimeoutIndex = TimeoutCombo.SelectedIndex;
			OrinateCheck.Checked += (_, __) => Settings.Current.VisionOrientation = true;
			OrinateCheck.Unchecked += (_, __) => Settings.Current.VisionOrientation = false;
			LiveLibraryCheck.Checked += (_, __) => Settings.Current.LiveLibrary = true;
			LiveLibraryCheck.Unchecked += (_, __) => Settings.Current.LiveLibrary = false;
			ExplicitCheck.Checked += (_, __) => Settings.Current.ExplicitContent = true;
			ExplicitCheck.Unchecked += (_, __) => Settings.Current.ExplicitContent = false;
			PlayOnPosCheck.Checked += (_, __) => Settings.Current.PlayOnPositionChange = true;
			PlayOnPosCheck.Unchecked += (_, __) => Settings.Current.PlayOnPositionChange = false;
			RevalidOnExitCheck.Checked += (_, __) => Settings.Current.RevalidateOnExit = true;
			RevalidOnExitCheck.Unchecked += (_, __) => Settings.Current.RevalidateOnExit = false;
			RememberMinimalCheck.Checked += (_, __) => Settings.Current.RememberMinimal = true;
			RememberMinimalCheck.Unchecked += (_, __) => Settings.Current.RememberMinimal = false;
			Player.BorderBack = Background;
			Player.ChangeFFmpegDirectory($@"{Settings.AppPath}\ffmpeg");
			Player.ChangeVolumeBySlider(Settings.Current.Volume * 100);
			Player.Volume = Settings.Current.Volume;

			foreach (var item in this.FindChildren<MenuItem>())
				item.Background = Menu.Background;
			foreach (MenuItem item in DataGrid.ContextMenu.Items)
				item.Background = Menu.Background;
			#endregion
			
			Left = Settings.Current.LastLocation.X;
			Top = Settings.Current.LastLocation.Y;
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
			Title = $"{(Topmost ?  "": "Elephant Player | ")}{Manager.Current.Artist} - {Manager.Current.Title}";
			LeftWindowCommands.Visibility = Topmost ? Visibility.Collapsed : Visibility.Visible;
			MinimalViewButton.Icon = Topmost ? Controls.IconType.OpenPaneMirrored : Controls.IconType.ClosePaneMirrored;
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
			Console.WriteLine(e.Key);
			//Key shortcuts when window is active and main key is down
			if (IsActive && e.Key.HasFlag(Key.LeftShift))
			{
				if (e.Key == Key.Delete) Menu_RemoveClick(this, null);
				if (e.Key == Key.Enter) List_DoubleClick(DataGrid, null);
				if (e.Key == Key.C) Menu_CopyClick(new MenuItem(), null);
				if (e.Key == Key.X) Menu_MoveClick(new MenuItem(), null);
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
				Play(med, false);
		}

		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Height = 1;
			TempHeight = Settings.Current.LastSize.Height;
			TempWidth = Settings.Current.LastSize.Width;
			if (Settings.Current.RememberMinimal && Settings.Current.WasMinimal)
			{
				MinimalViewButton.EmulateClick();
				TempHeight = Settings.Current.LastSize.Height;
				TempWidth = Settings.Current.LastSize.Width;
			}
			else
			{
				Height = TempHeight;
				Width = TempWidth;
			}
			while (!Player.IsFullyLoaded)
				await Task.Delay(10);
			Environment.GetCommandLineArgs().For(each => Manager.AddFromPath(each, true));
		}
		private void Window_Closing(object sender, CancelEventArgs e)
		{
			Settings.Current.LastSize = new Size(Width <= 310 ? TempWidth: Width, Height <= 130 ? TempHeight : Height);
			Settings.Current.LastLocation = new Point(Left, Top);
			Settings.Current.WasMinimal = Height <= 131;
			Settings.Current.Volume = Player.Volume;
			Settings.Current.Save();
			Manager.CloseSeason();
			Application.Current.Shutdown();
		}
		private void Window_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Space && !SearchBox.IsFocused)
				Player.PlayPause();
			if (e.Key == Key.S)
			{
				switch (SelectiveBoxesVisibility)
				{
					case Visibility.Visible:
						SelectiveBoxesVisibility = Visibility.Hidden;
						break;
					default:
						SelectiveBoxesVisibility = Visibility.Visible;
						break;
				}
			}
		}
		private void Window_Drop(object sender, DragEventArgs e)
		{
			((string[])e.Data.GetData(DataFormats.FileDrop)).For(each => Manager.AddFromPath(each));
		}

		private void Play(Media media, bool inQueueImpl = true)
		{
			if (!inQueueImpl)
				Manager.Next(media);
			Player.Play(media);
			
			Title = $"{(Topmost ? "" : "Elephant Player | ")}{media.Artist} - {media.Title}";
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
			IsQueried = !String.IsNullOrWhiteSpace(SearchBox.Text);
			Manager.QueueEnumerator.Filter(Manager, SearchBox.Text);
		}
		private void SearchIcon_Click(object sender, MouseButtonEventArgs e)
		{
			SearchPopup.IsOpen = true;
			SearchBox.Focus();
		}

		private void Menu_TagDetergent(object sender, RoutedEventArgs e)
		{
			For(item => item.CleanTag());
		}
		private void Menu_MoveClick(object sender, RoutedEventArgs e)
		{
			switch ((sender.As<MenuItem>().Header ?? "INDIV").ToString().Substring(0, 1))
			{
				case "B":
					MediaTransferDialog.Title = "Move";
					if (MediaTransferDialog.ShowDialog().Value)
					{
						Settings.Current.LastPath = MediaTransferDialog.FileName.Substring(0, MediaTransferDialog.FileName.LastIndexOf('\\') + 1);
						Resources["LastPath"] = Settings.Current.LastPath;
						goto default;
					}
					break;
				default:
					For(item => item.MoveTo(Resources["LastPath"].ToString()));
					break;
			}
		}
		private void Menu_CopyClick(object sender, RoutedEventArgs e)
		{
			switch ((sender.As<MenuItem>().Header ?? "INDIV").ToString().Substring(0, 1))
			{
				case "B":
					MediaTransferDialog.Title = "Copy";
					if (MediaTransferDialog.ShowDialog().Value)
					{
						Settings.Current.LastPath = MediaTransferDialog.FileName.Substring(0, MediaTransferDialog.FileName.LastIndexOf('\\') + 1);
						Resources["LastPath"] = Settings.Current.LastPath;
						goto default;
					}
					break;
				default:
					For(item => item.CopyTo(Resources["LastPath"].ToString()));
					break;
			}
		}
		private void Menu_RemoveClick(object sender, RoutedEventArgs e)
		{
			For(item => Manager.Remove(item));
		}
		private void Menu_DeleteClick(object sender, RoutedEventArgs e)
		{
			string msg = "Sure? These will be deleted:\r\n";
			For(item => msg += $"{item.Path}\r\n");
			if (MessageBox.Show(msg, "Sure?", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
				return;
			For(item => Manager.Delete(item));
		}
		private void Menu_LocationClick(object sender, RoutedEventArgs e)
		{
			For(item => Process.Start("explorer.exe", "/select," + item.Path));
		}
		private void Menu_PropertiesClick(object sender, RoutedEventArgs e)
		{
			For(each =>
			{
				var pro = new PropertiesUI();
				pro.SaveRequested += (_, f) =>
				{
					if (f.Parameter.Name == Manager.Current.Path)
					{
						var pos = Player.Position;
						Player.Stop();
						f.Parameter.Save();
						Manager.Current.Reload();
						Play(Manager.Current);
						Player.Position = pos;
						each.Reload();
					}
					else
					{
						f.Parameter.Save();
						each.Reload();
					}
				};
				pro.LoadFor(each);
			});
		}

		private void Menu_PlayModeClick(object sender, RoutedEventArgs e)
		{
			PlayModeSubMenu.Items[0].As<MenuItem>().IsChecked = false;
			PlayModeSubMenu.Items[1].As<MenuItem>().IsChecked = false;
			PlayModeSubMenu.Items[2].As<MenuItem>().IsChecked = false;
			byte tag = byte.Parse(sender.As<MenuItem>().Tag.ToString());
			PlayModeSubMenu.Items[tag].As<MenuItem>().IsChecked = true;
			Settings.Current.PlayMode = (PlayMode)tag;
		}
		private void Menu_LibraryImportClick(object sender, RoutedEventArgs e)
		{
			if (Dialogs.RequestFile(out var file, Dialogs.LibraryFilter))
				if (LibraryManager.TryLoad(file[0], out var lib))
				{
					Settings.Current.LibraryLocation = file[0];
					Manager.Clear();
					lib.For(each => Manager.Add(each));
				}
		}
		private void Menu_LibraryExportClick(object sender, RoutedEventArgs e)
		{
			if (Dialogs.RequestSave(out var file, Dialogs.LibraryFilter))
			{
				if (!file.EndsWith(".bin"))
					file += ".bin";
				Settings.Current.LibraryLocation = file;
				LibraryManager.Save(Manager);
			}
		}
		private void Menu_HeavyLoadClick(object sender, RoutedEventArgs e)
		{
			Manager.For(each => each.Load());
		}
		private void Menu_RevalidateClick(object sender, RoutedEventArgs e)
		{
			Hide();
			Manager.Revalidate();
			Close();
			Process.Start("Elephant Player.exe");
		}

		private void Data_Sorting(object sender, DataGridSortingEventArgs e)
		{
			var asc = (e.Column.SortDirection ?? ListSortDirection.Descending) == ListSortDirection.Descending;
			switch (e.Column.DisplayIndex)
			{
				case 1:
					Manager.SortQueueBy(each => each.Title, asc);
					break;
				case 2:
					Manager.SortQueueBy(each => each.Artist, asc);
					break;
				case 3:
					Manager.SortQueueBy(each => each.Album, asc);
					break;
				case 4:
					Manager.SortQueueBy(each => each.PlayCount, asc);
					break;
				case 5:
					Manager.SortQueueBy(each => each.AdditionDate, asc);
					break;
				default:
					break;
			}
		}

		private void For(Action<Media> action) =>
			DataGrid.SelectedItems.Cast<Media>().ToArray().For(each => action(each));
	}
}
