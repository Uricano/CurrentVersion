using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;

//Used namespaces
using Cherries.TFI.BusinessLogic.General;
using Cherries.TFI.BusinessLogic.Portfolio;
using System.Collections.Concurrent;
using Ness.DataAccess.Repository;
using TFI.BusinessLogic.Interfaces;
using TFI.BusinessLogic.Bootstraper;
using Cherries.Models.dbo;
using Cherries.Models.App;
using System.Web;
using Cherries.TFI.BusinessLogic.Collections;
using TFI.Entities.dbo;
using Entities = TFI.Entities;
using Cherries.Models.ViewModel;
using NHibernate.Linq;
using Cherries.Models.Queries;

namespace Cherries.TFI.BusinessLogic.Securities
{

    #region Comparer class

    public class SecsComparer : IComparer<ISecurity>
    { // Comparer class for binary search algorithm

        public int Compare(ISecurity cSec1, ISecurity cSec2)
        { // Compares 2 securities
            if (cSec1 == null)
            {
                if (cSec2 == null) // If x is null and y is null, they're equal.
                    return 0;
                else // If x is null and y is not null, y is greater.  
                    return -1;

            } else { // If x is not null
                if (cSec2 == null) // ...and y is null, x is greater.
                    return 1;
                else
                { // ...and y is not null, compare the lengths of the two strings.
                    if (cSec1.Properties.PortSecurityId == "" || cSec2.Properties.PortSecurityId == "") return 1;
                    double dSecId1 = Convert.ToDouble(cSec1.Properties.PortSecurityId);
                    double dSecId2 = Convert.ToDouble(cSec2.Properties.PortSecurityId);

                    if (dSecId1 == dSecId2) return 0; // Similar value

                    if (dSecId1 > dSecId2)
                        return 1;
                    else return -1;
                }
            }// Main if
        }//Compare

    }//Comparer class

    #endregion Comparer class

    public class cSecurities : ISecurities, IDisposable
    {

        #region Data Members

        // Main variables
        private IPortfolioBL m_objPortfolio; // Portfolio pointer class
        private List<ISecurity> m_colSecurities = new List<ISecurity>(); // Collection of securities

        // Data variables
        private cSecurity m_objTempSec; // Temporary security instance (for binary search)
        private SecsComparer m_objSecsComparer = new SecsComparer(); // Comparison class for binary search
        private IRepository repository;
        //private int[] m_arrPrevIndexes; // Sorting order of securities (by indexes)

        #endregion Data Members

        #region Consturctors, Initialization & Destructor

        public cSecurities(IPortfolioBL cPort) 
        {
            m_objPortfolio = cPort;
            repository = Resolver.Resolve<IRepository>();
        }//constructor

        #endregion Consturctors, Initialization & Destructor

        #region Methods

        #region General methods

        public cSecurities getListOfActiveSecs()
        { // Returns the collection of active securities
            cSecurities cActiveSecs = new cSecurities(m_objPortfolio);
            for (int iSecs = 0; iSecs < this.Count; iSecs++)
                if (this[iSecs].isActive)
                    cActiveSecs.Add(this[iSecs]);
            return cActiveSecs;
        }//getListOfActiveSecs

        public void undisableSecuritiesList()
        { // Sets all securities in collection to enabled
            for (int iSecs = 0; iSecs < this.Count; iSecs++)
                this[iSecs].enableCurrentSecurity();
        }//undisableSecuritiesList

        public List<Models.dbo.Security> GetTopSecurities(List<int> exchangesPackagees, List<int> ranks, string security_list = "ALL")
        {
            Dictionary<string, Tuple<object,NHibernate.Type.IType>> param = new Dictionary<string, Tuple<object, NHibernate.Type.IType>>();
            param.Add("stock_market_id_list", new Tuple<object, NHibernate.Type.IType>(string.Join(",", exchangesPackagees), NHibernate.NHibernateUtil.String)); 
            param.Add("ranking_id_list", new Tuple<object, NHibernate.Type.IType>(string.Join(",", ranks), NHibernate.NHibernateUtil.String));
            if(security_list != "ALL")
                param.Add("security_list", new Tuple<object, NHibernate.Type.IType>(security_list, NHibernate.NHibernateUtil.StringClob));

            List<TopSecurities> topSecs = repository.ExecuteSp<TopSecurities>("dataGetMainSecurities", param).GroupBy(x => x.strSymbol).Select(g => g.First()).OrderBy(x=>x.idMarket).ToList();
            return AutoMapper.Mapper.Map<List<Models.dbo.Security>>(topSecs);
        }


