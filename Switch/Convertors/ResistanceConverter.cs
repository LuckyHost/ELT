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
    internal class ResistanceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Предполагается, что value является вашим объектом, который содержит r и length
            if (value is Edge itemEdge)
            {
                Complex resultPositiveImpedance = itemEdge.GetPositiveSequenceImpedance();
                Complex resultZeroImpedance = itemEdge.GetZeroSequenceImpedance();

                // Выполняем умножение
                // Используем форматирование чисел (например, F4 для 4 знаков после запятой) для красивого вывода
                return $"Z₁ : {resultPositiveImpedance.ToElectricalString()}" + Environment.NewLine +
                       $"|Z|∠ ={resultPositiveImpedance.Magnitude:F4}∠{(resultPositiveImpedance.Phase * (180 / Math.PI)):F2}°" + Environment.NewLine +
                       "~~~~~~~~~~~~~~" + Environment.NewLine +
                       $"Z₀ : {resultZeroImpedance.ToElectricalString()}" + Environment.NewLine +
                       $"|Z|∠ ={resultZeroImpedance.Magnitude:F4}∠{(resultZeroImpedance.Phase * (180 / Math.PI)):F2}°";
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

