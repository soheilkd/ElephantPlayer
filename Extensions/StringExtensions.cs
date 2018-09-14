using System;

namespace Player.Extensions
{
	public static class StringExtensions
	{
		public static bool IncaseContains(this string source, string str)
		{
			return source.IndexOf(str, StringComparison.OrdinalIgnoreCase) != -1;
		}
	}

}
