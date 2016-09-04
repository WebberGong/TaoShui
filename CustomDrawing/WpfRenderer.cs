using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using CustomDrawing.Core;

namespace CustomDrawing
{

    #region WpfRenderer

    public class WpfRenderer : IRenderer
    {
        #region Set

        public void Set(DrawingContext drawingContext)
        {
            _dc = drawingContext;
        }

        #endregion

        #region Origin

        public void GetReferenceOrigin(Pin pin, out float x, out float y)
        {
            var parent = pin.Parent as Reference;
            if (parent != null)
            {
                var origin = parent.Origin;
                x = origin.X;
                y = origin.Y;
            }
            else
            {
                x = 0f;
                y = 0f;
            }
        }

        #endregion

        #region Fields

        private DrawingContext _dc;
        private Pen _pen;
        private Pen _spen;
        private Brush _brush;
        private Brush _sbrush;
        private readonly float pinRadius = 5f;
        private RotateTransform _rt;

        #endregion

        #region Constructor

        public WpfRenderer()
        {
            Initialize(1.0, Color.FromArgb(0xFF, 0x00, 0x00, 0x00), Color.FromArgb(0xC0, 0x00, 0xFF, 0x00));
        }

        private void Initialize(double thickness, Color normal, Color selected)
        {
            _brush = new SolidColorBrush(normal);
            _brush.Freeze();
            _sbrush = new SolidColorBrush(selected);
            _sbrush.Freeze();
            _pen = new Pen(_brush, thickness) {StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round};
            _pen.Freeze();
            _spen = new Pen(_sbrush, thickness) {StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round};
            _spen.Freeze();
            _rt = new RotateTransform(0f, 0f, 0f);
        }

        #endregion

        #region IRenderer

        public void Transform(float x, float y, out float tx, out float ty)
        {
            var p = _rt.Transform(new Point(x, y));
            tx = (float) p.X;
            ty = (float) p.Y;
        }

        public void PushRotate()
        {
            var frt = new RotateTransform(_rt.Angle, _rt.CenterX, _rt.CenterY);
            frt.Freeze();
            _dc.PushTransform(frt);
        }

        public void Pop()
        {
            _dc.Pop();
        }

        public void SetAngle(float angle, float cx, float cy)
        {
            _rt.Angle = angle;
            _rt.CenterX = cx;
            _rt.CenterY = cy;
        }

        public float GetAngle()
        {
            return (float) _rt.Angle;
        }

        public float GetCenterX()
        {
            return (float) _rt.CenterX;
        }

        public float GetCenterY()
        {
            return (float) _rt.CenterY;
        }

        public void Draw(Pin pin, float x, float y, IDictionary<int, string> variables)
        {
            var c = new Point(pin.X + x, pin.Y + y);

            var half = pin.IsSelected ? _spen.Thickness/2.0 : _pen.Thickness/2.0;
            var guidelines = new GuidelineSet();
            guidelines.GuidelinesX.Add(c.X - pinRadius + half);
            guidelines.GuidelinesX.Add(c.X + pinRadius + half);
            guidelines.GuidelinesY.Add(c.Y - pinRadius + half);
            guidelines.GuidelinesY.Add(c.Y + pinRadius + half);
            _dc.PushGuidelineSet(guidelines);
            _dc.DrawEllipse(pin.IsSelected ? _sbrush : _brush, pin.IsSelected ? _spen : _pen, c, pinRadius, pinRadius);
            _dc.Pop();
        }

        public void Draw(Line line, float x, float y, IDictionary<int, string> variables)
        {
            float ox0, oy0, ox1, oy1;
            GetReferenceOrigin(line.Start, out ox0, out oy0);
            GetReferenceOrigin(line.End, out ox1, out oy1);
            var p0 = new Point(line.UseTransforms ? line.Start.TransX : ox0 + line.Start.X + x,
                line.UseTransforms ? line.Start.TransY : oy0 + line.Start.Y + y);
            var p1 = new Point(line.UseTransforms ? line.End.TransX : ox1 + line.End.X + x,
                line.UseTransforms ? line.End.TransY : oy1 + line.End.Y + y);

            var half = line.IsSelected ? _spen.Thickness/2.0 : _pen.Thickness/2.0;
            var guidelines = new GuidelineSet();
            guidelines.GuidelinesX.Add(p0.X + half);
            guidelines.GuidelinesX.Add(p1.X + half);
            guidelines.GuidelinesY.Add(p0.Y + half);
            guidelines.GuidelinesY.Add(p1.Y + half);
            _dc.PushGuidelineSet(guidelines);
            _dc.DrawLine(line.IsSelected ? _spen : _pen, p0, p1);
            _dc.Pop();
        }

