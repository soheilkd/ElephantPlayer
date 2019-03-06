using Library;
using Library.Extensions;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using static Player.Views.ViewerOperator;

namespace Player.Views
{
	public partial class AlbumsView : ContentControl
	{
		private bool IsAnyItemExpanded => _CurrentAlbumContent.Visibility == Visibility.Visible;
		private AlbumContent _CurrentAlbumContent = new AlbumContent();
		private AlbumTile _CurrentAlbumTile = new AlbumTile();
		private List<AlbumTile> _Tiles = new List<AlbumTile>();
		private int CallTime = -1; //It's used for Lazy Loading, reaches 1 when user enters ArtistsView tab on MainWindow
		public AlbumsView() => InitializeComponent();

		private void Grid_Loaded(object sender, RoutedEventArgs e)
		{
			if (CallTime++ == 0)
			{
				Controller.Library.Albums.Keys.ForEach(each =>
				{
					var tile = new AlbumTile(each);
					tile.Expanded += Tile_Expanded;
					tile.Collapsed += Tile_Collapsed;
					MainGrid.Children.Add(tile);
					_Tiles.Add(tile);
				});
				MainGrid.Children.Add(_CurrentAlbumContent);
				MainGrid.SizeChanged += (_, __) => MainGrid.AlignChildrenVertical(TileSize);
				MainGrid.AlignChildrenVertical(TileSize);
			}
			_CurrentAlbumContent.Visibility = Visibility.Collapsed;
		}

		private void Tile_Collapsed(object sender, EventArgs e)
		{
			_CurrentAlbumContent.Visibility = Visibility.Collapsed;
			_Tiles.For(each => each.Margin = default);
		}

		private void Tile_Expanded(object sender, InfoExchangeArgs<string> e)
		{
			_CurrentAlbumContent.ChangeAlbum(e.Parameter);
			_CurrentAlbumContent.Visibility = Visibility.Visible;
			var senderTile = sender as ArtistTile;
			_Tiles.For(each => each.ChangeStatus(false));
			senderTile.ChangeStatus(true);
			if (senderTile.GetRow() != _CurrentAlbumContent.GetRow())
			{
				var row = senderTile.GetRow() + 1;
				_Tiles.For(each =>
				{
					if (each.GetRow() == row)
						each.Margin = new Thickness(0, 200, 0, 0);
				});
				Grid.SetRow(_CurrentAlbumContent, row);
				Grid.SetColumn(_CurrentAlbumContent, 0);
				Grid.SetColumnSpan(_CurrentAlbumContent, MainGrid.ColumnDefinitions.Count);
			}
		}
	}
}
