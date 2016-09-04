﻿namespace CustomDrawing.Core
{

    #region Pin

    public class Pin : Element
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float TransX { get; set; }
        public float TransY { get; set; }
        public PinTypes Type { get; set; }
    }

    #endregion
}