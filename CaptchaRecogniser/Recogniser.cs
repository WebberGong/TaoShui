using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Patagames.Ocr;
using Patagames.Ocr.Enums;
using Utils;

namespace CaptchaRecogniser
{
    public static class Recogniser
    {
        private const int DefaultMaxLength = 50;
        private const int DefaultBinarizeCount = 3;
        private static readonly OcrApi Ocr;
        private static readonly HashSet<EnumCaptchaType> DefaultCaptchaTypeSet;

        static Recogniser()
        {
            Ocr = OcrApi.Create();
            Ocr.Init(Languages.English, Application.StartupPath, OcrEngineMode.OEM_TESSERACT_ONLY);

            var allCaptchaType = Enum.GetValues(typeof (EnumCaptchaType));
            DefaultCaptchaTypeSet = new HashSet<EnumCaptchaType>();
            foreach (var captchaType in allCaptchaType)
            {
                DefaultCaptchaTypeSet.Add((EnumCaptchaType) captchaType);
            }
        }

        public static string RecognizeFromImage(Bitmap bitMap, int maxLength = DefaultMaxLength,
            int binarizeCount = DefaultBinarizeCount, HashSet<EnumCaptchaType> captchaTypeSet = null)
        {
            return Recognize(bitMap, maxLength, binarizeCount, captchaTypeSet);
        }

        public static string RecognizeFromImage(string imagePath, int maxLength = DefaultMaxLength,
            int binarizeCount = DefaultBinarizeCount, HashSet<EnumCaptchaType> captchaTypeSet = null)
        {
            using (var bitMap = Image.FromFile(imagePath) as Bitmap)
            {
                return Recognize(bitMap, maxLength, binarizeCount, captchaTypeSet);
            }
        }

        public static string RecognizeFromImage(Stream stream, int maxLength = DefaultMaxLength,
            int binarizeCount = DefaultBinarizeCount, HashSet<EnumCaptchaType> captchaTypeSet = null)
        {
            using (var bitMap = Image.FromStream(stream) as Bitmap)
            {
                return Recognize(bitMap, maxLength, binarizeCount, captchaTypeSet);
            }
        }

        public static string RecognizeFromImage(Uri imageUrl, int maxLength = DefaultMaxLength,
            int binarizeCount = DefaultBinarizeCount, HashSet<EnumCaptchaType> captchaTypeSet = null)
        {
            var request = WebRequest.Create(imageUrl) as HttpWebRequest;
            if (request != null)
            {
                var response = (request.GetResponse()) as HttpWebResponse;
                if (response != null)
                {
                    var stream = response.GetResponseStream();
                    if (stream != null)
                    {
                        using (var bitMap = Image.FromStream(stream) as Bitmap)
                        {
                            return Recognize(bitMap, maxLength, binarizeCount, captchaTypeSet);
                        }
                    }
                }
            }
            return string.Empty;
        }

        private static string Recognize(Bitmap bitMap, int maxLength = DefaultMaxLength,
            int binarizeCount = DefaultBinarizeCount, HashSet<EnumCaptchaType> captchaTypeSet = null)
        {
            var captha = new Captcha(bitMap);
            for (var i = 0; i < binarizeCount; i++)
            {
                captha.Binarize(200);
            }
            var image = captha.Image as Bitmap;

            var validValueListBuilder = new StringBuilder();

            if (captchaTypeSet == null || captchaTypeSet.Count < 1)
            {
                captchaTypeSet = DefaultCaptchaTypeSet;
            }
            foreach (var captchaType in captchaTypeSet)
            {
                validValueListBuilder.Append(Common.GetDescriptionAttribute(captchaType));
            }

            string plainText;
            lock (Ocr)
            {
                Ocr.SetVariable("tessedit_char_whitelist", validValueListBuilder.ToString());
                plainText = Ocr.GetTextFromImage(image);
            }

            if (!captchaTypeSet.Contains(EnumCaptchaType.WhiteSpace))
            {
                var matches =
                    new Regex("[^" + Common.GetDescriptionAttribute(EnumCaptchaType.WhiteSpace) + "]").Matches(plainText);
                var enumerator = matches.GetEnumerator();
                var filteredBuilder = new StringBuilder();
                var count = 0;
                while (enumerator.MoveNext() && count < maxLength)
                {
                    filteredBuilder.Append(enumerator.Current.ToString().ToCharArray());
                    count++;
                }
                plainText = filteredBuilder.ToString();
            }
            return plainText;
        }
    }
}