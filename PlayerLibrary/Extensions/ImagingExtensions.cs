using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Player.Extensions
{
	public static class ImagingExtensions
	{
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


		public static BitmapImage GetBitmap(this TagLib.IPicture picture)
		{
			if (picture.Data.Count == 0)
				return null;
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
			pixels = null;
			picture = null;
			return image;
		}
	}

}
