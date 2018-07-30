using System.IO;
using System.Windows.Media.Imaging;

namespace Player
{
	public static class Images
	{
		public static readonly BitmapImage MusicArt = Controls.Extensions.GetBitmap(Controls.IconType.MusicNote);
		public static readonly BitmapImage VideoArt = Controls.Extensions.GetBitmap(Controls.IconType.Video);

		public static BitmapImage GetBitmap(TagLib.IPicture picture)
		{
			if (picture.Data.Count == 0)
				return Controls.Extensions.GetBitmap(Controls.IconType.MusicNote);
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