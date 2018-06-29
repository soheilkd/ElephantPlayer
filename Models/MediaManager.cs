using Player.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace Player
{
	public enum PlayMode { Shuffle, RepeatOne, Repeat }

	public class MediaManager : ObservableCollection<Media>
	{
		public MediaManager()
		{
			LibraryManager.Load().Unordered.For(each => Add(each));
			DownloadManager.DownloadCompleted += DownloadCompleted;
		}

		public Collection<Media> Queue;
		private Random Shuffle = new Random(DateTime.Now.Millisecond);
		public Media CurrentlyPlaying;
		public event EventHandler<InfoExchangeArgs> Change;
		public readonly DownloadManager DownloadManager = new DownloadManager();

		public void Add(string path, bool requestPlay = false)
		{
			if (Directory.Exists(path))
				Directory.GetFiles(path, "*", SearchOption.AllDirectories).For(each => Add(each));
			var media = new Media(path);
			if (!DoesExists(media))
			{
				Console.WriteLine(path);
				return;
			}
			Load(media);
			var duplication = this.Where(item => item.Path == path);
			if (duplication.Count() != 0 && requestPlay)
			{
				RequestPlay(duplication.First());
				return;
			}
			Insert(0, media);
			if (requestPlay)
				RequestPlay();
		}

		public void Delete(Media media)
		{
			bool reqNext = CurrentlyPlaying == media;
			File.Delete(media.Path);
			this.Where(each => each.Path == media.Path).ToArray().For(each => Remove(each));
			if (reqNext)
				RequestPlay(Next());
		}

		public Media Play(int index = 0, Collection<Media> collection = null)
		{
			return Play((collection ?? this)[index]);
		}
		public Media Play(Media media)
		{
			this.For(each => each.IsPlaying = false);
			media.IsPlaying = true;
			CurrentlyPlaying = media;
			return media;
		}
		public Media Next(Collection<Media> coll = null, int currentlyPlayingIndex = -1)
		{
			if (currentlyPlayingIndex == -1)
				currentlyPlayingIndex = (coll ?? this).IndexOf(CurrentlyPlaying);
			switch (App.Settings.PlayMode)
			{
				case PlayMode.Shuffle: return Play(Shuffle.Next(0, Count));
				case PlayMode.Repeat: return Play(currentlyPlayingIndex == Count - 1 ? 0 : ++currentlyPlayingIndex);
				case PlayMode.RepeatOne: return Play(currentlyPlayingIndex);
				default: return null;
			}
		}
		public Media Previous(Collection<Media> coll = null, int currentlyPlayingIndex = -1)
		{
			if (currentlyPlayingIndex == -1)
				currentlyPlayingIndex = (coll ?? this).IndexOf(CurrentlyPlaying);
			switch (App.Settings.PlayMode)
			{
				case PlayMode.Shuffle: return Play(Shuffle.Next(0, Count));
				case PlayMode.Repeat: return Play(currentlyPlayingIndex == 0 ? Count - 1 : --currentlyPlayingIndex);
				case PlayMode.RepeatOne: return Play(currentlyPlayingIndex);
				default: return null;
			}
		}

		public void CloseSeason()
		{
			if (App.Settings.RevalidateOnExit)
				Revalidate();
			LibraryManager.Save(this);
		}

		private void RequestPlay() => RequestPlay(this[0]);
		private void RequestPlay(Media media)
		{
			CurrentlyPlaying = media;
			Change?.Invoke(media, new InfoExchangeArgs(InfoType.MediaRequest));
			if (App.Settings.ExplicitContent)
			{
				if (media.Title.ToLower().StartsWith("spankbang"))
					Remove(media);
			}
		}

		public void UpdateOnPath(Media source)
		{
			this.For(each => Reload(each), each => each.Path == source.Path);
		}
		public void Revalidate()
		{
			this.For(each => Reload(each));
			this.For(each => Remove(each), each => !DoesExists(each));
		}
		
		private void DownloadCompleted(object sender, InfoExchangeArgs e)
		{
			if (e.Type == InfoType.Media) Add(e.Object as Media);
			else e.Object.As<Media[]>().For(each => Add(each));
		}
		#region Singular Media Operations
		private static readonly string[] SupportedMusics = "mp3;wma;aac;m4a".Split(';');
		private static readonly string[] SupportedVideos = "mp4;mpg;mkv;wmv;mov;avi;m4v;ts;wav;mpeg;webm".Split(';');
		private static readonly string[] SupportedFiles = "zip;rar;bin;dat".Split(';');

		public static MediaType GetMediaType(Uri url)
		{
			var ext = GetExt(url);
			if (url.IsFile)
			{
				if (SupportedMusics.Contains(ext)) return MediaType.Music;
				else if (SupportedVideos.Contains(ext)) return MediaType.Video;
				else if (SupportedFiles.Contains(ext)) return MediaType.File;
				else return MediaType.None;
			}
			else
			{
				if (DownloadManager.IsDownloadable(url, out var type)) return type;
				else return MediaType.None;
			}
		}
		public static string GetExt(Uri url)
		{
			if (url.IsFile)
				return url.AbsolutePath.Substring((url.AbsolutePath ?? " . ").LastIndexOf('.') + 1).ToLower();
			else
				return url.Segments.Last().Substring(url.Segments.Last().LastIndexOf('.') + 1).ToLower();
		}

		public static void CleanTag(Media media, bool prompt = true)
		{
			if (media.Type != MediaType.Music)
			{
				MessageBox.Show("Not supported on this type of media", "", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			string deleteWord(string target, string word)
			{
				string output = "";
				int lit1 = target.ToLower().IndexOf(word);
				if (target.StartsWith(word)) lit1 = 0;
				if (lit1 == -1)
					return target;
				string temp1 = target.Substring(0, lit1);
				if (temp1 == String.Empty) temp1 = " ";
				if (temp1.LastIndexOf(' ') == -1) return " ";
				string temp2 = temp1.Substring(0, temp1.LastIndexOf(' '));
				output += temp2;
				int lit2 = target.ToLower().LastIndexOf(word);
				temp1 = target.Substring(lit2);
				if (!temp1.EndsWith(temp1))
					temp2 = temp1.Substring(temp1.IndexOf(' '));
				else temp2 = "";
				output += temp2;
				return output;
			}
			string c(string target)
			{
				if (String.IsNullOrWhiteSpace(target))
					return target;
				string[] litretures = new string[] { ".com", ".ir", ".org", "www.", "@", ".me", ".biz", ".net" };
				var lit = new List<string>();
				litretures.For(each => lit.Add(each), each => target.IncaseContains(each));
				lit.ToArray().For(each => target = deleteWord(target, each));
				return target;
			}

			using (var file = TagLib.File.Create(media.Path))
			{
				string[] form(TagLib.Tag tag2)
				{
					return new string[]
					{
						tag2.Album,
						tag2.Comment,
						tag2.FirstComposer,
						tag2.Conductor,
						tag2.Copyright,
						tag2.FirstGenre,
						tag2.FirstPerformer,
						tag2.Title
					};
				}
				var tag = file.Tag;
				var manip1 = form(tag);
				tag.Album = c(tag.Album ?? " ");
				tag.Comment = "";
				tag.Composers = new string[] { c(tag.FirstComposer ?? " ") };
				tag.Conductor = c(tag.Conductor ?? " ");
				tag.Copyright = "";
				tag.Genres = new string[] { c(tag.FirstGenre ?? " ") };
				tag.Performers = new string[] { c(tag.FirstPerformer ?? " ") };
				tag.Title = c(tag.Title ?? " ");
				var manip2 = form(tag);
				var manip = "Detergent will change these values: \r\n";
				if (manip1[0] != manip2[0]) manip += "Album: " + manip1[0] + " => " + manip2[0] + "\r\n";
				if (manip1[1] != manip2[1]) manip += "Comment: " + manip1[1] + " => " + manip2[1] + "\r\n";
				if (manip1[2] != manip2[2]) manip += "Composer: " + manip1[2] + " => " + manip2[2] + "\r\n";
				if (manip1[3] != manip2[3]) manip += "Conductor: " + manip1[3] + " => " + manip2[3] + "\r\n";
				if (manip1[4] != manip2[4]) manip += "Copyright: " + manip1[4] + " => " + manip2[4] + "\r\n";
				if (manip1[5] != manip2[5]) manip += "Genre: " + manip1[5] + " => " + manip2[5] + "\r\n";
				if (manip1[6] != manip2[6]) manip += "Artist: " + manip1[6] + " => " + manip2[6] + "\r\n";
				if (manip1[7] != manip2[7]) manip += "Title: " + manip1[7] + " => " + manip2[7] + "\r\n";
				if (manip.Length <= 42)
				{
					MessageBox.Show("Couldn't find any changable thing", "JIZZZ", MessageBoxButton.OK, MessageBoxImage.Asterisk);
					return;
				}
				manip += "\r\nContinue?";
				if (prompt)
				{
					var res = MessageBox.Show(manip, "Continue?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
					if (res == MessageBoxResult.No)
						return;
				}
				file.Save();
				Reload(media);
			}
		}

		public static void Load(Media media)
		{
			if (media.IsLoaded)
				return;
			if (media.IsOffline)
			{
				media.Name = media.Path.Substring(media.Path.LastIndexOf("\\") + 1);
				media.Directory = media.Path.Substring(0, media.Path.LastIndexOf("\\"));
				switch (GetMediaType(media.Url))
				{
					case MediaType.Music:
						using (var t = TagLib.File.Create(media.Path))
						{
							media.Artist = t.Tag.FirstPerformer ?? media.Path.Substring(0, media.Path.LastIndexOf("\\"));
							media.Title = t.Tag.Title ?? media.Name.Substring(0, media.Name.LastIndexOf("."));
							media.Album = t.Tag.Album ?? String.Empty;
							media.Artwork = t.Tag.Pictures.Length >= 1 ? Images.GetBitmap(t.Tag.Pictures[0]) : Images.MusicArt;
							media.Type = MediaType.Music;
							media.Lyrics = t.Tag.Lyrics ?? String.Empty;
						}
						break;
					case MediaType.Video:
						media.Title = media.Name;
						media.Artist = media.Path.Substring(0, media.Path.LastIndexOf("\\"));
						media.Artist = media.Artist.Substring(media.Artist.LastIndexOf("\\") + 1);
						media.Album = "Video";
						media.Artwork = Images.VideoArt;
						media.Type = MediaType.Video;
						media.Length = TimeSpan.Zero;
						break;
					default: break;
				}
				media.IsLoaded = true;
			}
			else
			{
				if (DownloadManager.IsDownloadable(media.Url, out media.Type))
				{
					media.Name = Uri.UnescapeDataString(media.Url.Segments.Last());
					media.Title = media.Name;
					media.Artist = media.Url.Host;
					media.Album = "Cloud";
					media.Artwork = Images.NetArt;
					media.IsLoaded = true;
				}
			}
		}
		public static void Reload(Media media)
		{
			media.IsLoaded = false;
			Load(media);
		}
		public static void Move(Media media, string toDir)
		{
			toDir += media.Name;
			File.Move(media.Path, toDir);
			media.Url = new Uri(toDir);
		}
		public static void Copy(Media media, string toDir)
		{
			toDir += media.Name;
			File.Copy(media.Path, toDir, true);
		}
		public static bool DoesExists(Uri url)
		{
			if (url.IsFile)
			{
				if (!File.Exists(url.AbsolutePath))
					return false;
				return File.Exists(url.AbsolutePath);
			}
			else
				return DownloadManager.IsDownloadable(url, out _);
		}
		public static bool DoesExists(Media media) => DoesExists(media.Url);

		public static Media CreateMedia(string path)
		{
			var m = new Media(path);
			Load(m);
			return m;
		}
		public static bool IsMedia(string path)
		{
			return GetMediaType(new Uri(path)) == MediaType.File;
		}
		#endregion
	}
}
