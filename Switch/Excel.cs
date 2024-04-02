

#region Namespaces

using Microsoft.Office.Interop.Excel;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Application = Microsoft.Office.Interop.Excel.Application;
using System.Collections.Generic;
using HostMgd.Windows;
using System.Windows.Forms;
using System.Threading;



#if nanoCAD
using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
using System.Collections.Generic;
using System.Windows.Controls;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Database = Teigha.DatabaseServices.Database;
using OpenFileDialog = HostMgd.Windows.OpenFileDialog;

#else
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Database = Autodesk.AutoCAD.DatabaseServices.Database;
using OpenFileDialog = Autodesk.AutoCAD.Windows.OpenFileDialog;


#endif

#endregion Namespaces


namespace ElectroTools
{
    static class Excel
    {

        public static void creadFile(int[,] matrixSmej, int[,] matrixInc, List<Edge> listEdge)
        {
            Editor ed = MyOpenDocument.ed;
            Database dbCurrent = MyOpenDocument.dbCurrent;
            Document doc = MyOpenDocument.doc;




            //Первое создани
            Application excel = new Application();
            Workbook workbook = excel.Workbooks.Add();
            try
            {
                ed.WriteMessage("Работаю....");

                Worksheet worksheetLenEdge = (Worksheet)workbook.Worksheets.Add();
                worksheetLenEdge.Name = "Матрица Веса Ребер (Длина)";

                Worksheet worksheetZEdge = (Worksheet)workbook.Worksheets.Add();
                worksheetZEdge.Name = "Матрица Веса Ребер (Z)";

                /*
                 Worksheet worksheet3 = (Worksheet)workbook.Worksheets.Add();
                 worksheet3.Name = "Матрица Веса Вершин";*/

                Worksheet worksheetMatrixInc = (Worksheet)workbook.Worksheets.Add();
                worksheetMatrixInc.Name = "Матрица Инцидентности";

                Worksheet worksheetMatrixSmej = (Worksheet)workbook.Worksheets.Add();
                worksheetMatrixSmej.Name = "Матрица Смежности";




                //Матрица смежности
                fillDataIntWorkSheet("Матрица Смежности Сети", "Узлы", Color.FromArgb(0, 127, 0), "Узлы", Color.FromArgb(0, 127, 0), matrixSmej, worksheetMatrixSmej);

                //Матрица инцинденции 
                fillDataIntWorkSheet("Матрица Инцидентности Сети", "Ветви", Color.FromArgb(255, 191, 0), "Узлы", Color.FromArgb(0, 127, 0), matrixInc, worksheetMatrixInc);

                //Матрица Длин ребер 
                fillDataListEdgeWorkSheet("Матрица Веса Ребер (Длина)", "Ветви", Color.FromArgb(255, 191, 0), listEdge, worksheetLenEdge);

                //Матрица Z ребер 
                fillDataZListEdgeWorkSheet("Матрица Веса Ребер (Полное сопротивление)", "Ветви", Color.FromArgb(255, 191, 0), listEdge, worksheetZEdge);



                //Путь на рабочий стол
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                //Клеим стрингу
                string filePath = Path.Combine(desktopPath, "Матрицы.xlsx");


                workbook.SaveAs(filePath);
                workbook.Close();
                excel.Quit();

                OpenFileExcel(filePath);
                ed.WriteMessage("Готово.");
            }

            catch (Exception ex)
            {
                // Обработка исключений
                Console.WriteLine($"Ошибка при создании файла Excel: {ex.Message}");
                ed.WriteMessage("Произошла какая-то ошибка.");
            }

            finally
            {

                // Освобождение ресурсов
                Marshal.ReleaseComObject(workbook);
                Marshal.ReleaseComObject(excel);


            }
        }

        static void fillDataIntWorkSheet(string nameTable, string NameLeft, Color colorLeft, string NameTop, Color colorTop, int[,] matrix, Worksheet worksheetMatrix)
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


