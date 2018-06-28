using System;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Windows;
namespace Player
{
	public class Settings
	{
		private Configuration Config;
		public Settings()
		{
			Config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
		}

		protected void Set<T>(T value, [CallerMemberName] string propertyName = null)
		{
			Config.AppSettings.Settings[propertyName].Value = value.ToString();
			Config.Save(ConfigurationSaveMode.Modified, true);
			ConfigurationManager.RefreshSection("appSettings");
		}
		protected string Get([CallerMemberName] string propertyName = null)
		{
			return Config.AppSettings.Settings[propertyName].Value;
		}
		protected int GetInt([CallerMemberName] string propertyName = null) => Int32.Parse(Get(propertyName));
		protected bool GetBool([CallerMemberName] string propertyName = null) => Get(propertyName) == Boolean.TrueString;

		public PlayMode PlayMode { get => (PlayMode)GetInt(); set => Set((int)value); }
		public int MainKey { get => GetInt(); set => Set(value); }
		public double Volume { get => Double.Parse(Get()); set => Set(value); }
		public bool VisionOrientation { get => GetBool(); set => Set(value); }
		public string LastPath { get => Get(); set => Set(value); }
		public bool LiveLibrary { get => GetBool(); set => Set(value); }
		public bool ExplicitContent { get => GetBool(); set => Set(value); }
		public bool PlayOnPositionChange { get => GetBool(); set => Set(value); }
		public bool RevalidateOnExit { get => GetBool(); set => Set(value); }
		public bool RememberMinimal { get => GetBool(); set => Set(value); }
		public bool WasMinimal { get => GetBool(); set => Set(value); }
		public int MouseTimeoutIndex { get => GetInt(); set => Set(value); }
		public int MouseOverTimeout
		{
			get
			{
				switch (MouseTimeoutIndex)
				{
					case 0: return 500;
					case 1: return 1000;
					case 2: return 2000;
					case 3: return 3000;
					case 4: return 4000;
					case 5: return 5000;
					case 6: return 10000;
					case 7: return 60000;
					default: return 2000;
				}
			}
		}
		public Size LastSize
		{
			get
			{
				var b = Get().Split(',');
				return new Size(Double.Parse(b[0]), Double.Parse(b[1]));
			}
			set => Set($"{value.Width},{value.Height}");
		}
		public Point LastLoc
		{
			get
			{
				var b = Get().Split(',');
				return new Point(Double.Parse(b[0]), Double.Parse(b[1]));
			}
			set => Set($"{value.X},{value.Y}");
		}
	}
}