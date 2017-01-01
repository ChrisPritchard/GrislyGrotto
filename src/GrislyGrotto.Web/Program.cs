using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace GrislyGrotto
{
    public class Program
    {
        public static TimeZoneInfo NzTimeZone = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time");

        public static DateTime NzTime(DateTime? source = null)
            => TimeZoneInfo.ConvertTime(source ?? DateTime.UtcNow, TimeZoneInfo.Utc, NzTimeZone);

        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }

        private static readonly List<string> _recentEvents = new List<string> { FormatEvent("Application (re)started") };

        private static string FormatEvent(string @event) => @event + " at " + NzTime().ToString("hh:mm:ss dd-MM-yy");

        public static void AddEvent(string @event) => _recentEvents.Add(FormatEvent(@event));

        public static IEnumerable<string> GetEvents() => _recentEvents;
    }
}
