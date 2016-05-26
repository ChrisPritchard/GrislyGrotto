using System;
using System.Collections.Generic;
using GrislyGrotto.Infrastructure.Domain;

namespace GrislyGrotto.Infrastructure
{
    public interface IPostRepository
    {
        IEnumerable<Post> GetLatestPosts(string authorFullname, int count);
        IEnumerable<DateTime> GetMonthsWithPosts(string authorFullname);
        IEnumerable<Post> GetPostsByMonth(string authorFullname, int year, int month);
        Post GetPostByID(int postID);

        int AddPost(string author, string title, string content);
        void UpdatePost(int postID, string title, string content);
    }
}
