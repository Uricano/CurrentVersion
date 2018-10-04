using Cherries.Models.dbo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.ViewModel
{
    public class TopSecuritiesViewModel
    {
        public List<Security> Securities { get; set; }
        public int NumOfRecords { get; set; }
    }
}
