using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ElectroTools
{
    static class Excel
    {

        public static void creadFile(int[,] matrix)
        {
            //Первое создани
            Application excel = new Application();
            Workbook workbook = excel.Workbooks.Add();
            try
            {
                Worksheet worksheet1 = (Worksheet)workbook.Worksheets.Add();
                worksheet1.Name = "Матрица Инцидентности";

                Worksheet worksheet2 = (Worksheet)workbook.Worksheets.Add();
                worksheet2.Name = "Матрица Смежности";

                Worksheet worksheet4 = (Worksheet)workbook.Worksheets.Add();
                worksheet4.Name = "Матрица Веса Ребер";

                Worksheet worksheet3 = (Worksheet)workbook.Worksheets.Add();
                worksheet3.Name = "Матрица Веса Вершин";



                Range range1 = (Range)worksheet1.Cells[2, 7];
                range1.Value = "Ветви";
                range1.Font.Color = ColorTranslator.ToOle(Color.FromArgb(255, 191, 0));
                range1.Font.Bold = true;

                Range range11 = (Range)worksheet1.Cells[1, 7];
                range11.Value = "Матрица Инцидентности Сети";
                range11.Font.Color = ColorTranslator.ToOle(Color.Black);
                range11.Font.Bold = true;
                range11.Font.Size = 20;

                Range range2 = (Range)worksheet1.Cells[7, 1];
                range2.Value = "Узлы";
                range2.Font.Color = ColorTranslator.ToOle(Color.FromArgb(0, 127, 0));
                range2.Font.Bold = true;




                int up = 2;
                int left = 1;

                for (int row = 1; row <= matrix.GetLength(0); row++)
                {

                    Range range3 = (Range)worksheet1.Cells[up + 1 + row, left + 1];
                    range3.Value = row;
                    range3.Font.Bold = true;
                    range3.HorizontalAlignment = Constants.xlCenter;
                }

                for (int col = 1; col <= matrix.GetLength(1); col++)
                {
                    Range range4 = worksheet1.Cells[up + 1, left + 1 + col] as Range;
                    range4.Value = col;
                    range4.Font.Bold = true;
                    range4.HorizontalAlignment = Constants.xlCenter;
                }


                for (int j = 0; j < matrix.GetLength(0); j++)
                {
                    for (int i = 0; i < matrix.GetLength(1); i++)
                    {
                        Range range5 = worksheet1.Cells[up + 2 + j, left + 2 + i] as Range;
                        range5.Value = matrix[j, i];


                    }
                }
                //Путь на рабочий стол
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                //Клеим стрингу
                string filePath = Path.Combine(desktopPath, "Матрицы.xlsx");

                workbook.SaveAs(filePath);

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
                workbook.Close();
                excel.Quit();
                Marshal.ReleaseComObject(workbook);
                Marshal.ReleaseComObject(excel);
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
