using System.Collections.Generic;

namespace CustomDrawing.Core
{

    #region Custom

    public class Custom : Element
    {
        public Pin Origin { get; set; }
        public IList<Element> Children { get; set; }
        public IList<Element> Variables { get; set; }
    }

    #endregion
}