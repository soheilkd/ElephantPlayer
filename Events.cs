namespace Player.Events
{
	public enum InfoType : byte
	{
		//Content exchange
		Integer, Double, Media, Object, StringArray, MediaCollection,
		//Media Control
		NextRequest, PrevRequest, LengthFound, Magnifiement,
		//Media Manager
		MediaRequest, TagEdit, CollectionUpdate,
	}

	public class InfoExchangeArgs : System.EventArgs
	{
		public InfoType Type { get; private set; }
		public object Object { get; set; }

		public InfoExchangeArgs() { }
		public InfoExchangeArgs(InfoType type) => Type = type;
		public InfoExchangeArgs(InfoType type, object value)
		{
			Type = type;
			Object = value;
		}
	}
}