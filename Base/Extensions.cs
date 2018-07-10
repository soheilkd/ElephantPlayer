using System;
using System.Collections.Generic;

namespace Player
{
	public static class Extensions
	{
		public static T To<T>(this object obj) where T : struct => (T)obj;
		public static T As<T>(this object obj) where T : class => obj as T;
		public static string ToNewString(this TimeSpan time) => time.ToString("c").Substring(3, 5);
		public static int IndexOf<T>(this IEnumerable<T> source, T value)
		{
			int index = 0;
			var comparer = EqualityComparer<T>.Default; // or pass in as a parameter
			foreach (T item in source)
			{
				if (comparer.Equals(item, value)) return index;
				index++;
			}
			return -1;
		}
		public static bool IncaseContains(this string item, string with) => item.ToLower().Contains(with.ToLower());

		public static void For<T>(this IList<T> collection, Action<T> action)
		{
			for (int i = 0; i < collection.Count; i++)
				action(collection[i]);
		}
		public static void For<T>(this IList<T> collection, Action<T> action, Func<T, bool> condition)
		{
			for (int i = 0; i < collection.Count; i++)
				if (condition(collection[i]))
					action(collection[i]);
		}

		public static void Do(Action action, int times)
		{
			for (int i = 0; i < times; i++)
				action();
		}
	}
}