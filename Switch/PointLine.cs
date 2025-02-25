#region Namespaces



using ElectroTools;
using System;
using System.Xml.Serialization;
using System.ComponentModel;
using MathNet.Numerics.Interpolation;
using System.Windows.Forms;







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

    public class PointLine : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private int _typeClient;
        private int _count;
        private double _Ko;
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
        public double Ia { get; set; }

        [XmlIgnore]
        public double Ib { get; set; }

        [XmlIgnore]
        public double Ic { get; set; }


        public double cos { get; set; }

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
        public double Ua { get; set; }
        [XmlIgnore]
        public double Ub { get; set; }
        [XmlIgnore]
        public double Uc { get; set; }


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
            Ia = 0;
            Ib = 0;
            Ic = 0;
            Ua = 0;
            Ub = 0;
            Uc = 0;
            tempBoll = false;
            isLastPoint = false;
            isFavorite = false;
            IDText = ObjectId.Null;
            cos = 0.96;

        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }




    }





}



