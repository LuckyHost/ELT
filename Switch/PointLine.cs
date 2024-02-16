#region Namespaces



using ElectroTools;
using System;
using System.Xml.Serialization;




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
        public class PointLine
        {
            private int _typeClient;
            private double _weight;
            private bool _isFavorite;
            [XmlAttribute("name")]
            public int name { get; set; }
            [XmlIgnore]
            public Point2d positionPoint { get; set; }
            [XmlElement("weight")]
            public double weight
            {
                get { return _weight; }
                set
                {
                    if (value >= 0)
                        _weight = value;
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

            [XmlElement("I")]
            public double I { get; set; }

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
                        else
                        { TextFun.updateColorMtext(this, 256); }
                    }
                }
            }


            [XmlElement("tempData")]
            public double tempData { get; set; }

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
                weight = 0;
                I = 0;
                tempData = 0;
                tempBoll = false;
                isLastPoint = false;
                isFavorite = false;
                IDText = ObjectId.Null;
                cos = 0.96;
                typeClient = 3;

            }


        }



    }



