using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.App
{
    public static class StaticData<T, V>
    {
        public static Dictionary<int, DateTime> dicUpdatedExchanges { get; set; }
        public static List<T> lst { get; set; }
        public static V BenchMarks { get; set; }
        //public static List<Models.dbo.Price> ALLprices { get; set; }
    }
}
