using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Player.Controls
{
	public partial class Icon : Viewbox
	{
		public static readonly DependencyProperty TypeProperty =
			DependencyProperty.Register(nameof(Type), typeof(IconType), typeof(Icon), new PropertyMetadata(IconType.Wifi));
		public static readonly DependencyProperty ForegroundProperty =
			DependencyProperty.Register(nameof(Foreground), typeof(Brush), typeof(Icon), new PropertyMetadata(Brushes.White));

		public IconType Type
		{
			get => (IconType)GetValue(TypeProperty);
			set
			{
				SetValue(TypeProperty, value);
				textBlock.Text = Properties.Resources.ResourceManager.GetString(value.ToString());
			}
		}

		public Brush Foreground
		{
			get => (Brush)GetValue(ForegroundProperty);
			set
			{
				SetValue(ForegroundProperty, value);
				textBlock.Foreground = value;
			}
		}

		public Icon() => InitializeComponent();

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			Type = Type;
			Foreground = Foreground;
		}
	}
}
