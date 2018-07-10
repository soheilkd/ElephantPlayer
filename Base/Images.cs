using MaterialDesignThemes.Wpf;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace Player
{
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
			control.Height = 200;
			control.Width = 200;
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
			if (picture.Data.Count == 0)
				return GetBitmap(PackIconKind.FileMusic);
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

		public static BitmapImage GetBitmap(byte[] data)
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

		public static byte[] GetBytes(BitmapImage bitmapImage)
		{
			byte[] data;
			JpegBitmapEncoder encoder = new JpegBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
			using (MemoryStream ms = new MemoryStream())
			{
				encoder.Save(ms);
				data = ms.ToArray();
			}
			return data;
		}
	}
}