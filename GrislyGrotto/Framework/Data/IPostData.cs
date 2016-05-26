using System.Collections.Generic;
using GrislyGrotto.Framework.Data.Primitives;

namespace GrislyGrotto.Framework.Data
{
    public interface IPostData
    {
        IEnumerable<Post> LatestPosts(int count, string user = null);
        IEnumerable<Post> PostsByStatus(string status, string user = null);
        IEnumerable<Post> PostsForMonth(int year, int month, string user = null);
        IEnumerable<Post> SearchResults(string searchTerm);
        IEnumerable<MonthCount> MonthPostCounts(string user = null);
        Post SinglePost(int id);
        int AddOrEditPost(Post post);
        void AddComment(Comment comment, int postID);
    }
}