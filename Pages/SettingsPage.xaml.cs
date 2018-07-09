using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Player
{
	/// <summary>
	/// Interaction logic for SettingsPage.xaml
	/// </summary>
	public partial class SettingsPanel : ScrollViewer
	{
		public SettingsPanel()
		{
			InitializeComponent();
			
			OrinateCheck.IsChecked = App.Settings.VisionOrientation;
			LiveLibraryCheck.IsChecked = App.Settings.LiveLibrary;
			ExplicitCheck.IsChecked = App.Settings.ExplicitContent;
			PlayOnPosCheck.IsChecked = App.Settings.PlayOnPositionChange;
			RevalidOnExitCheck.IsChecked = App.Settings.RevalidateOnExit;
			TimeoutCombo.SelectedIndex = App.Settings.MouseTimeoutIndex;

			Keys_AncestorBox.SelectedKey = App.Settings.AncestorKey;
			Keys_BackwardBox.SelectedKey = App.Settings.BackwardKey;
			Keys_CopyBox.SelectedKey = App.Settings.CopyKey;
			Keys_DeleteBox.SelectedKey = App.Settings.DeleteKey;
			Keys_FindBox.SelectedKey = App.Settings.FindKey;
			Keys_ForwardBox.SelectedKey = App.Settings.ForwardKey;
			Keys_MediaPlayBox.SelectedKey = App.Settings.MediaPlayKey;
			Keys_MoveBox.SelectedKey = App.Settings.MoveKey;
			Keys_NextBox.SelectedKey = App.Settings.NextKey;
			Keys_PlayModeBox.SelectedKey = App.Settings.PlayModeKey;
			Keys_PreviousBox.SelectedKey = App.Settings.PreviousKey;
			Keys_PrivatePlayPauseBox.SelectedKey = App.Settings.PrivatePlayPauseKey;
			Keys_PropertiesBox.SelectedKey = App.Settings.PropertiesKey;
			Keys_PublicPlayPauseBox.SelectedKey = App.Settings.PublicPlayPauseKey;
			Keys_RemoveBox.SelectedKey = App.Settings.RemoveKey;
			Keys_VolumeDecreaseBox.SelectedKey = App.Settings.VolumeDecreaseKey;
			Keys_VolumeIncreaseBox.SelectedKey = App.Settings.VolumeIncreaseKey;

			TimeoutCombo.SelectionChanged += (_, __) => App.Settings.MouseTimeoutIndex = TimeoutCombo.SelectedIndex;
			OrinateCheck.Checked += (_, __) => App.Settings.VisionOrientation = true;
			OrinateCheck.Unchecked += (_, __) => App.Settings.VisionOrientation = false;
			LiveLibraryCheck.Checked += (_, __) => App.Settings.LiveLibrary = true;
			LiveLibraryCheck.Unchecked += (_, __) => App.Settings.LiveLibrary = false;
			ExplicitCheck.Checked += (_, __) => App.Settings.ExplicitContent = true;
			ExplicitCheck.Unchecked += (_, __) => App.Settings.ExplicitContent = false;
			PlayOnPosCheck.Checked += (_, __) => App.Settings.PlayOnPositionChange = true;
			PlayOnPosCheck.Unchecked += (_, __) => App.Settings.PlayOnPositionChange = false;
			RevalidOnExitCheck.Checked += (_, __) => App.Settings.RevalidateOnExit = true;
			RevalidOnExitCheck.Unchecked += (_, __) => App.Settings.RevalidateOnExit = false;
			RememberMinimalCheck.Checked += (_, __) => App.Settings.RememberMinimal = true;
			RememberMinimalCheck.Unchecked += (_, __) => App.Settings.RememberMinimal = false;
			
			Keys_AncestorBox.SelectedKeyChanged += (_, e) =>
			{
				App.Settings.AncestorKey = e.Parameter;
			};
			Keys_BackwardBox.SelectedKeyChanged += (_, e) => App.Settings.BackwardKey = e.Parameter;
			Keys_CopyBox.SelectedKeyChanged += (_, e) => App.Settings.CopyKey = e.Parameter;
			Keys_DeleteBox.SelectedKeyChanged += (_, e) => App.Settings.DeleteKey = e.Parameter;
			Keys_FindBox.SelectedKeyChanged += (_, e) => App.Settings.FindKey = e.Parameter;
			Keys_ForwardBox.SelectedKeyChanged += (_, e) => App.Settings.ForwardKey = e.Parameter;
			Keys_MediaPlayBox.SelectedKeyChanged += (_, e) => App.Settings.MediaPlayKey = e.Parameter;
			Keys_MoveBox.SelectedKeyChanged += (_, e) => App.Settings.MoveKey = e.Parameter;
			Keys_NextBox.SelectedKeyChanged += (_, e) => App.Settings.NextKey = e.Parameter;
			Keys_PlayModeBox.SelectedKeyChanged += (_, e) => App.Settings.PlayModeKey = e.Parameter;
			Keys_PreviousBox.SelectedKeyChanged += (_, e) => App.Settings.PreviousKey = e.Parameter;
			Keys_PrivatePlayPauseBox.SelectedKeyChanged += (_, e) => App.Settings.PrivatePlayPauseKey = e.Parameter;
			Keys_PropertiesBox.SelectedKeyChanged += (_, e) => App.Settings.PropertiesKey = e.Parameter;
			Keys_PublicPlayPauseBox.SelectedKeyChanged += (_, e) => App.Settings.PublicPlayPauseKey = e.Parameter;
			Keys_RemoveBox.SelectedKeyChanged += (_, e) => App.Settings.RemoveKey = e.Parameter;
			Keys_VolumeDecreaseBox.SelectedKeyChanged += (_, e) => App.Settings.VolumeDecreaseKey = e.Parameter;
			Keys_VolumeIncreaseBox.SelectedKeyChanged += (_, e) => App.Settings.VolumeIncreaseKey = e.Parameter;
		}

		private async void RevalidateClick(object sender, RoutedEventArgs e)
		{
			sender.As<Button>().Content = "Revalidating... app may stop working";
			IsEnabled = false;
			await Task.Delay(2000);
			new MediaManager().Revalidate();
			Application.Current.Shutdown(-1);
			Process.Start(App.Path + "Elephant Player.exe");
		}
	}
}
