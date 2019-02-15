using System.Windows.Controls;

namespace Player.Views
{
	public partial class AlbumView : Grid
	{
		public AlbumView() => InitializeComponent();
		public AlbumView(string album) : this() => MediaDataGrid.ItemsSource = Controller.Library.Albums[album];
	}
}
