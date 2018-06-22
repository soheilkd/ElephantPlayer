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
using Forms = System.Windows.Forms;

namespace Player
{
	public partial class MainWindow : Window
	{
		private MediaManager Manager = new MediaManager();
		private ObservableCollection<Media>[] Collections = new ObservableCollection<Media>[5];
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

		public MainWindow()
		{
			InitializeComponent();
			Initialize();
			Width = App.Settings.LastSize.Width;
			Height = App.Settings.LastSize.Height;
			Left = App.Settings.LastLoc.X;
			Top = App.Settings.LastLoc.Y;
		}

		private void Initialize()
		{
			PlayCountTimer.Elapsed += (_, __) => Manager.CurrentlyPlaying.PlayCount++;
			App.NewInstanceRequested += (_, e) => e.Args.ToList().ForEach(each => Manager.Add(each, true));
			KeyboardEvents.KeyDown += Keyboard_KeyDown;
			Manager.Change += Manager_Change;
			Collections[0] = new ObservableCollection<Media>();
			Collections[1] = LibraryOperator.LoadedCollection.ByArtist;
			Collections[2] = LibraryOperator.LoadedCollection.ByAlbum;
			Collections[3] = LibraryOperator.LoadedCollection.ByType;
			Collections[4] = LibraryOperator.LoadedCollection.ByDirectory;
			TitlesView.ItemsSource = Manager;
			ArtistsView.ItemsSource = Collections[1];
			AlbumsView.ItemsSource = Collections[2];
			TypesView.ItemsSource = Collections[3];
			DirectoryView.ItemsSource = Collections[4];
			Settings_AncestorCombo.SelectedIndex = App.Settings.MainKey;
			Settings_OrinateCheck.IsChecked = App.Settings.VisionOrientation;
			Settings_LiveLibraryCheck.IsChecked = App.Settings.LiveLibrary;
			Settings_ExplicitCheck.IsChecked = App.Settings.ExplicitContent;
			Settings_PlayOnPosCheck.IsChecked = App.Settings.PlayOnPositionChange;
			Settings_RevalidOnExitCheck.IsChecked = App.Settings.RevalidateOnExit;
			Settings_TimeoutCombo.SelectedIndex = App.Settings.MouseOverTimeoutIndex;
			Settings_AncestorCombo.SelectionChanged += (_, __) => App.Settings.MainKey = Settings_AncestorCombo.SelectedIndex;
			Settings_TimeoutCombo.SelectionChanged += (_, __) => App.Settings.MouseOverTimeoutIndex = Settings_TimeoutCombo.SelectedIndex;
			Settings_OrinateCheck.Checked += (_, __) => App.Settings.VisionOrientation = true;
			Settings_OrinateCheck.Unchecked += (_, __) => App.Settings.VisionOrientation = false;
			Settings_LiveLibraryCheck.Checked += (_, __) => App.Settings.LiveLibrary = true;
			Settings_LiveLibraryCheck.Unchecked += (_, __) => App.Settings.LiveLibrary = false;
			Settings_ExplicitCheck.Checked += (_, __) => App.Settings.ExplicitContent = true;
			Settings_ExplicitCheck.Unchecked += (_, __) => App.Settings.ExplicitContent = false;
			Settings_PlayOnPosCheck.Checked += (_, __) => App.Settings.PlayOnPositionChange = true;
			Settings_PlayOnPosCheck.Unchecked += (_, __) => App.Settings.PlayOnPositionChange = false;
			Settings_RevalidOnExitCheck.Checked += (_, __) => App.Settings.RevalidateOnExit = true;
			Settings_RevalidOnExitCheck.Unchecked += (_, __) => App.Settings.RevalidateOnExit = false;

			Player.ParentWindow = this;
			TaskbarItemInfo = Player.Thumb.Info;

			Resources["LastPath"] = App.Settings.LastPath;

			RebindViews();
			ArtistsView.MouseDoubleClick += DMouseDoubleClick;
			TitlesView.MouseDoubleClick += DMouseDoubleClick;
			AlbumsView.MouseDoubleClick += DMouseDoubleClick;
			Manager.CollectionChanged += Manager_CollectionChanged;
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
			TypesView.ItemsSource = Collections[3];
			DirectoryView.ItemsSource = Collections[4];
			Views[0] = (CollectionView)CollectionViewSource.GetDefaultView(ArtistsView.ItemsSource);
			Descriptions[0] = new PropertyGroupDescription("Artist");
			Views[0].GroupDescriptions.Add(Descriptions[0]);

			Views[1] = (CollectionView)CollectionViewSource.GetDefaultView(AlbumsView.ItemsSource);
			Descriptions[1] = new PropertyGroupDescription("Album");
			Views[1].GroupDescriptions.Add(Descriptions[1]);

			Views[2] = (CollectionView)CollectionViewSource.GetDefaultView(TypesView.ItemsSource);
			Descriptions[2] = new PropertyGroupDescription("Type.ToString()");
			Views[2].GroupDescriptions.Add(Descriptions[2]);

			Views[3] = (CollectionView)CollectionViewSource.GetDefaultView(DirectoryView.ItemsSource);
			Descriptions[3] = new PropertyGroupDescription("Directory");
			Views[3].GroupDescriptions.Add(Descriptions[3]);
		}

		private void DMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (sender.As<ListView>().SelectedItem is Media med)
				Play(med);
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			var args = Environment.GetCommandLineArgs().Where(name => !name.EndsWith(".exe")).ToArray();
			args.For(each => Manager.Add(each, true));
			Player.SomethingHappened += Player_EventHappened;
		}

		private void Player_EventHappened(object sender, InfoExchangeArgs e)
		{
			switch (e.Type)
			{
				case InfoType.NextRequest: Play(Manager.Next()); break;
				case InfoType.PrevRequest: Play(Manager.Previous()); break;
				case InfoType.LengthFound: Manager.CurrentlyPlaying.Length = (TimeSpan)e.Object; break;
				case InfoType.Magnifiement: ControlsNotNeededOnVisionIsVisible = !(bool)e.Object; break;
				default: break;
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{

			App.Settings.LastSize = new Size(Width, Height);
			App.Settings.LastLoc = new Point(Left, Top);
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
		private void Menu_ConvertClick(object sender, RoutedEventArgs e)
		{
			For(item => ConverterWindow.Convert(item, media => Manager.Add(media)));
		}
		private void Menu_DownloadClick(object sender, RoutedEventArgs e)
		{
			For(item => Manager.DownloadManager.Download(item));
		}
		private void Menu_VLC(object sender, RoutedEventArgs e)
		{
			For(item => Process.Start(new ProcessStartInfo(@"C:\Program Files\VideoLAN\VLC\vlc.exe", $"\"{item.Path}\"")));
		}

		private void AnySettingChanged(object sender, RoutedEventArgs e)
		{
			if (!IsLoaded)
				return;
			App.Settings.Save();
		}
		private async void Settings_RevalidateClick(object sender, RoutedEventArgs e)
		{
			sender.As<Button>().Content = "Revalidating... will restart soon";
			IsEnabled = false;
			await Task.Delay(2000);
			Hide();
			Player.Stop();
			Manager.Revalidate();
			Close();
			Process.Start(App.Path + "Elephant Player.exe");
		}

		private ListView ActiveView
		{
			get
			{
				switch (TabControl.SelectedIndex)
				{
					case 0: return TitlesView;
					case 1: return ArtistsView;
					case 2: return AlbumsView;
					case 3: return TypesView;
					case 4: return DirectoryView;
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
