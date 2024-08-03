using System;
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


            using (DocumentLock doclock = MyOpenDocument.doc.LockDocument())
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

        static public T searchDataInBD <T> (string dbFilePath, string nameTable, string searchItem, string searchColum, string getColumn)
        {
            T resultValue = default;
            // Строка подключения к базе данных SQLite
            string connectionString = "Data Source=" + dbFilePath;
            //string connectionString = $"Data Source={dbFilePath};Version=3;";

            // SQL-запрос для выполнения (замените на свой запрос)
            string query = "SELECT * FROM " + nameTable + " WHERE " + searchColum + "= @searchItem";
            try
            {
                using (DocumentLock doclock = MyOpenDocument.doc.LockDocument())
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
                                    var value = reader[getColumn];

                                    if (value != DBNull.Value)
                                    {
                                        resultValue = (T)Convert.ChangeType(value, typeof(T));
                                    }

                                }
                                connection.Close();
                            }

                            return resultValue;
                        }
                    }
                }
            }


            catch (Exception ex)
            {

                MessageBox.Show("В базе данных отсутствует такая позиция, добавьте ее и повторите снова попытку."); ;

                return default;
            }
        }

        static public bool updateDataInDB(string dbFilePath, string nameTable, string searchColumn, string searchValue, string updateColumn, string newValue)
        {
            // Строка подключения к базе данных SQLite
            string connectionString = "Data Source=" + dbFilePath;

            // SQL-запрос для обновления данных
            string query = $"UPDATE {nameTable} SET {updateColumn} = @newValue WHERE {searchColumn} = @searchValue";

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@newValue", newValue);
                        command.Parameters.AddWithValue("@searchValue", searchValue);

                        int rowsAffected = command.ExecuteNonQuery();

                        // Проверяем, были ли затронуты строки
                        if (rowsAffected > 0)
                        {
                            return true; // Обновление прошло успешно
                        }
                        else
                        {
                            MessageBox.Show("Запись не найдена или значение не изменилось.");
                            return false; // Обновление не удалось
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении базы данных: {ex.Message}");
                return false; // Обновление не удалось
            }
        }
    }
}
