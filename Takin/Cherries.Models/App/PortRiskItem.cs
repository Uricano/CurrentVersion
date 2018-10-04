using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using TFI.BusinessLogic.Enums;

namespace Cherries.Models.App
{
    public class PortRiskItem
    {
        public class cPortRiskItem
        {

            #region Constructor

            public cPortRiskItem(int iPos, String strName, Color col, double lower, double upper, double stDevLower, double stDevUpper, enumRiskLevel eType)
            {
                m_iItemPos = iPos;
                m_strItemName = strName;
                m_objColor = col;
                m_enumType = eType;
                m_dStartVal = lower;
                m_dEndVal = upper;
                m_dStDevStart = stDevLower;
                m_dStDevEnd = stDevUpper;
            }//constructor

            #endregion Constructor

            #region Data Members

            private int m_iItemPos = 0; // Position of item in collection
            private string m_strItemName = ""; // Name of item (portfolio risk)
            private Color m_objColor = new Color(); // Color of portfolio risk item
            private enumRiskLevel m_enumType = enumRiskLevel.None; // Risk level type

            private double m_dStartVal = 0D; // Starting Prefered risk level (Lower bound)
            private double m_dEndVal = 0D; // Ending Prefered risk level (Upper bound)

            private double m_dStDevStart = 0D; // Starting Standard-deviation level (for security optimization)
            private double m_dStDevEnd = 0D; // Ending Standard-deviation level (for security optimization)

            #endregion Data Members

            #region Properties

            public int Position
            {
                get { return m_iItemPos; }
                set { m_iItemPos = value; }
            }//Position

            public string Name
            {
                get { return m_strItemName; }
                set { m_strItemName = value; }
            }//Name

            public Color Color
            {
                get { return m_objColor; }
                set { m_objColor = value; }
            }//Color

            public enumRiskLevel RiskType
            { get { return m_enumType; } }//RiskType

            public double LowerBound
            {
                get { return m_dStartVal; }
                set { m_dStartVal = value; }
            }//LowerBound

            public double UpperBound
            {
                get { return m_dEndVal; }
                set { m_dEndVal = value; }
            }//UpperBound

            public double stDevLowerBound
            {
                get { return m_dStDevStart; }
                set { m_dStDevStart = value; }
            }//stDevLowerBound

            public double stDevUpperBound
            {
                get { return m_dStDevEnd; }
                set { m_dStDevEnd = value; }
            }//stDevUpperBound

            #endregion Properties

        }//of class
    }
}
