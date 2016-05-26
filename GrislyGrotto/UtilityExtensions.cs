using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace GrislyGrotto
{
    public static class UtilityExtensions
    {
        public static string AsMonthName(this int monthNum)
        {
            if(monthNum < 1 || monthNum > 12)
                throw new ArgumentOutOfRangeException("monthNum", "monthNum must be between 1 (January) and 12 (December)");

            return CultureInfo.CurrentUICulture.DateTimeFormat.GetMonthName(monthNum);
        }

        public static int AsMonthNum(this string monthName)
        {
            return Array.IndexOf(CultureInfo.CurrentUICulture.DateTimeFormat.MonthGenitiveNames.Select(m => m.ToLower()).ToArray(), monthName.ToLower()) + 1;
        }

        public static XElement AsXml(this object serializable)
        {
            if (typeof(XElement).IsAssignableFrom(serializable.GetType()))
                return (XElement) serializable;
            var serializer = new XmlSerializer(serializable.GetType());
            var xmlStringWriter = new StringWriter();
            serializer.Serialize(xmlStringWriter, serializable);
            return XElement.Parse(xmlStringWriter.ToString());
        }

        public static bool EqualsIgnoreCase(this string thisString, string otherString)
        {
            return string.Compare(thisString, otherString, true) == 0;
        }

        public static bool IsInteger(this object o)
        {
            try
            {
                Convert.ToInt32(o);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string ToWebFormat(this DateTime dateTime)
        {
            return dateTime.ToString("dddd, d MMMM yyyy, hh:mm tt");
        }

        public static string ToWebFormat(this TimeSpan timeSpan)
        {
            return (timeSpan.Days > 0 ? timeSpan.Days + " day" + (timeSpan.Days == 1 ? string.Empty : "s") + ", " : string.Empty)
                + (timeSpan.Hours > 0 ? timeSpan.Hours + " hour" + (timeSpan.Hours == 1 ? string.Empty : "s") + ", " : string.Empty)
                + timeSpan.Minutes + " minute" + (timeSpan.Minutes == 1 ? string.Empty : "s");
        }

        public static string ContentTypeForExtension(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();

            switch (extension)
            {
                case ".gif":
                    return "image/gif";
                case ".png":
                    return "image/png";
                case ".bmp":
                    return "image/bitmap";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".css":
                    return "text/css";
                case ".js":
                    return "application/x-javascript";
                default:
                    return "text/html";
            }
        }

        public static string Value(this Dictionary<string, string> dictionary, string key)
        {
            return (dictionary.ContainsKey(key) && dictionary[key] != null) ? dictionary[key] : string.Empty;
        }

        public static string StripHtml(this string text)
        {
            return Regex.Replace(text, @"<(.|\n)*?>", string.Empty);
        }
    }
}