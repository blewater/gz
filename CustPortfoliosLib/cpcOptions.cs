using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDesk.Options;

namespace cpc
{
    public static class CpcOptions
    {

        private static List<int> customersToProc = new List<int>();
        public static List<int> CustomersToProc {
            get {
                return customersToProc;
            }
        }

        private static List<string> yearMonthsToProc = new List<string>();
        public static List<string> YearMonnthsToProc {
            get {
                return yearMonthsToProc;
            }
        }

        public static bool SaveToDb { get; private set; }
        public static bool MarketUpdOnly { get; private set; }
        public static bool DebugOutput { get; private set; }

        /// <summary>
        /// Static constructor
        /// </summary>
        static CpcOptions() {
            SaveToDb = true;
            MarketUpdOnly = false;
            DebugOutput = false;
        }


        public static void ProcArgs(string[] args) {

            bool show_help = false;

            var p = new OptionSet() {

                { "i|customer id=", "the {customer Id} to calculate monthly balances.\n" +
                    "this must be an integer.",
                  (int v) => CustomersToProc.Add (v) },

                { "m|month=",
                    "the {yearmonth} YYYYMM i.e. 201504 value to calculate portfolio values=",
                  v => YearMonnthsToProc.Add(v) },

                { "s|stock & currency market update only to the database\n",
                    "skip any portfolio calculations.",
                  v => { if (v != null) MarketUpdOnly = true; } },

                { "c|console only",
                    "output to the console without saving to the database",
                  v => { if (v != null) SaveToDb = false; } },

                { "v", "output debug verbosity",
                  v => { if (v != null) DebugOutput = true; } },

                { "h|help", "show this message and exit",
                  v => show_help = v != null },
            };

            List<string> extra;
            try {
                extra = p.Parse(args);
            } catch (OptionException e) {
                Console.Write("cpc: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `cpc --help' for more information.");
                return;
            }

            if (show_help) {
                ShowHelp(p);
                return;
            }

            string message;
            if (extra.Count > 0) {
                message = string.Join(" ", extra.ToArray());
                Debug("Using new message: {0}", message);
            } else {
                message = "Hello {0}!";
                Debug("Using default message: {0}", message);
            }
        }

        static void ShowHelp(OptionSet p) {
            Console.WriteLine("Usage: cpc [OPTIONS]+ message");
            Console.WriteLine("Calculate customer portfolio values on a monthly basis and save customer balances.");
            Console.WriteLine("If no parameter is specified, the current month is cleared for all customers.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

        static void Debug(string format, params object[] args) {
            if (DebugOutput) {

                

                Console.Write("# ");
                Console.WriteLine(format, args);
            }
        }
    }
}
