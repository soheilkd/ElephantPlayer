using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Library.Controls;
using Library.Controls.Navigation;
using Library.Extensions;

namespace Player.Views
{
    public partial class PlaylistsView : ContentControl
    {
        public PlaylistsView()
        {
            InitializeComponent();

			var grid = PlaylistNavigation.GetChildContent(1) as Grid;
			var navigations = new List<NavigationTile>();
			Controller.Playlists.ForEach(each =>
			{
				navigations.Add(
					new NavigationTile()
					{
						Tag = each.Name,
						TileStyle = TileStyle.Default,
						Navigation = new NavigationControl()
						{
							Tag = each.Name,
							Content = new PlaylistView(each)
						}
					});
			});
			navigations.ForEach(each => grid.Children.Add(each));
			grid.AlignChildrenVertical(new Size(50, 100));
			grid.SizeChanged +=  (_, __) => grid.AlignChildrenVertical(Tile.StandardSize);
		}
	}
}
