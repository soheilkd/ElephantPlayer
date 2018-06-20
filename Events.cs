﻿namespace Player.Events
{
	public enum InfoType : byte
	{
		//Variable exchange
		Integer, Double, Media, Object, StringArray,
		//Media Control
		NextRequest, PrevRequest, LengthFound, Magnifiement,
		//Media Manager
		MediaRequest, TagEdit, CollectionUpdate
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
	public class InfoExchangeArgs<T> : System.EventArgs
	{
		public T Info { get; set; }
		public InfoExchangeArgs() { }
		public InfoExchangeArgs(T info) => Info = info;
	}
}