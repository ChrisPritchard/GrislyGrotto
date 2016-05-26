using System;
using System.Collections.Generic;
using System.Linq;
using GrislyGrotto.Models.DTO;

namespace GrislyGrotto.Models.LinqToSql
{
    public class LinqCommentRepository : ICommentRepository
    {
        private GrislyGrottoDBDataContext linqDataRepository;

        public LinqCommentRepository(GrislyGrottoDBDataContext linqDataRepository)
        {
            this.linqDataRepository = linqDataRepository;
        }

        public int GetCommentCountOfPost(int postID)
        {
            return linqDataRepository.Comments.Where(c => c.Blog.BlogID == postID).Count();
        }

        public CommentInfo[] GetCommentsOfPost(int postID)
        {
            var comments = linqDataRepository.Comments.Where(comment => comment.BlogID == postID).ToList();
            var commentsList = new List<CommentInfo>();
            foreach (Comment comment in comments)
                commentsList.Add(new CommentInfo(comment.CommentID, comment.EntryDate, comment.Author, comment.Content));
            return commentsList.ToArray();
        }

        public void AddComment(CommentInfo comment)
        {
            var newComment = new Comment();

            newComment.BlogID = comment.PostID;
            newComment.Author = comment.Author;
            newComment.Content = comment.Text;
            newComment.EntryDate = DateTime.Now;

            linqDataRepository.Comments.InsertOnSubmit(newComment);
            linqDataRepository.SubmitChanges();
        }
    }
}
