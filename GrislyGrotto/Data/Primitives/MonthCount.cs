using System;

namespace GrislyGrotto.Data.Primitives
{
    internal class MonthCount : IComparable<MonthCount>
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get { return Month.AsMonthName(); } set { Month = value.AsMonthNum(); } }
        public int PostCount { get; set; }

        public bool CoversDate(DateTime date)
        {
            return Year == date.Year && Month == date.Month;
        }

        public int CompareTo(MonthCount other)
        {
            if (other.Year > Year)
                return -1;
            if (Year > other.Year)
                return 1;

            if (other.Month > Month)
                return -1;
            return Month > other.Month ? 1 : 0;
        }
    }
}