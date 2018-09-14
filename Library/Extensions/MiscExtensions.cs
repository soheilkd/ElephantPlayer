using System;

namespace Player.Extensions
{
	public static class MiscExtensions
	{
		public static string ToNewString(this TimeSpan time) => time.ToString("c").Substring(3, 5);

		public static void Repeat(Action action, int times)
		{
			for (int i = 0; i < times; i++)
				action();
		}
	}
}
