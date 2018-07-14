using Player.Controls;
using System.IO;
using System.Windows.Media.Imaging;

namespace Player
{
	public static class Images
	{
		public static readonly BitmapImage MusicArt = Controls.Extensions.GetBitmap(IconKind.Music);
		public static readonly BitmapImage VideoArt = Controls.Extensions.GetBitmap(IconKind.Video);
		public static readonly BitmapImage NetArt = Controls.Extensions.GetBitmap(IconKind.Cloud);

		public static BitmapImage GetBitmap(TagLib.IPicture picture)
		{
			if (picture.Data.Count == 0)
				return Controls.Extensions.GetBitmap(IconKind.FileMusic);
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