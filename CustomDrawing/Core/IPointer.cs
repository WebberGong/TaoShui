using System.Windows;

namespace CustomDrawing.Core
{

    #region IPointer

    public interface IPointer
    {
        Point GetPosition();
        bool IsCaptured();
        void Capture();
        void Release();
    }

    #endregion
}