using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Player.Controllers
{
	public static class ResourceController
	{
		public static string BinaryPath => $"{Settings.AppPath}Resources.rsc";

		private static Lazy<Dictionary<string, byte[]>> _LazyResources = new Lazy<Dictionary<string, byte[]>>(Load);

		private static Dictionary<string, byte[]> Resources
		{
			get => _LazyResources.Value;
		}

		public static void Save()
		{
			if (!_LazyResources.IsValueCreated)
				return; //Return without doing anything to prevent unneccessary I/O
			using (var stream = new FileStream(BinaryPath, FileMode.Create))
				new BinaryFormatter().Serialize(stream, Resources);
		}

		private static Dictionary<string, byte[]> Load()
		{
			using (var stream = new FileStream(BinaryPath, FileMode.Open))
				return (Dictionary<string, byte[]>)new BinaryFormatter().Deserialize(stream);
		}

		public static bool Contains(byte[] data)
		{
			return Resources.ContainsValue(data);
		}
		public static bool Contains(byte[] data, out string key)
		{
			if (Resources.ContainsValue(data))
			{
				key = Resources.Where(pairKey => pairKey.Value == data).First().Key;
				return true;
			}
			else
			{
				key = default;
				return false;
			}
		}
		public static bool Contains(string key)
		{
			return Resources.ContainsKey(key);
		}

		public static bool Contains(string key, out byte[] data)
		{
			return Resources.TryGetValue(key, out data);
		}
		public static void Add(string key, byte[] data)
		{
			Resources.Add(key, data);
		}
		public static byte[] Get(string key)
		{
			if (!Contains(key))
				return default;
			return Resources[key];
		}
		public static void Set(string key, byte[] data)
		{
			Resources[key] = data;
		}
		public static void AddOrSet(string key, byte[] data)
		{
			if (Contains(key))
				Resources[key] = data;
			else
				Resources.Add(key, data);
		}
		public static bool Remove(string key)
		{
			return Resources.Remove(key);
		}
	}
}
