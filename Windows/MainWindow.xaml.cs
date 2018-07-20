using Microsoft.Win32;
using Player.Controls;
using Player.Hook;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Input;

namespace Player
{
	public partial class MainWindow : RibbonWindow
	{
		private const int HeightOnMinimal = 110;
		private double TempHeight
		{
			get => (double)Resources["MinimalDoubleSys"];
			set => Resources["MinimalDoubleSys"] = value;
		}

		private MediaManager Manager = new MediaManager();
		private bool ControlsNotNeededOnVisionIsVisible
		{
			set
			{
				ListView.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
				MiniArtworkImage.Visibility = ListView.Visibility;
				SearchButton.Visibility = ListView.Visibility;
				SearchLabel.Visibility = ListView.Visibility;
			}
		}
		private bool WasMaximized, WasMinimal;

		public MainWindow()
		{
			InitializeComponent();

			#region Initialization
			
			App.NewInstanceRequested += (_, e) => e.Args.ToList().ForEach(each => Manager.AddFromPath(each, true));
			App.KeyDown += KeyboardListener_KeyDown;

			Manager.RequestReceived += (_, e) => Play(e.Parameter);
			Player.LengthFound += (_, e) => Manager.Current.Length = e.Parameter;
			Player.FullScreenClicked += Player_FullScreenClicked;
			Player.UpdateLayout();

			ListView.ItemsSource = Manager;
			TaskbarItemInfo = Player.Thumb.Info;
			Resources["LastPath"] = App.Settings.LastPath;

			Player.NextClicked += (_, __) => Play(Manager.Next());
			Player.PreviousClicked += (_, __) => Play(Manager.Previous());
			Player.VisionChanged += (_, e) => ControlsNotNeededOnVisionIsVisible = e.Parameter;
			Player.Volume = App.Settings.Volume;
			Player.AutoOrinateVision = App.Settings.VisionOrientation;
			Player.PlayOnPositionChange = App.Settings.PlayOnPositionChange;

			Timer timer = new Timer(1000) { AutoReset = true };
			timer.Elapsed += (_, __) =>
			Dispatcher.Invoke(() =>
			ListView.Margin = Ribbon.IsMinimized ? new Thickness(0, 50, 0, 80) : new Thickness(0, 140, 0, 80));
			timer.Start();
			
			#endregion

			Width = App.Settings.LastSize.Width;
			Left = App.Settings.LastLoc.X;
			Top = App.Settings.LastLoc.Y;
		}
		
		private void Player_FullScreenClicked(object sender, EventArgs e)
		{
			ResizeMode = Player.IsFullScreen ? ResizeMode.NoResize : ResizeMode.CanResize;
			WindowStyle = Player.IsFullScreen ? WindowStyle.None : WindowStyle.SingleBorderWindow;
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
			if ((int)MinimalViewButton.Icon == 449)
			{
				if (WasMaximized)
					WindowState = WindowState.Maximized;
				ResizeMode = ResizeMode.CanResize;
				WindowStyle = WindowStyle.ThreeDBorderWindow;
				Height = TempHeight;
				MinimalViewButton.Icon = IconKind.ChevronDoubleUp;
			}
			else
			{
				WasMaximized = WindowState == WindowState.Maximized;
				WindowState = WindowState.Normal;
				TempHeight = ActualHeight;
				ResizeMode = ResizeMode.CanMinimize;
				Height = HeightOnMinimal;
				WindowStyle = WindowStyle.ToolWindow;
				MinimalViewButton.Icon = IconKind.ChevronDoubleDown;
			}
		}

