using GrislyGrotto.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrislyGrotto
{
    public class PostService
    {
        private static Dictionary<string, Post> postCache = new Dictionary<string, Post>();
        private static List<string> latest = new List<string>();

        private static List<string> recentEvents = new List<string>();

        public PostService()
        {
            AddEvent("Application (re)started");
        }

        public async Task<Post[]> Latest(int latestCount, int page)
        {
            if(latest.Count >= page * latestCount)
            {
                var postKeys = latest.Skip((page - 1) * latestCount).Take(latestCount);
                return postKeys.Select(k => postCache[k]).ToArray();
            }

            var result = await SearchClient.Current.Search(orderBy: "date desc", count: latestCount, skip: (page - 1) * 5);
            var posts = result.Select(o => o.Document).ToArray();
            foreach (var post in posts)
            {
                latest.Add(post.Key);
                postCache[post.Key] = post;
            }
            return posts;
        }

        public async Task<bool> Exists(string key)
        {
            if (postCache.ContainsKey(key))
                return true;
            return await SearchClient.Current.Exists(key);
        }

        public async Task<Post> Get(string key)
        {
            if (postCache.ContainsKey(key))
                return postCache[key];
            return await SearchClient.Current.Get(key);
        }

        public async Task CreateOrUpdate(Post updatedPost)
        {
            await SearchClient.Current.CreateOrUpdate(updatedPost);
            postCache[updatedPost.Key] = updatedPost;
            if (latest.Count > 0 && updatedPost.Date > postCache[latest[0]].Date)
                latest.Insert(0, updatedPost.Key);
        }

        public void AddEvent(string @event, params object[] args)
        {
            var date = DateTime.UtcNow.Add(NzTimeZone);
            recentEvents.Add(string.Format(@event, args) + " at " + date.ToString("hh:mm:ss dd-MM-yy"));
        }

        public IEnumerable<string> GetEvents()
        {
            return recentEvents;
        }

        private static PostService dataService;
        public static PostService Current
        {
            get { return dataService ?? (dataService = new PostService()); }
        }

        public static TimeSpan NzTimeZone = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time").BaseUtcOffset;
    }
}