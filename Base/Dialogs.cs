using Microsoft.Win32;
using System.Linq;

namespace Player
{
	public static class Dialogs
	{
		public const string LibraryFilter = "Serialized Library |*.bin";

		public static bool RequestFile(out string[] files, string filter, bool multiSelect = false)
		{
			OpenFileDialog dialog = new OpenFileDialog()
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
			SaveFileDialog dialog = new SaveFileDialog()
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
				output = string.Empty;
				return false;
			}
		}
	}
}