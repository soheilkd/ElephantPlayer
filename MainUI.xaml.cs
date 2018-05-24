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

namespace Player
{
    public partial class MainUI : Window
    {
        private MediaManager Manager = new MediaManager();
        private MassiveLibrary Library = new MassiveLibrary();
        private Timer PlayCountTimer = new Timer(100000) { AutoReset = false };
        private Timer SizeChangeTimer = new Timer(50) { AutoReset = true };
        private bool[] IsVisionOn = new bool[] { false, false, false };
        private MenuItem[] MultipleSelectionMenuItems;
        
        public MainUI()
        {
            InitializeComponent();
            Manager.ContextMenuOpening += Manager_ContextMenuOpening;
            Manager.ContextMenuClosing += Manager_ContextMenuClosing;
            Manager.Change += Manager_Change;
            App.NewInstanceRequested += (_, e) => Manager.Add(e.Args, true);
            
            var lib = MassiveLibrary.Load();
            for (int i = 0; i < lib.Medias.Length; i++)
                Manager.Add(lib.Medias[i]);
            Width = App.Settings.LastSize.Width;
            Height = App.Settings.LastSize.Height;
            Left = App.Settings.LastLoc.X;
            Top = App.Settings.LastLoc.Y;
        }

        private void Manager_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {
            e.Source.As<MediaView>().ContextMenu.ItemsSource = e.Source.As<MediaView>().OriginMenuItems;
        }

        private void Manager_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (SelectedItems.Count() == 1)
                return;
            else
            {
                e.Source.As<MediaView>().ContextMenu.ItemsSource = MultipleSelectionMenuItems;
            }
        }

        private void BindUI()
        {
            PlayCountTimer.Elapsed += (_, __) => Manager.AddCount();
            (Resources["SettingsOnBoard"] as Storyboard).CurrentStateInvalidated += (_, __) => SettingsGrid.Visibility = Visibility.Visible;
            (Resources["SettingsOffBoard"] as Storyboard).Completed += (_, __) => SettingsGrid.Visibility = Visibility.Hidden;
            SettingsGrid.Visibility = Visibility.Hidden;
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
            Settings_TimeoutCombo.SelectedIndex = App.Settings.MouseOverTimeout;
            Settings_AncestorCombo.SelectionChanged += (_, __) => App.Settings.MainKey = Settings_AncestorCombo.SelectedIndex;
            Settings_TimeoutCombo.SelectionChanged += delegate
            {
                App.Settings.MouseOverTimeout = Settings_TimeoutCombo.SelectedIndex;
                switch (Settings_TimeoutCombo.SelectedIndex)
                {
                    case 0: App.Settings.MouseOverTimeout = 500; break;
                    case 1: App.Settings.MouseOverTimeout = 1000; break;
                    case 2: App.Settings.MouseOverTimeout = 2000; break;
                    case 3: App.Settings.MouseOverTimeout = 3000; break;
                    case 4: App.Settings.MouseOverTimeout = 4000; break;
                    case 5: App.Settings.MouseOverTimeout = 5000; break;
                    case 6: App.Settings.MouseOverTimeout = 10000; break;
                    case 7: App.Settings.MouseOverTimeout = 60000; break;
                    default: App.Settings.MouseOverTimeout = 5000; break;
                }
            };
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
            MultipleSelectionMenuItems = new MenuItem[]
            {
                Global.GetMenu("Remove", (_,__) => For(each => Manager.Remove(each)))
            };
            QueueListView.ItemsSource = Manager;
           
            ///BindingOperations.EnableCollectionSynchronization(Manager.Items, QueueListView);
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
                case InfoType.DragMoveRequest: DragMove(); break;
                case InfoType.NextRequest: Play(Manager.Next()); break;
                case InfoType.PrevRequest: Play(Manager.Previous()); break;
                case InfoType.Handling:
                    break;
                case InfoType.UserInterface:
                    break;
                case InfoType.LengthFound:
                    Manager.CurrentlyPlaying.UpdateLength((TimeSpan)e.Object);
                    break;
                case InfoType.PlayModeChange: App.Settings.PlayMode = (PlayMode)e.Object; break;
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

        private void Media_DeleteRequested(object sender, InfoExchangeArgs e)
        {
            if (QueueListView.SelectedItems.Count > 1)
            {
                string selectedFilesInString = "";
                For(each => selectedFilesInString += $"\r\n{each.Media.Path}");
                var res = MessageBox.Show($"Sure? this files will be deleted:{selectedFilesInString}", " ", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (res == MessageBoxResult.OK)
                {
                    var paths = SelectedItems.Select(item => item.Media.Path);
                    var views = SelectedItems.ToList();
                    For(each => Manager.Remove(each));
                    paths.AsParallel().ForAll(item => File.Delete(item));
                }
            }
            else
            {
                File.Delete((sender as MediaView).Media.Path);
                Manager.Remove(sender as MediaView);
            }
        }
        private void Media_ZipDownloaded(object sender, InfoExchangeArgs e)
        {
            Manager.Remove(sender as MediaView);
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
                    Play(e.Object.As<MediaView>().Media);
                    Player.Position = pos;
                    break;
                case InfoType.MediaRequested:
                    Play(Manager.Play(sender));
                    break;
                default:
                    break;
            }
        }
        private async void Keyboard_KeyDown(object sender, Forms::KeyEventArgs e)
        {
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
                        QueueListView.ScrollIntoView(QueueListView.Items[QueueListView.Items.Count - 1]);
                        break;
                    default: break;
                }
            }
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
            Player.Play(media);
            MiniArtworkImage.Source = media.Artwork;
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
        
        private void For(Action<MediaView> action)
        {
            foreach (var item in SelectedItems)
                action.Invoke(item);
        }
        private IEnumerable<MediaView> SelectedItems => QueueListView.SelectedItems.Cast<MediaView>();
    }
}
