using Player.Controls;
using Player.Enums;
using Player.Events;
using Player.Extensions;
using Player.InstanceManager;
using Player.Taskbar;
using Player.Types;
using Player.User;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static Player.User.Keyboard;
using I = MaterialIcons.MaterialIconType;
using Routed = System.Windows.RoutedPropertyChangedEventArgs<double>;
using W = System.Windows.Forms;
//#pragma warning disable CS4014
namespace Player
{
    public partial class App : Application, ISingleInstanceApp
    {
        public static event EventHandler<InstanceEventArgs> NewInstanceRequested;
        public const string LauncherIdentifier = "ElephantPlayerBySoheilKD_VERIFIED";
        public static string ExeFullPath = Environment.GetCommandLineArgs()[0];
        public static string ExePath = ExeFullPath.Substring(0, ExeFullPath.LastIndexOf("\\") + 1);
        public static string PrefPath = $"{ExePath}Settings.dll";
        public static string AppName = $"ELPWMP";
        [STAThread]
        public static void Main(string[] args)
        {
            if (!Environment.MachineName.Equals("Soheil-PC", StringComparison.CurrentCultureIgnoreCase) && !File.Exists($"{ExePath}\\Ejaze.xml"))
            {
                var res = W.MessageBox.Show("Bedune ejaze ?", "Syyyset nemikhore",
                    W.MessageBoxButtons.YesNo, W.MessageBoxIcon.Warning, W.MessageBoxDefaultButton.Button1, W.MessageBoxOptions.ServiceNotification);
                return;
            }
            if (Instance<App>.InitializeAsFirstInstance(LauncherIdentifier))
            {
                var application = new App();

                application.InitializeComponent();
                application.Run();
                Instance<App>.Cleanup();
            }
        }

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            NewInstanceRequested?.Invoke(this, new InstanceEventArgs(args));
            return true;
        }
    }
    public partial class MainUI : Window
    {
        #region Window & Mini Interface

        private List<MaterialButton> MainSegoeButtons = new List<MaterialButton>();
        private bool UserResizingWindow = false;
        public MainUI()
        {
            InitializeComponent();
            MainSettings.Reload();
            App.NewInstanceRequested += App_NewInstanceRequested;
            Loaded += MainUI_Loaded;
            var args = Environment.GetCommandLineArgs();
            if (args.Length != 0)
                Load(args, true);
        }

        private List<MediaView> MediaViews = new List<MediaView>();
        ushort UpdaterLatency = 0;
        TrayControl TrayController = new TrayControl();
        private void MainUI_Loaded(object sender, RoutedEventArgs e)
        {
            #region Initializing Controls
            TrayController.SwitchViewClicked += ContextSwitchView;
            TrayController.ClearViewClicked += (_, __) => { MediaViews.Clear(); RebuildView(new List<UIElement>(), new Size(1, 1)); GC.Collect(); GC.WaitForPendingFinalizers(); };
            TrayController.SettingsClicked += (_, __) => SettingsUI.Show();
            TrayController.CallGCClicked += (_, __) => { GC.Collect(); GC.WaitForPendingFinalizers(); };
            TrayController.ExitClicked += (_, __) => UserExiting(_, null);
            ((MenuItem)PlayerC_PlaybackMenu.Items[0]).Click += (_, __) => PlayPauseButtonClick(_, null);
            ((MenuItem)PlayerC_PlaybackMenu.Items[1]).Click += (_, __) => Play(PlayPara.Prev);
            ((MenuItem)PlayerC_PlaybackMenu.Items[2]).Click += (_, __) => Play(PlayPara.Next);
            ((MenuItem)PlayerC_PlaybackMenu.Items[3]).Click += (_, __) => SwitchMode(WindowMode.Default);
            ((MenuItem)PlayerC_PlaybackMenu.Items[4]).Click += (_, __) => SettingsUI.Show();
            Thumb.PlayPressed += (_, __) => PlayPauseButtonClick(_, null);
            Thumb.PausePressed += (_, __) => PlayPauseButtonClick(_, null);
            Thumb.PrevPressed += (_, __) => Play(PlayPara.Prev);
            Thumb.NextPressed += (_, __) => Play(PlayPara.Next);
            ReturnButton.Click += (_, __) => SwitchMode(WindowMode.Default);
            VideoButton.Click += (_, __) => SwitchMode(WindowMode.VideoWithControls);
            AudioButton.Click += (_, __) => VolumePopup.IsOpen = true;
            NextButton.Click += (_, __) => Play(PlayPara.Next);
            PreviousButton.Click += (_, __) => Play(PlayPara.Prev);
            FunctionButton.MouseUp += (_, __) => FunctionPopup.IsOpen = true;
            UniversalMetaEditor.SaveRequested += MainUI_TagSaveRequested1;
            UniversalMetaEditor.PreviousTagRequested += MainUI_PreviousTagRequested;
            UniversalMetaEditor.NextTagRequested += MainUI_NextTagRequested;
            #endregion
            ElementTimer.Elapsed += ElementTimer_Elapsed;
            Keyboard.KeyDown += Keyboard_KeyDown;
            Keyboard.KeyUp += Keyboard_KeyUp;
            foreach (MaterialButton sb in UI.FindVisualChildren<MaterialButton>(MainGrid))
                MainSegoeButtons.Add(sb); 
            SettingsChanged(this, new SettingsEventArgs() { NewSettings = MainSettings });
            switch (MainSettings.LatencyIndex)
            {
                case 0: UpdaterLatency = 1000; break;
                case 1: UpdaterLatency = 900; break;
                case 2: UpdaterLatency = 750; break;
                case 3: UpdaterLatency = 450; break;
                case 4: UpdaterLatency = 250; break;
                case 5: UpdaterLatency = 225; break;
                case 6: UpdaterLatency = 200; break;
                case 7: UpdaterLatency = 150; break;
                case 8: UpdaterLatency = 125; break;
                case 9: UpdaterLatency = 75; break;
                case 10: UpdaterLatency = 50; break;
                case 11: UpdaterLatency = 25; break;
                case 12: UpdaterLatency = 1; break;
                default: throw new InvalidDataException(nameof(UpdaterLatency));
            }
            TaskbarItemInfo = Thumb.Info;
            Thumb.Refresh(true);

            SettingsUI.SettingsChanged += SettingsChanged;
            SettingsUI.PlaybackSettingsChanged += SettingsUI_PlaybackSettingsChanged;
            ActiveViewMode = (ViewMode)MainSettings.ViewMode;
            UserExperience();
           
        }

        private void ElementTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ElementTimer.Stop();
            ElementTimer.Enabled = false;
        }
        private void App_NewInstanceRequested(object sender, InstanceEventArgs e)
        {
            string[] args = new string[e.ArgsCount];
            for (int i = 0; i < args.Length; i++)
                args[i] = e[i];
            Load(args);
        }
        private void SettingsUI_PlaybackSettingsChanged(object sender, PlaybackEventArgs e)
        {
            Player.Stretch = e.Stretch;
            Player.StretchDirection = e.StretchDirection;
            if (Player.SpeedRatio != e.SpeedRatio)
            {
                Player.SpeedRatio = e.SpeedRatio;
                ForcePositionChange(-2);
            }
            Player.Balance = e.SpeakerBalance;
        }
        private void SettingsChanged(object sender, SettingsEventArgs e)
        {
            MainSettings = e.NewSettings;
            ReloadBrush();
            CurrentPlayMode = (PlayMode)e.NewSettings.PlayMode;
            SearchEnabled = e.NewSettings.Search;
            if (e.NewSettings.Search)
                SearchExpander.Visibility = Visibility.Visible;
            else
            {
                SearchExpander.IsExpanded = false;
                SearchExpander.Visibility = Visibility.Hidden;
            }

            for (int i = 0; i < MediaViews.Count; i++)
                MediaViews[i].Change(e);
            MinHeight = Styling.XAML.Size.MediaView.Self((MediaViewMode)MainSettings.TileScheme).h + 100;
            MinWidth = Styling.XAML.Size.MediaView.Self((MediaViewMode)MainSettings.TileScheme).w + 30;
            RebuildView();
        }
        private void FileDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) Load((string[])e.Data.GetData(DataFormats.FileDrop), false);
        }
        private void Keyboard_KeyUp(object sender, KeyPressEventArgs e)
        {
            if (window.IsActive && e.Key == KeyboardKey.Space) PlayPauseButtonClick(this, null);
        }
        private void Keyboard_KeyDown(object sender, KeyPressEventArgs e)
        {
            if (window.IsActive || e.Alt)
            {
                if (!PositionSlider.IsFocused)
                {
                    switch (e.Key)
                    {
                        case KeyboardKey.Left: ForcePositionChange(PositionSlider.SmallChange * -1); break;
                        case KeyboardKey.Right: ForcePositionChange(PositionSlider.SmallChange); break;
                        default: break;
                    }
                }
            }
            switch (e.Key)
            {
                case KeyboardKey.MediaNext: Play(PlayPara.Next); break;
                case KeyboardKey.MediaPrevious: Play(PlayPara.Prev); break;
                case KeyboardKey.MediaStop: SwitchMode(WindowMode.Default); break;
                case KeyboardKey.MediaPlayPause: PlayPauseButtonClick(this, null); break;
                default: break;
            }
        }
        private void UserExiting(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TrayController.Dispose();
            Keyboard.Dispose();
            Application.Current.Shutdown();
        }
        private async Task UserExperience()
        {
            UX:
            await Task.Delay(UpdaterLatency);
            if (Player.NaturalDuration.HasTimeSpan) if (Player.NaturalDuration.TimeSpan != TimeSpan)
                {
                    //Update TimeSpan
                    TimeSpan = Player.NaturalDuration.TimeSpan;
                    PositionSlider.Maximum = TimeSpan.TotalMilliseconds;
                    PositionSlider.SmallChange = 1 * PositionSlider.Maximum / 100;
                    PositionSlider.LargeChange = 5 * PositionSlider.Maximum / 100;
                    try { CurrentlyPlaying.Max = PositionSlider.Maximum; } catch (NullReferenceException) { }
                }
            PositionSlider.Value = Player.Position.TotalMilliseconds;
            TimeLabel.Content = ConvertTime(Player.Position);
            (CurrentlyPlaying ?? null).Progress = PositionSlider.Value;
            if (PositionSlider.Value >= PositionSlider.Maximum - 250)
                Play(PlayPara.Next);
            goto UX;
        }
        #endregion
        #region Variables
        public static DependencyProperty SwitchmentDependency =
            DependencyProperty.Register("Switchment", typeof(bool), typeof(MainUI), new PropertyMetadata(false));
        public bool Switchment { get => (bool)GetValue(SwitchmentDependency); set => SetValue(SwitchmentDependency, value); }
        private List<GroupView> GroupViews = new List<GroupView>();
        private double LastWidth;
        private double LastHeight;
        private double LastLeft;
        private System.Timers.Timer ElementTimer = new System.Timers.Timer(250) { AutoReset = false };
        private ViewMode activeViewMode;
        private ViewMode ActiveViewMode { get => activeViewMode; set
            {
                activeViewMode = value;
                if (value == ViewMode.Singular)
                {
                    for (int i = 0; i < GroupViews.Count; i++)
                    {
                        GroupViews[i].Items.Clear();
                    }
                    GroupViews.Clear();
                    RebuildView(MediaViews, new Size()
                    {
                        Height = Styling.XAML.Size.MediaView.Self((MediaViewMode)MainSettings.TileScheme).h,
                        Width = Styling.XAML.Size.MediaView.Self((MediaViewMode)MainSettings.TileScheme).w
                    });
                }
                else
                {
                    RebuildView(new List<GroupView>(), new Size(1, 1));
                    GroupViews.Clear();
                    for (int i = 0; i < MediaViews.Count; i++)
                    {
                        int index = -1;
                        for (int j = 0; j < GroupViews.Count; j++)
                            if (GroupViews[j].DoesMatch(MediaViews[i].Media, ActiveViewMode))
                                index = j;
                        if (index == -1)
                        {
                            GroupViews.Add(new GroupView(MediaViews[i], MainSettings.TileTheme, ActiveViewMode));
                        }
                        else
                        {
                            GroupViews[index].Add(MediaViews[i]);
                        }
                    }
                    RebuildView(GroupViews, Styling.XAML.Size.GroupView.Self);
                }
            } }
        private double LastTop;
        private bool FullScreen = false;
        private bool SearchEnabled;
        private int ViewRows = 0;
        private int ViewColumns = 0;
        private int CapableColumns = 0;
        private int CapableRows = 0;
        private int GeneralCounter = 0;
        private SettingsUI SettingsUI = new SettingsUI();
        private Preferences MainSettings = new Preferences(true);
        private TimeSpan TimeSpan;
        private PlayMode CurrentPlayMode = PlayMode.RepeatAll;
        private MediaView CurrentlyPlaying = new MediaView();
        private Random Shuffle = new Random(0);
        private Random MediaViewRandom = new Random(100);
        private Boolean UserChangingPosition = false;
        private Thumb Thumb = new Thumb();
        private MediaView ActivePopup;
        private WindowState _State;
        private User.Keyboard Keyboard = new User.Keyboard();
        private (int mins, int secs) TimeConvertion;
        private MetaEditor UniversalMetaEditor = new MetaEditor();
        private LyricsView UniversalLyricsView = new LyricsView();

        private int RebuildHeight;
        private int RebuildWidth;
        #endregion
        #region Media Controls
        void MediaArtworkClicked(object sender, MediaEventArgs e)
        {
            if (CurrentlyPlaying == sender) PlayPauseButtonClick(this, null);
            else Play(e.Sender);
        }
        void PositionChanged(object sender, Routed e)
        {
            if (UserChangingPosition) Player.Position = new TimeSpan(0, 0, 0, 0, PositionSlider.Value.ToInt());
        }
        async void PositionMouseDown(object sender, MouseButtonEventArgs e)
        {
            UserChangingPosition = true;
            while (e.ButtonState == MouseButtonState.Pressed)
            {
                Player.Pause();
                await Task.Delay(300);
                Player.Play();
                await Task.Delay(50);
            }
            PlayPauseButton.Icon = I.ic_pause;
            Player.Play();
            UserChangingPosition = false;
        }
        void PlayPauseButtonClick(object sender, EventArgs e)
        {
            if (PlayPauseButton.Icon == I.ic_pause)
            {
                Player.Pause();
                PlayPauseButton.Icon = I.ic_play_arrow;
                Thumb.Refresh(false);
            }
            else
            {
                Player.Play();
                SwitchMode();
                PlayPauseButton.Icon = I.ic_pause;
                Thumb.Refresh(true);
            }
        }
        async void BackwardButtonClick(object sender, MouseButtonEventArgs e)
        {
            while (e.LeftButton == MouseButtonState.Pressed)
            {
                ForcePositionChange(PositionSlider.SmallChange * -1);
                await Task.Delay(100);
            }
        }
        async void ForwardButtonClick(object sender, MouseButtonEventArgs e)
        {
            while (e.LeftButton == MouseButtonState.Pressed)
            {
                ForcePositionChange(PositionSlider.SmallChange);
                await Task.Delay(100);
            }
        }
        void Player_MouseUp(object sender, MouseButtonEventArgs e) { }
        void Player_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ElementTimer.Enabled = true;
            ElementTimer.Start();
            try
            {
                if (!FullScreen) DragMove();
                if (ElementTimer.Enabled)
                {
                    SwitchMode(AudioButton.IsVisible ? WindowMode.VideoWithoutControls : WindowMode.VideoWithControls);
                }
            }
            catch (Exception)
            {
            }
        }
        void VolumeSlider_ValueChanged(object sender, Routed e)
        {
            Player.Volume = VolumeSlider.Value / 100;
            switch (Player.Volume)
            {
                case double n when (n > 0.75): AudioButton.Icon = I.ic_volume_up; break;
                case double n when (n > 0.25): AudioButton.Icon = I.ic_volume_down; break;
                default: AudioButton.Icon = I.ic_volume_off; break;
            }
        }
        #endregion
        #region WPF Controls
        async void ViewParent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (UserResizingWindow) return;
            if (ViewParent.Visibility == Visibility.Visible)
            {
                UserResizingWindow = true;
                await Task.Delay(100);
                if (UserResizingWindow) RebuildView();
                UserResizingWindow = false;
            }
        }
        void Med_PlayRequested(MediaView sender)
        {
            if (CurrentlyPlaying.Equals(sender)) PlayPauseButtonClick(this, null);
            else Play(sender);
        }
        void SearchBoxEnter(object sender, KeyEventArgs e)
        {
            /*if (e.Key == Key.Enter)
            {
                if (textBox1.Text.Length == 0) UpdateSource(Main.ToArray());
                else
                {
                    List<Media> result = new List<Media>();
                    for (int i = 0; i < Main.Count; i++) { if (Main[i].Contains(textBox1.Text)) result.Add(Main[i]); }
                    if (result.Count != 0) UpdateSource(result.ToArray());
                    else UpdateSource(Main.ToArray());
                }
            }*/
        }
        void ExpandSearch(object sender, RoutedEventArgs e) => View.Margin = new Thickness(0, 46, 0, 60);
        void CollapseSearch(object sender, RoutedEventArgs e) => View.Margin = new Thickness(0, 26, 0, 60);
        #endregion
        #region Methods
        private void ForcePositionChange(double ms, bool Seek = false)
        {
            UserChangingPosition = true;
            if (Seek) PositionSlider.Value = ms;
            else PositionSlider.Value += ms;
            UserChangingPosition = false;
        }
        private string ConvertTime(TimeSpan time)
        {
            TimeConvertion.mins = (time.TotalSeconds - (time.TotalSeconds % 60)).ToInt() / 60;
            TimeConvertion.secs = time.TotalSeconds.ToInt() % 60;
            return $"{TimeConvertion.mins}:{(TimeConvertion.secs.ToString().Length == 1 ? $"0{TimeConvertion.secs}" : TimeConvertion.secs.ToString())}";
        }
        private void Play(MediaView media)
        {
            for (int i = 0; i < MediaViews.Count; i++)
                MediaViews[i].IsPlaying = false;
            PositionSlider.Value = 0;
            CurrentlyPlaying = media;
            PlayPauseButton.Icon = I.ic_pause;
            Player.Source = new Uri(media.Media.Path);
            Player.Play();
            Title = "Player - " + CurrentlyPlaying.Media.Name;
            media.IsPlaying = true;
            SwitchMode();
        }
        private void Play(PlayPara para)
        {
            
            if (para == PlayPara.Prev && PositionSlider.Value >= 10000)
            {
                ForcePositionChange(0, true);
                return;
            }
            else
            {
                MediaView media = MediaViews[0];
                switch (CurrentPlayMode)
                {
                    case PlayMode.Single: PositionSlider.Value = 0; Player.Pause(); return;
                    case PlayMode.RepeatOne: Play(CurrentlyPlaying); break;
                    case PlayMode.Shuffle: Play(MediaViews[Shuffle.Next(0, MediaViews.Count)]); break;
                    case PlayMode.RepeatAll: Play(MediaViews[GetIndex(para, CurrentlyPlaying)]); break;
                    default: break;
                }
            }
        }
        private void SwitchMode(WindowMode mode = WindowMode.Auto)
        {
            if (mode == WindowMode.Default) RebuildView();
            if (mode == WindowMode.Auto)
            {
                switch (CurrentlyPlaying.Media.MediaType)
                {
                    case Media.Type.Music: SwitchMode(WindowMode.Default); break;
                    case Media.Type.Video: SwitchMode(WindowMode.VideoWithControls); break;
                    case Media.Type.NotMedia: break;
                    default: break;//
                }
                return;
            }
            MediaControlsBorder.Visibility = mode == WindowMode.VideoWithControls ? Visibility.Visible : Visibility.Hidden;
            VolumeButtonBorder.Visibility = MediaControlsBorder.Visibility;
            TimeLabelBorder.Visibility = MediaControlsBorder.Visibility;
            var MainControlsHidden = mode != WindowMode.Default;
            ViewParent.Visibility = MainControlsHidden ? Visibility.Hidden : Visibility.Visible;
            SearchExpander.Visibility = MainControlsHidden || !SearchEnabled ? Visibility.Hidden : Visibility.Visible;
            textBox1.Visibility = MainControlsHidden ? Visibility.Hidden : Visibility.Visible;
            Player.Visibility = MainControlsHidden ? Visibility.Visible : Visibility.Hidden;
            var VideoControlsVisibility = mode == WindowMode.VideoWithoutControls ? Visibility.Hidden : Visibility.Visible;
            PositionSlider.Visibility = VideoControlsVisibility;
            PlayPauseButton.Visibility = VideoControlsVisibility;
            PreviousButton.Visibility = VideoControlsVisibility;
            NextButton.Visibility = VideoControlsVisibility;
            TimeLabel.Visibility = VideoControlsVisibility;
            AudioButton.Visibility = VideoControlsVisibility;
            VolumeSlider.Visibility = VideoControlsVisibility;
            RewindButton.Visibility = VideoControlsVisibility;
            ForwardButton.Visibility = VideoControlsVisibility;
            FunctionButton.Visibility = VideoControlsVisibility;
            ReturnButton.Visibility = VideoControlsVisibility == Visibility.Hidden || !MainControlsHidden ? Visibility.Hidden : Visibility.Visible;
          
            if (CurrentlyPlaying.Media != null) VideoButton.Visibility = !MainControlsHidden && CurrentlyPlaying.Media.MediaType == Media.Type.Video ? Visibility.Visible : Visibility.Hidden;
            else VideoButton.Visibility = Visibility.Hidden;
            window.Cursor = (VideoControlsVisibility == Visibility.Hidden && FullScreen) ? Cursors.None : Cursors.Arrow;
            if (MainControlsHidden)
            {
                Background = Brushes.Black;
                WindowStyle = VideoControlsVisibility == Visibility.Hidden || FullScreen ? WindowStyle.None : WindowStyle.ThreeDBorderWindow;
            }
            else
            {
                Background = MainSettings.MainTheme.BackgroundBrush;
                if (!FullScreen)
                    WindowStyle = WindowStyle.ThreeDBorderWindow;
            }
            Topmost = VideoControlsVisibility == Visibility.Hidden;
        }
        private void Add(Media media)
        {
                MediaViews.Add(new MediaView(media, MainSettings, ref MediaViewRandom));
                MediaViews[MediaViews.Count - 1].ArtworkClicked += MediaArtworkClicked;
                MediaViews[MediaViews.Count - 1].PopupRequested += MainUI_PopupRequested;
            if (ActiveViewMode != ViewMode.Singular)
            {
                int index = -1;
                for (int i = 0; i < GroupViews.Count; i++)
                    if (GroupViews[i].DoesMatch(media, ActiveViewMode))
                        index = i;
                if (index == -1)
                {
                    GroupViews.Add(new GroupView(MediaViews[MediaViews.Count - 1], MainSettings.TileTheme, ActiveViewMode));
                    RebuildView(GroupViews, Styling.XAML.Size.GroupView.Self);
                }
                else
                {
                    GroupViews[index].Add(MediaViews[MediaViews.Count - 1]);
                }
            }
        }
        private void Load(string[] files, bool autoPlay = true)
        {
            var c = MediaViews.Count;
            for (int i = 0; i < files.Length; i++)
                if (Playlist.IsPlaylist(files[i], out var pL))
                {
                    var tr = pL.TracksMedia;
                    for (int j = 0; j < tr.Length; j++)
                        Add(tr[j]);
                }
            foreach (var item in Media.GetEnum(files))
                Add(item);
            RebuildView();
            if (c < MediaViews.Count && autoPlay)
                Play(MediaViews[c]);
        }
        private void MainUI_PopupRequested(object sender, MediaEventArgs e)
        {
            ActivePopup = e.Sender;
            OtherPopup.IsOpen = true;
        }
        private int GetIndex(PlayPara para, MediaView input)
        {
            int u = -1;
            for (int i = 0; i < MediaViews.Count; i++)
                if (MediaViews[i] == input) u = i;
            if (u == -1) return -1;
            switch (para)
            {
                case PlayPara.None: return u;
                case PlayPara.Next: if (u != MediaViews.Count - 1) return u + 1; else return 0;
                case PlayPara.Prev: if (u != 0) return u - 1; else return MediaViews.Count - 1;
                default: return -2;
            }

        }
        private void MainUI_PreviousTagRequested(object sender, MediaEventArgs e)
        {
            ActivePopup = MediaViews[GetIndex(PlayPara.Prev, ActivePopup)];
            MediaCcRequested(this, null);
        }
        private void MainUI_NextTagRequested(object sender, MediaEventArgs e)
        {
            ActivePopup = MediaViews[GetIndex(PlayPara.Next, ActivePopup)];
            MediaCcRequested(this, null);
        }
        private void MainUI_TagSaveRequested1(object sender, MediaEventArgs e)
        {
            if (e.Sender == CurrentlyPlaying)
            {
                var pos = PositionSlider.Value;
                Player.Stop();
                Player.Source = null;
                Thread.Sleep(10);
                bool saven = false;
                while (!saven)
                {
                    try { e.File.Save(); saven = true; }
                    catch (IOException) { Thread.Sleep(100); }
                }
                e.Sender.Media = new Media(e.Media.Path);
                e.Sender.Artwork.Source = e.Sender.Media.Artwork;
                e.Sender.UserControl_Loaded(this, null);
                Play(CurrentlyPlaying);
                ForcePositionChange(pos, true);
            }
            else
            {
                e.File.Save();
                e.Sender.Media = new Media(e.Media.Path);
                e.Sender.Artwork.Source = e.Sender.Media.Artwork;
                e.Sender.UserControl_Loaded(this, null);
            }
        }

        private void Add(string path) => Add(new Media(path));
        private void RebuildView()
        {
            if (MainSettings.ViewMode == 0)
                RebuildView(MediaViews, new Size()
                {
                    Height = Styling.XAML.Size.MediaView.Self((MediaViewMode)MainSettings.TileScheme).h,
                    Width = Styling.XAML.Size.MediaView.Self((MediaViewMode)MainSettings.TileScheme).w
                });
            else
            {
                RebuildView(GroupViews, Styling.XAML.Size.GroupView.Self);
            }
        }
        private void RebuildView<T>(List<T> Collection, Size standardSize) where T : UIElement
        {
            RebuildWidth = Convert.ToInt32(WindowState == WindowState.Maximized ? Screen.FullWidth : Width);
            RebuildHeight = Convert.ToInt32(WindowState == WindowState.Maximized ? Screen.FullHeight - 30: Height);

            if (MainSettings.Orientation == 0)
            {
                View.Children.Clear();
                CapableColumns = (RebuildWidth - 20) / (int)standardSize.Width;
                ViewColumns = Collection.Count > CapableColumns ? CapableColumns : Collection.Count;
                ViewRows = Collection.Count / CapableColumns + 1;
                View.RowDefinitions.Clear();
                View.ColumnDefinitions.Clear();
                for (int i = 0; i < ViewRows; i++)
                    View.RowDefinitions.Add(new RowDefinition());
                for (int i = 0; i < ViewColumns; i++)
                    View.ColumnDefinitions.Add(new ColumnDefinition());
                GeneralCounter = 0;
                for (int i = 0; i < ViewRows; i++)
                    for (int j = 0; j < ViewColumns; j++)
                    {
                        try
                        {
                            Grid.SetColumn(Collection[GeneralCounter], j);
                            Grid.SetRow(Collection[GeneralCounter++], i);
                        }
                        catch (Exception) { break; }
                    }
                for (int i = 0; i < Collection.Count; i++)
                    View.Children.Add(Collection[i]);
                ViewParent.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                ViewParent.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
            else
            {
                View.Children.Clear();
                CapableRows = (RebuildHeight - 70) / (int)standardSize.Height;
                ViewRows = Collection.Count > CapableRows ? CapableRows : Collection.Count;
                ViewColumns = Collection.Count / CapableRows + 1;
                View.RowDefinitions.Clear();
                View.ColumnDefinitions.Clear();
                
                for (int i = 0; i < ViewRows; i++)
                    View.RowDefinitions.Add(new RowDefinition());
                for (int i = 0; i < ViewColumns; i++)
                    View.ColumnDefinitions.Add(new ColumnDefinition());
                GeneralCounter = 0;
                for (int i = 0; i < ViewColumns; i++)
                    for (int j = 0; j < ViewRows; j++)
                    {
                        try
                        {
                            Grid.SetRow(Collection[GeneralCounter], j);
                            Grid.SetColumn(Collection[GeneralCounter++], i);
                        }
                        catch (Exception) { break; }
                    }
                for (int i = 0; i < Collection.Count; i++)
                {
                    View.Children.Add(Collection[i]);
                }
                ViewParent.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                ViewParent.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            }
        }
        private void ReloadBrush()
        {
            for (int i = 0; i < MainSegoeButtons.Count; i++) { MainSegoeButtons[i].Theme = MainSettings.MainTheme; }
            VolumeSlider.Background = MainSettings.MainTheme.BarsBrush;
            VolumeSlider.BorderBrush = MainSettings.MainTheme.ContextBrush;
            VolumeSlider.Foreground = MainSettings.MainTheme.BarsBrush;
            Background = MainSettings.MainTheme.BackgroundBrush;
            PositionSlider.Foreground = MainSettings.MainTheme.BarsBrush;
            PositionSlider.BorderBrush = MainSettings.MainTheme.ContextBrush;
            MediaControlsBorder.Background = Background;
            VolumeButtonBorder.Background = Background;
            TimeLabelBorder.Background = Background;
            FunctionCanvas.Background = Background;
            if (Player.IsVisible)
                Background = Brushes.Black;
            for (int i = 0; i < OthersCanvas.Children.Count; i++)
            {
                if (OthersCanvas.Children[i] is MaterialButton j)
                    j.Theme = MainSettings.TileTheme;
            }
            TimeLabel.Foreground = MainSettings.MainTheme.ContextBrush;
           // PopupBorder.Background = MainSettings.TileTheme.BackgroundBrush;
           // PopupBorder.BorderBrush = MainSettings.TileTheme.BarsBrush;
        }
        #endregion
        #region Context Menus
        private void PlayerCFullScreen(object sender, RoutedEventArgs e)
        {
            if (Height != Screen.FullHeight)
            {
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                FullScreen = true;
                LastWidth = Width;
                LastHeight = Height;
                LastLeft = Left;
                LastTop = Top;
                Width = Screen.FullWidth;
                Height = Screen.FullHeight;
                Left = 0;
                Top = 0;
                _State = WindowState;
                WindowState = WindowState.Normal;
            }
            else
            {
                FullScreen = false;
                ResizeMode = ResizeMode.CanResize;
                WindowState = _State;
                WindowStyle = WindowStyle.ThreeDBorderWindow;
                Width = LastWidth;
                Height = LastHeight;
                Left = LastLeft;
                Top = LastTop;
            }
            PlayerC_FullScreen.Header = FullScreen ? "Exit Full Screen" : "Enter Full Screen";
        }
        private void PlayerCPlaybackSettings(object sender, RoutedEventArgs e)
        {
            SettingsUI.TabController.SelectedIndex = 4;
            SettingsUI.ShowDialog();
        }
        private void ContextSwitchView(object sender, EventArgs e)
        {

        }
        private void ShareFunction(object sender, RoutedEventArgs e)
        {
            FunctionPopup.IsOpen = false;
            var (ok, path) = Dialogs.SaveFile("Player Playlist | *.elp", "Playlist", "elp", "Share Playlist");
            if (ok.Value)
                Playlist.SavePlaylist((from item in MediaViews select item.Media).ToArray(), path);
        }
        private void AboutFunction(object sender, RoutedEventArgs e)
        {
            AboutPopup.IsOpen = !AboutPopup.IsOpen;
        }
        private void BrowseFunction(object sender, RoutedEventArgs e)
        {
            FunctionPopup.IsOpen = false;
            var (ok, folder) = Dialogs.RequestDirectory();
            if (ok) Load(Directory.GetFiles(folder, "*", SearchOption.AllDirectories));
        }
        private void PlayerCSubtitle(object sender, RoutedEventArgs e)
        {
            var (ok, files) = Dialogs.RequestFiles("Subtitles|*.srt|Text|*.txt", false);
            if (ok)
            {
                Debug.Display("ram papapam papapa pam papapam papapa", "dorodorodo");
            }
        }
        private void OpenFunction(object sender, RoutedEventArgs e)
        {
            FunctionPopup.IsOpen = false;
            var (ok, files) = Dialogs.RequestFiles();
            if (ok) Load(files);
        }
        private void SettingsFunction(object sender, RoutedEventArgs e)
        {
            SettingsUI.Show();
            FunctionPopup.IsOpen = false;
        }
        #endregion
        #region Views Controller
        private void MediaLyricsRequested(object sender, RoutedEventArgs e)
        {
            UniversalLyricsView.Load(ActivePopup.Media);
            OtherPopup.IsOpen = false;
        }
        private void MediaRemoveRequested(object sender, RoutedEventArgs e)
        {
            if (ActiveViewMode == ViewMode.Singular)
                MediaViews.Remove(ActivePopup);
            else
            {
                for (int i = 0; i < GroupViews.Count; i++)
                {
                    if (GroupViews[i].DoesHave(ActivePopup))
                    {
                        GroupViews[i].Remove(ActivePopup);
                        if (GroupViews[i].Items.Count == 0)
                            GroupViews.RemoveAt(i);
                        break;
                    }
                }
            }
            RebuildView();
        }
        private void MediaDeleteRequested(object sender, RoutedEventArgs e)
        {
            var res = W.MessageBox.Show($"Sure? this file will be deleted:\r\n{ActivePopup.Media.Path}", " ", W.MessageBoxButtons.OKCancel, W.MessageBoxIcon.Warning);
            if (res != W.DialogResult.OK) return;
            try { File.Delete(ActivePopup.Media.Path); } catch (Exception f) { Debug.Display(f.Message, "Error"); return; }
            MediaRemoveRequested(sender, e); 
        }
        private void MediaMoveRequested(object sender, RoutedEventArgs e)
        {

            var (ok, path) = Dialogs.SaveFile(ActivePopup.Media.Filter(), ActivePopup.Media.Name, ActivePopup.Media.GetExt(), "Move To...");
            if (ok.Value)
            {
                try { File.Move(ActivePopup.Media.Path, path); } catch (Exception t) { Debug.Display(t.Message, "Error"); return; }
                ActivePopup.Media.Path = path;
            }
        }
        private void MediaCopyRequested(object sender, RoutedEventArgs e)
        {

            var (ok, path) = Dialogs.SaveFile(ActivePopup.Media.Filter(), ActivePopup.Media.Name, ActivePopup.Media.GetExt(), "Copy To...");
            if (ok.Value)
            {
                try { File.Copy(ActivePopup.Media.Path, path); } catch (Exception t) { Debug.Display(t.Message, "Error"); return; }
            }
        }
        private void MediaLocationRequested(object sender, RoutedEventArgs e) => Process.Start("explorer.exe", "/select," + ActivePopup.Media.Path);
        private void MediaCcRequested(object sender, RoutedEventArgs e)
        {
            OtherPopup.IsOpen = false;
            UniversalMetaEditor.Load(ref ActivePopup);
        }
        #endregion

        private void SwitchClicked(object sender, RoutedEventArgs e)
        {

        }
    }
}
