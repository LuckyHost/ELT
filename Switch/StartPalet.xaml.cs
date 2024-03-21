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
            _data._tools.getPointLine();
        }




        private void creatExcel(object sender, RoutedEventArgs e)
        {
            if (_data._tools.matrixInc != null)
            {
                Excel.creadFile(_data._tools.matrixInc);
            }

        }

        private void Button_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {

        }
    }
}
