using Library;
using Player.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
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
		private static string LibraryPath => Local.Path + "\\Library.bin";

		#region Library 

		public static Models.Library Library { get; } = ContractSerialization.Deserialize<Models.Library>(LibraryPath) ?? new Models.Library();

		#endregion

		public static void SaveAll()
		{
			ContractSerialization.Serialize(LibraryPath, Library);
		}
	}
}
