using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text.RegularExpressions;
using System.IO;
using DocumentFormat.OpenXml;

namespace Framework.Core.ReadFiles
{
    using System.Data;
    using ClosedXML.Excel;
    using Interfaces.ReadFiles;

    public class ExcelFileHelper : IExcelFileHelper
    {

        public ExcelFileHelper()
        {
        }


        public IFileData CreateFileAndGetFileData<T>(List<T> records, Dictionary<string, KeyValuePair<string, string>> columns, string startFileWith = null, Dictionary<int, List<KeyValuePair<string, string>>> introductionRows = null)
        {
            var directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"excel");

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            var fileName = startFileWith != null ? startFileWith + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx" : DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
            var bytes = GenerateExcel(Path.Combine(directoryPath, fileName), "sheet1", records, columns, introductionRows);

            IFileData fileData = new FileData()
            {
                Data = bytes,
                Name = fileName
            };

            return fileData;
        }

        public byte[] GenerateExcel<T>(string path, string sheetName, List<T> records, Dictionary<string, KeyValuePair<string, string>> columns, Dictionary<int, List<KeyValuePair<string, string>>> introductionRows = null)
        {
            var stream = new MemoryStream();
            var wb = new XLWorkbook();
            wb.RightToLeft = true;
            var ws = wb.Worksheets.Add(sheetName);
            //set introduction Row:
            int rowIdex = 1;
            int cellIndex = 1;
            if (introductionRows != null)
            {

                for (int i = 0; i < introductionRows.Count; i++)
                {

                    var introductionRow = introductionRows[i];
                    cellIndex = 1;
                  
                    foreach (var prop in introductionRow)
                    {
                        ws.Cell(rowIdex, cellIndex).Value = prop.Key;
                        ws.Cell(rowIdex, cellIndex).WorksheetColumn().Width = 15;
                        ws.Cell(rowIdex, cellIndex).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        if (prop.Value == "red")
                            ws.Cell(rowIdex, cellIndex).Style.Font.FontColor = XLColor.Red;
                        if(prop.Value == "title")
                        {
                            ws.Cell(rowIdex, cellIndex).Style.Font.Bold = true;
                            ws.Cell(rowIdex, cellIndex).Style.Font.FontSize = 14;
                        }
                           
                        cellIndex++;
                    }
                    rowIdex++;
                }
            }

            cellIndex = 1;
            foreach (PropertyInfo prop in typeof(T).GetProperties())
            {
                if (columns != null && !columns.ContainsKey(prop.Name))
                    continue;

                DisplayNameAttribute attr = (DisplayNameAttribute)Attribute.GetCustomAttribute(prop, typeof(DisplayNameAttribute));
                var propName = columns != null ? columns[prop.Name].Key : prop.Name;
                ws.Cell(rowIdex, cellIndex).Value = propName;
                ws.Cell(rowIdex, cellIndex).Value = propName;
                ws.Cell(rowIdex, cellIndex).WorksheetColumn().Width = 19;
                ws.Cell(rowIdex, cellIndex).Style.Font.Bold = true;
                ws.Cell(rowIdex, cellIndex).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                cellIndex++;
            }

            foreach (var record in records)
            {
                Type myType = record.GetType();
                IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());
                cellIndex = 1;
                rowIdex++;
                foreach (PropertyInfo prop in props)
                {
                    if (columns != null && !columns.ContainsKey(prop.Name))
                        continue;
                    object propValue = prop.GetValue(record, null);
                    string cellValue = propValue != null ? propValue.ToString() : "N/A";

                    if (propValue != null && columns != null && columns[prop.Name].Value != null)
                    {
                        cellValue = string.Format(columns[prop.Name].Value.ToString(), propValue);
                    }
                    ws.Cell(rowIdex, cellIndex).SetValue<string>(cellValue);
                    ws.Cell(rowIdex, cellIndex).WorksheetColumn().Width = 19;
                    ws.Cell(rowIdex, cellIndex).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    cellIndex++;
                }

            }

            wb.SaveAs(path);

            byte[] result = File.ReadAllBytes(path);

