namespace GrislyGrotto.Models.DTO
{
    public class QuoteInfo
    {
        public string Author { get; private set; }
        public string Text { get; private set; }

        public QuoteInfo(string author, string text)
        {
            Author = author;
            Text = text;
        }
    }
}
