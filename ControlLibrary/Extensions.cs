using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Player
{
	public static class Extensions
	{
		public static string ToNewString(this TimeSpan time) => time.ToString("c").Substring(3, 5);

		[DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool DeleteObject([In] IntPtr hObject);

		public static ImageSource ToImageSource(this System.Drawing.Bitmap bmp)
		{
			var handle = bmp.GetHbitmap();
			try
			{
				return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
			}
			finally
			{
				DeleteObject(handle);
			}
		}

		public static T To<T>(this object obj) where T : struct => (T)obj;
		public static T As<T>(this object obj) where T : class => obj as T;
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
		public static bool Contains(this string source, string str, StringComparison comparison)
		{
			return source.IndexOf(str, comparisonType: comparison) > -1;
		}
		public static bool IncaseContains(this string source, string str)
		{
			return Contains(source, str, StringComparison.OrdinalIgnoreCase);
		}

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
		public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
		{
			foreach (var item in collection)
				action(item);
		}

		public static void Repeat(Action action, int times)
		{
			for (int i = 0; i < times; i++)
				action();
		}
	}

}
