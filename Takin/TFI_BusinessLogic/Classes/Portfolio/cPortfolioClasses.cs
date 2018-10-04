using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Used namespaces
using Cherries.TFI.BusinessLogic.Categories;
using Cherries.TFI.BusinessLogic.Constraints;
using Cherries.TFI.BusinessLogic.DataManagement.Prices;
using Cherries.TFI.BusinessLogic.GMath;
//using Cherries.Classes.GMath.CAPM;
using Cherries.TFI.BusinessLogic.GMath.EF;
using Cherries.TFI.BusinessLogic.Optimization;
using DotNetScilab;
using TFI.BusinessLogic.Interfaces;

namespace Cherries.TFI.BusinessLogic.Portfolio
{
    public class cPortfolioClasses
    { // Contains the pointers for the class instances available to portfolio

        #region Data Members

        // Main variables
        private ICategoriesHandler m_objCategoryHandler; // Categories handler class

        // Data variables
        private cPricesHandler m_objPriceHandler; // Prices handler class pointer
        private IRateHandler m_objRateHandler; // Rates handler class pointer
        private ICovarCorrelHandler m_objCovarCorrel; // Covariance / Correlation handler
        private cConstHandler m_objConstHandler; // Constraint handler class
        //private cEFHandler m_objEfHandler; // Efficient frontier handler class
        private IOptimizationResults m_objOptimizer; // Optimization handler class

        #endregion Data Members

        #region Properties

        #region Main variables

        public ICategoriesHandler CategoryHandler
        { 
            get { return m_objCategoryHandler; }
            set { m_objCategoryHandler = value; }
        }//CategoryHandler

        #endregion Main variables

        #region Data variables

        public cPricesHandler PriceHandler
        {
            get { return m_objPriceHandler; }
            set { m_objPriceHandler = value; }
        }//PriceHandler

        public IRateHandler RatesHandler
        {
            get { return m_objRateHandler; }
            set { m_objRateHandler = value; }
        }//RatesHandler

        public ICovarCorrelHandler CovarCorrel
        {
            get { return m_objCovarCorrel; }
            set { m_objCovarCorrel = value; }
        }//CovarCorrel

        public cConstHandler ConstHandler
        {
            get { return m_objConstHandler; }
            set { m_objConstHandler = value; }
        }//ConstHandler

        //public cEFHandler EfficientFrontier
        //{
        //    get { return m_objEfHandler; }
        //    set { m_objEfHandler = value; }
        //}//EfficientFrontier

        public IOptimizationResults Optimizer
        {
            get { return m_objOptimizer; }
            set { m_objOptimizer = value; }
        }//Optimizer

        #endregion Data variables

        #endregion Properties

    }//of class
}
