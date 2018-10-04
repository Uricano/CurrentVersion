using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

// Used namespaces
using Cherries.TFI.BusinessLogic.General;
using Cherries.TFI.BusinessLogic.Collections;
using Cherries.TFI.BusinessLogic.DataManagement;
using TFI.BusinessLogic.Enums;
using TFI.Entities.Lookup;
using TFI.BusinessLogic.Interfaces;
using Ness.DataAccess.Repository;
using TFI.BusinessLogic.Bootstraper;

namespace Cherries.TFI.BusinessLogic.Categories
{
    public class cCategoriesHandler : ICategoriesHandler, IDisposable
    {

        #region Data Members

        // Main variables
        
        private cCatItems m_colCatItems = new cCatItems(); // Collection of category items
        private IErrorHandler m_objErrorHandler; // Error handler class
        private IRepository repository;
        #endregion Data Members

        #region Consturctors, Initialization & Destructor

        public cCategoriesHandler(IErrorHandler cErrors)
        {
            m_objErrorHandler = cErrors;
            repository = Resolver.Resolve<IRepository>();
            initMainCatCollection();
        }//constructor

        #endregion Consturctors, Initialization & Destructor

        #region Methods

        #region Internal methods

        private void initMainCatCollection()
        { // Initializes collection of categories with all values from DB
            m_colCatItems.Add(new cCatItem(m_objErrorHandler, repository, enumCatType.SecurityType));
            m_colCatItems[m_colCatItems.Count - 1].setCategoryItems<SelSecurityType>();
            m_colCatItems.Add(new cCatItem(m_objErrorHandler, repository, enumCatType.StockMarket));
            m_colCatItems[m_colCatItems.Count - 1].setCategoryItems<SelStockExchange>();
            m_colCatItems.Add(new cCatItem(m_objErrorHandler, repository, enumCatType.Sector));
            m_colCatItems[m_colCatItems.Count - 1].setCategoryItems<Sector>();
        }//initMainCatCollection

        #endregion Internal methods

        #region Data insertion

        //public void addNewValue(enumCatType eType, String strVal)
        //{ // Inserts new value to DB and Category collections
        //    String strTblName = getTblName(eType);
        //    insertValToDB(strTblName, strVal);
        //    m_colCatItems[m_colCatItems.getCollectionPos(eType)].Items.Add(new sCatData(getCategoryIdFromDB(eType, strTblName, strVal), strVal));
        //}//addNewValue

        //private void insertValToDB(String strTblName, String strVal)
        //{ // Inserts category value to DB
        //    String strCommand = cSqlStatements.insertCategoryItemToDbSQL(strTblName, strVal);
        //    SqlCommand sqlNewCommand = new SqlCommand(strCommand, m_objMainConnection.dbConnection);
        //    sqlNewCommand.ExecuteNonQuery();
        //    if (sqlNewCommand != null) sqlNewCommand.Dispose();
        //}//insertValToDB

        //private int getCategoryIdFromDB(enumCatType eType, String strTblName, String strVal)
        //{ // Retrieves the category ID after it has been inserted to DB
        //    SqlDataReader sqlNewReader = null;
        //    try
        //    {
        //        SqlCommand sqlNewCommand = new SqlCommand(cSqlStatements.getTblValsByStrConditionSQL(strTblName, "strName", strVal), m_objMainConnection.dbConnection);
        //        sqlNewReader = sqlNewCommand.ExecuteReader();
        //        if (sqlNewReader.Read()) return Convert.ToInt32(sqlNewReader[getIdFldName(eType)]);
        //    } catch (Exception ex) {
        //        m_objErrorHandler.LogInfo(ex);
        //    } finally {
        //        if (sqlNewReader != null) sqlNewReader.Dispose();
        //    }
        //    return -1;
        //}//getCatValIdFromDB

        #endregion Data insertion

        #region Retrieve data

        public int getCategoryPos(enumCatType eType, String strName)
        { // Retrieves the position of a given name from a given category array
            return m_colCatItems[m_colCatItems.getCollectionPos(eType)].getCategoryPos(strName);
        }//getCategoryPos

        public int getCategoryId(enumCatType eType, String strName)
        { // Retrieves the id of a given name from a given category table
            return m_colCatItems[m_colCatItems.getCollectionPos(eType)].getCategoryId(strName);
        }//getCategoryId

        public String getCategoryName(enumCatType eType, int iCatId)
        { // Returns category value for a given index
            return m_colCatItems[m_colCatItems.getCollectionPos(eType)].getCategoryVal(iCatId);
        }//getCategoryName

        public ICatItem getCategoryCol(enumCatType eType)
        { // Retrieves collection of a given category
            return m_colCatItems[m_colCatItems.getCollectionPos(eType)];
        }//getCategoryCol

        public String getCategoryDefault(enumCatType eType)
        { // Retrieves default value of a given category
            return m_colCatItems[m_colCatItems.getCollectionPos(eType)].DefaultItem.strValue;
        }//getCategoryDefault

        public void Dispose()
        {
            Resolver.Release(repository);
        }

        #endregion Retrieve data



        #endregion Methods

        public cCatItems Categories { get { return m_colCatItems; } }

    }//of class
}
