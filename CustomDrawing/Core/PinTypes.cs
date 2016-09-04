using System;

namespace CustomDrawing.Core
{

    #region PinTypes

    [Flags]
    public enum PinTypes
    {
        None = 0,
        Origin = 1,
        Snap = 2,
        Connector = 4,
        Guide = 8
    }

    #endregion
}