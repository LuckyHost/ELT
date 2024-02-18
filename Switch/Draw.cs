
#region Namespaces

using System.Collections.Generic;

#if nanoCAD
using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
using System.Collections.Generic;
using System.Windows.Controls;
using Teigha.Colors;
using Teigha.DatabaseServices;
using Teigha.Geometry;

#else
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;


#endif

#endregion Namespaces

namespace ElectroTools
{
    public static class Draw
    {

        
        public static void drawPolyline(List<PointLine> masterListPont, string nameLayer, short color, double ConstantWidth)
        {

            Editor ed = MyOpenDocument.ed;
            Database dbCurrent = MyOpenDocument.dbCurrent;
            Document doc = MyOpenDocument.doc;

            using (DocumentLock doclock = doc.LockDocument())
            {
                using (Transaction tr = dbCurrent.TransactionManager.StartTransaction())
                {
                    BlockTable bt = tr.GetObject(dbCurrent.BlockTableId, OpenMode.ForRead) as BlockTable;

                    BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    Polyline polyline = new Polyline();

                    foreach (PointLine itemPoint in masterListPont)
                    {
                        polyline.AddVertexAt(polyline.NumberOfVertices, itemPoint.positionPoint, 0, 0, 0);
                    }


                    polyline.Color = Color.FromColorIndex(ColorMethod.ByAci, color); // Color 256 is ByLayer
                    polyline.Layer = nameLayer;
                    polyline.ConstantWidth = ConstantWidth;

                    btr.AppendEntity(polyline);
                    tr.AddNewlyCreatedDBObject(polyline, true);

                    // Commit the transaction
                    tr.Commit();

                }
            }
        }

        public static ObjectId  drawCircle(PointLine itemPoint, string nameLayer)
        {
            Editor ed = MyOpenDocument.ed;
            Database dbCurrent = MyOpenDocument.dbCurrent;
            Document doc = MyOpenDocument.doc;

            using (DocumentLock doclock = doc.LockDocument())
            {
                // Начало транзакции
                using (Transaction tr = dbCurrent.TransactionManager.StartTransaction())
                {
                    // Открытие таблицы блоков для записи
                    BlockTable bt = tr.GetObject(dbCurrent.BlockTableId, OpenMode.ForWrite) as BlockTable;

                    // Открытие записи таблицы блоков для записи
                    BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    // Создание точки центра окружности
                    Point3d center = new Point3d(itemPoint.positionPoint.X, itemPoint.positionPoint.Y, 0);

                    // Создание окружности
                    Circle circle = new Circle(center, Vector3d.ZAxis, 10.0);
                    circle.Layer = nameLayer;

                    // Добавление окружности в блок таблицы записи
                    ObjectId circleId = btr.AppendEntity(circle);
                    tr.AddNewlyCreatedDBObject(circle, true);

                    // Завершение транзакции
                    tr.Commit();
                    return circleId;
                }
            }
        }

    }
}
