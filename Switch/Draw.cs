﻿
#region Namespaces

using System.Collections.Generic;
using System.Linq;
using System;



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



        public static ObjectId drawPolyline(List<PointLine> masterListPont, string nameLayer, short color, double ConstantWidth)
        {



            using (DocumentLock doclock = MyOpenDocument.doc.LockDocument())
            {
                using (Transaction tr = MyOpenDocument.dbCurrent.TransactionManager.StartTransaction())
                {
                    BlockTable bt = tr.GetObject(MyOpenDocument.dbCurrent.BlockTableId, OpenMode.ForRead) as BlockTable;

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


            using (DocumentLock doclock = MyOpenDocument.doc.LockDocument())
            {
                // Начало транзакции
                using (Transaction tr = MyOpenDocument.dbCurrent.TransactionManager.StartTransaction())
                {
                    // Открытие таблицы блоков для записи
                    BlockTable bt = tr.GetObject(MyOpenDocument.dbCurrent.BlockTableId, OpenMode.ForWrite) as BlockTable;

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
            using (DocumentLock doclock = MyOpenDocument.doc.LockDocument())
            {

                using (Transaction tr = MyOpenDocument.dbCurrent.TransactionManager.StartTransaction())
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
                            MyOpenDocument.ed.SetCurrentView(view);
                        }
                    }

                    tr.Commit();
                }
            }
        }

        public static ObjectId drawCoordinatePolyline(List<double> listX, List<double> listY /*List<PointLine> masterListPont, string nameLayer, short color, double ConstantWidth*/)
        {

            using (DocumentLock doclock = MyOpenDocument.doc.LockDocument())
            {
                using (Transaction tr = MyOpenDocument.dbCurrent.TransactionManager.StartTransaction())
                {
                    BlockTable bt = tr.GetObject(MyOpenDocument.dbCurrent.BlockTableId, OpenMode.ForRead) as BlockTable;

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
                    // ed.SetImpliedSelection(new ObjectId[] { polyline.ObjectId });
                    return polyline.ObjectId;

                }
            }


        }

        public static double getPolylineArea(ObjectId plId)
        {

            using (Transaction acTrans = MyOpenDocument.dbCurrent.TransactionManager.StartTransaction())
            {
                DBObject obj = acTrans.GetObject(plId, OpenMode.ForRead);

                double area = 0;

                // Проверяем тип полилинии и вычисляем площадь
                if (obj is Polyline)
                {
                    Polyline pl = obj as Polyline;
                    if (pl.Closed) // Проверяем, замкнута ли полилиния
                    {
                        area = pl.Area;
                    }
                    else
                    {
                        MyOpenDocument.ed.WriteMessage("\nВыбранная полилиния не замкнута.");
                    }
                }
                else if (obj is Polyline2d)
                {
                    Polyline2d pl2d = obj as Polyline2d;
                    // Для Polyline2d и Polyline3d необходимо самостоятельно вычислить площадь,
                    // так как свойство Area непосредственно не доступно.
                    // Можно конвертировать их в Polyline с использованием методов AutoCAD.
                    MyOpenDocument.ed.WriteMessage("\nВычисление площади для Polyline2d не поддерживается в данном примере.");
                }
                else if (obj is Polyline3d)
                {
                    Polyline3d pl3d = obj as Polyline3d;
                    // Аналогично Polyline2d
                    MyOpenDocument.ed.WriteMessage("\nВычисление площади для Polyline3d не поддерживается в данном примере.");
                }
                else
                {
                    MyOpenDocument.ed.WriteMessage("\nВыбранный объект не является полилинией.");
                }

                if (area > 0)
                {
                    MyOpenDocument.ed.WriteMessage($"\nПлощадь полилинии: {area}");
                    return area;
                }

                return 0;
            }
        }

        //Для отображение зоны поиска полилинии
        public static void drawZoneSearchPLRactangel(Point3d corner1, Point3d corner2, Database db, Transaction tr)
        {
            BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            // Создаем полилинию с 4 вершинами (прямоугольник)
            Polyline rect = new Polyline();
            rect.AddVertexAt(0, new Point2d(corner1.X, corner1.Y), 0, 0, 0);
            rect.AddVertexAt(1, new Point2d(corner2.X, corner1.Y), 0, 0, 0);
            rect.AddVertexAt(2, new Point2d(corner2.X, corner2.Y), 0, 0, 0);
            rect.AddVertexAt(3, new Point2d(corner1.X, corner2.Y), 0, 0, 0);
            rect.Closed = true;
            rect.Color = Color.FromColorIndex(ColorMethod.ByAci, 1); // Красный цвет

            btr.AppendEntity(rect);
            tr.AddNewlyCreatedDBObject(rect, true);
        }


        public static void drawZoneSearchPLCircle(Point3dCollection points, Database db, Transaction tr, string nameLayer)
        {
            BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            // Создаем полилинию с вершинами из коллекции points
            Polyline polygon = new Polyline();
            for (int i = 0; i < points.Count; i++)
            {
                polygon.AddVertexAt(i, new Point2d(points[i].X, points[i].Y), 0, 0, 0);
            }
            polygon.Closed = true;
            polygon.Color = Color.FromColorIndex(ColorMethod.ByAci, 1); // Красный цвет
            polygon.Layer = nameLayer;
            polygon.Closed = true;

            btr.AppendEntity(polygon);
            tr.AddNewlyCreatedDBObject(polygon, true);
        }


        public static Point3dCollection createCirclePolygon(Point3d center, double radius, int segments)
        {
            Point3dCollection points = new Point3dCollection();
            for (int i = 0; i < segments; i++)
            {
                double angle = 2 * Math.PI * i / segments;
                double x = center.X + radius * Math.Cos(angle);
                double y = center.Y + radius * Math.Sin(angle);
                points.Add(new Point3d(x, y, 0));
            }
            points.Add(points[0]); // Замыкаем многоугольник, добавляя первую точку в конец
            return points;
        }

        public static ObjectId сreatePoint(Point3d position, Database db, Transaction tr,  string nameLayer)
        {
            BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

            using (DBPoint dbPoint = new DBPoint(position))
            {
                btr.AppendEntity(dbPoint);
                dbPoint.Layer = nameLayer;
                tr.AddNewlyCreatedDBObject(dbPoint, true);
                return dbPoint.ObjectId;
            }
        }
    }
}

