using System.Collections.ObjectModel;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using TaoShui.Model;

namespace TaoShui.ViewModel
{
    public class SettingViewModel : ViewModelBase
    {
        private readonly WebSiteSettingViewModel _webSiteSettingViewModel = new WebSiteSettingViewModel();
        private readonly SystemSettingViewModel _systemSettingViewModel = new SystemSettingViewModel();
        private ViewModelBase _currentViewModel;
        private bool _isWebSiteSettingChecked = true;
        private bool _isSystemSettingChecked = false;

        public SettingViewModel()
        {
            _currentViewModel = _webSiteSettingViewModel;
            WebSiteSettingViewCommand = new RelayCommand(ExecuteWebSiteSettingViewCommand);
            SystemSettingViewCommand = new RelayCommand(ExecuteSystemSettingViewCommand);
        }

        public ViewModelBase CurrentViewModel
        {
            get { return _currentViewModel; }
            private set
            {
                if (_currentViewModel != value)
                {
                    _currentViewModel = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsWebSiteSettingChecked
        {
            get { return _isWebSiteSettingChecked; }
            private set
            {
                if (_isWebSiteSettingChecked != value)
                {
                    _isWebSiteSettingChecked = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsSystemSettingChecked
        {
            get { return _isSystemSettingChecked; }
            private set
            {
                if (_isSystemSettingChecked != value)
                {
                    _isSystemSettingChecked = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ICommand WebSiteSettingViewCommand { get; private set; }

        public ICommand SystemSettingViewCommand { get; private set; }

        private void ExecuteWebSiteSettingViewCommand()
        {
            CurrentViewModel = _webSiteSettingViewModel;
        }

        private void ExecuteSystemSettingViewCommand()
        {
            CurrentViewModel = _systemSettingViewModel;
        }
    }
}