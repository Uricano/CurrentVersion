using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

// Used namespaces
using Cherries.TFI.BusinessLogic.General;
using Cherries.TFI.BusinessLogic.Securities;
using Cherries.TFI.BusinessLogic.Categories;
using TFI.BusinessLogic.Enums;
using TFI.BusinessLogic.Interfaces;

namespace Cherries.TFI.BusinessLogic.Categories
{
    public class cCategoryCollection : CollectionBase, ICategoryCollection
    {

        // TODO: Replace with cCategoriesHandler


        #region Data members

        // Main variables
        private IErrorHandler m_objErrorHandler; // Error handler class pointer

        // Data variables
        private enumCatType m_enCategoryType; // Category type of collection

        #endregion Data members

        #region Constructor

        public cCategoryCollection(enumCatType eType, IErrorHandler cErrors)
        {
            m_objErrorHandler = cErrors;
            m_enCategoryType = eType;
        }//constructor

        #endregion Constructor

        #region Methods

        #region Initializing Collection

        public void setProperSecuritiesCollections(ISecurities cSecCol)
        { // Sets children (securities) for each collection item
            try
            {
                ICategoryItem cCurrItem;
                clearSecuritiesItems();
                for (int iSecs = 0; iSecs < cSecCol.Count; iSecs++)
                { // Goes through collection of securities
                    cCurrItem = cSecCol[iSecs].Properties.getCurrentCategoryValue(m_enCategoryType); // gets item
                    cCurrItem.Securities.Add(cSecCol[iSecs]);
                }
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//setProperSecuritiesCollections

        private void clearSecuritiesItems()
        { // Clears children (securities) of all collection items
            for (int iItems = 0; iItems < base.Count; iItems++)
                this[iItems].Securities.Clear();
        }//clearSecuritiesItems

        public void setCollectionWeights()
        { // Sets the weights for the items in collection (in case they are not of security type)
            for (int iItems = 0; iItems < base.Count; iItems++)
                this[iItems].calculateItemWeight();
        }//setCollectionWeights

        public int getCount()
        { return this.Count; }//getCount

        #endregion Initializing Collection

        #region Locating Items

        private Boolean isItemExistsInCollection(String strName)
        { // Checks whether a current item exists in collection
            for (int iItems = 0; iItems < base.Count; iItems++)
                if (this[iItems].ItemName == strName) return true;
            return false;
        }//isItemExistsInCollection

        public ICategoryItem getItemByID(int iId)
        { // Retrieves the proper item from collection by specified ID
            for (int iItems = 0; iItems < base.Count; iItems++)
                if (this[iItems].ID == iId) return this[iItems];
            return null;
        }//getItemByID

        public ICategoryItem getItemByName(String strName)
        { // Retrieves the proper item from collection by specified value
            for (int iItems = 0; iItems < base.Count; iItems++)
                if (this[iItems].ItemName == strName) return this[iItems];
            return null;
        }//getItemByName

        #endregion Locating Items

        #region Collection Methods

        public virtual void AddIfNotExists(ICategoryItem cNewItem)
        { // Adds item to collection only if it doesn't already exist
            if (!isItemExistsInCollection(cNewItem.ItemName)) Add(cNewItem);
        }//AddIfNotExists

        public virtual void Add(ICategoryItem NewItem)
        { // Adds Item to collection
            this.List.Add(NewItem);
        }//Add
        
        public virtual cCategoryItem this[int Index]
        { //return the Security at IList[Index]
            get { return (cCategoryItem)this.List[Index]; }
        }//this[int Index]

        #endregion Collection Methods

        #endregion Methods

    }//of class
}
