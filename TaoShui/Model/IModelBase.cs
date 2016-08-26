using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaoShui.Model
{
    public interface IModelBase
    {
        long Id { get; }

        string Name { get; }
    }
}
