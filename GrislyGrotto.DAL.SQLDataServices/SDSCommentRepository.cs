using System;
using System.Collections.Generic;
using GrislyGrotto.Infrastructure;
using GrislyGrotto.Infrastructure.Domain;
using SqlDataServices;

namespace GrislyGrotto.DAL.SqlDataServices
{
    public class SDSCommentRepository : ICommentRepository
    {
        ISDSClient client;
        ContainerInfo containerInfo;

        public SDSCommentRepository(ISDSClient client, ContainerInfo containerInfo)
        {
            this.client = client;
            this.containerInfo = containerInfo;
        }

        public IEnumerable<Comment> GetCommentsOfPost(int postID)
        {
            var entities = client.GetEntities(containerInfo,
                string.Format("from e in entities where e.Kind == \"Comment\" && e[\"postID\"] == {0} orderby e[\"EntryDate\"] ascending select e", postID.ToString()));

            foreach (Entity entity in entities)
                yield return new Comment(postID, DateTime.Parse(entity["entryDate"].ToString()), (string)entity["author"], (string)entity["text"]);
        }

        public void AddComment(int postID, string author, string text)
        {
            var properties = new Dictionary<string, object>();
            properties.Add("postID", postID);
            properties.Add("entryDate", DateTime.Now);
            properties.Add("author", author);
            properties.Add("text", text);

            client.AddEntity(containerInfo, "Comment", properties);
        }
    }
}
