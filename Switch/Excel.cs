using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Application = Microsoft.Office.Interop.Excel.Application;

namespace ElectroTools
{
    static class Excel
    {

        public static void creadFile(int[,] matrixSmej, int[,]  matrixInc)
        {
            //Первое создани
            Application excel = new Application();
            Workbook workbook = excel.Workbooks.Add();
            try
            {
                Worksheet worksheetMatrixInc = (Worksheet)workbook.Worksheets.Add();
                worksheetMatrixInc.Name = "Матрица Инцидентности";

                Worksheet worksheetMatrixSmej = (Worksheet)workbook.Worksheets.Add();
                worksheetMatrixSmej.Name = "Матрица Смежности";

                Worksheet worksheet4 = (Worksheet)workbook.Worksheets.Add();
                worksheet4.Name = "Матрица Веса Ребер";

                Worksheet worksheet3 = (Worksheet)workbook.Worksheets.Add();
                worksheet3.Name = "Матрица Веса Вершин";


                /*
                Range range1 = (Range)worksheet1.Cells[2, 7];
                range1.Value = "Ветви";
                range1.Font.Color = ColorTranslator.ToOle(Color.FromArgb(255, 191, 0));
                range1.Font.Bold = true;*/

                //Матрица смежности
                fillDataWorkSheet("Матрица Смежности Сети", "Узлы", Color.FromArgb(0, 127, 0), "Узлы", Color.FromArgb(0, 127, 0), matrixSmej, worksheetMatrixSmej);

                //Матрица инцинденции 
                fillDataWorkSheet("Матрица Инцидентности Сети", "Ветви", Color.FromArgb(255, 191, 0), "Узлы", Color.FromArgb(0, 127, 0), matrixInc, worksheetMatrixInc);



                //Путь на рабочий стол
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                //Клеим стрингу
                string filePath = Path.Combine(desktopPath, "Матрицы.xlsx");


                workbook.SaveAs(filePath);
                workbook.Close();
                excel.Quit();

                OpenFileExcel(filePath);

            }

            catch (Exception ex)
            {
                // Обработка исключений
                Console.WriteLine($"Ошибка при создании файла Excel: {ex.Message}");
            }

            finally
            {

                // Освобождение ресурсов
                Marshal.ReleaseComObject(workbook);
                Marshal.ReleaseComObject(excel);

            }
        }

        static void fillDataWorkSheet(string nameTable, string NameLeft,Color colorLeft , string NameTop, Color colorTop, int[,] matrix,Worksheet worksheetMatrix ) 
        {
            Range rangeNameLeft = (Range)worksheetMatrix.Cells[2, 7];
            rangeNameLeft.Value = NameLeft;
            rangeNameLeft.Font.Color = ColorTranslator.ToOle(colorLeft);
            rangeNameLeft.Font.Bold = true;

            Range rangeNameTable = (Range)worksheetMatrix.Cells[1, 7];
            rangeNameTable.Value = nameTable;
            rangeNameTable.Font.Color = ColorTranslator.ToOle(Color.Black);
            rangeNameTable.Font.Bold = true;
            rangeNameTable.Font.Size = 20;

            Range rangeNameTop = (Range)worksheetMatrix.Cells[7, 1];
            rangeNameTop.Value = NameTop;
            rangeNameTop.Font.Color = ColorTranslator.ToOle(colorTop);
            rangeNameTop.Font.Bold = true;




            int up = 2;
            int left = 1;

            for (int row = 1; row <= matrix.GetLength(0); row++)
            {

                Range range3 = (Range)worksheetMatrix.Cells[up + 1 + row, left + 1];
                range3.Value = row;
                range3.Font.Bold = true;
                range3.HorizontalAlignment = Constants.xlCenter;
            }

            for (int col = 1; col <= matrix.GetLength(1); col++)
            {
                Range range4 = worksheetMatrix.Cells[up + 1, left + 1 + col] as Range;
                range4.Value = col;
                range4.Font.Bold = true;
                range4.HorizontalAlignment = Constants.xlCenter;
            }


            for (int j = 0; j < matrix.GetLength(0); j++)
            {
                for (int i = 0; i < matrix.GetLength(1); i++)
                {
                    Range range5 = worksheetMatrix.Cells[up + 2 + j, left + 2 + i] as Range;
                    range5.Value = matrix[j, i];


                }
            }
        }



        private static void OpenFileExcel(string path)
        {
            Application excel = new Application();
            //Открыть книгу по пути
            Workbook workbook = excel.Workbooks.Open(path);
            excel.Visible = true;
            // Для первого плана
            excel.WindowState = XlWindowState.xlMaximized;

            //Активный лист
            //Worksheet ws =workbook.ActiveSheet as Worksheet;

            foreach (Worksheet ws in workbook.Sheets)
            {
                ws.Rows.RowHeight = 20;
                ws.Columns.ColumnWidth = 4;
            }

        }
    }

}
