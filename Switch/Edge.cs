
#region Namespaces


using System;
using System.Xml.Serialization;
using QuikGraph;
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
        public Complex Ia { get; set; }
        [XmlElement("Ib")]
        public Complex Ib { get; set; }
        [XmlElement("Ic")]
        public Complex Ic { get; set; }
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

        //Возвращает полное комплексное сопротивление ПРЯМОЙ последовательности(Z1).
        public Complex GetPositiveSequenceImpedance()
        {
            // Умножаем удельное сопротивление на длину
            return new Complex(this.r * this.length, this.x * this.length);
        }

        /// Возвращает полное комплексное сопротивление НУЛЕВОЙ последовательности (Z0).

        public Complex GetZeroSequenceImpedance()
        {
            // Умножаем удельное сопротивление на длину
            return new Complex(this.r0 * this.length, this.x0 * this.length);
        }

        // Возвращает полное комплексное сопротивление ОБРАТНОЙ последовательности (Z2).
        // Для кабелей оно равно сопротивлению прямой последовательности.
        public Complex GetNegativeSequenceImpedance()
        {
            return GetPositiveSequenceImpedance();
        }

        //Возвращает полное комплексное сопротивление ПРЯМОЙ последовательности нулевого проводника (Zn1).
        public Complex GetPositiveSequenceImpedanceNeutral()
        {
            // Умножаем удельное сопротивление на длину
            return new Complex(this.rN * this.length, this.xN * this.length);
        }

        public Edge(PointLine start, PointLine end, int edgeName, string sourceLine, CableProperties cableProps)
        {
            name = 0;
            startPoint = start;
            endPoint = end;
            cable = sourceLine;
            Ia = Complex.Zero;
            Ib = Complex.Zero;
            Ic = Complex.Zero;
            IDText = new ObjectId();

            if (start != null && end != null)
            {
                length = Math.Round(start.positionPoint.GetDistanceTo(end.positionPoint), 4);

                // Создаем центральную точку
                centerPoint = new PointLine
                {
                    positionPoint = new Point2d((start.positionPoint.X + end.positionPoint.X) / 2, (start.positionPoint.Y + end.positionPoint.Y) / 2)
                };
            }

            // Заполняем свойства из "кэша", а не из БД
            if (cableProps != null)
            {
                this.r = cableProps.R;
                this.x = cableProps.X;
                this.r0 = cableProps.R0;
                this.x0 = cableProps.X0;
                this.rN = cableProps.RN;
                this.xN = cableProps.XN;
                this.Ke = cableProps.Ke;
                this.Ce = cableProps.Ce;
                this.Icrict = cableProps.Icrict;
            }

           
        }

    }

}



