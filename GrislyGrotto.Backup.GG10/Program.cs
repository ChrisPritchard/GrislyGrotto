using GrislyGrotto.Backup.GG10.Data;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace GrislyGrotto.Backup.GG10
{
    class Program
    {
        static void Main(string[] args)
        {
            Container allData;
            var startTime = DateTime.Now;
            Console.Write("Starting...");

            var connectionString = ConfigurationManager.ConnectionStrings["GrislyGrotto_DB"].ConnectionString;
            using (var context = new GrislyGrottoContext(connectionString))
            {
                allData = new Container
                {
                    Posts = context.Posts.ToArray(),
                    Comments = context.Comments.ToArray()
                };

                if (args.Length == 0 || args[1] != "-ignoreusercontent")
                {
                    var userContentZip = ZipFile.Open("UserContent_" + startTime.Ticks + ".zip", ZipArchiveMode.Create);
                    var allKeys = context.Assets.Select(a => a.Key).ToArray();
                    foreach (var key in allKeys)
                    {
                        try
                        {
                            var asset = context.Assets.First(a => a.Key == key);
                            var entry = userContentZip.CreateEntry(asset.Key, CompressionLevel.Fastest);
                            using (var stream = entry.Open())
                                stream.Write(asset.Data, 0, asset.Data.Length);
                        }
                        catch { continue; }
                    }
                    userContentZip.Dispose();
                }
            }

            var serializer = new XmlSerializer(typeof(Container));
            var xmlBuilder = new StringBuilder();
            serializer.Serialize(new StringWriter(xmlBuilder), allData);

            File.WriteAllText("Backup_" + startTime.Ticks + ".xml", xmlBuilder.ToString());

            Console.WriteLine("Finished. {0} seconds", DateTime.Now.Subtract(startTime).TotalSeconds);
            if (Debugger.IsAttached)
                Console.ReadKey(true);
        }
    }

    public class Container
    {
        public Post[] Posts { get; set; }
        public Comment[] Comments { get; set; }
    }
}
