namespace EPlayer.Library.Models
{
	public interface IMediaLoader
	{
		Media GetMedia(string filePath);
		Song GetSong(string filePath);
		Video GetVideo(string filePath);
	}
}
