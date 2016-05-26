using System.Collections.Generic;
using GrislyGrotto.Models.DTO;

namespace GrislyGrotto.Models
{
    public interface IBlogRepository
    {
        PostInfo[] GetLatestPosts(string userFullname, int count, ICommentRepository commentRepository);
        PostInfo[] GetPostsByMonth(string userFullname, MonthInfo month, ICommentRepository commentRepository);
        PostInfo GetPostByID(int postID, ICommentRepository commentRepository);

        void AddPost(PostInfo post);
        void UpdatePost(PostInfo post);

        Dictionary<MonthInfo, int> GetMonthPostCounts(string userFullname);
        PostInfo[] GetRecentPostEntries(string userFullname, int count);
    }
}
