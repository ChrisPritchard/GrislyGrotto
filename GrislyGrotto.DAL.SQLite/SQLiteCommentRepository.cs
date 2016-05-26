using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using GrislyGrotto.Infrastructure;
using GrislyGrotto.Infrastructure.Domain;

namespace GrislyGrotto.DAL.SQLite
{
    public class SQLiteCommentRepository : ICommentRepository
    {
        readonly SQLiteDataAccess dataAccess;

        public SQLiteCommentRepository(SQLiteDataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        private IEnumerable<Comment> MapDatasetToComments(DataSet dataSet)
        {
            if (dataSet.Tables.Count == 0)
                yield break;

            var table = dataSet.Tables[0];
            if (table.Rows.Count == 0)
                yield break;

            foreach (DataRow row in table.Rows)
                yield return new Comment(
                    row.Field<int>("PostID"),
                    row.Field<DateTime>("EntryDate"),
                    row.Field<string>("Author"),
                    row.Field<string>("Text"));
        }

        public IEnumerable<Comment> GetCommentsOfPost(int postID)
        {
            return MapDatasetToComments(dataAccess.RetrieveDataSet("select * from Comments where PostID = @postID",
                new SQLiteParameter("@postID", postID)));
        }

        public void AddComment(int postID, string author, string text)
        {
            dataAccess.ExecuteNonQuery("insert into Comments (PostID, EntryDate, Author, Text) values (@postID, @entryDate, @author, @text)",
                new SQLiteParameter("@postID", postID),
                new SQLiteParameter("@entryDate", DateTime.Now.ToString()),
                new SQLiteParameter("@author", author),
                new SQLiteParameter("@text", text));
        }
    }
}
