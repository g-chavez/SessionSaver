using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SessionSaver.Model;
using SessionSaver.Business;
using SessionSaver.Commands;
using System.ComponentModel;

namespace SessionSaver.ViewModel
{
    internal class ApplicationViewModel : INotifyPropertyChanged
    {
        public List<Application> ApplicationList { get; }

        private string _Status = string.Empty;

        private int _ApplicationCount = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        public int ApplicationCount
        {
            get
            {
                return _ApplicationCount;
            }
            set
            {
                _ApplicationCount = value;
                // Notify property changed so that the status bar text in UI can be updated
                OnPropertyChanged(nameof(ApplicationCount));
            }
        }
        public string Status
        {
            get
            {
                return _Status;
            }
            set
            {
                _Status = value;
                // Notify property changed so that the status bar text in UI can be updated
                OnPropertyChanged(nameof(Status));
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Gets the SaveCommand for the viewmodel.
        /// </summary>
        public ICommand SaveCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Indicates whether the applications can be saved or not (e. g. if there are no apps selected to save or app list is empty).
        /// </summary>
        public bool CanSave
        {
            get
            {
                if (ApplicationList == null || ApplicationList.Count == 0)
                    return false;

                return ApplicationList.Any(x => x.IsSelected);
            }
            set
            {

            }
        }

        public ApplicationViewModel()
        {
            // Create and build the application list from Business
            ApplicationList = new ApplicationBL().GetUserApplications();
            _ApplicationCount = ApplicationList.Count();
            SaveCommand = new ApplicationSaveCommand(this);
            _Status = "Ready";
        }

        public void SaveSession()
        {
            Status = "Saving...";

            List<Application> SelectedApps = ApplicationList.Where(
                    x =>
                    x.IsSelected
                    && !string.IsNullOrEmpty(x.CommandLine)
                ).ToList();

            string FilePath = GetFilePathFromSaveDialog();
            if (!string.IsNullOrEmpty(FilePath))
                Status = new SessionBL().SaveSession(FilePath, SelectedApps);
            else
                Status = string.Empty;
        }

        private string GetFilePathFromSaveDialog()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = new SessionBL().GetFileName();
            dlg.DefaultExt = ".bat";
            dlg.Filter = "(.bat)|*.bat";

            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                return dlg.FileName;
            }
            return null;
        }
    }
}
