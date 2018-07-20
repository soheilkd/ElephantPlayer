using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;

namespace Player.Controls
{
	public partial class Icon : UserControl
	{
		public static readonly DependencyProperty IconProperty =
			DependencyProperty.Register(nameof(Kind), typeof(IconKind), typeof(Icon), new PropertyMetadata(IconKind.AccessPoint));

		public IconKind Kind
		{
			get => (IconKind)GetValue(IconProperty);
			set
			{
				SetValue(IconProperty, value);
				MainIcon.Kind = (PackIconKind)(int)value;
			}
		}

		public Icon() => InitializeComponent();

		private void UserControl_Loaded(object sender, RoutedEventArgs e) => Kind = Kind;
	}
}
