
using GrislyGrotto.ViewModels.Shared;

namespace GrislyGrotto.ViewModels.Home
{
    public class MonthViewModel
    {
        public string MonthName { get; set; }
        public int Year { get; set; }
        public PostViewModel[] Posts { get; set; }

        public MonthViewModel(string monthName, int year, PostViewModel[] posts)
        {
            MonthName = monthName;
            Year = year;
            Posts = posts;
        }
    }
}
