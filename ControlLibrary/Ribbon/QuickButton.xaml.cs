using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;

namespace Player.Controls
{
	public partial class QuickButton : Button
	{
		public static readonly DependencyProperty IconProperty =
			   DependencyProperty.Register(nameof(Icon), typeof(IconKind), typeof(QuickButton), new PropertyMetadata(IconKind.Sale, new PropertyChangedCallback(OnIconChange)));

		public IconKind Icon
		{
			get => (IconKind)GetValue(IconProperty);
			set
			{
				SetValue(IconProperty, value);
				MainIcon.Kind = (PackIconKind)(int)value;
			}
		}
		public QuickButton() => InitializeComponent();

		private void Button_Loaded(object sender, RoutedEventArgs e) => Icon = Icon;

		private static void OnIconChange(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
			d.SetValue(IconProperty, d.GetValue(IconProperty));

	}
}
