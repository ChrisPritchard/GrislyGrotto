using System.Collections.Generic;
using System.Linq;
using GrislyGrotto.ViewModels.Shared;

namespace GrislyGrotto.ViewModels.Post
{
    public enum ViewType
    {
        Normal, HTML
    }

    public class EditorViewModel
    {
        public int? ID { get; set; }

        public string Title { get; set; }
        public string Content { get; set; }

        public ViewType ViewType { get; set; }
        public bool IsStory { get; set; }

        public List<string> ExistingTags { get; set; }
        public List<string> SelectedTags { get; set; }

        public List<CommentViewModel> Comments { get; set; }

        public EditorViewModel()
        {
            SelectedTags = new List<string>();
        }

        public static EditorViewModel FromPost(Data.Post post)
        {
            return new EditorViewModel
            {
                ID = post.ID,
                Title = post.Title,
                Content = post.Content,
                IsStory = post.Type == Data.PostType.Story,
                SelectedTags = post.Tags.Select(t => t.Text).ToList()
            };
        }

        internal void UpdateWith(PostViewModel saved)
        {
            Title = saved.Title;
            Content = saved.Content;
            IsStory = saved.Type == PostTypeViewModel.Story;
        }
    }
}