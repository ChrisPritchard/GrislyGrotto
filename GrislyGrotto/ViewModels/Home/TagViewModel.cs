using GrislyGrotto.ViewModels.Shared;

namespace GrislyGrotto.ViewModels.Home
{
    public class TagViewModel : LatestViewModel
    {
        public string Tag { get; set; }
        public string DisplayTag
        {
            get { return Tag.Replace('_', ' '); }
        }

        public TagViewModel(PostViewModel[] initialPosts, string tagName)
            : base(initialPosts)
        {
            Tag = tagName;
        }
    }
}