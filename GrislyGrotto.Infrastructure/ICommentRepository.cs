using System.Collections.Generic;
using GrislyGrotto.Infrastructure.Domain;

namespace GrislyGrotto.Infrastructure
{
    public interface ICommentRepository
    {
        IEnumerable<Comment> GetCommentsOfPost(int postID);

        void AddComment(int postID, string author, string text);
    }
}
