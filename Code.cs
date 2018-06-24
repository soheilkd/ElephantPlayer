using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace Player
{
	[Serializable]
	public class Preferences
	{
		[field: NonSerialized]
		private static readonly BinaryFormatter Formatter = new BinaryFormatter();
		[field: NonSerialized]
		private static readonly string DllFilePath = $"{App.Path}SettingsProvider.dll";

		[field: NonSerialized]
		public event EventHandler Changed;

		public PlayMode PlayMode { get; set; }
		public int MainKey { get; set; }
		public double Volume { get; set; }
		public Size LastSize { get; set; }
		public bool VisionOrientation { get; set; }
		public Point LastLoc { get; set; }
		public string LastPath { get; set; }
		public bool LiveLibrary { get; set; }
		public bool ExplicitContent { get; set; }
		public bool PlayOnPositionChange { get; set; }
		public bool RevalidateOnExit { get; set; }

		private int _MouseOverTimeOutIndex;

		public int MouseOverTimeoutIndex { get => _MouseOverTimeOutIndex; set { _MouseOverTimeOutIndex = value; Changed?.Invoke(this, null); } }
		public int MouseOverTimeout
		{
			get
			{
				switch (MouseOverTimeoutIndex)
				{
					case 0: return 500;
					case 1: return 1000;
					case 2: return 2000;
					case 3: return 3000;
					case 4: return 4000;
					case 5: return 5000;
					case 6: return 10000;
					case 7: return 60000;
					default: return 2000;
				}
			}
		}

		public static Preferences Load()
		{
			using (FileStream stream = File.Open(DllFilePath, FileMode.Open))
				return Formatter.Deserialize(stream) as Preferences;
		}
		public void Save()
		{
			using (FileStream stream = File.Open(DllFilePath, FileMode.Create))
				Formatter.Serialize(stream, this);
		}
	}

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
	}

	public static class Images
	{
		public static readonly BitmapImage MusicArt = GetBitmap(PackIconKind.Music);
		public static readonly BitmapImage VideoArt = GetBitmap(PackIconKind.Video);
		public static readonly BitmapImage NetArt = GetBitmap(PackIconKind.Cloud);

		public static BitmapImage GetBitmap(PackIconKind icon, Brush foreground = null)
		{
			var control = new Controls.MaterialButton()
			{
				Icon = icon,
				Foreground = foreground ?? Brushes.White
			};
			control.UpdateLayout();
			if (Double.IsNaN(control.Height))
				control.Height = 50;
			if (Double.IsNaN(control.Width))
				control.Width = 50;
			PngBitmapEncoder encoder = new PngBitmapEncoder();
			encoder.Frames.Clear();
			Transform transform = control.LayoutTransform;
			control.LayoutTransform = null;
			Size size = new Size(control.Width, control.Height);
			control.Measure(size);
			control.Arrange(new Rect(size));

			RenderTargetBitmap renderBitmap =
			  new RenderTargetBitmap(
				(Int32)size.Width,
				(Int32)size.Height,
				96d,
				96d,
				PixelFormats.Pbgra32);
			renderBitmap.Render(control);

			MemoryStream memStream = new MemoryStream();

			encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
			encoder.Save(memStream);
			memStream.Flush();
			var output = new BitmapImage();
			output.BeginInit();
			output.StreamSource = memStream;
			output.EndInit();

			return output;
		}

		public static BitmapImage GetBitmap(TagLib.IPicture picture)
		{
			byte[] pixels = new byte[picture.Data.Count];
			picture.Data.CopyTo(pixels, 0);
			var image = new BitmapImage();
			using (var ms = new MemoryStream(pixels))
			{
				image.BeginInit();
				image.CacheOption = BitmapCacheOption.OnLoad;
				image.StreamSource = ms;
				image.EndInit();
			}
			picture.Data.Clear();
			pixels = new byte[0];
			return image;
		}
	}
}