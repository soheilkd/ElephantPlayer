namespace EPlayer.Library
{
	public delegate void InfoExchangeHandler<T>(object sender, InfoExchangeArgs<T> e);

	public static class Extension
	{
		public static void Invoke<T>(this InfoExchangeHandler<T> handler, T parameter, object sender = default)
		{
			handler?.Invoke(sender, new InfoExchangeArgs<T>(parameter));
		}
	}
}
