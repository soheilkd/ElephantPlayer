using System;
using System.Windows.Input;

namespace Player.Taskbar
{
	public class Command : ICommand
	{
		#pragma warning disable CS0067 //Suppres never used warning
		public event EventHandler CanExecuteChanged;
		#pragma warning restore CS0067
		public event EventHandler Raised;
		public bool CanExecute(object parameter) => true;
		public void Execute(object parameter) => Raised?.Invoke(this, null);
	}
}
