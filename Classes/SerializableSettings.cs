using System;
using System.Windows;
using System.Xml.Serialization;

namespace Player
{
	[Serializable]
	[XmlRoot(ElementName ="Settings")]
	public class SerializableSettings
	{
		public PlayMode PlayMode { get; set; }
		public double Volume { get; set; }
		public bool VisionOrientation{ get; set; }
		public string LastPath{ get; set; }
		public bool LiveLibrary{ get; set; }
		public bool ExplicitContent{ get; set; }
		public bool PlayOnPositionChange{ get; set; }
		public bool RevalidateOnExit{ get; set; }
		public bool RememberMinimal{ get; set; }
		public bool WasMinimal{ get; set; }
		public int MouseTimeoutIndex{ get; set; }
		public string LibraryLocation{ get; set; }
		public Size LastSize{ get; set; }
		public Point LastLocation{ get; set; }
	}
}