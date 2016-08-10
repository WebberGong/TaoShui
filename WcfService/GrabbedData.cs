using System;
using System.Runtime.Serialization;

namespace WcfService
{
    [DataContract]
    public class GrabbedData
    {
        [DataMember]
        public string[][] Data { get; set; }

        [DataMember]
        public string Type { get; set; }
    }
}