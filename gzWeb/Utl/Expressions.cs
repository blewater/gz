using gzWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gzWeb.Utl {
    static public class Expressions {

        public static string GetStrYearMonth(int year, int month) {

            return year.ToString("0000") + month.ToString("00");
        }

        public static string GetStrYearMonthDay(int year, int month) {

            int days = DateTime.DaysInMonth(year, month);

            return year.ToString("0000") + month.ToString("00") + days.ToString("00");
        }

        public static DateTime GetDtYearMonthDay(string yearMonthDay) {
            return new DateTime(int.Parse(yearMonthDay.Substring(0, 4))
                        , int.Parse(yearMonthDay.Substring(4, 2))
                        , int.Parse(yearMonthDay.Substring(6, 2)));
        }
    }
}