using Library.Extensions;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Player.Views
{
	public static class ViewerOperator
	{
		private const string InitializedTag = "Initialized";
		public static void Initialize(Grid grid, IEnumerable<string> keys, ItemSubcontent refToSubcontent)
		{
			if (grid.Tag?.ToString() == "Initialized")
				return;
			var _Tiles = new List<ItemTile>();
			ItemTile currentTile = default;

			void AlignContent(int row)
			{
				_Tiles.For(each => each.Margin = default);
				_Tiles.For(each =>
				{
					if (each.GetRow() == row)
						each.Margin = new Thickness(0, 0, 0, 200);
				});
				Grid.SetRow(refToSubcontent, row);
				Grid.SetColumnSpan(refToSubcontent, grid.ColumnDefinitions.Count);
			}
			keys.ForEach(each =>
			{
				var tile = new ItemTile(each, refToSubcontent.ImageLoader);
				tile.Expanded += (sender, e) =>
				{
					currentTile = sender as ItemTile;
					if (grid.Children.IndexOf(refToSubcontent) < 0)
						grid.Children.Add(refToSubcontent);
					refToSubcontent.ChangeContent(e.Parameter);
					_Tiles.For(item => item.ChangeStatus(false));
					currentTile.ChangeStatus(true);
					if (currentTile.GetRow() != refToSubcontent.GetRow())
						AlignContent(currentTile.GetRow());
				};
				tile.Collapsed += delegate
				{
					grid.Children.Remove(refToSubcontent);
					_Tiles.For(item => item.Margin = default);
					currentTile = null;
				};
				grid.Children.Add(tile);
				_Tiles.Add(tile);
			});
			grid.SizeChanged += (_, __) =>
			{
				grid.Children.Remove(refToSubcontent);
				grid.AlignChildrenVertical(Controller.TileSize);
				if (currentTile != null)
				{
					grid.Children.Add(refToSubcontent);
					AlignContent(currentTile.GetRow());
				}
			};
			grid.AlignChildrenVertical(Controller.TileSize);
			grid.Children.Add(refToSubcontent);
			grid.Tag = "Initialized";
		}
	}
}