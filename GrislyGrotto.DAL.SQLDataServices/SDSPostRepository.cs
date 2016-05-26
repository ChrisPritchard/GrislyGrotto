using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GrislyGrotto.Infrastructure;
using GrislyGrotto.Infrastructure.Domain;
using SqlDataServices;

namespace GrislyGrotto.DAL.SqlDataServices
{
    public class SDSPostRepository : IPostRepository
    {
        ISDSClient client;
        ContainerInfo containerInfo;

        public SDSPostRepository(ISDSClient client, ContainerInfo containerInfo)
        {
            this.client = client;
            this.containerInfo = containerInfo;
        }

        public IEnumerable<Post> GetLatestPosts(string authorFullname, int count)
        {
            var query = "from e in entities where e.Kind == \"Post\" orderby e[\"EntryDate\"] descending select e";
            if (!string.IsNullOrEmpty(authorFullname))
                query = string.Format("from e in entities where e.Kind == \"Post\" && e[\"Author\"] == \"{0}\" orderby e[\"EntryDate\"] descending select e", authorFullname);

            var entities = client.GetEntities(containerInfo, query).Take(count);

            foreach (Entity entity in entities)
            {
                yield return new Post(int.Parse(entity["PostID"].ToString()),
                    DateTime.Parse(entity["EntryDate"].ToString()),
                    (string)entity["Author"],
                    (string)entity["Title"],
                    (string)entity["Content"]);
            }
        }

        public IEnumerable<Post> GetPostsByMonth(string authorFullname, int year, int month)
        {
            var query = string.Format("from e in entities where e.Kind = \"Post\" && e[\"EntryDate\"] >= \"{0}\" && e[\"EntryDate\"] < \"{0}\" orderby e[\"EntryDate\"] descending select e",
                string.Format("{0} {1}", CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month), year));
            if (!string.IsNullOrEmpty(authorFullname))
                string.Format("from e in entities where e.Kind = \"Post\" && e[\"Author\"] == \"{0}\" && e[\"EntryDate\"] >= \"{1}\" && e[\"EntryDate\"] < \"{0}\" orderby e[\"EntryDate\"] descending select e",
                    authorFullname, string.Format("{0} {1}", month.ToString(), year));

            var entities = client.GetEntities(containerInfo, query);

            foreach (Entity entity in entities)
            {
                yield return new Post(int.Parse(entity["PostID"].ToString()),
                    DateTime.Parse(entity["EntryDate"].ToString()),
                    (string)entity["Author"],
                    (string)entity["Title"],
                    (string)entity["Content"]);
            }
        }

        public Post GetPostByID(int postID)
        {
            var query = string.Format("from e in entities where e[\"PostID\"] == {0} select e", postID);

            var entities = client.GetEntities(containerInfo, query);
            if (entities.Count() == 0)
                return null;

            var entity = entities.First();
            return new Post(int.Parse(entity["PostID"].ToString()),
                    DateTime.Parse(entity["EntryDate"].ToString()),
                    (string)entity["Author"],
                    (string)entity["Title"],
                    (string)entity["Content"]);
        }

        public int AddPost(string author, string title, string content)
        {
            var newPostID = client.GetEntities(containerInfo, string.Empty).Count() + 1;

            var properties = new Dictionary<string, object>();
            properties.Add("PostID", newPostID);
            properties.Add("EntryDate", DateTime.Now);
            properties.Add("Author", author);
            properties.Add("Title", title);
            properties.Add("Content", content);

            client.AddEntity(containerInfo, "Post", properties);

            return newPostID;
        }

        public void UpdatePost(int postID, string title, string content)
        {
            var query = string.Format("from e in entities where e[\"PostID\"] == {0} select e", postID);
            var entities = client.GetEntities(containerInfo, query);

            if (entities.Count() == 0)
                return;

            var entity = entities.First();
            entity["Title"] = title;
            entity["Content"] = content;

            client.UpdateEntity(containerInfo, entity);
        }

        public IEnumerable<DateTime> GetMonthsWithPosts(string authorFullname)
        {
            var query = "from e in entities where e.Kind == \"Post\" orderby e[\"EntryDate\"] descending select e";
            if (!string.IsNullOrEmpty(authorFullname))
                query = string.Format("from e in entities where e.Kind == \"Post\" && e[\"Author\"] == \"{0}\" orderby e[\"EntryDate\"] descending select e", authorFullname);

            var entities = client.GetEntities(containerInfo, query);

            var monthsWithPosts = new List<DateTime>();
            foreach (Entity entity in entities)
            {
                var entryDate = DateTime.Parse(entity["EntryDate"].ToString());
                var month = new DateTime(entryDate.Year, entryDate.Month, 1);
                if (!monthsWithPosts.Contains(month))
                    monthsWithPosts.Add(month);
            }
            return monthsWithPosts;
        }
    }
}
