using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using GrislyGrotto.Infrastructure;
using GrislyGrotto.Infrastructure.Domain;

namespace GrislyGrotto.DAL.SQLite
{
    public class SQLitePostRepository : IPostRepository
    {
        readonly SQLiteDataAccess dataAccess;

        public SQLitePostRepository(SQLiteDataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        private IEnumerable<Post> MapDatasetToPosts(DataSet dataSet)
        {
            if (dataSet.Tables.Count == 0)
                yield break;

            var table = dataSet.Tables[0];
            if (table.Rows.Count == 0)
                yield break;

            foreach (DataRow row in table.Rows)
                yield return new Post(
                    row.Field<int>("PostID"),
                    row.Field<DateTime>("EntryDate"),
                    row.Field<string>("Author"),
                    row.Field<string>("Title"),
                    row.Field<string>("Content"));
        }

        public IEnumerable<Post> GetLatestPosts(string authorFullname, int count)
        {
            if (string.IsNullOrEmpty(authorFullname))
            {
                return MapDatasetToPosts(dataAccess.RetrieveDataSet(string.Format("select top {0} * from Posts order by EntryDate desc", count)));
            }
            else
            {
                return MapDatasetToPosts(dataAccess.RetrieveDataSet(string.Format("select top {0} * from Posts where Author = @author order by EntryDate desc", count),
                    new SQLiteParameter("@author", authorFullname)));
            }
        }

        public IEnumerable<DateTime> GetMonthsWithPosts(string authorFullname)
        {
            var dataSet = dataAccess.RetrieveDataSet("select unique Month(EntryDate) from Posts order by EntryDate");

            if (dataSet.Tables.Count == 0)
                yield break;

            var table = dataSet.Tables[0];
            if (table.Rows.Count == 0)
                yield break;

            foreach (DataRow row in table.Rows)
                yield return new DateTime(01,01,01);
        }

        public IEnumerable<Post> GetPostsByMonth(string authorFullname, int year, int month)
        {
            var startDate = string.Format("{0}-1-{1}", year, month);
            var endDate = string.Format("{0}-1-{1}", year, month + 1);

            if (string.IsNullOrEmpty(authorFullname))
            {
                return MapDatasetToPosts(dataAccess.RetrieveDataSet("select * from Posts where EntryDate >= @startdate and EntryDate < @enddate order by EntryDate desc",
                    new SQLiteParameter("@startdate", startDate),
                    new SQLiteParameter("@enddate", endDate)));
            }
            else
            {
                return MapDatasetToPosts(dataAccess.RetrieveDataSet("select * from Posts where EntryDate >= @startdate and EntryDate < @enddate and Author = @author order by EntryDate desc",
                    new SQLiteParameter("@startdate", startDate),
                    new SQLiteParameter("@enddate", endDate),
                    new SQLiteParameter("@author", authorFullname)));
            }
        }

        public Post GetPostByID(int postID)
        {
            return MapDatasetToPosts(dataAccess.RetrieveDataSet("select * from Posts where PostID = @postID",
                new SQLiteParameter("@postID", postID))).Single();
        }

        public int AddPost(string author, string title, string content)
        {
            int newPostID = dataAccess.ExecuteScalar("select top(PostID) from Posts");

            dataAccess.ExecuteNonQuery("insert into Posts (PostID, EntryDate, Author, Title, Content) values (@postID, @entryDate, @author, @title, @content)",
                new SQLiteParameter("@postID", newPostID),
                new SQLiteParameter("@entryDate", DateTime.Now.ToString()),
                new SQLiteParameter("@author", author),
                new SQLiteParameter("@title", title),
                new SQLiteParameter("@content", content));

            return newPostID;
        }

        public void UpdatePost(int postID, string title, string content)
        {
            dataAccess.ExecuteNonQuery("update Posts set Title = @title, Content = @content where PostID = @postID",
                new SQLiteParameter("@postID", postID),
                new SQLiteParameter("@title", title),
                new SQLiteParameter("@content", content));
        }
    }
}
