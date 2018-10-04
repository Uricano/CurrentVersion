using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

// Used namespaces
using Cherries.TFI.BusinessLogic.Securities;
using Cherries.TFI.BusinessLogic.General;
using TFI.BusinessLogic.Enums;
using Cherries.TFI.BusinessLogic.Portfolio;
using TFI.BusinessLogic.Interfaces;
using TFI.BusinessLogic.Bootstraper;
using Ness.DataAccess.Repository;

namespace Cherries.TFI.BusinessLogic.Categories
{
    public class cCategoryItem : ICategoryItem
    {

        #region Data Members

        // Main variables
        private IPortfolioBL m_objPortfolio; // Portfolio data
        private IErrorHandler m_objErrorHandler; // Error handler class

        // Data variables
        private enumCatType m_enumItemType = enumCatType.Sector; // Item type (Sector / Exchange / Type)
        private int m_iItemId = 0; // Item ID (as found in DB)
        private String m_strName = ""; // Item Name
        private double m_dWeight = 0D; // Item weight (for EF calculation)
        private Color m_objColor = new Color(); // Item Color (for charts)

        // Collection variables
        private ISecurities m_colSecurities; // Securities belonging to current

        #endregion Data Members

        #region Constructors, Initialization & Destructors

        public cCategoryItem(enumCatType eType, String strName, int iId, IErrorHandler cErrors, IPortfolioBL cPort)
        {
            m_enumItemType = eType;
            m_strName = strName;
            m_iItemId = iId;
            m_objErrorHandler = cErrors;
            m_objPortfolio = cPort;
            m_colSecurities = new cSecurities(m_objPortfolio);

            try
            {
                m_objColor = Color.FromArgb(cProperties.RndGenerator.Next(0, 255), cProperties.RndGenerator.Next(0, 255), cProperties.RndGenerator.Next(0, 255));
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//constructor

        #endregion Constructors, Initialization & Destructors

        #region Methods

        public void calculateItemWeight()
        { // Calculated the weight of the item (if not security item)
            try
            {
                m_dWeight = 0D;
                for (int iSecs = 0; iSecs < m_colSecurities.Count; iSecs++)
                    m_dWeight += m_colSecurities[iSecs].Weight;
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
        }//calculateItemWeight

        public cCategoryCollection getCategoryMatchingItems(enumCatType eType)
        { // Retrieves the desired category items matching the current item
            cCategoryCollection cFinalCol = new cCategoryCollection(eType, m_objErrorHandler);
            try
            {
                for (int iSecs = 0; iSecs < m_colSecurities.Count; iSecs++) // Goes through item securities (to obtain target items)
                    cFinalCol.AddIfNotExists(m_colSecurities[iSecs].Properties.getCurrentCategoryValue(eType));
            } catch (Exception ex) {
                m_objErrorHandler.LogInfo(ex);
            }
            return cFinalCol;
        }//getCategoryMatchingItems

        public Boolean isWithSecurity(cSecurity cCurrSec)
        { // Verifies the specified security is found within the collection
            for (int iSecs = 0; iSecs < m_colSecurities.Count; iSecs++)
                if (m_colSecurities[iSecs].Properties.PortSecurityId == cCurrSec.Properties.PortSecurityId) return true;
            return false;
        }//isWithSecurity

        #endregion Methods

        #region Properties

        public int ID
        { 
            get { return m_iItemId; }
            set { m_iItemId = value; }
        }//ID

        public String ItemName
        { get { return m_strName; } }//ItemName

        public double Weight
        { get { return m_dWeight; } }//Weight

        public enumCatType Type
        { get { return m_enumItemType; } }//Type

        public ISecurities Securities
        { get { return m_colSecurities; } }//Securities

        public Color Color
        { get { return m_objColor; } }//MainColor

        #endregion Properties

    }//of class
}
