
namespace GrislyGrotto.Website.Models.ViewModels
{
    public class MonthPostCount
    {
        public int Year { get; private set; }
        public string Month { get; private set; }
        public int PostCount { get; private set; }

        public MonthPostCount(int year, string month, int postCount)
        {
            Year = year;
            Month = month;
            PostCount = postCount;
        }
    }
}
