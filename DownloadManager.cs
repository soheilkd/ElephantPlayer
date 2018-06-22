using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using Player.Events;

namespace Player
{
	public class DownloadManager
	{
		public event EventHandler<InfoExchangeArgs> DownloadCompleted;
		internal Dictionary<Media, WebClient> Pairs = new Dictionary<Media, WebClient>();

		public DownloadManager()
		{
			ServicePointManager.DefaultConnectionLimit = 10;
		}

		public void Download(Media media)
		{
			var SavePath = $"{App.Path}Downloads\\{media.Title}";
			Pairs.Add(media, new WebClient());
			Pairs[media].DownloadProgressChanged += (_, e) => media.Title = $"Downloading... {e.ProgressPercentage}%";
			Pairs[media].DownloadFileCompleted += (_, e) =>
			{
				if (media.Type == MediaType.OnlineFile)
				{
					var path = GetProperPath(media);
					using (var zip = new Ionic.Zip.ZipFile(SavePath))
						zip.ExtractAll(path, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
					File.Delete(SavePath);
					var d = from item in Directory.GetFiles(path, "*", SearchOption.AllDirectories) where MediaManager.IsMedia(item) select new Media(item);
					foreach (var item in d)
						MediaManager.CleanTag(item, false);
					DownloadCompleted?.Invoke(media, new InfoExchangeArgs(InfoType.MediaCollection, d.ToArray()));
				}
				else
					DownloadCompleted?.Invoke(media, new InfoExchangeArgs(InfoType.Media, new Media(SavePath)));
				Pairs.Remove(media);
			};
			Pairs[media].DownloadFileAsync(media.Url, SavePath);
		}

		public void Cancel(Media media)
		{
			Pairs[media].CancelAsync();
			Pairs.Remove(media);
			media.Reload();
			if (File.Exists(GetProperPath(media)))
				File.Delete(GetProperPath(media));
		}

		public static bool IsDownloadable(Uri Url, out MediaType mediaType)
		{
			var request = (HttpWebRequest)WebRequest.Create(Url);
			request.AddRange(0, 10);
			request.Timeout = 5000;
			try
			{
				var response = request.GetResponse();
				mediaType = GetMediaType(response.ContentType);
				response.Dispose();
				response = null;
				return mediaType != MediaType.None;
			}
			catch (Exception)
			{
				mediaType = MediaType.None;
				return false;
			}
		}

		public static MediaType GetMediaType(string contentType)
		{
			if (contentType.EndsWith("zip-compressed") || contentType.EndsWith("zip"))
				return MediaType.OnlineFile;
			if (contentType.EndsWith("octet-stream"))
				return MediaType.File;
			if (contentType.StartsWith("audio"))
				return MediaType.Music;
			if (contentType.StartsWith("video"))
				return MediaType.Video;
			return MediaType.None;
		}

		private static string GetProperPath(Media media)
		{
			if (media.Type == MediaType.OnlineFile)
				return $"{App.Path}Downloads\\{media.Name.Substring(0, media.Name.LastIndexOf(".z"))}";
			else
				return $"{App.Path}Downloads\\{media.Title}";
		}

	}
}
