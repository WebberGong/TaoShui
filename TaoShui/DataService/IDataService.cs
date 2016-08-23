using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using TaoShui.Model;

namespace TaoShui.DataService
{
    public interface IDataService
    {
        ObservableCollection<WebSiteSetting> GetWebSiteSettings();

        ObservableCollection<WebSite> GetWebSites();

        void SaveWebSiteSetting(WebSiteSetting setting);

        void SaveWebSite(WebSite site);
    }
}
