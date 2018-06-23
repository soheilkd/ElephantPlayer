using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
namespace Player.Controls
{
	public partial class MaterialButton : UserControl
	{
		public static readonly MouseButtonEventArgs DefaultMouseUpArgs =
			new MouseButtonEventArgs(Mouse.PrimaryDevice, 1, MouseButton.Left) { RoutedEvent = MouseUpEvent };
		public static readonly DependencyProperty EllipseProperty =
			DependencyProperty.Register(nameof(EllipseType), typeof(EllipseTypes), typeof(MaterialButton), new PropertyMetadata(EllipseTypes.Circular));
		public static readonly DependencyProperty IconProperty =
			DependencyProperty.Register(nameof(Icon), typeof(PackIconKind), typeof(MaterialButton), new PropertyMetadata(PackIconKind.Settings, new PropertyChangedCallback(OnIconChange)));

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
		public PackIconKind Icon
		{
			get => (PackIconKind)GetValue(IconProperty);
			set
			{
				SetValue(IconProperty, value);
				MainIcon.Kind = value;
			}
		}

		public MaterialButton() => InitializeComponent();

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			EllipseType = EllipseType;
			Icon = Icon;
		}
		private static void OnIconChange(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
			d.SetValue(IconProperty, (PackIconKind)d.GetValue(IconProperty));

		public void EmulateClick() => RaiseEvent(DefaultMouseUpArgs);
	}
}