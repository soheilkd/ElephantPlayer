using Library.Extensions;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using static Player.Views.ViewerOperator;

namespace Player.Views
{
	public partial class ArtistsView : ContentControl
	{
		private bool IsAnyItemExpanded => _CurrentArtistContent.Visibility == Visibility.Visible;
		private ArtistContent _CurrentArtistContent = new ArtistContent();
		private ArtistTile _CurrentArtistTile = new ArtistTile();
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
				MainGrid.Children.Add(_CurrentArtistContent);
				MainGrid.SizeChanged += (_, __) => MainGrid.AlignChildrenVertical(TileSize);
				MainGrid.AlignChildrenVertical(TileSize);
			}
			_CurrentArtistContent.Visibility = Visibility.Collapsed;
		}

		private void Tile_Collapsed(object sender, EventArgs e)
		{
			_CurrentArtistContent.Visibility = Visibility.Collapsed;
			_Tiles.For(each => each.Margin = default);
		}

		private void Tile_Expanded(object sender, Library.InfoExchangeArgs<string> e)
		{
			_CurrentArtistContent.ChangeArtist(e.Parameter);
			_CurrentArtistContent.Visibility = Visibility.Visible;
			var senderTile = sender as ArtistTile;
			_Tiles.For(each => each.ChangeStatus(false));
			senderTile.ChangeStatus(true);
			if (senderTile.GetRow() != _CurrentArtistContent.GetRow())
			{
				var row = senderTile.GetRow() + 1;
				_Tiles.For(each =>
				{
					if (each.GetRow() == row)
						each.Margin = new Thickness(0, 200, 0, 0);
				});
				Grid.SetRow(_CurrentArtistContent, row);
				Grid.SetColumn(_CurrentArtistContent, 0);
				Grid.SetColumnSpan(_CurrentArtistContent, MainGrid.ColumnDefinitions.Count);
			}
		}
	}
}
