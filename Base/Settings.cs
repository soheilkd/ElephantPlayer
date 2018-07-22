using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;

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
		public string LibraryLocation { get; set; } = "@Library.bin";
		
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
		public Point LastLocation { get; set; }
	}
}