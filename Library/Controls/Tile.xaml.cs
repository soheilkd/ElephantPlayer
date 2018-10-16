using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Player.Extensions;

namespace Player.Controls
{
	public partial class Tile : UserControl
	{
		public static readonly Size StandardSize = new Size(110, 40);

		public static readonly DependencyProperty ImageProperty =
			DependencyProperty.Register(nameof(Image), typeof(ImageSource), typeof(Tile), new PropertyMetadata(null));
		public static readonly DependencyProperty TileStyleProperty =
			DependencyProperty.Register(nameof(TileStyle), typeof(TileStyle), typeof(Tile), new PropertyMetadata(TileStyle.Default));

		public ImageSource Image
		{
			get => (ImageSource)GetValue(ImageProperty);
			set
			{
				SetValue(ImageProperty, value);
				MainImage.Source = value;
			}
		}
		public TileStyle TileStyle
		{
			get => (TileStyle)GetValue(TileStyleProperty);
			set => SetValue(TileStyleProperty, value);
		}

		public Tile()
		{
			InitializeComponent();
		}

		private void Parent_Loaded(object sender, RoutedEventArgs e)
		{
			MainImage.Source = Image;
			Style = (Style)Resources[TileStyle.ToString()];
			UpdateLayout();
			if (MainTextBlock.ActualWidth < Width)
			{
				parent.Content.As<Grid>().Children.Remove(TilePopup);
				TilePopup.Child = null;
			}
		}
	}
}
