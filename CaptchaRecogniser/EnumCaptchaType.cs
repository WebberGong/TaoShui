using System.ComponentModel;

namespace CaptchaRecogniser
{
    public enum EnumCaptchaType
    {
        //Regex: [0-9]
        [Description("0123456789")]
        Number = 1,

        //Regex: [A-Z]
        [Description("ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
        UpperCaseLetter = 2,

        //Regex: [a-z]
        [Description("abcdefghijklmnopqrstuvwxyz")]
        LowerCaseLetter = 4,

        //Regex: [\\s]
        [Description("\f\n\r\t\v")]
        WhiteSpace = 8,

        //Regex: ((?=[\x21-\x7e]+)[^A-Za-z0-9])
        [Description("`~!@#$%^&*()-_=+[{]}\\|;:'\",<.>/?")]
        Punctuation = 16
    }
}