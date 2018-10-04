using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Player.Extensions
{
	public static class CollectionExtensions
	{
		public static void For<T>(this IList<T> list, Action<T> action)
		{
			for (int i = 0; i < list.Count; i++)
				action(list[i]);
		}
		public static void For<T>(this IList<T> list, Func<T, bool> condition, Action<T> action)
		{
			for (int i = 0; i < list.Count; i++)
				if (condition(list[i]))
					action(list[i]);
		}
		public static void For<T>(this IList<T> list, Action<int, T> action)
		{
			for (int i = 0; i < list.Count; i++)
				action(i, list[i]);
		}
		public static void ForEach<T>(this IEnumerable enumerable, Action<T> action)
		{
			foreach (T item in enumerable)
				action(item);
		}
		public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
		{
			foreach (T item in collection)
				action(item);
		}
		public static int IndexOf<T>(this IEnumerable<T> source, T value)
		{
			int index = 0;
			EqualityComparer<T> comparer = EqualityComparer<T>.Default; // or pass in as a parameter
			foreach (T item in source)
			{
				if (comparer.Equals(item, value)) return index;
				index++;
			}
			return -1;
		}
	}

}
