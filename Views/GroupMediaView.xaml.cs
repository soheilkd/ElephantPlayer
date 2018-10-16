using System;
using System.Windows;
using System.Windows.Controls;
using Player.Extensions;
using Player.Models;

namespace Player.Views
{
	public partial class GroupMediaView : Grid
	{
		public GroupMediaView(MediaQueue queue, Action<MediaQueue, Media> onPlay)
		{
			InitializeComponent();
			MediaDataGrid.ItemsSource = queue;
			MediaDataGrid.MediaRequested += (_, e) => onPlay(e.Parameter.Item1, e.Parameter.Item2);
		}

		public GroupMediaView()
		{
			InitializeComponent();
		}

		private void Grid_Loaded(object sender, RoutedEventArgs e)
		{
			TimeSpan totalLength = new TimeSpan(0);
			MediaDataGrid.ItemsSource.For(each => totalLength += each.Length);
			BarTextBlock.Text = $"Showing {MediaDataGrid.ItemsSource.Count} media[s], totally {(int)totalLength.TotalMinutes} minutes";
		}
	}
}
