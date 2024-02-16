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
    internal class ResistanceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Предполагается, что value является вашим объектом, который содержит r и length
            if (value is Edge itemEdge)
            {
                // Предположим, что r и length - это public свойства вашего класса
                double r = itemEdge.r;
                double x = itemEdge.x;
                double r0 = itemEdge.r0;
                double x0 = itemEdge.x0;
                double length = itemEdge.length;
                double resultR = Math.Round(r * length,6);
                double resultX = Math.Round(x * length,6);
                double rezultZ=Math.Round( Math.Sqrt(Math.Pow(resultR, 2)+Math.Pow(resultX, 2)),6);

                // Выполняем умножение
                return resultR +"+j"+resultX +" (Z="+ rezultZ + ")";
            }

            // Возвращаем значение по умолчанию или обработку ошибки, если необходимо
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

