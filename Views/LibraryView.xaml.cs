using System.Windows.Controls;

namespace Player.Views
{
	public partial class LibraryView : ContentControl
	{
		public LibraryView()
		{
			InitializeComponent();
			DataGrid.Items = Controller.Library;
		}
	}
}
