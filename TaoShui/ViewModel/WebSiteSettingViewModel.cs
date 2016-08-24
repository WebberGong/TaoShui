using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using TaoShui.DataService;
using TaoShui.Model;

namespace TaoShui.ViewModel
{
    public class WebSiteSettingViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;

        public WebSiteSettingViewModel(IDataService dataService)
        {
            _dataService = dataService;

            WebSiteSettings = dataService.GetWebSiteSettings();
            WebSites = dataService.GetWebSites();
            foreach (var site in WebSites)
                site.Setting = WebSiteSettings.FirstOrDefault(x => x.Id == site.SettingId);

            WebSiteSettingEditCommand =
                new RelayCommand<DataGridRowEditEndingEventArgs>(ExecuteWebSiteSettingEditCommand);
            WebSiteEditCommand = new RelayCommand<DataGridRowEditEndingEventArgs>(ExecuteWebSiteEditCommand);
        }

        public ObservableCollection<WebSiteSetting> WebSiteSettings { get; set; }

        public ObservableCollection<WebSite> WebSites { get; set; }

        public ICommand WebSiteSettingEditCommand { get; private set; }

        public ICommand WebSiteEditCommand { get; private set; }

        private void ExecuteWebSiteSettingEditCommand(DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var setting = e.Row.Item as WebSiteSetting;
                _dataService.SaveWebSiteSetting(setting);
            }
        }

        private void ExecuteWebSiteEditCommand(DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
                _dataService.SaveWebSite(e.Row.Item as WebSite);
        }
    }
}