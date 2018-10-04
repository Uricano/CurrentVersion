using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.Queries
{
    public class GetPortfolioQuery
    {
        public int pageNumber { get; set; }
        public int pageSize { get; set; }
        public string sortField { get; set; }
        public long userId { get; set; }
    }
}
