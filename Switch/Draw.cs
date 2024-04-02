
#region Namespaces

using System.Collections.Generic;
using System.Linq;


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

        static Editor ed = MyOpenDocument.ed;
        static Database dbCurrent = MyOpenDocument.dbCurrent;
        static Document doc = MyOpenDocument.doc;

        public static ObjectId drawPolyline(List<PointLine> masterListPont, string nameLayer, short color, double ConstantWidth)
        {



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
                    return polyline.ObjectId;

                }
            }
        }

        public static ObjectId drawCircle(PointLine itemPoint, string nameLayer)
        {


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

        public static void ZoomToEntity(ObjectId entityId, double zoomPercent)
        {
            using (DocumentLock doclock = doc.LockDocument())
            {

                using (Transaction tr = dbCurrent.TransactionManager.StartTransaction())
                {
                    Entity entity = tr.GetObject(entityId, OpenMode.ForRead) as Entity;

                    if (entity != null)
                    {
                        Extents3d extents = entity.GeometricExtents;

                        // Определение точек пределов объекта
                        Point3d minPoint = extents.MinPoint;
                        Point3d maxPoint = extents.MaxPoint;

                        // Создание новой записи представления
                        using (ViewTableRecord view = new ViewTableRecord())
                        {
                            // Задание пределов представления
                            view.CenterPoint = new Point2d((minPoint.X + maxPoint.X) / 2, (minPoint.Y + maxPoint.Y) / 2);
                            view.Height = (maxPoint.Y - minPoint.Y) * zoomPercent;
                            view.Width = (maxPoint.X - minPoint.X) * zoomPercent;

                            // Установка представления текущим
                            ed.SetCurrentView(view);
                        }
                    }

                    tr.Commit();
                }
            }
        }

        public static ObjectId drawCoordinatePolyline(List<double> listX, List<double> listY /*List<PointLine> masterListPont, string nameLayer, short color, double ConstantWidth*/)
        {

            using (DocumentLock doclock = doc.LockDocument())
            {
                using (Transaction tr = dbCurrent.TransactionManager.StartTransaction())
                {
                    BlockTable bt = tr.GetObject(dbCurrent.BlockTableId, OpenMode.ForRead) as BlockTable;

                    BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    Polyline polyline = new Polyline();


                    for (int i = 0; i < listX.Count; i++)
                    {

                        polyline.AddVertexAt(polyline.NumberOfVertices, new Point2d(listX[i], listY[i]), 0, 0, 0);
                    }


                    // polyline.Color = Color.FromColorIndex(ColorMethod.ByAci, color); // Color 256 is ByLayer
                    //polyline.Layer = nameLayer;
                    // polyline.ConstantWidth = ConstantWidth;

                    btr.AppendEntity(polyline);
                    tr.AddNewlyCreatedDBObject(polyline, true);

                    // Commit the transaction
                    tr.Commit();
                    ZoomToEntity(polyline.ObjectId, 4);
                    ed.SetImpliedSelection(new ObjectId[] { polyline.ObjectId });
                    return polyline.ObjectId;

                }
            }


        }
    }
}
