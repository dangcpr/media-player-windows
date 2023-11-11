using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace media_player_windows.classes
{
    public class MyClasses : INotifyPropertyChanged
    {
        public string targetVideoUrl { get; set; }

        public List<media_player_windows.classes.Video> videos = new List<media_player_windows.classes.Video>();

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
