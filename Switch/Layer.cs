
#region Namespaces






#if nanoCAD
using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
using System.Windows.Controls;
using Teigha.DatabaseServices;
using Color = Teigha.Colors.Color;

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
using Autodesk.AutoCAD.Windows;

#endif

#endregion Namespaces

namespace ElectroTools
{
    public static class Layer
    {

        //Создание слоев
        public static void creatLayer(string Name, byte ColorR, byte ColorG, byte ColorB)
        {
            Editor ed = MyOpenDocument.ed;
            Database dbCurrent = MyOpenDocument.dbCurrent;
            Document doc = MyOpenDocument.doc;
            using (DocumentLock docloc = doc.LockDocument())
            {
                using (Transaction trAdding = dbCurrent.TransactionManager.StartTransaction())
                {

                    LayerTable layerTable = trAdding.GetObject(dbCurrent.LayerTableId, OpenMode.ForWrite) as LayerTable;

                    if (!layerTable.Has(Name))
                    {
                        // Создание слоя
                        LayerTableRecord acLyrTblRec = new LayerTableRecord();
                        acLyrTblRec.Name = Name;
                        acLyrTblRec.Color = Color.FromRgb(ColorR, ColorG, ColorB);
                        layerTable.UpgradeOpen();
                        ObjectId acObjId = layerTable.Add(acLyrTblRec);
                        trAdding.AddNewlyCreatedDBObject(acLyrTblRec, true);
                        ed.WriteMessage("\nСлой создан: " + Name + " !!!Не удаляйте данный слой!!");
                    }
                    else
                    {
                        // ed.WriteMessage("\nСлой уже существует: " + Name);
                    }
                    trAdding.Commit();
                }
            }


        }


        public static void deleteObjectsOnLayer(string layerNameDelete)
        {

            Editor ed = MyOpenDocument.ed;
            Database dbCurrent = MyOpenDocument.dbCurrent;
            Document doc = MyOpenDocument.doc;

            TypedValue[] filterlist = new TypedValue[1];
            // Фильтр по имени слоя
            filterlist[0] = new TypedValue(8, layerNameDelete);

            SelectionFilter filter = new SelectionFilter(filterlist);
            PromptSelectionResult selRes = ed.SelectAll(filter);

            if (selRes.Status != PromptStatus.OK)
            {
                // ed.WriteMessage("\nОшибка метода selectAll");
                return;
            }

            ObjectId[] ids = selRes.Value.GetObjectIds();

            using (DocumentLock docloc = doc.LockDocument())
            {

                using (Transaction tr = dbCurrent.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId objectId in ids)
                    {
                        Entity entity = tr.GetObject(objectId, OpenMode.ForWrite) as Entity;

                        if (entity != null && !entity.IsErased)
                        {
                            // Объект не стерт, можно его удалить
                            entity.Erase();
                        }
                        // Если объект уже стерт, пропускаем его
                    }

                    tr.Commit();
                }
            }

            ed.Regen(); // Обновляем отображение
            ed.WriteMessage("Все объекты со слоя " + layerNameDelete + " удалены.");

        }
    }
}
