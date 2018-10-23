using System.IO;
using System.Windows.Media.Imaging;
using TagLib;

namespace Player.Extensions
{
	public static class ImagingExtensions
	{
		public static BitmapImage ToBitmapImage(this IPicture picture)
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
	}

}
