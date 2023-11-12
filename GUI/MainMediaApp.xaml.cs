using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using media_player_windows.classes;
using Microsoft.Win32;

namespace media_player_windows.GUI
{
    /// <summary>
    /// Interaction logic for MainMediaApp.xaml
    /// </summary>
    public partial class MainMediaApp : Window
    {
        media_player_windows.classes.MyClasses myClasses = new MyClasses();

        private bool mediaPlayerIsPlaying = false;
        private bool userIsDraggingSlider = false;

        public MainMediaApp()
        {
            InitializeComponent();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();

            this.DataContext = myClasses;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            myClasses.audioMode = "PlayCircle";
            myClasses.replayMode = "White";
            myClasses.shuffleMode = "White";
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if ((mePlayer.Source != null) && (mePlayer.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
            {
                sliProgress.Minimum = 0;
                sliProgress.Maximum = mePlayer.NaturalDuration.TimeSpan.TotalSeconds;
                sliProgress.Value = mePlayer.Position.TotalSeconds;
            }
        }

        private void audioModeBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(myClasses.audioMode == "PlayCircle")
            {
                myClasses.audioMode = "PauseCircle";
                mePlayer.Play();
            }
            else if(myClasses.audioMode == "PauseCircle")
            {
                myClasses.audioMode = "PlayCircle";
                mePlayer.Pause();
            }
        }

        private void mePlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            // Replay mode is true
            if(myClasses.replayMode == "#27cc4d")
            {
                mePlayer.Position = TimeSpan.Zero;
                mePlayer.Play();
            }

            // Shuffle mode is true
            if (myClasses.shuffleMode == "#27cc4d")
            {
                int index = myClasses.videos.FindIndex(a => a.url == myClasses.targetVideoUrl);

                // Get next media url
                if(index != (myClasses.videos.Count() - 1)) myClasses.targetVideoUrl = myClasses.videos[index + 1].url;
                else myClasses.targetVideoUrl = myClasses.videos[0].url;

                // Set video duration
                setMediaDuration(myClasses.targetVideoUrl);

                // Set media source
                mePlayer.Source = new Uri(myClasses.targetVideoUrl);
                thumnailPlayer.Source = new Uri(myClasses.targetVideoUrl);
                mePlayer.Play();
            }
        }

        private void setMediaDuration(string videoUrl)
        {
            var tfile = TagLib.File.Create(videoUrl);
            StringBuilder sb = new();
            sb.AppendLine(tfile.Properties.Duration.ToString(@"hh\:mm\:ss"));
            lblTimeStatus.Text = sb.ToString();
        }

        private void addPlaylistBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Media Files (*.mp3,*.mp4)|*.mp3;*.mp4";
            open.Multiselect = true;
            open.Title = "Open Text Files";

            if (open.ShowDialog() == true)
            {
                myClasses.videos.Clear();

                foreach (String fileName in open.FileNames)
                {
                    var names = fileName.Split("\\");
                    var nameWithExts = names[names.Length - 1];

                    var nameWithExt = nameWithExts.Split(".");
                    var name = nameWithExt[0];

                    var tfile = TagLib.File.Create(fileName);
                    StringBuilder sb = new();
                    sb.AppendLine(tfile.Properties.Duration.ToString(@"hh\:mm\:ss"));
                    var duration = sb.ToString();

                    myClasses.videos.Add(new media_player_windows.classes.Video() { url = fileName, name = name, duration = duration });
                }

                myClasses.targetVideoUrl = myClasses.videos[0].url;

                mePlayer.Source = new Uri(myClasses.targetVideoUrl);
                thumnailPlayer.Source = new Uri(myClasses.targetVideoUrl);
                thumnailPlayer.Volume = 0;
                thumnailPlayer.ScrubbingEnabled = true;

                setMediaDuration(myClasses.targetVideoUrl);

                mePlayer.Play();

                mediaPlayerIsPlaying = true;

                playlistListView.ItemsSource = myClasses.videos;

                myClasses.audioMode = "PauseCircle";
            }
        }

        private void playlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (ListBox)sender;
            var video = (media_player_windows.classes.Video)item.SelectedItem;

            myClasses.targetVideoUrl = video.url;
            mePlayer.Source = new Uri(myClasses.targetVideoUrl);
            thumnailPlayer.Source = new Uri(myClasses.targetVideoUrl);

            setMediaDuration(myClasses.targetVideoUrl);
        }

        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }

        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            mePlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);
        }

        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lblProgressStatus.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"hh\:mm\:ss");

            if (userIsDraggingSlider)
            {
                thumnailPlayer.Width = 150;
                thumnailPlayer.Height = 80;
                thumnailPlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);

                //Debug.WriteLine(thumnailPlayer.Position.TotalSeconds, "thumnailPlayer");

                thumnailPlayer.Play();
                thumnailPlayer.Stop();
            }
            else
            {
                thumnailPlayer.Width = 0;
                thumnailPlayer.Height = 0;
            }
        }

        private void replayModeBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (myClasses.replayMode == "#27cc4d")
            {
                myClasses.replayMode = "White";
            }
            else if (myClasses.replayMode == "White")
            {
                myClasses.replayMode = "#27cc4d";
                myClasses.shuffleMode = "White";
            }
        }

        private void shuffleModeBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (myClasses.shuffleMode == "#27cc4d")
            {
                myClasses.shuffleMode = "White";
            }
            else if (myClasses.shuffleMode == "White")
            {
                myClasses.shuffleMode = "#27cc4d";
                myClasses.replayMode = "White";
            }
        }

        private void skipNextBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int index = myClasses.videos.FindIndex(a => a.url == myClasses.targetVideoUrl);

            // Get next media url
            if (index != 0) myClasses.targetVideoUrl = myClasses.videos[index - 1].url;
            else myClasses.targetVideoUrl = myClasses.videos[myClasses.videos.Count - 1].url;

            // Set video duration
            setMediaDuration(myClasses.targetVideoUrl);

            // Set media source
            mePlayer.Source = new Uri(myClasses.targetVideoUrl);
            thumnailPlayer.Source = new Uri(myClasses.targetVideoUrl);
            mePlayer.Play();
        }

        private void skipPreviousBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int index = myClasses.videos.FindIndex(a => a.url == myClasses.targetVideoUrl);

            // Get next media url
            if (index != (myClasses.videos.Count() - 1)) myClasses.targetVideoUrl = myClasses.videos[index + 1].url;
            else myClasses.targetVideoUrl = myClasses.videos[0].url;

            // Set video duration
            setMediaDuration(myClasses.targetVideoUrl);

            // Set media source
            mePlayer.Source = new Uri(myClasses.targetVideoUrl);
            thumnailPlayer.Source = new Uri(myClasses.targetVideoUrl);
            mePlayer.Play();
        }
    }
}
