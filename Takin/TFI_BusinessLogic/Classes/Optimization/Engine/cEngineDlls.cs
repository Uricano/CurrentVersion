using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

// Used namespaces
using Cherries.TFI.BusinessLogic.General;

namespace DotNetScilab
{

    #region Kernel Class

    public static class NativeMethods
    {

        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);

    }//of class

    #endregion Kernel Class

    public class cEngineDlls
    { // Dynamically calls to the dlls of the math engine

        #region API structures

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public unsafe struct api_Ctx
        {
            public String pstName; /**< Function name */
        }//api_Ctx

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public unsafe struct api_Err
        {
            public int iErr;
            public int iMsgCount;
            public fixed int pstructMsg[5];
        }//api_Err

        #endregion API structures

        #region Function Variables

        // Main variables
        private SendScilabJob m_fnSciJob;
        private StartScilab m_fnSciStart;
        private TerminateScilab m_fnSciEnd;
        private DisableInteractiveMode m_fnInteractiveOff;

        // Matrix variables
        private createNamedMatrixOfDouble m_fnMatSetDbl;
        private createNamedMatrixOfString m_fnMatSetStr;
        private createNamedMatrixOfInteger32 m_fnMatSetInt;
        private readNamedMatrixOfString m_fnMatReadStr;
        private readNamedMatrixOfDouble m_fnMatReadDbl;
        private readNamedMatrixOfInteger32 m_fnMatReadInt;
        private getNamedVarDimension m_fnMatDim;

        #endregion Function Variables

        #region Set function attributes

        #region Main functions

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SendScilabJob([In]String job);//SendScilabJob

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int StartScilab([In] String SCIpath, [In] String ScilabStartup, [In] Int32[] Stacksize);//StartScilab

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int TerminateScilab([In] String ScilabQuit);//TerminateScilab

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DisableInteractiveMode();//DisableInteractiveMode

        #endregion Main functions

        #region Matrix functions

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate api_Err createNamedMatrixOfDouble([In]IntPtr pvApiCtx, [In] String _pstName, [In] int _iRows, [In] int _iCols, [In] double[] _pdblReal);//createNamedMatrixOfDouble

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate api_Err createNamedMatrixOfString([In]IntPtr pvApiCtx, [In] String _pstName, [In] int _iRows, [In] int _iCols, [In] String[] _pstStrings);//createNamedMatrixOfString

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate api_Err createNamedMatrixOfInteger32([In]IntPtr pvApiCtx, [In] String _pstName, [In] int _iRows, [In] int _iCols, [In] int[] _piData);//createNamedMatrixOfInteger32

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private unsafe delegate api_Err readNamedMatrixOfString([In]IntPtr pvApiCtx, [In] String _pstName, [Out]  Int32* _piRows, [Out]  Int32* _piCols, [In, Out] int[] _piLength, [In, Out] String[] _pstStrings);//readNamedMatrixOfString

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private unsafe delegate api_Err readNamedMatrixOfDouble([In]IntPtr pvApiCtx, [In] String _pstName, [Out] Int32* _piRows, [Out] Int32* _piCols, [In, Out] Double[] _pdblReal);//readNamedMatrixOfDouble

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private unsafe delegate api_Err readNamedMatrixOfInteger32([In]IntPtr pvApiCtx, [In] String _pstName, [Out] Int32* _piRows, [Out] Int32* _piCols, [In, Out] int[] _piData);//readNamedMatrixOfInteger32

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private unsafe delegate api_Err getNamedVarDimension([In]IntPtr pvApiCtx, [In] String _pstName, [Out] Int32* _piRows, [Out] Int32* _piCols);//getNamedVarDimension

        #endregion Matrix functions

        #endregion Set function attributes

        #region Constructors, Initialization & Destructor

        public cEngineDlls()
        {
            setMainFunctions();
            setMatrixFunctions();
        }//constructor

        #endregion Constructors, Initialization & Destructor

        #region Define functions

        private void setMainFunctions()
        { // Sets main functions (to operate engine)
            IntPtr pDll = NativeMethods.LoadLibrary(cProperties.DataFolder + "\\Engine\\bin\\call_scilab.dll"); //
            IntPtr pAddressOfFunctionToCall = NativeMethods.GetProcAddress(pDll, "SendScilabJob");//SendScilabJob
            m_fnSciJob = (SendScilabJob)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(SendScilabJob));
            pAddressOfFunctionToCall = NativeMethods.GetProcAddress(pDll, "StartScilab");//StartScilab
            m_fnSciStart = (StartScilab)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(StartScilab));
            pAddressOfFunctionToCall = NativeMethods.GetProcAddress(pDll, "TerminateScilab");//TerminateScilab
            m_fnSciEnd = (TerminateScilab)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(TerminateScilab));
            pAddressOfFunctionToCall = NativeMethods.GetProcAddress(pDll, "DisableInteractiveMode");//DisableInteractiveMode
            m_fnInteractiveOff = (DisableInteractiveMode)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(DisableInteractiveMode));
        }//setMainFunctions

        private void setMatrixFunctions()
        { // Sets the functions necessary for matrix handling
            IntPtr pDll = NativeMethods.LoadLibrary(@cProperties.DataFolder + "\\Engine\\bin\\api_scilab.dll");
            IntPtr pAddressOfFunctionToCall = NativeMethods.GetProcAddress(pDll, "createNamedMatrixOfDouble");//createNamedMatrixOfDouble
            m_fnMatSetDbl = (createNamedMatrixOfDouble)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(createNamedMatrixOfDouble));
            pAddressOfFunctionToCall = NativeMethods.GetProcAddress(pDll, "createNamedMatrixOfString");//createNamedMatrixOfString
            m_fnMatSetStr = (createNamedMatrixOfString)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(createNamedMatrixOfString));
            pAddressOfFunctionToCall = NativeMethods.GetProcAddress(pDll, "createNamedMatrixOfInteger32");//createNamedMatrixOfInteger32
            m_fnMatSetInt = (createNamedMatrixOfInteger32)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(createNamedMatrixOfInteger32));
            pAddressOfFunctionToCall = NativeMethods.GetProcAddress(pDll, "readNamedMatrixOfString");//readNamedMatrixOfString
            m_fnMatReadStr = (readNamedMatrixOfString)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(readNamedMatrixOfString));
            pAddressOfFunctionToCall = NativeMethods.GetProcAddress(pDll, "readNamedMatrixOfDouble");//readNamedMatrixOfDouble
            m_fnMatReadDbl = (readNamedMatrixOfDouble)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(readNamedMatrixOfDouble));
            pAddressOfFunctionToCall = NativeMethods.GetProcAddress(pDll, "readNamedMatrixOfInteger32");//readNamedMatrixOfInteger32
            m_fnMatReadInt = (readNamedMatrixOfInteger32)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(readNamedMatrixOfInteger32));
            pAddressOfFunctionToCall = NativeMethods.GetProcAddress(pDll, "getNamedVarDimension");//getNamedVarDimension
            m_fnMatDim = (getNamedVarDimension)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(getNamedVarDimension));
        }//setMatrixFunctions

        #endregion Define functions

        #region Function calls

        #region Main functions

        public int callEngineJob([In]String job)
        { return m_fnSciJob(job); }//callEngineJob

        public int startEngine([In] String SCIpath, [In] String ScilabStartup, [In] Int32[] Stacksize)
        { return m_fnSciStart(SCIpath, ScilabStartup, Stacksize); }//startEngine

        public int endEngine([In] String ScilabQuit)
        { return m_fnSciEnd(ScilabQuit); }//endEngine

        public void disableInteractive()
        { m_fnInteractiveOff(); }//disableInteractive

        #endregion Main functions

        #region Matrix functions

        public api_Err createDblMat([In]IntPtr pvApiCtx, [In] String _pstName, [In] int _iRows, [In] int _iCols, [In] double[] _pdblReal)
        { return m_fnMatSetDbl(pvApiCtx, _pstName, _iRows, _iCols, _pdblReal); }//createDblMat

        public api_Err createStrMat([In]IntPtr pvApiCtx, [In] String _pstName, [In] int _iRows, [In] int _iCols, [In] String[] _pstStrings)
        { return m_fnMatSetStr(pvApiCtx, _pstName, _iRows, _iCols, _pstStrings); }//createStrMat

        public api_Err createIntMat([In]IntPtr pvApiCtx, [In] String _pstName, [In] int _iRows, [In] int _iCols, [In] int[] _piData)
        { return m_fnMatSetInt(pvApiCtx, _pstName, _iRows, _iCols, _piData); }//createIntMat

        public unsafe api_Err readStrMat([In]IntPtr pvApiCtx, [In] String _pstName, [Out]  Int32* _piRows, [Out]  Int32* _piCols, [In, Out] int[] _piLength, [In, Out] String[] _pstStrings)
        { return m_fnMatReadStr(pvApiCtx, _pstName, _piRows, _piCols, _piLength, _pstStrings); }//readStrMat

        public unsafe api_Err readDblMat([In]IntPtr pvApiCtx, [In] String _pstName, [Out] Int32* _piRows, [Out] Int32* _piCols, [In, Out] Double[] _pdblReal)
        { return m_fnMatReadDbl(pvApiCtx, _pstName, _piRows, _piCols, _pdblReal); }//readDblMat

        public unsafe api_Err readIntMat([In]IntPtr pvApiCtx, [In] String _pstName, [Out] Int32* _piRows, [Out] Int32* _piCols, [In, Out] int[] _piData)
        { return m_fnMatReadInt(pvApiCtx, _pstName, _piRows, _piCols, _piData); }//readIntMat

        public unsafe api_Err getVarDimension([In]IntPtr pvApiCtx, [In] String _pstName, [Out] Int32* _piRows, [Out] Int32* _piCols)
        { return m_fnMatDim(pvApiCtx, _pstName, _piRows, _piCols); }//getVarDimension

        #endregion Matrix functions

        #endregion Function calls

    }//of class
}
