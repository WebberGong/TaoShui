using System.Collections.Generic;

namespace Entity
{
    public class WebSiteData
    {
        public string WebSiteStatus { get; set; }
        public IDictionary<string, IDictionary<string, string[][][]>> GrabbedData { get; set; }
    }
}