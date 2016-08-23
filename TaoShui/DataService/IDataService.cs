using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Repository.Dto;
using TaoShui.Model;

namespace TaoShui.DataService
{
    public interface IDataService
    {
        ObservableCollection<WebSiteSettingDto> GetWebSiteSettings();

        ObservableCollection<WebSiteDto> GetWebSites();

        void SaveWebSiteSetting(WebSiteSettingDto setting);
    }
}
