using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Player.Controls
{
	public partial class Button : UserControl
	{
		public Button() => InitializeComponent();

		public static readonly MouseButtonEventArgs DefaultMouseUpArgs =
			new MouseButtonEventArgs(Mouse.PrimaryDevice, 1, MouseButton.Left) { RoutedEvent = MouseUpEvent };

		public static readonly DependencyProperty IconProperty =
			DependencyProperty.Register(nameof(Icon), typeof(IconType), typeof(Button), new PropertyMetadata(IconType.Wifi));

		public IconType Icon
		{
			get => (IconType)GetValue(IconProperty);
			set
			{
				SetValue(IconProperty, value);
				MainIcon.Type = value;
			}
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e) => Icon = Icon;

		public void EmulateClick() => RaiseEvent(DefaultMouseUpArgs);
	}
}