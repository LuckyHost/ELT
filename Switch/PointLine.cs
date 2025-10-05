#region Namespaces



using ElectroTools;
using System;
using System.Xml.Serialization;
using System.ComponentModel;
using MathNet.Numerics.Interpolation;
using System.Windows.Forms;
using System.Numerics;








#if nanoCAD
using Application = HostMgd.ApplicationServices.Application;
using Color = Teigha.Colors.Color;
using HostMgd.ApplicationServices;
using Teigha.DatabaseServices;
using HostMgd.EditorInput;
using Teigha.Geometry;
using Teigha.Runtime;
using Teigha.Colors;
using Exception = Teigha.Runtime.Exception;
#else
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Color = Autodesk.AutoCAD.Colors.Color;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Colors;
using Exception = Autodesk.AutoCAD.Runtime.Exception;
#endif
#endregion Namespaces



namespace ElectroTools
{


    [Serializable]

    public class PointLine : INotifyPropertyChanged, IEquatable<PointLine>
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private int _typeClient;
        private int _count;
        private double _Ko;
        private double _cos;
        private double _weightA;
        //Безвыходность
        [XmlElement("weightB")]
        public double _weightB;
        [XmlElement("weightC")]
        public double _weightC;
        private bool _isFavorite;



        public int name { get; set; }
        [XmlIgnore]
        public Point2d positionPoint { get; set; }

        public double weightA
        {
            get { return _weightA; }
            set
            {
                if (value >= 0)
                    _weightA = value;

                if (value != null & IDText != null)
                {
                    if (_typeClient == 3)
                    {
                        Text.updateTextById(IDText, name + "\\P" + value.ToString(), 256);
                    }
                    else
                    {
                        Text.updateTextById(IDText, name + "\\P" + value.ToString() + "; " + _weightB + "; " + _weightC, 201);
                    }
                }
            }

        }

        [XmlIgnore] 
        public double weightB
        {
            get { return _weightB; }
            set
            {
                if (value >= 0 & typeClient != 3)
                    _weightB = value;


                if (value != null & IDText != null)
                {
                    if (_typeClient == 3)
                    {
                        //Text.updateTextById(IDText, name + "\\P" +  value.ToString(), 256);
                    }
                    else
                    {
                        Text.updateTextById(IDText, name + "\\P" + _weightA + "; " + value.ToString() + "; " + _weightC, 201);
                    }

                }
            }
        }

        [XmlIgnore]
        public double weightC
        {
            get { return _weightC; }
            set
            {
                if (value >= 0 & typeClient != 3)
                    _weightC = value;

                if (value != null & IDText != null)
                {

                    if (_typeClient == 3)
                    {
                        //Text.updateTextById(IDText, name + "\\P"  + value.ToString(), 256);
                    }
                    else
                    {
                        Text.updateTextById(IDText, name + "\\P" + _weightA + "; " + _weightB + "; " + value.ToString(), 201);
                    }

                }
            }

        }



        public int count
        {
            get { return _count; }
            set
            {
                if (value >= 0)
                {
                    _count = value;
                    //Коээфицент интерполяции
                    double[] x = { 1, 2, 3, 5, 7, 10, 15, 20, 50, 100, 200, 500 };
                    double[] y = { 1, 0.75, 0.64, 0.53, 0.47, 0.42, 0.37, 0.34, 0.27, 0.24, 0.20, 0.18 };
                    IInterpolation interpolation = LinearSpline.InterpolateSorted(x, y);
                    Ko = Math.Round(interpolation.Interpolate(value), 3);
                    OnPropertyChanged(nameof(count));
                }
            }

        }


        public double Ko
        {
            get { return _Ko; }
            set
            {
                if (value >= 0)
                    _Ko = value;
            }

        }



        [XmlIgnore]
        public bool isLastPoint { get; set; }

        [XmlIgnore]
        public bool tempBoll { get; set; }

        [XmlIgnore]
        public Complex Ia { get; set; }

        [XmlIgnore]
        public Complex Ib { get; set; }

        [XmlIgnore]
        public Complex Ic { get; set; }


        public double cos
        {
            get { return _cos; }
            set
            {
                // 2. Проверяем, что входящее значение находится в допустимом диапазоне
                if (value >= 0.1 && value <= 1.0)
                {
                    _cos = value;
                }
                else
                {
                }
            }
        }

        public int typeClient
        {
            get { return _typeClient; }
            set
            {
                if (value == 1 | value == 3)
                {
                    _typeClient = value;

                    if (value == 1)
                    {
                        Text.updateColorMtext(this, 201);
                    }
                    if (value == 3)
                    {
                        Text.updateColorMtext(this, 256);
                        _weightB = 0;
                        _weightC = 0;
                        OnPropertyChanged(nameof(typeClient));
                    }
                }
            }
        }



        [XmlIgnore]
        public Complex Ua { get; set; }
        [XmlIgnore]
        public Complex Ub { get; set; }
        [XmlIgnore]
        public Complex Uc { get; set; }


        public bool isFavorite
        {
            get
            {
                return _isFavorite;
            }
            set { _isFavorite = value; }
        }

