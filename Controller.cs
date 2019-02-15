using Library;
using Library.Serialization.Models;
using Player.Models;
using System.Collections.Generic;

namespace Player
{
	public static class Controller
	{
		public static string AppPath => App.Path;

		#region Player Related
		public static MediaQueue Queue { get; set; } = new MediaQueue();

		public static event InfoExchangeHandler<(MediaQueue, Media)> PlayRequest;

		/// <summary>
		/// Plays the specified <paramref name="media"/>
		/// </summary>
		/// <param name="media">The media to play</param>
		/// <param name="queue">The queue that media is in. If not specified, Library is used</param>
		public static void Play(Media media, MediaQueue queue = default)
		{
			PlayRequest?.Invoke(default, (queue ?? Library, media));
		}

		public static void Play(string path)
		{
			var media = Library.Add(path);
			if (media != default)
				Play(media);
		}
		public static void Play(string[] paths)
		{
			Media latestMedia = default;
			Media currentMedia = default;
			foreach (var item in paths)
				if ((currentMedia = Library.Add(item)) != default)
					latestMedia = currentMedia;
			if (latestMedia != default)
				Play(latestMedia);
		}

		#endregion

		#region Public Settings
		
		private static LazyXml<SerializableSettings> _Settings = new LazyXml<SerializableSettings>($"{AppPath}Settings.xml");
		public static SerializableSettings Settings => _Settings.Value;

		public static void SaveSettings()
		{
			_Settings.Save();
		}

		#endregion

		#region Resources 

		private static LazySerializable<Dictionary<string, byte[]>> _Resource = new LazySerializable<Dictionary<string, byte[]>>($"{AppPath}Resources.rsc");
		public static Dictionary<string, byte[]> Resource => _Resource.Value;

		#endregion

		#region Library 

		private static LazySerializable<Models.Library> _Library = new LazySerializable<Models.Library>($"{AppPath}Library.bin");
		public static Models.Library Library=> _Library.Value;

		#endregion

		public static void SaveAll()
		{
			_Settings.Save();
			_Resource.Save();
			_Library.Save();
		}
	}
}
