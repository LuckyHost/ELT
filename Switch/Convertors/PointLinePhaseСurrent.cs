using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ElectroTools
{
    internal class PointLinePhaseСurrent : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            if (value is Edge itemEdge)
            {


                StringBuilder tempText = new StringBuilder();
                tempText.Append("Фаза А: "+ itemEdge.Ia + Environment.NewLine + "Фаза В: "  + itemEdge.Ib + Environment.NewLine + "Фаза С: " + itemEdge.Ic);

                return tempText;
            }

            // Возвращаем значение по умолчанию или обработку ошибки, если необходимо
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
