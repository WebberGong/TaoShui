using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using TaoShui.DataService;
using TaoShui.Model;
using DataGrid = System.Windows.Controls.DataGrid;

namespace TaoShui.ViewModel
{
    public class WebSiteSettingViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;

        public WebSiteSettingViewModel(IDataService dataService)
        {
            _dataService = dataService;

            WebSiteSettings = dataService.SelectAllWebSiteSettings();
            WebSites = dataService.SelectAllWebSites();
            foreach (var site in WebSites)
            {
                site.Setting = WebSiteSettings.FirstOrDefault(x => x.Id == site.SettingId);
            }

            WebSiteSettingAddCommand = new RelayCommand(ExecuteWebSiteSettingAddCommand);
            WebSiteSettingEditCommand = new RelayCommand<DataGridCellEditEndingEventArgs>(ExecuteWebSiteSettingEditCommand);
            WebSiteSettingRemoveCommand = new RelayCommand<ContentPresenter>(ExecuteWebSiteSettingRemoveCommand);
            WebSiteAddCommand = new RelayCommand(ExecuteWebSiteAddCommand);
            WebSiteEditCommand = new RelayCommand<DataGridCellEditEndingEventArgs>(ExecuteWebSiteEditCommand);
            WebSiteRemoveCommand = new RelayCommand<ContentPresenter>(ExecuteWebSiteRemoveCommand);
        }

        public ObservableCollection<WebSiteSetting> WebSiteSettings { get; set; }

        public ObservableCollection<WebSite> WebSites { get; set; }

        public ICommand WebSiteSettingAddCommand { get; private set; }

        public ICommand WebSiteSettingEditCommand { get; private set; }

        public ICommand WebSiteSettingRemoveCommand { get; private set; }

        public ICommand WebSiteAddCommand { get; private set; }

        public ICommand WebSiteEditCommand { get; private set; }

        public ICommand WebSiteRemoveCommand { get; private set; }

        private void ExecuteWebSiteSettingAddCommand()
        {
        }

        private void ExecuteWebSiteSettingEditCommand(DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                _dataService.UpdateWebSiteSetting(e.Row.Item as WebSiteSetting);
            }
        }

        private void ExecuteWebSiteSettingRemoveCommand(ContentPresenter contentPresenter)
        {
            var setting = contentPresenter.Content as WebSiteSetting;
            if (setting != null)
            {
                if (_dataService.DeleteWebSiteSetting(setting))
                {
                    WebSiteSettings.Remove(setting);
                    var removeList = WebSites.Where(x => x.SettingId == setting.Id).ToList();
                    removeList.ForEach(x => WebSites.Remove(x));
                }
            }
        }

        private void ExecuteWebSiteAddCommand()
        {
        }

        private void ExecuteWebSiteEditCommand(DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                _dataService.UpdateWebSite(e.Row.Item as WebSite);
            }
        }

        private void ExecuteWebSiteRemoveCommand(ContentPresenter contentPresenter)
        {
            var site = contentPresenter.Content as WebSite;
            if (site != null)
            {
                if (_dataService.DeleteWebSite(site))
                {
                    WebSites.Remove(site);
                }
            }
        }
    }
}