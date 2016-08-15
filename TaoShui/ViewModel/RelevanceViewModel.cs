using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;

namespace TaoShui.ViewModel
{
    public class RelevanceViewModel : ViewModelBase
    {
        private string _name = "Relevance";

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}
