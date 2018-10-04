using Cherries.TFI.BusinessLogic.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TFI.Entities.Lookup;

namespace TFI.BusinessLogic.Interfaces
{
    public interface ICatItem : IBaseBL
    {
        void setCategoryItems<T>() where T : LookupBase;
        int getCategoryPos(String strVal);
        int getCategoryId(String strVal);
        String getCategoryVal(int iId);
       
    }
}
