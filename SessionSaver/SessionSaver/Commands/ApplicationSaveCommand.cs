using SessionSaver.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SessionSaver.Commands
{
    internal class ApplicationSaveCommand : ICommand
    {
        private ApplicationViewModel _ViewModel;
        public ApplicationSaveCommand(ApplicationViewModel viewModel)
        {
            _ViewModel = viewModel;
        }

        public event EventHandler CanExecuteChanged // Wire to the WPF command system
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Enable or disable controls (e. g. the save button), based on the return value of this method.
        /// Pass this to our view model.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            return _ViewModel.CanSave;
        }

        public void Execute(object parameter)
        {
            _ViewModel.SaveSession();
        }
    }
}
