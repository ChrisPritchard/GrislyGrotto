namespace GrislyGrotto.Framework.Data.Primitives
{
    public class Story
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int WordCount { get; set; }

        public Story()
        { }

        public Story(Post basePost)
        {
            ID = basePost.ID.Value;
            Title = basePost.Title;
            Author = basePost.Username;
            WordCount = basePost.RawContent.Split(' ').Length;
        }
    }
}