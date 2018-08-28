using System;
using System.Collections.Generic;

namespace Player.Extensions
{
	public static class CollectionExtensions
	{
		public static void For<T>(this IList<T> collection, Action<T> action)
		{
			for (int i = 0; i < collection.Count; i++)
				action(collection[i]);
		}
		public static void For<T>(this IList<T> collection, Func<T, bool> condition, Action<T> action)
		{
			for (int i = 0; i < collection.Count; i++)
				if (condition(collection[i]))
					action(collection[i]);
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
