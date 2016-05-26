using System;
using GrislyGrotto.Models.Components;

namespace GrislyGrotto.Models.DTO
{
    public class MonthInfo
    {
        public int Year { get; private set; }
        public Month Month { get; private set; }

        public MonthInfo(int year, Month month)
        {
            Year = year;
            Month = month;
        }

        public MonthInfo(int year, int month)
        {
            Year = year;
            Month = (Month)month;
        }

        public MonthInfo(int year, string month)
        {
            Year = year;
            Month = (Month)Enum.Parse(typeof(Month), month);
        }
    }
}
