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
    public partial class AlbumsView : ContentControl
    {
        private int CallTime = -1; //It's used for Lazy Loading, reaches 1 when user enters AlbumsView tab on MainWindow
        public AlbumsView()
        {
            InitializeComponent();
        }
        List<NavigationTile> Navigations = new List<NavigationTile>();
        List<string> Albums = new List<string>();

        private void AddNavigationsToGrid()
        {
            var grid = AlbumNavigation.GetChildContent(1) as Grid;
            grid.Children.Clear();
            Navigations.For(each => grid.Children.Add(each));
            grid.AlignChildrenVertical(Tile.StandardSize);
        }
        private void LoadImagesOfTiles()
        {
            BitmapSource unknownAlbumImage = IconProvider.GetBitmap(IconType.Music);
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
                        var album = Web.API.GetAlbum(tag);
                        var url = album != default ? album.GetImageURL() : default;
                        if (string.IsNullOrWhiteSpace(url))
                            each.Dispatcher.Invoke(() => each.Image = unknownAlbumImage);
                        var client = new WebClient();
                        client.DownloadDataCompleted += (_, d) =>
                        {
                            each.Dispatcher.Invoke(() => each.Image = d.Result.ToBitmap());
                            Dispatcher.Invoke(() => Resource[tag] = new SerializableBitmap(each.Image as BitmapImage));
                        };
                        client.DownloadDataAsync(new Uri(url));
                    });
                }
            }
            );
        }

        private NavigationTile GetTileFor(string album)
        {
            return
                    new NavigationTile()
                    {
                        Tag = album,
                        Navigation = new NavigationControl()
                        {
                            Tag = album,
                            Content = new AlbumView(album)
                        }
                    };
        }

        public void Load()
        {
            Albums = Controller.Library.GetAlbums().ToList();

            Albums.For(each => Navigations.Add(GetTileFor(each)));
            AddNavigationsToGrid();
            LoadImagesOfTiles();
        }
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            if (CallTime++ != 0)
                return;
            Load();
            Controller.Library.CollectionChanged += LibraryChanged;
            var grid = AlbumNavigation.GetChildContent(1) as Grid;
            grid.SizeChanged += (_, __) => grid.AlignChildrenVertical(Tile.StandardSize);
        }

        private void LibraryChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Media item in e.NewItems)
                {
                    if (!Albums.Contains(item.Album))
                    {
                        Albums.Add(item.Album);
                        Navigations.Add(GetTileFor(item.Album));
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Media item in e.OldItems)
                {
                    if (Controller.Library.Where(each => each.Album == item.Album).Count() == 0)
                        Navigations.RemoveAll(each => each.Tag.ToString() == item.Album);
                }
            }
        }
    }
}