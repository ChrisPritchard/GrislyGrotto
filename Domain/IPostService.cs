using System;
using System.Collections.Generic;
using GrislyGrotto.Core.Entities;

namespace GrislyGrotto.Core
{
    public interface IPostService
    {
        IEnumerable<Post> GetLatest(User user, DateTime fromTime, int? count, PostStatus type);
        Post GetSpecific(int postID);
        IEnumerable<Post> GetFromMonth(User user, int year, string monthName, PostStatus type);
        Dictionary<DateTime, int> GetMonthPostCounts(User user);
        IEnumerable<Post> Search(User user, string searchTerm, PostStatus type);

        Post AddPost(Post post);
        void UpdatePost(Post post);
        void DeletePost(int postID);

        void AddComment(Post post, Comment comment);
    }
}