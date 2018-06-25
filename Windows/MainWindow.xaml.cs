using Microsoft.Win32;
using Player.Controls;
using Player.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Forms = System.Windows.Forms;

namespace Player
{
	public partial class MainWindow : Window
	{
		private MediaManager Manager = new MediaManager();
		private ObservableCollection<Media>[] Collections = new ObservableCollection<Media>[3];
		private Timer PlayCountTimer = new Timer(100000) { AutoReset = false };
		private Gma.System.MouseKeyHook.IKeyboardMouseEvents KeyboardEvents = Gma.System.MouseKeyHook.Hook.GlobalEvents();
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
		private Storyboard MinimalOnBoard, MinimalOffBoard;

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
			PlayCountTimer.Elapsed += (_, __) => Manager.CurrentlyPlaying.PlayCount++;
			App.NewInstanceRequested += (_, e) => e.Args.ToList().ForEach(each => Manager.Add(each, true));
			KeyboardEvents.KeyDown += Keyboard_KeyDown;
			Manager.Change += Manager_Change;
			Player.SomethingHappened += Player_EventHappened;
			Player.UpdateLayout();
			Collections[0] = new ObservableCollection<Media>();
			Collections[1] = LibraryOperator.LoadedCollection.ByArtist;
			Collections[2] = LibraryOperator.LoadedCollection.ByAlbum;
			MinimalOnBoard = Resources["MinimalOnBoard"] as Storyboard;
			MinimalOffBoard = Resources["MinimalOffBoard"] as Storyboard;
			ArtistsView.ItemsSource = Collections[1];
			AlbumsView.ItemsSource = Collections[2];
			TitlesView.ItemsSource = Manager;
			Player.ParentWindow = this;
			TaskbarItemInfo = Player.Thumb.Info;
			MinimalOnBoard.CurrentStateInvalidated += (_, __) => TabControl.Height = Height >= 131 ? Height - 80 : TabControl.Height;
			MinimalOffBoard.Completed += (_, __) => TabControl.Height = Double.NaN;
			Resources["LastPath"] = App.Settings.LastPath;

			RebindViews();
			ArtistsView.MouseDoubleClick += DMouseDoubleClick;
			TitlesView.MouseDoubleClick += DMouseDoubleClick;
			AlbumsView.MouseDoubleClick += DMouseDoubleClick;
			Manager.CollectionChanged += Manager_CollectionChanged;
			TabControl.SelectedIndex = 1;
		}