        [XmlIgnore]
        public ObjectId IDText { get; set; }


        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool Equals(PointLine other)
        {
            {
                if (other is null) return false;
                return Math.Abs(positionPoint.X - other.positionPoint.X) < 1e-6 && Math.Abs(positionPoint.Y - other.positionPoint.Y) < 1e-6;
            }
        }

        public override bool Equals(object obj) => Equals(obj as PointLine);

        public override int GetHashCode() => HashCode.Combine(positionPoint.X, positionPoint.Y);

        public static bool operator ==(PointLine left, PointLine right) => Equals(left, right);

        public static bool operator !=(PointLine left, PointLine right) => !Equals(left, right);

        public void UpdatePhaseCurrents(double nominalLineVoltage)
        {
            // Углы для фаз B и C относительно фазы A (в радианах)
            double angleB_rad = -120 * (Math.PI / 180.0); // -120 градусов
            double angleC_rad = 120 * (Math.PI / 180.0);  // +120 градусов

            

            // --- СЛУЧАЙ 1: ОДНОФАЗНАЯ НАГРУЗКА ---
            // Предполагаем, что однофазный потребитель подключен к фазе А.
            if (this.typeClient == 1)
            {
                // Расчет для фазы A
                if (this.weightA > 0)
                {
                    double powerA = (this.weightA* count*Ko) * 1000; // кВт в Вт
                                                         // I = P / (Uф * cos(φ))
                    double currentMagA = powerA / (nominalLineVoltage/Math.Sqrt(3) * this.cos);
                    double angleA = Math.Acos(this.cos); 

                    this.Ia = Complex.FromPolarCoordinates(currentMagA, angleA);
                }
                else
                {
                    this.Ia = Complex.Zero;
                }


                // Расчет для фазы B
                if (this.weightB > 0)
                {
                    double powerB = (this.weightB * count * Ko) * 1000; // кВт в Вт
                                                         // I = P / (Uф * cos(φ))
                    double currentMagB = powerB / (nominalLineVoltage / Math.Sqrt(3) * this.cos);
                    double angleB = Math.Acos(this.cos) + angleB_rad; // Сдвигаем угол на -120°; 

                    this.Ib = Complex.FromPolarCoordinates(currentMagB, angleB);
                }
                else
                {
                    this.Ib = Complex.Zero;
                }


                // Расчет для фазы C
                if (this.weightC > 0)
                {
                    double powerC = (this.weightC * count * Ko) * 1000; // кВт в Вт
                    // I = P / (Uф * cos(φ))
                    double currentMagC = powerC / (nominalLineVoltage / Math.Sqrt(3) * this.cos);
                    double angleC = Math.Acos(this.cos) + angleC_rad; // Сдвигаем угол на -120°; 

                    this.Ic = Complex.FromPolarCoordinates(currentMagC, angleC);
                }
                else
                {
                    this.Ic = Complex.Zero;
                }

            }

            // --- СЛУЧАЙ 2: ТРЕХФАЗНАЯ НАГРУЗКА ---
            else
            {
                

                // Расчет для фазы A
                if (this.weightA > 0)
                {
                    double powerA = (this.weightA * count * Ko) * 1000;
                    double currentMagA = powerA / (Math.Sqrt(3)*nominalLineVoltage * this.cos);
                    double angleA = Math.Acos(this.cos);
                    this.Ia = Complex.FromPolarCoordinates(currentMagA, angleA);
                }
                else
                {
                    this.Ia = Complex.Zero;
                }

                // Расчет для фазы B
                if (this.weightA > 0)
                {
                    double powerB = (this.weightA * count * Ko) * 1000;
                    double currentMagB = powerB / (Math.Sqrt(3)*nominalLineVoltage * this.cos);
                    double angleB = Math.Acos(this.cos) + angleB_rad; // Сдвигаем угол на -120°
                    this.Ib = Complex.FromPolarCoordinates(currentMagB, angleB);
                }
                else
                {
                    this.Ib = Complex.Zero;
                }

                // Расчет для фазы C
                if (this.weightA > 0)
                {
                    double powerC = (this.weightA * count * Ko) * 1000;
                    double currentMagC = powerC / (Math.Sqrt(3)*nominalLineVoltage * this.cos);
                    double angleC = Math.Acos(this.cos) + angleC_rad; // Сдвигаем угол на +120°
                    this.Ic = Complex.FromPolarCoordinates(currentMagC, angleC);
                }
                else
                {
                    this.Ic = Complex.Zero;
                }
            }
        }


        public PointLine()
        {
            name = 0;
            positionPoint = new Point2d();
            typeClient = 3;
            count = 1;
            Ko = 1;
            weightA = 0;
            weightB = 0;
            weightC = 0;
            Ia = Complex.Zero;
            Ib = Complex.Zero;
            Ic = Complex.Zero;
            Ua = Complex.Zero;
            Ub = Complex.Zero;
            Uc = Complex.Zero;
            tempBoll = false;
            isLastPoint = false;
            isFavorite = false;
            IDText = ObjectId.Null;
            cos = 0.96;

        }
        
    }





}



