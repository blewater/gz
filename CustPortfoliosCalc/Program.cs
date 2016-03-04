using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDesk.Options;

namespace CustPortfoliosCalc {

    class Program {
        static int verbosity;

        static void Main(string[] args) {
            bool show_help = false;
            List<int> customers = new List<int>();
            List<string> yearMonths = new List<string>();
            bool saveDb = true;
            bool marketUpdOnly = false;
            bool debugOutput = false;

            var p = new OptionSet() {

                { "i|customer id=", "the {customer Id} to calculate monthly balances.\n" +
                    "this must be an integer.",
                  (int v) => customers.Add (v) },

                { "m|month=",
                    "the {yearmonth} YYYYMM i.e. 201504 value to calculate portfolio values=",
                  v => yearMonths.Add(v) },

                { "s|stock & currency market update only to the database\n",
                    "skip any portfolio calculations.",
                  v => marketUpdOnly = v==null?true:false },

                { "c|console only",
                    "output to the console without saving to the database",
                  v => saveDb = v==null?true:false },

                { "v", "output debug verbosity",
                  v => { if (v != null) debugOutput = true; } },

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
            if (verbosity > 0) {
                Console.Write("# ");
                Console.WriteLine(format, args);
            }
        }
    }
}
