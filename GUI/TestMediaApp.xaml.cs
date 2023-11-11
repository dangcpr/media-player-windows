using media_player_windows.classes;
using Microsoft.Win32;
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

namespace media_player_windows.GUI
{
    /// <summary>
    /// Interaction logic for TestMediaApp.xaml
    /// </summary>
    public partial class TestMediaApp : Window
    {
        media_player_windows.classes.MyClasses myClasses = new MyClasses();

        private bool mediaPlayerIsPlaying = false;
        private bool userIsDraggingSlider = false;

        public TestMediaApp()
        {
            InitializeComponent();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = myClasses;
        }

        private void lstBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            var item = (ListBox)sender;
            var video = (media_player_windows.classes.Video)item.SelectedItem;

            myClasses.targetVideoUrl = video.url;
            mePlayer.Source = new Uri(myClasses.targetVideoUrl);
            thumnailPlayer.Source = new Uri(myClasses.targetVideoUrl);
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

        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Media Files (*.mp3,*.mp4)|*.mp3;*.mp4";

            if (openFileDialog.ShowDialog() == true)
            {
                mePlayer.Source = new Uri(openFileDialog.FileName);
                Debug.WriteLine(openFileDialog.FileName);

                mePlayer.Play();
                mediaPlayerIsPlaying = true;
            }
        }

        private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (mePlayer != null) && (mePlayer.Source != null);
        }

        private void Play_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mePlayer.Play();
            mediaPlayerIsPlaying = true;
        }

        private void Pause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayerIsPlaying;
        }

        private void Pause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mePlayer.Pause();
        }

        private void Stop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayerIsPlaying;
        }

        private void Stop_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mePlayer.Stop();
            mediaPlayerIsPlaying = false;
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

                Debug.WriteLine(thumnailPlayer.Position.TotalSeconds, "thumnailPlayer");

                thumnailPlayer.Play();
                thumnailPlayer.Stop();
            }
            else
            {
                thumnailPlayer.Width = 0;
                thumnailPlayer.Height = 0;
            }
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            mePlayer.Volume += (e.Delta > 0) ? 0.1 : -0.1;
        }

        private void handleOpenVideos(object sender, RoutedEventArgs e)
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

                    myClasses.videos.Add(new media_player_windows.classes.Video() { url = fileName, name = name });
                }

                myClasses.targetVideoUrl = myClasses.videos[0].url;

                mePlayer.Source = new Uri(myClasses.targetVideoUrl);
                thumnailPlayer.Source = new Uri(myClasses.targetVideoUrl);
                thumnailPlayer.Volume = 0;
                thumnailPlayer.ScrubbingEnabled = true;

                mePlayer.Play();
                mediaPlayerIsPlaying = true;

                lvDataBinding.ItemsSource = myClasses.videos;
            }
        }
    }
}
