using MahApps.Metro.Controls;
using Microsoft.Win32;
using Player.Hook;
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
			
			App.NewInstanceRequested += (_, e) => e.Args.ToList().ForEach(each => Manager.AddFromPath(each, true));
			Hook.Events.KeyDown += KeyboardListener_KeyDown;

			Manager.RequestReceived += (_, e) => Play(e.Parameter);
			Player.LengthFound += (_, e) => Manager.Current.Length = e.Parameter;
			Player.FullScreenClicked += Player_FullScreenClicked;
			Player.UpdateLayout();

			DataGrid.ItemsSource = Manager.QueueEnumerator;

			TaskbarItemInfo = Player.Thumb.Info;
			Resources["LastPath"] = App.Settings.LastPath;

			Player.NextClicked += (_, __) => Play(Manager.Next());
			Player.PreviousClicked += (_, __) => Play(Manager.Previous());
			Player.VisionChanged += (_, e) => ControlsNotNeededOnVisionVisibility = e.Parameter ? Visibility.Hidden : Visibility.Visible;
			Player.Volume = App.Settings.Volume;
			Player.AutoOrinateVision = App.Settings.VisionOrientation;
			Player.PlayOnPositionChange = App.Settings.PlayOnPositionChange;
			Player.PlayCounterElapsed += (_, __) => Manager.Current.PlayCount++;
			
			OrinateCheck.IsChecked = App.Settings.VisionOrientation;
			LiveLibraryCheck.IsChecked = App.Settings.LiveLibrary;
			ExplicitCheck.IsChecked = App.Settings.ExplicitContent;
			PlayOnPosCheck.IsChecked = App.Settings.PlayOnPositionChange;
			RevalidOnExitCheck.IsChecked = App.Settings.RevalidateOnExit;
			TimeoutCombo.SelectedIndex = App.Settings.MouseTimeoutIndex;

			TimeoutCombo.SelectionChanged += (_, __) => App.Settings.MouseTimeoutIndex = TimeoutCombo.SelectedIndex;
			OrinateCheck.Checked += (_, __) => App.Settings.VisionOrientation = true;
			OrinateCheck.Unchecked += (_, __) => App.Settings.VisionOrientation = false;
			LiveLibraryCheck.Checked += (_, __) => App.Settings.LiveLibrary = true;
			LiveLibraryCheck.Unchecked += (_, __) => App.Settings.LiveLibrary = false;
			ExplicitCheck.Checked += (_, __) => App.Settings.ExplicitContent = true;
			ExplicitCheck.Unchecked += (_, __) => App.Settings.ExplicitContent = false;
			PlayOnPosCheck.Checked += (_, __) => App.Settings.PlayOnPositionChange = true;
			PlayOnPosCheck.Unchecked += (_, __) => App.Settings.PlayOnPositionChange = false;
			RevalidOnExitCheck.Checked += (_, __) => App.Settings.RevalidateOnExit = true;
			RevalidOnExitCheck.Unchecked += (_, __) => App.Settings.RevalidateOnExit = false;
			RememberMinimalCheck.Checked += (_, __) => App.Settings.RememberMinimal = true;
			RememberMinimalCheck.Unchecked += (_, __) => App.Settings.RememberMinimal = false;
			Player.BorderBack = Background;

			foreach (var item in this.FindChildren<MenuItem>())
			{
				item.Background = Menu.Background;
			}
			foreach (MenuItem item in DataGrid.ContextMenu.Items)
			{
				item.Background = Menu.Background;
			}
			#endregion
			
			Left = App.Settings.LastLocation.X;
			Top = App.Settings.LastLocation.Y;
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
			Title = $"{(Topmost ?  "": "Elephant Player | ")}{Manager.Current.Artist} - {Manager.Current.Title}";
			LeftWindowCommands.Visibility = Topmost ? Visibility.Collapsed : Visibility.Visible;
			MinimalViewButton.Icon = Topmost ? Controls.IconType.OpenPaneMirrored : Controls.IconType.ClosePaneMirrored;
			Menu.Visibility = Topmost ? Visibility.Hidden : Visibility.Visible;
			Player.VolumeBorder.Visibility = Menu.Visibility;
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
			TempHeight = App.Settings.LastSize.Height;
			TempWidth = App.Settings.LastSize.Width;
			if (App.Settings.RememberMinimal && App.Settings.WasMinimal)
			{
				MinimalViewButton.EmulateClick();
				TempHeight = App.Settings.LastSize.Height;
				TempWidth = App.Settings.LastSize.Width;
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
			App.Settings.LastSize = new Size(Width <= 310 ? TempWidth: Width, Height <= 130 ? TempHeight : Height);
			App.Settings.LastLocation = new Point(Left, Top);
			App.Settings.WasMinimal = Height <= 131;
			App.Settings.Volume = Player.Volume;
			App.Settings.Save();
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
					SaveFileDialog saveDiag = new SaveFileDialog()
					{
						AddExtension = false,
						CheckPathExists = true,
						CreatePrompt = false,
						DereferenceLinks = true,
						InitialDirectory = App.Settings.LastPath,
						Title = "Move"
					};
					if (saveDiag.ShowDialog().Value)
					{
						App.Settings.LastPath = saveDiag.FileName.Substring(0, saveDiag.FileName.LastIndexOf('\\') + 1);
						Resources["LastPath"] = App.Settings.LastPath;
						goto default;
					}
					break;
				default:
					For(item => item.Move(toDir: Resources["LastPath"].ToString()));
					break;
			}
		}
		private void Menu_CopyClick(object sender, RoutedEventArgs e)
		{
			switch ((sender.As<MenuItem>().Header ?? "INDIV").ToString().Substring(0, 1))
			{
				case "B":
					SaveFileDialog saveDiag = new SaveFileDialog()
					{
						AddExtension = false,
						CheckPathExists = true,
						CreatePrompt = false,
						DereferenceLinks = true,
						InitialDirectory = App.Settings.LastPath,
						Title = "Copy"
					};
					if (saveDiag.ShowDialog().Value)
					{
						App.Settings.LastPath = saveDiag.FileName.Substring(0, saveDiag.FileName.LastIndexOf('\\') + 1);
						Resources["LastPath"] = App.Settings.LastPath;
						goto default;
					}
					break;
				default:
					For(item => item.Copy(toDir: Resources["LastPath"].ToString()));
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
			byte tag = Byte.Parse(sender.As<MenuItem>().Tag.ToString());
			PlayModeSubMenu.Items[tag].As<MenuItem>().IsChecked = true;
			App.Settings.PlayMode = (PlayMode)tag;
		}
		private void Menu_LibraryImportClick(object sender, RoutedEventArgs e)
		{
			if (Dialogs.RequestFile(out var file, Dialogs.LibraryFilter))
				if (LibraryManager.TryLoad(file[0], out var lib))
				{
					App.Settings.LibraryLocation = file[0];
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
				if (!file.StartsWith(App.Path)) App.Settings.LibraryLocation = file;
				else App.Settings.LibraryLocation = file.Replace(App.Path, String.Empty);
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

		private void DataGrid_Sorting(object sender, DataGridSortingEventArgs e)
		{
			var asc = e.Column.SortDirection == ListSortDirection.Ascending;
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

		private void For(Action<Media> action)
		{
			Manager.For(each => action(each), each => each.IsSelected);
		}
	}
}
