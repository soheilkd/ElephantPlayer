using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Player
{
	/// <summary>
	/// Interaction logic for SettingsPage.xaml
	/// </summary>
	public partial class SettingsPanel : StackPanel
	{
		public SettingsPanel()
		{
			InitializeComponent();

			AncestorCombo.SelectedIndex = App.Settings.MainKey;
			OrinateCheck.IsChecked = App.Settings.VisionOrientation;
			LiveLibraryCheck.IsChecked = App.Settings.LiveLibrary;
			ExplicitCheck.IsChecked = App.Settings.ExplicitContent;
			PlayOnPosCheck.IsChecked = App.Settings.PlayOnPositionChange;
			RevalidOnExitCheck.IsChecked = App.Settings.RevalidateOnExit;
			TimeoutCombo.SelectedIndex = App.Settings.MouseOverTimeoutIndex;
			AncestorCombo.SelectionChanged += (_, __) => App.Settings.MainKey = AncestorCombo.SelectedIndex;
			TimeoutCombo.SelectionChanged += (_, __) => App.Settings.MouseOverTimeoutIndex = TimeoutCombo.SelectedIndex;
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
