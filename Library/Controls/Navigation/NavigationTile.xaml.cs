using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Player.Extensions;

namespace Player.Controls.Navigation
{
	public partial class NavigationTile : UserControl
	{
		public static readonly DependencyProperty NavigationProperty =
			DependencyProperty.Register(nameof(Navigation), typeof(NavigationControl), typeof(NavigationTile), new PropertyMetadata(default));
		public static readonly DependencyProperty TileStyleProperty =
			DependencyProperty.Register(nameof(TileStyle), typeof(TileStyle), typeof(NavigationTile), new PropertyMetadata(TileStyle.Default));

		public NavigationControl Navigation
		{
			get => (NavigationControl)GetValue(NavigationProperty);
			set => SetValue(NavigationProperty, value);
		}

		public static readonly DependencyProperty ImageProperty =
			DependencyProperty.Register(nameof(Image), typeof(ImageSource), typeof(NavigationTile), new PropertyMetadata(null));

		public TileStyle TileStyle
		{
			get => (TileStyle)GetValue(TileStyleProperty);
			set => SetValue(TileStyleProperty, value);
		}

		public ImageSource Image
		{
			get => (ImageSource)GetValue(ImageProperty);
			set => SetValue(ImageProperty, value);
		}

		public object ParentContent;

		private NavigationViewer ParentNavigationViewer;

		public NavigationTile()
		{
			InitializeComponent();
		}

		private void GetParentFrame()
		{
			for (int i = 0; i < 20; i++)
			{
				DependencyObject p = this.GetParent(i);
				if (p == null)
					throw new NotSupportedException("Couldn't find parent navigation viewer");
				if (p is NavigationViewer control)
				{
					ParentNavigationViewer = control;
					break;
				}
			}
			ParentContent = ParentNavigationViewer.Content;
		}

		private void Tile_MouseUp(object sender, MouseButtonEventArgs e)
		{
			GetParentFrame();

			ParentNavigationViewer.OpenView(Navigation);
		}

		private void Tile_Loaded(object sender, RoutedEventArgs e)
		{
			tile.TileStyle = TileStyle;
			tile.Image = Image;
		}

		private void Navigation_BackClicked(object sender, EventArgs e)
		{
			ParentNavigationViewer.ReturnToMainView();
		}
	}
}
