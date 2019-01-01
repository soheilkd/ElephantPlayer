using System.Windows;

namespace Player.Windows
{
	public partial class NewPlaylistWindow : Window
	{
		private bool EnteredName = false;
		public NewPlaylistWindow()
		{
			InitializeComponent();
		}

		public static string RequestName()
		{
			var window = new NewPlaylistWindow();
			window.ShowDialog();
			return window.EnteredName ? window.NameBox.Text : string.Empty;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(NameBox.Text))
				MessageBox.Show("Cannot be empty");
			else
			{
				EnteredName = true;
				Close();
			}
		}
	}
}
