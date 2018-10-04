using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

// Used namespaces
using Cherries.TFI.BusinessLogic.General;
using Cherries.TFI.BusinessLogic.Securities;
using TFI.BusinessLogic.Interfaces;

namespace Cherries.TFI.BusinessLogic.DataManagement.StaticMethods
{
    public static class cQaMethods
    {

        public static void setQaDataTable()
        { // Sets the Qa Datatable to be ready for use
            if (cProperties.DataForQa != null)
            { cProperties.DataForQa.Clear(); return; }

            cProperties.DataForQa = new DataTable("tbl_DataForQa");
            cProperties.DataForQa.Columns.Add(new DataColumn("idSecurity", typeof(String)));
            cProperties.DataForQa.Columns.Add(new DataColumn("strSymbol", typeof(String)));
            cProperties.DataForQa.Columns.Add(new DataColumn("dDate", typeof(DateTime)));
            cProperties.DataForQa.Columns.Add(new DataColumn("dPriceClose", typeof(double)));
            cProperties.DataForQa.Columns.Add(new DataColumn("dPriceVal", typeof(double)));
            cProperties.DataForQa.Columns.Add(new DataColumn("FAC", typeof(double)));
            cProperties.DataForQa.Columns.Add(new DataColumn("FacAcc", typeof(double)));
            cProperties.DataForQa.Columns.Add(new DataColumn("dReturn", typeof(double)));
            cProperties.DataForQa.Columns.Add(new DataColumn("dReturnMinusAvg", typeof(double)));
            cProperties.DataForQa.Columns.Add(new DataColumn("dVariance", typeof(double)));
        }//setQaDataTable

        public static void writeQaDataToCsv(ISecurities cSecsCol)
        { // Writes all QA data to Csv file
            if (cProperties.DataForQa.Rows.Count == 0) return; // No Data collected

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Security ID, Symbol, Date, Close Price, Adjusted Price, FAC, FAC Accumulated, Return, R - AVG(R), (R - AVG(R))2");

            ISecurity cCurrSec;
            String strCurrId = cProperties.DataForQa.Rows[0]["idSecurity"].ToString(), strNextId = "";
            for (int iRows = 0; iRows < cProperties.DataForQa.Rows.Count; iRows++) 
            {
                strNextId = cProperties.DataForQa.Rows[iRows]["idSecurity"].ToString();
                if (strCurrId != strNextId)
                { // New security
                    cCurrSec = cSecsCol.getSecurityById(strCurrId);
                    if (cCurrSec != null)
                        writeSecuritySummary(sb, cCurrSec);
                    strCurrId = strNextId;
                }

                IEnumerable<string> fields = cProperties.DataForQa.Rows[iRows].ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(",", fields));
            }

            File.WriteAllText(cProperties.DataFolder + "\\" + cGeneralFunctions.getDateFormatStr(DateTime.Today,"_") + "_Rate_Risk_Calcs.csv", sb.ToString());
        }//writeQaDataToCsv

        private static void writeSecuritySummary(StringBuilder sb, ISecurity cCurrSec)
        { // Writes the totals for the current security
            sb.AppendLine(",,,,Average,, Sum, Variance, St Dev");
            sb.AppendLine(",,,Totals, " + cCurrSec.RatesClass.ExpectedReturnAvg + ",, " + cCurrSec.CovarClass.SumRates + ", " + cCurrSec.CovarClass.Variance + ", " + cCurrSec.CovarClass.StDevWeekly);
            sb.AppendLine(",,,Annualized, " + cCurrSec.RatesClass.FinalRate + ",,,, " +  cCurrSec.CovarClass.StandardDeviation);
            sb.AppendLine("");
            sb.AppendLine("");
            sb.AppendLine("");
            sb.AppendLine("");
            sb.AppendLine("");
        }//writeSecuritySummary

    }//of class
}
