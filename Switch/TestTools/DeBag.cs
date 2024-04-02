using System.Text;
using ElectroTools.TestTools;

#if nanoCAD
using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
using Teigha.DatabaseServices;
using Teigha.Runtime;
#else
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;



#endif


namespace ElectroTools
{
    public partial class  DeBag
    {
        static Editor ed = MyOpenDocument.ed;
        static Database dbCurrent = MyOpenDocument.dbCurrent;
        static Document doc = MyOpenDocument.doc;

        [CommandMethod("myDeBag", CommandFlags.UsePickSet |
              CommandFlags.Redraw | CommandFlags.Modal)]
        public void testDeBag()
        {
            WinTools form = new WinTools();
            form.Show();
        }

        public void getInfo()
        {
            using (Transaction trAdding = dbCurrent.TransactionManager.StartTransaction())
            {
                PromptEntityOptions magistral = new PromptEntityOptions("\nВыберите объект для получения ID : ");
                PromptEntityResult perMagistral = ed.GetEntity(magistral);
                if (perMagistral.Status != PromptStatus.OK) { return; }
                Entity Plyline = trAdding.GetObject(perMagistral.ObjectId, OpenMode.ForRead) as Entity;

                ed.WriteMessage("\n  ");
                ed.WriteMessage("!!!!!!!!!!!!!!!!!!!");
                ed.WriteMessage("У выбранного объекта ID:  " + Plyline.ObjectId);
                ed.WriteMessage("!!!!!!!!!!!!!!!!!!!");
                ed.WriteMessage("\n  ");
            }

        }
        public void getSlovari()

        {
            PromptEntityOptions item = new PromptEntityOptions("\nВыберите объект: ");
            PromptEntityResult perItem = ed.GetEntity(item);
        }

        public void ExportSelectedToDxf()
        {

            // Запрос на выбор объектов
            PromptSelectionResult selRes = ed.GetSelection();
            if (selRes.Status != PromptStatus.OK)
            {
                ed.WriteMessage("\nОтмена выбора.");
                return;
            }
            SelectionSet acSSet = selRes.Value;

            using (Transaction acTrans = dbCurrent.TransactionManager.StartTransaction())
            {
                StringBuilder dxfString = new StringBuilder();

                // Перебор всех выбранных объектов
                foreach (SelectedObject acSSObj in acSSet)
                {
                    if (acSSObj != null)
                    {
                        // Получение объекта из выборки
                        DBObject acDbObj = acTrans.GetObject(acSSObj.ObjectId, OpenMode.ForRead);

                        // Получение DXF имени через ObjectClass
                        RXClass objClass = acDbObj.GetRXClass();
                        dxfString.AppendLine($"DXF имя: {objClass.DxfName}");
                        

                        // Дополнительная информация в зависимости от типа объекта
                        // Например, для линии:
                        if (acDbObj is Line)
                        {
                            Line line = acDbObj as Line;
                            dxfString.AppendLine($"Начальная точка: {line.StartPoint.ToString()}, Конечная точка: {line.EndPoint.ToString()}");
                        }
                        // Добавьте здесь обработку других типов объектов по необходимости
                    }
                }

                // Завершение транзакции
                acTrans.Commit();

                // Вывод информации DXF в окно редактора AutoCAD
                ed.WriteMessage(dxfString.ToString());
            }
        }
    }
}
