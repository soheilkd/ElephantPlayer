using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace Player.Models
{
	public static class Operator
	{
		private static readonly string[] SupportedMusics = "mp3;wma;aac;m4a".Split(';');
		private static readonly string[] SupportedVideos = "mp4;mpg;mkv;wmv;mov;avi;m4v;ts;wav;mpeg;webm".Split(';');
		private static readonly string[] SupportedFiles = "zip;rar;bin;dat".Split(';');

		public static MediaType GetMediaType(string path)
		{
			var ext = path.Substring(path.LastIndexOf('.') + 1).ToLower();
			if (SupportedMusics.Contains(ext)) return MediaType.Music;
			else if (SupportedVideos.Contains(ext)) return MediaType.Video;
			else if (SupportedFiles.Contains(ext)) return MediaType.File;
			else return MediaType.None;
		}

		public static void CleanTag(Media media, bool prompt = true)
		{
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

			using (var file = TagLib.File.Create(media))
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
			media.Name = media.Path.Substring(media.Path.LastIndexOf("\\") + 1);
			media.Directory = media.Path.Substring(0, media.Path.LastIndexOf("\\"));
			switch (GetMediaType(media))
			{
				case MediaType.Music:
					using (var t = TagLib.File.Create(media))
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
		public static void Reload(Media media)
		{
			media.IsLoaded = false;
			Load(media);
		}
		public static void Move(Media media, string toDir)
		{
			toDir += media.Name;
			File.Move(media, toDir);
			media.Path = toDir;
		}
		public static void Copy(Media media, string toDir)
		{
			toDir += media.Name;
			File.Copy(media, toDir, true);
		}
		public static bool DoesExists(Media media) => File.Exists(media);
	}
}
