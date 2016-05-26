using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using GrislyGrotto.Infrastructure;
using Domain = GrislyGrotto.Infrastructure.Domain;

namespace GrislyGrotto.DAL.SQLServer
{
    public class SQLServerCommentRepository : ICommentRepository
    {
        private GrislyGrottoDBDataContext linqDataRepository;

        public SQLServerCommentRepository(ConnectionStringSettingsCollection connectionStrings)
        {
            this.linqDataRepository = new GrislyGrottoDBDataContext(connectionStrings);
        }

        public IEnumerable<Domain.Comment> GetCommentsOfPost(int postID)
        {
            var comments = linqDataRepository.Comments.Where(comment => comment.BlogID == postID).ToList();

            foreach (Comment comment in comments)
                yield return new Domain.Comment(comment.BlogID, comment.EntryDate, comment.Author, comment.Content);
        }

        public void AddComment(int postID, string author, string text)
        {
            var newComment = new Comment();

            newComment.BlogID = postID;
            newComment.Author = author;
            newComment.Content = text;
            newComment.EntryDate = DateTime.Now;

            linqDataRepository.Comments.InsertOnSubmit(newComment);
            linqDataRepository.SubmitChanges();
        }
    }
}
