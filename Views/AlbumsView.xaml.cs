using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Library.Controls;
using Library.Controls.Navigation;
using Library.Extensions;
using Player.Controllers;
using Player.Models;

namespace Player.Views
{
	public partial class AlbumsView : Grid
	{
		public event EventHandler<QueueEventArgs> PlayRequested;
		private int CallTime = -1; //It's used for Lazy Loading, reaches 1 when user enters AlbumsView tab on MainWindow
		public AlbumsView()
		{
			InitializeComponent();
		}

		private void Grid_Loaded(object sender, RoutedEventArgs e)
		{
			if (CallTime++ != 0)
				return;
			var albums = LibraryManager.Data.GroupBy(each => each.Album).OrderBy(each => each.Key);
			var grid = AlbumNavigation.GetChildContent(1) as Grid;
			albums.ForEach(each =>
				grid.Children.Add(
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
			grid.AlignItems(Tile.StandardSize);
			grid.SizeChanged += (_, __) => grid.AlignItems(Tile.StandardSize);
		}
	}
}
