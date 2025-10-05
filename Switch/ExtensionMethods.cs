using NLog.LayoutRenderers.Wrappers;
using System.Globalization;
using System.Numerics;

namespace ElectroTools
{
    public static class ComplexExtensions
    {
        /// <summary>
        /// ����������� ����������� ����� � ������������������ ������ "r � jx".
        /// </summary>
        /// <param name="c">����������� �����.</param>
        /// <param name="decimals">���������� ������ ����� �������.</param>
        /// <returns>������ � ������� "r � jx".</returns>
        public static string ToElectricalString(this Complex c, int decimals = 4)
        {
            // ���������� ���� ��� ������ �����
            string sign = c.Imaginary < 0 ? "-" : "+";

            // ���������� InvariantCulture, ����� ���������� ������������ ������ ���� �����
            string realPart = c.Real.ToString($"F{decimals}", CultureInfo.InvariantCulture);
            string imagPart = System.Math.Abs(c.Imaginary).ToString($"F{decimals}", CultureInfo.InvariantCulture);
            
            // �������� �������� ������
            return   $"{realPart} {sign} j{imagPart}".Replace(".",",");
        }
    }
}