using System.Drawing;
using System.Linq;
using CaptchaRecogniser.Helpers;

namespace CaptchaRecogniser
{
    public class Captcha
    {
        public Captcha(Image image)
        {
            Image = image;
        }

        public Image Image { get; private set; }

        public void Binarize(byte threshold)
        {
            Image = Image.Median(0).Threshold(threshold).Invert();
        }

        public Image[] ExtractSymbols(int numberOfSymbols)
        {
            var symbolCounter = 0;

            var images = new Image[numberOfSymbols];

            foreach (var blob in Image.ExtractBlobs().OrderBy(si => si.Rectangle.X))
            {
                if (blob.Area > 30)
                {
                    var newSymbol = new Bitmap(45, 45);

                    using (var g = Graphics.FromImage(newSymbol))
                    {
                        var blobBitmap =
                            Image.Crop(new Rectangle(blob.Rectangle.X, blob.Rectangle.Y - 10, blob.Rectangle.Width,
                                blob.Rectangle.Height + 10));
                        g.Clear(Color.Black);
                        var x = (newSymbol.Width - blobBitmap.Width)/2;
                        var y = (newSymbol.Height - blobBitmap.Height)/2;
                        g.DrawImage(blobBitmap, x, y);
                    }

                    if (symbolCounter < numberOfSymbols)
                        images[symbolCounter] = newSymbol;

                    symbolCounter++;
                }
            }

            return images;
        }
    }
}