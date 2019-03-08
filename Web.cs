using Lastfm.Services;
using Library.Extensions;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Player
{
	public static class Web
	{
		//Session is required for API connection to LastFM 
		private static readonly Session _Session = new Session("cab344dc5414176234071148bc813382", "ef529dce9081c695dc32f31b800c7b9a");
		private static BlockingCollection<(string key, string)> ImageDatas = new BlockingCollection<(string, string)>(4);

		public static Album GetAlbum(string album, string artist = default)
		{
			try
			{
				if (artist != default)
					return new Album(artist, album, _Session);
				return Album.Search(album, _Session).GetFirstMatch();
			}
			catch (Exception)
			{
				return default;
			}
		}
		public static Artist GetArtist(string name)
		{
			try
			{
				return Artist.Search(name, _Session).GetFirstMatch();
			}
			catch (Exception)
			{
				return default;
			}
		}

		public static async Task<BitmapImage> GetArtistImage(string name)
		{
			if (Controller.Resource.TryGetValue(name, out var data))
				return data.ToBitmap();
			var artist = GetArtist(name);
			if (artist == null)
				return null;
			var image = await DownloadImage(artist.GetImageURL());
			Controller.Resource.Add(name, image.ToData());
			return image;
		}

		public static async Task<BitmapImage> GetAlbumImage(string name)
		{
			if (Controller.Resource.TryGetValue(name, out var data))
				return data.ToBitmap();
			var album = GetAlbum(name);
			if (album == null)
				return null;
			var image = await DownloadImage(album.GetImageURL());
			Controller.Resource.Add(name, image.ToData());
			return image;
		}

		public static void DownloadImageSync(string url, Action<BitmapImage> onDone)
		{
			var client = new WebClient();
			client.DownloadDataCompleted += (_, d) => onDone(d.Result.ToBitmap());
			client.DownloadDataAsync(new Uri(url));
		}
		public static async Task<BitmapImage> DownloadImage(string url)
		{
			if (string.IsNullOrWhiteSpace(url))
				return null;
			var client = new WebClient();
			var image = await client.DownloadDataTaskAsync(url);
			return image.ToBitmap();
		}
	}
}