		private void Manager_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					Collections.For(each => each.Insert(e.NewStartingIndex, e.NewItems[0] as Media));
					break;
				case NotifyCollectionChangedAction.Remove:
					Collections.For(each => each.Remove(e.OldItems[0] as Media));
					break;
				case NotifyCollectionChangedAction.Replace:
					Collections.For(each => each[e.NewStartingIndex] = e.NewItems[0] as Media);
					break;
				case NotifyCollectionChangedAction.Move:
					Collections.For(each => each.Move(e.OldStartingIndex, e.NewStartingIndex));
					break;
				case NotifyCollectionChangedAction.Reset:
					Collections.For(each => each.Clear());
					break;
				default: break;
			}
		}

		CollectionView[] Views = new CollectionView[4];
		PropertyGroupDescription[] Descriptions = new PropertyGroupDescription[4];
		private void RebindViews()
		{
			ArtistsView.ItemsSource = Collections[1];
			AlbumsView.ItemsSource = Collections[2];
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
				Play(med);
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Height = 1;
			Resources["MinimalDoubleSys"] = App.Settings.LastSize.Height;
			if (App.Settings.RememberMinimal && App.Settings.WasMinimalic)
			{
				Player.MinimalViewButton.EmulateClick();
				Resources["MinimalDoubleSys"] = App.Settings.LastSize.Height;
			}
			else
				MinimalOffBoard.Begin();
			SemiLoad();
		}

		private async void SemiLoad()
		{
			while (!Player.IsFullyLoaded)
			{
				await Task.Delay(10);
			}
			var args = Environment.GetCommandLineArgs().Where(name => !name.EndsWith(".exe")).ToArray();
			args.For(each => Manager.Add(each, true));
		}

		private void Player_EventHappened(object sender, InfoExchangeArgs e)
		{
			switch (e.Type)
			{
				case InfoType.NextRequest: Play(Manager.Next()); break;
				case InfoType.PrevRequest: Play(Manager.Previous()); break;
				case InfoType.LengthFound: Manager.CurrentlyPlaying.Length = (TimeSpan)e.Object; break;
				case InfoType.Magnifiement: ControlsNotNeededOnVisionIsVisible = !(bool)e.Object; break;
				case InfoType.CollapseRequest:
					WasMaximized = WindowState == WindowState.Maximized;
					WindowState = WindowState.Normal;
					Resources["MinimalDoubleSys"] = ActualHeight;
					ResizeMode = ResizeMode.CanMinimize;
					MinimalOnBoard.Begin();
					WindowStyle = WindowStyle.ToolWindow;
					break;
				case InfoType.ExpandRequest:
					if (WasMaximized)
						WindowState = WindowState.Maximized;
					ResizeMode = ResizeMode.CanResize;
					WindowStyle = WindowStyle.ThreeDBorderWindow;
					MinimalOffBoard.Begin();
					break;
				default: break;
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			App.Settings.LastSize = new Size(Width, Height <= 130 ? (double)Resources["MinimalDoubleSys"]: Height);
			App.Settings.LastLoc = new Point(Left, Top);
			App.Settings.WasMinimalic = Height <= 130;
			App.Settings.Volume = Player.Volume;
			App.Settings.Save();
			Manager.CloseSeason();
			Application.Current.Shutdown();
		}
		private void Window_KeyUp(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Space:
					if (!SearchBox.IsFocused)
						Player.PlayPause();
					break;
				default: break;
			}
		}
		private void Window_Drop(object sender, DragEventArgs e)
		{
			((string[])e.Data.GetData(DataFormats.FileDrop)).For(each => Manager.Add(each));
		}
		
		private async void Manager_Change(object sender, InfoExchangeArgs e)
		{
			switch (e.Type)
			{
				case InfoType.TagEdit:
					IsEnabled = false;
					var pos = Player.Position;
					Player.Stop();
					await Task.Delay(500);
					try
					{
						(e.Object as TagLib.File).Save();
					}
					catch (IOException)
					{
						MessageBox.Show("I/O Exception occured, try again");
					}
					finally
					{
						Play(sender as Media);
						Player.Position = pos;
						IsEnabled = true;
					}
					break;
				case InfoType.MediaRequest:
					Play(sender as Media);
					break;
				case InfoType.CollectionUpdate:
					RebindViews();
					break;
				default:
					break;
			}
		}

		private async void Keyboard_KeyDown(object sender, Forms::KeyEventArgs e)
		{
			if (IsActive && SearchBox.IsFocused)
				return;
			//Key shortcuts when window is active and main key is down (default: alt)
			if (IsActive && IsAncestorKeyDown(e))
			{
				switch (e.KeyCode)
				{
					case Forms.Keys.Delete: Menu_RemoveClick(this, null); break;
					case Forms.Keys.Enter: DMouseDoubleClick(ActiveView, null); break;
					case Forms.Keys.C: Menu_CopyClick(new MenuItem(), null); break;
					case Forms.Keys.M: Menu_MoveClick(new MenuItem(), null); break;
					case Forms.Keys.P: Menu_PropertiesClick(this, null); break;
					case Forms.Keys.L: Menu_LocationClick(this, null); break;
					case Forms.Keys.F:
						SearchBox.IsEnabled = false;
						SearchBox.Text = "";
						SearchButton.EmulateClick();
						await Task.Delay(100);
						SearchBox.IsEnabled = true;
						SearchBox.Focus();
						break;
					default: break;
				}
			}
			//Key shortcuts whether window is active or main key is down (default: alt)
			if (IsActive || IsAncestorKeyDown(e))
			{
				switch (e.KeyCode)
				{
					case Forms::Keys.Left: Player.SlidePosition(false); break;
					case Forms::Keys.Right: Player.SlidePosition(true); break;
					case Forms::Keys.A:
						var cb = Clipboard.GetText() ?? String.Empty;
						if (Uri.TryCreate(cb, UriKind.Absolute, out var uri))
						{
							Manager.Add(uri.AbsoluteUri);
							if (Manager[0].Type == MediaType.OnlineFile)
								Manager.DownloadManager.Download(Manager[0]);
						}
						else
							return;
						break;
					default: break;
				}
			}
			//Key shortcuts always invokable
			switch (e.KeyCode)
			{
				case Forms::Keys.MediaNextTrack: Player.PlayNext(); break;
				case Forms::Keys.MediaPreviousTrack: Player.PlayPrevious(); break;
				case Forms::Keys.MediaPlayPause: Player.PlayPause(); break;
				default: break;
			}
		}

		private void Play(Media media)
		{
			Player.Play(Manager.Play(media));
			MiniArtworkImage.Source = media.Artwork;
			DeepBackEnd.NativeMethods.SHAddToRecentDocs(DeepBackEnd.NativeMethods.ShellAddToRecentDocsFlags.Path,
				media.Path);
		}
		private bool IsAncestorKeyDown(Forms::KeyEventArgs e)
		{
			switch (App.Settings.MainKey)
			{
				case 0: return e.Control;
				case 1: return e.Alt;
				case 2: return e.Shift;
				case 3: return e.Control && e.Alt;
				case 4: return e.Control && e.Shift;
				case 5: return e.Shift && e.Alt;
				case 6: return e.Control && e.Shift && e.Alt;
				default: return false;
			}
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
			For(item => MediaManager.CleanTag(item));
		}
		private void Menu_PlayAfterClick(object sender, RoutedEventArgs e)
		{
			For(item =>
			{
				Manager.Remove(item);
				Manager.Insert(Manager.IndexOf(Manager.CurrentlyPlaying) + 1, item);
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
					For(item => item.MoveTo(Resources["LastPath"].ToString()));
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
			For(item => PropertiesUI.OpenFor(item, (_, f) =>
			{
				var file = f.Object as TagLib.File;
				if (file.Name == Manager.CurrentlyPlaying.Path)
				{
					var pos = Player.Position;
					Player.Stop();
					file.Save();
					Manager.CurrentlyPlaying.Reload();
					Play(Manager.CurrentlyPlaying);
					Player.Position = pos;
					Manager.UpdateOnPath(item);
				}
				else
				{
					f.Object.As<TagLib.File>().Save();
					Manager.UpdateOnPath(item);
				}
			}));
		}
		private void Menu_DownloadClick(object sender, RoutedEventArgs e)
		{
			For(item => Manager.DownloadManager.Download(item));
		}
		private void Menu_VLC(object sender, RoutedEventArgs e)
		{
			For(item => Process.Start(new ProcessStartInfo(@"C:\Program Files\VideoLAN\VLC\vlc.exe", $"\"{item.Path}\"")));
		}
		
		private Collection<Media> ActiveCollection
		{
			get
			{
				switch (TabControl.SelectedIndex)
				{
					case 0: case 1: return null;
					default: return Collections[TabControl.SelectedIndex - 1];
				}
			}
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
