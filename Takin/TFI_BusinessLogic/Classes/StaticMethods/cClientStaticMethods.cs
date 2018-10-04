using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

// Used namespaces
using Cherries.TFI.BusinessLogic.General;
using Cherries.TFI.BusinessLogic.Collections;
using Cherries.TFI.BusinessLogic.Securities;
using Cherries.TFI.BusinessLogic.Portfolio;
using Cherries.TFI.BusinessLogic.DataManagement.DataTypes;
using Cherries.TFI.BusinessLogic.Protection.LicenseManagement;
using Cherries.TFI.BusinessLogic.GMath.StaticMethods;
using Cherries.Models.dbo;
using System.IO;

namespace Cherries.TFI.BusinessLogic.DataManagement.StaticMethods
{
    public static class cClientStaticMethods
    {
         #region dValue Calculations

       
        public static void CalculateYieldsSTDandAVG(DataTable dtSecsData)
        {   // There is no need for calculations of dValue and Weight fields, 
            // just adjust stdYield and avgYield to display annual value (not weekly),

            DataRow dr;

            if (!dtSecsData.Columns.Contains("calcDValue"))
            {
                // For USD calculations
                //dtSecsData.Columns.Add("calcDValue", typeof(Double));
                //dtSecsData.Columns.Add("calcSecWeight", typeof(Double));
                dtSecsData.Columns.Add("calcAvgYield", typeof(Double));
                dtSecsData.Columns.Add("calcStdYield", typeof(Double));

                // For Israeli shekel calculation
                //dtSecsData.Columns.Add("calcDValueNIS", typeof(Double));
                //dtSecsData.Columns.Add("calcSecWeightNIS", typeof(Double));
                dtSecsData.Columns.Add("calcAvgYieldNIS", typeof(Double));
                dtSecsData.Columns.Add("calcStdYieldNIS", typeof(Double));
            }

            for (int i = 0; i < dtSecsData.Rows.Count; i++)
            {
                dr = dtSecsData.Rows[i];

                if (dr["AvgYield"] is DBNull)
                    dr["AvgYield"] = 0;
                if (dr["StdYield"] is DBNull)
                    dr["StdYield"] = 0;
                if (dr["AvgYieldNIS"] is DBNull)
                    dr["AvgYieldNIS"] = 0;
                if (dr["StdYieldNIS"] is DBNull)
                    dr["StdYieldNIS"] = 0;

                dr["calcAvgYield"] = Convert.ToDouble(dr["AvgYield"]) * 52 * 100;       // to display in percents we multiply by 100
                dr["calcStdYield"] = Convert.ToDouble(dr["StdYield"]) * Math.Sqrt(52);
                dr["calcAvgYieldNIS"] = Convert.ToDouble(dr["AvgYieldNIS"]) * 52 * 100; // to display in percents we multiply by 100
                dr["calcStdYieldNIS"] = Convert.ToDouble(dr["StdYieldNIS"]) * Math.Sqrt(52);

                //// Calculate Monetary in USD and Shekels and assign to separate calculated fields

                //// Calculate monetary in USD  (assign it to calcDValue - it is always in dollars)
                //dr["calcDValue"] = Convert.ToDouble(dr["dValue"]);
                //if (dr["idCurrency"].ToString() == "ILS")
                //    dr["calcDValue"] = Convert.ToDouble(dr["dValue"]) / (cProperties.LastDollarVal / 100.0);

                //// Calculate monetary in Shekels  (assign it to calcDValueNIS - it is always in Shekels)
                //dr["calcDValueNIS"] = Convert.ToDouble(dr["dValue"]);
                //if (dr["idCurrency"].ToString() == "9001")
                //    dr["calcDValueNIS"] = Convert.ToDouble(dr["dValue"]) * (cProperties.LastDollarVal / 100.0);

                //// Calculate Weight

                //// For USD portfolio
                //if (Convert.ToDouble(dr["calcStdYield"]) == 0)
                //    dr["calcSecWeight"] = 0;
                //else
                //    dr["calcSecWeight"] = Convert.ToDouble(dr["calcDValue"]) * Convert.ToDouble(dr["calcAvgYield"]) / Convert.ToDouble(dr["calcStdYield"]);

                //// For ILS - Shekels portfolio
                //if (Convert.ToDouble(dr["calcStdYieldNIS"]) == 0)
                //    dr["calcSecWeightNIS"] = 0;
                //else
                //    dr["calcSecWeightNIS"] = Convert.ToDouble(dr["calcDValueNIS"]) * Convert.ToDouble(dr["calcAvgYieldNIS"]) / Convert.ToDouble(dr["calcStdYieldNIS"]);
            }

        }//CalculateMonetaryAndSecWeight


