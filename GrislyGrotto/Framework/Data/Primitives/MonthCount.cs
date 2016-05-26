namespace GrislyGrotto.Framework.Data.Primitives
{
    public class MonthCount
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get { return Month.AsMonthName(); } set { Month = value.AsMonthNum(); } }
        public int PostCount { get; set; }
    }
}