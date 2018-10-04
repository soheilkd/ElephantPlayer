using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Player.Controls.Navigation
{
	public partial class NavigationControl : ContentControl
	{
		public event EventHandler BackClicked;

		public NavigationControl() =>
			InitializeComponent();

		private async void Button_MouseUp(object sender, MouseButtonEventArgs e)
		{
			BeginStoryboard(Resources["BackStoryboard"] as Storyboard);
			await Task.Delay(850);
			BackClicked?.Invoke(this, e);
		}

		public void EmulateBack() => Button_MouseUp(null, null);

		public void BeginOpenStoryboard() =>
			BeginStoryboard(Resources["OpenStoryboard"] as Storyboard);
	}
}
