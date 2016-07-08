using System.Collections.Generic;
using Newtonsoft.Json;

namespace TaoShui
{
    public class CommonEqualityComparer : IEqualityComparer<object>
    {
        public new bool Equals(object x, object y)
        {
            return JsonConvert.SerializeObject(x) == JsonConvert.SerializeObject(y);
        }

        public int GetHashCode(object obj)
        {
            return JsonConvert.SerializeObject(obj).GetHashCode();
        }
    }
}