            return result;
        }


        public IFileData CreateExcelFile(DataTable records, Dictionary<int,string> columns = null, string startFileWith = null)
        {
            records.TableName = "Data";
            var directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"excel");            

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            var fileName = startFileWith != null ? startFileWith + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx" : DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
            var bytes = GenerateExcel(Path.Combine(directoryPath, fileName),records, columns);

            IFileData fileData = new FileData()
            {
                Data = bytes,
                Name = fileName
            };

            return fileData;
        }

        public byte[] GenerateExcel(string path, DataTable dt, Dictionary<int, string> columns = null)
        {
            var stream = new MemoryStream();
            var wb = new XLWorkbook();
            wb.RightToLeft = true;
            var ws = wb.Worksheets.Add(dt);
            
            if (columns != null)
            {
                foreach (var key in columns)
                {                    
                    ws.Cell(1, key.Key).Value = key.Value;                    
                }

            }           

            wb.SaveAs(path);
            byte[] result = File.ReadAllBytes(path);
            return result;
        }

        //public byte[] GenerateExcel<T>(string path, string sheetName, List<T> records, Dictionary<string, KeyValuePair<string, string>> columns, List<string> introductionRow = null)
        //{
        //    var stream = new MemoryStream();
        //    var document = SpreadsheetDocument.Create(/*stream*/path, SpreadsheetDocumentType.Workbook, true);

        //    var workbookpart = document.AddWorkbookPart();
        //    workbookpart.Workbook = new Workbook();
        //    var worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
        //    var sheetData = new SheetData();

        //    worksheetPart.Worksheet = new Worksheet(sheetData);

        //    var sheets = document.WorkbookPart.Workbook.
        //        AppendChild<Sheets>(new Sheets());

        //    var sheet = new Sheet()
        //    {
        //        Id = document.WorkbookPart
        //            .GetIdOfPart(worksheetPart),
        //        SheetId = 1,
        //        Name = sheetName
        //    };
        //    sheets.AppendChild(sheet);

        //    //// Add header
        //    //UInt32 rowIdex = 0;
        //    //var row = new Row { RowIndex = ++rowIdex };
        //    //sheetData.AppendChild(row);
        //    //var cellIdex = 0;

        //    //foreach (var header in data.Headers)
        //    //{
        //    //    row.AppendChild(CreateTextCell(ColumnLetter(cellIdex++),
        //    //        rowIdex, header??string.Empty));
        //    //}
        //    //if (data.Headers.Count > 0)
        //    //{
        //    //    // Add the column configuration if available
        //    //    if (data.ColumnConfigurations != null)
        //    //    {
        //    //        var columns = (Columns)data.ColumnConfigurations.Clone();
        //    //        worksheetPart.Worksheet
        //    //            .InsertAfter(columns, worksheetPart
        //    //            .Worksheet.SheetFormatProperties);
        //    //    }
        //    //}

        //    // Add sheet data

        //    //set introduction Row:
        //    UInt32 rowIdex = 0;
        //    if (introductionRow != null)
        //    {
        //        var rowPrefix = new Row { RowIndex = ++rowIdex };
        //        sheetData.AppendChild(rowPrefix);
        //        int cellIdex1 = 0;
        //        foreach (var prop in introductionRow)
        //        {
        //            var cell = CreateTextCell(ColumnLetter(cellIdex1++),
        //                        rowIdex, prop);
        //            rowPrefix.AppendChild(cell);

        //        }
        //    }

        //    // Set Headers
        //    //UInt32 rowIdex = 0;
        //    var rowHeader = new Row { RowIndex = ++rowIdex };
        //    sheetData.AppendChild(rowHeader);
        //    int cellIdex = 0;
        //    foreach (PropertyInfo prop in typeof(T).GetProperties())
        //    {
        //        if (columns != null && !columns.ContainsKey(prop.Name))
        //            continue;

        //        DisplayNameAttribute attr = (DisplayNameAttribute)Attribute.GetCustomAttribute(prop, typeof(DisplayNameAttribute));
        //        var propName = columns != null ? columns[prop.Name].Key : prop.Name;

        //        var cell = CreateTextCell(ColumnLetter(cellIdex++),
        //            rowIdex, (attr == null ? propName : attr.DisplayName));
        //        rowHeader.AppendChild(cell);

        //    }

        //    foreach (var record in records)
        //    {
        //        Type myType = record.GetType();
        //        IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());

        //        var row = new Row { RowIndex = ++rowIdex };
        //        cellIdex = 0;

        //        foreach (PropertyInfo prop in props)
        //        {
        //            if (columns != null && !columns.ContainsKey(prop.Name))
        //                continue;
        //            object propValue = prop.GetValue(record, null);
        //            string cellValue = propValue != null ? propValue.ToString() : "N/A";

        //            if (propValue != null && columns != null && columns[prop.Name].Value != null)
        //            {
        //                cellValue = string.Format(columns[prop.Name].Value.ToString(), propValue);
        //            }
        //            var cell = CreateTextCell(ColumnLetter(cellIdex++),
        //                rowIdex, (cellValue));
        //            row.AppendChild(cell);

        //        }

        //        sheetData.AppendChild(row);
        //    }


        //    //foreach (var rowData in records.Headers)
        //    //{
        //    //    int cellIdex = 0;
        //    //    var row = new Row { RowIndex = ++rowIdex };
        //    //    sheetData.AppendChild(row);
        //    //    foreach (var callData in rowData.Values)
        //    //    {
        //    //        var cell = CreateTextCell(ColumnLetter(cellIdex++),
        //    //            rowIdex, callData);
        //    //        row.AppendChild(cell);
        //    //    }
        //    //}

        //    workbookpart.Workbook.Save();

        //    //document.
        //    //byte[] res = null;
        //    //stream.Position = 0;
        //    //using (MemoryStream memoryStream = new MemoryStream())
        //    //{
        //    //    stream.CopyTo(memoryStream);
        //    //    res = memoryStream.ToArray();
        //    //}



        //    document.Close();


        //    //return res;

        //    byte[] result = File.ReadAllBytes(path);


        //    return result;

        //    //MemoryStream m = new MemoryStream();
        //    //workbookpart.Workbook.SaveToStream(m);

        //    //return File(m, "application/vnd.ms-excel");


        //    //            workbookpar
        //    //            document.
        //    //            return stream.ToArray();
        //}

        #region Private Methods

        private string GetColumnName(string cellReference)
        {
            var regex = new Regex("[A-Za-z]+");
            var match = regex.Match(cellReference);

            return match.Value;
        }

        private int ConvertColumnNameToNumber(string columnName)
        {
            var alpha = new Regex("^[A-Z]+$");
            if (!alpha.IsMatch(columnName)) throw new ArgumentException();

            char[] colLetters = columnName.ToCharArray();
            Array.Reverse(colLetters);

            var convertedValue = 0;
            for (int i = 0; i < colLetters.Length; i++)
            {
                char letter = colLetters[i];
                // ASCII 'A' = 65
                int current = i == 0 ? letter - 65 : letter - 64;
                convertedValue += current * (int)Math.Pow(26, i);
            }

            return convertedValue;
        }

        private IEnumerator<Cell> GetExcelCellEnumerator(Row row)
        {
            int currentCount = 0;
            foreach (Cell cell in row.Descendants<Cell>())
            {
                string columnName = GetColumnName(cell.CellReference);

                int currentColumnIndex = ConvertColumnNameToNumber(columnName);

                for (; currentCount < currentColumnIndex; currentCount++)
                {
                    var emptycell = new Cell()
                    {
                        DataType = null,
                        CellValue = new CellValue(string.Empty)
                    };
                    yield return emptycell;
                }

                yield return cell;
                currentCount++;
            }
        }

        private string ReadExcelCell(Cell cell, WorkbookPart workbookPart)
        {
            var cellValue = cell.CellValue;
            var text = (cellValue == null) ? cell.InnerText : cellValue.Text;
            if ((cell.DataType != null) && (cell.DataType == CellValues.SharedString))
            {
                text = workbookPart.SharedStringTablePart.SharedStringTable
                    .Elements<SharedStringItem>().ElementAt(
                        int.Parse(cell.CellValue.Text)).InnerText;
            }

            return (text ?? string.Empty).Trim();
        }

        private string ColumnLetter(int intCol)
        {
            var intFirstLetter = ((intCol) / 676) + 64;
            var intSecondLetter = ((intCol % 676) / 26) + 64;
            var intThirdLetter = (intCol % 26) + 65;

            var firstLetter = (intFirstLetter > 64)
                ? (char)intFirstLetter : ' ';
            var secondLetter = (intSecondLetter > 64)
                ? (char)intSecondLetter : ' ';
            var thirdLetter = (char)intThirdLetter;

            return string.Concat(firstLetter, secondLetter,
                thirdLetter).Trim();
        }

        private Cell CreateTextCell(string header, UInt32 index, string text)
        {
            var cell = new Cell
            {
                DataType = CellValues.InlineString,
                CellReference = header + index
            };

            var istring = new InlineString();
            var t = new Text { Text = text };
            istring.AppendChild(t);
            cell.AppendChild(istring);
            return cell;
        }

        #endregion Private Methods

    }
}