using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Library.Extensions;
using Player.Models;

namespace Player.Views
{
	public partial class AlbumView : Grid
	{
        private string AlbumName;

        public AlbumView(string album) : this()
        {
            MediaDataGrid.ItemsSource = new MediaQueue(Controller.Library.Where(each => each.Album == album));
            AlbumName = album;
        }

        public AlbumView()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var totalLength = new TimeSpan(0);
            MediaDataGrid.ItemsSource.For(each => totalLength += each.Length);
            Controller.Library.CollectionChanged += Library_CollectionChanged;
        }

        private void Library_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            MediaDataGrid.ItemsSource = new MediaQueue(Controller.Library.Where(each => each.Album == AlbumName));
        }
    }
}