        static void fillDataListEdgeWorkSheet(string nameTable, string NameLeft, Color colorLeft, List<Edge> listEdge, Worksheet worksheetMatrix)
        {


            Range rangeNameTop = (Range)worksheetMatrix.Cells[7, 1];
            rangeNameTop.Value = NameLeft;
            rangeNameTop.Font.Color = ColorTranslator.ToOle(colorLeft);
            rangeNameTop.Font.Bold = true;

            Range rangeNameTable = (Range)worksheetMatrix.Cells[1, 7];
            rangeNameTable.Value = nameTable;
            rangeNameTable.Font.Color = ColorTranslator.ToOle(Color.Black);
            rangeNameTable.Font.Bold = true;
            rangeNameTable.Font.Size = 20;



            int up = 2;
            int left = 1;

            for (int row = 1; row <= listEdge.Count; row++)
            {

                Range range3 = (Range)worksheetMatrix.Cells[up + 1 + row, left + 1];
                range3.Value = row;
                range3.Font.Bold = true;
                range3.HorizontalAlignment = Constants.xlCenter;
            }



            for (int j = 0; j < listEdge.Count; j++)
            {
                Range range5 = worksheetMatrix.Cells[up + 2 + j, left + 2] as Range;
                range5.Value = listEdge[j].length;
            }

        }

        static void fillDataZListEdgeWorkSheet(string nameTable, string NameLeft, Color colorLeft, List<Edge> listEdge, Worksheet worksheetMatrix)
        {


            Range rangeNameTop = (Range)worksheetMatrix.Cells[7, 1];
            rangeNameTop.Value = NameLeft;
            rangeNameTop.Font.Color = ColorTranslator.ToOle(colorLeft);
            rangeNameTop.Font.Bold = true;

            Range rangeNameTable = (Range)worksheetMatrix.Cells[1, 7];
            rangeNameTable.Value = nameTable;
            rangeNameTable.Font.Color = ColorTranslator.ToOle(Color.Black);
            rangeNameTable.Font.Bold = true;
            rangeNameTable.Font.Size = 20;



            int up = 2;
            int left = 1;

            for (int row = 1; row <= listEdge.Count; row++)
            {

                Range range3 = (Range)worksheetMatrix.Cells[up + 1 + row, left + 1];
                range3.Value = row;
                range3.Font.Bold = true;
                range3.HorizontalAlignment = Constants.xlCenter;
            }



            for (int j = 0; j < listEdge.Count; j++)
            {
                Range range5 = worksheetMatrix.Cells[up + 2 + j, left + 2] as Range;
                range5.Value = Math.Round(listEdge[j].length * (Math.Sqrt(Math.Pow(listEdge[j].r, 2) + Math.Pow(listEdge[j].x, 2))), 5);
            }

        }



        private static void OpenFileExcel(string path, bool ISFormatRange = true)
        {
            Application excel = new Application();
            //Открыть книгу по пути
            Workbook workbook = excel.Workbooks.Open(path);
            excel.Visible = true;
            // Для первого плана
            excel.WindowState = XlWindowState.xlMaximized;

            //Активный лист
            //Worksheet ws =workbook.ActiveSheet as Worksheet;
            if (ISFormatRange)
            {

                foreach (Worksheet ws in workbook.Sheets)
                {
                    ws.Rows.RowHeight = 20;
                    ws.Columns.ColumnWidth = 4;
                }
            }

        }

        public static void creatExcelFileCoordinate()
        {
            Editor ed = MyOpenDocument.ed;
            Database dbCurrent = MyOpenDocument.dbCurrent;
            Document doc = MyOpenDocument.doc;


            Application excel = new Application();
            Workbook workbook = excel.Workbooks.Add();
            try
            {
                ed.WriteMessage("Создаю файл шаблон на рабочем столе");

                Worksheet workSheet = (Worksheet)workbook.Worksheets.Add();
                workSheet.Name = "Лист Координат";

                Range Y = (Range)workSheet.Cells[5, 3];
                Y.Value = "Y";
                Y.Font.Color = ColorTranslator.ToOle(Color.Black);
                Y.Font.Bold = true;
                Y.HorizontalAlignment = HorizontalAlignment.Center;
                Y.VerticalAlignment = XlVAlign.xlVAlignCenter;
                Y.Offset[1, 0].Value = 2205789.66;
                Y.Offset[2, 0].Value = 2205808.02;
                Y.Offset[3, 0].Value = 2205773.68;
                Y.Offset[4, 0].Value = 2205761.73;
                Y.Offset[5, 0].Value = 2205789.66;
                Y.Offset[7, 0].Value = 2205822.81;
                Y.Offset[8, 0].Value = 2205807.4238;

                Range X = (Range)workSheet.Cells[5, 4];
                X.Value = "X";
                X.Font.Color = ColorTranslator.ToOle(Color.Black);
                X.Font.Bold = true;
                X.HorizontalAlignment = HorizontalAlignment.Center;
                X.VerticalAlignment = XlVAlign.xlVAlignCenter;
                X.Offset[1, 0].Value = 587220.57;
                X.Offset[2, 0].Value = 587193.26;
                X.Offset[3, 0].Value = 587184.01;
                X.Offset[4, 0].Value = 587201.52;
                X.Offset[5, 0].Value = 587220.57;
                X.Offset[7, 0].Value = 587230.86;
                X.Offset[8, 0].Value = 587275.9215;

                //Путь на рабочий стол
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                //Клеим стрингу
                string filePath = Path.Combine(desktopPath, "Шаблон_Координат.xlsx");


                workbook.SaveAs(filePath);
                workbook.Close();
                excel.Quit();

                OpenFileExcel(filePath, false);
                ed.WriteMessage("Шаблон создан.");
            }

            catch (Exception ex)
            {
                // Обработка исключений
                Console.WriteLine($"Ошибка при создании файла Excel: {ex.Message}");
                ed.WriteMessage("Произошла какая-то ошибка.");
            }

            finally
            {

                // Освобождение ресурсов
                Marshal.ReleaseComObject(workbook);
                Marshal.ReleaseComObject(excel);


            }

        }

