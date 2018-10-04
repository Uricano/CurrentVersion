using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.TFI.BusinessLogic.Constraints
{
    public class cConstRecord
    {

        #region Data members

        // Data variables
        private String m_strCategoryName; // Name of node
        private int m_iIdentifier; // ID
        private int m_iParentId; // Parent ID
        private int m_iLevel; // Level in tree
        private int m_iDtPos; // Position in Datatable containing records
        private int m_iRecordsLen = 1; // Length of similar records
        private Boolean m_isSelected = false; // Whether the node is selected

        #endregion Data members

        #region Constructors, Initialization & Destructor

        public cConstRecord(string name, int id, int parentID, int iLevel, int iDtPos)
        {
            m_strCategoryName = name;
            m_iIdentifier = id;
            m_iParentId = parentID;
            m_iLevel = iLevel;
            m_iDtPos = iDtPos;
		}//constructor

        #endregion Constructors, Initialization & Destructor

        #region Properties

        public int ID
        { get { return m_iIdentifier; } }//ID

        public int ParentID
        { get { return m_iParentId; } }//ParentID

        public string Category
        { get { return m_strCategoryName; } }//Category

        public int Level
        { get { return m_iLevel; } }//Level

        public int DtPosition
        { get { return m_iDtPos; } }//DtPosition

        public int RecLength
        {
            get { return m_iRecordsLen; }
            set { m_iRecordsLen = value; }
        }//RecLength

        public Boolean Checked
        {
            get { return m_isSelected; }
            set { m_isSelected = value; }
        }//Checked

        #endregion Properties

    }//of class
}
