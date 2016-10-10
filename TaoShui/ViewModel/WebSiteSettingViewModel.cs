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

namespace TaoShui.ViewModel
{
    public class WebSiteSettingViewModel : ViewModelBase
    {
        private readonly IDataService<WebSiteAccount, WebSiteAccountDto> _webSiteAccountDs;
        private readonly IDataService<WebSiteSetting, WebSiteSettingDto> _webSiteSettingDs;
        private ObservableCollection<WebSiteSetting> _webSiteSettings;
        private ObservableCollection<WebSiteAccount> _webSiteAccounts;

        public WebSiteSettingViewModel(
            IDataService<WebSiteSetting, WebSiteSettingDto> webSiteSettingDs,
            IDataService<WebSiteAccount, WebSiteAccountDto> webSiteAccountDs)
        {
            _webSiteSettingDs = webSiteSettingDs;
            _webSiteAccountDs = webSiteAccountDs;

            WebSiteSettings = _webSiteSettingDs.SelectAllModel();
            WebSiteAccounts = _webSiteAccountDs.SelectAllModel();

            foreach (var item1 in WebSiteSettings)
            {
                var relatedWebSiteSettings = new ObservableCollection<SelectableObject<WebSiteSetting>>();
                foreach (var item2 in WebSiteSettings)
                {
                    relatedWebSiteSettings.Add(new SelectableObject<WebSiteSetting>() { IsSelected = false, Object = item2 });
                }
                item1.RelatedWebSiteSettings = relatedWebSiteSettings;
            }

            foreach (var item1 in WebSiteSettings)
            {
                foreach (var item2 in item1.RelatedWebSiteSettings)
                {
                    item2.IsSelected = item2.Object.RelatedWebSiteSettingsString != null && item2.Object.RelatedWebSiteSettingsString.Contains(item1.Id.ToString());
                    var setting1 = item1;
                    item2.PropertyChanged += (sender, e) =>
                    {
                        var s1 = sender as SelectableObject<WebSiteSetting>;
                        if (e.PropertyName == "IsSelected" && s1 != null)
                        {
                            var setting2 = WebSiteSettings.FirstOrDefault(x => x.Id == s1.Object.Id);
                            if (setting2 != null)
                            {
                                var s2 = setting2.RelatedWebSiteSettings.FirstOrDefault(x => x.Object.Id == setting1.Id);
                                if (s2 != null)
                                {
                                    s2.IsSelected = s1.IsSelected;
                                }
                            }
                        }
                    };
                }
            }

            foreach (var site in WebSiteAccounts)
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
            SaveWebSiteSettingsCommand = new RelayCommand(ExecuteSaveWebSiteSettingsCommand);
        }

        public ObservableCollection<WebSiteSetting> WebSiteSettings
        {
            get { return _webSiteSettings; }
            private set
            {
                _webSiteSettings = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<WebSiteAccount> WebSiteAccounts
        {
            get { return _webSiteAccounts; }
            private set
            {
                _webSiteAccounts = value;
                RaisePropertyChanged();
            }
        }

        public ICommand WebSiteSettingAddCommand { get; private set; }

        public ICommand WebSiteSettingEditCommand { get; private set; }

        public ICommand WebSiteSettingRemoveCommand { get; private set; }

        public ICommand WebSiteAddCommand { get; private set; }

        public ICommand WebSiteEditCommand { get; private set; }

        public ICommand WebSiteRemoveCommand { get; private set; }

        public ICommand SaveWebSiteSettingsCommand { get; private set; }

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
                        WebSiteSettings.Add(model);
                }).ShowDialog();
        }

        private void ExecuteWebSiteSettingEditCommand(ContentPresenter contentPresenter)
        {
            var setting = contentPresenter.Content as WebSiteSetting;
            if (setting != null)
                new NewModelWindow<WebSiteSetting, WebSiteSettingDto>(
                    false,
                    setting,
                    _webSiteSettingDs,
                    (result, model) => { }).ShowDialog();
        }

        private void ExecuteWebSiteSettingRemoveCommand(ContentPresenter contentPresenter)
        {
            var setting = contentPresenter.Content as WebSiteSetting;
            if (setting != null)
                if (MyMessageBox.ShowQuestionDialog("确定要删除该条数据吗?") == DialogResult.OK)
                {
                    var result = _webSiteSettingDs.Delete(setting);
                    if (result.IsSuccess)
                    {
                        WebSiteSettings.Remove(setting);
                        var removeList = WebSiteAccounts.Where(x => x.SettingId == setting.Id).ToList();
                        removeList.ForEach(x => WebSiteAccounts.Remove(x));
                        MyMessageBox.ShowInformationDialog(result.CombinedMsg);
                    }
                    else
                    {
                        MyMessageBox.ShowWarningDialog(result.CombinedMsg);
                    }
                }
        }

        private void ExecuteWebSiteAddCommand()
        {
            var site = new WebSiteAccount(WebSiteSettings);
            new NewModelWindow<WebSiteAccount, WebSiteAccountDto>(
                true,
                site,
                _webSiteAccountDs,
                (result, model) =>
                {
                    if (result.IsSuccess)
                        WebSiteAccounts.Add(model);
                }).ShowDialog();
        }

        private void ExecuteWebSiteEditCommand(ContentPresenter contentPresenter)
        {
            var site = contentPresenter.Content as WebSiteAccount;
            if (site != null)
                new NewModelWindow<WebSiteAccount, WebSiteAccountDto>(
                    false,
                    site,
                    _webSiteAccountDs,
                    (result, model) => { }).ShowDialog();
        }

        private void ExecuteWebSiteRemoveCommand(ContentPresenter contentPresenter)
        {
            var site = contentPresenter.Content as WebSiteAccount;
            if (site != null)
                if (MyMessageBox.ShowQuestionDialog("确定要删除该条数据吗?") == DialogResult.OK)
                {
                    var result = _webSiteAccountDs.Delete(site);
                    if (result.IsSuccess)
                    {
                        WebSiteAccounts.Remove(site);
                        MyMessageBox.ShowInformationDialog(result.CombinedMsg);
                    }
                    else
                    {
                        MyMessageBox.ShowWarningDialog(result.CombinedMsg);
                    }
                }
        }

        private void ExecuteSaveWebSiteSettingsCommand()
        {
            bool result = _webSiteSettingDs.SaveAllModel(WebSiteSettings);
            MyMessageBox.ShowInformationDialog(result ? "保存成功！" : "保存失败！");
        }
    }
}