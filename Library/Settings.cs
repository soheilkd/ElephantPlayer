using System;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace Player
{
	public static class Settings
	{
		public static string AppPath { get; set; }

		//Default XmlSerializer for boxing/unboxing settings object to Xml stream/file
		private static readonly XmlSerializer DefaultSerializer = new XmlSerializer(typeof(SerializableSettings));

		#region Lazy Loading
		//Lazy loading will help to assign AppPath first, then intialize Current with Load()
		private static Lazy<SerializableSettings> LazySettings = new Lazy<SerializableSettings>(Load);
		private static SerializableSettings Current => LazySettings.Value;
		#endregion
		
		public static void Save()
		{
			using (FileStream stream = new FileStream($"{AppPath}Settings.xml", FileMode.Create))
				DefaultSerializer.Serialize(stream, Current);
		}
		private static SerializableSettings Load()
		{
			using (FileStream stream = new FileStream($"{AppPath}Settings.xml", FileMode.Open))
				return (SerializableSettings)DefaultSerializer.Deserialize(stream);
		}

		public static PlayMode PlayMode { get => Current.PlayMode; set => Current.PlayMode = value; }
		public static double Volume { get => Current.Volume; set => Current.Volume = value; }
		public static bool VisionOrientation { get => Current.VisionOrientation; set => Current.VisionOrientation = value; }
		public static string LastPath { get => Current.LastPath; set => Current.LastPath = value; }
		public static bool LiveLibrary { get => Current.LiveLibrary; set => Current.LiveLibrary = value; }
		public static bool ExplicitContent { get => Current.ExplicitContent; set => Current.ExplicitContent = value; }
		public static bool PlayOnPositionChange { get => Current.PlayOnPositionChange; set => Current.PlayOnPositionChange = value; }
		public static bool RevalidateOnExit { get => Current.RevalidateOnExit; set => Current.RevalidateOnExit = value; }
		public static bool RememberMinimal { get => Current.RememberMinimal; set => Current.RememberMinimal = value; }
		public static bool WasMinimal { get => Current.WasMinimal; set => Current.WasMinimal = value; }
		public static int MouseTimeoutIndex { get => Current.MouseTimeoutIndex; set => Current.MouseTimeoutIndex = value; }
		public static string LibraryLocation { get => Current.LibraryLocation; set => Current.LibraryLocation = value; }
		public static Size LastSize { get => Current.LastSize; set => Current.LastSize = value; }
		public static Point LastLocation { get => Current.LastLocation; set => Current.LastLocation = value; }
		
		public static int MouseOverTimeout
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
	}
}