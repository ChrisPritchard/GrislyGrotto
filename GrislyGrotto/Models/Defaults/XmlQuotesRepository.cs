using System;
using System.Collections.Generic;
using System.Xml;
using GrislyGrotto.Models.DTO;

namespace GrislyGrotto.Models.Defaults
{
    public class XmlQuotesRepository : IQuoteRepository
    {
        private QuoteInfo[] quotes;
        private Random numberGenerator;

        public XmlQuotesRepository(string xmlQuoteFilePath)
        {
            var quotesFile = new XmlDocument();
            quotesFile.Load(xmlQuoteFilePath);

            var quoteLoader = new List<QuoteInfo>();
            foreach (XmlNode quote in quotesFile.SelectNodes("quotes/quote"))
            {
                quoteLoader.Add(new QuoteInfo(quote.Attributes["author"].Value, quote.InnerXml));
            }

            quotes = quoteLoader.ToArray();

            numberGenerator = new Random();
        }

        public QuoteInfo GetRandomQuote()
        {
            var quoteIndex = numberGenerator.Next(0, quotes.Length);
            return quotes[quoteIndex];
        }
    }
}
