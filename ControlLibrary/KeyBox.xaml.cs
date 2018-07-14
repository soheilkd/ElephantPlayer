using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Player.Controls
{
	public partial class KeyBox : UserControl
	{
		public event EventHandler<KeyEventArgs> SelectedKeyChanged;

		private Key _selectedKey;
		public Key SelectedKey
		{
			get => _selectedKey;
			set
			{
				_selectedKey = value;
				SelectedKeyChanged?.Invoke(this, new KeyEventArgs(null, null, 0, value));
			}
		}

		public KeyBox() => InitializeComponent();
		
		private void TextBox_KeyUp(object sender, KeyEventArgs e)
		{
			SelectedKey = e.Key;
			MainBox.Text = SelectedKey.ToString();
		}

		private void TextBox_GotFocus(object sender, RoutedEventArgs e) => MainBox.Text = "Waiting for a key...";
		private void MainBox_LostFocus(object sender, RoutedEventArgs e) => MainBox.Text = SelectedKey.ToString();

		private void UserControl_Loaded(object sender, RoutedEventArgs e) => MainBox.Text = SelectedKey.ToString();
	}
}
