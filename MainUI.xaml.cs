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
            App.NewInstanceRequested += (_, e) => Manager.Add(e.Args);
            
            var lib = MassiveLibrary.Load();
            for (int i = 0; i < lib.Medias.Length; i++)
                Manager.Add(lib.Medias[i]);
            Width = App.Preferences.LastSize.Width;
            Height = App.Preferences.LastSize.Height;
            Left = App.Preferences.LastLoc.X;
            Top = App.Preferences.LastLoc.Y;
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
            Settings_AncestorCombo.SelectedIndex = App.Preferences.MainKey;
            Settings_OrinateCheck.IsChecked = App.Preferences.VisionOrientation;
            Settings_TimeoutCombo.SelectedIndex = App.Preferences.MouseOverTimeout;
            Settings_AncestorCombo.SelectionChanged += (_, __) => App.Preferences.MainKey = Settings_AncestorCombo.SelectedIndex;
            Settings_TimeoutCombo.SelectionChanged += delegate
            {
                App.Preferences.MouseOverTimeout = Settings_TimeoutCombo.SelectedIndex;
                switch (Settings_TimeoutCombo.SelectedIndex)
                {
                    case 0: App.Preferences.MouseOverTimeout = 500; break;
                    case 1: App.Preferences.MouseOverTimeout = 1000; break;
                    case 2: App.Preferences.MouseOverTimeout = 2000; break;
                    case 3: App.Preferences.MouseOverTimeout = 3000; break;
                    case 4: App.Preferences.MouseOverTimeout = 4000; break;
                    case 5: App.Preferences.MouseOverTimeout = 5000; break;
                    case 6: App.Preferences.MouseOverTimeout = 10000; break;
                    case 7: App.Preferences.MouseOverTimeout = 60000; break;
                    default: App.Preferences.MouseOverTimeout = 5000; break;
                }
            };
            Settings_OrinateCheck.Checked += (_, __) => App.Preferences.VisionOrientation = Settings_OrinateCheck.IsChecked.Value;
            Settings_OrinateCheck.Unchecked += (_, __) => App.Preferences.VisionOrientation = Settings_OrinateCheck.IsChecked.Value;
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
                case InfoType.PlayModeChange: Manager.ActivePlayMode = (PlayMode)e.Object; break;
                default: break;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App.Preferences.LastSize = new Size(Width, Height);
            App.Preferences.LastLoc = new Point(Left, Top);
            App.Preferences.Volume = Player.Volume;
            App.Preferences.Save();
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
                OnSelectedMediaViews(each => selectedFilesInString += $"\r\n{each.Media.Path}");
                var res = MessageBox.Show($"Sure? this files will be deleted:{selectedFilesInString}", " ", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (res == MessageBoxResult.OK)
                {
                    var paths = SelectedMediaViews.Select(item => item.Media.Path);
                    var views = SelectedMediaViews.ToList();
                    views.ForEach(item => item.RemoveRequest(this, null));
                    paths.AsParallel().ForAll(item => File.Delete(item));
                }
            }
            else
                (sender as MediaView).DeleteRequest(this, null);
        }
        private void Media_LocationRequested(object sender, InfoExchangeArgs e)
        {
            OnSelectedMediaViews(each => each.LocationRequest(this, null));
        }
        private void Media_RemoveRequested(object sender, InfoExchangeArgs e)
        {
            OnSelectedMediaViews(each => Manager.Remove(each));
        }
        private void Media_PropertiesRequested(object sender, InfoExchangeArgs e)
        {
            OnSelectedMediaViews(each => each.PropertiesRequest(this, null));
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
                case InfoType.NewMedia:
                    QueueListView.Items.Add(e.Object as MediaView);
                    Height--; Height++;
                    break;
                case InfoType.EditingTag:
                    var pos = Player.Position;
                    Player.FullStop();
                    await Task.Delay(500);
                    (e.Object as TagLib.File).Save();
                    Play(e.Object as MediaView);
                    Player.Position = pos;
                    break;
                case InfoType.MediaRemoved:
                    QueueListView.Items.Clear();
                    for (int i = 0; i < Manager.Count; i++)
                        QueueListView.Items.Add(Manager[i]);
                    break;
                case InfoType.MediaRequested:
                    Play(Manager.Next(sender));
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
                            Manager.Add(uri);
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
            App.Preferences.Save();
        }

        private void Play(MediaView mediaView)
        {
            Player.Play(mediaView.Media);
            MiniArtworkImage.Source = mediaView.Media.Artwork;
            Manager.ResetIsPlayings();
            mediaView.IsPlaying = true;
        }
        private bool IsAncestorKeyDown(Forms::KeyEventArgs e)
        {
            switch (App.Preferences.MainKey)
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
        
        private void OnSelectedMediaViews(Action<MediaView> action) => QueueListView.SelectedItems.Cast<MediaView>().AsParallel().ForAll(action);
        private IEnumerable<MediaView> SelectedMediaViews => QueueListView.SelectedItems.Cast<MediaView>();
    }
}
