using GrislyGrotto.Models.DTO;

namespace GrislyGrotto.Models
{
    public interface ICommentRepository
    {
        int GetCommentCountOfPost(int postID);
        CommentInfo[] GetCommentsOfPost(int postID);

        void AddComment(CommentInfo comment);
    }
}
