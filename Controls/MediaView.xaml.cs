using Player.Events;
using Player.Enums;
using Player.Getters;
using Player.Types;
using Player.User;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using M = Player.Styling.XAML.Margin;
using S = Player.Styling.XAML.Size;
using Player.Styling;
using System.Windows;
#pragma warning disable 1591
namespace Player
{
    public partial class MediaView : UserControl
    {
        public void Change(SettingsEventArgs e)
        {
            ViewMode = (MediaViewMode)e.NewSettings.TileScheme;
            ActiveTheme = e.NewSettings.TileTheme;
            FontSize = e.NewSettings.TileFontSize;
            FontFamily = e.NewSettings.TileFont;
            ProgressBar1.Visibility = e.NewSettings.TileProgress ? Visibility.Visible : Visibility.Hidden;
            SomethingChanged?.Invoke(this, new MediaEventArgs() { Sender = this });
        }
        public Preferences Settings;
        public event EventHandler<MediaEventArgs> PopupRequested;
        public event EventHandler<MediaEventArgs> MoveRequested;
        public event EventHandler<MediaEventArgs> ArtworkClicked;
        public event EventHandler<MediaEventArgs> SomethingChanged;
        public new bool Equals(object obj)
        {
            try { return (obj as MediaView).Media.Path.Equals(Media.Path); } catch (NullReferenceException) { return false; }
        }
        public int GetIndex(MediaView[] array)
        {
            for (int i = 0; i < array.Length; i++) { if (Equals(array[i])) return i; }
            return -1;
        }
        private MediaViewMode viewMode;
        public MediaViewMode ViewMode { get => viewMode; set
            {
                viewMode = value;
               
                Artwork.Margin = M.MediaView.Artwork(value);
                TitleLabel.Margin = M.MediaView.TitleLabel(value);
                Height = S.MediaView.Self(value).h;
                Width = S.MediaView.Self(value).w;
                Artwork.Height = S.MediaView.Artwork(value).h;
                Artwork.Width = S.MediaView.Artwork(value).w;
                TitleLabel.Width = S.MediaView.TitleLabel(value);
            } }
        private bool isPlaying = false;
        public bool IsPlaying { get => isPlaying; set
            {
                isPlaying = value;
                Progress = 0;
                ProgressBar1.Visibility = value ? Visibility.Visible : Visibility.Hidden;
                SubBorder.BorderBrush = value ? ActiveTheme.BarsBrush : ActiveTheme.ContextBrush;
                SubBorder.BorderThickness = value ? new Thickness(2) : new Thickness(1);
                Artwork.Margin = M.MediaView.Artwork(ViewMode, value);
                TitleLabel.FontWeight = value ? FontWeights.Bold : FontWeights.Normal;
                SomethingChanged?.Invoke(this, new MediaEventArgs() { Sender = this });
            } }
        public Theme ActiveTheme
        {
            get => activeTheme;
            set
            {
                TitleLabel.Foreground = value.ContextBrush;
                MainBorder.Background = value.BackgroundBrush;
                ProgressBar1.Foreground = value.BarsBrush;
                //MoveButton.Theme = value;
                activeTheme = value;
                SubBorder.BorderBrush = IsPlaying ? value.BarsBrush : value.ContextBrush;
                //SubBorder.BorderBrush = value.BarsBrush;
            }
        }
        public Theme activeTheme = Theme.Get();
        public Media Media;
        public double Progress { get => ProgressBar1.Value; set => ProgressBar1.Value = value; }
        public double Max { get => ProgressBar1.Maximum; set => ProgressBar1.Maximum = value; }
        public MediaView()
        {
            InitializeComponent();
            Artwork.MouseUp += Artwork_MouseUp;
            //MoveButton.MouseUp += (sender, e) => MoveRequested?.Invoke(this);
        }
        int ClickCount = 0;
        private async void Artwork_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ClickCount++;
                if (ClickCount == 1)
                    ClickTimer.Enabled = true;
                if (ClickCount == 2)
                {
                    //ClickTimer.Stop();
                    ArtworkClicked?.Invoke(this, new MediaEventArgs() { Sender = this });
                    SomethingChanged.Invoke(this, new MediaEventArgs() { Sender = this });
                }
            }
            if (e.RightButton == MouseButtonState.Pressed)
            {
                OtherPopup.IsOpen = true;
                OtherPopup.StaysOpen = true;
                await Task.Delay(100);
                OtherPopup.StaysOpen = false;
            }
        }

        Random rand;
        public MediaView(Media media, Preferences pref, ref Random rand)
        {
            viewMode = (MediaViewMode)pref.TileScheme;
            InitializeComponent();
            this.rand = rand;
            Artwork.MouseDown += Artwork_MouseUp;
            TitleLabel.MouseDown += Artwork_MouseUp;
            //MoveButton.MouseUp += (sender, e) => MoveRequested?.Invoke(this);
            Media = media;
            TitleLabel.Content = media.Name;
            ActiveTheme = pref.TileTheme;
            ProgressBar1.Maximum = media.Length != 0 ? media.Length : 1;
            ProgressBar1.Visibility = pref.TileProgress ? Visibility.Visible : Visibility.Hidden;
            IsPlaying = false;
            FontFamily = pref.TileFont;
            ViewMode = ViewMode;
            ClickTimer.Tick += Timer1_Tick;
            Settings = pref;
        }
        System.Windows.Forms.Timer ClickTimer = new System.Windows.Forms.Timer() { Enabled = true, Interval = 600 };
        private void Timer1_Tick(object sender, System.EventArgs e)
        {
            ClickCount = 0;
            ClickTimer.Enabled = false;
        }
        public async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (Media != null) Artwork.Source = Media.Artwork;
            ProgressBar1.Visibility = Visibility.Hidden;
            Artwork.Visibility = Visibility.Hidden;
            ProgressBar ProgressBar2 = new ProgressBar()
            {
                IsIndeterminate = true,
                Margin = Artwork.Margin,
                Height = Artwork.Height,
                Width = Artwork.Width,
                Background = Theming.ToBrush(Colors.Transparent),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Foreground = ActiveTheme.ContextBrush
            };
            Grid.Children.Add(ProgressBar2);
            if (ViewMode != MediaViewMode.Compact)
            {
                var opa = new DoubleAnimation()
                {
                    From = 0,
                    To = Settings.Orientation == 0 ? S.MediaView.Self(ViewMode).h : S.MediaView.Self(viewMode).w,
                    Duration = TimeSpan.FromMilliseconds(1000),
                    By = 1,
                    AccelerationRatio = 1
                };
                Storyboard.SetTarget(opa, OutputCanvas);
                Storyboard.SetTargetProperty(opa, new PropertyPath(Settings.Orientation == 0? HeightProperty: WidthProperty));
                var sb2 = new Storyboard() { AutoReverse = false };
                sb2.Children.Add(opa);
                sb2.Begin();
                await Task.Delay(1000);
                sb2.Stop();
                sb2 = null;
                
                if (rand != null) await Task.Delay(rand.Next(500, 2000));
                var fade = new DoubleAnimation()
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(1000)
                };

                Storyboard.SetTarget(fade, Artwork);
                Storyboard.SetTargetProperty(fade, new PropertyPath(OpacityProperty));

                var sb1 = new Storyboard() { AutoReverse = false };
                sb1.Children.Add(fade);
                Artwork.Visibility = Visibility.Visible;
                sb1.Begin();
                Grid.Children.RemoveAt(Grid.Children.Count - 1);
                await Task.Delay(1000);
                sb1.Stop();
                ProgressBar2 = null;
                sb1 = null;
                fade = null;
            }
            ProgressBar1.Visibility = IsPlaying ? Visibility.Visible : Visibility.Hidden;
            ActiveTheme = activeTheme;
        }

        private void OtherPopup_Opened(object sender, EventArgs e)
        {
            OtherPopup.IsOpen = false; 
            PopupRequested?.Invoke(this, new MediaEventArgs() { Sender = this });
        }
    }
}
