using GrislyGrotto.Infrastructure.Domain;

namespace GrislyGrotto.Website.Models.ViewModels
{
    public class PostWithCommentCount : Post
    {
        public int CommentCount { get; private set; }

        public PostWithCommentCount(Post initialPost, int commentCount): 
            base(initialPost.PostID, initialPost.EntryDate, initialPost.Author, initialPost.Title, initialPost.Content)
        {
            CommentCount = commentCount;
        }
    }
}
