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
			DependencyProperty.Register(nameof(Icon), typeof(IconKind), typeof(MaterialButton), new PropertyMetadata(IconKind.Sale, new PropertyChangedCallback(OnIconChange)));
		public static readonly DependencyProperty EllipseMarginProperty =
			DependencyProperty.Register(nameof(EllipseMargin), typeof(Thickness), typeof(MaterialButton), new PropertyMetadata(new Thickness(-7), new PropertyChangedCallback(OnEllipseMarginChange)));

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
		public IconKind Icon
		{
			get => (IconKind)GetValue(IconProperty);
			set
			{
				SetValue(IconProperty, value);
				MainIcon.Kind = (PackIconKind)(int)value;
			}
		}
		public Thickness EllipseMargin
		{
			get => (Thickness)GetValue(EllipseMarginProperty);
			set
			{
				SetValue(EllipseMarginProperty, value);
				MainEllipse.Margin = value;

			}
		}

		public MaterialButton() => InitializeComponent();

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			EllipseType = EllipseType;
			Icon = Icon;
		}
		private static void OnIconChange(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
			d.SetValue(IconProperty, d.GetValue(IconProperty));

		private static void OnEllipseMarginChange(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
			d.SetValue(EllipseMarginProperty, d.GetValue(EllipseMarginProperty));

		public void EmulateClick() => RaiseEvent(DefaultMouseUpArgs);
	}
}