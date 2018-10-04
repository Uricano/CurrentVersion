using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Core.Utils
{
    public static class DateExtender
    {
        public static readonly DateTime MinDate = new DateTime(1948, 1, 1);

        public static DateTime DateWithSeperator(this DateTime date, string dayMonth, int year, char separator, bool isFromDate)
        {
            DateTime fromDateValue;

            var dayMonthYearCombine = dayMonth + separator + year;
            var format = string.Format("d{0}M{1}yyyy", separator, separator);

            if (DateTime.TryParseExact(dayMonthYearCombine, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDateValue))
            {  // valid date
                return fromDateValue;
            }

            return DateTime.MinValue;
        }

        public static DateTime DateWithSeperator(this DateTime date, string dayMonth, int year, char separator)
        {
            DateTime fromDateValue;

            var dayMonthYearCombine = dayMonth + separator + year;
            var format = string.Format("d{0}M{1}yyyy", separator, separator);

            if (DateTime.TryParseExact(dayMonthYearCombine, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDateValue))
            {  // valid date
                return fromDateValue;
            }

            return DateTime.MinValue;
        }

        public static DateTime FromDate(this DateTime date)
        {
            DateTime fromDateValue = new DateTime(date.Year, date.Month, date.Day, 00, 00, 00);
            return fromDateValue;
        }

        public static DateTime EndDate(this DateTime date)
        {
            DateTime endDateValue = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
            return endDateValue;
        }

        public static DateTime Now(this DateTime date)
        {
            var timeUtc = DateTime.UtcNow;
            TimeZoneInfo israelZone = TimeZoneInfo.FindSystemTimeZoneById("Israel Standard Time");
            DateTime israelTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, israelZone);
            return israelTime;
        }

        public static DateTime Today(this DateTime date)
        {
            return date.Now().FromDate();
        }

        public static DateTime EndOfToday(this DateTime date)
        {
            return date.Now().EndDate();
        }

        public static bool IsValid(this DateTime date)
        {
            if (date == DateTime.MinValue || date <= MinDate) return false;
            return true;
        }

        public static bool IsValidRange(this DateTime fromDate, DateTime endDate)
        {
            if (fromDate > endDate) return false;
            return true;
        }
    }
}
