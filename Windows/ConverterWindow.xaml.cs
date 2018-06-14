using NReco.VideoConverter;
using System;
using System.Threading;
using System.Windows;

namespace Player.Controls
{
    public partial class ConverterWindow : Window
    {
        Media media;
        Thread ConverterThread;
        FFMpegConverter Converter = new FFMpegConverter();
        Action<Media> ActionOnDone;
        string ProperPath = "";

        public static void Convert(Media media, Action<Media> actionOnDone)
        {
            ConverterWindow window = new ConverterWindow();
            window.InitializeComponent();
            window.Converter.ConvertProgress += window.Converter_ConvertProgress;
            window.Converter.LogReceived += window.Converter_LogReceived;
            window.Expander1.Expanded += (_, __) => window.Height = 270;
            window.Expander1.Collapsed += (_, __) => window.Height = 105;
            window.ProperPath = $"{App.Path}Downloads\\{media.Name.Substring(0, media.Name.LastIndexOf("."))}.mp4";
            window.ConverterThread = new Thread(new ThreadStart(() =>
            window.Converter.ConvertMedia(media.Path, window.ProperPath, Format.mp4)))
            {
                IsBackground = false,
                Priority = ThreadPriority.Highest,
                Name = "ConverterThread"
            };
            window.Show();
            window.media = media;
            window.ConverterThread.Start();
        }

        private void Converter_LogReceived(object sender, FFMpegLogEventArgs e)
        {
            Dispatcher.Invoke(() => { TextBox1.AppendText(e.Data + "\r\n"); TextBox1.ScrollToEnd(); });
        }

        private void Converter_ConvertProgress(object sender, ConvertProgressEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                ProgressBar.Maximum = e.TotalDuration.TotalSeconds;
                ProgressBar.Value = e.Processed.TotalSeconds;
                Label1.Content = $"Converting {media.Title}...";
                Title = $"Converting... {(int)(e.Processed.TotalMilliseconds * 100 / e.TotalDuration.TotalMilliseconds)}%";
            });
            if (e.TotalDuration.Equals(e.Processed))
            {
                ActionOnDone.Invoke(new Media(ProperPath));
                Close();
                media = null;
                ConverterThread = null;
                Converter = null;
                ActionOnDone = null;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();
            Converter.Stop();
            ConverterThread.Abort();
            System.IO.File.Delete(ProperPath);
        }
    }
}
