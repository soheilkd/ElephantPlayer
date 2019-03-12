using Library.Extensions;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Player.Views
{
	public partial class AlbumsView : ContentControl
	{
		private bool IsAnyItemExpanded => _CurrentAlbumContent.Visibility == Visibility.Visible;
		private AlbumContent _CurrentAlbumContent = new AlbumContent();
		private AlbumTile _CurrentAlbumTile = null;
		private List<AlbumTile> _Tiles = new List<AlbumTile>();
		private int CallTime = -1; //It's used for Lazy Loading, reaches 1 when user enters AlbumsView tab on MainWindow
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
				MainGrid.SizeChanged += (_, __) =>
				{
					MainGrid.Children.Remove(_CurrentAlbumContent);
					MainGrid.AlignChildrenVertical(Controller.TileSize);
					if (_CurrentAlbumTile != null)
					{
						MainGrid.Children.Add(_CurrentAlbumContent);
						AlignAlbumContent(_CurrentAlbumTile.GetRow());
					}
				};
				MainGrid.AlignChildrenVertical(Controller.TileSize);
				MainGrid.Children.Add(_CurrentAlbumContent);
			}
		}

		private void Tile_Collapsed(object sender, EventArgs e)
		{
			MainGrid.Children.Remove(_CurrentAlbumContent);
			_Tiles.For(each => each.Margin = default);
			_CurrentAlbumTile = null;
		}

		private void Tile_Expanded(object sender, Library.InfoExchangeArgs<string> e)
		{
			_CurrentAlbumTile = sender as AlbumTile;
			if (MainGrid.Children.IndexOf(_CurrentAlbumContent) < 0)
				MainGrid.Children.Add(_CurrentAlbumContent);
			_CurrentAlbumContent.ChangeAlbum(e.Parameter);
			_Tiles.For(each => each.ChangeStatus(false));
			_CurrentAlbumTile.ChangeStatus(true);
			if (_CurrentAlbumTile.GetRow() != _CurrentAlbumContent.GetRow())
				AlignAlbumContent(_CurrentAlbumTile.GetRow());
		}

		private void AlignAlbumContent(int row)
		{
			_Tiles.For(each => each.Margin = default);
			_Tiles.For(each =>
			{
				if (each.GetRow() == row)
					each.Margin = new Thickness(0, 0, 0, 200);
			});
			Grid.SetRow(_CurrentAlbumContent, row);
			Grid.SetColumnSpan(_CurrentAlbumContent, MainGrid.ColumnDefinitions.Count);
		}
	}
}
