using System.Collections.Generic;

namespace Framework.Core.Interfaces.ReadFiles
{
    public interface IExcelFileHelper
    {
        IFileData CreateFileAndGetFileData<T>(List<T> records, Dictionary<string, KeyValuePair<string, string>> columns, string startFileWith = null, Dictionary<int, List<KeyValuePair<string, string>>> introductionRows = null);
        byte[] GenerateExcel<T>(string path, string sheetName, List<T> records, Dictionary<string, KeyValuePair<string, string>> columns, Dictionary<int, List<KeyValuePair<string, string>>> introductionRows = null);
    }
}