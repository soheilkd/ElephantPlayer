using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Library.Controls;
using Library.Controls.Navigation;
using Library.Extensions;
using Library.Serialization.Models;
using Player.Models;
using static Player.App; //For Resource

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
			var unknownArtistImage = Properties.Resources.UnknownArtist.ToImageSource();
			IOrderedEnumerable<IGrouping<string, Media>> artists = LibraryManager.Data.GroupBy(each => each.Artist).OrderBy(each => each.Key);
			var grid = ArtistNavigation.GetChildContent(1) as Grid;
			var navigations = new List<NavigationTile>();
			artists.ForEach(each =>
				navigations.Add(
					new NavigationTile()
					{
						Tag = each.Key,
						TileStyle = TileStyle.Default,
						Navigation = new NavigationControl()
						{
							Tag = each.Key,
							Content = new GroupMediaView(new MediaQueue(each),
							onPlay: (queue, media) => PlayRequested?.Invoke(this, new QueueEventArgs(queue, media)))
						}
					}));
			navigations.For(each => grid.Children.Add(each));
			grid.AlignChildrenVertical(new Size(50, 100));
			grid.SizeChanged += (_, __) => grid.AlignChildrenVertical(Tile.StandardSize);
			navigations.For(each =>
			{
				if (Resource.Value.TryGetValue(each.Tag.ToString(), out byte[] imageData))
				{
					each.Image = new SerializableBitmap(imageData);
				}
				else
				{
					Task.Run(() =>
					{
						var tag = string.Empty;
						each.Dispatcher.Invoke(() => tag = each.Tag.ToString());
						var url = Web.API.GetArtistImageUrl(tag);
						if (string.IsNullOrWhiteSpace(url))
							each.Dispatcher.Invoke(() => each.Image = unknownArtistImage);
						var client = new WebClient();
						client.DownloadDataCompleted += (_, d) =>
						{
							each.Dispatcher.Invoke(() => each.Image = d.Result.ToBitmap());
							Dispatcher.Invoke(() => Resource.Value[tag] = new SerializableBitmap(each.Image as BitmapImage));
						};
						client.DownloadDataAsync(new Uri(url));
					});
				}
			}
			);
			await Task.Delay(1);
		}
	}
}