		private async void KeyboardListener_KeyDown(object sender, RawKeyEventArgs e)
		{
			if (IsActive && SearchBox.IsFocused)
				return;
			//Key shortcuts when window is active and main key is down
			if (IsActive && e.Key.HasFlag(Key.LeftShift))
			{
				if (e.Key == Key.Delete) Menu_RemoveClick(this, null);
				if (e.Key == Key.Enter) DMouseDoubleClick(ListView, null);
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

		private void DMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (ListView.SelectedItem is Media med)
				Play(med, false);
		}

		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Height = 1;
			TempHeight = App.Settings.LastSize.Height;
			if (App.Settings.RememberMinimal && App.Settings.WasMinimal)
			{
				MinimalViewButton.EmulateClick();
				TempHeight = App.Settings.LastSize.Height;
			}
			else
				Height = TempHeight;
			while (!Player.IsFullyLoaded)
				await Task.Delay(10);
			Environment.GetCommandLineArgs().For(each => Manager.AddFromPath(each, true));

			ListView.IsHitTestVisibleChanged += (_, __) => Console.WriteLine("CHANGE");

		}
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			App.Settings.LastSize = new Size(Width, Height <= 130 ? TempHeight : Height);
			App.Settings.LastLoc = new Point(Left, Top);
			App.Settings.WasMinimal = Height <= 131;
			App.Settings.Volume = Player.Volume;
			App.Settings.Save();
			Manager.CloseSeason();
		}
		private void Window_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Space && !SearchBox.IsFocused)
				Player.PlayPause();
		}
		private void Window_Drop(object sender, DragEventArgs e)
		{
			((string[])e.Data.GetData(DataFormats.FileDrop)).For(each => Manager.AddFromPath(each));
		}

		private void Play(Media media, bool inQueueImpl = true)
		{
			if (!inQueueImpl)
			{
				Manager.Play(media);
			}
			MediaOperator.Reload(media);
			Player.Play(media);
			MiniArtworkImage.Source = media.Artwork;

			DeepBackEnd.NativeMethods.SHAddToRecentDocs(DeepBackEnd.NativeMethods.ShellAddToRecentDocsFlags.Path, media);
			Title = $"Elephant Player| {media.Artist} - {media.Title}";
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

		private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
		{

		}
		private void SearchIcon_Click(object sender, MouseButtonEventArgs e)
		{
			SearchPopup.IsOpen = true;
			SearchBox.Focus();
		}

		private void Menu_TagDetergent(object sender, RoutedEventArgs e)
		{
			For(item => MediaOperator.CleanTag(item));
		}
		private void Menu_PlayAfterClick(object sender, RoutedEventArgs e)
		{
			For(item =>
			{
				Manager.Remove(item);
				Manager.Insert(Manager.IndexOf(Manager.Current) + 1, item);
			});
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
					For(item => MediaOperator.Move(item, toDir: Resources["LastPath"].ToString()));
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
					For(item => MediaOperator.Copy(item, toDir: Resources["LastPath"].ToString()));
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
						MediaOperator.Reload(Manager.Current);
						Play(Manager.Current);
						Player.Position = pos;
						MediaOperator.Reload(each);
					}
					else
					{
						f.Parameter.Save();
						MediaOperator.Reload(each);
					}
				};
				pro.LoadFor(each);
			});
		}

		private void For(Action<Media> action)
		{
			ListView.SelectedItems.Cast<Media>().ToArray().For(action);
		}

		private void PlayModeButton_Click(object sender, RoutedEventArgs e)
		{
			switch (PlayModeButton.Icon)
			{
				case IconKind.Repeat:
					PlayModeButton.Icon = IconKind.RepeatOnce;
					PlayModeButton.Label = "Repeat One";
					App.Settings.PlayMode = PlayMode.RepeatOne;
					break;
				case IconKind.RepeatOnce:
					PlayModeButton.Icon = IconKind.Shuffle;
					PlayModeButton.Label = "Random";
					App.Settings.PlayMode = PlayMode.Shuffle;
					break;
				case IconKind.Shuffle:
					PlayModeButton.Icon = IconKind.Repeat;
					PlayModeButton.Label = "Repeat All";
					App.Settings.PlayMode = PlayMode.Repeat;
					break;
				default:
					break;
			}
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Manager.For(each => MediaOperator.Load(each));
		}

		private void RibbonApplicationMenuItem_Click(object sender, RoutedEventArgs e)
		{

		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			GC.Collect();
			GC.WaitForFullGCComplete();
		}
	}
}
