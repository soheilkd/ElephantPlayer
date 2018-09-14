namespace Player.Extensions
{
	public static class ObjectExtensions
	{
		public static T To<T>(this object obj) where T : struct => (T)obj;
		public static T As<T>(this object obj) where T : class => obj as T;
	}

}
