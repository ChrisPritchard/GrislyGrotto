using System;
using System.Collections.Generic;

namespace GrislyGrotto.App.Shared
{
    public static class Events
    {
        private static readonly List<string> _recentEvents = new List<string>
        { FormatEvent("Application (re)started") };

        private static string FormatEvent(string @event)
        {
            var date = DateTime.UtcNow.Add(NzTimeZone);
            return @event + " at " + date.ToString("hh:mm:ss dd-MM-yy");
        }

        public static void Add(string @event)
        {
            _recentEvents.Add(FormatEvent(@event));
        }

        public static IEnumerable<string> GetEvents()
        {
            return _recentEvents;
        }
        
        public static TimeSpan NzTimeZone = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time").BaseUtcOffset;
    }
}