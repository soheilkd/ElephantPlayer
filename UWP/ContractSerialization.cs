using System.IO;
using System.Runtime.Serialization;

namespace Player
{
	public static class ContractSerialization
	{
		public static void Serialize<T>(string path, T obj)
		{
			var serializer = new DataContractSerializer(typeof(T));
			using (var stream = new FileStream(path, FileMode.Create))
				serializer.WriteObject(stream, obj);
		}

		public static T Deserialize<T>(string path)
		{
			if (!File.Exists(path))
				return default;
			var serializer = new DataContractSerializer(typeof(T));
			using (var stream = new FileStream(path, FileMode.Open))
				return (T)serializer.ReadObject(stream);
		}
	}
}
