using Player.Controls;
using Player.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Linq;
using Forms = System.Windows.Forms;
using System.IO;
using System.Windows.Data;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.Diagnostics;

namespace Player
{
    public partial class MainUI : Window
    {
        private MediaManager Manager = new MediaManager();
        private MassiveLibrary Library = new MassiveLibrary();
        private Timer PlayCountTimer = new Timer(100000) { AutoReset = false };
        private Timer SizeChangeTimer = new Timer(50) { AutoReset = true };
        private bool[] IsVisionOn = new bool[] { false, false, false };
      
        
        public MainUI()
        {
            InitializeComponent();
            Manager.Change += Manager_Change;
            App.NewInstanceRequested += (_, e) => Manager.Add(e.Args, true);
            
            var lib = MassiveLibrary.Load();
            for (int i = 0; i < lib.Medias.Length; i++)
                Manager.Add(lib.Medias[i]);
            Width = App.Settings.LastSize.Width;
            Height = App.Settings.LastSize.Height;
            Left = App.Settings.LastLoc.X;
            Top = App.Settings.LastLoc.Y;
            Manager.ActiveQueue = Manager;
        }

        private void BindUI()
        {
            PlayCountTimer.Elapsed += (_, __) => Manager.AddCount();
            SizeChangeTimer.Elapsed += delegate
            {
                Dispatcher.Invoke(
                    delegate
                    {
                        if (Player.Magnified)
                            return;
                    });
                SizeChangeTimer.Stop();
            };
            User.Keyboard.Events.KeyDown += Keyboard_KeyDown;
            Settings_AncestorCombo.SelectedIndex = App.Settings.MainKey;
            Settings_OrinateCheck.IsChecked = App.Settings.VisionOrientation;
            Settings_TimeoutCombo.SelectedIndex = App.Settings.MouseOverTimeoutIndex;
            Settings_AncestorCombo.SelectionChanged += (_, __) => App.Settings.MainKey = Settings_AncestorCombo.SelectedIndex;
            Settings_TimeoutCombo.SelectionChanged += (_, __) => App.Settings.MouseOverTimeoutIndex = Settings_TimeoutCombo.SelectedIndex;
            Settings_OrinateCheck.Checked += (_, __) => App.Settings.VisionOrientation = Settings_OrinateCheck.IsChecked.Value;
            Settings_OrinateCheck.Unchecked += (_, __) => App.Settings.VisionOrientation = Settings_OrinateCheck.IsChecked.Value;
            SettingsButton.MouseUp += delegate
            {
                if (!IsVisionOn[2])
                {
                    (Resources["SettingsOnBoard"] as Storyboard).Begin();
                }
                else
                {
                    (Resources["SettingsOffBoard"] as Storyboard).Begin();
                }
                IsVisionOn[2] = !IsVisionOn[2];
            };

            Player.ParentWindow = this;
            TaskbarItemInfo = Player.Thumb.Info;

            Resources["LastPath"] = App.Settings.LastPath;

            RebindViews();
            ///BindingOperations.EnableCollectionSynchronization(Manager.Items, QueueListView);
            Manager.CollectionChanged += Manager_CollectionChanged;
            ArtistsView.MouseDoubleClick += DMouseDoubleClick;
            TitlesView.MouseDoubleClick += DMouseDoubleClick;
            AlbumsView.MouseDoubleClick += DMouseDoubleClick;
        }

        CollectionView[] Views = new CollectionView[2];
        PropertyGroupDescription[] Descriptions = new PropertyGroupDescription[2];
        private void RebindViews()
        {
            TitlesView.ItemsSource = Manager.VariousSources[0];
            ArtistsView.ItemsSource = Manager.VariousSources[1];
            AlbumsView.ItemsSource = Manager.VariousSources[2];

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

        private void Manager_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

        }

        private void Item_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var test = sender as Media;
            Console.WriteLine(test);

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BindUI();
            var cml = Environment.GetCommandLineArgs().Where(name => !name.EndsWith(".exe")).ToArray();
            Manager.Add(cml, true);
            Player.EventHappened += Player_EventHappened;
        }

