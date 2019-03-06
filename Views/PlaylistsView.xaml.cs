using Library.Extensions;
using Player.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static Player.Views.ViewerOperator;

namespace Player.Views
{
	public partial class PlaylistsView : ContentControl
	{
		private bool IsAnyItemExpanded => _CurrentPlaylistContent.Visibility == Visibility.Visible;
		private PlaylistContent _CurrentPlaylistContent = new PlaylistContent();
		private PlaylistTile _CurrentPlaylistTile = new PlaylistTile();
		private List<PlaylistTile> _Tiles = new List<PlaylistTile>();
		private int CallTime = -1; //It's used for Lazy Loading, reaches 1 when user enters ArtistsView tab on MainWindow
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
				MainGrid.Children.Add(_CurrentPlaylistContent);
				MainGrid.SizeChanged += (_, __) => MainGrid.AlignChildrenVertical(TileSize);
				MainGrid.AlignChildrenVertical(TileSize);
			}
			_CurrentPlaylistContent.Visibility = Visibility.Collapsed;
		}

		private void Tile_Collapsed(object sender, EventArgs e)
		{
			_CurrentPlaylistContent.Visibility = Visibility.Collapsed;
			_Tiles.For(each => each.Margin = default);
		}

		private void Tile_Expanded(object sender, Library.InfoExchangeArgs<string> e)
		{
			_CurrentPlaylistContent.ChangePlaylist(e.Parameter);
			_CurrentPlaylistContent.Visibility = Visibility.Visible;
			var senderTile = sender as ArtistTile;
			_Tiles.For(each => each.ChangeStatus(false));
			senderTile.ChangeStatus(true);
			if (senderTile.GetRow() != _CurrentPlaylistContent.GetRow())
			{
				var row = senderTile.GetRow() + 1;
				_Tiles.For(each =>
				{
					if (each.GetRow() == row)
						each.Margin = new Thickness(0, 200, 0, 0);
				});
				Grid.SetRow(_CurrentPlaylistContent, row);
				Grid.SetColumn(_CurrentPlaylistContent, 0);
				Grid.SetColumnSpan(_CurrentPlaylistContent, MainGrid.ColumnDefinitions.Count);
			}
		}
		private void CreateButtonClick(object sender, RoutedEventArgs e)
		{
			Controller.Library.Playlists.Add(new MediaQueue() { Name = NewPlaylistBox.Text });
			NewPlaylistBox.Text = "New Playlist Name";
			AddTiles(MainGrid, typeof(PlaylistTile), Controller.Library.Playlists.Select(each => each.Name));
		}
	}
}
