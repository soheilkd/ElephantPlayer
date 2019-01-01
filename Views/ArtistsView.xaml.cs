using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using static Player.Controller;

namespace Player.Views
{
	public partial class ArtistsView : ContentControl
	{
		private int CallTime = -1; //It's used for Lazy Loading, reaches 1 when user enters ArtistsView tab on MainWindow
		public ArtistsView()
		{
			InitializeComponent();
		}

		List<NavigationTile> Navigations = new List<NavigationTile>();
		List<string> Artists = new List<string>();

		private void AddNavigationsToGrid()
		{
			var grid = ArtistNavigation.GetChildContent(1) as Grid;
			grid.Children.Clear();
			Navigations.For(each => grid.Children.Add(each));
			grid.AlignChildrenVertical(Tile.StandardSize);
		}
    
		private void LoadImagesOfTiles()
		{
			BitmapSource unknownArtistImage = IconProvider.GetBitmap(IconType.Person);
			Navigations.For(each =>
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
                        var artist = Web.API.GetArtist(tag);
                        var url = artist.GetImageURL() ?? default;
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

		private NavigationTile GetTileFor(string artist)
		{
			return
					new NavigationTile()
					{
						Tag = artist,
						Navigation = new NavigationControl()
						{
							Tag = artist,
							Content = new ArtistView(artist)
						}
					};
		}

		public void Load()
		{
			Artists = Controller.Library.GetArtists().ToList();

			Artists.For(each => Navigations.Add(GetTileFor(each)));
			AddNavigationsToGrid();
			LoadImagesOfTiles();
		}
		private void Grid_Loaded(object sender, RoutedEventArgs e)
		{
			if (CallTime++ != 0)
				return;
			Load();
			Controller.Library.CollectionChanged += LibraryChanged;
			var grid = ArtistNavigation.GetChildContent(1) as Grid;
			grid.SizeChanged += (_, __) => grid.AlignChildrenVertical(Tile.StandardSize);
		}

		private void LibraryChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			 if (e.Action == NotifyCollectionChangedAction.Add)
			{
				foreach (Media item in e.NewItems)
				{
					if (!Artists.Contains(item.Artist))
					{
						Artists.Add(item.Artist);
						Navigations.Add(GetTileFor(item.Artist));
					}
				}
			}
			else if (e.Action == NotifyCollectionChangedAction.Remove)
			{
				foreach (Media item in e.OldItems)
				{
					if (Controller.Library.Where(each => each.Artist == item.Artist).Count() == 0)
						Navigations.RemoveAll(each => each.Tag.ToString() == item.Artist);
				}
			}
		}
	}
}
