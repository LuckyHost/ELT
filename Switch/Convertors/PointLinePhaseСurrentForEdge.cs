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
    internal class PointLinePhaseСurrentForEdge : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            if (value is Edge itemEdge)
            {

                StringBuilder tempText = new StringBuilder();
                tempText.Append("Фаза А: "+ $"|{itemEdge.Ia.Magnitude:F3}|∠{itemEdge.Ia.Phase * (180 / Math.PI):F2}° ( {itemEdge.Ia.Real:F3}+j{itemEdge.Ia.Imaginary:F3})"+ Environment.NewLine + 
                                "Фаза В: "+ $"|{itemEdge.Ib.Magnitude:F3}|∠{itemEdge.Ib.Phase * (180 / Math.PI):F2}° ( {itemEdge.Ib.Real:F3}+j{itemEdge.Ib.Imaginary:F3})"+ Environment.NewLine + 
                                "Фаза С: "+ $"|{itemEdge.Ic.Magnitude:F3}|∠{itemEdge.Ic.Phase * (180 / Math.PI):F2}° ( {itemEdge.Ic.Real:F3}+j{itemEdge.Ic.Imaginary:F3})"+ Environment.NewLine  
                                );

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