        #endregion dValue Calculations

        #region Datatable operations

        public static void writeDataTableToCsv(DataTable sourceTable, String strFileName, bool includeHeaders)
        { // Writes datatable (of covariance matrix) to csv flie
            StreamWriter writer = new StreamWriter(strFileName);
            if (includeHeaders)
            { // Writes headers
                IEnumerable<String> headerValues = sourceTable.Columns
                    .OfType<DataColumn>()
                    .Select(column => quoteValue(column.ColumnName));

                writer.WriteLine(String.Join(",", headerValues));
            }

            IEnumerable<String> items = null;

            foreach (DataRow row in sourceTable.Rows)
            { // Writes data
                items = row.ItemArray.Select(o => quoteValue(o?.ToString() ?? String.Empty));
                writer.WriteLine(String.Join(",", items));
            }

            writer.Flush();
        }//writeDataTableToCsv

        private static string quoteValue(string value)
        {
            return String.Concat("\"",
            value.Replace("\"", "\"\""), "\"");
        }//quoteValue

        #endregion Datatable operations

        #region Z Value

        public static double NormalRisk(double dRiskVal)
        {
            double dZValue = cClientStaticMethods.StdNormal_CDF(dRiskVal);
            return (2 * dZValue - 1);
        }//NormalRisk

        public static double StdNormal_CDF(double u)
        { // Calculates Z-Value (zone below curve - in Normal distribution)
            double outValue;

            // type doubleArray is varray(9) of float;

            double[] a = {
							1.161110663653770e-002,3.951404679838207e-001,2.846603853776254e+001,
							1.887426188426510e+002,3.209377589138469e+003
							};

            double[] b = {
							1.767766952966369e-001,8.344316438579620e+000,1.725514762600375e+002,
							1.813893686502485e+003,8.044716608901563e+003
							};

            double[] c = {
							2.15311535474403846e-8,5.64188496988670089e-1,8.88314979438837594e00,
							6.61191906371416295e01,2.98635138197400131e02,8.81952221241769090e02,
							1.71204761263407058e03,2.05107837782607147e03,1.23033935479799725E03
							};

            double[] d = {
							1.00000000000000000e00,1.57449261107098347e01,1.17693950891312499e02,
							5.37181101862009858e02,1.62138957456669019e03,3.29079923573345963e03,
							4.36261909014324716e03,3.43936767414372164e03,1.23033935480374942e03
							};

            double[] p = {
							1.63153871373020978e-2,3.05326634961232344e-1,3.60344899949804439e-1,
							1.25781726111229246e-1,1.60837851487422766e-2,6.58749161529837803e-4
							};

            double[] q = {
							1.00000000000000000e00,2.56852019228982242e00,1.87295284992346047e00,
							5.27905102951428412e-1,6.05183413124413191e-2,2.33520497626869185e-3
							};

            double z;
            double y = Math.Abs(u);


            if (y <= 0.46875 * Math.Sqrt(2))
            {
                /* evaluate erf() for |u| <= sqrt(2)*0.46875 */
                z = y * y;
                y = u * ((((a[0] * z + a[1]) * z + a[2]) * z + a[3]) * z + a[4])
                / ((((b[0] * z + b[1]) * z + b[2]) * z + b[3]) * z + b[4]);

                return 0.5 + y;
            }

            z = Math.Exp(-y * y / 2) / 2;
            if (y <= 4.0 * Math.Sqrt(2))
            {
                /* evaluate erfc() for sqrt(2)*0.46875 <= |u| <= sqrt(2)*4.0 */
                y = y / Math.Sqrt(2);
                y =
                ((((((((c[0] * y + c[1]) * y + c[2]) * y + c[3]) * y + c[4]) * y + c[5]) * y + c[6]) * y + c[7]) * y + c[8])
                / ((((((((d[0] * y + d[1]) * y + d[2]) * y + d[3]) * y + d[4]) * y + d[5]) * y + d[6]) * y + d[7]) * y + d[8]);
                y = z * y;
            } else { /* evaluate erfc() for |u| > sqrt(2)*4.0 */
                z = z * Math.Sqrt(2) / y;
                y = 2 / (y * y);
                y = y * (((((p[0] * y + p[1]) * y + p[2]) * y + p[3]) * y + p[4]) * y + p[5])
                / (((((q[0] * y + q[1]) * y + q[2]) * y + q[3]) * y + q[4]) * y + q[5]);
                y = z * (1 / Math.Sqrt(Math.PI) - y); // 3.141592654 means PI
            }

            if (u < 0)
                outValue = y;
            else
                outValue = 1 - y;

            return outValue;
        }//StdNormal_CDF

