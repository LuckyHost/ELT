using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Xml;

namespace ElectroTools
{
    static class UserData
    {


        //Дальность поиска
        public static int searchDistancePL = 1;
        public static string defaultBlock = "Блок_Нумерации_Makarov.D";
        public static int roundCoordinateDistFileExcel = 2;
        public static int roundCoordinateXYFileExcel = 2;
        public static bool isDrawZoneSearchPL = false;
        public static double searchLengthPL = 1;
        public static bool isSelectSearchPL = true;
        public static int coefficientMultiplicity = 3;




        static public void updateUserData(string dbFilePath)
        {
            searchDistancePL = Convert.ToInt32(BDSQL.searchDataInBD<string>(dbFilePath, "userData", "searchDistancePL", "name", "valve"));
            defaultBlock = (BDSQL.searchDataInBD<string>(dbFilePath, "userData", "defaultBlock", "name", "valve"));
            roundCoordinateDistFileExcel = Convert.ToInt32(BDSQL.searchDataInBD<string>(dbFilePath, "userData", "roundCoordinateDistFileExcel", "name", "valve"));
            roundCoordinateXYFileExcel = Convert.ToInt32(BDSQL.searchDataInBD<string>(dbFilePath, "userData", "roundCoordinateXYFileExcel", "name", "valve"));
            isDrawZoneSearchPL = Convert.ToBoolean(Convert.ToInt32(BDSQL.searchDataInBD<string>(dbFilePath, "userData", "isDrawZoneSearchPL", "name", "valve")));
            isSelectSearchPL = Convert.ToBoolean(Convert.ToInt32(BDSQL.searchDataInBD<string>(dbFilePath, "userData", "isSelectSearchPL", "name", "valve")));
            searchLengthPL = (Convert.ToDouble( BDSQL.searchDataInBD<string>(dbFilePath, "userData", "searchLengthPL", "name", "valve").Replace(".",",")));
            coefficientMultiplicity = Convert.ToInt32(BDSQL.searchDataInBD<string>(dbFilePath, "userData", "ratioCircuitBreaker", "name", "valve"));
            OnStaticPropertyChanged(nameof(searchDistancePL));
        }

        public static event PropertyChangedEventHandler StaticPropertyChanged;

        public static void OnStaticPropertyChanged(string propertyName)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }

     

    }




}
