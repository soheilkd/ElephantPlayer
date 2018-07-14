using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Player.Controls
{
	public partial class MaterialButton : UserControl
	{
		public MaterialButton() => InitializeComponent();

		public static readonly MouseButtonEventArgs DefaultMouseUpArgs =
			new MouseButtonEventArgs(Mouse.PrimaryDevice, 1, MouseButton.Left) { RoutedEvent = MouseUpEvent };

		public static readonly DependencyProperty EllipseProperty =
			DependencyProperty.Register(nameof(EllipseType), typeof(EllipseType), typeof(MaterialButton), new PropertyMetadata(EllipseType.Circular));
		public static readonly DependencyProperty IconProperty =
			DependencyProperty.Register(nameof(Icon), typeof(IconKind), typeof(MaterialButton), new PropertyMetadata(IconKind.Sale, new PropertyChangedCallback(OnIconChange)));

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
		
		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			EllipseType = EllipseType;
			Icon = Icon;
		}

		private static void OnIconChange(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
			d.SetValue(IconProperty, d.GetValue(IconProperty));

		public void EmulateClick() => RaiseEvent(DefaultMouseUpArgs);
	}
}