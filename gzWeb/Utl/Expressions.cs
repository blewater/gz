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

        public static string GetStrYearMonthLastDay(int year, int month) {

            int days = DateTime.DaysInMonth(year, month);

            return year.ToString("0000") + month.ToString("00") + days.ToString("00");
        }
    }
}