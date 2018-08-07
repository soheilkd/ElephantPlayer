using Microsoft.Win32;
using System;
using System.Linq;

namespace Player
{
	public static class Dialogs
	{
		public static readonly string LibraryFilter = "Serialized Library (.bin)|*.bin";

		public static bool RequestFile(out string[] files, string filter, bool multiSelect = false)
		{
			var dialog = new OpenFileDialog()
			{
				Multiselect = multiSelect,
				Filter = filter
			};
			if (dialog.ShowDialog() ?? false)
			{
				files = dialog.FileNames.ToArray();
				return true;
			}
			else
			{
				files = new string[0];
				return false;
			}
		}

		public static bool RequestSave(out string output, string filter)
		{
			var dialog = new SaveFileDialog()
			{
				Filter = filter
			};
			if (dialog.ShowDialog() ?? false)
			{
				output = dialog.FileName;
				return true;
			}
			else
			{
				output = String.Empty;
				return false;
			}
		}
	}
}