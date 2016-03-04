using System.Collections.Generic;
using System.IO;
using System.Linq;
using FPS.Helpers;
using FPS.ViewModels.Timekeeping;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace FPS.Reports
{
    public class TimekeepingReport
    {
        private readonly ICollection<TimeAttendance> _collection;

        public TimekeepingReport(IEnumerable<TimeAttendance> enumerable)
        {
            this._collection = enumerable.ToList();
        }

        public Stream Generate()
        {
            var stream = new MemoryStream();
            using (var xls = new ExcelPackage(stream))
            {
                CreateSummary(xls);
                CreateDetails(xls);
                xls.Save();
            }
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        private void CreateDetails(ExcelPackage xls)
        {
            var sheet = xls.Workbook.Worksheets.Add("Details");
            var list = _collection.OrderBy(q => q.Date).ThenBy(q => q.EmployeeName).ToList();
            // General Styles
            sheet.Cells.Style.Font.Name = "Arial";
            sheet.Cells.Style.Font.Size = 9;
            // REPORT HEADERS
            var row = 1;
            var col = 1;
            sheet.Column(col).Width = 40;
            sheet.Cells[row, col++].SetColumnHeader("NAME");
            sheet.Column(col).Width = 14;
            sheet.Cells[row, col++].SetColumnHeader("DATE");
            sheet.Column(col).Width = 14;
            sheet.Cells[row, col++].SetColumnHeader("TIME IN");
            sheet.Column(col).Width = 14;
            sheet.Cells[row, col++].SetColumnHeader("TIME OUT");
            sheet.Column(col).Width = 14;
            sheet.Cells[row, col++].SetColumnHeader("WORKTIME");
            sheet.Column(col).Width = 14;
            sheet.Cells[row, col++].SetColumnHeader("OVERTIME");
            sheet.Column(col).Width = 14;
            sheet.Cells[row, col++].SetColumnHeader("LATE");
            sheet.Column(col).Width = 14;
            sheet.Cells[row, col++].SetColumnHeader("UNDERTIME");
            sheet.Column(col).Width = 40;
            sheet.Cells[row, col].SetColumnHeader("REMARKS");
            sheet.Row(row).Height = 20.5;
            row++;
            sheet.Cells["A1:I1"].AutoFilter = true;
            foreach (var i in list)
            {
                sheet.Cells[$"A{row}"].SetValue(i.EmployeeName);
                sheet.Cells[$"B{row}"].SetValue(i.Date, "mm/dd/yyyy");
                sheet.Cells[$"C{row}"].SetValue(i.TimeIn?.TimeOfDay, "hh:mm");
                sheet.Cells[$"D{row}"].SetValue(i.TimeOut?.TimeOfDay, "hh:mm");
                // worktime
                sheet.Cells[$"E{row}"].SetValue(i.Worktime, "hh:mm");
                // overtime
                sheet.Cells[$"F{row}"].SetValue(i.Overtime, "hh:mm");
                // late
                sheet.Cells[$"G{row}"].SetValue(i.Late, "hh:mm");
                // undertime
                sheet.Cells[$"H{row}"].SetValue(i.Undertime, "hh:mm");
                // remarks
                sheet.Cells[$"I{row}"].SetValue(i.Remarks);

                sheet.Cells[$"B{row}:H{row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Row(row).Height = 14.5;
                row++;
            }

            // subtotals
            sheet.Cells[$"E{row}"].SetFormula($"=SUBTOTAL(9,E2:E{row - 1})", "dd:hh:mm");
            sheet.Cells[$"F{row}"].SetFormula($"=SUBTOTAL(9,F2:F{row - 1})", "hh:mm");
            sheet.Cells[$"G{row}"].SetFormula($"=SUBTOTAL(9,G2:G{row - 1})", "hh:mm");
            sheet.Cells[$"H{row}"].SetFormula($"=SUBTOTAL(9,H2:H{row - 1})", "hh:mm");
            sheet.Cells[$"E{row}:H{row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Row(row).Height = 20.5;
        }

        private void CreateSummary(ExcelPackage xls)
        {
            var sheet = xls.Workbook.Worksheets.Add("Summary");
            var list = _collection.Select(q => q.EmployeeName).Distinct().OrderBy(q => q).ToList();
            var count = _collection.Count + 1;
            // General Styles
            sheet.Cells.Style.Font.Name = "Arial";
            sheet.Cells.Style.Font.Size = 9;
            // REPORT HEADERS
            sheet.Cells["A1:A2"].Style.Font.Bold = true;
            sheet.Cells["A1:A2"].Style.Font.Size = 11;

            sheet.Row(1).Height = 15.5;
            sheet.Cells["A1"].Value = "TIME ATTENDANCE ";
            sheet.Row(2).Height = 15.5;
            sheet.Cells["A2"].Value = "SUMMARY REPORT";

            sheet.Cells["A4"].SetColumnHeader("NAME");
            sheet.Cells["B4"].SetColumnHeader("DAYS WORKED");
            sheet.Cells["C4"].SetColumnHeader("OVERTIME");
            sheet.Cells["C5"].SetColumnHeader("HH:MM");
            sheet.Cells["D5"].SetColumnHeader("DAYS");
            sheet.Cells["E4"].SetColumnHeader("LATE");
            sheet.Cells["E5"].SetColumnHeader("HH:MM");
            sheet.Cells["F5"].SetColumnHeader("DAYS");
            sheet.Cells["G4"].SetColumnHeader("UNDERTIME");
            sheet.Cells["G5"].SetColumnHeader("HH:MM");
            sheet.Cells["H5"].SetColumnHeader("DAYS");

            sheet.Cells["A4:A5"].Merge = true;
            sheet.Cells["A4:A5"].Style.BorderAround(ExcelBorderStyle.Thin, 14);

            sheet.Cells["B4:B5"].Merge = true;
            sheet.Cells["B4:B5"].Style.BorderAround(ExcelBorderStyle.Thin, 14);

            sheet.Cells["C4:D4"].Merge = true;
            sheet.Cells["C4:D4"].Style.BorderAround(ExcelBorderStyle.Thin, 14);

            sheet.Cells["E4:F4"].Merge = true;
            sheet.Cells["E4:F4"].Style.BorderAround(ExcelBorderStyle.Thin, 14);

            sheet.Cells["G4:H4"].Merge = true;
            sheet.Cells["G4:H4"].Style.BorderAround(ExcelBorderStyle.Thin, 14);

            sheet.Cells["B4:B5"].Style.WrapText = true;
            sheet.Column(1).Width = 40;
            sheet.Column(2).Width = 10;
            sheet.Column(3).Width = 10;
            sheet.Column(4).Width = 10;
            sheet.Column(5).Width = 10;
            sheet.Column(6).Width = 10;
            sheet.Column(7).Width = 10;
            sheet.Column(8).Width = 10;
            sheet.Column(9).Width = 10;
            sheet.Column(10).Width = 10;
            sheet.Column(11).Width = 10;
            sheet.Column(12).Width = 10;

            sheet.Row(4).Height = 15.5;
            sheet.Row(5).Height = 15.5;

            var r = 6;
            foreach (var name in list)
            {
                sheet.Cells[$"A{r}"].SetValue(name);
                sheet.Cells[$"B{r}"].SetFormula($"=COUNTIF(Details!A2:A{count},Summary!A{r})");
                sheet.Cells[$"C{r}"].SetFormula($"=SUMIF(Details!A2:A{count},Summary!A{r},Details!F2:F{count})", "hh:mm");
                sheet.Cells[$"D{r}"].SetFormula(
                    $"=COUNTIFS(Details!A2:A{count},Summary!A{r},Details!F2:F{count},\">0\")");
                sheet.Cells[$"E{r}"].SetFormula($"=SUMIF(Details!A2:A{count},Summary!A{r},Details!G2:G{count})", "hh:mm");
                sheet.Cells[$"F{r}"].SetFormula(
                    $"=COUNTIFS(Details!A2:A{count},Summary!A{r},Details!G2:G{count},\">0\")");
                sheet.Cells[$"G{r}"].SetFormula($"=SUMIF(Details!A2:A{count},Summary!A{r},Details!H2:H{count})", "hh:mm");
                sheet.Cells[$"H{r}"].SetFormula(
                    $"=COUNTIFS(Details!A2:A{count},Summary!A{r},Details!H2:H{count},\">0\")");

                sheet.Row(r).Height = 14.5;
                r++;
            }
        }
    }
}
