using GrislyGrotto.ViewModels.Shared;

namespace GrislyGrotto.ViewModels.Home
{
    public class LatestViewModel
    {
        public PostViewModel[] Posts { get; set; }
        public int Start { get; set; }
        public int Count { get; set; }

        public LatestViewModel(PostViewModel[] initialPosts)
        {
            Posts = initialPosts;
            Count = initialPosts.Length;
        }
    }
}