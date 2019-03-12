using Library.Extensions;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Player.Views
{
	public partial class ArtistsView : ContentControl
	{
		private bool IsAnyItemExpanded => _CurrentArtistContent.Visibility == Visibility.Visible;
		private ArtistContent _CurrentArtistContent = new ArtistContent();
		private ArtistTile _CurrentArtistTile = null;
		private List<ArtistTile> _Tiles = new List<ArtistTile>();
		private int CallTime = -1; //It's used for Lazy Loading, reaches 1 when user enters ArtistsView tab on MainWindow
		public ArtistsView() => InitializeComponent();

		private void Grid_Loaded(object sender, RoutedEventArgs e)
		{
			if (CallTime++ == 0)
			{
					Controller.Library.Artists.Keys.ForEach(each =>
				{
					var tile = new ArtistTile(each);
					tile.Expanded += Tile_Expanded;
					tile.Collapsed += Tile_Collapsed;
					MainGrid.Children.Add(tile);
					_Tiles.Add(tile);
				});
				MainGrid.SizeChanged += (_, __) =>
				{
					MainGrid.Children.Remove(_CurrentArtistContent);
					MainGrid.AlignChildrenVertical(Controller.TileSize);
					if (_CurrentArtistTile != null)
					{
						MainGrid.Children.Add(_CurrentArtistContent);
						AlignArtistContent(_CurrentArtistTile.GetRow());
					}
				};
				MainGrid.AlignChildrenVertical(Controller.TileSize);
				MainGrid.Children.Add(_CurrentArtistContent);
			}
		}

		private void Tile_Collapsed(object sender, EventArgs e)
		{
			MainGrid.Children.Remove(_CurrentArtistContent);
			_Tiles.For(each => each.Margin = default);
			_CurrentArtistTile = null;
		}

		private void Tile_Expanded(object sender, Library.InfoExchangeArgs<string> e)
		{
			_CurrentArtistTile = sender as ArtistTile;
			if (MainGrid.Children.IndexOf(_CurrentArtistContent) < 0)
				MainGrid.Children.Add(_CurrentArtistContent);
			_CurrentArtistContent.ChangeArtist(e.Parameter);
			_Tiles.For(each => each.ChangeStatus(false));
			_CurrentArtistTile.ChangeStatus(true);
			if (_CurrentArtistTile.GetRow() != _CurrentArtistContent.GetRow())
				AlignArtistContent(_CurrentArtistTile.GetRow());
		}

		private void AlignArtistContent(int row)
		{
			_Tiles.For(each => each.Margin = default);
			_Tiles.For(each =>
			{
				if (each.GetRow() == row)
					each.Margin = new Thickness(0, 0, 0, 200);
			});
			Grid.SetRow(_CurrentArtistContent, row);
			Grid.SetColumnSpan(_CurrentArtistContent, MainGrid.ColumnDefinitions.Count);
		}
	}
}
