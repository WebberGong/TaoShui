using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;

namespace TaoShui.ViewModel
{
    public class WebSiteViewModel : ViewModelBase
    {
        private string _name = "WebSite";

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}
