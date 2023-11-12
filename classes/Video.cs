using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace media_player_windows.classes
{
    public class Video : INotifyPropertyChanged
    {
        public string url { get; set; }

        public string name { get; set; }

        public string duration { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
