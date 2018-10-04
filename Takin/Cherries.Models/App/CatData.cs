using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.App
{
    public class CatData
    {
        public int iIndex; // Index value
        public String strValue; // Value
        public string strValueShort;
        public Boolean isNew; // Is new value (just inserted)

        public CatData(int iId, String strName, string shortValue = "")
        {
            iIndex = iId;
            strValue = strName;
            isNew = false;
            strValueShort = "";
        }//constructor
    }
}
