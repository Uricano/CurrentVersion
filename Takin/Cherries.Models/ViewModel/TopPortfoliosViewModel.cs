using Cherries.Models.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.ViewModel
{
    public class TopPortfoliosViewModel
    {
        public List<PortfolioDetails> Portfolios { get; set; }
        public int NumOfRecords { get; set; }
    }
}

