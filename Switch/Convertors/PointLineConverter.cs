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
    internal class PointLineConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Предполагается, что value является вашим объектом, который содержит r и length
            if (value is PowerLine itemPowerLine)
            {


                StringBuilder tempText = new StringBuilder();
                tempText.Append(string.Join(" ", itemPowerLine.points.Select(itemPoint => itemPoint.name)));

                return tempText;
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

