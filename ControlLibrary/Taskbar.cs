using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;

namespace Player.Controls.Taskbar
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
			ImageSource = IconKind.Play.GetBitmap()
		};
		public ThumbButtonInfo PauseThumb = new ThumbButtonInfo()
		{
			Description = "Pause",
			ImageSource = IconKind.Pause.GetBitmap()
		};
		public ThumbButtonInfo PreviousThumb = new ThumbButtonInfo()
		{
			Description = "Previous",
			ImageSource = IconKind.SkipPrevious.GetBitmap()
		};
		public ThumbButtonInfo NextThumb = new ThumbButtonInfo()
		{
			Description = "Next",
			ImageSource = IconKind.SkipNext.GetBitmap()
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

		public static JumpItem[] GetJumps(Application app)
		{
			return JumpList.GetJumpList(app).JumpItems.ToArray();
			
		}
	}
}
