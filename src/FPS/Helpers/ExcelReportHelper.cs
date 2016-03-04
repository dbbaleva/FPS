using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace FPS.Helpers
{
    public static class ExcelReportHelper
    {
        public static ExcelRange SetBackgroundColor(this ExcelRange range, int colorIndex)
        {
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.Indexed = colorIndex;
            return range;
        }

        public static void SetColumnHeader(this ExcelRange range, string text)
        {
            range.Style.BorderAround(ExcelBorderStyle.Thin, 14);
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            range.Style.Font.Bold = true;
            range.Value = text;
        }

        public static void BorderAround(this ExcelStyle style, ExcelBorderStyle borderStyle, int colorIndex)
        {
            style.Border.Left.Style = borderStyle;
            style.Border.Left.Color.Indexed = colorIndex;
            style.Border.Top.Style = borderStyle;
            style.Border.Top.Color.Indexed = colorIndex;
            style.Border.Bottom.Style = borderStyle;
            style.Border.Bottom.Color.Indexed = colorIndex;
            style.Border.Right.Style = borderStyle;
            style.Border.Right.Color.Indexed = colorIndex;
        }

        public static ExcelRange SetFormula(this ExcelRange range, string formula, bool bordered = true)
        {
            range.Formula = formula;
            if (bordered)
                range.Style.BorderAround(ExcelBorderStyle.Thin, 14);
            return range;
        }

        public static ExcelRange SetFormula(this ExcelRange range, string formula, string numberFormat,
            bool bordered = true)
        {
            range.SetFormula(formula, bordered);
            range.Style.Numberformat.Format = numberFormat;
            return range;
        }

        public static ExcelRange SetValue(this ExcelRange range, object value, bool bordered = true)
        {
            range.Value = value;
            if (bordered)
                range.Style.BorderAround(ExcelBorderStyle.Thin, 14);
            return range;
        }

        public static ExcelRange SetValue(this ExcelRange range, object value, string numberFormat, bool bordered = true)
        {
            range.SetValue(value, bordered);
            range.Style.Numberformat.Format = numberFormat;
            return range;
        }
    }
}
