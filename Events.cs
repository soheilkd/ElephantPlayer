namespace Player.Events
{
	public enum InfoType
	{
		//Variable exchange
		Integer, Double, Media, Object, StringArray,
		//Media Control
		NextRequest, PrevRequest, LengthFound, Magnifiement,
		//Media Manager
		MediaRequested, EditingTag
	}

	public class InfoExchangeArgs : System.EventArgs
	{
		public InfoType Type { get; set; }
		public object Object { get; set; }
		public int Integer { get; set; }

		public InfoExchangeArgs() { }
		public InfoExchangeArgs(InfoType type) => Type = type;
		public InfoExchangeArgs(int integer)
		{
			Type = InfoType.Integer;
			Integer = integer;
		}
		public InfoExchangeArgs(int integer, object obj)
		{
			Type = InfoType.Integer;
			Integer = integer;
			Object = obj;
		}
		public InfoExchangeArgs(object obj) => Object = obj;
	}
}