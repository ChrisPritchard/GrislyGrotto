using System;
using System.Collections.Generic;
using GrislyGrotto.DAL.AzureTables.Entities;
using GrislyGrotto.Core;
using GrislyGrotto.Core.Entities;
using Microsoft.WindowsAzure.StorageClient;

namespace GrislyGrotto.DAL.AzureTables
{
    static class Extensions
    {
        public static void CreateTableWithSchemaIfDoesntExist(this CloudTableClient client, string tableName, TableServiceContext context, object schemaObject)
        {
            if (client.DoesTableExist(tableName)) return;

            client.CreateTable(tableName);
            context.AddObject(tableName, schemaObject);
            context.SaveChanges();
        }

        public static CommentEntity AsAzureEntity(this Comment comment, int postID)
        {
            return new CommentEntity
            {
                PostID = postID,
                Author = comment.Author,
                Content = comment.Content,
                Created = comment.Created,
            };
        }

        public static Comment AsDomainEntity(this CommentEntity commentEntity)
        {
            return new Comment
            {
                Author = commentEntity.Author,
                Content = commentEntity.Content,
                Created = commentEntity.Created,
            };
        }

        public static PostEntity AsAzureEntity(this Post post)
        {
            return new PostEntity
            {
                UserLoginName = post.User.LoginName,
                Title = post.Title,
                Content = post.Content,
                Created = post.Created,
                ID = post.ID,
                Status = post.Status.ToString()
            };
        }

        public static Post AsDomainEntity(this PostEntity postEntity, IUserService userService, IEnumerable<Comment> comments)
        {
            return new Post
            {
                User = userService.GetUserByLoginName(postEntity.UserLoginName),
                Title = postEntity.Title,
                Content = postEntity.Content,
                Created = postEntity.Created,
                ID = postEntity.ID,
                Status = (PostStatus)Enum.Parse(typeof(PostStatus), postEntity.Status),
                Comments = comments
            };
        }

        public static UserEntity AsAzureEntity(this User user)
        {
            return new UserEntity
            {
                FullName = user.FullName,
                LoginName = user.LoginName,
                LoginPassword = user.LoginPassword
            };
        }

        public static User AsDomainEntity(this UserEntity userEntity)
        {
            return new User
            {
                FullName = userEntity.FullName,
                LoginName = userEntity.LoginName,
                LoginPassword = userEntity.LoginPassword
            };
        }
    }
}