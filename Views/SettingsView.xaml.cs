using System.Windows;
using System.Windows.Controls;
using static Player.Controller;

namespace Player.Views
{
	public partial class SettingsView : ContentControl
	{
		public SettingsView()
		{
			InitializeComponent();
		}
		private void PlayModeChanged(object sender, RoutedEventArgs e)
		{
			Settings.PlayMode = (PlayMode)int.Parse(((RadioButton)sender).Tag.ToString());
		}

		private void StackPanel_Loaded(object sender, RoutedEventArgs e)
		{
			PlayOnPosCheck.IsChecked = Settings.PlayOnPositionChange;
			TimeoutSlider.Value = Settings.MouseTimeout;

			switch (Settings.PlayMode)
			{
				case PlayMode.Repeat:
					PlayModeRadio1.IsChecked = true;
					break;
				case PlayMode.RepeatOne:
					PlayModeRadio2.IsChecked = true;
					break;
				case PlayMode.Shuffle:
					PlayModeRadio3.IsChecked = true;
					break;
				default:
					break;
			}
			
			TimeoutSlider.ValueChanged += (_, __) => Settings.MouseTimeout = TimeoutSlider.Value;
			PlayOnPosCheck.Checked += (_, __) => Settings.PlayOnPositionChange = true;
			PlayOnPosCheck.Unchecked += (_, __) => Settings.PlayOnPositionChange = false;
		}
	}
}
