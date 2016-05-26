using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;

namespace GrislyGrotto
{
    internal static class Extensions
    {
        public static string AsMonthName(this int monthNum)
        {
            if (monthNum < 1 || monthNum > 12)
                throw new ArgumentOutOfRangeException("monthNum", "monthNum must be between 1 (January) and 12 (December)");

            return CultureInfo.CurrentUICulture.DateTimeFormat.GetMonthName(monthNum);
        }

        public static int AsMonthNum(this string monthName)
        {
            return Array.IndexOf(CultureInfo.CurrentUICulture.DateTimeFormat.MonthGenitiveNames.Select(m => m.ToLower()).ToArray(), monthName.ToLower()) + 1;
        }

        internal static string StripHtml(this string text)
        {
            return Regex.Replace(text, @"<(.|\n)*?>", string.Empty);
        }

        internal static string ToWebFormat(this DateTime dateTime)
        {
            return dateTime.ToString("dddd, d MMMM yyyy, hh:mm tt");
        }

        internal static string ToWebFormat(this TimeSpan timeSpan)
        {
            return (timeSpan.Days > 0 ? timeSpan.Days + " day" + (timeSpan.Days == 1 ? string.Empty : "s") + ", " : string.Empty)
                + (timeSpan.Hours > 0 ? timeSpan.Hours + " hour" + (timeSpan.Hours == 1 ? string.Empty : "s") + ", " : string.Empty)
                + timeSpan.Minutes + " minute" + (timeSpan.Minutes == 1 ? string.Empty : "s");
        }

        internal static void WriteJson(this HttpResponse response, object o)
        {
            var serializer = new JavaScriptSerializer();
            response.Write(serializer.Serialize(o));
        }
    }
}