using System;
using Player.Controls;
using System.Windows.Shell;
using Library.Controls;

namespace Player
{
	public class ThumbController
	{
		private TaskbarItemInfo _Info;

		private readonly ThumbButtonInfo _PlayThumb = new ThumbButtonInfo()
		{
			Description = "Play",
			ImageSource = IconProvider.GetBitmap(IconType.Play)
		};
		private readonly ThumbButtonInfo _PauseThumb = new ThumbButtonInfo()
		{
			Description = "Pause",
			ImageSource = IconProvider.GetBitmap(IconType.Pause)
		};
		private readonly ThumbButtonInfo _PreviousThumb = new ThumbButtonInfo()
		{
			Description = "Previous",
			ImageSource = IconProvider.GetBitmap(IconType.Previous)
		};
		private readonly ThumbButtonInfo _NextThumb = new ThumbButtonInfo()
		{
			Description = "Next",
			ImageSource = IconProvider.GetBitmap(IconType.Next)
		};

		public ThumbController(TaskbarItemInfo info, MediaPlayer player)
		{
			info.ThumbButtonInfos.Add(_PreviousThumb);
			info.ThumbButtonInfos.Add(_PlayThumb);
			info.ThumbButtonInfos.Add(_NextThumb);
			_PlayThumb.Click += (s, e) => player.PlayPause();
			_PauseThumb.Click += (s, e) => player.PlayPause();
			_PreviousThumb.Click += (s, e) => player.Previous();
			_NextThumb.Click += (s, e) => player.Next();
			_Info = info;
		}

		public void SetPlayingStateOnThumb(bool isPlaying)
		{
			_Info.ThumbButtonInfos[1] = isPlaying ? _PauseThumb : _PlayThumb;
		}
	}
}