        public static double StdNormal_Inv(double p)
        {
            double u;
            double q;
            double t;

            double[] a = {
							-3.969683028665376e+01,  2.209460984245205e+02,
							-2.759285104469687e+02,  1.383577518672690e+02,
							-3.066479806614716e+01,  2.506628277459239e+00
							};

            double[] b = {
							-5.447609879822406e+01,  1.615858368580409e+02,
							-1.556989798598866e+02,  6.680131188771972e+01,
							-1.328068155288572e+01
							};

            double[] c = {
							-7.784894002430293e-03, -3.223964580411365e-01,
							-2.400758277161838e+00, -2.549732539343734e+00,
							4.374664141464968e+00,  2.938163982698783e+00
							};

            double[] d = {
							7.784695709041462e-03,  3.224671290700398e-01,
							2.445134137142996e+00,  3.754408661907416e+00
							};


            if (p == 0.0)
                return -10000;

            if (p == 1.0)
                return 10000;

            q = Math.Min(p, 1 - p);

            if (q > 0.02425)
            {
                /* Rational approximation for central region. */
                u = q - 0.5;
                t = u * u;
                u = u * (((((a[0] * t + a[1]) * t + a[2]) * t + a[3]) * t + a[4]) * t + a[5])
                / (((((b[0] * t + b[1]) * t + b[2]) * t + b[3]) * t + b[4]) * t + 1);
            }
            else
            {
                /* Rational approximation for both tail regions. */
                t = Math.Sqrt(-2 * Math.Log(q));
                u = (((((c[0] * t + c[1]) * t + c[2]) * t + c[3]) * t + c[4]) * t + c[5])
                / ((((d[0] * t + d[1]) * t + d[2]) * t + d[3]) * t + 1);
            }

            /* The relative error of the approximation has absolute value less
            than 1.15e-9.  One iteration of Halley's rational method (third
            order) gives full machine precision... */
            t = StdNormal_CDF(u);
            t = t - q;    /* error */
            t = t * Math.Sqrt(2 * Math.PI) * Math.Exp(u * u / 2);   /* f(u)/df(u) */
            u = u - t / (1 + u * t / 2);     /* Halley's method */

            if (p > 0.5)
                u = -u;

            return u;
        }//StdNormal_Inv

        #endregion Z Value

        //public static List<T> ConvertDataTableToList<T>(DataTable dt)
        //{
        //    var ret = AutoMapper.Mapper.DynamicMap<IDataReader, List<T>>(dt.CreateDataReader());
        //    return ret;
        //}

    }//of class
}
