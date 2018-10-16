using System;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Player.Extensions;

namespace Player.Models
{
	[Serializable]
	public class SerializableBitmap : ISerializable
	{
		[NonSerialized]
		private BitmapImage _Bitmap;
		private byte[] _Data;

		public SerializableBitmap() { }
		public SerializableBitmap(BitmapImage bitmap) => _Bitmap = bitmap;
		public SerializableBitmap(byte[] data)
		{
			_Data = data;
			_Bitmap = data.ToBitmap();
		}
		protected SerializableBitmap(SerializationInfo info, StreamingContext context)
		{
			_Data = (byte[])info.GetValue("_data", typeof(byte[]));
		}

		[OnSerializing]
		private void OnSerializing(StreamingContext context)
		{
			_Data = _Bitmap.ToData();
		}

		[OnDeserializing]
		private void OnDeserializing(StreamingContext context)
		{
			_Bitmap = _Data.ToBitmap();
		}

		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException();
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException();
		}

		//Implicit and explicit operators for easy assigning .net's bitmap
		public static implicit operator BitmapImage(SerializableBitmap bitmap) => bitmap._Bitmap;
		public static implicit operator byte[] (SerializableBitmap bitmap)
		{
			if (bitmap._Data == null)
				bitmap._Data = bitmap._Bitmap.ToData();
			return bitmap._Data;
		}
		public static explicit operator SerializableBitmap(BitmapImage bitmap) => new SerializableBitmap(bitmap);
	}
}