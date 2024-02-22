
#define NCAD 

using System;
using System.Collections.Generic;
using System.Xml.Serialization;




#if NCAD
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

namespace ElectroTools
{


    [Serializable]
    
    public class BD
    {

        
        public List<PowerLine> listPowerLine { get; set; }

        public List<PointLine> listPointLine { get; set; }

        [XmlIgnore]
        public List<Edge> listEdge { get; set; }

    }

}



