using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Media;

namespace Player.Controls.Ribbon
{
	public partial class MenuButton : System.Windows.Controls.Ribbon.RibbonMenuButton
	{
		public static readonly DependencyProperty IconProperty =
			DependencyProperty.Register(nameof(Icon), typeof(IconKind), typeof(MenuButton), new PropertyMetadata(IconKind.AccessPoint, new PropertyChangedCallback(OnIconChange)));

		public IconKind IconKind
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
		public MenuButton()
		{
			InitializeComponent();
		}
	}
}
