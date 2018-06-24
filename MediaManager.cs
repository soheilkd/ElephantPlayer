using Player.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;

namespace Player
{
	[Flags]
	public enum MediaType
	{
		None = 0B0,
		File = 0B10,
		Music = 0B100 | File,
		Video = 0B1000 | File,
		OnlineFile = 0B10000 | File,
		OnlineMusic = Music | OnlineFile,
		OnlineVideo = Video | OnlineFile
	}

	[Serializable]
	public class Media : INotifyPropertyChanged
	{
		protected void Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
		{
			field = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		public Media() { }
		private string _Name;
		public string Name { get => _Name; private set => Set(ref _Name, value); }
		private string _Artist;
		public string Artist { get => _Artist; private set => Set(ref _Artist, value); }
		private string _Title;
		public string Title { get => _Title; set => Set(ref _Title, value); }
		private string _Album;
		public string Album { get => _Album; private set => Set(ref _Album, value); }
		private string _Directory;
		public string Directory { get => _Directory; private set => Set(ref _Directory, value); }
		private TimeSpan _Len;
		public TimeSpan Length { get => _Len; set => Set(ref _Len, value); }
		private int _PlayCount;
		public int PlayCount { get => _PlayCount; set => Set(ref _PlayCount, value); }
		[NonSerialized]
		private bool _IsPlaying;
		public bool IsPlaying { get => _IsPlaying; set => Set(ref _IsPlaying, value); }
		public DateTime AdditionDate { get; set; }
		private Uri _Url;
		public Uri Url
		{
			get => _Url;
			set
			{
				if (value.IsFile)
					_Url = value;
				else
				{
					if (value.AbsoluteUri[4] == 's')
						_Url = new Uri(value.AbsoluteUri.Remove(4, 1));
					else _Url = value;
				}
			}
		}
		public bool IsOffline => Url.IsFile;
		public MediaType Type;
		public string Path => Url.IsFile ? Url.LocalPath : Url.AbsoluteUri;
		[NonSerialized] public string Lyrics = "";
		[NonSerialized] public bool IsLoaded = false;
		[NonSerialized] public System.Windows.Media.Imaging.BitmapSource Artwork;
		public bool IsVideo => Type == MediaType.Video;
		public bool Exists
		{
			get
			{
				if (Url.IsFile)
					return File.Exists(Path);
				else
					return DownloadManager.IsDownloadable(Url, out _);
			}
		}
		
		[field: NonSerialized]
		public event PropertyChangedEventHandler PropertyChanged;

		public Media(string path) : this(new Uri(path)) { }
		public Media(Uri url)
		{
			Url = url;
			AdditionDate = DateTime.Now;
			Load();
		}
		public override string ToString() => $"{Artist} - {Title}";

		public void MoveTo(string dir)
		{
			dir += Name;
			File.Move(Path, dir);
			Url = new Uri(dir);
		}
		public void CopyTo(string dir)
		{
			dir += Name;
			File.Copy(Path, dir, true);
		}
		public void Load()
		{
			if (IsLoaded)
				return;
			if (IsOffline)
			{
				if (!Exists)
				{
					Name = null;
					Directory = null;
					Artist = null;
					Title = null;
					Album = null;
					Artwork = null;
					Type = MediaType.None;
					Lyrics = null;
				}

				Name = Path.Substring(Path.LastIndexOf("\\") + 1);
				Directory = Path.Substring(0, Path.LastIndexOf("\\"));
				switch (MediaManager.GetMediaType(Url))
				{
					case MediaType.Music:
						using (var t = TagLib.File.Create(Path))
						{
							Artist = t.Tag.FirstPerformer ?? Path.Substring(0, Path.LastIndexOf("\\"));
							Title = t.Tag.Title ?? Name.Substring(0, Name.LastIndexOf("."));
							Album = t.Tag.Album ?? String.Empty;
							Artwork = t.Tag.Pictures.Length >= 1 ? Images.GetBitmap(t.Tag.Pictures[0]) : Images.MusicArt;
							Type = MediaType.Music;
							Lyrics = t.Tag.Lyrics ?? String.Empty;
						}
						break;
					case MediaType.Video:
						Title = Name;
						Artist = Path.Substring(0, Path.LastIndexOf("\\"));
						Artist = Artist.Substring(Artist.LastIndexOf("\\") + 1);
						Album = "Video";
						Artwork = Images.VideoArt;
						Type = MediaType.Video;
						Length = TimeSpan.Zero;
						break;
					default: break;
				}
				IsLoaded = true;
			}
			else
			{
				if (DownloadManager.IsDownloadable(Url, out Type))
				{
					Name = Uri.UnescapeDataString(Url.Segments.Last());
					Title = Name;
					Url = Url;
					Artist = Url.Host;
					Album = "Cloud";
					Artwork = Images.NetArt;
					IsLoaded = true;
				}
			}
		}
		public void Reload()
		{
			IsLoaded = false;
			Load();
		}

		public Media Shallow => MemberwiseClone() as Media;
	}

	public enum PlayMode { Shuffle, RepeatOne, Repeat }
	public enum QueueType { Unordered, Artist, Album, Type, Dir, Title }
	public class MediaManager : ObservableCollection<Media>
	{
		public MediaManager()
		{
			LibraryOperator.Load().Unordered.For(each => Add(each));
			DownloadManager.DownloadCompleted += DownloadManager_DownloadCompleted;
		}

		private void DownloadManager_DownloadCompleted(object sender, InfoExchangeArgs e)
		{
			switch (e.Type)
			{
				case InfoType.Media:
					Add(e.Object as Media);
					break;
				case InfoType.MediaCollection:
					e.Object.As<Media[]>().For(each => Add(each));
					break;
				default: break;
			}
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
			if (!media.Exists)
				return;
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
			LibraryOperator.Save(this);
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
			this.For(each => each.Reload(), each => each.Path == source.Path);
		}
		public void Revalidate()
		{
			this.For(each => each.Reload());
			this.For(each => Remove(each), each => !each.Exists);
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
		public static bool IsMedia(string path) => new Media(path).Type != MediaType.None;

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
				media.Reload();
			}
		}
		#endregion
	}

	public static class LibraryOperator
	{
		private static string LibraryPath => $"{App.Path}\\Library.dll";
		public static SerializableMediaCollection LoadedCollection;
		public static void Save(Collection<Media> medias)
		{
			SerializableMediaCollection collection;
			collection.Unordered = new ObservableCollection<Media>(medias);
			collection.ByArtist = new ObservableCollection<Media>(medias.OrderBy(each => each.Artist));
			collection.ByAlbum = new ObservableCollection<Media>(medias.OrderBy(each => each.Album));
			using (FileStream stream = new FileStream(LibraryPath, FileMode.Create))
				(new BinaryFormatter()).Serialize(stream, collection);
		}
		public static SerializableMediaCollection Load()
		{
			if (!File.Exists(LibraryPath))
				return new SerializableMediaCollection()
				{
					Unordered = new ObservableCollection<Media>(),
					ByAlbum = new ObservableCollection<Media>(),
					ByArtist = new ObservableCollection<Media>(),
				};
			using (FileStream stream = new FileStream(LibraryPath, FileMode.Open))
				LoadedCollection = (SerializableMediaCollection)(new BinaryFormatter()).Deserialize(stream);
			return LoadedCollection;
		}
	}
	[Serializable]
	public struct SerializableMediaCollection
	{
		public ObservableCollection<Media> Unordered;
		public ObservableCollection<Media> ByArtist;
		public ObservableCollection<Media> ByAlbum;
	}
}
