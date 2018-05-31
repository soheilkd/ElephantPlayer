using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Player;
using NReco.VideoConverter;
using Player.Events;
using System.Threading;

namespace Player.Controls
{
    /// <summary>
    /// Interaction logic for ConverterWindow.xaml
    /// </summary>
    public partial class ConverterWindow : Window
    {
        Thread ConverterThread;
        FFMpegConverter converter = new FFMpegConverter();
        Media media;
        public event EventHandler<InfoExchangeArgs> Done;
        public ConverterWindow(Media media) 
        {
            InitializeComponent();
            converter.ConvertProgress += Converter_ConvertProgress;
            ConverterThread = new Thread(new ThreadStart(ThreadOn))
            {
                IsBackground = false,
                Priority = ThreadPriority.Highest,
                Name = "ConverterThread"
            };
            Show();
            this.media = media;
            ConverterThread.Start();
        }
        private void ThreadOn()
        {
            converter.ConvertMedia(media.Path, $"{App.Path}Converted\\{media.Name.Substring(0, media.Name.LastIndexOf("."))}.mp4", Format.mp4);
            return;
        }
        private void Converter_ConvertProgress(object sender, ConvertProgressEventArgs e)
        {
            var perc = (int)(e.Processed.TotalMilliseconds * 100 / e.TotalDuration.TotalMilliseconds);
            Dispatcher.Invoke(() =>
            {
                ProgressBar.Maximum = e.TotalDuration.TotalSeconds;
                ProgressBar.Value = e.Processed.TotalSeconds;
                Label1.Content = $"Converting {media.Title}...";
                Title = $"Converting... {perc}%";
            });
            if (e.TotalDuration.Equals(e.Processed))
            {
                media = new Media($"{App.Path}Converted\\{media.Name.Substring(0, media.Name.LastIndexOf("."))}.mp4");
                Done.Invoke(this, new InfoExchangeArgs(InfoType.Media) { Object = media });
            }
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            converter.Stop();
            ConverterThread.Abort();
            Close();
        }
    }
}
