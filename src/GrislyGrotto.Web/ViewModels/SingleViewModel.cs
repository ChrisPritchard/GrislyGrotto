
using System.ComponentModel.DataAnnotations;

namespace GrislyGrotto
{
    public class SingleViewModel: Post
    {
        [Required]
        public string CommentAuthor { get; set; }
        [Required]
        public string CommentContent { get; set; }

        public SingleViewModel() { }

        public SingleViewModel(Post post)
        {
            Key = post.Key;
            Title = post.Title;
            Author = post.Author;
            Content = post.Content;
            Date = post.Date;
            Comments = post.Comments;
        }

        public SingleViewModel(Post post, string commentAuthor, string commentContent = null)
            : this(post)
        {
            CommentAuthor = commentAuthor;
            CommentContent = commentContent;
        }
    }
}