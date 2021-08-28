using System.ComponentModel;

namespace SessionSaver.Model
{
    public class Application : INotifyPropertyChanged
    {
        public Application()
        {

        }

        public bool HasWindow { get; set; }

        public string Title { get; set; }

        public string DescriptiveName { get; set; }

        public string Name { get; set; }

        public string Owner{ get; set; }

        public string CommandLine { get; set; }

        public int Id { get; set; }

        public string FileName { get; set; }

        public string StartTime { get; set; }

        public System.Windows.Media.Imaging.BitmapSource Icon { get; set; }

        private bool _IsSelected = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsSelected
        {
            get {
                return _IsSelected;
            }
            set {
                _IsSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
