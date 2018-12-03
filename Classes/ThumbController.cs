using System;
using System.Windows.Shell;
using Library.Controls;

namespace Player
{
	public class ThumbController
	{
		private TaskbarItemInfo _Info;

		public event EventHandler PlayPauseClicked;
		public event EventHandler PreviousClicked;
		public event EventHandler NextClicked;

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

		public ThumbController(TaskbarItemInfo info)
		{
			info.ThumbButtonInfos.Add(_PreviousThumb);
			info.ThumbButtonInfos.Add(_PlayThumb);
			info.ThumbButtonInfos.Add(_NextThumb);
			_PlayThumb.Click += (s, e) => PlayPauseClicked?.Invoke(this, default);
			_PauseThumb.Click += (s, e) => PlayPauseClicked?.Invoke(this, default);
			_PreviousThumb.Click += (s, e) => PreviousClicked?.Invoke(this, default);
			_NextThumb.Click += (s, e) => NextClicked?.Invoke(this, default);
			_Info = info;
		}

		public void SetPlayingStateOnThumb(bool isPlaying)
		{
			_Info.ThumbButtonInfos[1] = isPlaying ? _PauseThumb : _PlayThumb;
		}
	}
}
