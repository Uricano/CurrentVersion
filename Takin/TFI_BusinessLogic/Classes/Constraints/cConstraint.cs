using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

// Used namespaces
using Cherries.TFI.BusinessLogic.DataManagement;
using TFI.BusinessLogic.Enums;
using Cherries.TFI.BusinessLogic.Constraints;

namespace Cherries.TFI.BusinessLogic.Constraints
{

    #region cConstraint class

    public class cConstraint 
    {

        #region Data members

        // Main variables
        private List<String> m_lstConstraintItems; // List of ids of items in constraint (securities)
        private String m_strDisplayText; // Final display text (of items)
        private double m_dMin = double.MinValue, m_dMax = double.MaxValue, m_dEqual = double.MaxValue; // Equality values
        
        // Constraint properties
        private enumConstraintType m_enumConstType; // Constraint type
        private Boolean m_isSingelSec = false; // Whether constraint operates on a single security
        private Boolean m_isEqualityConst = false; // Whether constraint is an equality constraint

        // Constants
        private const double m_cnstSlackVariable = 0.000001D; // Slack variable (if needed)

        #endregion Data members

        #region Consturctors, Initialization & Destructor

        public cConstraint() { }

        public cConstraint(List<String> lstItems, String sDispText, enumConstraintType eConstType, double dEqual)
        {
            m_dEqual = dEqual;
            if (!setIsEqualToZero(dEqual)) m_isEqualityConst = true; // if equal to zero - becomes range constraint
            setBasicVars(lstItems, sDispText, eConstType);
        }//constructor
        public cConstraint(List<String> lstItems, String sDispText, enumConstraintType eConstType, double dMin, double dMax)
        {
            m_dMin = dMin;
            m_dMax = dMax;
            if ((m_dMin == 0D) && (m_dMax == 0D)) setIsEqualToZero(0D);
            setBasicVars(lstItems, sDispText, eConstType);
        }//constructor

        private void setBasicVars(List<String> lstItems, String sDispText, enumConstraintType eConstType)
        { // constraint initializator
            m_lstConstraintItems = lstItems;
            m_strDisplayText = sDispText;
            m_enumConstType = eConstType;

            m_isSingelSec = getIsSingleSecurityConst();
        }//setBasicVars

        private Boolean setIsEqualToZero(double dEquality)
        { // Checks if constraint is equal to zero - if so, make it range constraint with slack variable
            if (dEquality != 0D) return false;
            m_dEqual = double.MaxValue;
            m_dMin = 0D;
            m_dMax = 0D + m_cnstSlackVariable;
            return true;
        }//setIsEqualToZero

        #endregion Consturctors, Initialization & Destructor

        #region Methods

        #region Initialization

        private Boolean getIsSingleSecurityConst()
        { // returns whether constraint operates on a single security
            if ((m_lstConstraintItems.Count == 1) && (m_enumConstType == enumConstraintType.Security)
                && (m_dEqual == double.MaxValue)) 
                return true;
            return false;
        }//getIsSingleSecurityConst

        public void setSingleSecConst()
        { m_isSingelSec = getIsSingleSecurityConst(); }//setSingleSecConst

        #endregion Initialization

        #region General methods

        public Boolean isEqual(cConstraint cConst)
        { // Checks if current constraint equals the constraint in the parameter
            if (this.m_dEqual != cConst.Equality) return false;
            if ((this.m_dMin != cConst.Minimum) || (this.m_dMax != cConst.Maximum)) return false;
            return isConstItemsMatch(cConst);
        }//isEqual

        private Boolean isConstItemsMatch(cConstraint cConst)
        { // Compares two constraints and checks whether their inner items match
            if (m_lstConstraintItems.Count != cConst.ItemIds.Count) return false;
            for (int iItems = 0; iItems < m_lstConstraintItems.Count; iItems++)
                if (m_lstConstraintItems[iItems] != cConst.ItemIds[iItems]) return false;
            return true;
        }//isConstItemsMatch

        #endregion General methods

        #endregion Methods

        #region Properties

        public List<String> ItemIds
        { get { return m_lstConstraintItems; } }//ItemIds

        public String DisplayText
        {
            get { return m_strDisplayText; }
            set { m_strDisplayText = value; }
        }//DisplayText

        public double Equality
        { get { return m_dEqual; } }//Equality

        public double Minimum
        { get { return m_dMin; } }//Minimum

        public double Maximum
        { get { return m_dMax; } }//Maximum

        public Boolean isOneSec
        { get { return m_isSingelSec; } }//isOneSec

        public Boolean isEquality
        { get { return m_isEqualityConst; } }//isEquality

        public enumConstraintType ConstraintType
        { get { return m_enumConstType; } }//ConstraintType

        #endregion Properties

    }//of class

    #endregion cConstraints class

    #region cConstraints collection class

    public class cConstraints : CollectionBase
    {

        #region Consturctors, Initialization & Destructor

        public cConstraints() { }

        #endregion Consturctors, Initialization & Destructor

        #region Collection methods

        public virtual void Add(cConstraint NewConstraint)
        { // Adds a constraint to collection
            this.List.Add(NewConstraint);
        }//Add

        public virtual cConstraint this[int Index]
        { //return the constraint at IList[Index]
            get { return (cConstraint)this.List[Index]; }
        }//this[int Index]

        #endregion Collection methods

    }//of class

    #endregion cConstraints collection class

}//of namespace
