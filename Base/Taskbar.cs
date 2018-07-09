using MaterialDesignThemes.Wpf;
using System;
using System.Windows.Input;
using System.Windows.Shell;

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
	public class Thumb
	{
		public event EventHandler PlayPressed;
		public event EventHandler PausePressed;
		public event EventHandler PrevPressed;
		public event EventHandler NextPressed;

		public ThumbButtonInfo PlayThumb = new ThumbButtonInfo()
		{
			Description = "Play",
			ImageSource = Images.GetBitmap(PackIconKind.Play)
		};
		public ThumbButtonInfo PauseThumb = new ThumbButtonInfo()
		{
			Description = "Pause",
			ImageSource = Images.GetBitmap(PackIconKind.Pause)
		};
		public ThumbButtonInfo PreviousThumb = new ThumbButtonInfo()
		{
			Description = "Previous",
			ImageSource = Images.GetBitmap(PackIconKind.SkipPrevious)
		};
		public ThumbButtonInfo NextThumb = new ThumbButtonInfo()
		{
			Description = "Next",
			ImageSource = Images.GetBitmap(PackIconKind.SkipNext)
		};

		private Command PlayHandler = new Command();
		private Command PauseHandler = new Command();
		private Command PrevHandler = new Command();
		private Command NextHandler = new Command();
		public TaskbarItemInfo Info { get; } = new TaskbarItemInfo();

		public Thumb()
		{
			PlayHandler.Raised += (sender, e) => PlayPressed?.Invoke(sender, e);
			PauseHandler.Raised += (sender, e) => PausePressed?.Invoke(sender, e);
			PrevHandler.Raised += (sender, e) => PrevPressed?.Invoke(sender, e);
			NextHandler.Raised += (sender, e) => NextPressed?.Invoke(sender, e);
			PreviousThumb.Command = PrevHandler;
			NextThumb.Command = NextHandler;
			PlayThumb.Command = PlayHandler;
			PauseThumb.Command = PauseHandler;

			Info.ThumbButtonInfos.Clear();
			Info.ThumbButtonInfos.Add(PreviousThumb);
			Info.ThumbButtonInfos.Add(PlayThumb);
			Info.ThumbButtonInfos.Add(NextThumb);
		}

		public void SetPlayingState(bool IsPlaying = false) => Info.ThumbButtonInfos[1] = IsPlaying ? PauseThumb : PlayThumb;
		public void SetProgressState(TaskbarItemProgressState state) => Info.ProgressState = state;
		public void SetProgressValue(double value) => Info.ProgressValue = value;
	}
}
