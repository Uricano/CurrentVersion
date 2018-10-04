using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Linq;
// Used namespaces
using Cherries.TFI.BusinessLogic.DataManagement;
using Cherries.TFI.BusinessLogic.General;
using TFI.BusinessLogic.Enums;
using TFI.Entities.Lookup;
using Ness.DataAccess.Repository;
using NHibernate.Linq;
using TFI.BusinessLogic.Interfaces;
using Cherries.Models.App;

namespace Cherries.TFI.BusinessLogic.Categories
{

    #region Structs
    
    #endregion Structs

    #region cCatItem class

    public class cCatItem : ICatItem, IBaseBL
    { // a class containing the items (contents) of a certain categories table

        #region Data members

        // Main variables
        
        private IErrorHandler m_objErrorHandler; // Error handler class
        private IRepository repository;
        // Data variables
        private enumCatType m_enumCategory; // Category type
        private List<CatData> m_colCatItems = new List<CatData>(); // Items contained within class
        private int m_iDefaultValue = 0; // Default value index

        #endregion Data members

        #region Consturctors, Initialization & Destructor

        public cCatItem(IErrorHandler cErrors, IRepository rep, enumCatType eType)
        {
            m_objErrorHandler = cErrors;
            repository = rep;
            m_enumCategory = eType;
            //setCategoryItems();
        }//constructor

        #endregion Consturctors, Initialization & Destructor

        #region Methods

        #region Load main data

        public void setCategoryItems<T>() where T : LookupBase
        { // Fills the items of the current category
            try
            {
                repository.Execute(session =>
                {
                    m_colCatItems.Clear();
                    //var dtCatItems =    //cDbOleConnection.FillDataTable(cSqlStatements.getTblItemsSQL(cCategoriesHandler.getTblName(m_enumCategory)), m_objMainConnection.dbConnection);
                    var dtCatItems = session.Query<T>().Where(x=> x.IsActive).ToList();
                    for (int iRows = 0; iRows < dtCatItems.Count; iRows++)
                        m_colCatItems.Add(new CatData(Convert.ToInt32(dtCatItems[iRows].id), dtCatItems[iRows].strName != null ? dtCatItems[iRows].strName : dtCatItems[iRows].strHebName, dtCatItems[iRows].strHebName));
                });
               
               
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            } 
        }//setCategoryItems

        #endregion Load main data

        #region Retrieve data

        public int getCategoryPos(String strVal)
        { // Retrieves the position of a certain category 
            for (int iItems = 0; iItems < m_colCatItems.Count; iItems++)
                if (m_colCatItems[iItems].strValue == strVal) return iItems;
            return -1;
        }//getCategoryId

        public int getCategoryId(String strVal)
        { // Retrieves the position of a certain category 
            for (int iItems = 0; iItems < m_colCatItems.Count; iItems++)
                if (m_colCatItems[iItems].strValue == strVal) return m_colCatItems[iItems].iIndex;
            return -1;
        }//getCategoryId

        public String getCategoryVal(int iId)
        { // Retrieves category item which matches a given id
            for (int iItems = 0; iItems < m_colCatItems.Count; iItems++)
                if (m_colCatItems[iItems].iIndex == iId) return m_colCatItems[iItems].strValue;
            return "";
        }//getCategoryVal

        #endregion Retrieve data

        #endregion Methods

        #region Properties

        public List<CatData> Items
        { get { return m_colCatItems; } }//Items

        public CatData DefaultItem
        { get { return m_colCatItems[m_iDefaultValue]; } }//DefaultItem

        public enumCatType CategoryType
        { get { return m_enumCategory; } }//CategoryType

        #endregion Properties

    }//of cCatItem class

    #endregion cCatItem class

    #region cCatItems class collection

    public class cCatItems : CollectionBase
    {
        #region Data members

        #endregion Data members

        #region Methods

        public int getCollectionPos(enumCatType eCat)
        { // Retrieves the position of the category in collection
            for (int iCats = 0; iCats < base.Count; iCats++)
                if (this[iCats].CategoryType == eCat) return iCats;
            return 0;
        }//getCollectionPos

        #endregion Methods

        #region Collection methods

        public virtual void Add(cCatItem NewCategory)
        { // Adds security to collection
            this.List.Add(NewCategory);
        }//Add

        public virtual cCatItem this[int Index]
        { //return the Security at IList[Index]
            get { return (cCatItem)this.List[Index]; }
        }//this[int Index]

        #endregion Collection methods

    }//of class collection

    #endregion cCatItems class collection

}//namespace
