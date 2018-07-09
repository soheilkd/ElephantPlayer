using System;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Player
{
	public class Settings
	{
		private Configuration Config;
		public Settings()
		{
			Config = ConfigurationManager.OpenExeConfiguration(0);
		}

		public void Save()
		{
			Config.Save(0, true);
			ConfigurationManager.RefreshSection("appSettings");
		}
		protected void Set<T>(T value, [CallerMemberName] string propertyName = null)
		{
			Config.AppSettings.Settings[propertyName].Value = value.ToString();
		}
		protected string Get([CallerMemberName] string propertyName = null)
		{
			return Config.AppSettings.Settings[propertyName].Value;
		}
		protected int GetInt([CallerMemberName] string propertyName = null) => Int32.Parse(Get(propertyName));
		protected bool GetBool([CallerMemberName] string propertyName = null) => Get(propertyName) == Boolean.TrueString;
		protected TEnum GetEnum<TEnum>([CallerMemberName] string propertyName = null) 
			where TEnum: Enum => (TEnum)Enum.Parse(typeof(TEnum), Get(propertyName));

		public PlayMode PlayMode { get => GetEnum<PlayMode>(); set => Set(value); }
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

		#region Shortcut Keys
		public Key AncestorKey { get => GetEnum<Key>(); set => Set(value); }
		public Key PreviousKey { get => GetEnum<Key>(); set => Set(value); }
		public Key PrivatePlayPauseKey { get => GetEnum<Key>(); set => Set(value); }
		public Key PublicPlayPauseKey { get => GetEnum<Key>(); set => Set(value); }
		public Key NextKey { get => GetEnum<Key>(); set => Set(value); }
		public Key PlayModeKey { get => GetEnum<Key>(); set => Set(value); }
		public Key VolumeIncreaseKey { get => GetEnum<Key>(); set => Set(value); }
		public Key VolumeDecreaseKey { get => GetEnum<Key>(); set => Set(value); }
		public Key CopyKey { get => GetEnum<Key>(); set => Set(value); }
		public Key MoveKey { get => GetEnum<Key>(); set => Set(value); }
		public Key RemoveKey { get => GetEnum<Key>(); set => Set(value); }
		public Key DeleteKey { get => GetEnum<Key>(); set => Set(value); }
		public Key MediaPlayKey { get => GetEnum<Key>(); set => Set(value); }
		public Key PropertiesKey { get => GetEnum<Key>(); set => Set(value); }
		public Key FindKey { get => GetEnum<Key>(); set => Set(value); }
		public Key BackwardKey { get => GetEnum<Key>(); set => Set(value); }
		public Key ForwardKey { get => GetEnum<Key>(); set => Set(value); }
		#endregion

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

		public static Settings GetDefaults()
		{
			return new Settings
			{
				AncestorKey = Key.LeftShift,
				BackwardKey = Key.Left,
				CopyKey = Key.C,
				DeleteKey = Key.Delete,
				FindKey = Key.F,
				ForwardKey = Key.Right,
				MediaPlayKey = Key.Enter,
				MoveKey = Key.M,
				NextKey = Key.MediaNextTrack,
				PlayModeKey = Key.P,
				PreviousKey = Key.MediaPreviousTrack,
				PrivatePlayPauseKey = Key.Space,
				PublicPlayPauseKey = Key.MediaPlayPause,
				RemoveKey = Key.R,
				VolumeDecreaseKey = Key.Down,
				VolumeIncreaseKey = Key.Up,
				ExplicitContent = true,
				LastLoc = new Point(0, 0),
				LastPath = @"D:\",
				LastSize = new Size(600, 700),
				LiveLibrary = true,
				MouseTimeoutIndex = 2,
				PlayMode = PlayMode.Repeat,
				PlayOnPositionChange = true,
				RememberMinimal = true,
				RevalidateOnExit = false,
				VisionOrientation = true,
				Volume = 0.99,
				WasMinimal = false
			};
		}
	}
}