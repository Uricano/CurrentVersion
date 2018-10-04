using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.App
{
    public class BacktestingPort
    {

        #region Data Members

        // Data variables
        private PortfolioDetails PortDetails;   // Portfolio details variable
        private List<String> m_colBenchmarkNames = new List<string>(); // Collection of benchmark names

        // Date variables
        private DateTime m_dtDateStart = DateTime.MinValue; // Date portfolio was created
        private DateTime m_dtDateEnd = DateTime.MinValue;  // Date the portfolio was last edited

        #endregion Data Members

        #region Properties

        public PortfolioDetails Details
        {
            get { return PortDetails; }
            set { PortDetails = value; }
        }//Details

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
       

        public List<String> BenchmarkNames
        {
            get { return m_colBenchmarkNames; }
            set { m_colBenchmarkNames = value; }
        }//BenchmarkNames

        #endregion Properties

    }//of Model
}
