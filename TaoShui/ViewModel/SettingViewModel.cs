using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Practices.ServiceLocation;
using TaoShui.DataService;

namespace TaoShui.ViewModel
{
    public class SettingViewModel : ViewModelBase
    {
        private readonly SystemSettingViewModel _systemSettingViewModel = ServiceLocator.Current.GetInstance<SystemSettingViewModel>();
        private readonly WebSiteSettingViewModel _webSiteSettingViewModel = ServiceLocator.Current.GetInstance<WebSiteSettingViewModel>();
        private ViewModelBase _currentViewModel;
        private bool _isSystemSettingChecked;
        private bool _isWebSiteSettingChecked = true;

        public SettingViewModel(IDataService dataService)
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