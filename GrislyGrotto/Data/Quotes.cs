using System;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using GrislyGrotto.Data.Primitives;

namespace GrislyGrotto.Data
{
    internal static class Quotes
    {
        public static Quote RandomQuote()
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
    }
}