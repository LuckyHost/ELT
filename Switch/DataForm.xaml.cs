using ElectroTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static ElectroTools.Tools;

namespace ElectroTools
{
    /// <summary>
    /// Логика взаимодействия для DataForm.xaml
    /// </summary>
    public partial class DataForm : Window, INotifyPropertyChanged
    {
        private MyData _data;
        private object _T;

        public event PropertyChangedEventHandler PropertyChanged;

        public DataForm( MyData data, object T)
       // public DataFormPoints( MyData data, object myObject)
        {
            _data = data;
            _T = T;
            InitializeComponent();
            this.DataContext = data;

            if (T is PointLine ) 
            {
            ConfigureColumnsForPointLines();
            }

            if (T is Edge)
            {
                ConfigureColumnsForEdges();
            }

            if (T is PowerLine)
            {
                ConfigureColumnsForPowerLine();
            }



            _data.PropertyChanged += MyData_PropertyChanged;
        }

        private void DataGridCell_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Получаем выбранную ячейку
            DataGridCell cell = sender as DataGridCell;

            // Получаем данные из этой ячейки
          //  if (cell != null && cell.Content is TextBlock textBlock)
            if (cell != null )
            {
                if (cell.DataContext is PointLine pointLine)
                {
                Draw.ZoomToEntity(pointLine.IDText,10) ;
                //string cellValue = textBlock.Text;
                //MessageBox.Show($"Вы нажали на ячейку с значением: {cellValue}");
                }

                if (cell.DataContext is Edge edge)
                {
                    Draw.ZoomToEntity(edge.IDText, 10);
                    //string cellValue = textBlock.Text;
                    //MessageBox.Show($"Вы нажали на ячейку с значением: {cellValue}");
                }

                if (cell.DataContext is PowerLine powerLine)
                {
                    Draw.ZoomToEntity(powerLine.IDText, 10);
                    //string cellValue = textBlock.Text;
                    //MessageBox.Show($"Вы нажали на ячейку с значением: {cellValue}");
                }
            }
        }

        //для EdgeList
        private void ConfigureColumnsForEdges()
        {
            tableData.Columns.Clear();
            tableData.ItemsSource = _data.listEdge;
            // Добавьте столбец с настройками для номера ребра
            AddDataGridTextColumn("№ Ребра", "name", true, Visibility.Visible, FontWeights.Bold, new SolidColorBrush(Color.FromRgb(255, 119, 0)), TextAlignment.Center, true);
            //Марка провода
            AddDataGridTextColumn("Марка провода", "cable", true, Visibility.Visible, FontWeights.Normal, new SolidColorBrush(Color.FromRgb(0, 0, 0)), TextAlignment.Center);
            // Сопротивление R
            AddDataGridTextColumn("Сопротивление R +jX (Z), Ом", "", true, Visibility.Visible, FontWeights.Normal, new SolidColorBrush(Color.FromRgb(0, 0, 0)), TextAlignment.Center,false, "ResistanceConverter");
            // Длина ребра
            AddDataGridTextColumn("Длина ребра, м", "length", true, Visibility.Visible, FontWeights.Normal, new SolidColorBrush(Color.FromRgb(0, 0, 0)), TextAlignment.Center);
            //Допустимый ток
            AddDataGridTextColumn("Допустимый ток, А", "Icrict", true, Visibility.Visible, FontWeights.Normal, new SolidColorBrush(Color.FromRgb(0, 0, 0)), TextAlignment.Center);
            //Продекаемый ток в ребре 
            AddDataGridTextColumn("Протекаемый ток, А", "", true, Visibility.Visible, FontWeights.UltraLight, new SolidColorBrush(Color.FromRgb(0, 0, 0)), TextAlignment.Center,false, "PointLinePhaseСurrent");
        }

