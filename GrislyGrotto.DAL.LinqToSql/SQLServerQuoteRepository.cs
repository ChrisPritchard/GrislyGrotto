using System;
using System.Configuration;
using System.Linq;
using GrislyGrotto.Infrastructure;
using Domain = GrislyGrotto.Infrastructure.Domain;

namespace GrislyGrotto.DAL.SQLServer
{
    public class SQLServerQuoteRepository : IQuoteRepository
    {
        private GrislyGrottoDBDataContext linqDataRepository;

        public SQLServerQuoteRepository(ConnectionStringSettingsCollection connectionStrings)
        {
            this.linqDataRepository = new GrislyGrottoDBDataContext(connectionStrings);
        }

        /// <summary>
        /// Retrieve a random quote from the database
        /// </summary>
        public Domain.Quote GetRandomQuote()
        {
            var count = linqDataRepository.Quotes.Count();
            var randomGenerator = new Random();
            var selected = randomGenerator.Next(0, count);

            var quote = linqDataRepository.Quotes.Take(selected + 1).ToList().Last();
            return new Domain.Quote(quote.Author, quote.Content);
        }
    }
}
