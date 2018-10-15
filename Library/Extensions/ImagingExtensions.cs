using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static Player.NativeMethods;

namespace Player.Extensions
{
	public static class ImagingExtensions
	{
		public static ImageSource ToImageSource(this Bitmap bmp)
		{
			IntPtr handle = bmp.GetHbitmap();
			try
			{
				return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
			}
			finally
			{
				DeleteObject(handle);
			}
		}

		public static BitmapImage ToBitmap(this TagLib.IPicture picture)
		{
			if (picture.Data.Count == 0)
				return null;
			var pixels = new byte[picture.Data.Count];
			picture.Data.CopyTo(pixels, 0);
			var image = new BitmapImage();
			using (var ms = new MemoryStream(pixels))
			{
				image.BeginInit();
				image.CacheOption = BitmapCacheOption.OnLoad;
				image.StreamSource = ms;
				image.EndInit();
			}
			return image;
		}

		public static BitmapImage ToBitmap(this byte[] data)
		{
			if (data == null || data.Length == 0)
				return new BitmapImage();
			using (var memStream = new MemoryStream())
			{
				data.For(eachByte => memStream.WriteByte(eachByte));
				var image = new BitmapImage();
				image.BeginInit();
				image.CacheOption = BitmapCacheOption.OnLoad;
				image.StreamSource = memStream;
				image.EndInit();
				return image;
			}
		}
		public static byte[] ToData(this BitmapImage bitmapImage)
		{
			byte[] data;
			var encoder = new PngBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
			using (var ms = new MemoryStream())
			{
				encoder.Save(ms);
				data = ms.ToArray();
			}
			return data;
		}
	}

}