        private void ConfigureColumnsForPowerLine()
        {
            tableData.Columns.Clear();
            tableData.ItemsSource = _data.listPowerLine;
            // Название
            AddDataGridTextColumn("Название линии", "name", false, Visibility.Visible, FontWeights.Bold, new SolidColorBrush(Color.FromRgb(0, 191, 255)), TextAlignment.Center, true);
            //Родитель
            AddDataGridTextColumn("Родитель", "parent.name", true, Visibility.Visible, FontWeights.Normal, new SolidColorBrush(Color.FromRgb(0, 0, 0)), TextAlignment.Center);
            //Лежит в ребрах
            AddDataGridTextColumn("Лежит в ребрах", "", true, Visibility.Visible, FontWeights.Bold, new SolidColorBrush(Color.FromRgb(0, 0, 0)), TextAlignment.Center,false, "EdgeConverter");
            //Лежит в вершинах
            AddDataGridTextColumn("Лежит в вершинах", "", true, Visibility.Visible, FontWeights.Bold, new SolidColorBrush(Color.FromRgb(0, 0, 0)), TextAlignment.Center,false, "PointLineConverter");
             // Марка провода
            AddDataGridTextColumn("Марка провода", "cable", true, Visibility.Visible, FontWeights.Normal, new SolidColorBrush(Color.FromRgb(0, 0, 0)), TextAlignment.Center);
            //Длина
            AddDataGridTextColumn("Длина, м", "lengthLine", true, Visibility.Visible, FontWeights.Normal, new SolidColorBrush(Color.FromRgb(0, 0, 0)), TextAlignment.Center);
            //Допустимый ток
            AddDataGridTextColumn("Допустимый ток, А", "Icrict", true, Visibility.Visible, FontWeights.Normal, new SolidColorBrush(Color.FromRgb(0, 0, 0)), TextAlignment.Center);
        }


        //Для PointLines
        private void ConfigureColumnsForPointLines()
        {
            // Очистите существующие столбцы
            tableData.Columns.Clear();
            tableData.ItemsSource = _data.listpoint;

            // Добавьте столбец с настройками для номера вершины
            AddDataGridTextColumn("№ вершины", "name", true, Visibility.Visible, FontWeights.Bold, new SolidColorBrush(Color.FromRgb(0, 127, 0)), TextAlignment.Center, true);
            
            // Количество потребителей
            AddDataGridTextColumn("Количество ,шт", "count", false, Visibility.Visible, FontWeights.Bold, new SolidColorBrush(Color.FromRgb(0, 0, 0)), TextAlignment.Center, true);

            //КоэфОдновремености
            AddDataGridTextColumn("Ко, о.е", "Ko", false, Visibility.Visible, FontWeights.Normal, new SolidColorBrush(Color.FromRgb(0, 0, 0)), TextAlignment.Center, true);



            // Добавьте столбец с настройками для нагрузки
            AddDataGridTextColumn("Нагрузка фазы А (или ABC), кВт", "weightA", false, Visibility.Visible, FontWeights.Normal, new SolidColorBrush(Color.FromRgb(0, 0, 0)), TextAlignment.Center);
            AddDataGridTextColumn("Нагрузка фазы В, кВт", "weightB", false, Visibility.Visible, FontWeights.Normal, new SolidColorBrush(Color.FromRgb(0, 0, 0)), TextAlignment.Center);
            AddDataGridTextColumn("Нагрузка фазы С, кВт", "weightC", false, Visibility.Visible, FontWeights.Normal, new SolidColorBrush(Color.FromRgb(0, 0, 0)), TextAlignment.Center);

            // Добавьте столбец с настройками для тока
            AddDataGridTextColumn("Ток фазы А, А", "Ia", true, Visibility.Visible, FontWeights.Normal, new SolidColorBrush(Color.FromRgb(0, 0, 0)), TextAlignment.Center);
            AddDataGridTextColumn("Ток фазы B, А", "Ib", true, Visibility.Visible, FontWeights.Normal, new SolidColorBrush(Color.FromRgb(0, 0, 0)), TextAlignment.Center);
            AddDataGridTextColumn("Ток фазы С, А", "Ic", true, Visibility.Visible, FontWeights.Normal, new SolidColorBrush(Color.FromRgb(0, 0, 0)), TextAlignment.Center);


            // Добавьте столбец с настройками для Cosφ
            AddDataGridTextColumn("Cosφ", "cos", false, Visibility.Visible, FontWeights.Normal, new SolidColorBrush(Color.FromRgb(0, 0, 0)), TextAlignment.Center);

            // Добавьте столбец с настройками для типа нагрузки
            AddDataGridTextColumn("Тип нагрузки", "typeClient", false, Visibility.Visible, FontWeights.Normal, new SolidColorBrush(Color.FromRgb(0, 0, 0)), TextAlignment.Center);


        }

       

