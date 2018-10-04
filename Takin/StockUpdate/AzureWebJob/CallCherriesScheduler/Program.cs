using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace CallCherriesScheduler
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {

            List<Task> tasks = new List<Task>();
            // Main task - updates ExchangesDB database
            ////tasks.Add(new Task(delegate { Action1(); }));

            //********************************************************
            // New task (in development process, not for use!!!), updates ExchangesDB_Quandl database
            tasks.Add(new Task(delegate { Action2(); }));
            //********************************************************

            foreach (var item in tasks)
            {
                item.Start();
            }
            Task.WaitAll(tasks.ToArray());

            CherriesBatchProcess.GeneralFunctions.NotifyCherriesApp();    ////////   UNCOMMENT WHEN READY!!!!!!!  
        }

        public static void Action1()
        {
            // Application MAIN RUN 
            CherriesBatchProcess.Program.Main(new string[] { "-import", "eod", "-daily", "-s", "-p" });
            CherriesBatchProcess.Program.Main(new string[] { "-import", "eod", "-daily", "-i" });
            CherriesBatchProcess.Program.Main(new string[] { "-import", "predicta", "-daily", "-s", "-p", "-i" });
            CherriesBatchProcess.Program.Main(new string[] { "-calc" });

            // Test calls
            ////CherriesBatchProcess.Program.Main(new string[] { "-import", "eod", "2018/02/19", "2018/02/20", "-p" }); // it works - period, just was testing
            ////CherriesBatchProcess.Program.Main(new string[] { "-import", "eod", "-daily", "-p" }); // for testing
        }


        private static void Action2()
        {
            // ******************** INTRINIO MAIN PROCESS *** START **************************
            // Securities import
            // STEP 1
            CherriesBatchProcess.Program.Main(new string[] { "-import", "intrinio", "-initial", "-s" });        // getting new securities

            ////CherriesBatchProcess.Program.Main(new string[] { "-import", "intrinio", "-weekly", "-s" });     // fundamental - filling in data in existing securities


            // WARNING!!! It WAS executed for all USA and TASE securities ONCE ! 
            // DO NOT RUN IT without planning your steps (FOR DEVELOPERS ONLY) !!!!!!!!!!!!!
            ////!!!!!!!!CherriesBatchProcess.Program.Main(new string[] { "-import", "intrinio", "-daily", "-h" });       // historical prices starting from 01/01/2012

            // STEP 2
            CherriesBatchProcess.Program.Main(new string[] { "-import", "intrinio", "-daily", "-p" });                          // DAILY prices
            ////CherriesBatchProcess.Program.Main(new string[] { "-import", "intrinio", "2018/07/28", "2018/09/04", "-p" });    // Date range prices - not for indices!!!!!!

            // STEP 3
            CherriesBatchProcess.Program.Main(new string[] { "-calcintrinio" });    // Batch calculations after Intrinio import

            // ******************** INTRINIO MAIN PROCESS *** END ****************************


            ////// ************** CUSTOM RUN: ************** 
            ////// 1. IMPORT USA SEC.PRICES ONLY, 
            ////// 2. RUN dataCompleteMissingPrices for USA sec. only, 
            ////// 3. RUN calcPriceReturns for USA sec only
            ////// In order to run custom run there were 2 CUSTOM parts in Program.cs: historical and calcintrinio.
            ////CherriesBatchProcess.Program.Main(new string[] { "-import", "intrinio", "-daily", "-h" });  // historical prices starting from 01/01/2012
            ////CherriesBatchProcess.Program.Main(new string[] { "-calcintrinio" });                        // BatchCalculationsIntrinio_CUSTOM after Intrinio import
            ////// ___________________________________________________________________



            //===================  QUANDL company test calls (NOT USED) =======================================
            //CherriesBatchProcess.Program.Main(new string[] { "-import", "quandl", "-daily", "-s" });  // securities import   via ...ZACKS/CP.csv
            //CherriesBatchProcess.Program.Main(new string[] { "-import", "quandl", "-daily", "-p" });  // daily prices import via QuoteMedia
            //CherriesBatchProcess.Program.Main(new string[] { "-import", "quandl", "-daily", "-h" });  // historical data without date range restrictions via QuoteMedia
            //CherriesBatchProcess.Program.Main(new string[] { "-import", "quandl", "-daily", "-p" });
            // *****************************************************************


        }

    }
}
