using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
namespace Player.Controls
{
    public partial class SegoeButton : UserControl
    {
        public static readonly MouseButtonEventArgs DefaultMouseUpArgs =
            new MouseButtonEventArgs(Mouse.PrimaryDevice, 1, MouseButton.Left) { RoutedEvent = MouseUpEvent };
        public static readonly DependencyProperty EllipseProperty =
            DependencyProperty.Register(nameof(EllipseType), typeof(EllipseTypes), typeof(SegoeButton), new PropertyMetadata(EllipseTypes.Circular));
        public static readonly DependencyProperty GlyphProperty =
            DependencyProperty.Register(nameof(Glyph), typeof(Glyph), typeof(SegoeButton), new PropertyMetadata(Glyph.Settings, new PropertyChangedCallback(OnGlyphChange)));
        
        public enum EllipseTypes { Rectular, Circular }

        public EllipseTypes EllipseType
        {
            get => (EllipseTypes)GetValue(EllipseProperty);
            set
            {
                SetValue(EllipseProperty, value);
                MainEllipse.CornerRadius = new CornerRadius(value == 0 ? 2 : 20);
                ClickEllipse.CornerRadius = new CornerRadius(value == 0 ? 2 : 100);
            }
        }
        public Glyph Glyph
        {
            get => (Glyph)GetValue(GlyphProperty);
            set
            {
                SetValue(GlyphProperty, value);
                MainIcon.Glyph = value;
            }
        }

        public SegoeButton() => InitializeComponent();

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            EllipseType = EllipseType;
            Glyph = Glyph;
        }
        private static void OnGlyphChange(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            d.SetValue(GlyphProperty, (Glyph)d.GetValue(GlyphProperty));

        public void EmulateClick() => RaiseEvent(DefaultMouseUpArgs);
    }
}