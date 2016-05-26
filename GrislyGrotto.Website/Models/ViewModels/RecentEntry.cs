using System;

namespace GrislyGrotto.Website.Models.ViewModels
{
    public class RecentEntry
    {
        public int PostID { get; private set; }
        public string Title { get; private set; }
        public DateTime EntryDate { get; private set; }

        public RecentEntry(int postID, string title, DateTime entryDate)
        {
            PostID = postID;
            Title = title;
            EntryDate = entryDate;
        }
    }
}