        public List<Models.dbo.BMsecurity> GetBMSecurities()
        {
            Dictionary<string, Tuple<object, NHibernate.Type.IType>> param = new Dictionary<string, Tuple<object, NHibernate.Type.IType>>();
            //param.Add("stock_market_id_list", new Tuple<object, NHibernate.Type.IType>(string.Join(",", exchangesPackagees), NHibernate.NHibernateUtil.String));
            //param.Add("ranking_id_list", new Tuple<object, NHibernate.Type.IType>(string.Join(",", ranks), NHibernate.NHibernateUtil.String));

            List<Entities.dbo.BMsecurity> BMSecs = repository.ExecuteSp<Entities.dbo.BMsecurity>("dataGetBMSecurities", param).ToList();
            return AutoMapper.Mapper.Map<List<Models.dbo.BMsecurity>>(BMSecs);
        }//GetBMSecurities

        public ISecurities GetCustomSecurities(IQueryable<TopSecurities> topSecs)
        {
            List<Models.dbo.Security> colSecs = AutoMapper.Mapper.Map<List<Models.dbo.Security>>(topSecs);
            List<cSecurity> finalSecs = m_objPortfolio.ColHandler.convertListToCSecurity(colSecs);


            ISecurities secs = new cSecurities(m_objPortfolio);
            for (int iSecs = 0; iSecs < finalSecs.Count; iSecs++)
                secs.Add(finalSecs[iSecs]);

            return secs;
            //return AutoMapper.Mapper.Map<List<Models.dbo.Security>>(topSecs);
        }



        public List<Models.dbo.BMPrice> GetBMPrices()
        {
            Dictionary<string, Tuple<object, NHibernate.Type.IType>> param = new Dictionary<string, Tuple<object, NHibernate.Type.IType>>();
            //param.Add("stock_market_id_list", new Tuple<object, NHibernate.Type.IType>(string.Join(",", exchangesPackagees), NHibernate.NHibernateUtil.String));
            //param.Add("ranking_id_list", new Tuple<object, NHibernate.Type.IType>(string.Join(",", ranks), NHibernate.NHibernateUtil.String));

            List<Entities.dbo.BMPrice> BMprices = repository.ExecuteSp<Entities.dbo.BMPrice>("dataGetIndexPrices", param).ToList();
            return AutoMapper.Mapper.Map<List<Models.dbo.BMPrice>>(BMprices);
        }//GetBMPrices



        public TopSecuritiesViewModel GetAllSecurities(AllSecuritiesQuery query)
        {
            TopSecuritiesViewModel vm = new TopSecuritiesViewModel();
            repository.Execute(session =>
            {
                int startRow = (query.pageNumber - 1) * query.pageSize;
                var securities = session.Query<TopSecurities>().Where(x => query.exchangesPackagees.Contains(x.idMarket));
                if (query.sectors.Count > 0) securities = securities.Where(x => query.sectors.Contains(x.idSector));
                if (query.maxRiskLevel > 0)
                {
                    double weekRiskLevel = (query.maxRiskLevel / Math.Sqrt(52)) / 100;
                    securities = securities.Where(x => x.StdYield <= weekRiskLevel);
                }
                if (!string.IsNullOrEmpty(query.searchText))
                    securities = securities.Where(x => x.strName.Contains(query.searchText));
                if (query.hideDisqualified) securities = securities.Where(x => x.idSecurityRank != 3);
                securities = securities.OrderBy($"{query.field} {query.direction}");
                vm.NumOfRecords = securities.Count();
                vm.Securities = AutoMapper.Mapper.Map<List<Models.dbo.Security>>(securities.Skip(startRow).Take(query.pageSize));

                for (int iSecs = 0; iSecs < vm.Securities.Count; iSecs++)
                {  // Fixes Risk / return values to yearly
                    vm.Securities[iSecs].AvgYield *= 52;
                    vm.Securities[iSecs].AvgYieldNIS *= 52;
                    vm.Securities[iSecs].StdYield *= Math.Sqrt(52);
                    vm.Securities[iSecs].StdYieldNIS *= Math.Sqrt(52);
                }
            });
            return vm;
        }
        public int GetSecurityInMemoryCount()
        {
            var secs = StaticData<cSecurity, ISecurities>.lst;
            return secs.Count;
        }

