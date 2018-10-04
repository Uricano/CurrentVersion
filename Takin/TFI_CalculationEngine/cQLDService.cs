using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Cherries.EF
{
    public class cQLDService
    {

        #region Dll
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        // [DllImport("C:\\Users\\p1122385\\Downloads\\Net Samples\\TestSciEngine_NewPlatform\\TestSciEngine")]
        [DllImport(@"TakinQLD.dll")]
        //[DllImport("TakinQLD.dll")]
        //[DllImport("C:\\AppsCode\\TestSci\\QLD\\TakinQLD.dll")]
        private static extern int QLD(double[] Q, int NMAX, double[] p, double[] C, int MMAX, double[] B,
                            double[] ci, double[] cs, int me, double[] x, double eps);

        #endregion Dll

        #region Data Members
        
        public double[,] __Q;                     // 
        public double[,] __Q1;                    // Objective function matrix which should be symmetric and positive definite

        public double[] __p;                      // 
        public double[] __R;                      // Containts the constant vector of the objective function

        public double[,] __C1;
        public double[,] __C2;
        public double[,] __C;                     //Contains the data matrix of the linear constraints

        public double[,] __W;                     //optimal solution matrix

        public double[] __B1;
        public double[] __B2;
        public double[] __B;                      //Contains the constant data of the linear constraints

        public double[] __ci;                     //column vector of lower-bounds 
        public double[] __cs;                     //column vector of upper-bounds.

        public int __me;                          //number of equality constraints
        public int __eps;                         //floatting point number, required precision.

        public int __info;                        //integer, return the execution status
                                                         //info==1 : Too many iterations needed
                                                         //info==2 : Accuracy insufficient to statisfy convergence criterion
                                                         //info==5 : Length of working array is too short
                                                         //info>=10: The constraints are inconsistent


        public double[] __x;                      //optimal solution found
        public static Process compiler;

        #endregion Data Members

        #region Methods

        public static void StartProcess(string path)
        {
            compiler = new Process();
            compiler.StartInfo.FileName = path;
            //System.IO.File.WriteAllText(fileName, Newtonsoft.Json.JsonConvert.SerializeObject(data));
            //compiler.StartInfo.Arguments = "\"" + fileName + "\"";
            compiler.StartInfo.UseShellExecute = false;
            compiler.StartInfo.RedirectStandardOutput = true;
            compiler.StartInfo.RedirectStandardError = true;
            compiler.StartInfo.RedirectStandardInput = true;
            compiler.Start();
        }

        public void Create_Q(double[,] Q)
        {//Create real positive definite symmetric matrix 
            int NMAX = Q.GetLength(0);
            __Q = new double[NMAX, NMAX];

            for (int row = 0; row < NMAX; row++)
            {
                for (int col = 0; col < NMAX; col++)
                {
                    __Q[row, col] = Q[row, col];
                }
            }//Create_Q
        }


        public void Create_R(double[] R)
        {//Create real (column) vector 
            int NMAX = R.Length;
            __R = new double[NMAX];

            for (int i = 0; i < NMAX; i++)
            {
                __R[i] = R[i];
            }
        }//Create_R


        public double[] Get_X()
        {//Get optimal solution found
            return __x;
        }//Get_X


        public int Get_Info()
        {//Get integer, return the execution status
            return __info;
        }//Get_Info

        public void multiplyRatesVector(double[] arrRates, double dCurrCoeff)
        { // Multiplies rates array
            for (int iRows = 0; iRows < __R.Length; iRows++)
                __R[iRows] = -(arrRates[iRows] * dCurrCoeff);
        }
        public string LastError()
        {//Get last error
            string error = "";

            if (__info == 0)
            {

            }
            else if (__info == 1)
            {
                error = "Too many iterations.";
            }
            else if (__info == 2)
            {
                error = "Accuracy insufficient to satisfy convergence criterion.";
            }
            else if (__info == 5)
            {
                error = "Length of working array is too short.";
            }
            else if (__info > 10)
            {
                error = "The constraints are inconsistent.";
            }
            else
            {
            }
            return error;
        }//LastError




        public void CreateEmptyMatrix_W()
        {//Create Empty Matrix W
            __W = null;
        }//CreateEmptyMatrix_W



        public double[] getMatrix_W()
        {//get Matrix W
            double[] w;
            int ind = 0;

            int rowN = __W.GetLength(0);
            int colN = __W.GetLength(1);

            w = new double[rowN * colN];


            for (int cN = 0; cN < colN; cN++)
            {
                for (int rN = 0; rN < rowN; rN++)
                {
                    w[ind] = __W[rN, cN];
                    ind++;
                }
            }

            return w;
        }//getMatrix_W




        public void Add_X_To_W()
        {//Add X To W
            if (__W == null)
            {
                int ln = __x.Length;
                __W = new double[1, ln];

                for (int i = 0; i < ln; i++)
                {
                    __W[0, i] = __x[i];
                }
            }
            else
            {
                int rowN = __W.GetLength(0);
                int colN = __W.GetLength(1);

                double[,] w = new double[rowN + 1, colN];

                for (int rN = 0; rN < rowN; rN++)
                {
                    for (int cN = 0; cN < colN; cN++)
                    {
                        w[rN, cN] = __W[rN, cN];
                    }
                }

                for (int i = 0; i < colN; i++)
                {
                    w[rowN, i] = __x[i];
                }

                __W = w;
            }
        }//Add_X_To_W




        public void multiply_matrix_coefficient(int coefficient)
        {// multiply matrix coefficient
            int rowNum = __Q.GetLength(0);
            int colNum = __Q.GetLength(1);

            __Q1 = new double[rowNum, colNum];

            for (int row = 0; row < rowNum; row++)
            {
                for (int col = 0; col < colNum; col++)
                {
                    __Q1[row, col] = __Q[row, col] * coefficient;
                }
            }
        }// multiply_matrix_coefficient




        public void minusVector()
        {//minus Vector
            int Num = __R.GetLength(0);

            __p = new double[Num];


            for (int i = 0; i < Num; i++)
            {
                __p[i] = -__R[i];
            }
        }//minusVector



        public void CreateMatrix_C1(List<List<double>> c1)
        {

        }//CreateMatrix_C1


        public void CreateMatrix_C2(List<List<double>> c2)
        {

        }//CreateMatrix_C2




        public void CreateMatrix_C()
        {//Create real matrix 
            if ((__C1 == null) && (__C2 == null))
            {
                return;
            }

            if (__C1 == null)
            {
                __C = __C2;
                return;
            }

            if (__C2 == null)
            {
                __C = __C1;
                return;
            }

            int rowN1 = __C1.GetLength(0);
            int rowN2 = __C2.GetLength(0);

            int colN = __C1.GetLength(1);

            int rowN = rowN1 + rowN2;

            __C = new double[rowN, colN];



            for (int rN = 0; rN < rowN1; rN++)
            {
                for (int cN = 0; cN < colN; cN++)
                {
                    __C[rN, cN] = __C1[rN, cN];
                }
            }


            for (int rN = 0; rN < rowN2; rN++)
            {
                for (int cN = 0; cN < colN; cN++)
                {
                    __C[rN + rowN1, cN] = __C2[rN, cN];
                }
            }

        }//CreateMatrix_C


        public void C_equal_c1_plus_c2(List<List<double>> c1, List<List<double>> c2)
        {//C = c1 + c2
            int rowN = 0;
            int colN = 0;

            __C1 = null;
            __C2 = null;

            if ((c1 != null) && (c1.Count != 0))
            {
                rowN = c1.Count;
                colN = c1[0].Count;

                __C1 = new double[rowN, colN];

                for (int rN = 0; rN < rowN; rN++)
                {
                    for (int cN = 0; cN < colN; cN++)
                    {
                        __C1[rN, cN] = (c1[rN])[cN];
                    }
                }
            }


            if ((c2 != null) && (c2.Count != 0))
            {
                rowN = c2.Count;
                colN = c2[0].Count;

                __C2 = new double[rowN, colN];

                for (int rN = 0; rN < rowN; rN++)
                {
                    for (int cN = 0; cN < colN; cN++)
                    {
                        __C2[rN, cN] = (c2[rN])[cN];
                    }
                }
            }

            CreateMatrix_C();
        }//C_equal_c1_plus_c2

        public void CreateVector_B1(double[] b1)
        {

        }//CreateVector_B1


        public void CreateVector_B2(double[] b2)
        {

        }//CreateVector_B2



        public void B_equal_b1_plus_b2(double[] b1, double[] b2)
        {//B = b1  + b2
            if ((b1 == null) && (b2 == null))
            {
                return;
            }

            if (b1 == null)
            {
                __B = b2;
                return;
            }

            if (b2 == null)
            {
                __B = b1;
                return;
            }


            int N1 = b1.Length;
            int N2 = b2.Length;


            int N = N1 + N2;

            __B = new double[N];



            for (int i = 0; i < N1; i++)
            {
                __B[i] = b1[i];
            }


            for (int i = 0; i < N2; i++)
            {
                __B[i + N1] = b2[i];
            }

        }//B_equal_b1_plus_b2


        public void CreateDoubleVector_ci(double[] ci)
        {//Create column vector of lower-bounds 
            __ci = ci;
        }//CreateDoubleVector_ci


        public void CreateDoubleVector_cs(double[] cs)
        {//Create column vector of upper-bounds
            __cs = cs;
        }//CreateDoubleVector_cs


        public void Create_ME(int me)
        {//Create number of equality constraints 
            __me = me;
        }//Create_ME



        public void QLD()
        {//Call linear quadratic programming solver
            int m;
            int n;
            int mnn;

            n = __Q.GetLength(0);
            m = __C.GetLength(0);

            mnn = m + n + n;

            __x = new double[n];

            __eps = 0;

            __info = CallMatrixQLD(__Q1, __R, __C, __B, __ci, __cs, __me, ref __x, __eps);
        }//QLD


        public static int CallMatrixQLD(double[,] Q, double[] p, double[,] C, double[] B,
                           double[] ci, double[] cs, int me, ref double[] x, double eps)
        {// Transforms matrix for call QLD
            int NMAX = Q.GetLength(0);
            int MMAX = C.GetLength(0);

            double[] vQ = new double[NMAX * NMAX];
            double[] vC = new double[NMAX * MMAX];

            for (int col = 0; col < NMAX; col++)
            {
                for (int row = 0; row < NMAX; row++)
                {
                    vQ[col * NMAX + row] = Q[row, col];
                }
            }

            for (int col = 0; col < NMAX; col++)
            {
                for (int row = 0; row < MMAX; row++)
                {
                    vC[col * MMAX + row] = C[row, col];
                }
            }

            int CompCode = QLD(vQ, NMAX, p, vC, MMAX, B, ci, cs, me, x, eps);   // Call library function
            return CompCode;
        }//CallMatrixQLD


        public static double[,] get1dTo2dArr(double[] origMatrix, Size matSize)
        { // Transforms a 1-dimensional array to a 2-dimensional matrix
            double[,] finalArr = null;
            finalArr = new double[matSize.Height, matSize.Width];
            int iPosVar = 0;
            int iPosVarCnt = 0;
            int iLen = 0;
            
                for (iLen = 0; iLen <= origMatrix.Length - 1; iLen++)
                {
                    if (iPosVarCnt == matSize.Height)
                    {
                        iPosVar += 1;
                        iPosVarCnt = 0;
                    }
                    finalArr[iPosVarCnt, iPosVar] = origMatrix[iLen];
                    iPosVarCnt += 1;
                }
                return finalArr;
            
           
        } // get1dTo2dArr


        #endregion Methods

    }//of class
}