        public void Draw(Rectangle rectangle, float x, float y, IDictionary<int, string> variables)
        {
            float ox0, oy0, ox1, oy1;
            GetReferenceOrigin(rectangle.TopLeft, out ox0, out oy0);
            GetReferenceOrigin(rectangle.BottomRight, out ox1, out oy1);
            var x0 = rectangle.UseTransforms ? rectangle.TopLeft.TransX : ox0 + rectangle.TopLeft.X + x;
            var y0 = rectangle.UseTransforms ? rectangle.TopLeft.TransY : oy0 + rectangle.TopLeft.Y + y;
            var x1 = rectangle.UseTransforms ? rectangle.BottomRight.TransX : ox1 + rectangle.BottomRight.X + x;
            var y1 = rectangle.UseTransforms ? rectangle.BottomRight.TransY : oy1 + rectangle.BottomRight.Y + y;
            var r = new Rect(Math.Min(x0, x1), Math.Min(y0, y1), Math.Abs(x1 - x0), Math.Abs(y1 - y0));

            var half = rectangle.IsSelected ? _spen.Thickness/2.0 : _pen.Thickness/2.0;
            var guidelines = new GuidelineSet();
            guidelines.GuidelinesX.Add(r.Left + half);
            guidelines.GuidelinesX.Add(r.Right + half);
            guidelines.GuidelinesY.Add(r.Top + half);
            guidelines.GuidelinesY.Add(r.Bottom + half);
            _dc.PushGuidelineSet(guidelines);
            _dc.DrawRectangle(rectangle.IsSelected ? _sbrush : _brush, rectangle.IsSelected ? _spen : _pen, r);
            _dc.Pop();
        }

        public void Draw(Text text, float x, float y, IDictionary<int, string> variables)
        {
            string textToFormat;
            if ((variables == null) || !variables.TryGetValue(text.Id, out textToFormat))
                textToFormat = text.Value;

            var ft = GetFormattedTextFromCache(text, textToFormat);
            ft.SetFontFamily("Microsoft YaHei");
            ft.SetFontSize(12);

            float ox, oy;
            GetReferenceOrigin(text.Origin, out ox, out oy);

            double px = text.UseTransforms ? text.Origin.TransX : ox + text.Origin.X + x; // HAlign.Left
            double py = text.UseTransforms ? text.Origin.TransY : oy + text.Origin.Y + y; // VAlign.Top

            switch (text.HAlign)
            {
                case HAlign.Right:
                    px = px - ft.WidthIncludingTrailingWhitespace;
                    break;
                case HAlign.Center:
                    px = px - ft.WidthIncludingTrailingWhitespace/2.0;
                    break;
            }

            switch (text.VAlign)
            {
                case VAlign.Bottom:
                    py = py - ft.Height;
                    break;
                case VAlign.Center:
                    py = py - ft.Height/2.0;
                    break;
            }

            var p = new Point(px, py);
            _dc.DrawText(ft, p);
        }

        #endregion

        #region FormattedText Cache

        private readonly IDictionary<string, FormattedText> _formattedTextCache = new Dictionary<string, FormattedText>();

        private FormattedText GetFormattedTextFromCache(Text text, string textToFormat)
        {
            FormattedText ft;
            if (!_formattedTextCache.TryGetValue(textToFormat, out ft))
            {
                ft = new FormattedText(textToFormat, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    new Typeface("Arial"), 15.0, text.IsSelected ? _sbrush : _brush);
                _formattedTextCache.Add(textToFormat, ft);
            }
            else
            {
                ft.SetForegroundBrush(text.IsSelected ? _sbrush : _brush);
            }
            return ft;
        }

        #endregion
    }

    #endregion
}