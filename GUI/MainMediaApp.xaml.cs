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

            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);
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
                if(index != (myClasses.videos.Count() - 1))
                {
                    myClasses.targetVideoUrl = myClasses.videos[index + 1].url;
                    myClasses.targetVideoName = myClasses.videos[index + 1].name;
                    myClasses.targetVideoAuthor = myClasses.videos[index + 1].author;
                }
                else
                {
                    myClasses.targetVideoUrl = myClasses.videos[0].url;
                    myClasses.targetVideoName = myClasses.videos[0].name;
                    myClasses.targetVideoAuthor = myClasses.videos[0].author;

                }

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
                playlistListView.ItemsSource = "";

                foreach (String fileName in open.FileNames)
                {
                    var names = fileName.Split("\\");
                    var nameWithExts = names[names.Length - 1];

                    var nameWithExt = nameWithExts.Split(".");

                    var name = nameWithExt[0].Split("-")[0];
                    var author = nameWithExt[0].Split("-")[1];

                    var tfile = TagLib.File.Create(fileName);
                    StringBuilder sb = new();
                    sb.AppendLine(tfile.Properties.Duration.ToString(@"hh\:mm\:ss"));
                    var duration = sb.ToString();

                    myClasses.videos.Add(new media_player_windows.classes.Video()
                    { 
                        url = fileName,
                        name = name,
                        author = author,
                        duration = duration
                    });
                }

                myClasses.targetVideoUrl = myClasses.videos[0].url;
                myClasses.targetVideoName = myClasses.videos[0].name;
                myClasses.targetVideoAuthor = myClasses.videos[0].author;


                mePlayer.Source = new Uri(myClasses.targetVideoUrl);
                thumnailPlayer.Source = new Uri(myClasses.targetVideoUrl);
                thumnailPlayer.Volume = 0;
                thumnailPlayer.ScrubbingEnabled = true;

                setMediaDuration(myClasses.targetVideoUrl);

                mePlayer.Play();

                mediaPlayerIsPlaying = true;

                playlistListView.ItemsSource = myClasses.videos;

                myClasses.audioMode = "PauseCircle";

                sliVolume.Value = 5.0;
                var setValue = 0.1 * sliVolume.Value;
                mePlayer.Volume = setValue;
            }
        }

        private void playlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (ListBox)sender;
            var video = (media_player_windows.classes.Video)item.SelectedItem;

            if(video != null)
            {
                myClasses.targetVideoUrl = video.url;
                myClasses.targetVideoName = video.name;
                myClasses.targetVideoAuthor = video.author;

                mePlayer.Source = new Uri(myClasses.targetVideoUrl);
                thumnailPlayer.Source = new Uri(myClasses.targetVideoUrl);

                setMediaDuration(myClasses.targetVideoUrl);
            }
            else if (video == null && myClasses.videos.Count != 0)
            {
                myClasses.targetVideoUrl = myClasses.videos[0].url;
                myClasses.targetVideoName = myClasses.videos[0].name;
                myClasses.targetVideoAuthor = myClasses.videos[0].author;

                mePlayer.Source = new Uri(myClasses.targetVideoUrl);
                thumnailPlayer.Source = new Uri(myClasses.targetVideoUrl);

                setMediaDuration(myClasses.targetVideoUrl);
            }
            else if (video == null && myClasses.videos.Count == 0)
            {
                myClasses.targetVideoUrl = "";
                myClasses.targetVideoName = "";
                myClasses.targetVideoAuthor = "";

                mePlayer.Stop();
                mePlayer.Position = TimeSpan.Zero;

                thumnailPlayer.Stop();
                thumnailPlayer.Position = TimeSpan.Zero;
            }        
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
            if (index != 0)
            {
                myClasses.targetVideoUrl = myClasses.videos[index - 1].url;
                myClasses.targetVideoName = myClasses.videos[index - 1].name;
                myClasses.targetVideoAuthor = myClasses.videos[index - 1].author;
            }
            else
            {
                myClasses.targetVideoUrl = myClasses.videos[myClasses.videos.Count - 1].url;
                myClasses.targetVideoName = myClasses.videos[myClasses.videos.Count - 1].name;
                myClasses.targetVideoAuthor = myClasses.videos[myClasses.videos.Count - 1].author;
            }

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
            if (index != (myClasses.videos.Count() - 1))
            {
                myClasses.targetVideoUrl = myClasses.videos[index + 1].url;
                myClasses.targetVideoName = myClasses.videos[index + 1].name;
                myClasses.targetVideoAuthor = myClasses.videos[index + 1].author;
            }
            else
            {
                myClasses.targetVideoUrl = myClasses.videos[0].url;
                myClasses.targetVideoName = myClasses.videos[0].name;
                myClasses.targetVideoAuthor = myClasses.videos[0].author;
            }

            // Set video duration
            setMediaDuration(myClasses.targetVideoUrl);

            // Set media source
            mePlayer.Source = new Uri(myClasses.targetVideoUrl);
            thumnailPlayer.Source = new Uri(myClasses.targetVideoUrl);
            mePlayer.Play();
        }

        private void removeMediaBtn_Click(object sender, RoutedEventArgs e)
        {
            List<media_player_windows.classes.Video> filteredList = new List<media_player_windows.classes.Video>();

            for(var i = 0; i < myClasses.videos.Count(); i++)
            {
                if (myClasses.videos[i].url != myClasses.targetVideoUrl)
                {
                    filteredList.Add(myClasses.videos[i]);
                }       
            }

            if(filteredList.Count > 0)
            {
                myClasses.targetVideoUrl = filteredList[0].url;
                myClasses.targetVideoName = filteredList[0].name;
                myClasses.targetVideoAuthor = filteredList[0].author;

                myClasses.videos = filteredList;
                playlistListView.ItemsSource = filteredList;
            }
            else
            {
                myClasses.targetVideoUrl = "";
                myClasses.targetVideoName = "";
                myClasses.targetVideoAuthor = "";

                myClasses.videos.Clear();
                playlistListView.ItemsSource = "";
            }          
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            mePlayer.Volume += (e.Delta > 0) ? 0.1 : -0.1;
        }

        private void sliVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var setValue = 0.1 * sliVolume.Value;
            mePlayer.Volume = setValue;
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key.ToString() == "Space")
            {
                if (myClasses.audioMode == "PlayCircle")
                {
                    myClasses.audioMode = "PauseCircle";
                    mePlayer.Play();
                }
                else if (myClasses.audioMode == "PauseCircle")
                {
                    myClasses.audioMode = "PlayCircle";
                    mePlayer.Pause();
                }
            }
            else if(e.Key.ToString() == "Right")
            {
                int index = myClasses.videos.FindIndex(a => a.url == myClasses.targetVideoUrl);

                // Get next media url
                if (index != 0)
                {
                    myClasses.targetVideoUrl = myClasses.videos[index - 1].url;
                    myClasses.targetVideoName = myClasses.videos[index - 1].name;
                    myClasses.targetVideoAuthor = myClasses.videos[index - 1].author;
                }
                else
                {
                    myClasses.targetVideoUrl = myClasses.videos[myClasses.videos.Count - 1].url;
                    myClasses.targetVideoName = myClasses.videos[myClasses.videos.Count - 1].name;
                    myClasses.targetVideoAuthor = myClasses.videos[myClasses.videos.Count - 1].author;
                }

                // Set video duration
                setMediaDuration(myClasses.targetVideoUrl);

                // Set media source
                mePlayer.Source = new Uri(myClasses.targetVideoUrl);
                thumnailPlayer.Source = new Uri(myClasses.targetVideoUrl);
                mePlayer.Play();
            }
            else if (e.Key.ToString() == "Left")
            {
                int index = myClasses.videos.FindIndex(a => a.url == myClasses.targetVideoUrl);

                // Get next media url
                if (index != (myClasses.videos.Count() - 1))
                {
                    myClasses.targetVideoUrl = myClasses.videos[index + 1].url;
                    myClasses.targetVideoName = myClasses.videos[index + 1].name;
                    myClasses.targetVideoAuthor = myClasses.videos[index + 1].author;
                }
                else
                {
                    myClasses.targetVideoUrl = myClasses.videos[0].url;
                    myClasses.targetVideoName = myClasses.videos[0].name;
                    myClasses.targetVideoAuthor = myClasses.videos[0].author;
                }

                // Set video duration
                setMediaDuration(myClasses.targetVideoUrl);

                // Set media source
                mePlayer.Source = new Uri(myClasses.targetVideoUrl);
                thumnailPlayer.Source = new Uri(myClasses.targetVideoUrl);
                mePlayer.Play();
            }
            else if(e.Key.ToString() == "R")
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
        }
    }
}
