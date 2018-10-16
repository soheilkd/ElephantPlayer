using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Player.Controls.Navigation;
using Player.Extensions;
using Player.Models;

namespace Player.Views
{
	public partial class ArtistsView : Grid
	{
		public event EventHandler<InfoExchangeArgs<(MediaQueue, Media)>> PlayRequested;
		private int CallTime = -1; //It's used for Lazy Loading, reaches 1 when user enters AlbumsView tab on MainWindow
		public ArtistsView()
		{
			InitializeComponent();
		}

		private async void Grid_Loaded(object sender, RoutedEventArgs e)
		{
			if (CallTime++ != 0)
				return;

			var artists = Controllers.LibraryController.LoadedCollection.GroupBy(each => each.Artist).OrderBy(each => each.Key);
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
							onPlay: (queue, media) => PlayRequested?.Invoke(this, new InfoExchangeArgs<(MediaQueue, Media)>((queue, media))))
						}
					}));
			navigations.For(each => grid.Children.Add(each));
			grid.AlignItems(Controls.Tile.StandardSize);
			grid.SizeChanged += (_, __) => grid.AlignItems(Controls.Tile.StandardSize);
			Console.WriteLine($"Count: {navigations.Count}");
			navigations.For(each =>
			{
				if (Controllers.ResourceController.Contains(each.Tag.ToString(), out var imageData))
				{
					each.Image = new SerializableBitmap(Controllers.ResourceController.Get(each.Tag.ToString()));
				}
				else
				{
					each.DownloadAndApplyImage(new Func<string, string>(Web.API.GetArtistImageUrl), each.Tag.ToString());
				}
			}
			);
			await Task.Delay(1);
		}
	}
}
