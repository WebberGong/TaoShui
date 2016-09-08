using System.Collections.Generic;

namespace CustomDrawing.Core
{

    #region Reference

    public class Reference : Element
    {
        public Pin Origin { get; set; }
        public Custom Content { get; set; }
        public IList<Pin> Connectors { get; set; }
        public IDictionary<int, string> Variables { get; set; }
        public Postion Position { get; set; }
    }

    #endregion
}