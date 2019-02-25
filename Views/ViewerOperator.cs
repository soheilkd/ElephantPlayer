using Lastfm.Services;
using Library.Controls;
using Library.Controls.Navigation;
using Library.Extensions;
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
		public static void ApplyNavigations(IEnumerable<string> keys, Type lookupType, Type viewType, NavigationViewer viewer)
		{
			var navigations = new List<NavigationTile>();
			keys.ForEach(each => navigations.Add(GetTile(each, viewType)));
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
						Foreground = System.Windows.Media.Brushes.White,
						Navigation = new NavigationControl()
						{
							Tag = name,
							Content = Activator.CreateInstance(viewType, name) //Since input may vary (Now: ArtistView, AlbumView or PlaylistView)
						}
					};
		}
		public static string GetImageUrl(string key, Type lookupType)
		{
			try
			{
				if (lookupType == typeof(Artist))
				{
					Artist artist = Web.GetArtist(key);
					return artist == null ? default : artist.GetImageURL();
				}
				else if (lookupType == typeof(Album))
				{
					Album album = Web.GetAlbum(key);
					return album == null ? default : album.GetImageURL();
				}
				else
					return default;
			}
			catch (Exception)
			{
				return default;
			}
		}
		public static void LoadImagesOfTiles(List<NavigationTile> navigations, Type lookupType)
		{
			BitmapSource unknownArtistImage = IconProvider.GetBitmap(IconType.Person);
			navigations.For(each =>
			{
				string tag = default;
				each.Dispatcher.Invoke(() => tag = each.Tag.ToString());
				Task.Run(() =>
				{
					if (Resource.ContainsKey(tag))
					{
						if (Resource[tag].Length != 0) each.ChangeImageByDispatcher(Resource[tag].ToBitmap());
						else each.Dispatcher.Invoke(() => each.Image = unknownArtistImage);
					}
					else
					{
						var url = GetImageUrl(tag, lookupType);
						if (string.IsNullOrWhiteSpace(url))
						{
							each.ChangeImageByDispatcher(unknownArtistImage);
							Resource.Add(tag, new byte[0]);
						}
						else
						{
							Web.DownloadImage(url, image =>
							{
								each.ChangeImageByDispatcher(image);
								Resource.Add(tag, image.ToData());
							});
						}
					}
				});
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