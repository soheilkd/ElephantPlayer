namespace EPlayer.Library
{
	public class InfoExchangeArgs<T>: System.EventArgs
	{
		public T Parameter { get; set; }
		
		public InfoExchangeArgs() {  }
		public InfoExchangeArgs(T para) => Parameter = para;

		public static implicit operator InfoExchangeArgs<T>(T obj) => new InfoExchangeArgs<T>(obj);
		public static explicit operator T(InfoExchangeArgs<T> info) => info.Parameter;
	}
}