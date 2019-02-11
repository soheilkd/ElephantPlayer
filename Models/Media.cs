using Library.Extensions;
using Player.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Player.Models
{
	[Serializable]
	public class Media : INotifyPropertyChanged
	{
		public string Sublabel
		{
			get
			{
				if (string.IsNullOrWhiteSpace(Album))
					return Artist;
				else
					return $"{Artist} - {Album}";
			}
		}

		private string _Name;
		private string _Artist;
		private string _Title;
		private string _Album;
		private string _Dir;
		private bool _IsPlaying;
		private TimeSpan _Len;
		public MediaType Type;
		public string Lyrics = "";
		[field: NonSerialized] public bool IsLoaded = false;
		[field: NonSerialized] private byte[] _Artwork = default;
		[field: NonSerialized] public event PropertyChangedEventHandler PropertyChanged;

		public string Name { get => _Name; set => Set(ref _Name, value); }
		public string Artist { get => _Artist; set => Set(ref _Artist, value); }
		public string Title { get => _Title; set => Set(ref _Title, value); }
		public string Album { get => _Album; set => Set(ref _Album, value); }
		public string Directory { get => _Dir; set => Set(ref _Dir, value); }
		public bool IsPlaying { get => _IsPlaying; set => Set(ref _IsPlaying, value); }
		public TimeSpan Length { get => _Len; set => Set(ref _Len, value); }
		public List<DateTime> PlayTimes { get; set; } = new List<DateTime>();
		public DateTime AdditionDate { get; set; }
		public string Path { get; set; }
		public bool IsVideo => Type == MediaType.Video;
		public bool DoesExist => File.Exists(Path);
		public BitmapImage Artwork
		{
			get
			{
				if (_Artwork == default)
					using (var file = TagLib.File.Create(Path))
						_Artwork = file.Tag.Pictures.Length != 0 ? file.Tag.Pictures[0].GetBytes() : new byte[0];
				return _Artwork.GetBitmapImage();
			}
		}

		public Media() { }
		public Media(string path)
		{
			Path = path;
			AdditionDate = DateTime.Now;
		}

		protected void Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
		{
			field = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public bool Matches(string query)
		{
			return string.IsNullOrWhiteSpace(query)
				? true
				: Title.IncaseContains(query) ||
				Name.IncaseContains(query) ||
				Artist.IncaseContains(query) ||
				Album.IncaseContains(query) ||
				Path.IncaseContains(query);
		}

		public void MoveTo(string dir)
		{
			dir += Name;
			if (File.Exists(dir))
				return;
			File.Move(Path, dir);
			Path = dir;
		}
		public void CopyTo(string dir)
		{
			dir += Name;
			File.Copy(Path, dir, true);
		}

		#region Load
		private static readonly string[] SupportedMusics = "mp3;wma;aac;m4a".Split(';');
		private static readonly string[] SupportedVideos = "mp4;mpg;mkv;wmv;mov;avi;m4v;ts;wav;mpeg;webm".Split(';');

		private static MediaType GetMediaType(string path)
		{
			var ext = path.Substring(path.LastIndexOf('.') + 1).ToLower();
			if (SupportedMusics.Contains(ext))
				return MediaType.Music;
			else if (SupportedVideos.Contains(ext))
				return MediaType.Video;
			else
				return MediaType.None;
		}

		public void Load()
		{
			if (IsLoaded)
				return;
			if (!DoesExist)
			{
				Type = MediaType.None;
				return;
			}
			Name = Path.Substring(Path.LastIndexOf("\\") + 1);
			Directory = Path.Substring(0, Path.LastIndexOf("\\"));
			switch (Type = GetMediaType(Path))
			{
				case MediaType.Music:
					using (var t = TagLib.File.Create(Path))
					{
						Artist = t.Tag.FirstPerformer ?? Path.Substring(0, Path.LastIndexOf("\\"));
						Title = t.Tag.Title ?? Name.Substring(0, Name.LastIndexOf("."));
						Album = t.Tag.Album ?? string.Empty;
						Lyrics = t.Tag.Lyrics ?? string.Empty;
					}
					break;
				case MediaType.Video:
					Title = Name;
					Artist = Path.Substring(0, Path.LastIndexOf("\\"));
					Artist = Artist.Substring(Artist.LastIndexOf("\\") + 1);
					Album = "Video";
					break;
				default: break;
			}
			IsLoaded = true;
		}
		public void Reload()
		{
			IsLoaded = false;
			Load();
		}
		#endregion
		public void CleanTag()
		{
			string deleteWord(string target, string word)
			{
				var output = "";
				var lit1 = target.ToLower().IndexOf(word);
				if (target.StartsWith(word)) lit1 = 0;
				if (lit1 == -1) return target;
				var temp1 = target.Substring(0, lit1);
				if (temp1 == string.Empty) temp1 = " ";
				if (temp1.LastIndexOf(' ') == -1) return " ";
				var temp2 = temp1.Substring(0, temp1.LastIndexOf(' '));
				output += temp2;
				var lit2 = target.ToLower().LastIndexOf(word);
				temp1 = target.Substring(lit2);
				temp2 = !temp1.EndsWith(temp1) ? temp1.Substring(temp1.IndexOf(' ')) : "";
				output += temp2;
				return output;
			}
			string c(string target)
			{
				if (string.IsNullOrWhiteSpace(target)) return target;
				var litretures = ".com;.ir;.org;www.;@;.me;.biz;.net;.us;.az".Split(';');
				var lit = new List<string>();
				litretures.For(each => target.IncaseContains(each), each => lit.Add(each));
				lit.ToArray().For(each => target = deleteWord(target, each));
				return target;
			}

			using (var file = TagLib.File.Create(Path))
			{
				TagLib.Tag tag = file.Tag;
				tag.Album = c(tag.Album ?? " ");
				tag.Comment = "";
				tag.Composers = new string[] { c(tag.FirstComposer ?? " ") };
				tag.Conductor = c(tag.Conductor ?? " ");
				tag.Copyright = "";
				tag.Genres = new string[] { c(tag.FirstGenre ?? " ") };
				tag.Performers = new string[] { c(tag.FirstPerformer ?? " ") };
				tag.Title = c(tag.Title ?? " ");
				file.Save();
				Reload();
			}
		}

		public static bool TryLoadFromPath(string path, out Media media)
		{
			media = new Media(path);
			if (!media.DoesExist)
				return false;
			media.Load();
			return media.Type != MediaType.None;
		}

		public static implicit operator Uri(Media media) => new Uri(media.Path);
		public static implicit operator MediaType(Media media) => media.Type;
	}
}
