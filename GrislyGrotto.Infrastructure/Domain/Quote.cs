
namespace GrislyGrotto.Infrastructure.Domain
{
    public class Quote
    {
        public string Author { get; private set; }
        public string Text { get; private set; }

        public Quote(string author, string text)
        {
            Author = author;
            Text = text;
        }
    }
}
