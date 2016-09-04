namespace CustomDrawing.Core
{

    #region Text

    public class Text : Element
    {
        public Pin Origin { get; set; }
        public int HAlign { get; set; }
        public int VAlign { get; set; }
        public string Value { get; set; }
    }

    #endregion
}