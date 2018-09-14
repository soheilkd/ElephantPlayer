using System;
using System.Windows.Shell;
using Player.Controls;

namespace Player.Taskbar
{
	public class ThumbManager
	{
		public event EventHandler PlayClicked;
		public event EventHandler PauseClicked;
		public event EventHandler PrevClicked;
		public event EventHandler NextClicked;

		private ThumbButtonInfo PlayThumb = new ThumbButtonInfo()
		{
			Description = "Play",
			ImageSource = IconProvider.GetBitmap(IconType.Play)
		};
		private ThumbButtonInfo PauseThumb = new ThumbButtonInfo()
		{
			Description = "Pause",
			ImageSource = IconProvider.GetBitmap(IconType.Pause)
		};
		private ThumbButtonInfo PreviousThumb = new ThumbButtonInfo()
		{
			Description = "Previous",
			ImageSource = IconProvider.GetBitmap(IconType.Previous)
		};
		private ThumbButtonInfo NextThumb = new ThumbButtonInfo()
		{
			Description = "Next",
			ImageSource = IconProvider.GetBitmap(IconType.Next)
		};

		private Command PlayHandler = new Command();
		private Command PauseHandler = new Command();
		private Command PrevHandler = new Command();
		private Command NextHandler = new Command();
		public TaskbarItemInfo Info { get; } = new TaskbarItemInfo();

		public ThumbManager()
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
