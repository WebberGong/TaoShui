using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Repository.Dto;
using TaoShui.DataService;
using TaoShui.Model;
using TaoShui.Shared;
using Utils;
using MessageBox = System.Windows.MessageBox;

namespace TaoShui.ViewModel
{
    public class WebSiteSettingViewModel : ViewModelBase
    {
        private readonly IDataService<WebSiteSetting, WebSiteSettingDto> _webSiteSettingDs;
        private readonly IDataService<WebSite, WebSiteDto> _webSiteDs;

        public WebSiteSettingViewModel(
            IDataService<WebSiteSetting, WebSiteSettingDto> webSiteSettingDs, 
            IDataService<WebSite, WebSiteDto> webSiteDs)
        {
            _webSiteSettingDs = webSiteSettingDs;
            _webSiteDs = webSiteDs;

            WebSiteSettings = _webSiteSettingDs.SelectAllModel();
            WebSites = _webSiteDs.SelectAllModel();
            foreach (var site in WebSites)
            {
                site.Setting = WebSiteSettings.FirstOrDefault(x => x.Id == site.SettingId);
                site.Settings = WebSiteSettings;
            }

            WebSiteSettingAddCommand = new RelayCommand(ExecuteWebSiteSettingAddCommand);
            WebSiteSettingEditCommand = new RelayCommand<ContentPresenter>(ExecuteWebSiteSettingEditCommand);
            WebSiteSettingRemoveCommand = new RelayCommand<ContentPresenter>(ExecuteWebSiteSettingRemoveCommand);
            WebSiteAddCommand = new RelayCommand(ExecuteWebSiteAddCommand);
            WebSiteEditCommand = new RelayCommand<ContentPresenter>(ExecuteWebSiteEditCommand);
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
            var setting = new WebSiteSetting();
            new NewModelWindow<WebSiteSetting, WebSiteSettingDto>(
                true,
                setting, 
                _webSiteSettingDs,
                (result, model) =>
                {
                    if (result.IsSuccess)
                    {
                        WebSiteSettings.Add(model);
                    }
                }).ShowDialog();
        }

        private void ExecuteWebSiteSettingEditCommand(ContentPresenter contentPresenter)
        {
            var setting = contentPresenter.Content as WebSiteSetting;
            if (setting != null)
            {
                new NewModelWindow<WebSiteSetting, WebSiteSettingDto>(
                    false,
                    setting,
                    _webSiteSettingDs,
                    (result, model) =>
                    {
                    }).ShowDialog();
            }
        }

        private void ExecuteWebSiteSettingRemoveCommand(ContentPresenter contentPresenter)
        {
            var setting = contentPresenter.Content as WebSiteSetting;
            if (setting != null)
            {
                if (MyMessageBox.ShowQuestionDialog("确定要删除该条数据吗?") == DialogResult.OK)
                {
                    var result = _webSiteSettingDs.Delete(setting);
                    if (result.IsSuccess)
                    {
                        WebSiteSettings.Remove(setting);
                        var removeList = WebSites.Where(x => x.SettingId == setting.Id).ToList();
                        removeList.ForEach(x => WebSites.Remove(x));
                        MyMessageBox.ShowInformationDialog(result.CombinedMsg);
                    }
                    else
                    {
                        MyMessageBox.ShowWarningDialog(result.CombinedMsg);
                    }
                }
            }
        }

        private void ExecuteWebSiteAddCommand()
        {
            var site = new WebSite(WebSiteSettings);
            new NewModelWindow<WebSite, WebSiteDto>(
                true,
                site,
                _webSiteDs,
                (result, model) =>
                {
                    if (result.IsSuccess)
                    {
                        WebSites.Add(model);
                    }
                }).ShowDialog();
        }

        private void ExecuteWebSiteEditCommand(ContentPresenter contentPresenter)
        {
            var site = contentPresenter.Content as WebSite;
            if (site != null)
            {
                new NewModelWindow<WebSite, WebSiteDto>(
                    false,
                    site,
                    _webSiteDs,
                    (result, model) =>
                    {
                    }).ShowDialog();
            }
        }

        private void ExecuteWebSiteRemoveCommand(ContentPresenter contentPresenter)
        {
            var site = contentPresenter.Content as WebSite;
            if (site != null)
            {
                if (MyMessageBox.ShowQuestionDialog("确定要删除该条数据吗?") == DialogResult.OK)
                {
                    var result = _webSiteDs.Delete(site);
                    if (result.IsSuccess)
                    {
                        WebSites.Remove(site);
                        MyMessageBox.ShowInformationDialog(result.CombinedMsg);
                    }
                    else
                    {
                        MyMessageBox.ShowWarningDialog(result.CombinedMsg);
                    }
                }
            }
        }
    }
}