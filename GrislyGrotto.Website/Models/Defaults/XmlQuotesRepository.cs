using System;
using System.Collections.Generic;
using System.Xml;
using GrislyGrotto.Infrastructure;
using GrislyGrotto.Infrastructure.Domain;

namespace GrislyGrotto.Website.Models.Defaults
{
    public class XmlQuotesRepository : IQuoteRepository
    {
        private Quote[] quotes;
        private Random numberGenerator;

        public XmlQuotesRepository(string xmlQuoteFilePath)
        {
            var quotesFile = new XmlDocument();
            quotesFile.Load(xmlQuoteFilePath);

            var quoteLoader = new List<Quote>();
            foreach (XmlNode quote in quotesFile.SelectNodes("quotes/quote"))
            {
                quoteLoader.Add(new Quote(quote.Attributes["author"].Value, quote.InnerXml));
            }

            quotes = quoteLoader.ToArray();

            numberGenerator = new Random();
        }

        public Quote GetRandomQuote()
        {
            var quoteIndex = numberGenerator.Next(0, quotes.Length);
            return quotes[quoteIndex];
        }
    }
}
