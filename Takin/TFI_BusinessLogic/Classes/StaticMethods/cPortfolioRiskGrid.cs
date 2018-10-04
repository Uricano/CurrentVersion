using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Data;

// Used namespaces
using Cherries.TFI.BusinessLogic.General;
using Cherries.TFI.BusinessLogic.DataManagement;
using static Cherries.Models.App.PortRiskItem;
using TFI.BusinessLogic.Enums;

namespace Cherries.TFI.BusinessLogic.StaticMethods
{
    
    public class cPortfolioRiskGrid
    {

        #region Data Members

        public List<cPortRiskItem> PortRisks = new List<cPortRiskItem>(); // Collection of all portfolio risks

        #endregion Data Members

        #region Relevant classes
        

        #endregion Relevant classes

        #region Methods

        #region Main methods

        public void populateInitialCollectionValues()
        { // Creates collection of portfolio risks with the default values
            PortRisks.Clear();
            PortRisks.Add(new cPortRiskItem(0, "None", Color.Black, -10F, 0F, -10D, 0D, enumRiskLevel.None));
            PortRisks.Add(new cPortRiskItem(1, "Solid", Color.LawnGreen, 0F, 0.09F, 0D, 0.10D, enumRiskLevel.Solid));
            PortRisks.Add(new cPortRiskItem(2, "Low", Color.Yellow, 0.09F, 0.14F, 0.10D, 0.16D, enumRiskLevel.Low));
            PortRisks.Add(new cPortRiskItem(3, "Moderate", Color.FromArgb(240, 187, 2), 0.14F, 0.25F, 0.16D, 0.25D, enumRiskLevel.Moderate));
            PortRisks.Add(new cPortRiskItem(4, "High", Color.DarkOrange, 0.25F, 0.4F, 0.25D, 0.67D, enumRiskLevel.High));
            PortRisks.Add(new cPortRiskItem(5, "Very High", Color.Red, 0.4F, 1F, 0.67D, 10D, enumRiskLevel.VeryHigh));
        }//populateInitialCollectionValues

        //public void populateGridLookupPortRisks(double dStartRisk, cErrorHandler cErrors)
        //{ // Populates the Grid lookup editor with the portfolio risk levels
        //    try
        //    {
        //        DataTable dtFinal = getGridTablePortRisksStruct();
        //        dtFinal.Rows.Add(PortRisks[0].Position, PortRisks[0].Name, PortRisks[0].LowerBound, PortRisks[0].UpperBound);
        //        dtFinal.Rows.Add(PortRisks[1].Position, PortRisks[1].Name, PortRisks[1].LowerBound, PortRisks[1].UpperBound);
        //        dtFinal.Rows.Add(PortRisks[2].Position, PortRisks[2].Name, PortRisks[2].LowerBound, PortRisks[2].UpperBound);
        //        dtFinal.Rows.Add(PortRisks[3].Position, PortRisks[3].Name, PortRisks[3].LowerBound, PortRisks[3].UpperBound);
        //        dtFinal.Rows.Add(PortRisks[4].Position, PortRisks[4].Name, PortRisks[4].LowerBound, PortRisks[4].UpperBound);

                
        //    } catch (Exception ex) {
        //        cErrors.LogInfo(ex);
        //    }
        //}//populateGridLookupPortRisks

        //public static double getPortfolioStDevValue(double dRiskLevel)
        //{ // Receives the Standard-deviation value for the given risk level
        //    if (PortRisks.Count == 0) return 0.15D;
        //    dRiskLevel -= 0.0001D;
        //    for (int iPorts = 0; iPorts < PortRisks.Count; iPorts++)
        //        if (PortRisks[iPorts].UpperBound >= dRiskLevel)
        //            return PortRisks[iPorts].stDevUpperBound;
        //    return PortRisks[0].stDevUpperBound;
        //}//getPortfolioStDevValue

        //private static DataTable getGridTablePortRisksStruct()
        //{ // Gets structure of table for portfolio risks
        //    DataTable dtFinal = new DataTable("tbl_PortRisks");
        //    dtFinal.Columns.Add(new DataColumn("iColorImage", typeof(int)));
        //    dtFinal.Columns.Add(new DataColumn("strName", typeof(String)));
        //    dtFinal.Columns.Add(new DataColumn("dRiskFrom", typeof(float)));
        //    dtFinal.Columns.Add(new DataColumn("dRiskTo", typeof(float)));
        //    return dtFinal;
        //}//getGridTableStruct

        //public static double getGridLookupPortRiskLevel(GridView ctlGridView, GridLookUpEdit ctlGrid)
        //{ // Sets the user's prefered risk level
        //    if (ctlGridView.FocusedRowHandle < 0) return 0D; // No focused row

        //    ctlGrid.BackColor = PortRisks[ctlGridView.FocusedRowHandle].Color;
        //    if (ctlGridView.FocusedRowHandle < 0) return PortRisks[0].UpperBound;
        //    return PortRisks[ctlGridView.FocusedRowHandle].UpperBound;
        //}//selPortRiskLevel

        //public static void savePortfolioRiskLevelToDB(double dRiskValue, int idPortfolio, cDbOleConnection connection)
        //{ cDbOleConnection.executeSqlSatatement(cSqlStatements.updtPortfolioRiskLevel(dRiskValue, idPortfolio), connection.dbConnection); }//savePortfolioRiskLevelToDB

        //public static void savePortfolioRiskAndEquityToDB(double dRiskValue, double dEquity, int iCalcPref, int idPortfolio, cDbOleConnection connection)
        //{ cDbOleConnection.executeSqlSatatement(cSqlStatements.updtPortfolioRisk_Equity_AndPreference(dRiskValue, dEquity, iCalcPref, idPortfolio), connection.dbConnection); }//savePortfolioRiskAndEquityToDB

        public enumRiskLevel getRiskType(double dRiskVal)
        { // Returns the risk type for the current risk level
            for (int iRisks = 0; iRisks < PortRisks.Count; iRisks++)
                if ((dRiskVal >= PortRisks[iRisks].LowerBound) && (dRiskVal < PortRisks[iRisks].UpperBound))
                    return PortRisks[iRisks].RiskType;
            return enumRiskLevel.None;
        }//getRiskType

        public cPortRiskItem getPortRiskItem(double dRiskVal)
        { // Returns the risk type for the current risk level
            dRiskVal -= 0.001;
            for (int iRisks = 0; iRisks < PortRisks.Count; iRisks++)
                if ((dRiskVal >= PortRisks[iRisks].LowerBound) && (dRiskVal < PortRisks[iRisks].UpperBound))
                    return PortRisks[iRisks];
            return PortRisks[0];
        }//getPortRiskItem

        #endregion Main methods

        #region Appearance methods
        


        #endregion Appearance methods

        #endregion Methods

    }//Main class
}
