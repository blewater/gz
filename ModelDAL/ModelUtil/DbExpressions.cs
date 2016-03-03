using gzWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gzWeb.Model.Util {

    static public class DbExpressions {

        /// <summary>
        /// How to truncate milliseconds off of a .NET DateTime
        /// There's a bug in AddorUpdate method when a datetime key is used which has milli accuracy: 
        /// it always inserts because it won't add +1 on millis to find the record in SQL Server
        /// As of Entity framework 6.1.3
        /// http://stackoverflow.com/questions/1004698/how-to-truncate-milliseconds-off-of-a-net-datetime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static DateTime Truncate(this DateTime dateTime, TimeSpan timeSpan) {
            if (timeSpan == TimeSpan.Zero) return dateTime; // Or could throw an ArgumentException
            return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
        }

        /// <summary>
        /// date1 > date2 giving a positive value and date2 > date1 a negative value
        /// http://stackoverflow.com/questions/4638993/difference-in-months-between-two-dates
        /// </summary>
        /// <param name="date1"></param>
        /// <param name="date2"></param>
        /// <returns></returns>
        public static int MonthDiff(DateTime date1, DateTime date2) {
            return ((date1.Year - date2.Year) * 12) + date1.Month - date2.Month;
        }

        /// <summary>
        /// Slack chat 
        /// .6 to round up on our fees calculation. 0 Decimals.
        /// For example: 3.5 --> 3, 3.6 --> 4
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static decimal RoundCustomerBalanceAmount(decimal amount) {

            return Math.Round(amount - 0.1M, 0);
        }

        /// <summary>
        /// Slack chat 
        /// .5 to round up on our fees calculation. 0 Decimals.
        /// For example: 3.5 --> 4
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static decimal RoundGzFeesAmount(decimal amount) {

            return Math.Round(amount, 0);
        }

        /// <summary>
        /// Get string rep of previous month
        /// i.e. June 2015 -> "201505"
        /// January 2016 -> "201512"
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public static string GetPrevYearMonth(int year, int month) {

            var prevMonthDt = new DateTime(year, month, 1).AddMonths(-1);
            return DbExpressions.GetStrYearMonth(prevMonthDt.Year, prevMonthDt.Month);

        }

        /// <summary>
        /// Useful in where clauses
        /// i.e. Jun 2015 -> "201506"
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public static string GetStrYearMonth(int year, int month) {

            return year.ToString("0000") + month.ToString("00");
        }

        /// <summary>
        /// Useful in where clauses
        /// i.e. Jun 2015 -> "20150630"
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public static string GetStrYearMonthDay(int year, int month) {

            int days = DateTime.DaysInMonth(year, month);

            return year.ToString("0000") + month.ToString("00") + days.ToString("00");
        }

        /// <summary>
        /// i.e. "20150630" -> new DateTime(2015, 6, 30)
        /// </summary>
        /// <param name="yearMonthDay"></param>
        /// <returns></returns>
        public static DateTime GetDtYearMonthDay(string yearMonthDay) {
            return new DateTime(int.Parse(yearMonthDay.Substring(0, 4))
                        , int.Parse(yearMonthDay.Substring(4, 2))
                        , int.Parse(yearMonthDay.Substring(6, 2)));
        }
    }
}