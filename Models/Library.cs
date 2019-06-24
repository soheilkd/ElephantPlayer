using Player.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Player.Models
{
	[Serializable]
	public class Library
	{
		public MediaQueue Songs { get; set; } = new MediaQueue();
		public MediaQueue Videos { get; set; } = new MediaQueue();
		public Dictionary<string, MediaQueue> Playlists { get; set; } = new Dictionary<string, MediaQueue>();

		public async void ReadLibraryAsync()
		{
			await Task.Run(() => ReadLibrary());
		}
		public void ReadLibrary()
		{
			ReadMusicLibrary();
			ReadVideoLibrary();
		}

		public async void ReadMusicLibraryAsync()
		{
			await Task.Run(() => ReadMusicLibrary());
		}
		public void ReadMusicLibrary()
		{
			var songs = GetFiles(Environment.SpecialFolder.MyMusic);
			foreach (var item in songs)
				AddSong(item);
		}
		public void AddSong(string path)
		{
			if (Songs.Where(song => song.Path == path).Count() != 0)
				Songs.Add(new Song(path));
		}

		public async void ReadVideoLibraryAsync()
		{
			await Task.Run(() => ReadVideoLibrary());
		}
		public void ReadVideoLibrary()
		{
			var videos = GetFiles(Environment.SpecialFolder.MyVideos);
			foreach (var item in videos)
				AddSong(item);
		}
		public void AddVideo(string path)
		{
			if (Videos.Where(video => video.Path == path).Count() != 0)
				Videos.Add(new Video(path));
		}

		public void AddMedia(string path)
		{
			var media = MediaFactory.GetMedia(path);
			if (media is Song)
				Songs.Add(media);
			else if (media is Video)
				Videos.Add(media);
			else return;
		}

		private string[] GetFiles(Environment.SpecialFolder specialFolder)
		{
			return Directory.GetFiles(Environment.GetFolderPath(specialFolder));
		}
	}
}
