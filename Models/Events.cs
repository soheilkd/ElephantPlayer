namespace Player.Events
{
	public enum RequestType
	{
		Media, Sync,
		Next, Previous, Magnifiement, Collapse, Expand
	}

	public class RequestArgs: System.EventArgs
	{
		public RequestType Request { get; set; }
		public RequestArgs(RequestType request) => Request = request;
	}
	
	public class InfoExchangeArgs<T>: System.EventArgs
	{
		public T Parameter { get; set; }

		public InfoExchangeArgs() { }
		public InfoExchangeArgs(T para) => Parameter = para;
	}
}