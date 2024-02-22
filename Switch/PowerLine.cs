#region Namespaces



using ElectroTools;
using System;
using System.Collections.Generic;
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
   
    public class PowerLine
    {
        private string _name = "";

        
        public string name
        {
            get { return _name; }
            set
            {
                if (value != null & IDText != null)
                {
                    _name = value;
                    Text.updateTextById(IDText, _name, 256);
                }
            }
        }



        [XmlIgnore]
        public List<PointLine> points { get; set; }

        [XmlIgnore]
        public ObjectId IDLine { get; set; }

        // Свойство для сериализации/десериализации ObjectId
        [XmlIgnore]
        public string SerializedObjectId
        {
            get { return IDLine.ToString(); }
            set
            {
                IntPtr myValve = (IntPtr)long.Parse(value);
                IDLine = new ObjectId(myValve);
            }
        }

        [XmlIgnore]
        public PowerLine parent { get; set; }

        [XmlIgnore]
        public PointLine parentPoint { get; set; }

        [XmlIgnore]
        public PointLine endPoint { get; set; }

        [XmlIgnore]
        public List<PowerLine> children { get; set; }
        [XmlIgnore]
        public double lengthLine { get; set; }
        [XmlIgnore]
        public string cable { get; set; }
        [XmlIgnore]
        public List<Edge> Edges { get; set; }
        [XmlIgnore]
        public double Icrict { get; set; }

        [XmlIgnore]
        //[XmlElement("IDText")]
        public ObjectId IDText { get; set; }


        // Свойство для сериализации/десериализации ObjectId
        [XmlIgnore]
        public string SerializedObjectIdText
        {
            get { return IDText.ToString(); }
            set
            {
                IntPtr myValve = (IntPtr)long.Parse(value);
                IDText = new ObjectId(myValve);
            }
        }





        public PowerLine()
        {
            name = "";
            IDLine = ObjectId.Null;
            lengthLine = 0.0;
            points = new List<PointLine>();
            parentPoint = new PointLine();
            endPoint = new PointLine();
            Edges = new List<Edge>();
            cable = "-";
            Icrict = 0;
            IDText = ObjectId.Null;


        }

    }

}



