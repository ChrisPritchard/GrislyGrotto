using System;
using System.Collections.Generic;
using System.Linq;
using GrislyGrotto.Models.DTO;

namespace GrislyGrotto.Models.LinqToSql
{
    public class LinqBlogRepository : IBlogRepository
    {
        private GrislyGrottoDBDataContext linqDataRepository;

        public LinqBlogRepository(GrislyGrottoDBDataContext linqDataRepository)
        {
            this.linqDataRepository = linqDataRepository;
        }

        public PostInfo[] GetLatestPosts(string userFullname, int count, ICommentRepository commentRepository)
        {
            var posts = linqDataRepository.Blogs.Where(b => string.IsNullOrEmpty(userFullname) || b.User.Fullname == userFullname).
                OrderByDescending(b => b.EntryDate).Take(count).ToList();

            var postInfoList = new List<PostInfo>();
            foreach (Blog post in posts)
            {
                CommentInfo[] comments = commentRepository.GetCommentsOfPost(post.BlogID);
                UserInfo author = new UserInfo(post.User.Fullname, post.User.Username);
                postInfoList.Add(new PostInfo(post.BlogID, post.EntryDate, author, post.Title, post.Content, comments));
            }

            return postInfoList.ToArray();
        }

        public PostInfo[] GetPostsByMonth(string userFullname, MonthInfo month, ICommentRepository commentRepository)
        {
            var posts = linqDataRepository.Blogs.Where(b => string.IsNullOrEmpty(userFullname) || b.User.Fullname == userFullname).
                Where(b => b.EntryDate.Year == month.Year && b.EntryDate.Month == (int)month.Month).
                OrderByDescending(b => b.EntryDate).ToList();

            var postInfoList = new List<PostInfo>();
            foreach (Blog post in posts)
            {
                CommentInfo[] comments = commentRepository.GetCommentsOfPost(post.BlogID);
                UserInfo author = new UserInfo(post.User.Fullname, post.User.Username);
                postInfoList.Add(new PostInfo(post.BlogID, post.EntryDate, author, post.Title, post.Content, comments));
            }

            return postInfoList.ToArray();
        }

        public PostInfo GetPostByID(int postID, ICommentRepository commentRepository)
        {
            var post = linqDataRepository.Blogs.Where(b => b.BlogID == postID).Single();
            UserInfo author = new UserInfo(post.User.Fullname, post.User.Username);
            return new PostInfo(post.BlogID, post.EntryDate, author, post.Title, post.Content, commentRepository.GetCommentCountOfPost(post.BlogID));
        }

        public void AddPost(PostInfo post)
        {
            var newPost = new Blog();

            int authorID = linqDataRepository.Users.Where(author => author.Username.Equals(post.Author.Username)).First().UserID;
            newPost.AuthorID = authorID;
            newPost.Title = post.Title;
            newPost.Tags = string.Empty;
            newPost.Content = post.Content;
            newPost.EntryDate = DateTime.Now;

            linqDataRepository.Blogs.InsertOnSubmit(newPost);
            linqDataRepository.SubmitChanges();
        }

        public void UpdatePost(PostInfo post)
        {
            var existingPost = linqDataRepository.Blogs.Where(b => b.BlogID == post.PostID).Single();

            existingPost.Title = post.Title;
            existingPost.Content = post.Content;

            linqDataRepository.SubmitChanges();
        }

        public Dictionary<MonthInfo, int> GetMonthPostCounts(string userFullname)
        {
            var monthPostCounts = linqDataRepository.Blogs.Where(b => string.IsNullOrEmpty(userFullname) || b.User.Fullname == userFullname).
               Select(b => new { b.EntryDate.Month, b.EntryDate.Year }).
               Distinct().
               OrderByDescending(b => new DateTime(b.Year, b.Month, 1)).ToList();

            var monthPostCountsList = new Dictionary<MonthInfo, int>();
            foreach (var month in monthPostCounts)
            {
                var postCount = linqDataRepository.Blogs.Where(b => string.IsNullOrEmpty(userFullname) || b.User.Fullname == userFullname).
                    Where(b => b.EntryDate.Month == month.Month && b.EntryDate.Year == month.Year).
                    Count();

                monthPostCountsList.Add(new MonthInfo(month.Year, month.Month), postCount);
            }

            return monthPostCountsList;
        }

        public PostInfo[] GetRecentPostEntries(string userFullname, int count)
        {
            var posts = linqDataRepository.Blogs.Where(b => string.IsNullOrEmpty(userFullname) || b.User.Fullname.Equals(userFullname)).
                OrderByDescending(b => b.EntryDate).
                Select(b => new { b.BlogID, b.Title, b.EntryDate, b.Content }).
                Take(count).ToList();

            var recentEntries = new List<PostInfo>();
            foreach (var post in posts)
            {
                recentEntries.Add(new PostInfo(post.BlogID, post.EntryDate, post.Title));
            }

            return recentEntries.ToArray();
        }
    }
}