        //Для PointLines
        private void ConfigureColumnsForPointLinesSS()
        {
            // Очистите существующие столбцы
            tableData.Columns.Clear();
            tableData.ItemsSource = _data.listpoint;

            // Добавьте столбец с настройками для номера вершины
            AddDataGridTextColumn("№ вершины", "name", true, Visibility.Visible, FontWeights.Bold, new SolidColorBrush(Color.FromRgb(0, 127, 0)), TextAlignment.Center, true);

            // Количество 
            AddDataGridTextColumn("Количество потребителей, шт", "count", false, Visibility.Visible, FontWeights.Normal, new SolidColorBrush(Color.FromRgb(0, 0, 0)), TextAlignment.Center);
            //Важность
            AddDataGridCheckBoxColumn("Важность потребителя, Да\\Нет", "isFavorite");
        }

        //Функция менят данные 
        void reTable(bool isReTable)
        {

            if (isReTable)
            {
                ConfigureColumnsForPointLinesSS();
                
                SnackbarSeven.MessageQueue.Enqueue("При выборе ''Важности'', обязательно затем перейдите на другую строку для сохранение данных");
            }
            else
            {
                ConfigureColumnsForPointLines(); 
            }
        }




        //Функция добавления столбцов
        private void AddDataGridTextColumn (string header, string bindingPath, bool isReadOnly, Visibility visibility, FontWeight fontWeight, SolidColorBrush foreground, TextAlignment textAlignment, bool addEvent = false, string converter = null, string triggerPropertyName = null, object triggerValue = null, Brush triggerBackgroundColor = null, Brush triggerForegroundColor = null)
        {

            DataGridTextColumn column = new DataGridTextColumn
            {
                Header = header,
                Binding = new Binding(bindingPath),
                IsReadOnly = isReadOnly,
                Visibility = visibility,
                
            };

            if (converter != null)
            {
                // Используйте конвертер, если он указан
                column.Binding = new Binding(bindingPath)
                {
                   Converter = (IValueConverter)FindResource(converter) // Предполагается, что конвертер объявлен в ресурсах окна
                };
            }

            if (fontWeight != null)
            {
                column.FontWeight = fontWeight;
            }

            if (foreground != null)
            {
                column.Foreground = foreground;
            }

            column.ElementStyle = new Style
            {
                Setters = { new Setter(TextBlock.TextAlignmentProperty, textAlignment) }
            };

            if (addEvent)
            {
                column.CellStyle = new Style(typeof(DataGridCell))
                {
                    Setters = { new EventSetter(DataGridCell.PreviewMouseDownEvent, new MouseButtonEventHandler(DataGridCell_PreviewMouseDown)) }
                };
            }

         
            tableData.Columns.Add(column);

           
        }

      







        //Функция с чекбоксами
        private void AddDataGridCheckBoxColumn(string header, string bindingPath)
        {
            var checkBoxColumn = new DataGridCheckBoxColumn
            {
                Header = header,
                Binding = new Binding(bindingPath)
                {
                    Mode = BindingMode.Default
                }
            };

            tableData.Columns.Add(checkBoxColumn);
        }







        private void MyData_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_data.listpoint) & _T is PointLine )
            {
                tableData.ItemsSource = _data.listpoint;
            }
            
            if (e.PropertyName == nameof(_data.listEdge) & _T is Edge)
            {
                tableData.ItemsSource = _data.listEdge;
            }

            if (e.PropertyName == nameof(_data.listPowerLine) & _T is PowerLine)
            {
                tableData.ItemsSource = _data.listPowerLine;
            }

            if ((e.PropertyName == nameof(_data.isOpenTableSS) | (e.PropertyName == nameof(_data.isOpenTableSS)) & _T is PowerLine))
            {
                reTable(_data.isOpenTableSS);
            }
        }

      
    }


}
