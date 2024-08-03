using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Xml;

namespace ElectroTools
{
    static class UserData 
    {
       
       public static int coefficientMultiplicity=3;

        //Дальность поиска
       public static int searchDistancePL=1;
       public static string defaultBlock= "Блок_Нумерации_Makarov.D";
       public static int roundCoordinateDistFileExcel= 2;
       public static int roundCoordinateXYFileExcel= 2;
       public static bool isDrawZoneSearchPL = false;

        static public void updateUserData(string dbFilePath) 
         {
            searchDistancePL =  Convert.ToInt32( BDSQL.searchDataInBD<string>(dbFilePath, "userData", "searchDistancePL", "name", "valve"));
            defaultBlock =  ( BDSQL.searchDataInBD<string>(dbFilePath, "userData", "defaultBlock", "name", "valve"));
            roundCoordinateDistFileExcel = Convert.ToInt32(BDSQL.searchDataInBD<string>(dbFilePath, "userData", "roundCoordinateDistFileExcel", "name", "valve"));
            roundCoordinateXYFileExcel = Convert.ToInt32(BDSQL.searchDataInBD<string>(dbFilePath, "userData", "roundCoordinateXYFileExcel", "name", "valve"));
            isDrawZoneSearchPL = Convert.ToBoolean( Convert.ToInt32(BDSQL.searchDataInBD<string>(dbFilePath, "userData", "isDrawZoneSearchPL", "name", "valve")));
         }


    
    }
}
    