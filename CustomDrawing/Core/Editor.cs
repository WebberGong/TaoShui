using System;
using System.Windows;

namespace CustomDrawing.Core
{

    #region Editor

    public class Editor
    {
        #region Properties

        public Painter Painter { get; set; }
        public Factory Factory { get; set; }
        public IPointer Pointer { get; set; }
        public Action Redraw { get; set; }

        #endregion

        #region Fields

        public int CurrentEditMode = EditMode.Create;
        public int CurrentCreateMode = CreateMode.Line;
        private Point _previous;
        private Line _line;
        private Rectangle _rect;
        private Text _text;

        #endregion

        #region Mode

        public void SetCreateModeToLine()
        {
            CurrentCreateMode = CreateMode.Line;
        }

        public void SetCreateModeToText()
        {
            CurrentCreateMode = CreateMode.Text;
        }

        public void SetCreateModeToRectangle()
        {
            CurrentCreateMode = CreateMode.Rectangle;
        }

        public void SetEditModeToHitTest()
        {
            CurrentEditMode = EditMode.HitTest;
        }

        public void SetEditModeToCreate()
        {
            CurrentEditMode = EditMode.Create;
        }

        public void SetEditModeToMove()
        {
            CurrentEditMode = EditMode.Move;
        }

        #endregion

        #region Edit

        private void RedrawIfDirty()
        {
            if (Painter.IsDirty())
                Redraw();
        }

        public void LeftDown(Point p)
        {
            if (CurrentEditMode == EditMode.HitTest)
            {
                Pointer.Capture();
                StartHitTest(p, Redraw);
            }
            else if (CurrentEditMode == EditMode.Create)
            {
                StartEdit(p);
            }
            else if (CurrentEditMode == EditMode.Move)
            {
                FinishEdit();
                RedrawIfDirty();
            }
        }

        public void LeftUp()
        {
            if (CurrentEditMode == EditMode.HitTest)
                Pointer.Release();
        }

        public void Move(Point p)
        {
            if (CurrentEditMode == EditMode.HitTest)
                if (Pointer.IsCaptured() && (Painter.Selected != null))
                    MoveSelected(p);
                else
                    StartHitTest(p, RedrawIfDirty);
            else if (CurrentEditMode == EditMode.Create)
                StartHitTest(p, RedrawIfDirty);
            else if (CurrentEditMode == EditMode.Move)
                if (Pointer.IsCaptured())
                    MovedCurrentElement(p);
        }

        private void MoveSelected(Point p)
        {
            Painter.Selected.X += (float) (p.X - _previous.X);
            Painter.Selected.Y += (float) (p.Y - _previous.Y);
            _previous = p;
            Redraw();
        }

        private void MovedCurrentElement(Point p)
        {
            if (CurrentCreateMode == CreateMode.Line)
            {
                if (_line != null)
                {
                    _line.End.X = (float) p.X;
                    _line.End.Y = (float) p.Y;
                    StartHitTest(p, Redraw);
                }
            }
            else if (CurrentCreateMode == CreateMode.Rectangle)
            {
                _rect.BottomRight.X = (float) p.X;
                _rect.BottomRight.Y = (float) p.Y;
                StartHitTest(p, Redraw);
            }
            else if (CurrentCreateMode == CreateMode.Text)
            {
                _text.Origin.X = (float) p.X;
                _text.Origin.Y = (float) p.Y;
                StartHitTest(p, Redraw);
            }
        }

        public void RightDown()
        {
            if (CurrentEditMode == EditMode.Move)
                CancelEdit();
        }

        public void ToggleSnapMode(PinTypes type)
        {
            var current = Painter.SnapMode;
            Painter.SnapMode = (current & type) == type ? current & ~type : current | type;
        }

        public void MoveTextLeft()
        {
            if ((CurrentCreateMode == CreateMode.Text) && (_text != null))
            {
                switch (_text.HAlign)
                {
                    case HAlign.Left:
                        _text.HAlign = HAlign.Right;
                        break;
                    case HAlign.Center:
                        _text.HAlign = HAlign.Left;
                        break;
                    case HAlign.Right:
                        _text.HAlign = HAlign.Center;
                        break;
                }
                Redraw();
            }
        }

        public void MoveTextRight()
        {
            if ((CurrentCreateMode == CreateMode.Text) && (_text != null))
            {
                switch (_text.HAlign)
                {
                    case HAlign.Left:
                        _text.HAlign = HAlign.Center;
                        break;
                    case HAlign.Center:
                        _text.HAlign = HAlign.Right;
                        break;
                    case HAlign.Right:
                        _text.HAlign = HAlign.Left;
                        break;
                }
                Redraw();
            }
        }

