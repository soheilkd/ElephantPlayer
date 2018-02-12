using Player.Enums;
using Player.Styling;
using Player.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Player.Controls
{
    /// <summary>
    /// Interaction logic for GroupMediaView.xaml
    /// </summary>
    public partial class GroupView : UserControl
    {
        const int ExpandDuration = 500;
        bool isOpen = false;
        public ImageSource TileImageSource { get => TileImage.Source; set => TileImage.Source = value; }
        private List<MediaView> MediaViews = new List<MediaView>();
        private Theme _ActiveTheme;
        public List<MediaView> Items => MediaViews;
        static MediaViewMode mediaViewMode = MediaViewMode.Default;
        private DoubleAnimation ExpandAnimation = new DoubleAnimation()
        {
            From = 172,
            To = 193 + Styling.XAML.Size.MediaView.Self(mediaViewMode).h,
            AccelerationRatio = 0.75,
            AutoReverse = false,
            Duration = TimeSpan.FromMilliseconds(ExpandDuration)
        };
        Storyboard ExpandStory = new Storyboard() { AutoReverse = true };
        public Theme ActiveTheme
        {
            get => _ActiveTheme;
            set {
                _ActiveTheme = value;
                ParentalBorder.BorderBrush = value.BarsBrush;
                ModernalGrid.Background = value.BackgroundBrush;
                ScrollViewer.Background = value.BackgroundBrush;
                TitleLabel.Foreground = value.ContextBrush;
                MetadataLabel.Foreground = value.ContextBrush;
            }
        }
        public GroupView()
        {
            InitializeComponent();
        }
        public GroupView(MediaView mediaView, Theme theme = null, ViewMode mode = ViewMode.GroupByArtist)
            : this(new MediaView[] { mediaView }, mediaView.Media.Artwork, mode, theme){ }
        public GroupView(MediaView[] mediaViews, ImageSource image = null, ViewMode viewMode = ViewMode.GroupByArtist, Theme theme = null)
        {
            InitializeComponent();
            for (int i = 0; i < mediaViews.Length; i++)
                MediaViews.Add(mediaViews[i]);
            TileImage.Source = image ?? Getters.Image.ToBitmapSource(Properties.Resources.Music);
            switch (viewMode)
            {
                case ViewMode.GroupByArtist: TitleLabel.Content = MediaViews[0].Media.Artist; break;
                case ViewMode.GroupByDir: TitleLabel.Content = MediaViews[0].Media.Path.Substring(MediaViews[0].Media.Path.LastIndexOf("\\")); break;
                case ViewMode.GroupByAlbum: TitleLabel.Content = MediaViews[0].Media.Album; break;
                    
                default: break;
            }
            MatchString = TitleLabel.Content != null ? TitleLabel.Content.ToString() : "Unknown";
            ActiveTheme = theme;
            Rebuild();
            for (int i = 0; i < MediaViews.Count; i++)
            {
                MediaViews[i].SomethingChanged += GroupView_SomethingChanged;
            }
            RefreshMeta();
        }
       
        private void GroupView_SomethingChanged(object sender, Events.MediaEventArgs e)
        {
            RefreshMeta();
            mediaViewMode = e.Sender.ViewMode;
            ExpandAnimation.To = 193 + Styling.XAML.Size.MediaView.Self(mediaViewMode).h;
        }

        public void Add(MediaView mediaView)
        {
            MediaViews.Add(mediaView);
            MediaViews[MediaViews.Count - 1].SomethingChanged += GroupView_SomethingChanged;
            Rebuild();
            RefreshMeta();
        }
        public void Remove(MediaView mediaView)
        {
            MediaViews.Remove(mediaView);
            Rebuild();
        }
        public string MatchString = "";
        public bool DoesMatch(Media media, ViewMode viewMode)
        {
            switch (viewMode)
            {
                case ViewMode.GroupByArtist: return media.Artist == MatchString;
                case ViewMode.GroupByDir: return media.Path.Substring(media.Path.LastIndexOf("\\")) == MatchString;
                case ViewMode.GroupByAlbum: return media.Album == MatchString;
                default: return false;
            }
        }
        private void Rebuild()
        {
            ViewerChild.ColumnDefinitions.Clear();
            ViewerChild.Children.Clear();
            for (int i = 0; i < MediaViews.Count; i++)
            {
                ViewerChild.ColumnDefinitions.Add(new ColumnDefinition());
                ViewerChild.Children.Add(MediaViews[i]);
                Grid.SetColumn(ViewerChild.Children[i], i);
                Grid.SetRow(ViewerChild.Children[i], 0);
            }
        }
        public bool DoesHave(MediaView mediaView)
        {
            for (int i = 0; i < MediaViews.Count; i++)
                if (MediaViews[i] == mediaView) return true;
            return false;
        }
        public void RefreshMeta()
        {
            int index = -1;
            for (int i = 0; i < MediaViews.Count; i++)
                if (MediaViews[i].IsPlaying) index = i;
            string pkat = index == -1 ? "None playing here" : $"Currently Playing: \r\n{MediaViews[index].Media.Name}";

            MetadataLabel.Content =
                $"Totally {MediaViews.Count} medias\r\n{pkat}";
        }
        private void MaterialButton_Click(object sender, EventArgs e)
        {
            if (isOpen)
            {
                OpenButton.Icon = MaterialIcons.MaterialIconType.ic_expand_more;
                ExpandStory.AutoReverse = true;
                ExpandStory.Begin();
                ExpandStory.Seek(TimeSpan.FromMilliseconds(ExpandDuration));
            }
            else
            {
                OpenButton.Icon = MaterialIcons.MaterialIconType.ic_expand_less;
                ExpandStory.Seek(TimeSpan.FromMilliseconds(0));
                ExpandStory.AutoReverse = false;
                ExpandStory.Begin();
            }
            isOpen = !isOpen;
            ScrollViewer.Visibility = isOpen ? Visibility.Visible : Visibility.Hidden;
        }

        private async void ParentalControl_Loaded(object sender, RoutedEventArgs e)
        {
            Storyboard.SetTarget(ExpandAnimation, this);
            Storyboard.SetTargetProperty(ExpandAnimation, new PropertyPath(HeightProperty));
            ExpandStory.Children.Add(ExpandAnimation);
        }
    }
}
