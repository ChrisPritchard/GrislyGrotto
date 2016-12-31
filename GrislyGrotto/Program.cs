using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace GrislyGrotto
{
    public class Program
    {
        public static TimeSpan NzTimeZone = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time").BaseUtcOffset;

        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
