using System;
using System.Collections.Generic;
using System.Windows;
using Library.Instances;
using Library.Serialization.Models;

namespace Player
{
	public partial class App : Application, ISingleInstanceApp
	{
		public static event EventHandler<InstanceEventArgs> NewInstanceRequested;
		public static LazySerializable<Dictionary<string, byte[]>> Resource { get; set; }

		[STAThread]
		public static void Main()
		{
			Settings.AppPath = Environment.GetCommandLineArgs()[0].Substring(0, Environment.GetCommandLineArgs()[0].LastIndexOf("\\") + 1);
			Resource = new LazySerializable<Dictionary<string, byte[]>>($"{Settings.AppPath}Resources.rsc");
			AppDomain.CurrentDomain.ProcessExit += (_, __) => Library.Hook.Events.Dispose();
			AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				Console.WriteLine($"Unhandled {e.ExceptionObject}\r\n", "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
				MessageBox.Show($"Unhandled error occurred", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
