using GrislyGrotto.Infrastructure.Domain;

namespace GrislyGrotto.Infrastructure
{
    public interface IQuoteRepository
    {
        Quote GetRandomQuote();
    }
}
