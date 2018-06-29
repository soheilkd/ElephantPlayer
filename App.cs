using Player.DeepBackEnd.InstanceManagement;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Player
{
	public partial class App : Application, ISingleInstanceApp
	{
		public static event EventHandler<InstanceEventArgs> NewInstanceRequested;

		//It's here for those classes need app's path for working, Preferences, Library, etc. (future)
		public static readonly string Path =
			Environment.GetCommandLineArgs()[0].Substring(0, Environment.GetCommandLineArgs()[0].LastIndexOf("\\") + 1);

		public static Settings Settings { get; } = new Settings();
		[STAThread]
		public static void Main()
		{
			
			AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				MessageBox.Show($"Unhandled {e.ExceptionObject}\r\n" +
					$"Terminating: {e.IsTerminating}", "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
			};
			if (Instance<App>.InitializeAsFirstInstance("ElephantIPC_soheilkd"))
			{
				var application = new App();
				application.InitializeComponent();
				application.Run();
				Instance<App>.Cleanup();
			}
		}

		public bool SignalExternalCommandLineArgs(IList<string> args)
		{
			args.Remove(Environment.GetCommandLineArgs()[0]);
			NewInstanceRequested?.Invoke(this, new InstanceEventArgs(args));
			return true;
		}
	}
}
