using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Player.Controls
{
    public enum Glyph
    {
        None,
        Music,
        Video,
        OndemandVideo,
        Pause,
        Play,
        Warning,
        FullScreen,
        Settings,
        Text,
        Folder,
        Next,
        Previous,
        Copy,
        DownArrow,
        UpArrow,
        HorizMore,
        Equalizer,
        ExpandLess,
        ExpandMore,
        RepeatAll,
        RepeatOne,
        Shuffle,
        Volume0,
        Volume1,
        Volume2,
        Volume3,
        MusicQueue,
        Add,
        Cancel,
        Cloud,
        ExitFullScreen,
        Search
    }
    public partial class SegoeIcon : UserControl
    {
        public static readonly GeometryCollection Icons = Application.Current.Resources["Icons"] as GeometryCollection;
        public static readonly DependencyProperty GlyphProperty =
            DependencyProperty.Register(nameof(Glyph), typeof(Glyph), typeof(SegoeIcon), new PropertyMetadata(Glyph.Shuffle, new PropertyChangedCallback(OnGlyphChange)));
        public static readonly DependencyProperty GlyphDataProperty =
            DependencyProperty.Register(nameof(GlyphData), typeof(Geometry), typeof(SegoeIcon));
        public Glyph Glyph { get => (Glyph)GetValue(GlyphProperty); set => SetValue(GlyphProperty, value); }
        public Geometry GlyphData { get => GetValue(GlyphDataProperty) as Geometry; set => SetValue(GlyphDataProperty, value); }
        public SegoeIcon() => InitializeComponent();
        private static void OnGlyphChange(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            d.SetValue(GlyphDataProperty, Icons[(int)e.NewValue]);
    }
}
