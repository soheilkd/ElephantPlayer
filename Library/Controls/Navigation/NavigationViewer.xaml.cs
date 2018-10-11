using System.Windows;
using System.Windows.Controls;

namespace Player.Controls.Navigation
{
	public partial class NavigationViewer : ContentControl
	{
		private NavigationControl _Nav;
		private ContentPresenter Presenter;

		public NavigationViewer()
		{
			InitializeComponent();
		}

		private void ContentControl_Loaded(object sender, RoutedEventArgs e)
		{
			_Nav = (NavigationControl)Template.FindName("NavigationContent", this);
			Presenter = (ContentPresenter)Template.FindName("Presenter", this);
		}

		public void ReturnToMainView()
		{
			Presenter.Visibility = Visibility.Visible;
			_Nav.Content = null;
		}
		public void OpenView(NavigationControl view)
		{
			Presenter.Visibility = Visibility.Hidden;
			_Nav.Content = view.Content;
			_Nav.Tag = view.Tag;
			_Nav.BeginOpenStoryboard();
			_Nav.BackClicked += (_, __) => ReturnToMainView();
		}

		public object GetChildContent(int dim = 0)
		{
			ContentControl ret = Content as ContentControl;
			object rett = null;
			for (int i = 0; ;)
			{
				if (++i < dim - 1)
					ret = ret.Content as ContentControl;
				else
				{
					rett = ret.Content;
					break;
				}
			}
			return rett;
		}
	}
}
