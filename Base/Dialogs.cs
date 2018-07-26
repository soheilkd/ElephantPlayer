using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Player
{
	public static class Dialogs
    {
		public static readonly Collection<CommonFileDialogFilter> DefaultLibraryFilter = 
			new Collection<CommonFileDialogFilter>(new[]
			{
				new CommonFileDialogFilter("Binary File", "*.bin")
			});

		public static bool RequestFolder(out string[] folders, bool multiSelect = true)
		{
			var dialog = new CommonOpenFileDialog()
			{
				IsFolderPicker = true,
				Multiselect = multiSelect
			};
			if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
			{
				folders = dialog.FileNames.ToArray();
				return true;
			}
			else
			{
				folders = new string[0];
				return false;
			}
		}
		public static bool RequestFile(out string[] files, IList<CommonFileDialogFilter> filters, bool multiSelect = false)
		{
			var dialog = new CommonOpenFileDialog()
			{
				Multiselect = multiSelect
			};
			filters.For(each => dialog.Filters.Add(each));
			if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
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

		public static bool RequestSave(out string output, IList<CommonFileDialogFilter> filters)
		{
			var dialog = new CommonSaveFileDialog();
			filters.For(each => dialog.Filters.Add(each));
			if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
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

		public static CommonFileDialogFilter GetFilter(string name, string ext)
			   => new CommonFileDialogFilter(name, ext);
    }
}