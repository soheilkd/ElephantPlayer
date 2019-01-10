using Lastfm.Services;
using Library.Controls;
using Library.Controls.Navigation;
using Library.Extensions;
using Library.Serialization.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static Player.Controller;

namespace Player.Views
{
	public static class ViewerOperator
	{
		public static void ApplyNavigations(string[] keys, Type lookupType, Type viewType, NavigationViewer viewer)
		{
			var navigations = new List<NavigationTile>();
			keys.For(each => navigations.Add(GetTile(each, viewType)));
			LoadImagesOfTiles(navigations, lookupType);
			AddNavigations(navigations, viewer);
			var grid = viewer.GetChildContent(1) as Grid;
			grid.SizeChanged += (_, __) => grid.AlignChildrenVertical(Tile.StandardSize);
		}

		public static NavigationTile GetTile(string name, Type viewType)
		{
			return
					new NavigationTile()
					{
						Tag = name,
						Navigation = new NavigationControl()
						{
							Tag = name,
							Content = Activator.CreateInstance(viewType, name) //Since input may vary (Now: ArtistView, AlbumView or PlaylistView)
						}
					};
		}

		public static void LoadImagesOfTiles(List<NavigationTile> navigations, Type lookupType)
		{
			if (lookupType == default)
				return;
			BitmapSource unknownArtistImage = IconProvider.GetBitmap(IconType.Person);
			navigations.For(each =>
			{
				if (Resource.TryGetValue(each.Tag.ToString(), out var imageData))
				{
					each.Image = new SerializableBitmap(imageData);
				}
				else
				{
					Task.Run(() =>
					{
						var tag = string.Empty;
						each.Dispatcher.Invoke(() => tag = each.Tag.ToString());
						string url = default;
						if (lookupType == typeof(Artist))
							url = Web.API.GetArtist(tag).GetImageURL();
						else if (lookupType == typeof(Album))
							url = Web.API.GetAlbum(tag).GetImageURL();
						else
							throw new ArgumentException(nameof(lookupType));
						if (string.IsNullOrWhiteSpace(url))
							each.Dispatcher.Invoke(() => each.Image = unknownArtistImage);
						Web.API.DownloadImage(url, image =>
						{
							each.Dispatcher.Invoke(() => each.Image = image);
							Resource[tag] = new SerializableBitmap(image);
						});
					});
				}
			}
			);
		}

		public static void AddNavigations(List<NavigationTile> navigations, NavigationViewer navigationViewer)
		{
			var grid = navigationViewer.GetChildContent(1) as Grid;
			grid.Children.Clear();
			navigations.For(each => grid.Children.Add(each));
			grid.AlignChildrenVertical(Tile.StandardSize);
		}
	}
}
