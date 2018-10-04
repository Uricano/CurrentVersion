using Cherries.TFI.BusinessLogic.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TFI.BusinessLogic.Interfaces
{
    public interface ICategoryCollection : IBaseBL
    {
        void setProperSecuritiesCollections(ISecurities cSecCol);
        void setCollectionWeights();
        int getCount();
        ICategoryItem getItemByName(String strName);
        ICategoryItem getItemByID(int iId);
        void AddIfNotExists(ICategoryItem cNewItem);
        void Add(ICategoryItem NewItem);
        void Clear();
    }
}
