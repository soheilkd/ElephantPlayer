using Library;
using Player.Models;
using System.Collections.Generic;
using System.Windows;
using Windows.Foundation;
using Windows.Storage;
namespace Player
{
	public static class Controller
	{
		#region Player Related
		public static MediaQueue Queue { get; set; } = new MediaQueue();
		
		public static event TypedEventHandler<MediaQueue, Media> PlayRequest;

		public static void Play(Media media, MediaQueue queue = default)
		{
			PlayRequest?.Invoke(queue, media);
		}

		public static async void Play(string path)
		{

		}

		#endregion

		public static ApplicationDataContainer Settings => ApplicationData.Current.LocalSettings;
		public static StorageFolder Local => ApplicationData.Current.LocalFolder;

		#region Library 

		static Controller()
		{
			
			Library.ReadLibrary();
		}

		public static Models.Library Library { get; } = Serialization.Deserialize<Models.Library>(Local.Path + "Library.bin")  ?? new Models.Library();

		#endregion

		public static void SaveAll()
		{
			Serialization.Serialize(Library, Local.Path + "Library.bin");
		}
	}
}
