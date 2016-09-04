﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace CustomDrawing.Core
{

    #region Painter

    public class Painter
    {
        #region Constructor

        public Painter()
        {
            Elements = new List<Element>();
            Origin = new Pin {Id = -1, X = 0f, Y = 0f};
            Selected = null;
        }

        #endregion

        #region IsDirty

        public bool IsDirty()
        {
            var sw = Stopwatch.StartNew();

            var wasDirty = SelectedPins.Count > 0;
            DoRender = false;
            Draw(Elements, Origin, 0f, 0f);
            ResetSelected();
            var isDirty = SelectedPins.Count > 0;

            sw.Stop();
            Debug.Print("IsDirty: " + sw.Elapsed.TotalMilliseconds + "ms");

            return isDirty || wasDirty;
        }

        #endregion

        #region Fields

        public IRenderer Renderer { get; set; }
        public List<Element> Elements { get; protected set; }
        public Pin Origin { get; protected set; }
        public Pin Selected { get; set; }
        public float HitX;
        public float HitY;
        public bool DoHitTest = false;
        public bool DoRender;
        public PinTypes SnapMode = PinTypes.Origin | PinTypes.Connector;
        public float HitRadius = 10f;
        private HashSet<int> _selectedHash;
        private HashSet<int> _hitTestHash;
        private List<Selected> SelectedPins { get; set; }

        #endregion

        #region HitTest

        private void Transform(Pin pin, float cx, float cy, out float tcx, out float tcy)
        {
            if (!_hitTestHash.Contains(pin.Id))
            {
                Renderer.Transform(cx, cy, out tcx, out tcy);
                pin.TransX = tcx;
                pin.TransY = tcy;
                _hitTestHash.Add(pin.Id);
            }
            else
            {
                tcx = pin.TransX;
                tcy = pin.TransY;
            }
        }

        private bool HitTest(Pin pin, float radius, float cx, float cy, float hitX, float hitY)
        {
            float tcx, tcy;
            Transform(pin, cx, cy, out tcx, out tcy);
            return new Rect(tcx - radius, tcy - radius, radius + radius, radius + radius).Contains(hitX, hitY);
        }

        private bool HitTest(Pin pin, float x, float y)
        {
            var testResult = HitTest(pin, HitRadius, pin.X + x, pin.Y + y, HitX, HitY);
            return (Selected == null) && !pin.IsSelected && testResult;
        }

        #endregion

        #region Selected

        private void SetSelected(Pin pin)
        {
            Selected = pin;
            pin.IsSelected = true;
        }

        public void ResetSelected()
        {
            if (Selected != null)
            {
                Selected.IsSelected = false;
                Selected = null;
            }
        }

        private void AddSelected(Pin pin, float x, float y, float angle, float cx, float cy)
        {
            if (!_selectedHash.Contains(pin.Id))
            {
                _selectedHash.Add(pin.Id);
                SelectedPins.Add(new Selected(pin, x, y, angle, cx, cy));
            }
        }

        private bool CanDeselect(Pin pin)
        {
            return pin.IsSelected && !((Selected != null) && (pin.Id == Selected.Id));
        }

        #endregion

        #region Draw Primitives

        private void Draw(Pin pin, float x, float y, IDictionary<int, string> variables)
        {
            if (DoRender)
            {
                var parent = pin.Parent as Reference;
                if (parent != null)
                {
                    var origin = parent.Origin;
                    Renderer.Draw(pin, origin.X + x, origin.Y + y, variables);
                }
                else
                {
                    Renderer.Draw(pin, x, y, variables);
                }
            }
        }

        private void HitTestForSelected(Pin pin, float x, float y)
        {
            if (DoHitTest)
            {
                bool hit;
                var parent = pin.Parent as Reference;
                if (parent != null)
                {
                    var origin = parent.Origin;
                    hit = HitTest(pin, origin.X + x, origin.Y + y);
                }
                else
                {
                    hit = HitTest(pin, x, y);
                }

                if (hit && ((SnapMode & pin.Type) == pin.Type))
                {
                    SetSelected(pin);
                    AddSelected(pin, x, y, Renderer.GetAngle(), Renderer.GetCenterX(), Renderer.GetCenterY());
                }
                else if (CanDeselect(pin))
                {
                    pin.IsSelected = false;
                }
            }
            else
            {
                var parent = pin.Parent as Reference;
                if (parent != null)
                {
                    var origin = parent.Origin;
                    HitTest(pin, origin.X + x, origin.Y + y);
                }
                else
                {
                    HitTest(pin, x, y);
                }

                if (pin.IsSelected)
                    AddSelected(pin, x, y, Renderer.GetAngle(), Renderer.GetCenterX(), Renderer.GetCenterY());
            }
        }

        private void Draw(Pin pin, float x, float y, bool isVisible, IDictionary<int, string> variables)
        {
            HitTestForSelected(pin, x, y);

            if (isVisible)
                Draw(pin, x, y, variables);
        }

        private void Draw(Line line, float x, float y, IDictionary<int, string> variables)
        {
            if (DoRender)
                Renderer.Draw(line, x, y, variables);

            Draw(line.Start, x, y, false, variables);
            Draw(line.End, x, y, false, variables);
        }

        private void Draw(Rectangle rectangle, float x, float y, IDictionary<int, string> variables)
        {
            if (DoRender)
                Renderer.Draw(rectangle, x, y, variables);

            Draw(rectangle.TopLeft, x, y, false, variables);
            Draw(rectangle.BottomRight, x, y, false, variables);
        }

        private void Draw(Text text, float x, float y, IDictionary<int, string> variables)
        {
            if (DoRender)
                Renderer.Draw(text, x, y, variables);

            Draw(text.Origin, x, y, false, variables);
        }

        private void Draw(Custom custom, float x, float y, IDictionary<int, string> variables)
        {
            for (var i = 0; i < custom.Children.Count; i++)
                Draw(custom.Children[i], custom.Origin, x, y, variables);

            Draw(custom.Origin, x, y, false, variables);
        }

        private void Draw(Reference reference, float x, float y, IDictionary<int, string> variables)
        {
            Draw(reference.Content, reference.Origin.X + x, reference.Origin.Y + y, variables);

            for (var i = 0; i < reference.Connectors.Count; i++)
            {
                var connector = reference.Connectors[i];
                Draw(connector, null, x, y, variables);
            }

            Draw(reference.Origin, x, y, false, variables);
        }

        #endregion

        #region Draw

        private void UpdateAngle(Element element)
        {
            if (element is Custom)
            {
                var custom = element as Custom;
                Renderer.SetAngle(element.Angle, custom.Origin.X, custom.Origin.Y);
            }
            else if (element is Reference)
            {
                var reference = element as Reference;
                Renderer.SetAngle(element.Angle, reference.Origin.X, reference.Origin.Y);
            }
            else
            {
                Renderer.SetAngle(element.Angle, 0f, 0f);
            }
        }

        private void Draw(Element child, Pin origin, float x, float y, IDictionary<int, string> variables)
        {
            if (child is Pin)
                Draw(child as Pin, origin == null ? x : origin.X + x, origin == null ? y : origin.Y + y, false, variables);
            else if (child is Line)
                Draw(child as Line, origin.X + x, origin.Y + y, variables);
            else if (child is Rectangle)
                Draw(child as Rectangle, origin.X + x, origin.Y + y, variables);
            else if (child is Text)
                Draw(child as Text, origin.X + x, origin.Y + y, variables);
            else if (child is Custom)
                Draw(child as Custom, origin.X + x, origin.Y + y, variables);
            else if (child is Reference)
                Draw(child as Reference, origin.X + x, origin.Y + y, (child as Reference).Variables);
        }

        private void DrawRoot(Pin origin, float x, float y, Element element)
        {
            UpdateAngle(element);

            if (DoRender)
                Renderer.PushRotate();

            Draw(element, origin, x, y, null);

            if (DoRender)
                Renderer.Pop();
        }

        private void DrawSelected(Selected selected)
        {
            Renderer.SetAngle(selected.Angle, selected.CenterX, selected.CenterY);

            if (DoRender)
                Renderer.PushRotate();

            Draw(selected.Pin, selected.X, selected.Y, null);

            if (DoRender)
                Renderer.Pop();
        }

        public void Draw(List<Element> elements, Pin origin, float x, float y)
        {
            Reset();

            foreach (Element t in elements)
                DrawRoot(origin, x, y, t);

            foreach (Selected selected in SelectedPins)
                DrawSelected(selected);
        }

        private void Reset()
        {
            _selectedHash = new HashSet<int>();
            _hitTestHash = new HashSet<int>();
            SelectedPins = new List<Selected>();
        }

        #endregion
    }

    #endregion
}