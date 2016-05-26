using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GrislyGrotto.Infrastructure;
using GrislyGrotto.Infrastructure.Domain;
using GrislyGrotto.Website.Models.ViewModels;

namespace GrislyGrotto.Website.Models
{
    public class BlogServices
    {
        IPostRepository postRepository;
        ICommentRepository commentRepository;

        public BlogServices(IPostRepository postRepository, ICommentRepository commentRepository)
        {
            this.postRepository = postRepository;
            this.commentRepository = commentRepository;
        }

        public IEnumerable<PostWithCommentCount> PostsWithCommentCounts(IEnumerable<Post> posts)
        {
            foreach (var post in posts)
            {
                yield return new PostWithCommentCount(post, commentRepository.GetCommentsOfPost(post.PostID).Count());
            }
        }

        public IEnumerable<RecentEntry> RecentEntries(string authorFullname)
        {
            foreach (var post in postRepository.GetLatestPosts(authorFullname, 10))
            {
                yield return new RecentEntry(post.PostID, post.Title, post.EntryDate);
            }
        }

        public IEnumerable<MonthPostCount> MonthPostCounts(string authorFullname)
        {
            foreach (var month in postRepository.GetMonthsWithPosts(authorFullname))
            {
                var postsOfMonth =  postRepository.GetPostsByMonth(authorFullname, month.Year, month.Month);
                yield return new MonthPostCount(month.Year, GetMonthName(month.Month), postsOfMonth.Count());
            }
        }

        public string GetMonthName(int monthIndex)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthIndex);
        }

        public int GetMonthIndex(string monthName)
        {
            return Array.IndexOf(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames, monthName) + 1;
        }
    }
}
