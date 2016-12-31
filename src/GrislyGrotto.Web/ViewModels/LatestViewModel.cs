using System;

namespace GrislyGrotto
{
    public class LatestViewModel
    {
        public Post[] Posts { get; set; }

        public DateTime? Month { get; set; }

        public int Page { get; set; }

        public LatestViewModel(Post[] posts, int page)
        {
            Posts = posts;
            Page = page;
        }

        public LatestViewModel(Post[] posts, string month, int year)
        {
            Posts = posts;
            Month = DateTime.Parse($"1/{month}/{year}");
        }
    }
}