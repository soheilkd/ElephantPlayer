﻿using System;
using System.Windows;
using System.Windows.Controls;
using Library.Extensions;
using Player.Models;

namespace Player.Views
{
	public partial class AlbumView : Grid
	{
		public AlbumView(MediaQueue queue)
		{
			InitializeComponent();
			MediaDataGrid.ItemsSource = queue;
		}

		public AlbumView()
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