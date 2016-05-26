using System;

namespace GrislyGrotto.Website.Models
{
    public static class Extensions
    {
        public static string AsFormattedDate(this DateTime date)
        {
            if (date.ToString("dd MM yyyy").Equals(DateTime.Now.ToString("dd MM yyyy")))
            {
                return "Today, " + date.ToString("h:mm tt");
            }
            else if (date.ToString("dd MM yyyy").Equals(DateTime.Now.AddDays(-1).ToString("dd MM yyyy")))
            {
                return "Yesterday, " + date.ToString("h:mm tt");
            }
            else
                return date.ToString("dddd, dd/MM/yyyy, h:mm tt");
        }

        public static string AsFormattedText(this TimeSpan timespan)
        {
            return (timespan.Days > 0 ? timespan.Days + " day" + (timespan.Days == 1 ? string.Empty : "s") + ", " : string.Empty)
                + (timespan.Hours > 0 ? timespan.Hours + " hour" + (timespan.Hours == 1 ? string.Empty : "s") + ", " : string.Empty)
                + timespan.Minutes + " minute" + (timespan.Minutes == 1 ? string.Empty : "s") + " ago";
        }
    }
}
