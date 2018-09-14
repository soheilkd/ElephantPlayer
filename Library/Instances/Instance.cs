using Player.Native;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Player.Instances
{
	public static class Instance<TApplication> where TApplication : Application, ISingleInstanceApp
	{
		private const string Delimiter = ":";
		private const string ChannelNameSuffix = "SingeInstanceIPCChannel";
		private const string RemoteServiceName = "SingleInstanceApplicationService";
		private const string IpcProtocol = "ipc://";
		private static Mutex singleInstanceMutex;
		private static IpcServerChannel channel;
		public static IList<string> CommandLineArgs { get; private set; }

		public static bool InitializeAsFirstInstance(string uniqueName)
		{
			CommandLineArgs = GetCommandLineArgs(uniqueName);
			string applicationIdentifier = uniqueName + Environment.UserName;
			string channelName = string.Concat(applicationIdentifier, Delimiter, ChannelNameSuffix);
			singleInstanceMutex = new Mutex(true, applicationIdentifier, out bool firstInstance);
			if (firstInstance)
				CreateRemoteService(channelName);
			else
				SignalFirstInstance(channelName, CommandLineArgs);
			return firstInstance;
		}
		public static void Cleanup()
		{
			if (singleInstanceMutex != null)
			{
				singleInstanceMutex.Close();
				singleInstanceMutex = null;
			}
			if (channel != null)
			{
				ChannelServices.UnregisterChannel(channel);
				channel = null;
			}
		}
		private static IList<string> GetCommandLineArgs(string uniqueApplicationName)
		{
			string[] args = null;
			if (AppDomain.CurrentDomain.ActivationContext == null)
				args = Environment.GetCommandLineArgs();
			else
			{
				string appFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), uniqueApplicationName);
				string cmdLinePath = Path.Combine(appFolderPath, "cmdline.txt");
				if (File.Exists(cmdLinePath))
				{
					try
					{
						using (TextReader reader = new StreamReader(cmdLinePath, System.Text.Encoding.Unicode))
							args = Methods.CommandLineToArgvW(reader.ReadToEnd());
						File.Delete(cmdLinePath);
					}
					catch (IOException) { }
				}
			}
			if (args == null)
				args = new string[] { };

			return new List<string>(args);
		}
		private static void CreateRemoteService(string channelName)
		{
			BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider
			{
				TypeFilterLevel = TypeFilterLevel.Full
			};
			IDictionary props = new Dictionary<string, string>
			{
				["name"] = channelName,
				["portName"] = channelName,
				["exclusiveAddressUse"] = "false"
			};
			channel = new IpcServerChannel(props, serverProvider);
			ChannelServices.RegisterChannel(channel, true);
			IPCRemoteService remoteService = new IPCRemoteService();
			RemotingServices.Marshal(remoteService, RemoteServiceName);
		}
		private static void SignalFirstInstance(string channelName, IList<string> args)
		{
			IpcClientChannel secondInstanceChannel = new IpcClientChannel();
			ChannelServices.RegisterChannel(secondInstanceChannel, true);
			string remotingServiceUrl = IpcProtocol + channelName + "/" + RemoteServiceName;
			IPCRemoteService firstInstanceRemoteServiceReference = (IPCRemoteService)RemotingServices.Connect(typeof(IPCRemoteService), remotingServiceUrl);
			firstInstanceRemoteServiceReference?.InvokeFirstInstance(args);
		}
		private static object ActivateFirstInstanceCallback(object arg)
		{
			IList<string> args = arg as IList<string>;
			ActivateFirstInstance(args);
			return null;
		}
		private static void ActivateFirstInstance(IList<string> args)
		{
			if (Application.Current == null) return;
			((TApplication)Application.Current).SignalExternalCommandLineArgs(args);
		}
		private class IPCRemoteService : MarshalByRefObject
		{
			public void InvokeFirstInstance(IList<string> args)
			{
				if (Application.Current != null)
					Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(ActivateFirstInstanceCallback), args);
			}
			public override object InitializeLifetimeService() => null;
		}
	}
}