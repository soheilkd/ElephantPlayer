using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Player.Controllers;
using Player.Controls.Navigation;
using Player.Extensions;
using Player.Models;

namespace Player.Views
{
	public partial class ArtistsView : Grid
	{
		public event EventHandler<QueueEventArgs> PlayRequested;
		private int CallTime = -1; //It's used for Lazy Loading, reaches 1 when user enters AlbumsView tab on MainWindow
		public ArtistsView()
		{
			InitializeComponent();
		}

		private async void Grid_Loaded(object sender, RoutedEventArgs e)
		{
			if (CallTime++ != 0)
				return;

			IOrderedEnumerable<IGrouping<string, Media>> artists = Library.Data.GroupBy(each => each.Artist).OrderBy(each => each.Key);
			var grid = ArtistNavigation.GetChildContent(1) as Grid;
			var navigations = new List<NavigationTile>();
			artists.ForEach(each =>
				navigations.Add(
					new NavigationTile()
					{
						Tag = each.Key,
						TileStyle = Controls.TileStyle.Default,
						Navigation = new NavigationControl()
						{
							Tag = each.Key,
							Content = new GroupMediaView(new MediaQueue(each),
							onPlay: (queue, media) => PlayRequested?.Invoke(this, new QueueEventArgs(queue, media)))
						}
					}));
			navigations.For(each => grid.Children.Add(each));
			grid.AlignItems(Controls.Tile.StandardSize);
			grid.SizeChanged += (_, __) => grid.AlignItems(Controls.Tile.StandardSize);
			Console.WriteLine($"Count: {navigations.Count}");
			navigations.For(each =>
			{
				if (Resource.Contains(each.Tag.ToString(), out var imageData))
				{
					each.Image = new SerializableBitmap(Resource.Get(each.Tag.ToString()));
				}
				else
				{
					Task.Run(() =>
					{
						var tag = string.Empty;
						each.Dispatcher.Invoke(() => tag = each.Tag.ToString());
						var url = Web.API.GetArtistImageUrl(tag);
						if (string.IsNullOrWhiteSpace(url))
							return;
						try
						{
							var client = new WebClient();
							client.DownloadDataCompleted += (_, d) =>
							{
								each.Dispatcher.Invoke(() => each.Image = d.Result.ToBitmap());
								Dispatcher.Invoke(() => Resource.AddOrSet(tag, new SerializableBitmap(each.Image as BitmapImage)));
							};
							client.DownloadDataAsync(new Uri(url));
						}
						catch (Exception)
						{

						}
					});
				}
			}
			);
			await Task.Delay(1);
		}
	}
}
