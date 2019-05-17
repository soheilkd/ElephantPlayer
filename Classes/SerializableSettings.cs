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
		public string LastPath{ get; set; }
		public bool LiveLibrary{ get; set; }
		public bool PlayOnPositionChange{ get; set; }
		public double MouseTimeout { get; set; }
		public string LibraryLocation{ get; set; }
		public Size LastSize{ get; set; }
	}
}