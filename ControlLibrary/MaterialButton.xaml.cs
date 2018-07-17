using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Player.Controls
{
	public partial class MaterialButton : UserControl
	{
		public MaterialButton() => InitializeComponent();

		public static readonly MouseButtonEventArgs DefaultMouseUpArgs =
			new MouseButtonEventArgs(Mouse.PrimaryDevice, 1, MouseButton.Left) { RoutedEvent = MouseUpEvent };
		public static readonly SolidColorBrush DefaultEllipseBackground =
			new SolidColorBrush(new Color() { R = 0, G = 0, B = 0, A = 10 });

		public static readonly DependencyProperty EllipseProperty =
			DependencyProperty.Register(nameof(EllipseType), typeof(EllipseType), typeof(MaterialButton), new PropertyMetadata(EllipseType.Circular));
		public static readonly DependencyProperty IconProperty =
			DependencyProperty.Register(nameof(Icon), typeof(IconKind), typeof(MaterialButton), new PropertyMetadata(IconKind.AccessPoint));
		public static readonly DependencyProperty EllipseBackgroundProperty =
			DependencyProperty.Register(nameof(EllipseBackground), typeof(Brush), typeof(MaterialButton), new PropertyMetadata(DefaultEllipseBackground));

		public EllipseType EllipseType
		{
			get => (EllipseType)GetValue(EllipseProperty);
			set
			{
				SetValue(EllipseProperty, value);
				MainEllipse.CornerRadius = new CornerRadius(value == 0 ? 2 : 20);
				ClickEllipse.CornerRadius = new CornerRadius(value == 0 ? 2 : 100);
			}
		}
		public IconKind Icon
		{
			get => (IconKind)GetValue(IconProperty);
			set
			{
				SetValue(IconProperty, value);
				MainIcon.Kind = (PackIconKind)(int)value;
			}
		}
		public Brush EllipseBackground
		{
			get => (Brush)GetValue(EllipseBackgroundProperty);
			set
			{
				SetValue(EllipseBackgroundProperty, value);
				MainEllipse.Background = value;
			}
		}
		
		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			EllipseType = EllipseType;
			Icon = Icon;
			EllipseBackground = EllipseBackground;
		}

		public void EmulateClick() => RaiseEvent(DefaultMouseUpArgs);
	}
}