using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using CustomDrawing.Core;

namespace CustomDrawing
{

    #region WpfElement

    public class WpfElement : FrameworkElement
    {
        #region Constructor

        public WpfElement(Painter painter)
        {
            Painter = painter;
            painter.Renderer = new WpfRenderer();
        }

        #endregion

        #region Properties

        public Painter Painter { get; protected set; }

        #endregion

        #region OnRender

        protected override void OnRender(DrawingContext drawingContext)
        {
            var sw = Stopwatch.StartNew();

            Render(drawingContext);

            sw.Stop();
            Debug.Print("Render: " + sw.Elapsed.TotalMilliseconds + "ms");
        }

        private void Render(DrawingContext dc)
        {
            var wpfRenderer = Painter.Renderer as WpfRenderer;
            if (wpfRenderer != null) wpfRenderer.Set(dc);
            Painter.DoRender = true;
            Painter.Draw(Painter.Elements, Painter.Origin, 0f, 0f);
            Painter.DoHitTest = false;
            Painter.DoRender = false;
        }

        public void Redraw()
        {
            InvalidateVisual();
        }

        #endregion
    }

    #endregion
}