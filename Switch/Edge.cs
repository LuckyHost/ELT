
#region Namespaces


using System;
using System.Xml.Serialization;
using QuikGraph;





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
    public class Edge : IEdge<PointLine>
    {
        [XmlAttribute("name")]
        public int name { get; set; }
        //[XmlElement("startPoint")]
        [XmlIgnore]
        public PointLine startPoint { get; set; }
        //[XmlElement("centerPoint")]
        [XmlIgnore]
        public PointLine centerPoint { get; set; }
        //  [XmlElement("endPoint")]
        [XmlIgnore]
        public PointLine endPoint { get; set; }
        [XmlElement("cable")]
        public string cable { get; set; }
        [XmlElement("r")]
        public double r { get; set; }

        [XmlElement("x")]
        public double x { get; set; }

        [XmlElement("r0")]
        public double r0 { get; set; }

        [XmlElement("x0")]
        public double x0 { get; set; }

        [XmlElement("rN")]
        public double rN { get; set; }

        [XmlElement("xN")]
        public double xN { get; set; }


        [XmlElement("Ce")]
        public double Ce { get; set; }

        [XmlElement("Ke")]
        public double Ke { get; set; }


        [XmlElement("length")]
        public double length { get; set; }
        [XmlElement("Icrict")]
        public double Icrict { get; set; }
        [XmlElement("Ia")]
        public double Ia { get; set; }
        [XmlElement("Ib")]
        public double Ib { get; set; }
        [XmlElement("Ic")]
        public double Ic { get; set; }
        [XmlIgnore]
        //[XmlElement("IDText")]
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

        //Начальная вершина ребра.QuikGraph будет использовать это свойство.
        public PointLine Source => this.startPoint;

        // Конечная вершина ребра. QuikGraph будет использовать это свойство.
        public PointLine Target => this.endPoint;

        public Edge()
        {
            name = 0;
            startPoint = new PointLine();
            centerPoint = new PointLine();
            endPoint = new PointLine();
            cable = "";
            r = 0;
            x = 0;
            r0 = 0;
            x0 = 0;
            rN = 0;
            xN = 0;
            Ke = 1;
            Ce = 1;
            length = 0;
            Icrict = 0;
            Ia = 0;
            IDText = new ObjectId();
        }

    }

}