        public void MoveTextDown()
        {
            if ((CurrentCreateMode == CreateMode.Text) && (_text != null))
            {
                switch (_text.VAlign)
                {
                    case VAlign.Top:
                        _text.VAlign = VAlign.Center;
                        break;
                    case VAlign.Center:
                        _text.VAlign = VAlign.Bottom;
                        break;
                    case VAlign.Bottom:
                        _text.VAlign = VAlign.Top;
                        break;
                }
                Redraw();
            }
        }

        public void MoveTextUp()
        {
            if ((CurrentCreateMode == CreateMode.Text) && (_text != null))
            {
                switch (_text.VAlign)
                {
                    case VAlign.Top:
                        _text.VAlign = VAlign.Bottom;
                        break;
                    case VAlign.Center:
                        _text.VAlign = VAlign.Top;
                        break;
                    case VAlign.Bottom:
                        _text.VAlign = VAlign.Center;
                        break;
                }
                Redraw();
            }
        }

        private void StartHitTest(Point p, Action redraw)
        {
            Painter.ResetSelected();
            _previous = p;
            Painter.HitX = (float) p.X;
            Painter.HitY = (float) p.Y;
            Painter.DoHitTest = true;
            redraw();
        }

        private void StartEdit(Point p)
        {
            CreateElement(p);
            SetEditModeToMove();
            Pointer.Capture();
            Painter.DoHitTest = false;
            Redraw();
        }

        private void CreateElement(Point p)
        {
            if (CurrentCreateMode == CreateMode.Line)
            {
                if (Painter.Selected != null)
                {
                    var start = Painter.Selected ?? Factory.CreatePin(null, (float) p.X, (float) p.Y, PinTypes.Snap);
                    var end = Factory.CreatePin(null, start.TransX, start.TransY, PinTypes.Snap);
                    _line = Factory.CreateLine(null, start, end);
                    _line.UseTransforms = true;
                    Painter.Elements.Add(_line);
                }
            }
            else if (CurrentCreateMode == CreateMode.Rectangle)
            {
                var tl = Painter.Selected ?? Factory.CreatePin(null, (float) p.X, (float) p.Y, PinTypes.Snap);
                var br = Factory.CreatePin(null, tl.TransX, tl.TransY, PinTypes.Snap);
                _rect = Factory.CreateRectangle(null, tl, br);
                _rect.UseTransforms = true;
                Painter.Elements.Add(_rect);
            }
            else if (CurrentCreateMode == CreateMode.Text)
            {
                var origin = Painter.Selected ?? Factory.CreatePin(null, (float) p.X, (float) p.Y, PinTypes.Snap);
                _text = Factory.CreateText(null, origin, "text", HAlign.Center, VAlign.Center);
                _text.UseTransforms = true;
                Painter.Elements.Add(_text);
            }
        }

        private void FinishEdit()
        {
            if (CurrentCreateMode == CreateMode.Line)
                ResetCurrentLine();
            else if (CurrentCreateMode == CreateMode.Rectangle)
                ResetCurrentRectangle();
            else if (CurrentCreateMode == CreateMode.Text)
                ResetCurrentText();

            SetEditModeToCreate();
            Pointer.Release();
        }

        private void ResetCurrentLine()
        {
            if (Painter.Selected != null)
            {
                _line.End = Painter.Selected;
                Painter.Elements.Add(_line.ExternalRectangle);
                Painter.ResetSelected();
            }
            else
            {
                RemoveCurrentElement();
            }
            Redraw();
            _line = null;
        }

        private void ResetCurrentRectangle()
        {
            if (Painter.Selected != null)
            {
                _rect.BottomRight = Painter.Selected;
                Painter.ResetSelected();
            }

            _rect = null;
        }

        private void ResetCurrentText()
        {
            if (Painter.Selected != null)
            {
                _text.Origin = Painter.Selected;
                Painter.ResetSelected();
            }

            _text = null;
        }

        public void CancelEdit()
        {
            RemoveCurrentElement();
            FinishEdit();
            Redraw();
        }

        private void RemoveCurrentElement()
        {
            switch (CurrentCreateMode)
            {
                case CreateMode.Line:
                    Painter.Elements.Remove(_line);
                    break;
                case CreateMode.Rectangle:
                    Painter.Elements.Remove(_rect);
                    break;
                case CreateMode.Text:
                    Painter.Elements.Remove(_text);
                    break;
            }
        }

        #endregion
    }

    #endregion
}