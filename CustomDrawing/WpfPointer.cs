using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CustomDrawing.Core;

namespace CustomDrawing
{

    #region WpfPointer

    public class WpfPointer : IPointer
    {
        #region Constructor

        public WpfPointer(Canvas canvas)
        {
            Canvas = canvas;
        }

        #endregion

        #region Fields

        public Canvas Canvas { get; private set; }

        #endregion

        #region IPointer

        public Point GetPosition()
        {
            return Mouse.GetPosition(Canvas);
        }

        public bool IsCaptured()
        {
            return Canvas.IsMouseCaptured;
        }

        public void Capture()
        {
            Canvas.CaptureMouse();
        }

        public void Release()
        {
            if (Canvas.IsMouseCaptured)
                Canvas.ReleaseMouseCapture();
        }

        #endregion
    }

    #endregion
}