        static public string selectExcelFile()
        {
            Editor ed = MyOpenDocument.ed;

            // Создаем новый диалог выбора файла
            OpenFileDialog ofd = new OpenFileDialog("Выберите файл Excel c координатами", null, "xlsx;xls", "ExcelFileDialog", OpenFileDialog.OpenFileDialogFlags.DoNotTransferRemoteFiles);
            // Показываем диалог пользователю и проверяем результат
            DialogResult dr = ofd.ShowDialog();
            if (dr == DialogResult.OK)
            {
                // Если пользователь выбрал файл, выводим путь к файлу в командную строку AutoCAD
                ed.WriteMessage("\nВыбранный файл: " + ofd.Filename);
                return ofd.Filename;

                // Здесь может быть ваш код для дальнейшей работы с выбранным файлом Excel
            }
            else
            {

                ed.WriteMessage("\nВыбор файла отменен пользователем.");
                return null;
            }
        }


        public static void openExceleFileForCreatPL(string filePath)
        {
            Editor ed = MyOpenDocument.ed;
            Workbook workbook = null;
            Application excelApp = null;

            List<ObjectId> listObjectID = new List<ObjectId>();

            try
            {
                excelApp = new Application
                {
                    Visible = false, // Запуск Excel в фоновом режиме
                    ScreenUpdating = false // Отключение обновления экрана для ускорения
                };

                workbook = excelApp.Workbooks.Open(filePath);

                // Получаем доступ к первому листу книги
                Worksheet worksheet = (Worksheet)workbook.Sheets[1];

                // Запускаем поиск на листе
                Range range = worksheet.Cells.Find("Y");

                if (range != null)
                {
                    ed.WriteMessage($"Значение 'Y' найдено в ячейке: {range.Address}");

                    int count = 1;
                    List<double> listX = new List<double>();
                    List<double> listY = new List<double>();
                    while (true)
                    {

                        var value1 = range.Offset[count, 0].Value;
                        var value2 = range.Offset[count + 1, 0].Value;
                        var value3 = range.Offset[count, 1].Value;

                        // Проверяем, если оба значения равны null, то выходим из цикла
                        if (value1 == null && value2 == null)
                        {
                            break;
                        }

                        if (value1 == null && value2 != null)
                        {
                            count++;
                            listObjectID.Add( Draw.drawCoordinatePolyline(listX, listY));
                            listX.Clear();
                            listY.Clear();
                            continue;
                        }
                        else
                        {
                            listX.Add(value1);
                            listY.Add(value3);
                        }

                        count++;

                    }

                    listObjectID.Add(Draw.drawCoordinatePolyline(listX, listY));

                    ed.SetImpliedSelection(listObjectID.ToArray());
                    

                }
                else
                {
                    ed.WriteMessage("Значение 'X' не найдено.");
                }

                // Закрываем книгу и приложение
                workbook.Close(false); // Закрыть без сохранения изменений
                excelApp.Quit();


            }
            catch (Exception ex)
            {
                // Обработка исключений
                ed.WriteMessage("Произошла какая-то ошибка.");
            }

            finally
            {
                Marshal.ReleaseComObject(workbook);
                Marshal.ReleaseComObject(excelApp);


            }
        }
    }

}
