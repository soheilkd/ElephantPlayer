using Library.Extensions;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Player.Views
{
	public partial class PlaylistsView : ContentControl
	{
		private bool IsAnyItemExpanded => _CurrentPlaylistContent.Visibility == Visibility.Visible;
		private PlaylistContent _CurrentPlaylistContent = new PlaylistContent();
		private PlaylistTile _CurrentPlaylistTile = null;
		private List<PlaylistTile> _Tiles = new List<PlaylistTile>();
		private int CallTime = -1; //It's used for Lazy Loading, reaches 1 when user enters PlaylistsView tab on MainWindow
		public PlaylistsView() => InitializeComponent();

		private void Grid_Loaded(object sender, RoutedEventArgs e)
		{
			if (CallTime++ == 0)
			{
					Controller.Library.Playlists.ForEach(each =>
				{
					var tile = new PlaylistTile(each.Name);
					tile.Expanded += Tile_Expanded;
					tile.Collapsed += Tile_Collapsed;
					MainGrid.Children.Add(tile);
					_Tiles.Add(tile);
				});
				MainGrid.SizeChanged += (_, __) =>
				{
					MainGrid.Children.Remove(_CurrentPlaylistContent);
					MainGrid.AlignChildrenVertical(Controller.TileSize);
					if (_CurrentPlaylistTile != null)
					{
						MainGrid.Children.Add(_CurrentPlaylistContent);
						AlignPlaylistContent(_CurrentPlaylistTile.GetRow());
					}
				};
				MainGrid.AlignChildrenVertical(Controller.TileSize);
				MainGrid.Children.Add(_CurrentPlaylistContent);
			}
		}

		private void Tile_Collapsed(object sender, EventArgs e)
		{
			MainGrid.Children.Remove(_CurrentPlaylistContent);
			_Tiles.For(each => each.Margin = default);
			_CurrentPlaylistTile = null;
		}

		private void Tile_Expanded(object sender, Library.InfoExchangeArgs<string> e)
		{
			_CurrentPlaylistTile = sender as PlaylistTile;
			if (MainGrid.Children.IndexOf(_CurrentPlaylistContent) < 0)
				MainGrid.Children.Add(_CurrentPlaylistContent);
			_CurrentPlaylistContent.ChangePlaylist(e.Parameter);
			_Tiles.For(each => each.ChangeStatus(false));
			_CurrentPlaylistTile.ChangeStatus(true);
			if (_CurrentPlaylistTile.GetRow() != _CurrentPlaylistContent.GetRow())
				AlignPlaylistContent(_CurrentPlaylistTile.GetRow());
		}

		private void AlignPlaylistContent(int row)
		{
			_Tiles.For(each => each.Margin = default);
			_Tiles.For(each =>
			{
				if (each.GetRow() == row)
					each.Margin = new Thickness(0, 0, 0, 200);
			});
			Grid.SetRow(_CurrentPlaylistContent, row);
			Grid.SetColumnSpan(_CurrentPlaylistContent, MainGrid.ColumnDefinitions.Count);
		}
	}
}
