using System;
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
		public event EventHandler PlayClicked;
		public event EventHandler PauseClicked;
		public event EventHandler PrevClicked;
		public event EventHandler NextClicked;

		private ThumbButtonInfo PlayThumb = new ThumbButtonInfo()
		{
			Description = "Play",
			ImageSource = IconKind.Play.GetBitmap()
		};
		private ThumbButtonInfo PauseThumb = new ThumbButtonInfo()
		{
			Description = "Pause",
			ImageSource = IconKind.Pause.GetBitmap()
		};
		private ThumbButtonInfo PreviousThumb = new ThumbButtonInfo()
		{
			Description = "Previous",
			ImageSource = IconKind.SkipPrevious.GetBitmap()
		};
		private ThumbButtonInfo NextThumb = new ThumbButtonInfo()
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
			PlayHandler.Raised += (sender, e) => PlayClicked?.Invoke(sender, e);
			PauseHandler.Raised += (sender, e) => PauseClicked?.Invoke(sender, e);
			PrevHandler.Raised += (sender, e) => PrevClicked?.Invoke(sender, e);
			NextHandler.Raised += (sender, e) => NextClicked?.Invoke(sender, e);
			PreviousThumb.Command = PrevHandler;
			NextThumb.Command = NextHandler;
			PlayThumb.Command = PlayHandler;
			PauseThumb.Command = PauseHandler;

			Info.ThumbButtonInfos.Clear();
			Info.ThumbButtonInfos.Add(PreviousThumb);
			Info.ThumbButtonInfos.Add(PlayThumb);
			Info.ThumbButtonInfos.Add(NextThumb);
		}

		public void SetPlayingState(bool isPlaying) 
			=> Info.ThumbButtonInfos[1] = isPlaying ? PauseThumb : PlayThumb;
	}
}
