namespace CustomDrawing.Core
{

    #region Element

    public abstract class Element
    {
        public int Id { get; set; }
        public Element Parent { get; set; }
        public bool IsSelected { get; set; }
        public bool UseTransforms { get; set; }
        public float Angle { get; set; }
    }

    #endregion
}