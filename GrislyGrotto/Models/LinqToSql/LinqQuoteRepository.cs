using System;
using System.Linq;
using GrislyGrotto.Models.DTO;

namespace GrislyGrotto.Models.LinqToSql
{
    public class LinqQuoteRepository : IQuoteRepository
    {
        private GrislyGrottoDBDataContext linqDataRepository;

        public LinqQuoteRepository(GrislyGrottoDBDataContext linqDataRepository)
        {
            this.linqDataRepository = linqDataRepository;
        }

        /// <summary>
        /// Retrieve a random quote from the database
        /// </summary>
        public QuoteInfo GetRandomQuote()
        {
            var count = linqDataRepository.Quotes.Count();
            var randomGenerator = new Random();
            var selected = randomGenerator.Next(0, count);

            Quote quote = linqDataRepository.Quotes.Take(selected + 1).ToList().Last();
            return new QuoteInfo(quote.Author, quote.Content);
        }
    }
}
