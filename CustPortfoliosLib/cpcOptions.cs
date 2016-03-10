using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace cpc
{
    public class CpcOptions
    {
        public bool ParsingSuccess { get; private set; }

        [OptionArray('m', "month",
            HelpText = "the {yearmonth} YYYYMM value to calculate portfolio i.e. -m 201504 201506")]
        public uint[] YearMonthsToProc { get; set; }

        [OptionArray('i', "customerIds",  
            HelpText = "the customer Id(s) to calculate monthly balances."
                + " These must be integers i.e. -i 5 10 63")]
        public int[] CustomersToProc { get; set; }

        [Option('c', "console", DefaultValue = false,
            HelpText = "Prints all customer updates to standard output without updating the database.")]
        public bool ConsoleOutOnly { get; set; }

        [Option('f', "finance", MutuallyExclusiveSet = "market", DefaultValue = false,
            HelpText = "Update stock & currency market prices to the database; skip any portfolio calculations.")]
        public bool MarketUpdOnly { get; set; }

        [Option('s', "stock", MutuallyExclusiveSet = "market", DefaultValue = false,
            HelpText = "Update stock market prices to last closing day; skip any portfolio calculations.")]
        public bool StockMarketUpdOnly { get; set; }

        [Option('r', "currencies rates", MutuallyExclusiveSet = "market", DefaultValue = false,
            HelpText = "Update currencies to latest exchange rates; skip any portfolio calculations.")]
        public bool CurrenciesMarketUpdOnly { get; set; }

        [Option('v', "verbose", DefaultValue = false,
            HelpText = "Output extra debugging info.")]
        public bool VerboseOutput { get; set; }

        [Option('h', "help", DefaultValue = false,
            HelpText = "Output help info.")]
        public bool HelpOutput { get; set; }

        [HelpOption]
        string GetUsage() {
            var help = new HelpText {
                Heading = new HeadingInfo("<<Customers Portfolio Clearance>>", "<<0.9>>"),
                Copyright = new CopyrightInfo("GreenZorro", 2016),
                MaximumDisplayWidth = 80,
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };
            help.AddPreOptionsLine("Usage: cpc [options]");
            help.AddOptions(this);
            return help;
        }

        /// <summary>
        /// Parse the cpc CommandLineParser Options Parser from incoming argurments
        /// </summary>
        /// <param name="args">The string argurments array to parse and resolve</param>
        /// <returns></returns>
        public static CpcOptions ProcArgs(string[] args) {

            var options = new CpcOptions();

            var parser = new Parser(s => {
                s.MutuallyExclusive = true; s.CaseSensitive = false; s.HelpWriter = Console.Error;
            });

            options.ParsingSuccess = parser.ParseArguments(args, options);

            if (!options.ParsingSuccess || options.HelpOutput || args.Length == 0) {

                // Display the default usage information
                Console.WriteLine(options.GetUsage());
            }

            return options;
        }
    }
}
