using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using Windows.Storage;

#pragma warning disable CS1998 //Disable async - await warning for ReadProperties

namespace Player.Models
{
	[DataContract]
	public abstract class Media : INotifyPropertyChanged
	{
		[DataMember]
		public List<DateTime> PlayTimes { get; set; } = new List<DateTime>();
		[DataMember]
		public DateTime AdditionDate { get; private set; }
		[DataMember]
		public string Title { get; protected set; }
		[DataMember]
		public TimeSpan Duration { get; protected set; }
		[DataMember]
		public string Path { get; set; }
		public bool DoesExist => File.Exists(Path);
		[IgnoreDataMember]
		public bool IsPlaying { get; set; }
		[field:IgnoreDataMember]
		public event PropertyChangedEventHandler PropertyChanged;

		public Media() => AdditionDate = DateTime.Now;
		public Media(string path)
		{
			Path = path;
			ReadProperties(path);
		}
		public Media(StorageFile file)
		{
			Path = file.Path;
			ReadProperties(file);
		}

		public virtual bool MatchesQuery(string query)
		{
			throw new NotImplementedException();
		}

		protected virtual async void ReadProperties(StorageFile file)
		{
			throw new NotImplementedException();
		}
		protected virtual async void ReadProperties(string path)
		{
			throw new NotImplementedException();
		}

		protected void NotifyPropertyChange<T>(T field)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(field)));
		}
		protected void SetProperty<T>(T field, T value)
		{
			field = value;
			NotifyPropertyChange(field);
		}

	}
}
