using GrislyGrotto.Models.DTO;

namespace GrislyGrotto.Models
{
    public interface IQuoteRepository
    {
        QuoteInfo GetRandomQuote();
    }
}
