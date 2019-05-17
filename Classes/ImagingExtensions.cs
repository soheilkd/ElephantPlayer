using System.IO;
using System.Windows.Media.Imaging;
using TagLib;

namespace Player.Extensions
{
	public static class ImagingExtensions
	{
		public static byte[] GetBytes(this IPicture picture)
		{
			var bytes = new byte[picture.Data.Count];
			picture.Data.CopyTo(bytes, 0);
			return bytes;
		}
		public static BitmapImage GetBitmapImage(this byte[] bytes)
		{
			if (bytes.Length == 0)
				return default;
			var image = new BitmapImage();
			using (var ms = new MemoryStream(bytes))
			{
				image.BeginInit();
				image.CacheOption = BitmapCacheOption.OnLoad;
				image.StreamSource = ms;
				image.EndInit();
			}
			return image;
		}
		public static BitmapImage GetBitmapImage(this IPicture picture)
		{
			return picture.GetBytes().GetBitmapImage();
		}
	}
}
