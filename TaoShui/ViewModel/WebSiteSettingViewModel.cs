using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using AutoMapper;
using AutoMapper.Configuration;
using AutoMapper.Configuration.Conventions;
using AutoMapper.Mappers;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Repository.Dto;
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

            WebSiteSettingEditCommand = new RelayCommand<DataGridRowEditEndingEventArgs>(ExecuteWebSiteSettingEditCommand);
        }

        public ObservableCollection<WebSiteSettingDto> WebSiteSettings { get; set; }

        public ObservableCollection<WebSiteDto> WebSites { get; set; }

        public ICommand WebSiteSettingEditCommand { get; private set; }

        private void ExecuteWebSiteSettingEditCommand(DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                _dataService.SaveWebSiteSetting(e.Row.Item as WebSiteSettingDto);
            }
        }
    }
}