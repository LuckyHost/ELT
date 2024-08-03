using System.Collections.Generic;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;

namespace ElectroTools
{
    /// <summary>
    /// Логика взаимодействия для TestForm.xaml
    /// </summary>
    public partial class StartPalet : UserControl
    {
        private MyData _data;
        private DataForm _formData;
        public StartPalet(MyData data)
        {
            InitializeComponent();
            _data = data;
            this.DataContext = _data;

        }




        //Кнопка Анализ
        private void bt_Analysis(object sender, RoutedEventArgs e)
        {

            _data.isLoadProcessAnim = true;
            _data._tools.addInfo();
            _data.isLoadProcessAnim = false;

        }
        //Сохранение веса
        private void bt_weight(object sender, RoutedEventArgs e)
        {
            //this.Hide();

            //this.Show();
        }
        //ТКЗ
        private void bt_TKZ(object sender, RoutedEventArgs e)
        {
            _data._tools.getPathKZ();

        }
        //ТКЗ до точки
        private void bt_TKZ_Knot(object sender, RoutedEventArgs e)
        {
            _data._tools.getMyPathKZ();
        }
        //ТКЗ АВ
        private void bt_TKZ_AV(object sender, RoutedEventArgs e)
        {
            _data._tools.getMyPathKZFromAV();
        }
        private void bt_TKZ_i3(object sender, RoutedEventArgs e)
        {
            _data._tools.getPathKZ(false);

        }
        private void bt_myTKZ_i3(object sender, RoutedEventArgs e)
        {
            _data._tools.getMyPathKZ(false);

        }
        //SS
        private void bt_SS(object sender, RoutedEventArgs e)
        {
            _data._tools.getLocalREC();
        }
        //Падение напряжения
        private void bt_voltage(object sender, RoutedEventArgs e)
        {
            _data._tools.getVoltage();
        }

        private void bt_saveBD(object sender, RoutedEventArgs e)
        {
            _data._tools.setBD();
        }

        private void bt_getBD(object sender, RoutedEventArgs e)
        {
            _data._tools.getBD();
        }
        private void getKnot(object sender, RoutedEventArgs e)
        {
            //_data._tools.getAllPointLine();
            _formData = new DataForm(_data, new PointLine());
            _formData.Show();
        }

        //Получить все поинты
        private void getDataAllEdge(object sender, RoutedEventArgs e)
        {
            //_data._tools.getAllEdeg();
            _formData = new DataForm(_data, new Edge());
            _formData.sp_check.Visibility = Visibility.Collapsed;
            _formData.Show();
        }

        //Получить все линии
        private void getDataAllPowerLine(object sender, RoutedEventArgs e)
        {
            _data._tools.getAllPowerLine();
            _formData = new DataForm(_data, new PowerLine());
            _formData.sp_check.Visibility = Visibility.Collapsed;
            _formData.Show();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_data._tools.listPoint != null)
            {
                _data._tools.getPointLine();
            }
        }


        private void creatPathPoint(object sender, RoutedEventArgs e)
        {
            if (_data._tools.matrixSmej != null)
            {
                _data._tools.creatPathPoint();
            }

        }

        private void creatExcel(object sender, RoutedEventArgs e)
        {
            if (_data._tools.matrixSmej != null)
            {
                Excel.creadFile(_data._tools.matrixSmej, _data._tools.matrixInc, _data._tools.listEdge);
            }
        }

        private void insertBlock(object sender, RoutedEventArgs e)
        {
            //Это что бы когда чисто используешь функционал координат
            MyOpenDocument.ed = HostMgd.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            MyOpenDocument.doc = HostMgd.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            MyOpenDocument.dbCurrent = HostMgd.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;


            _data._tools.InsertBlockAtVertices();
        }


        private void deleteDraw(object sender, RoutedEventArgs e)
        {
            if (_data._tools.listPowerLine != null)
            {
                Layer.deleteObjectsOnLayer("Узлы_Saidi_Saifi_Makarov.D");
                Layer.deleteObjectsOnLayer("Граф_Saidi_Saifi_Makarov.D");
                Layer.deleteObjectsOnLayer("НазванияЛиний_Saidi_Saifi_Makarov.D");
                Layer.deleteObjectsOnLayer("Ребра_Saidi_Saifi_Makarov.D");
                Layer.deleteObjectsOnLayer("TKZ_Makarov.D");
                Layer.deleteObjectsOnLayer("Напряжение_Makarov.D");
            }
        }

        private void creatExcelFile(object sender, RoutedEventArgs e)
        {

            //Это что бы когда чисто используешь функционал координат
            MyOpenDocument.ed = HostMgd.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            MyOpenDocument.doc = HostMgd.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            MyOpenDocument.dbCurrent = HostMgd.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;


            if (_data._tools.listPowerLine != null)
            {
                string result= Text.creatPromptKeywordOptions("Имеется шаблон с координатами ? : ", new List<string>() { "Да", "Нет" }, 1);

                switch (result)
                {
                    case null: return;
                    case "Да":
                        string filePath = Excel.selectExcelFile();
                        if (filePath != null)
                        {
                            Excel.openExceleFileForCreatPL(filePath);
                        } break;
                    case "Нет": Excel.creatExcelFileCoordinate(); break;
                    default:
                        break;
                }
                
                
            }
        }

        private void settings(object sender, RoutedEventArgs e)
        {
            //Для Windows
            // Сделать окно немодальным
            //SettingWindows formSetting = new SettingWindows();
           //  formSetting.Show();
            // Сделать окно поверх других окон
            // form.Topmost = true;
            //Отдельное окно Windows
            //Это блокирует окно NCada
            //Application.ShowModalWindow(form);
        }


    }
}