        private void Player_EventHappened(object sender, InfoExchangeArgs e)
        {
            switch (e.Type)
            {
                case InfoType.NextRequest: Play(Manager.Next()); break;
                case InfoType.PrevRequest: Play(Manager.Previous()); break;
                case InfoType.LengthFound: Manager.CurrentlyPlaying.Length = (TimeSpan)e.Object; break;
                default: break;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App.Settings.LastSize = new Size(Width, Height);
            App.Settings.LastLoc = new Point(Left, Top);
            App.Settings.Volume = Player.Volume;
            App.Settings.Save();
            Manager.Close();
            Application.Current.Shutdown();
        }
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    Player.PlayPause();
                    break;
                default: break;
            }
        }
        private void Window_Drop(object sender, DragEventArgs e) => Manager.Add((string[])e.Data.GetData(DataFormats.FileDrop));
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SizeChangeTimer.Start();
            Player.Size_Changed(this, null);
        }

        private void Media_ZipDownloaded(object sender, InfoExchangeArgs e)
        {
            Manager.Remove(sender as Media);
            Manager.Add((string[])e.Object);
        }
       
        private async void Manager_Change(object sender, InfoExchangeArgs e)
        {
            switch (e.Type)
            {
                case InfoType.EditingTag:
                    var pos = Player.Position;
                    Player.FullStop();
                    await Task.Delay(500);
                    (e.Object as TagLib.File).Save();
                    Play(e.Object as Media);
                    Player.Position = pos;
                    break;
                case InfoType.MediaRequested:
                    Play(sender as Media);
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
                    default: break;
                }
            }
            //Key shortcuts whether window is active or main key is down (default: alt)
            if (IsActive || IsAncestorKeyDown(e))
            {
                switch (e.KeyCode)
                {
                    case Forms::Keys.Left:
                        Player.SmallSlideLeft();
                        await Task.Delay(200);
                        break;
                    case Forms::Keys.Right:
                        Player.SmallSlideRight();
                        await Task.Delay(200);
                        break;
                    case Forms::Keys.A:
                        var cb = Clipboard.GetText() ?? String.Empty;
                        if (Uri.TryCreate(cb, UriKind.Absolute, out var uri))
                            Manager.Add(uri.AbsoluteUri);
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

        private void AnySettingChanged(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
                return;
            App.Settings.Save();
        }

        private void Play(Media media)
        {
            Player.FullStop();
            Player.Play(Manager.Play(Manager.IndexOf(media)));
            if (Manager.IsFiltered)
                Manager.ActiveQueue = Manager.VariousSources[TabControl.SelectedIndex];
            else
                Manager.ActiveQueue = Manager;
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
            Manager.FilterVariousSources(SearchBox.Text);
            RebindViews();
        }

        private void SearchIcon_MouseEnter(object sender, MouseEventArgs e)
        {
            SearchPopup.IsOpen = true;
        }

        private void Menu_PlayAfterClick(object sender, RoutedEventArgs e)
        {
            if (App.Settings.PlayMode != PlayMode.RepeatAll)
            {
                MessageBox.Show("Hey, illegal on shuffle or repeat-single playing mode", "WHAT THA FUCK", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            For(item => Manager.Move(Manager.IndexOf(item), Manager.CurrentlyPlayingIndex + 1));
        }
        private void Menu_DuplicateClick(object sender, RoutedEventArgs e)
        {
            var d = Int32.Parse(e.Source.As<MenuItem>().Header.ToString());
            for (int i = 0; i < d; i++)
                For(item => Manager.Insert(Manager.IndexOf(item), item.Shallow));
        }
        private void Menu_MoveClick(object sender, RoutedEventArgs e)
        {
            switch (e.Source.As<MenuItem>().Header ?? "INDIV".ToString().Substring(0, 1).ToLower())
            {
                case "b":
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
            switch (e.Source.As<MenuItem>().Header ?? "INDIV".ToString().Substring(0, 1).ToLower())
            {
                case "b":
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
            For(item => File.Delete(item.Path));
            For(item => Manager.Remove(item));
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
                    Player.FullStop();
                    file.Save();
                    Manager.CurrentlyPlaying.Reload();
                    Play(Manager.CurrentlyPlaying);
                    Player.Seek(pos);
                    Manager.UpdateOnPath(item);
                }
                else
                {
                    f.Object.As<TagLib.File>().Save();
                    Manager.UpdateOnPath(item);
                }
            }));
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
                    default: return null;
                }
            }
        }
        private void For(Action<Media> action)
        {
            ActiveView.SelectedItems.Cast<Media>().ToList().ForEach(action);
        }

        private async void RevalidateLibraryClick(object sender, RoutedEventArgs e)
        {
            sender.As<Button>().Content = "Revalidating... will restart soon";
            IsEnabled = false;
            await Task.Delay(2000);
            Hide();
            Player.FullStop();
            Manager.Revalidate();
            Close();
            Process.Start(App.Path + "Elephant Player.exe");
        }

        private void ResetRatingsClick(object sender, RoutedEventArgs e)
        {
            foreach (var item in Manager)
                item.Rate = 0;
            Close();
            Process.Start(App.Path + "Elephant Player.exe");
        }
    }
}
