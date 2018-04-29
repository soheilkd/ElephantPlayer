using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System;
using System.Windows.Controls;
#pragma warning disable 1591
namespace Player.Controls
{
    public partial class MaterialButton : UserControl
    {
        public enum EllipseTypes { Rectular, Circular }
        public event EventHandler Click;

        public EllipseTypes EllipseType
        {
            get => (EllipseTypes)GetValue(EllipseProperty);
            set
            {
                SetValue(EllipseProperty, value);
                MainEllipse.CornerRadius = new CornerRadius(value == 0 ? 10 : 20);
            }
        }
        public IconType Icon
        {
            get => (IconType)GetValue(IconProperty);
            set
            {
                SetValue(IconProperty, value);
                MainIcon.Icon = value;
            }
        }

        public static readonly DependencyProperty EllipseProperty =
            DependencyProperty.Register(nameof(EllipseType), typeof(EllipseTypes), typeof(MaterialButton), new PropertyMetadata(EllipseTypes.Circular));
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(IconType), typeof(MaterialButton), new PropertyMetadata(IconType.UpArrow));
        
        public MaterialButton()
        {
            InitializeComponent();
        }
        public void UXMouseUp(object sender, MouseButtonEventArgs e)
        {
            Click?.Invoke(this, null);
            //MainEllipse.Background = System.Windows.Media.Brushes.Transparent;
        }
        void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            MainEllipse.CornerRadius = new CornerRadius(EllipseType == 0 ? 2 : 20);
            MainIcon.Icon = Icon;
        }
        
    }
}