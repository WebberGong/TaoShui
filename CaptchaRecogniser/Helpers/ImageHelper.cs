using System;
using System.Drawing;
using System.Drawing.Imaging;
using AForge.Imaging;
using AForge.Imaging.Filters;
using Image = System.Drawing.Image;

namespace CaptchaRecogniser.Helpers
{
    public static class ImageHelper
    {
        private static Bitmap BitmapGrayscale(Image image)
        {
            var grayScaler = new Grayscale(0.2125, 0.7154, 0.0721);
            return grayScaler.Apply(new Bitmap(image));
        }

        /// <summary>
        ///     Load image from file without filetype exception
        /// </summary>
        /// <param name="filename">Name of file</param>
        /// <returns>Loaded image or null otherwise</returns>
        public static Image SafeLoadFromFile(string filename)
        {
            try
            {
                return Image.FromFile(filename);
            }
            catch (OutOfMemoryException)
            {
                //The file does not have a valid image format.
                //-or- GDI+ does not support the pixel format of the file

                return null;
            }
        }

        /// <summary>
        ///     Blur image with Gauss
        /// </summary>
        /// <param name="image"></param>
        /// <param name="sigma">Gaussia sigma</param>
        /// <param name="kernelSize">Gaussia kermel</param>
        /// <returns></returns>
        public static Image Gauss(this Image image, double sigma, int kernelSize)
        {
            var blur = new GaussianBlur(sigma, kernelSize);
            return blur.Apply(new Bitmap(image));
        }

        /// <returns>Cropped image</returns>
        public static Image Crop(this Image image, Rectangle rectangle)
        {
            var cropper = new Crop(rectangle);
            return cropper.Apply(new Bitmap(image));
        }

