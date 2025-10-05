using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
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
            
            if (value is Complex itemComplex)
            {

                StringBuilder tempText = new StringBuilder();
                tempText.Append(
                                $"|{itemComplex.Magnitude:F3}|∠{itemComplex.Phase * (180 / Math.PI):F2}°" + Environment.NewLine  +
                                $"( {itemComplex.ToElectricalString()} )" 
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
