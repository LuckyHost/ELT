using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;



#if nanoCAD
using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
using Teigha.DatabaseServices;
using Exception = Teigha.Runtime.Exception;
#else
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Exception = Autodesk.AutoCAD.Runtime.Exception;
using Autodesk.AutoCAD.EditorInput;

#endif

namespace ElectroTools
{
    public static class BDSQL

    {

        static Editor ed = MyOpenDocument.ed;
        static Database dbCurrent = MyOpenDocument.dbCurrent;
        static Document doc = MyOpenDocument.doc;

        static public List<string> searchAllDataInBD(string dbFilePath, string nameTable, string searchColum, string filterColumn = null, string filterValue = null)
        {

            List<string> tempList = new List<string>();
            // Строка подключения к базе данных SQLite
            string connectionString = "Data Source=" + dbFilePath;
            //string connectionString = $"Data Source={dbFilePath};Version=3;";

            // SQL-запрос для выполнения (замените на свой запрос)
            string query;
            if (string.IsNullOrEmpty(filterValue) | string.IsNullOrEmpty(filterColumn))
            {
                query = "SELECT * FROM " + nameTable;
            }
            else
            {
                query = "SELECT * FROM " + nameTable + " WHERE " + filterColumn + " = " + filterValue;
            }


            using (DocumentLock doclock = doc.LockDocument())
            {

                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {



                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {

                                tempList.Add(reader[searchColum].ToString());
                                // Обрабатывайте результаты запроса
                                //ed.WriteMessage(reader[$"{searchColum}"].ToString());
                            }
                        }

                    }
                    connection.Close();
                }
            }

            return tempList;
        }

        static public double searchDataInBD(string dbFilePath, string nameTable, string searchItem, string searchColum, string gethColum)
        {
            double resultValue = 0;
            // Строка подключения к базе данных SQLite
            string connectionString = "Data Source=" + dbFilePath;
            //string connectionString = $"Data Source={dbFilePath};Version=3;";

            // SQL-запрос для выполнения (замените на свой запрос)
            string query = "SELECT * FROM " + nameTable + " WHERE " + searchColum + "= @searchItem";
            try
            {
                using (DocumentLock doclock = doc.LockDocument())
                {
                    using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                    {
                        connection.Open();
                        using (SQLiteCommand command = new SQLiteCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@searchItem", searchItem);

                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    if (double.TryParse(reader[gethColum].ToString(), out resultValue))
                                    {
                                        // Вернуть найденное значение из столбца "result"
                                        //ed.WriteMessage(resultValue.ToString());
                                    }
                                }

                            }
                            connection.Close();
                        }

                        return resultValue;
                    }
                }
            }


            catch (Exception ex)
            {

                MessageBox.Show("В базе данных отсутствует такая позиция, добавьте ее и повторите снова попытку."); ;

            return -1;
            }
        }
    }
}
