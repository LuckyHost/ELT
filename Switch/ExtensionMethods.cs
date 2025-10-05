using NLog.LayoutRenderers.Wrappers;
using System.Globalization;
using System.Numerics;

namespace ElectroTools
{
    public static class ComplexExtensions
    {
        /// <summary>
        /// Преобразует комплексное число в электротехнический формат "r ± jx".
        /// </summary>
        /// <param name="c">Комплексное число.</param>
        /// <param name="decimals">Количество знаков после запятой.</param>
        /// <returns>Строка в формате "r ± jx".</returns>
        public static string ToElectricalString(this Complex c, int decimals = 4)
        {
            // Определяем знак для мнимой части
            string sign = c.Imaginary < 0 ? "-" : "+";

            // Используем InvariantCulture, чтобы десятичным разделителем всегда была точка
            string realPart = c.Real.ToString($"F{decimals}", CultureInfo.InvariantCulture);
            string imagPart = System.Math.Abs(c.Imaginary).ToString($"F{decimals}", CultureInfo.InvariantCulture);
            
            // Собираем итоговую строку
            return   $"{realPart} {sign} j{imagPart}".Replace(".",",");
        }
    }
}