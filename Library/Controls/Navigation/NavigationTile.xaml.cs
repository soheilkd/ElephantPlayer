using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Player.Controllers;
using Player.Extensions;
using Player.Models;

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
		
		public TileStyle TileStyle
		{
			get => (TileStyle)GetValue(TileStyleProperty);
			set => SetValue(TileStyleProperty, value);
		}

		public ImageSource Image
		{
			get => tile.Image;
			set => tile.Image = value;
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
		
		public void DownloadAndApplyImage<T>(Func<T, string> func, T arg)
		{
			Task.Run(() => DownloadAndApplyImage(func(arg)));
		}
		public void DownloadAndApplyImage(string url)
		{
			if (string.IsNullOrWhiteSpace(url))
				return;
			try
			{
				var client = new WebClient();
				client.DownloadDataCompleted += (_, e) =>
				{
					Dispatcher.Invoke(() => Image = e.Result.ToBitmap());
					Dispatcher.Invoke(() => ResourceController.AddOrSet(Tag.ToString(), new SerializableBitmap(Image as BitmapImage)));
				};
				client.DownloadDataAsync(new Uri(url));
			}
			catch (Exception)
			{

			}
		}
	}
}
