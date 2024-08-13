
#if nanoCAD
using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
using System.Threading;
using Teigha.DatabaseServices;
#else
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

#endif

namespace ElectroTools
{
    internal class MyOpenDocument
    {
        public static Document doc;
        public static Database dbCurrent;
        public static Editor ed;
        public static CancellationTokenSource cts;
    }
}
