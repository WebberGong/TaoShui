﻿using System.Collections.Generic;
using System.Windows.Forms;

namespace CaptchaRecogniser
{
    public class Test
    {
        public static void DoTest()
        {
            var str1 = Recogniser.Instance.RecognizeFromImage(Application.StartupPath + @"\image\test\test1.jpg", 100, 0,
                new HashSet<EnumCaptchaType>());
            var str2 = Recogniser.Instance.RecognizeFromImage(Application.StartupPath + @"\image\test\test2.jpg", 100, 0,
                new HashSet<EnumCaptchaType>());
            var str3 = Recogniser.Instance.RecognizeFromImage(Application.StartupPath + @"\image\test\test3.jpg", 4, 3,
                new HashSet<EnumCaptchaType> {EnumCaptchaType.Number});
            var str4 = Recogniser.Instance.RecognizeFromImage(Application.StartupPath + @"\image\test\test4.jpg", 4, 3,
                new HashSet<EnumCaptchaType> {EnumCaptchaType.Number});
            var str5 = Recogniser.Instance.RecognizeFromImage(Application.StartupPath + @"\image\test\test5.jpg", 4, 3,
                new HashSet<EnumCaptchaType> {EnumCaptchaType.Number});
            var str6 = Recogniser.Instance.RecognizeFromImage(Application.StartupPath + @"\image\test\test6.jpg", 4, 3,
                new HashSet<EnumCaptchaType> {EnumCaptchaType.Number});
            var str7 = Recogniser.Instance.RecognizeFromImage(Application.StartupPath + @"\image\test\test7.jpg", 4, 3,
                new HashSet<EnumCaptchaType> {EnumCaptchaType.Number});
            var str8 = Recogniser.Instance.RecognizeFromImage(Application.StartupPath + @"\image\test\test8.jpg", 4, 1,
                new HashSet<EnumCaptchaType> {EnumCaptchaType.Number});
            var str9 = Recogniser.Instance.RecognizeFromImage(Application.StartupPath + @"\image\test\test9.jpg", 4, 3,
                new HashSet<EnumCaptchaType> {EnumCaptchaType.Number});
            var str10 = Recogniser.Instance.RecognizeFromImage(Application.StartupPath + @"\image\test\test10.jpg", 4, 3,
                new HashSet<EnumCaptchaType> {EnumCaptchaType.Number});
            var str11 = Recogniser.Instance.RecognizeFromImage(Application.StartupPath + @"\image\test\test11.jpg", 4, 3,
                new HashSet<EnumCaptchaType> {EnumCaptchaType.Number});
            var str12 = Recogniser.Instance.RecognizeFromImage(Application.StartupPath + @"\image\test\test12.jpg", 4, 3,
                new HashSet<EnumCaptchaType> {EnumCaptchaType.Number});
        }
    }
}