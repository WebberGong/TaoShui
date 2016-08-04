using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class WebSiteData
    {
        public WebSiteStatus WebSiteStatus { get; set; }
        public IDictionary<string, IDictionary<string, IList<string>>> GrabbedData { get; set; }
    }
}
