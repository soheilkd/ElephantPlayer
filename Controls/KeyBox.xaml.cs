using Player.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Player.Controls
{
	/// <summary>
	/// Interaction logic for KeyBox.xaml
	/// </summary>
	public partial class KeyBox : UserControl
	{
		public event EventHandler<InfoExchangeArgs<Key>> SelectedKeyChanged;
		public event EventHandler<InfoExchangeArgs<bool>> IsGlobalChanged;

		private Key _selectedKey;
		public Key SelectedKey
		{
			get => _selectedKey;
			set
			{
				_selectedKey = value;
				SelectedKeyChanged?.Invoke(this, new InfoExchangeArgs<Key>(value));
			}
		}

		private bool _isGlobalChecked;
		public bool IsGlobalChecked
		{
			get => _isGlobalChecked;
			set
			{
				_isGlobalChecked = value;
				IsGlobalChanged?.Invoke(this, new InfoExchangeArgs<bool>(value));
			}
		}

		public KeyBox()
		{
			InitializeComponent();
		}

		private void TextBox_GotFocus(object sender, RoutedEventArgs e)
		{
			MainBox.Text = "Waiting for a key...";
		}

		private void TextBox_KeyUp(object sender, KeyEventArgs e)
		{
			SelectedKey = e.Key;
			MainBox.Text = SelectedKey.ToString();
		}

		private void MainBox_LostFocus(object sender, RoutedEventArgs e)
		{
			MainBox.Text = SelectedKey.ToString();
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			MainBox.Text = SelectedKey.ToString();
		}

		private void GlobalCheckBox_Checked(object sender, RoutedEventArgs e)
		{

		}

		private void GlobalCheckBox_Unchecked(object sender, RoutedEventArgs e)
		{

		}
	}
}
