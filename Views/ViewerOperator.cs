using Library.Extensions;
using Library.Serialization.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Player.Views
{
	public static class ViewerOperator
	{
		public static readonly Size TileSize = new Size(100, 130);

		public static void AddTiles(Grid grid, Type tileType, IEnumerable<string> args)
		{
			args.ForEach(each => grid.Children.Add(GetTile(tileType, each)));
			grid.SizeChanged += (_, __) => grid.AlignChildrenVertical(TileSize);
			grid.AlignChildrenVertical(TileSize);
		}

		public static UserControl GetTile(Type tileType, string arg)
		{
			return Activator.CreateInstance(tileType, arg) as UserControl;
		}
	}
}