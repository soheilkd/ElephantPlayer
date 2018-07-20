using System.Windows;
using System.Windows.Controls.Ribbon;
using System.Windows.Media;

namespace Player.Controls.Ribbon
{
	public partial class RibbonApplicationMenuButton : RibbonApplicationMenuItem
	{
		public static readonly DependencyProperty IconKindProperty =
			DependencyProperty.Register(nameof(Icon), typeof(IconKind), typeof(RibbonApplicationMenuButton), new PropertyMetadata(IconKind.AccessPoint));
		public static readonly DependencyProperty IconForegroundProperty =
			DependencyProperty.Register(nameof(IconForeground), typeof(Brush), typeof(RibbonApplicationMenuButton), new PropertyMetadata(Brushes.Black));

		public RibbonApplicationMenuButton() => InitializeComponent();

		public IconKind IconKind
		{
			get => (IconKind)GetValue(IconKindProperty);
			set => SetValue(IconKindProperty, value);
		}
		public Brush IconForeground
		{
			get => (Brush)GetValue(IconForegroundProperty);
			set => SetValue(IconForegroundProperty, value);
		}

		private void Item_Loaded(object sender, RoutedEventArgs e) => ImageSource = IconKind.GetBitmap(IconForeground);
	}
}
