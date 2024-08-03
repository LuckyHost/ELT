using HostMgd.Windows.ToolPalette;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ElectroTools
{
    /// <summary>
    /// Логика взаимодействия для SettingForm.xaml
    /// </summary>
    public partial class SettingForm : Window
    {
        private MyData _data;

       

        public SettingForm(MyData data)
        {

            InitializeComponent();
            _data = data;
            this.DataContext = _data;
           
        }


        //Обновить пользовательские данные в BD
        private void sendDataInDB(object sender, RoutedEventArgs e)
        {
           

            if (   //Обновить данные блока по умолчанию
                        BDSQL.updateDataInDB(_data._tools.dbFilePath, "userData", "name", "defaultBlock", "valve",this.tbDefautNameBlock.Text) &&
                    //Обновить данные округ координат
                    BDSQL.updateDataInDB(_data._tools.dbFilePath, "userData", "name", "roundCoordinateXYFileExcel", "valve", this.tbCoord.Text)&&
                    //Обновить данные окргу расстояние
                    BDSQL.updateDataInDB(_data._tools.dbFilePath, "userData", "name", "roundCoordinateDistFileExcel", "valve", this.tbDistCoord.Text)&&
                    //Обновить данные радиус поиска
                    BDSQL.updateDataInDB(_data._tools.dbFilePath, "userData", "name", "searchDistancePL", "valve", this.tbRadiusSearchPL.Text)&&
                    //Обновить данные границы поиска
                     BDSQL.updateDataInDB(_data._tools.dbFilePath, "userData", "name", "isDrawZoneSearchPL", "valve", (bool)this.isActZone.IsChecked ? 1.ToString():0.ToString())

                )
            { 
            MessageBox.Show("Запись успешно обновлена.Перезапустите приложение");
            }
            else
            {
                MessageBox.Show("Не удалось обновить запись.");
            }



        }

   


    }
}
