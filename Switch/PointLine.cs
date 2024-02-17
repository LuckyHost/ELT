﻿#region Namespaces



using ElectroTools;
using System;
using System.Xml.Serialization;
using System.ComponentModel;





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
    public class PointLine :INotifyPropertyChanged
    {
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        private int _typeClient;
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
                    TextFun.updateTextById(IDText, name + "\\P" + value.ToString(), 256);
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
                    TextFun.updateTextById(IDText, name + "\\P" + value.ToString(), 256);
                }
            }

        }


        [XmlElement("weightC")]
        public double weightC
        {
            get { return _weightC; }
            set
            {
                if (value >= 0 & typeClient !=3)
                    _weightC = value;
                if (value != null & IDText != null)
                {
                    TextFun.updateTextById(IDText, name + "\\P" + value.ToString(), 256);
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
                    { TextFun.updateColorMtext(this, 201); }
                    if (value == 3)
                    { TextFun.updateColorMtext(this, 256);
                        weightB = 0;
                        weightC = 0;
                        OnPropertyChanged(nameof(weightB));
                        OnPropertyChanged(nameof(weightC));
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



