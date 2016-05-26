using System.Collections.Generic;
using System.IO;
using System.Web;
using GrislyGrotto.Data.Primitives;

namespace GrislyGrotto.Data
{
    internal interface IPosts
    {
        IEnumerable<Post> LatestPosts(string user = null);
        IEnumerable<Story> PostsThatAreStories(string user = null);
        IEnumerable<Post> PostsForMonth(int year, int month, string user = null);
        IEnumerable<Post> SearchResults(string searchTerm);
        IEnumerable<MonthCount> MonthPostCounts(string user = null);
        Post SinglePost(int id);
        int AddOrEditPost(Post post);
        void AddComment(Comment comment, int postId);
    }

    internal static class PostsFactory
    {
        public static IPosts GetInstance()
        {
            return File.Exists(HttpContext.Current.Server.MapPath("/app_data/grislygrotto.db3"))
                ? (IPosts)new DatabasePosts()
                : new MockPosts();
        }
    }
}
