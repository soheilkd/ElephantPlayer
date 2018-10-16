using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Player.Controls.Navigation;
using Player.Extensions;
using Player.Models;

namespace Player.Views
{
	/// <summary>
	/// Interaction logic for AlbumsView.xaml
	/// </summary>
	public partial class AlbumsView : Grid
	{
		public event EventHandler<InfoExchangeArgs<(MediaQueue, Media)>> PlayRequested;
		private int CallTime = -1; //It's used for Lazy Loading, reaches 1 when user enters AlbumsView tab on MainWindow
		public AlbumsView()
		{
			InitializeComponent();
		}

		private void Grid_Loaded(object sender, RoutedEventArgs e)
		{
			if (CallTime++ != 0)
				return;
			var albums = Controllers.LibraryController.LoadedCollection.GroupBy(each => each.Album).OrderBy(each => each.Key);
			var grid = AlbumNavigation.GetChildContent(1) as Grid;
			albums.ForEach(each =>
				grid.Children.Add(
					new NavigationTile()
					{
						Tag = each.Key,
						TileStyle = Controls.TileStyle.Default,
						Navigation = new NavigationControl()
						{
							Tag = each.Key,
							Content = new GroupMediaView(new MediaQueue(each),
							onPlay: (queue, media) => PlayRequested?.Invoke(this, new InfoExchangeArgs<(MediaQueue, Media)>((queue, media))))
						}
					}));
			grid.AlignItems(Controls.Tile.StandardSize);
			grid.SizeChanged += (_, __) => grid.AlignItems(Controls.Tile.StandardSize);
		}
	}
}
