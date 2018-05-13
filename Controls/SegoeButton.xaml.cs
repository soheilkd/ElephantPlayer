using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System;
using System.Windows.Controls;
namespace Player.Controls
{
    public partial class SegoeButton : UserControl
    {
        public static readonly DependencyProperty EllipseProperty =
            DependencyProperty.Register(nameof(EllipseType), typeof(EllipseTypes), typeof(SegoeButton), new PropertyMetadata(EllipseTypes.Circular));
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Glyph), typeof(Glyph), typeof(SegoeButton), new PropertyMetadata(Glyph.GlobalNavigationButton, new PropertyChangedCallback(OnGlyphChange)));
        public enum EllipseTypes { Rectular, Circular }

        public EllipseTypes EllipseType
        {
            get => (EllipseTypes)GetValue(EllipseProperty);
            set
            {
                SetValue(EllipseProperty, value);
                MainEllipse.CornerRadius = new CornerRadius(value == 0 ? 10 : 20);
            }
        }
        public Glyph Glyph
        {
            get => (Glyph)GetValue(IconProperty);
            set
            {
                SetValue(IconProperty, value);
                MainIcon.Icon = value;
            }
        }

        public SegoeButton() => InitializeComponent();

        void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            MainEllipse.CornerRadius = new CornerRadius(EllipseType == 0 ? 2 : 20);
            MainIcon.Icon = Icon;
        }
        private static void OnGlyphChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }
    }
}