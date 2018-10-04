using System;
using System.Globalization;

namespace Framework.Core.Utils
{
    public class DateHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dayMonth"></param>
        /// <param name="year"></param>
        /// <param name="separator"></param>
        /// <returns> DateTime.Min incase error </returns>

        #region Props

        public static readonly DateTime MinDate = new DateTime(1948, 1, 1);

        #endregion

        #region Date Validations

        public static bool IsValid(DateTime date)
        {
            if (date == DateTime.MinValue || date <= MinDate) return false; 
            return true;
        }

        public static bool IsValidRange(DateTime fromDate, DateTime endDate)
        {
            if (fromDate > endDate) return false;
            return true;
        }

        public static bool IsValidTimeToSendMails()
        {

            DateTime now = DateTime.Now.Now();

            if (now.DayOfWeek == DayOfWeek.Saturday) return false;
            if (now.Hour < 8) return false;

            if (now.DayOfWeek == DayOfWeek.Friday)
            {
                if (now.Hour > 14) return false;
            }

            if (now.Hour > 21) return false;

            return true;


        }

        #endregion

        #region Methods
        public static DateTime SetDate(string dayMonth, int year, char separator)
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

        public static DateTime SetDate(string dayMonth, int year, char separator,bool isFromDate)
        {
            DateTime DateValue;
            DateValue = SetDate(dayMonth, year, separator);

            if (DateValue == DateTime.MinValue) return DateValue;

            if (isFromDate == true)
            {
                return SetFromDate(DateValue);
            }
            else
            {
                return SetEndDate(DateValue);
            }           
            
        }

        public static DateTime SetFromDate(DateTime fromDate)
        {
            DateTime fromDateValue = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 00, 00, 00);
            return fromDateValue;
        }

        public static DateTime SetEndDate(DateTime endDate)
        {
            DateTime endDateValue = new DateTime(endDate.Year, endDate.Month, endDate.Day, 23, 59, 59);
            return endDateValue;          
        }

        public static DateTime Now()
        {
            var timeUtc = DateTime.UtcNow;
            TimeZoneInfo israelZone = TimeZoneInfo.FindSystemTimeZoneById("Israel Standard Time");
            DateTime israelTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, israelZone);
            return israelTime;
        }

        #endregion
    }
}
