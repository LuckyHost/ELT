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
        private double _weightB;
        private double _weightC;
        private bool _isFavorite;


        [XmlAttribute("name")]
        public int name { get; set; }
        [XmlIgnore]
        public Point2d positionPoint { get; set; }
        [XmlElement("weightA")]
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

        [XmlElement("weightB")]
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


        [XmlElement("count")]
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
                    Ko = Math.Round( interpolation.Interpolate(value),3);
                    OnPropertyChanged(nameof(count));
                }
            }

        }

        [XmlElement("Ko")]
        public double Ko
        {
            get { return _Ko; }
            set
            {
                if (value >= 0)
                    _Ko = value;
            }

        }



        [XmlElement("weightC")]
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





        [XmlElement("isLastPoint")]
        public bool isLastPoint { get; set; }

        [XmlElement("tempBoll")]
        public bool tempBoll { get; set; }

        [XmlElement("Ia")]
        public double Ia { get; set; }

        [XmlElement("Ib")]
        public double Ib { get; set; }

        [XmlElement("Ic")]
        public double Ic { get; set; }

        [XmlElement("cos")]
        public double cos { get; set; }

        [XmlElement("typleClient")]

        //public double typeClient { get; set; }
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



        [XmlElement("Ua")]
        public double Ua { get; set; }
        [XmlElement("Ub")]
        public double Ub { get; set; }
        [XmlElement("Uc")]
        public double Uc { get; set; }

        [XmlElement("isFavorite")]
        public bool isFavorite
        {
            get
            {
                return _isFavorite;
            }
            set { _isFavorite = value; }
        }

        [XmlIgnore]
        // [XmlElement("IDText")]
        public ObjectId IDText { get; set; }


        // Свойство для сериализации/десериализации ObjectId
        [XmlElement("IDText")]
        public string SerializedObjectId
        {
            get { return IDText.ToString(); }
            set
            {
                IntPtr myValve = (IntPtr)long.Parse(value);
                IDText = new ObjectId(myValve);
            }
        }




        public PointLine()
        {
            name = 0;
            positionPoint = new Point2d();
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
            typeClient = 3;

        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        


    }





}



