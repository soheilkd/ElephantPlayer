using Player.DeepBackEnd.InstanceManagement;
using System;
using System.Collections.Generic;
using System.Windows;
using Player.Hook;

namespace Player
{
	public partial class App : Application, ISingleInstanceApp
	{
		public static event EventHandler<InstanceEventArgs> NewInstanceRequested;

		private static KeyboardListener Listener = KeyboardListener.Create();

		public static event EventHandler<RawKeyEventArgs> KeyDown
		{
			add => Listener.KeyDown += value;
			remove => Listener.KeyDown -= value;
		}
		public static event EventHandler<RawKeyEventArgs> KeyUp
		{
			add => Listener.KeyUp += value;
			remove => Listener.KeyUp -= value;
		}

		//It's here for those classes need app's path for working, Preferences, Library, etc. (future)
		public static readonly string Path =
			Environment.GetCommandLineArgs()[0].Substring(0, Environment.GetCommandLineArgs()[0].LastIndexOf("\\") + 1);

		public static Settings Settings { get; } = Settings.Load();
		[STAThread]
		public static void Main()
		{
			Settings.LibraryLocation = $"{App.Path}\\Library.bin";
			Settings.Save();
			AppDomain.CurrentDomain.ProcessExit += (_, __) => Listener.Dispose();
			AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				MessageBox.Show($"Unhandled {e.ExceptionObject}\r\n", "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
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
