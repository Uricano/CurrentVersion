using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Cherries.TFI.BusinessLogic.DataManagement.DataTypes
{

    public class cDateRange
    {
        #region Data members

        // Main variables
        private DateTime m_dtStartDate; // Starting date
        private DateTime m_dtEndDate; // Ending date

        #endregion Data members

        #region Constructors, Initialization & Destructor

        public cDateRange() { }

        public cDateRange(DateTime dtSDate, DateTime dtEDate)
        {
            m_dtStartDate = dtSDate.Date;
            m_dtEndDate = dtEDate.Date;
        }//constructor

        #endregion Constructors, Initialization & Destructor

        #region Methods

        #region General operations

        public TimeSpan getDateRangeDiff()
        { return m_dtEndDate.Subtract(m_dtStartDate); }//getDateRangeDiff

        public Boolean isWithinRange(DateTime dtCompare)
        { // Checks whether the given date is within the current cDateRange class
            if (dtCompare < m_dtStartDate) return false;
            if (dtCompare > m_dtEndDate) return false;
            return true;
        }//isWithinRange

        #endregion General operations

        #endregion Methods

        #region Properties

        public DateTime StartDate
        {
            get { return m_dtStartDate; }
            set { m_dtStartDate = value; }
        }//StartDate

        public DateTime EndDate
        {
            get { return m_dtEndDate; }
            set { m_dtEndDate = value; }
        }//EndDate

        #endregion Properties

    }// of class
}
