namespace Player
{
	public class InfoExchangeArgs<T>: System.EventArgs
	{
		public T Parameter { get; set; }

		public InfoExchangeArgs() { }
		public InfoExchangeArgs(T para) => Parameter = para;
	}
}