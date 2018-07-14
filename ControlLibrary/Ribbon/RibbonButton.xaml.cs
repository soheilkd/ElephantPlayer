using System.Windows;
using System.Windows.Media;

namespace Player.Controls.Ribbon
{
	public partial class Button : System.Windows.Controls.Ribbon.RibbonButton
	{
		public Button() => InitializeComponent();

		public static readonly DependencyProperty IconProperty =
			DependencyProperty.Register(nameof(Icon), typeof(IconKind), typeof(Button), new PropertyMetadata(IconKind.AccessPoint, new PropertyChangedCallback(OnIconChange)));

		public IconKind Icon
		{
			get => (IconKind)GetValue(IconProperty);
			set
			{
				SetValue(IconProperty, value);
				LargeImageSource = value.GetBitmap(Brushes.Black);
			}
		}

		private static void OnIconChange(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
			d.SetValue(IconProperty, d.GetValue(IconProperty));
		
		private void RibbonButton_Loaded(object sender, RoutedEventArgs e) => Icon = Icon;

	}
}