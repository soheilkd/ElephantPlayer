using Microsoft.Win32;
using Player.Events;
using Player.Hook;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Player
{
	public partial class MainWindow : Window
	{
		private const int HeightOnMinimal = 130;
		private double TempHeight
		{
			get => (double)Resources["MinimalDoubleSys"];
			set => Resources["MinimalDoubleSys"] = value;
		}

		private MediaManager Manager = new MediaManager();
		private Timer PlayCountTimer = new Timer(100000) { AutoReset = false };
		private bool ControlsNotNeededOnVisionIsVisible
		{
			set
			{
				TabControl.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
				MiniArtworkImage.Visibility = TabControl.Visibility;
				SearchButton.Visibility = TabControl.Visibility;
				SearchLabel.Visibility = TabControl.Visibility;
			}
		}
		private bool WasMaximized;
		public MainWindow()
		{
			InitializeComponent();
			Initialize();
			Width = App.Settings.LastSize.Width;
			Left = App.Settings.LastLoc.X;
			Top = App.Settings.LastLoc.Y;
		}

		private void Initialize()
		{
			PlayCountTimer.Elapsed += (_, __) => Manager.Current.PlayCount++;
			App.NewInstanceRequested += (_, e) => e.Args.ToList().ForEach(each => Manager.AddFromPath(each, true));
			App.KeyDown += KeyboardListener_KeyDown;

			Manager.RequestReceived += (_, e) => Play(e.Parameter);
			Player.RequestReceived += Player_RequestReceived;
			Player.LengthFound += (_, e) => Manager.Current.Length = e.Parameter;
			Player.UpdateLayout();
			ArtistsView.ItemsSource = Manager.OrderBy(each => each.Artist);
			AlbumsView.ItemsSource = Manager.OrderBy(each => each.Album);
			TitlesView.ItemsSource = Manager;
			Player.ParentWindow = this;
			TaskbarItemInfo = Player.Thumb.Info;
			Resources["LastPath"] = App.Settings.LastPath;

			RebindViews();
			ArtistsView.MouseDoubleClick += DMouseDoubleClick;
			TitlesView.MouseDoubleClick += DMouseDoubleClick;
			AlbumsView.MouseDoubleClick += DMouseDoubleClick;
			TabControl.SelectedIndex = 1;

			SettingsPanel.RevalidationRequest += async (sender, __) =>
			{
				sender.As<Button>().Content = "Revalidating... App will freeze";
				IsEnabled = false;
				await Task.Delay(2000);
				Manager.Revalidate();
				Application.Current.Shutdown(0);
				Process.Start(App.Path + "Elephant Player.exe");
			};
		}

		private async void KeyboardListener_KeyDown(object sender, RawKeyEventArgs e)
		{
			if (TabControl.SelectedIndex == TabControl.Items.Count - 1)
				return;
			if (IsActive && SearchBox.IsFocused)
				return;
			//Key shortcuts when window is active and main key is down (default: alt)
			if (IsActive && e.Key.HasFlag(App.Settings.AncestorKey))
			{
				if (e.Key == App.Settings.RemoveKey) Menu_RemoveClick(this, null);
				if (e.Key == App.Settings.MediaPlayKey) DMouseDoubleClick(ActiveView, null);
				if (e.Key == App.Settings.CopyKey) Menu_CopyClick(new MenuItem(), null);
				if (e.Key == App.Settings.MoveKey) Menu_MoveClick(new MenuItem(), null);
				if (e.Key == App.Settings.PropertiesKey) Menu_PropertiesClick(this, null);
				if (e.Key == App.Settings.FindKey)
				{
					SearchBox.IsEnabled = false;
					SearchBox.Text = "";
					SearchButton.EmulateClick();
					await Task.Delay(100);
					SearchBox.IsEnabled = true;
					SearchBox.Focus();
				}
			}
			//Key shortcuts whether window is active or main key is down (default: alt)
			if (IsActive || e.Key.HasFlag(App.Settings.AncestorKey))
			{
				if (e.Key == App.Settings.BackwardKey) Player.SlidePosition(false);
				if (e.Key == App.Settings.ForwardKey) Player.SlidePosition(true);
			}
			//Key shortcuts always invokable
			if (e.Key == App.Settings.PublicPlayPauseKey) Player.PlayPause();
			if (e.Key == App.Settings.NextKey) Player.Next();
			if (e.Key == App.Settings.PreviousKey) Player.Previous();
		}

		private void Player_RequestReceived(object sender, RequestArgs e)
		{
			switch (e.Request)
			{
				case RequestType.Next: Play(Manager.Next()); break;
				case RequestType.Previous: Play(Manager.Previous()); break;
				case RequestType.Magnifiement: ControlsNotNeededOnVisionIsVisible = !Player.IsMagnified; break;
				case RequestType.Collapse:
					WasMaximized = WindowState == WindowState.Maximized;
					WindowState = WindowState.Normal;
					TempHeight = ActualHeight;
					ResizeMode = ResizeMode.CanMinimize;
					Height = HeightOnMinimal;
					WindowStyle = WindowStyle.ToolWindow;
					Player.MinimalViewButton.Icon = MaterialDesignThemes.Wpf.PackIconKind.ChevronDoubleDown;
					break;
				case RequestType.Expand:
					if (WasMaximized)
						WindowState = WindowState.Maximized;
					ResizeMode = ResizeMode.CanResize;
					WindowStyle = WindowStyle.ThreeDBorderWindow;
					Height = TempHeight;
					Player.MinimalViewButton.Icon = MaterialDesignThemes.Wpf.PackIconKind.ChevronDoubleUp;
					break;
				default: break;
			}
		}

		private CollectionView[] Views = new CollectionView[4];
		private PropertyGroupDescription[] Descriptions = new PropertyGroupDescription[4];
		private void RebindViews()
		{
			Views[0] = (CollectionView)CollectionViewSource.GetDefaultView(ArtistsView.ItemsSource);
			Descriptions[0] = new PropertyGroupDescription("Artist");
			Views[0].GroupDescriptions.Add(Descriptions[0]);

			Views[1] = (CollectionView)CollectionViewSource.GetDefaultView(AlbumsView.ItemsSource);
			Descriptions[1] = new PropertyGroupDescription("Album");
			Views[1].GroupDescriptions.Add(Descriptions[1]);
		}

		private void DMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (sender.As<ListView>().SelectedItem is Media med)
				Play(med, false);
		}

		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Height = 1;
			TempHeight = App.Settings.LastSize.Height;
			if (App.Settings.RememberMinimal && App.Settings.WasMinimal)
			{
				Player.MinimalViewButton.EmulateClick();
				TempHeight = App.Settings.LastSize.Height;
			}
			else
				Height = TempHeight;
			while (!Player.IsFullyLoaded)
				await Task.Delay(10);
			Environment.GetCommandLineArgs().For(each => Manager.AddFromPath(each, true));

			TitlesView.IsHitTestVisibleChanged += (_, __) => Console.WriteLine("CHANGE");
		}
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			App.Settings.LastSize = new Size(Width, Height <= 130 ? TempHeight: Height);
			App.Settings.LastLoc = new Point(Left, Top);
			App.Settings.WasMinimal = Height <= 131;
			App.Settings.Volume = Player.Volume;
			App.Settings.Save();
			Manager.CloseSeason();
		}
		private void Window_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == (Key)Enum.Parse(typeof(Key), App.Settings.PrivatePlayPauseKey.ToString()))
				if (!SearchBox.IsFocused)
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
			Player.Play(media);
			MiniArtworkImage.Source = media.Artwork;
			DeepBackEnd.NativeMethods.SHAddToRecentDocs(DeepBackEnd.NativeMethods.ShellAddToRecentDocsFlags.Path, media);
			Title = $"Elephant Player| {media.Artist} - {media.Title}";
		}

		private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			RebindViews();
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
		
		private ListView ActiveView
		{
			get
			{
				switch (TabControl.SelectedIndex)
				{
					case 1: return TitlesView;
					case 2: return ArtistsView;
					case 3: return AlbumsView;
					default: return null;
				}
			}
		}
		private void For(Action<Media> action)
		{
			ActiveView.SelectedItems.Cast<Media>().ToArray().For(action);
		}
	}
}