        /// <returns>Extract Blobs from image</returns>
        public static Blob[] ExtractBlobs(this Image image)
        {
            var processImage = BitmapGrayscale(image);

            if (processImage.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new NotSupportedException("Blob extractor can be applied to binary 8bpp images only");

            var bc = new BlobCounter(processImage);

            return bc.GetObjects(processImage, true);
        }

        /// <returns>Invert image</returns>
        public static Image Invert(this Image image)
        {
            var filter = new Invert();
            // apply the filter
            return filter.Apply(BitmapGrayscale(image));
        }

        /// <summary>
        ///     Binarize image with threshold filter
        /// </summary>
        /// <param name="image"></param>
        /// <param name="threshold">threshold for binarization</param>
        public static Image Threshold(this Image image, byte threshold)
        {
            var thresholdFilter = new Threshold(threshold);
            return thresholdFilter.Apply(BitmapGrayscale(image));
        }

        /// <summary>
        ///     Binarize image with SIS threshold filter
        /// </summary>
        /// <param name="image"></param>
        public static Image SisThreshold(this Image image)
        {
            var thresholdFilter = new SISThreshold();
            return thresholdFilter.Apply(BitmapGrayscale(image));
        }

        /// <summary>
        ///     Binarize image with Otsu threshold filter
        /// </summary>
        /// <param name="image"></param>
        public static Image OtsuThreshold(this Image image)
        {
            var thresholdFilter = new OtsuThreshold();
            return thresholdFilter.Apply(BitmapGrayscale(image));
        }

        /// <summary>
        ///     Binarize image with iterative threshold filter
        /// </summary>
        /// <param name="image"></param>
        /// <param name="threshold">threshold for binarization</param>
        public static Image IterativeThreshold(this Image image, byte threshold)
        {
            var thresholdFilter = new IterativeThreshold(2, threshold);
            return thresholdFilter.Apply(BitmapGrayscale(image));
        }

        /// <summary>
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static Image CannyEdges(this Image image)
        {
            var cannyEdge = new CannyEdgeDetector();
            return cannyEdge.Apply(BitmapGrayscale(image));
        }

        /// <summary>
        ///     Aforge grayscale
        /// </summary>
        public static Image Grayscale(this Image image)
        {
            var grayScaler = new Grayscale(0.2125, 0.7154, 0.0721);
            return grayScaler.Apply(new Bitmap(image));
        }

        /// <summary>
        ///     Aforge median dilatation morfology with default kernel
        /// </summary>
        public static Image Dilatation(this Image image)
        {
            // default kernel
            var se = new short[,]
            {
                {1, 1, 1},
                {1, 1, 1},
                {1, 1, 1}
            };

            var dilatationFilter = new Dilatation(se);
            return dilatationFilter.Apply(BitmapGrayscale(image));
        }

        /// <summary>
        ///     Aforge median filtration
        /// </summary>
        /// <param name="image"></param>
        /// <param name="core">Median core size</param>
        public static Image Median(this Image image, int core)
        {
            var bitmapImage = new Bitmap(image);
            var medianFilter = new Median(core);
            medianFilter.ApplyInPlace(bitmapImage);
            return bitmapImage;
        }

        /// <summary>
        ///     Implementation of Kuwahara filter. See https://en.wikipedia.org/wiki/Kuwahara_filter
        /// </summary>
        /// <param name="image"></param>
        /// <param name="coreSize">Kuwahara core size</param>
        public static Image Kuwahara(this Image image, int coreSize)
        {
            var tempBitmap = new Bitmap(image);
            var newBitmap = new Bitmap(tempBitmap.Width, tempBitmap.Height);
            var newGraphics = Graphics.FromImage(newBitmap);
            newGraphics.DrawImage(tempBitmap, new Rectangle(0, 0, tempBitmap.Width, tempBitmap.Height),
                new Rectangle(0, 0, tempBitmap.Width, tempBitmap.Height), GraphicsUnit.Pixel);
            newGraphics.Dispose();
            int[] apetureMinX = {-(coreSize/2), 0, -(coreSize/2), 0};
            int[] apetureMaxX = {0, coreSize/2, 0, coreSize/2};
            int[] apetureMinY = {-(coreSize/2), -(coreSize/2), 0, 0};
            int[] apetureMaxY = {0, 0, coreSize/2, coreSize/2};
            for (var x = 0; x < newBitmap.Width; ++x)
                for (var y = 0; y < newBitmap.Height; ++y)
                {
                    int[] rValues = {0, 0, 0, 0};
                    int[] gValues = {0, 0, 0, 0};
                    int[] bValues = {0, 0, 0, 0};
                    int[] numPixels = {0, 0, 0, 0};
                    int[] maxRValue = {0, 0, 0, 0};
                    int[] maxGValue = {0, 0, 0, 0};
                    int[] maxBValue = {0, 0, 0, 0};
                    int[] minRValue = {255, 255, 255, 255};
                    int[] minGValue = {255, 255, 255, 255};
                    int[] minBValue = {255, 255, 255, 255};
                    for (var i = 0; i < 4; ++i)
                        for (var x2 = apetureMinX[i]; x2 < apetureMaxX[i]; ++x2)
                        {
                            var tempX = x + x2;
                            if ((tempX >= 0) && (tempX < newBitmap.Width))
                                for (var y2 = apetureMinY[i]; y2 < apetureMaxY[i]; ++y2)
                                {
                                    var tempY = y + y2;
                                    if ((tempY >= 0) && (tempY < newBitmap.Height))
                                    {
                                        var tempColor = tempBitmap.GetPixel(tempX, tempY);
                                        rValues[i] += tempColor.R;
                                        gValues[i] += tempColor.G;
                                        bValues[i] += tempColor.B;
                                        if (tempColor.R > maxRValue[i])
                                            maxRValue[i] = tempColor.R;
                                        else if (tempColor.R < minRValue[i])
                                            minRValue[i] = tempColor.R;

                                        if (tempColor.G > maxGValue[i])
                                            maxGValue[i] = tempColor.G;
                                        else if (tempColor.G < minGValue[i])
                                            minGValue[i] = tempColor.G;

                                        if (tempColor.B > maxBValue[i])
                                            maxBValue[i] = tempColor.B;
                                        else if (tempColor.B < minBValue[i])
                                            minBValue[i] = tempColor.B;
                                        ++numPixels[i];
                                    }
                                }
                        }
                    var j = 0;
                    var minDifference = 10000;
                    for (var i = 0; i < 4; ++i)
                    {
                        var currentDifference = maxRValue[i] - minRValue[i] + (maxGValue[i] - minGValue[i]) +
                                                (maxBValue[i] - minBValue[i]);
                        if ((currentDifference < minDifference) && (numPixels[i] > 0))
                        {
                            j = i;
                            minDifference = currentDifference;
                        }
                    }

                    var meanPixel = Color.FromArgb(rValues[j]/numPixels[j],
                        gValues[j]/numPixels[j],
                        bValues[j]/numPixels[j]);
                    newBitmap.SetPixel(x, y, meanPixel);
                }
            return newBitmap;
        }
    }
}