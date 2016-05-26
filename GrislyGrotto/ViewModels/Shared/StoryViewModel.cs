using System;

namespace GrislyGrotto.ViewModels.Shared
{
    public class StoryViewModel
    {
        public int ID { get; set; }
        public string Author { get; set; }
        public string Title { get; set; }
        public DateTime Created { get; set; }
        public int WordCount { get; set; }
    }
}