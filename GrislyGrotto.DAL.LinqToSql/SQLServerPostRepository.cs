using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using GrislyGrotto.Infrastructure;
using Domain = GrislyGrotto.Infrastructure.Domain;

namespace GrislyGrotto.DAL.SQLServer
{
    public class SQLServerPostRepository : IPostRepository
    {
        private GrislyGrottoDBDataContext linqDataRepository;

        public SQLServerPostRepository(ConnectionStringSettingsCollection connectionStrings)
        {
            this.linqDataRepository = new GrislyGrottoDBDataContext(connectionStrings);
        }

        public IEnumerable<Domain.Post> GetLatestPosts(string authorFullname, int count)
        {
            var posts = linqDataRepository.Blogs.Where(b => string.IsNullOrEmpty(authorFullname) || b.User.Fullname == authorFullname).
                OrderByDescending(b => b.EntryDate).Take(count).ToList();

            foreach (Blog post in posts)
            {
                yield return new Domain.Post(post.BlogID, post.EntryDate, post.User.Fullname, post.Title, post.Content);
            }
        }

        public IEnumerable<Domain.Post> GetPostsByMonth(string authorFullname, int year, int month)
        {
            var posts = linqDataRepository.Blogs.Where(b => string.IsNullOrEmpty(authorFullname) || b.User.Fullname == authorFullname).
                Where(b => b.EntryDate.Year == year && b.EntryDate.Month == month).
                OrderByDescending(b => b.EntryDate).ToList();

            foreach (Blog post in posts)
            {
                yield return new Domain.Post(post.BlogID, post.EntryDate, post.User.Fullname, post.Title, post.Content);
            }
        }

        public Domain.Post GetPostByID(int postID)
        {
            var post = linqDataRepository.Blogs.Where(b => b.BlogID == postID).Single();
            return new Domain.Post(post.BlogID, post.EntryDate, post.User.Fullname, post.Title, post.Content);
        }

        public int AddPost(string author, string title, string content)
        {
            var newPost = new Blog();

            var authorID = linqDataRepository.Users.Where(a => a.Fullname.Equals(author)).First().UserID;
            newPost.AuthorID = authorID;
            newPost.Title = title;
            newPost.Tags = string.Empty;
            newPost.Content = content;
            newPost.EntryDate = DateTime.Now;

            linqDataRepository.Blogs.InsertOnSubmit(newPost);
            linqDataRepository.SubmitChanges();

            return newPost.BlogID;
        }

        public void UpdatePost(int postID, string title, string content)
        {
            var existingPost = linqDataRepository.Blogs.Where(b => b.BlogID == postID).Single();

            existingPost.Title = title;
            existingPost.Content = content;

            linqDataRepository.SubmitChanges();
        }

        public IEnumerable<DateTime> GetMonthsWithPosts(string authorFullname)
        {
            var monthsWithPosts = linqDataRepository.Blogs.Where(b => string.IsNullOrEmpty(authorFullname) || b.User.Fullname == authorFullname).
               Select(b => new DateTime(b.EntryDate.Year, b.EntryDate.Month, 1)).
               Distinct().
               OrderByDescending(b => b).ToList();

            return monthsWithPosts;
        }

        public IEnumerable<Domain.Post> AllPosts(IUserRepository userRepository)
        {
            var posts = linqDataRepository.Blogs.ToList();

            foreach (Blog post in posts)
            {
                yield return new Domain.Post(post.BlogID, post.EntryDate, post.User.Fullname, post.Title, post.Content);
            }
        }
    }
}
