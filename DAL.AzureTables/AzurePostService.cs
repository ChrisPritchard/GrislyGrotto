using System;
using System.Collections.Generic;
using System.Linq;
using GrislyGrotto.DAL.AzureTables.Entities;
using GrislyGrotto.Core;
using GrislyGrotto.Core.Entities;
using Microsoft.WindowsAzure.StorageClient;

namespace GrislyGrotto.DAL.AzureTables
{
    public class AzurePostService : IPostService
    {
        private readonly IUserService postUserService;
        private readonly TableServiceContext context;

        public AzurePostService(CloudTableClient client, TableServiceContext globalContext, IUserService userService)
        {
            postUserService = userService;
            context = globalContext;
            client.CreateTableWithSchemaIfDoesntExist("Posts", context, new PostEntity { ID = 1, Content = "schema", Created = DateTime.MaxValue, Status = "Draft", Title = "schema", UserLoginName = "schema" });
            client.CreateTableWithSchemaIfDoesntExist("Comments", context, new CommentEntity { Author = "schema", Content = "schema", Created = DateTime.MaxValue, PostID = 1 });
        }

        public IEnumerable<Post> GetLatest(User user, DateTime fromTime, int? count, PostStatus type)
        {
            var query = context.CreateQuery<PostEntity>("Posts");

            var posts = (user != null ?
                query.Where(post => post.UserLoginName.Equals(user.LoginName) && post.Created < fromTime && post.Status.Equals(type.ToString())).OrderByDescending(post => post.Created)
            : query.Where(post => post.Created < fromTime && post.Status.Equals(type.ToString())).OrderByDescending(post => post.Created))
            as IQueryable<PostEntity>;

            if (count.HasValue)
                posts = posts.Take(count.Value);

            foreach (var post in posts)
                yield return post.AsDomainEntity(postUserService, GetCommentsOfPost(post.ID));
        }

        public Post GetSpecific(int postID)
        {
            var query = context.CreateQuery<PostEntity>("Posts");
            var specificPost = query.Where(post => post.ID.Equals(postID)).SingleOrDefault();
            return specificPost == null ? null : specificPost.AsDomainEntity(postUserService, GetCommentsOfPost(specificPost.ID));
        }

        public IEnumerable<Post> GetFromMonth(User user, int year, string monthName, PostStatus type)
        {
            var query = context.CreateQuery<PostEntity>("Posts");

            var posts =
                user != null ?
                query.Where(post => post.UserLoginName.Equals(user.LoginName) && post.Status.Equals(type.ToString()) && post.Created.ToString("yyyyMMMM").Equals(year + monthName))
                : query.Where(post => post.Status.Equals(type.ToString()) && post.Created.ToString("yyyyMMMM").Equals(year + monthName));

            foreach (var post in posts)
                yield return post.AsDomainEntity(postUserService, GetCommentsOfPost(post.ID));
        }

        public Dictionary<DateTime, int> GetMonthPostCounts(User user)
        {
            var query = context.CreateQuery<PostEntity>("Posts");

            var returnMonths = new Dictionary<DateTime, int>();
            if (user != null)
            {
                var months = query.Where(p => p.UserLoginName.Equals(user.LoginName)).Select(p => new { p.Created.Year, p.Created.Month }).Distinct();
                foreach (var month in months)
                    returnMonths.Add(new DateTime(month.Year, month.Month, 1), query.Count(p => p.UserLoginName.Equals(user.LoginName) && p.Created.Year.Equals(month.Year) && p.Created.Month.Equals(month.Month)));
            }
            else
            {
                var months = query.Select(p => new { p.Created.Year, p.Created.Month }).Distinct();
                foreach (var month in months)
                    returnMonths.Add(new DateTime(month.Year, month.Month, 1), query.Count(p => p.Created.Year.Equals(month.Year) && p.Created.Month.Equals(month.Month)));
            }

            return returnMonths;
        }

        public IEnumerable<Post> Search(User user, string searchTerm, PostStatus type)
        {
            var query = context.CreateQuery<PostEntity>("Posts");

            var posts = 
                user != null ? 
                query.Where(post => post.UserLoginName.Equals(user.LoginName) && post.Status.Equals(type.ToString()) && (post.Title.Contains(searchTerm) || post.Content.Contains(searchTerm)))
                : query.Where(post => post.Status.Equals(type.ToString()) && (post.Title.Contains(searchTerm) || post.Content.Contains(searchTerm)));

            foreach (var post in posts)
                yield return post.AsDomainEntity(postUserService, GetCommentsOfPost(post.ID));
        }

        private IEnumerable<Comment> GetCommentsOfPost(int postID)
        {
            var query = context.CreateQuery<CommentEntity>("Comments");

            foreach (var comment in query.Where(comment => comment.PostID.Equals(postID)).OrderBy(comment => comment.Created))
                yield return comment.AsDomainEntity();
        }

        public Post AddPost(Post post)
        {
            var query = context.CreateQuery<PostEntity>("Posts");
            var nextID = query.Select(existingPost => existingPost.ID).OrderByDescending(existingPost => existingPost).First() + 1;
            post.ID = nextID;
            context.AddObject("Posts", post.AsAzureEntity());
            context.SaveChanges();
            return post;
        }

        public void UpdatePost(Post post)
        {
            var query = context.CreateQuery<PostEntity>("Posts");
            var postToBeUpdated = query.Where(existingPost => existingPost.ID.Equals(post.ID)).SingleOrDefault();
            if (postToBeUpdated == null)
                return;

            postToBeUpdated.Title = post.Title;
            postToBeUpdated.Content = post.Content;
            postToBeUpdated.Status = post.Status.ToString();
            postToBeUpdated.Created = post.Created;

            context.UpdateObject(postToBeUpdated);
            context.SaveChanges();
        }

        public void DeletePost(int postID)
        {
            var query = context.CreateQuery<PostEntity>("Posts");
            var postToBeDeleted = query.Where(post => post.ID.Equals(postID)).SingleOrDefault();
            if(postToBeDeleted == null)
                return;
            context.DeleteObject(postToBeDeleted);
            context.SaveChanges();
        }

        public void AddComment(Post post, Comment comment)
        {
            context.AddObject("Comments", comment.AsAzureEntity(post.ID));
            context.SaveChanges();
        }
    }
}