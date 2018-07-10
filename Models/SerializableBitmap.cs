using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Windows.Media.Imaging;

namespace Player.Models
{
	[Serializable]
	public class SerializableBitmap: ISerializable
	{
		[NonSerialized]
		private BitmapImage _source;
		private byte[] _data;

		[OnSerializing]
		private void OnSerializing(StreamingContext context)
		{
			_data = Images.GetBytes(_source);
		}
		[OnDeserializing]
		private void OnDeserializing(StreamingContext context)
		{
			_source = Images.GetBitmap(_data);
		}

		public void Set(BitmapImage value)
		{
			_source = value;
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			
		}
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("n1", n1);
		}

		public static implicit operator BitmapImage(SerializableBitmap bitmap) => bitmap._source;

		public SerializableBitmap() { }
		public SerializableBitmap(BitmapImage source) : base() => _source = source;
	}
}
