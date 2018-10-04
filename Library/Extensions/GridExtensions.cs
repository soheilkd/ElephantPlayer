using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Player.Extensions
{
	public static class GridExtensions
	{
		public static void AlignInRow(IList<UIElement> elements, int row, int zIndex = 0)
		{
			for (int i = 0; i < elements.Count; i++)
			{
				Grid.SetRow(elements[i], row);
				Grid.SetColumn(elements[i], i);
				Panel.SetZIndex(elements[i], zIndex);
			}
		}
		public static void AlignInColumn(IList<UIElement> elements, int col)
		{
			for (int i = 0; i < elements.Count; i++)
			{
				Grid.SetRow(elements[i], i);
				Grid.SetColumn(elements[i], col);
			}
		}
		

		public static void AlignItems(this Grid grid, Size itemSize, Orientation orientation = Orientation.Vertical)
		{
			int itemsCount = grid.Children.Count;
			var children = grid.Children.Cast<UIElement>().ToArray();
			int rowCount, colCount;
			grid.RowDefinitions.Clear();
			grid.ColumnDefinitions.Clear();
			List<List<UIElement>> groupedElements = new List<List<UIElement>>();
			switch (orientation)
			{
				case Orientation.Horizontal:
					rowCount = (int)Math.Floor(grid.ActualHeight / itemSize.Height);
					colCount = (int)Math.Floor((double)children.Count() / rowCount);
					MiscExtensions.Repeat(() => grid.RowDefinitions.Add(new RowDefinition()), rowCount);
					MiscExtensions.Repeat(() => grid.ColumnDefinitions.Add(new ColumnDefinition()), colCount);
					
					for (int i = 0; i < colCount; i++)
					{
						groupedElements.Add(new List<UIElement>());
						for (int j = 0; j < rowCount; j++)
						{
							if (children.Count() <= i * rowCount + j)
								break;
							groupedElements[i].Add(children[i * rowCount + j]);
						}
					}
					groupedElements.For((index, group) => AlignInColumn(group, index));
					break;
				case Orientation.Vertical:
					colCount = (int)Math.Floor(grid.ActualWidth / itemSize.Width);
					rowCount = (int)Math.Floor((double)children.Count() / colCount) + 1;
					MiscExtensions.Repeat(() => grid.RowDefinitions.Add(new RowDefinition()), rowCount);
					MiscExtensions.Repeat(() => grid.ColumnDefinitions.Add(new ColumnDefinition()), colCount);
					int zIndex = rowCount;
					for (int i = 0; i < rowCount; i++)
					{
						groupedElements.Add(new List<UIElement>());
						for (int j = 0; j < colCount; j++)
						{
							if (children.Count() <= i * colCount + j)
								break;
							groupedElements[i].Add(children[i * colCount + j]);
						}
					}
					groupedElements.For((index, group) => AlignInRow(group, index, zIndex--));
					break;
				default: break;
			}
		}
	}
}
