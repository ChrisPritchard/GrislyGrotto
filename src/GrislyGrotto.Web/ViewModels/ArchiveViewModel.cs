
namespace GrislyGrotto
{
    public class ArchiveViewModel
    {
        public YearViewModel[] Years { get; set; }
        public Post[] Stories { get; set; }
    }

    public class YearViewModel
    {
        public int Year { get; set; }
        public MonthViewModel[] Months { get; set; }

        public YearViewModel(int year, MonthViewModel[] months)
        {
            Year = year;
            Months = months;
        }
    }

    public class MonthViewModel
    {
        public string Month { get; set; }
        public int Count { get; set; }

        public MonthViewModel(string month, int count)
        {
            Month = char.ToUpper(month[0]) + month.Substring(1);
            Count = count;
        }
    }
}