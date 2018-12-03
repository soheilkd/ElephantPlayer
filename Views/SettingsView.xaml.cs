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
			OrinateCheck.IsChecked = Settings.VisionOrientation;
			LiveLibraryCheck.IsChecked = Settings.LiveLibrary;
			ExplicitCheck.IsChecked = Settings.ExplicitContent;
			PlayOnPosCheck.IsChecked = Settings.PlayOnPositionChange;
			RevalidOnExitCheck.IsChecked = Settings.RevalidateOnExit;
			TimeoutCombo.SelectedIndex = Settings.MouseTimeoutIndex;
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

			TimeoutCombo.SelectionChanged += (_, __) => Settings.MouseTimeoutIndex = TimeoutCombo.SelectedIndex;
			OrinateCheck.Checked += (_, __) => Settings.VisionOrientation = true;
			OrinateCheck.Unchecked += (_, __) => Settings.VisionOrientation = false;
			LiveLibraryCheck.Checked += (_, __) => Settings.LiveLibrary = true;
			LiveLibraryCheck.Unchecked += (_, __) => Settings.LiveLibrary = false;
			ExplicitCheck.Checked += (_, __) => Settings.ExplicitContent = true;
			ExplicitCheck.Unchecked += (_, __) => Settings.ExplicitContent = false;
			PlayOnPosCheck.Checked += (_, __) => Settings.PlayOnPositionChange = true;
			PlayOnPosCheck.Unchecked += (_, __) => Settings.PlayOnPositionChange = false;
			RevalidOnExitCheck.Checked += (_, __) => Settings.RevalidateOnExit = true;
			RevalidOnExitCheck.Unchecked += (_, __) => Settings.RevalidateOnExit = false;
			RememberMinimalCheck.Checked += (_, __) => Settings.RememberMinimal = true;
			RememberMinimalCheck.Unchecked += (_, __) => Settings.RememberMinimal = false;
		}
	}
}
