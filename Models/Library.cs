using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace Player.Models
{
	[Serializable]
	public class Library
	{
		public MediaQueue<Song> Songs { get; set; } = new MediaQueue<Song>();
		public MediaQueue<Video> Videos { get; set; } = new MediaQueue<Video>();
		public List<MediaQueue> Playlists { get; set; } = new List<MediaQueue>();

		public async Task ReadMusicLibrary()
		{
			StorageLibrary musics = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);
			foreach (StorageFolder folder in musics.Folders)
				await AddMusicFolder(folder);
		}
		public async Task AddMusicFolder(StorageFolder folder)
		{
			foreach (var subitem in await folder.GetItemsAsync())
			{
				if (subitem is StorageFolder subfolder)
					await AddMusicFolder(subfolder);
				else if (subitem is StorageFile file)
					AddMusic(file);
			}
		}
		public void AddMusic(StorageFile file)
		{
			if (!Songs.Contains(file.Path))
				Songs.Add(new Song(file));
		}

		public async Task ReadVideoLibrary()
		{
			StorageLibrary videos = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Videos);
			foreach (StorageFolder folder in videos.Folders)
				await AddVideoFolder(folder);
		}
		public async Task AddVideoFolder(StorageFolder folder)
		{
			foreach (var subitem in await folder.GetItemsAsync())
			{
				if (subitem is StorageFolder subfolder)
					await AddVideoFolder(subfolder);
				else if (subitem is StorageFile file)
					AddVideo(file);
			}
		}
		public void AddVideo(StorageFile file)
		{
			if (!Videos.Contains(file.Path))
				Videos.Add(new Video(file));
		}

		public async void ReadLibrary()
		{
			await ReadMusicLibrary();
			await ReadVideoLibrary();
		}
	}
}
