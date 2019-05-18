using System.Windows;
using System.Windows.Controls;

namespace Player.Views
{
	public partial class AlbumsView : ContentControl
	{
		private ItemSubcontent _CurrentContent = new ItemSubcontent()
		{
			MediaLoader = key => Controller.Library.Albums[key],
			ImageLoader = key => Web.GetAlbumImage(key)
		};

		public AlbumsView() => InitializeComponent();

		private void Grid_Loaded(object sender, RoutedEventArgs e)
		{
			ViewerOperator.Initialize(MainGrid, Controller.Library.Albums.Keys, _CurrentContent);
		}
	}
}
