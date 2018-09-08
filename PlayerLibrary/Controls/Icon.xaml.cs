using System.Windows;
using System.Windows.Controls;

namespace Player.Controls
{
	public partial class Icon : ContentControl
	{
		public static readonly DependencyProperty TypeProperty =
			DependencyProperty.Register(nameof(Type), typeof(IconType), typeof(Icon), new PropertyMetadata(IconType.Wifi));

		public IconType Type
		{
			get => (IconType)GetValue(TypeProperty);
			set
			{
				SetValue(TypeProperty, value);
				textBlock.Text = Properties.Resources.ResourceManager.GetString(value.ToString());
			}
		}

		public Icon() => InitializeComponent();

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			Type = Type;
		}
	}
}
