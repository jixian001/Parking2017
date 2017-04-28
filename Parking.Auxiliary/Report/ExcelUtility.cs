#region
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI;
using NPOI.HPSF;
using NPOI.HSSF;
using NPOI.HSSF.UserModel;
using NPOI.POIFS;
using NPOI.Util;
using NPOI.HSSF.Util;
using NPOI.HSSF.Extractor;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System.IO;
using System.Data;
#endregion

namespace Parking.Auxiliary
{
    public class ExcelUtility
    {
        private static HSSFWorkbook workbook;
        private static ExcelUtility _singleton;

        public static ExcelUtility Instance
        {
            get
            {
                return _singleton ?? (_singleton = new ExcelUtility());
            }
        }


        private void InitizeWorkbook()
        {
            if (workbook == null)
            {
                workbook = new HSSFWorkbook();
            }
            DocumentSummaryInformation dsinfo = PropertySetFactory.CreateDocumentSummaryInformation();
            dsinfo.Company = "中集智能停车";
            workbook.DocumentSummaryInformation = dsinfo;
            SummaryInformation si = PropertySetFactory.CreateSummaryInformation();
            si.Subject = "智能车库报表";
            si.Title = "智能车库报表";
            si.Author = "mo";
            si.Comments = "谢谢您的使用";
            workbook.SummaryInformation = si;
        }

        private void WriteStreamToFile(MemoryStream ms, string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            byte[] data = ms.ToArray();
            fs.Write(data, 0, data.Length);
            fs.Flush();
            fs.Close();

            data = null;
            ms = null;
            fs = null;
        }

        private Stream RenderDataTableToStream(DataTable SourceTable, string title, int colNum)
        {
            InitizeWorkbook();
            MemoryStream ms = new MemoryStream();
            HSSFSheet sheet = (HSSFSheet)workbook.CreateSheet(title);

            //表格名字
            HSSFRow titleRow = (HSSFRow)sheet.CreateRow(0);
            HSSFCell cell = (HSSFCell)titleRow.CreateCell(0);
            cell.SetCellValue(title);
            HSSFCellStyle style = (HSSFCellStyle)workbook.CreateCellStyle();
            style.VerticalAlignment = VerticalAlignment.Center;
            style.Alignment = HorizontalAlignment.Center;
            //新建一个字体样式对象
            IFont font = workbook.CreateFont();
            //设置字体加粗样式
            font.Boldweight = short.MaxValue;
            font.FontName = "微软雅黑";
            font.FontHeightInPoints = 11;
            //使用SetFont方法将字体样式添加到单元格样式中 
            style.SetFont(font);
            //将新的样式赋给单元格
            cell.CellStyle = style;
            //单元格合并
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, colNum - 1));

            HSSFRow headerRow = (HSSFRow)sheet.CreateRow(1);
            //handling header
            foreach (DataColumn column in SourceTable.Columns)
            {
                headerRow.CreateCell(column.Ordinal).SetCellValue(column.ColumnName);
            }

            //handling value
            int rowIndex = 2;

            foreach (DataRow row in SourceTable.Rows)
            {
                HSSFRow dataRow = (HSSFRow)sheet.CreateRow(rowIndex);

                foreach (DataColumn column in SourceTable.Columns)
                {
                    dataRow.CreateCell(column.Ordinal).SetCellValue(row[column].ToString());
                }

                rowIndex++;
            }

            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;

            sheet = null;
            headerRow = null;
            workbook = null;

            return ms;
        }

        /// <summary>
        /// 将datatable输出到Excel中
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="title"></param>
        /// <param name="fileName"></param>
        public void RenderDataTableToExcel(DataTable dt, string title, string fileName, int col)
        {            
            MemoryStream ms = RenderDataTableToStream(dt, title, col) as MemoryStream;
            WriteStreamToFile(ms, fileName);
        }


    }
}
