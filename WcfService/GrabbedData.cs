using System;
using System.Runtime.Serialization;
using Entity;

namespace WcfService
{
    [DataContract]
    public class GrabbedData
    {
        [DataMember]
        public FootballData[] Data { get; set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public DateTime GrabbedTime { get; set; }
    }
}