using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotVisualizer
{
    public class MoveToViewModel : INotifyPropertyChanged
    {
        private string x;
        private string y;
        private string scale;

        public event PropertyChangedEventHandler PropertyChanged;

        public string X { get => x; set { x = value; OnPropertyChanged("X"); } }
        public string Y { get => y; set { y = value; OnPropertyChanged("Y"); } }
        public string Scale { get => scale; set { scale = value; OnPropertyChanged("Scale"); } }

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
