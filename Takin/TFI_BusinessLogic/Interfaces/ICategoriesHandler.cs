using Cherries.TFI.BusinessLogic.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TFI.BusinessLogic.Enums;

namespace TFI.BusinessLogic.Interfaces
{
    public interface ICategoriesHandler : IBaseBL
    {
        int getCategoryPos(enumCatType eType, String strName);
        int getCategoryId(enumCatType eType, String strName);
        String getCategoryName(enumCatType eType, int iCatId);
        ICatItem getCategoryCol(enumCatType eType);
        String getCategoryDefault(enumCatType eType);
        cCatItems Categories { get; }
    }
}
