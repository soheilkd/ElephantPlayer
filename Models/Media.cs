using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Windows.Storage;

#pragma warning disable CS1998 //Disable async - await warning for ReadProperties

namespace Player.Models
{
	[Serializable]
	public abstract class Media : INotifyPropertyChanged
	{
		public List<DateTime> PlayTimes { get; set; } = new List<DateTime>();
		public DateTime AdditionDate { get; set; }
		public string Path { get; set; }
		public bool DoesExist => File.Exists(Path);
		public bool IsPlaying { get; set; }

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
