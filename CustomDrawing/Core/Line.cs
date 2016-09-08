using System;

namespace CustomDrawing.Core
{

    #region Line

    public class Line : Element
    {
        public Pin Start { get; set; }
        public Pin End { get; set; }

        public Custom ExternalRectangle
        {
            get
            {
                if (Start != null && End != null)
                {
                    float thickness = 8f / 2f;
                    double alpha = Math.Atan2(Start.TransY - End.TransY, End.TransX - Start.TransX);
                    double theta = Math.PI - alpha;
                    double zet = theta - Math.PI / 2;
                    float sizeX = (float)(Math.Cos(zet) * thickness);
                    float sizeY = -(float)(Math.Sin(zet) * thickness);

                    float xOffset = End.TransX - Start.TransX;
                    float yOffset = End.TransY - Start.TransY;

                    var factory = new Factory();

                    var custom = factory.CreateCustom(null, Start.TransX, Start.TransY);

                    var p0 = factory.CreatePin(custom, sizeX, -sizeY, PinTypes.Snap);
                    var p1 = factory.CreatePin(custom, xOffset + sizeX, yOffset - sizeY, PinTypes.Snap);
                    var p2 = factory.CreatePin(custom, xOffset - sizeX, yOffset + sizeY, PinTypes.Snap);
                    var p3 = factory.CreatePin(custom, -sizeX, sizeY, PinTypes.Snap);

                    var l0 = factory.CreateLine(custom, p0, p1);
                    var l1 = factory.CreateLine(custom, p1, p2);
                    var l2 = factory.CreateLine(custom, p2, p3);
                    var l3 = factory.CreateLine(custom, p3, p0);

                    custom.Children.Add(l0);
                    custom.Children.Add(l1);
                    custom.Children.Add(l2);
                    custom.Children.Add(l3);

                    return custom;
                }
                return null;
            }
        }
    }

    #endregion
}