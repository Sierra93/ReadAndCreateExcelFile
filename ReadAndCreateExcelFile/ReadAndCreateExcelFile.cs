using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace ReadAndCreateExcelFile {
    class UserDetails {
        internal int Id { get; set; }

        internal string Name { get; set; }

        internal string City { get; set; }

        internal string Country { get; set; }
    }

    class ReadAndCreateExcelFile {
        static void Main() {
            //WriteExcelFile();
            ReadExcelFile();

            Console.ReadKey();
        }

        /// <summary>
        /// Метод создает Excel-файл.
        /// </summary>
        static void WriteExcelFile() {
            try {
                List<UserDetails> persons = new List<UserDetails>() {
               new UserDetails() { Id = 1001, Name = "ABCD", City = "City1", Country = "USA" },
               new UserDetails() { Id = 1002, Name = "PQRS", City = "City2", Country = "INDIA" },
               new UserDetails() { Id = 1003, Name = "XYZZ", City = "City3", Country = "CHINA" },
               new UserDetails() { Id = 1004, Name = "LMNO", City = "City4", Country = "UK" },
          };

                DataTable table = (DataTable)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(persons), typeof(DataTable));

                using (SpreadsheetDocument document = SpreadsheetDocument.Create(@"c:\test_excel\TestFile.xlsx", SpreadsheetDocumentType.Workbook)) {
                    WorkbookPart workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    var sheetData = new SheetData();
                    worksheetPart.Worksheet = new Worksheet(sheetData);

                    Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
                    Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Sheet1" };

                    sheets.Append(sheet);

                    Row headerRow = new Row();

                    List<string> columns = new List<string>();
                    foreach (DataColumn column in table.Columns) {
                        columns.Add(column.ColumnName);

                        Cell cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(column.ColumnName);
                        headerRow.AppendChild(cell);
                    }

                    sheetData.AppendChild(headerRow);

                    foreach (DataRow dsrow in table.Rows) {
                        Row newRow = new Row();
                        foreach (string col in columns) {
                            Cell cell = new Cell();
                            cell.DataType = CellValues.String;
                            cell.CellValue = new CellValue(dsrow[col].ToString());
                            newRow.AppendChild(cell);
                        }

                        sheetData.AppendChild(newRow);
                    }

                    workbookPart.Workbook.Save();
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message.ToString());
            }
        }

        /// <summary>
        /// Метод читает Excel-файл.
        /// </summary>
        static void ReadExcelFile() {
            try {
                using (SpreadsheetDocument doc = SpreadsheetDocument.Open(@"c:\test_excel\TestFile.xlsx", false)) {
                    WorkbookPart workbookPart = doc.WorkbookPart;
                    Sheets thesheetcollection = workbookPart.Workbook.GetFirstChild<Sheets>();
                    StringBuilder excelResult = new StringBuilder();

                    foreach (Sheet thesheet in thesheetcollection) {
                        excelResult.AppendLine("Excel Sheet Name : " + thesheet.Name);
                        excelResult.AppendLine("----------------------------------------------- ");
                        Worksheet theWorksheet = ((WorksheetPart)workbookPart.GetPartById(thesheet.Id)).Worksheet;

                        SheetData thesheetdata = theWorksheet.GetFirstChild<SheetData>();
                        foreach (Row thecurrentrow in thesheetdata) {
                            foreach (Cell thecurrentcell in thecurrentrow) {
                                string currentcellvalue = string.Empty;
                                if (thecurrentcell.DataType != null) {
                                    if (thecurrentcell.DataType == CellValues.SharedString) {
                                        int id;
                                        if (int.TryParse(thecurrentcell.InnerText, out id)) {
                                            SharedStringItem item = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(id);
                                            if (item.Text != null) {
                                                excelResult.Append(item.Text.Text + " ");
                                            }
                                            else if (item.InnerText != null) {
                                                currentcellvalue = item.InnerText;
                                            }
                                            else if (item.InnerXml != null) {
                                                currentcellvalue = item.InnerXml;
                                            }
                                        }
                                    }
                                }
                                else {
                                    excelResult.Append(Convert.ToInt16(thecurrentcell.InnerText) + " ");
                                }
                            }
                            excelResult.AppendLine();
                        }
                        excelResult.Append("");
                        Console.WriteLine(excelResult.ToString());
                    }
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message.ToString());
            }
        }
    }
}
