using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Utils
{
    public class Common
    {
        public static string GetDescriptionAttribute(Enum e)
        {
            var enumType = e.GetType();
            var enumName = Enum.GetName(enumType, e);
            var fieldInfo = enumType.GetField(enumName);

            var attributes = fieldInfo.GetCustomAttributes(
                typeof (DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            return enumName;
        }

        public static string GetDescriptionAttribute(PropertyInfo propertyInfo)
        {
            var attributes = propertyInfo.GetCustomAttributes(
                typeof (DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            return propertyInfo.Name;
        }

        public static string GetNumericFromString(string str)
        {
            return str.Where(c => c >= 48 && c <= 58).Aggregate(string.Empty, (current, c) => current + c);
        }
    }
}