        public List<GeneralItem> GetBenchmarkSecurities()
        {
            var secs = StaticData<cSecurity, ISecurities>.BenchMarks;
            return secs.Securities.Select(x => new GeneralItem { ID = x.Properties.PortSecurityId, Name = x.Properties.SecurityName }).ToList();
        }
        public void removeInactiveSecurities(ISecurities cDisabled)
        { // Removes the inactive securities after the calculation (after they have been filtered)
            
            for (int iSecs = 0; iSecs < cDisabled.Count; iSecs++)
            { // Removes disabled secs from collection
                //iSecPos = m_colSecurities.Select
                //if (iSecPos >= 0) { this[iSecPos].setSecurityActivity(false); this.RemoveAt(iSecPos); }
                var sec = m_colSecurities.Where(x => x.Properties.PortSecurityId == cDisabled[iSecs].Properties.PortSecurityId).FirstOrDefault();
                if (sec != null) { sec.setSecurityActivity(false); m_colSecurities.Remove(sec); } //LR
            }
        }//removeInactiveSecurities

        private void setUniqueSecName(ISecurity NewSecurity)
        { // Sets the given security with a unique name (no repetition)
            int iSecPos = getSecurityIndByName(NewSecurity.Properties.SecurityName, 0);
            if (iSecPos > -1)
            {
                NewSecurity.Properties.SecurityName = NewSecurity.Properties.SecurityName.Replace(" (" + NewSecurity.Properties.SecuritySymbol + ")", "");
                NewSecurity.Properties.SecurityName += " (" + NewSecurity.Properties.SecuritySymbol + ")";
            }
        }//setUniqueSecName

        public void sortSecurities()
        { m_colSecurities = m_colSecurities.OrderBy(x => x.Properties.PortSecurityId).ToList(); }//sortSecurities

        #endregion General methods

        #region Locate security

        private int searchSecurity(String strSecId)
        { // Searches for security by its ID
            m_objTempSec = new cSecurity(m_objPortfolio, "Blah", "BL");
            m_objTempSec.Properties.PortSecurityId = strSecId;
            return searchSecurity(m_objTempSec);
        }//searchSecurity

        public int searchSecurity(ISecurity cCurrSec)
        {
            // search security
            int iPos = m_colSecurities.BinarySearch(cCurrSec, m_objSecsComparer);

            return iPos;
        }//searchSecurity

        private int getSecurityIndByName(String sSecName, int iStartPos)
        { // Returns the security's index by the given name

            for (int iSecs = iStartPos; iSecs < this.Count; iSecs++)
                if (this[iSecs].Properties.SecurityDisplay == sSecName)
                    return iSecs;
            return -1;
        }//getSecurityIndByName

        public ISecurity getSecurityById(String iSecId)
        { // Returns the security class of the given ID
            int iSecPos = searchSecurity(iSecId);
            if (iSecPos <= -1) return null;
            return this[iSecPos];
        }//getSecurityById

        public ISecurity getSecurityByIdNormalSearch(String iSecId)
        { // Returns the security class of the given ID
            for (int iSecs = 0; iSecs < this.Count; iSecs++)
                if (this[iSecs].Properties.PortSecurityId == iSecId)
                    return this[iSecs];
            return null;
        }//getSecurityByIdNormalSearch

        public int getIndSecurityById(String iSecId)
        { return searchSecurity(iSecId); }//getSecurityById

        #endregion Locate security

        #endregion Methods

        #region Collection methods

        public virtual void Add(ISecurity NewSecurity)
        { // Adds security to collection
            //m_iNameIndicator = 0;
            setUniqueSecName(NewSecurity);
            m_colSecurities.Add(NewSecurity);
        }//Add

        public virtual ISecurity this[int Index]
        { //return the Security at IList[Index]
            get { return (ISecurity)m_colSecurities[Index]; }
        }//this[int Index]

        public void RemoveAt(int iPos)
        { m_colSecurities.RemoveAt(iPos); }//RemoveAt

        public virtual int Count
        { get { return m_colSecurities.Count; } }//Count

        public virtual void Clear()
        { m_colSecurities.Clear(); }//Clear

        public void Dispose()
        {
            Resolver.Release(repository);
        }

        #endregion Collection methods

        public List<ISecurity> Securities { get { return m_colSecurities; } set { m_colSecurities = value; } }
    }//of class
}
