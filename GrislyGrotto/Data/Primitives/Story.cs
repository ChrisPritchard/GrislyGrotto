using System;

namespace GrislyGrotto.Data.Primitives
{
    internal class Story
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime TimePosted { get; set; }
        public int WordCount { get; set; }

        public Story()
        { }

        public Story(Post basePost)
        {
            Id = basePost.Id.Value;
            Title = basePost.Title;
            Author = basePost.Author;
            TimePosted = basePost.TimePosted;
            WordCount = basePost.Content.Split(' ').Length;
        }
    }
}