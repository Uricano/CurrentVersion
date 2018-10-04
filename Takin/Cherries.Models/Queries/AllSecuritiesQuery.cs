using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TFI.BusinessLogic.Enums;

namespace Cherries.Models.Queries
{
    public class AllSecuritiesQuery
    {
        public List<int> exchangesPackagees { get; set; }
        public List<int> sectors { get; set; }
        public double maxRiskLevel { get; set; }
        public int pageNumber { get; set; }
        public int pageSize { get; set; }
        public string searchText { get; set; }
        public string field { get; set; }
        public string direction { get; set; }
        public bool hideDisqualified { get; set; }
    }

}
