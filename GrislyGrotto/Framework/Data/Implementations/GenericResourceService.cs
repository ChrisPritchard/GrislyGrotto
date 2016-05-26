using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using GrislyGrotto.Framework.Data.Primitives;

namespace GrislyGrotto.Framework.Data.Implementations
{
    public class GenericResourceService : IResourceService
    {
        public Quote RandomQuote()
        {
            var quotes = XDocument.Load(HttpContext.Current.Server.MapPath("/resources/quotes.xml"));
            var random = new Random();
            var quote = quotes.Descendants("Quote").ElementAt(random.Next(quotes.Descendants("Quote").Count()));
            return new Quote
            {
                Author = quote.Attribute("Author").Value,
                Content = quote.Value
            };
        }

        public void ReturnFile(HttpContext fileRequestContext)
        {
            var fileName = fileRequestContext.Request.PhysicalPath;
            if (File.Exists(fileName))
            {
                fileRequestContext.Response.ContentType = UtilityExtensions.ContentTypeForExtension(fileName);
                fileRequestContext.Response.TransmitFile(fileName);
            }
            else if (Path.GetFileName(fileName).EqualsIgnoreCase("divinecircuits.jpg"))
            {
                fileRequestContext.Response.ContentType = UtilityExtensions.ContentTypeForExtension(fileName);
                DivineCircuits(Color.Black)
                    .Save(fileRequestContext.Response.OutputStream, ImageFormat.Jpeg); // png's didnt work on the server
            }
            else
                fileRequestContext.Response.StatusCode = 404;

            fileRequestContext.Response.End();
        }

        private static Bitmap DivineCircuits(Color foregroundColour)
        {
            const int width = 600, height = 600;
            var random = Container.GetInstance<Random>();

            var bitmap = new Bitmap(width, height);
            var graphics = Graphics.FromImage(bitmap);
            graphics.FillRectangle(new SolidBrush(Color.White), 0, 0, width, height);

            int mostLeft = width, mostRight = 0, mostTop = height, mostBottom = 0;

            const int segmentX = width/3, segmentY = height/3;
            for(var x = 1; x <= 2; x++)
                for (var y = 1; y <= 2; y++)
                    for (var i = 0; i < 5; i++)
                        RecurseDraw(graphics, 1, new Point(segmentX * x, segmentY * y), random, foregroundColour, 
                            ref mostLeft, ref mostRight, ref mostTop, ref mostBottom);

            mostLeft = Math.Max(0, mostLeft);
            mostRight = Math.Min(width, mostRight);
            mostTop = Math.Max(0, mostTop);
            mostBottom = Math.Min(height, mostBottom);

            var clippedBitmap = new Bitmap(mostRight - mostLeft, mostBottom - mostTop);
            var clippedGraphics = Graphics.FromImage(clippedBitmap);
            clippedGraphics.DrawImage(bitmap, 0, 0, new Rectangle(mostLeft, mostTop, mostRight - mostLeft, mostBottom - mostTop), GraphicsUnit.Pixel);

            return clippedBitmap;
        }

        private static void RecurseDraw(Graphics graphics, int iteration, Point point, Random random, Color foregroundColor, 
            ref int mostLeft, ref int mostRight, ref int mostTop, ref int mostBottom)
        {
            const int maxLength = 100;
            var currentLength = maxLength / (iteration + 1);
            if (currentLength < 5)
                return;

            int xOffSet = 0, yOffSet = 0;
            while (xOffSet == 0)
                xOffSet = random.Next(-1, 2);
            while (yOffSet == 0)
                yOffSet = random.Next(-1, 2);
            xOffSet *= currentLength;
            yOffSet *= currentLength;

            var newPoint = new Point(point.X + xOffSet, point.Y + yOffSet);
            graphics.DrawLine(new Pen(foregroundColor), point, newPoint);

            if (mostLeft > newPoint.X) mostLeft = newPoint.X;
            if (mostRight < newPoint.X) mostRight = newPoint.X;
            if (mostTop > newPoint.Y) mostTop = newPoint.Y;
            if (mostBottom < newPoint.Y) mostBottom = newPoint.Y;

            if (random.NextDouble() > 0.7)
                RecurseDraw(graphics, iteration + 1, newPoint, random, foregroundColor, 
                    ref mostLeft, ref mostRight, ref mostTop, ref mostBottom);
            RecurseDraw(graphics, iteration + 1, newPoint, random, foregroundColor, 
                ref mostLeft, ref mostRight, ref mostTop, ref mostBottom);
        }
    }
}