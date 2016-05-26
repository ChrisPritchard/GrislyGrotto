using GrislyGrotto.ViewModels.Shared;

namespace GrislyGrotto.ViewModels.Home
{
    public class SearchViewModel : LatestViewModel
    {
        public string SearchTerm { get; set; }

        public SearchViewModel(PostViewModel[] initialPosts, string searchTerm)
            : base(initialPosts)
        {
            SearchTerm = searchTerm;
        }
    }
}