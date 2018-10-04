using Cherries.Models.App;
using Cherries.Models.Queries;
using Cherries.Models.ViewModel;
using Cherries.TFI.BusinessLogic.DataManagement.DataTypes;
using Ness.DataAccess.Repository;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFI.BusinessLogic.Bootstraper;
using TFI.BusinessLogic.Interfaces;
using TFI.Entities.dbo;

namespace TFI.BusinessLogic.Classes.Optimization.Backtesting
{
    public class cBacktestingSecurities : IBacktestingSecurities
    {

        #region Data Members

        // Main variables
        private IPortfolioBL m_objPortfolio; // Portfolio pointer class
        private List<IBacktestingSecurity> m_colBackSecurities = new List<IBacktestingSecurity>(); // Collection of backtesting securities
        private ISecurities m_colSecurities; // Collection of securities

        // Data variables
        private IBacktestingSecurity m_objTempSec; // Temporary security instance (for binary search)
        private IRepository m_repository;

        #endregion Data Members

        #region Consturctors, Initialization & Destructor

        public cBacktestingSecurities(IPortfolioBL cPort)
        {
            m_objPortfolio = cPort;
            repository = Resolver.Resolve<IRepository>();
        }//constructor

        #endregion Consturctors, Initialization & Destructor

        #region Methods

        #region General methods

        public void setBacktestingSecuritiesCollection(ISecurities secsCol, cDateRange cDrRange)
        { // Sets the collection of backtesting securities, based on a given securities collection
            m_colSecurities = secsCol;

            m_colBackSecurities.Clear();
            for (int iSecs = 0; iSecs < secsCol.Count; iSecs++)
                m_colBackSecurities.Add(new cBacktestingSecurity(m_objPortfolio, secsCol[iSecs], cDrRange));
        }//setBacktestingSecuritiesCollection

        public int GetSecurityInMemoryCount()
        {
            var secs = StaticData<cBacktestingSecurity, IBacktestingSecurities>.lst;
            return secs.Count;
        }//GetSecurityInMemoryCount

        #endregion General methods
        
        #endregion Methods

        #region Collection methods

        public virtual void Add(IBacktestingSecurity NewSecurity)
        { // Adds security to collection
            m_colBackSecurities.Add(NewSecurity);
        }//Add

        public virtual IBacktestingSecurity this[int Index]
        { //return the Security at IList[Index]
            get { return m_colBackSecurities[Index]; }
        }//this[int Index]

        public void RemoveAt(int iPos)
        { m_colBackSecurities.RemoveAt(iPos); }//RemoveAt

        public virtual int Count
        { get { return m_colSecurities.Count; } }//Count

        public virtual void Clear()
        { m_colSecurities.Clear(); }//Clear

        public void Dispose()
        {
            Resolver.Release(repository);
        }//Dispose

        #endregion Collection methods

        #region Properties

        public IPortfolioBL Portfolio { get { return m_objPortfolio; } set { m_objPortfolio = value; } }//Portfolio
        public IBacktestingSecurity TempSec { get { return m_objTempSec; } set { m_objTempSec = value; } }//TempSec
        public IRepository repository { get { return m_repository; } set { m_repository = value; } }//repository

        public List<IBacktestingSecurity> BacktestingSecurities { get { return m_colBackSecurities; } set { m_colBackSecurities = value; } }//BacktestingSecurities
        public ISecurities Securities { get { return m_colSecurities; } set { m_colSecurities = value; } }//Securities

        #endregion Properties

    }//of class
}
