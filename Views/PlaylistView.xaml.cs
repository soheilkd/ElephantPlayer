using System;
using System.Windows;
using System.Windows.Controls;
using Library.Extensions;
using Player.Models;

namespace Player.Views
{
	public partial class PlaylistView : ContentControl
	{
		public PlaylistView(Playlist playlist)
		{
			InitializeComponent();
			MediaDataGrid.ItemsSource = new MediaQueue(playlist);
		}

		public PlaylistView()
		{
			InitializeComponent();
		}

		private void Grid_Loaded(object sender, RoutedEventArgs e)
		{
			var totalLength = new TimeSpan(0);
			MediaDataGrid.ItemsSource.For(each => totalLength += each.Length);
			BarTextBlock.Text = $"Showing {MediaDataGrid.ItemsSource.Count} media[s], totally {(int)totalLength.TotalMinutes} minutes";
		}
	}
}
