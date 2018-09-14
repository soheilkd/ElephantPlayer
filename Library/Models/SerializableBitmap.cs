using Player.Extensions;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Windows.Media.Imaging;

namespace Player.Models
{
	[Serializable]
	public class SerializableBitmap : ISerializable
	{
		[NonSerialized]
		private BitmapImage _source;
		private byte[] _data;

		public SerializableBitmap() { }
		public SerializableBitmap(BitmapImage source) : base() => _source = source;

		[OnSerializing]
		private void OnSerializing(StreamingContext context)
		{
			_data = GetBytes(_source);
		}

		[OnDeserializing]
		private void OnDeserializing(StreamingContext context)
		{
			_source = GetBitmap(_data);
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{

		}
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{

		}

		private static BitmapImage GetBitmap(byte[] data)
		{
			if (data == null || data.Length == 0)
				return new BitmapImage();
			using (MemoryStream memStream = new MemoryStream())
			{
				data.For(eachByte => memStream.WriteByte(eachByte));
				BitmapImage image = new BitmapImage();
				image.BeginInit();
				image.CacheOption = BitmapCacheOption.OnLoad;
				image.StreamSource = memStream;
				image.EndInit();
				return image;
			}
		}
		private static byte[] GetBytes(BitmapImage bitmapImage)
		{
			byte[] data;
			JpegBitmapEncoder encoder = new JpegBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
			using (MemoryStream ms = new MemoryStream())
			{
				encoder.Save(ms);
				data = ms.ToArray();
			}
			return data;
		}


		public static implicit operator BitmapImage(SerializableBitmap bitmap) => bitmap._source;
		public static explicit operator SerializableBitmap(BitmapImage bitmap) => new SerializableBitmap(bitmap);
	}
}