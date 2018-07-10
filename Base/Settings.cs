using System;
using System.Configuration;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Input;

namespace Player
{
	[Serializable]
	public class Settings
	{
		private static readonly BinaryFormatter DefaultFormatter = new BinaryFormatter();

		public void Save()
		{
			using (var stream = new FileStream($"{App.Path}Settings.bin", FileMode.Create))
				DefaultFormatter.Serialize(stream, this);
		}
		public static Settings Load()
		{
			using (var stream = new FileStream($"{App.Path}Settings.bin", FileMode.Open))
				return (Settings)DefaultFormatter.Deserialize(stream);
		}

		public PlayMode PlayMode { get; set; }
		public double Volume { get; set; }
		public bool VisionOrientation { get; set; }
		public string LastPath { get; set; }
		public bool LiveLibrary { get; set; }
		public bool ExplicitContent { get; set; }
		public bool PlayOnPositionChange { get; set; }
		public bool RevalidateOnExit { get; set; }
		public bool RememberMinimal { get; set; }
		public bool WasMinimal { get; set; }
		public int MouseTimeoutIndex { get; set; }
		
		public Key AncestorKey { get; set; }
		public Key PreviousKey { get; set; }
		public Key PrivatePlayPauseKey { get; set; }
		public Key PublicPlayPauseKey { get; set; }
		public Key NextKey { get; set; }
		public Key PlayModeKey { get; set; }
		public Key VolumeIncreaseKey { get; set; }
		public Key VolumeDecreaseKey { get; set; }
		public Key CopyKey { get; set; }
		public Key MoveKey { get; set; }
		public Key RemoveKey { get; set; }
		public Key DeleteKey { get; set; }
		public Key MediaPlayKey { get; set; }
		public Key PropertiesKey { get; set; }
		public Key FindKey { get; set; }
		public Key BackwardKey { get; set; }
		public Key ForwardKey { get; set; }

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
		public Size LastSize { get; set; }
		public Point LastLoc { get; set; }

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