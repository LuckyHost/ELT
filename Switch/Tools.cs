
#region Namespaces


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Windows.Forms.Integration;
using System.Windows;
using ControlzEx.Standard;
using Teigha.Colors;



#if nanoCAD
using Application = HostMgd.ApplicationServices.Application;
using HostMgd.ApplicationServices;
using Teigha.DatabaseServices;
using HostMgd.EditorInput;
using Teigha.Geometry;
using Teigha.Runtime;
using Exception = Teigha.Runtime.Exception;
using HostMgd.Windows;

#else
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Exception = Autodesk.AutoCAD.Runtime.Exception;
using Autodesk.AutoCAD.Windows;

#endif

#endregion Namespaces

namespace ElectroTools
{

    public partial class Tools : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

         Editor oldEd;
        //Для передачи Lock состояния
        private MyData _myData;
        private NetWork _netWork = new NetWork();
        private string _pathDLLFile;
        private List<PointLine> _listPoint = new List<PointLine>();
        private List<Edge> _listEdge = new List<Edge>();
        private List<PowerLine> _listPowerLine = new List<PowerLine>();


        public Tools()
        {
            
            MyOpenDocument.ed = Application.DocumentManager.MdiActiveDocument.Editor;
            MyOpenDocument.doc = Application.DocumentManager.MdiActiveDocument;
            MyOpenDocument.dbCurrent = Application.DocumentManager.MdiActiveDocument.Database;


            List<string> assemblies = new List<string>
            {
                "MaterialDesignColors.dll",
                "MaterialDesignThemes.MahApps.dll",
                "MaterialDesignThemes.Wpf.dll",
                "Newtonsoft.Json.dll",
            };

            var getListAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            List<string> assembliesToLoadCopy = new List<string>(assemblies);

            foreach (string assemblyName in assemblies)
            {
                if (getListAssemblies.Any(assembly => assembly.GetName().Name.StartsWith(assemblyName.Substring(0, assemblyName.Length - 4))))
                {
                    assembliesToLoadCopy.Remove(assemblyName);
                }

            }
            //Загрузка dll
            string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            foreach (string assembly in assembliesToLoadCopy)
            {
                Assembly.LoadFrom(Path.Combine(currentDir, assembly));
            }


            //Получить Путь где лежит DLL
            Assembly assemblyBD = Assembly.GetExecutingAssembly();
            pathDLLFile = assemblyBD.Location;

            //Получаем путь для BD
            int lastDelimiterIndex = pathDLLFile.LastIndexOf("\\");
            if (lastDelimiterIndex != -1)
            {
                dbFilePath = pathDLLFile.Substring(0, lastDelimiterIndex).Trim();
            }
            dbFilePath = @dbFilePath + "\\DataBD.db";


            //Обновить пользовательские данные
            UserData.updateUserData(dbFilePath);
        }




       


        public List<PointLine> listPoint
        {
            get { return _listPoint; }
            set
            {
                if (_listPoint != value)
                {
                    _listPoint = value;
                    OnPropertyChanged(nameof(listPoint));
                }
            }
        }




        public List<PowerLine> listPowerLine
        {
            get { return _listPowerLine; }
            set
            {
                if (_listPowerLine != value)
                {
                    _listPowerLine = value;
                    OnPropertyChanged(nameof(listPowerLine));
                }
            }
        }
        public List<Point2d> listPointXY = new List<Point2d>();
        public List<Edge> listEdge
        {
            get { return _listEdge; }
            set
            {
                if (_listEdge != value)
                {
                    _listEdge = value;
                    OnPropertyChanged(nameof(listEdge));
                }
            }
        }

        public List<PointLine> listLastPoint = new List<PointLine>();
        //public  string pathDLLFile = "";
        public string dbFilePath = "";

        public string pathDLLFile
        {
            get { return _pathDLLFile; }
            set
            {
                if (_pathDLLFile != value)
                {
                    _pathDLLFile = value;
                    OnPropertyChanged(nameof(pathDLLFile));

                }
            }
        }

        public int[,] matrixInc;
        public int[,] matrixSmej;
        public int[,] matrixVertexWeight;
        public double[,] matrixEdjeWeight;


        /**
        [CommandMethod("фв", CommandFlags.UsePickSet |
                       CommandFlags.Redraw | CommandFlags.Modal)] // название команды, вызываемой в Autocad */

        

        public void addInfo()
        {

            //подписывается на обновление чертежа
            Application.DocumentManager.DocumentActivated += DocumentActivatedEventHandler;

            oldEd = Application.DocumentManager.MdiActiveDocument.Editor;


            //Обновляем при запуске анализа
            MyOpenDocument.ed = Application.DocumentManager.MdiActiveDocument.Editor;
            MyOpenDocument.doc = Application.DocumentManager.MdiActiveDocument;
            MyOpenDocument.dbCurrent = Application.DocumentManager.MdiActiveDocument.Database;



            OnPropertyChanged(nameof(MyOpenDocument.ed));

            
            //Обновить пользовательские данные
            UserData.updateUserData(dbFilePath);

            //Нужное
            listPoint.Clear();
            listPowerLine.Clear();
            listPointXY.Clear();
            listEdge.Clear();
            listLastPoint.Clear();
            matrixInc = null; matrixSmej = null; matrixVertexWeight = null; matrixEdjeWeight = null;


            int j = 0;

            int defult = BDSQL.searchAllDataInBD(dbFilePath, "text", "default").IndexOf("true") + 1;
            string sizeTextPoint = Text.creatPromptKeywordOptions("Выберите высотку текста для вершины и ребра", BDSQL.searchAllDataInBD(dbFilePath, "text", "size"), defult);
            if (string.IsNullOrEmpty(sizeTextPoint)) { return; }
            string sizeTextLine = Text.creatPromptKeywordOptions("Выберите высотку текста названия линии", BDSQL.searchAllDataInBD(dbFilePath, "text", "size"), defult);
            if (string.IsNullOrEmpty(sizeTextLine)) { return; }



            //Создание слоев
            Layer.creatLayer("Узлы_Saidi_Saifi_Makarov.D", 0, 127, 0);
            Layer.creatLayer("Граф_Saidi_Saifi_Makarov.D", 76, 153, 133);
            Layer.creatLayer("НазванияЛиний_Saidi_Saifi_Makarov.D", 0, 191, 255);
            Layer.creatLayer("Ребра_Saidi_Saifi_Makarov.D", 255, 191, 0);
            Layer.creatLayer("TKZ_Makarov.D", 255, 0, 0);
            Layer.creatLayer("Напряжение_Makarov.D", 0, 255, 63);


            //Удалить все объекты со слоев
            Layer.deleteObjectsOnLayer("Узлы_Saidi_Saifi_Makarov.D");
            Layer.deleteObjectsOnLayer("Граф_Saidi_Saifi_Makarov.D");
            Layer.deleteObjectsOnLayer("НазванияЛиний_Saidi_Saifi_Makarov.D");
            Layer.deleteObjectsOnLayer("Ребра_Saidi_Saifi_Makarov.D");
            Layer.deleteObjectsOnLayer("TKZ_Makarov.D");
            Layer.deleteObjectsOnLayer("Напряжение_Makarov.D");


            using (DocumentLock docloc = MyOpenDocument.doc.LockDocument())
            {
                using (Transaction trAdding = MyOpenDocument.dbCurrent.TransactionManager.StartTransaction())
                {
                    //Создаем магистраль
                    PowerLine magistralLine = creatMagistral(MyOpenDocument.ed, trAdding, listPoint, listPointXY, listPowerLine);

                    //Ищем все отпайки у магистрали и у их детей и делает листпоинт
                    searchPlyline(MyOpenDocument.ed, magistralLine, trAdding, listPoint, listPointXY, j);


                    //Собрать весь список PowerLine
                    listPowerLine = creatListPowerLine(magistralLine);

                    foreach (PowerLine item in listPowerLine)
                    {
                        //Проверка на наличие в БД
                        if (!BDSQL.searchAllDataInBD(dbFilePath, "cable", "name").Contains(item.cable))
                        {
                            _myData.isLock = false;
                            _myData.isLoadProcessAnim = true;
                            Draw.ZoomToEntity(item.IDLine, 5);
                            MyOpenDocument.ed.SetImpliedSelection(new ObjectId[] { item.IDLine });
                            MessageBox.Show("Элемент отсутствует в базе данных, добавьте его и повторите попытку.");
                            return;

                        };
                    }

                    //Добавление родителя точки и обновление списка
                    addPointPerent(listPowerLine, listPoint, trAdding);

                    //Добавление последней токи в Line и обновление списка
                    addEndPoint(listPowerLine, listPoint, trAdding);

                    //Создать список ребер
                    listEdge = creatListEdegs(listPowerLine);

                    //
                    insertEdgeInPowerLine(listPowerLine, listEdge);

                    //Создает list bool последних поинтов
                    creatListLastPoint();

                    //Создание матрици инкрименции
                    matrixInc = CreatMatrixInc(listPoint, listEdge);

                    //Создание матрици смежности
                    matrixSmej = сreatMatrixSmej(listPoint, listEdge);

                    //Создает наименования у каждого узла
                    Text.creatTextFromKnot("Узлы_Saidi_Saifi_Makarov.D", listPoint, sizeTextPoint);

                    //Создает наименования у каждого ребра
                    Text.creatTextFromEdge("Ребра_Saidi_Saifi_Makarov.D", listEdge, sizeTextPoint);

                    //Создает наименования у линии
                    Text.creatTextFromLine("НазванияЛиний_Saidi_Saifi_Makarov.D", listPowerLine, sizeTextLine);

                    // Иначе рабоать не будет				
                    trAdding.Commit();

                }
            }

            OnPropertyChanged(nameof(listPoint));
            OnPropertyChanged(nameof(listEdge));
            OnPropertyChanged(nameof(listPowerLine));

            //Подписываемся на все PointLine
            listPoint.ForEach(it => it.PropertyChanged += Tools_PropertyChanged);


        }






        // Fun Для получения PointLine 
        [CommandMethod("фв3", CommandFlags.UsePickSet |
                       CommandFlags.Redraw | CommandFlags.Modal)] // название команды, вызываемой в Autocad
        public void getPointLine()
        {
            using (Transaction trAdding = MyOpenDocument.dbCurrent.TransactionManager.StartTransaction())
            {
                PromptEntityOptions magistral = new PromptEntityOptions("\n Выберите объект для получения Информации Линии : ");
                PromptEntityResult perMagistral = MyOpenDocument.ed.GetEntity(magistral);
                if (perMagistral.Status != PromptStatus.OK) { return; }
                Polyline Plyline = trAdding.GetObject(perMagistral.ObjectId, OpenMode.ForRead) as Polyline;

                foreach (PowerLine itemLine in listPowerLine)
                {

                    if (itemLine.IDLine == perMagistral.ObjectId)
                    {
                        MyOpenDocument.ed.WriteMessage("\n  ");
                        MyOpenDocument.ed.WriteMessage("~~~~~~~~~~~~~~~~");
                        MyOpenDocument.ed.WriteMessage("\n У выбранного объекта ID:  " + itemLine.IDLine);
                        MyOpenDocument.ed.WriteMessage("\n У выбранного объекта имя:  " + itemLine.name);
                        MyOpenDocument.ed.WriteMessage("\n У выбранного объекта марка провода:  " + itemLine.cable);
                        MyOpenDocument.ed.WriteMessage("\n У выбранного объекта допустимый ток :  " + itemLine.Icrict);
                        MyOpenDocument.ed.WriteMessage("\n У выбранного объекта длинна:  " + itemLine.lengthLine);
                        MyOpenDocument.ed.WriteMessage("\n У выбранного объекта родитель:  " + itemLine.parent.name);
                        MyOpenDocument.ed.WriteMessage("\n Отпайка от вершины родителя:  " + itemLine.parentPoint.name);
                        MyOpenDocument.ed.WriteMessage("\n Последняя вершина объекта:  " + itemLine.endPoint.name);
                        MyOpenDocument.ed.WriteMessage("\n У выбранного объекта детей: " + itemLine.children.Count + " шт.");
                        MyOpenDocument.ed.WriteMessage("Выбранный объект основан на вершинах:  ");

                        StringBuilder text = new StringBuilder();
                        foreach (PointLine itemPoint in itemLine.points)
                        {
                            text.Append(itemPoint.name + " ");
                        }
                        MyOpenDocument.ed.WriteMessage(text.ToString());

                        MyOpenDocument.ed.WriteMessage("Выбранный объект лежит в ребрах:  ");

                        StringBuilder text2 = new StringBuilder();
                        foreach (Edge itemEdge in itemLine.Edges)
                        {
                            text2.Append(itemEdge.name + " ");
                        }
                        MyOpenDocument.ed.WriteMessage(text2.ToString());

                        MyOpenDocument.ed.WriteMessage("~~~~~~~~~~~~~~~~");
                        MyOpenDocument.ed.WriteMessage("\n  ");
                    }

                }

                trAdding.Commit();
            }

        }

        // Fun Для получения все поинты и их коорниданты 
        [CommandMethod("фв4", CommandFlags.UsePickSet |
                       CommandFlags.Redraw | CommandFlags.Modal)] // название команды, вызываемой в Autocad


        public void getAllPointLine()
        {
            using (Transaction trAdding = MyOpenDocument.dbCurrent.TransactionManager.StartTransaction())
            {
                MyOpenDocument.ed.WriteMessage("\n  ");
                MyOpenDocument.ed.WriteMessage("~~~~~~~~~~~~~~~~");
                MyOpenDocument.ed.WriteMessage("СПИСОК ВСЕХ ВЕРШИН: ");

                foreach (PointLine itemPoint in listPoint)
                {

                    MyOpenDocument.ed.WriteMessage(itemPoint.name.ToString() + " " + "weight: " + itemPoint.weightA + " " + itemPoint.positionPoint + " I: " + itemPoint.Ia + " " + itemPoint.isFavorite);
                }
                MyOpenDocument.ed.WriteMessage("~~~~~~~~~~~~~~~~");
                MyOpenDocument.ed.WriteMessage("\n  ");

            }

            OnPropertyChanged(nameof(listPoint));

        }

        /*
        // Fun Для получения все наименований 
        [CommandMethod("фв5", CommandFlags.UsePickSet |
                       CommandFlags.Redraw | CommandFlags.Modal)] // название команды, вызываемой в Autocad
        */
        public void getAllPowerLine()
        {
            using (Transaction trAdding = MyOpenDocument.dbCurrent.TransactionManager.StartTransaction())
            {
                MyOpenDocument.ed.WriteMessage("\n  ");
                MyOpenDocument.ed.WriteMessage("~~~~~~~~~~~~~~~~");
                MyOpenDocument.ed.WriteMessage("СПИСОК ВСЕХ СУЩЕСТВУЮЩИХ ЛИНИЙ: ");

                foreach (PowerLine itemLine in listPowerLine)
                {
                    MyOpenDocument.ed.WriteMessage(itemLine.name);
                }

                MyOpenDocument.ed.WriteMessage("~~~~~~~~~~~~~~~~");
                MyOpenDocument.ed.WriteMessage("\n  ");

            }
        }

        // Fun Для получения всех ребер 
        [CommandMethod("фв6", CommandFlags.UsePickSet |
                       CommandFlags.Redraw | CommandFlags.Modal)] // название команды, вызываемой в Autocad
        public void getAllEdeg()
        {
            using (Transaction trAdding = MyOpenDocument.dbCurrent.TransactionManager.StartTransaction())
            {
                MyOpenDocument.ed.WriteMessage("\n  ");
                MyOpenDocument.ed.WriteMessage("~~~~~~~~~~~~~~~~");
                MyOpenDocument.ed.WriteMessage("СПИСОК ВСЕХ РЕБЕР: ");

                foreach (Edge itemEdge in listEdge)
                {
                    MyOpenDocument.ed.WriteMessage(itemEdge.name.ToString() + " | Длина ребра: " + itemEdge.length + " | Марка провода ребра: " + itemEdge.cable + " | Допустимый ток: " + itemEdge.Icrict + " | Протекаемый ток: " + itemEdge.Ia + " | Data: " + itemEdge.r + " | StartPoint: " + itemEdge.startPoint.name + " |EndPoint: " + itemEdge.endPoint.name);

                    // ed.WriteMessage(itemEdge.name.ToString() + " | Длина ребра: " + itemEdge.length + " | Марка провода: " + itemEdge.cable + " | Data: " + itemEdge.data + " | " + itemEdge.centerPoint.name.ToString() + ": " + itemEdge.centerPoint.positionPoint);
                }
                MyOpenDocument.ed.WriteMessage("~~~~~~~~~~~~~~~~");
                MyOpenDocument.ed.WriteMessage("\n  ");

            }
            OnPropertyChanged(nameof(listEdge));
        }

        // Fun Для получения информации по одному выбранному узлу 
        [CommandMethod("фв7", CommandFlags.UsePickSet |
                       CommandFlags.Redraw | CommandFlags.Modal)] // название команды, вызываемой в Autocad
        public void getInfoPoint()
        {
            using (Transaction trAdding = MyOpenDocument.dbCurrent.TransactionManager.StartTransaction())
            {
                PromptEntityOptions InfoPoint = new PromptEntityOptions("\nВыберите узел для получения информации : ");
                PromptEntityResult perInfoPoint = MyOpenDocument.ed.GetEntity(InfoPoint);
                MText mtext = trAdding.GetObject(perInfoPoint.ObjectId, OpenMode.ForRead) as MText;

                int index = (mtext.Contents).IndexOf('P');

                MyOpenDocument.ed.WriteMessage("\n  ");
                MyOpenDocument.ed.WriteMessage("~~~~~~~~~~~~~~~~");
                MyOpenDocument.ed.WriteMessage("ИНФОРМАЦИЯ ПО УЗЛУ №: ");

                foreach (PointLine itemPoint in listPoint)
                {
                    //Для осечения веса узла
                    if (index > 1)
                    {

                        if (int.Parse((mtext.Contents).Substring(0, index - 1)) == itemPoint.name)
                        {
                            MyOpenDocument.ed.WriteMessage(int.Parse((mtext.Contents).Substring(0, index - 1)) + " | Вес вершины: " + itemPoint.weightA + " | " + itemPoint.positionPoint);
                        }
                    }
                    else
                    {
                        if (int.Parse(mtext.Contents) == itemPoint.name)
                        {
                            MyOpenDocument.ed.WriteMessage(itemPoint.name.ToString() + " | Вес вершины: " + itemPoint.weightA + " | " + itemPoint.positionPoint);
                        }
                    }



                }

                MyOpenDocument.ed.WriteMessage("~~~~~~~~~~~~~~~~");
                MyOpenDocument.ed.WriteMessage("\n  ");

            }
        }


        // Fun Для получения информации по одному выбранному ребру 
        [CommandMethod("фв8", CommandFlags.UsePickSet |
                       CommandFlags.Redraw | CommandFlags.Modal)] // название команды, вызываемой в Autocad
        public void getInfoEdge()
        {
            using (Transaction trAdding = MyOpenDocument.dbCurrent.TransactionManager.StartTransaction())
            {
                PromptEntityOptions InfoPoint = new PromptEntityOptions("\nВыберите ребро для получения информации : ");
                PromptEntityResult perInfoPoint = MyOpenDocument.ed.GetEntity(InfoPoint);
                MText mtext = trAdding.GetObject(perInfoPoint.ObjectId, OpenMode.ForRead) as MText;

                MyOpenDocument.ed.WriteMessage("\n  ");
                MyOpenDocument.ed.WriteMessage("~~~~~~~~~~~~~~~~");
                MyOpenDocument.ed.WriteMessage("ИНФОРМАЦИЯ ПО РЕБРУ №: " + mtext.Contents);

                foreach (Edge itemEdge in listEdge)
                {

                    if (mtext.Contents == itemEdge.name.ToString())
                    {
                        MyOpenDocument.ed.WriteMessage(itemEdge.name.ToString() + " | Длина ребра: " + itemEdge.length + " | Марка провода ребра: " + itemEdge.cable + " | Допустимый ток: " + itemEdge.Icrict + " | Протекаемый ток: " + itemEdge.Ia + " | Data: " + itemEdge.r + " | StartPoint: " + itemEdge.startPoint.name + " |EndPoint: " + itemEdge.endPoint.name);
                    }


                }

                MyOpenDocument.ed.WriteMessage("~~~~~~~~~~~~~~~~");
                MyOpenDocument.ed.WriteMessage("\n  ");

            }

        }


        // Fun Для Создания Пути обхода 
        public void creatPathPoint()
        {
            int startPoint = 0;
            int endPoint = 0;

            PromptResult result = MyOpenDocument.ed.GetString("С какой вершине начать обходить ?: ");
            if (result.Status == PromptStatus.OK)
            {
                startPoint = int.Parse(result.StringResult) - 1; //+1 что бы билось с визуализацией 
            }

            //Ввод Конечной вершины
            PromptResult result2 = MyOpenDocument.ed.GetString("Конечная вершина обхода ?:");
            if (result2.Status == PromptStatus.OK)
            {
                endPoint = int.Parse(result2.StringResult) - 1;//+1 что бы билось с визуализацией 
            }

            //Создает путь из классов !!
            List<PointLine> path = ListPathIntToPoint(findPath(matrixSmej, startPoint, endPoint));

            //Нарисовать и приблизить 
            ObjectId idPL = Draw.drawPolyline(path, "Напряжение_Makarov.D", 52, 0.4);
            Draw.ZoomToEntity(idPL, 1);
            MyOpenDocument.ed.SetImpliedSelection(new ObjectId[] { idPL });


            MyOpenDocument.ed.WriteMessage("~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            MyOpenDocument.ed.WriteMessage("Путь ОБХОДА :");

            StringBuilder resultPath = new StringBuilder();
            foreach (PointLine item in path)
            {
                resultPath.Append(item.name + " ");
            }
            MyOpenDocument.ed.WriteMessage(resultPath.ToString());
            MyOpenDocument.ed.WriteMessage("~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

        }


        public void getPathKZ(bool isI1Tkz = true)
        {

            //Получает данные TKZ
            TKZ tkz = сreatTKZ(isI1Tkz);

            //Добавляю свой ввод

            string strResistancetTransformers = Text.creatPromptKeywordOptions("Выберите мощность тр-р с группой соед.: ", BDSQL.searchAllDataInBD(dbFilePath, "transformer", "name"), 2);
            if (string.IsNullOrEmpty(strResistancetTransformers)) { return; }

            //берем сопротивление в BD по тексту
            tkz.transformersR =  BDSQL.searchDataInBD<double>(dbFilePath, "transformer", strResistancetTransformers, "name", "r");
            tkz.transformersX = BDSQL.searchDataInBD<double>(dbFilePath, "transformer", strResistancetTransformers, "name", "x");
            tkz.transformersR0 = BDSQL.searchDataInBD<double>(dbFilePath, "transformer", strResistancetTransformers, "name", "r0");
            tkz.transformersX0 = BDSQL.searchDataInBD<double>(dbFilePath, "transformer", strResistancetTransformers, "name", "x0");

            //Фазное напряжение сети
            double Uline;
            double transformersPetliya;
            double rdop = 0;
            double lineZ = 0;

            if (!isI1Tkz)
            {
                List<string> tempList = BDSQL.searchAllDataInBD(dbFilePath, "voltage", "kV");
                tempList.Insert(0, "Свое");

                string resultPromt = Text.creatPromptKeywordOptions("Выберите ЛИНЕЙНОЕ  напряжение сети.: ", tempList, 2);

                switch (resultPromt)
                {
                    case null:
                        MyOpenDocument.ed.WriteMessage("Отмена");
                        return;
                    case "Свое":


                        PromptDoubleOptions options = new PromptDoubleOptions("Введите свою ЛИНЕЙНУЮ ЭДС:");
                        PromptDoubleResult result = MyOpenDocument.ed.GetDouble(options);
                        if (result.Status == PromptStatus.OK)
                        {
                            MyOpenDocument.ed.WriteMessage("\n\nВы ввели: " + result.Value.ToString());
                            resultPromt = result.Value.ToString();
                        }
                        break;
                    default:
                        // Код, который выполнится, если ни один из случаев не совпадает
                        break;

                }


                Uline = double.Parse(resultPromt);
                //Сумма трех последовательностей по факту тут не петля
                transformersPetliya = Math.Sqrt(Math.Pow((tkz.transformersR), 2) + Math.Pow((tkz.transformersX), 2));
                lineZ = Math.Sqrt(Math.Pow((tkz.lineR), 2) + Math.Pow((tkz.lineX), 2));
                tkz.resultTKZ = Uline / ((transformersPetliya + lineZ) * Math.Sqrt(3));
            }
            else
            {
                //Сопротивление учит. контакты  и т.д   
                rdop = 0.015;
                Uline = 400;
                transformersPetliya = Math.Sqrt(Math.Pow((tkz.transformersR * 2 + tkz.transformersR0), 2) + Math.Pow((tkz.transformersX * 2 + tkz.transformersX0), 2));
                tkz.resultTKZ = Uline / ((transformersPetliya / 3 + tkz.linePetlia + rdop) * Math.Sqrt(3));
            }

            StringBuilder text = new StringBuilder();

            //Реверс пути
            tkz.pathPointTKZ.Reverse();
            foreach (PointLine item in tkz.pathPointTKZ)
            {
                text.Append(item.name + " ");

            }

            //Создает PL
            Draw.drawPolyline(tkz.pathPointTKZ, "TKZ_Makarov.D", 256, 2);
            MyOpenDocument.ed.WriteMessage("Линия протекания ТКЗ построена!");

            MyOpenDocument.ed.WriteMessage("~~~~~~~~~~~~~~~~~~~~~~");
            MyOpenDocument.ed.WriteMessage("| Самая нечувствительная точка КЗ: " + tkz.pointTKZ.name + " ");
            MyOpenDocument.ed.WriteMessage("| Путь ТКЗ: " + text + " ");
            MyOpenDocument.ed.WriteMessage("| Длинна ТКЗ: " + tkz.length + " м.");
            MyOpenDocument.ed.WriteMessage("| Линейное напряжение сети: " + Uline + " В.");
            if (!isI1Tkz)
            {
                MyOpenDocument.ed.WriteMessage("| Сопротивление тр-р: " + tkz.transformersR + " +j " + tkz.transformersX + " (Z= " + Math.Sqrt(Math.Pow((tkz.transformersR), 2) + Math.Pow((tkz.transformersX), 2)) + ")" + " Ом.");
                MyOpenDocument.ed.WriteMessage("| Итоговое сопротивление линии: " + tkz.lineR + " +j " + tkz.lineX + " (Z= " + Math.Sqrt(Math.Pow((tkz.lineR), 2) + Math.Pow((tkz.lineX), 2)) + ")" + " Ом.");
                MyOpenDocument.ed.WriteMessage("| Ток КЗ в конце: " + tkz.resultTKZ + " А.");
                //ed.WriteMessage("| Рекомендуемый автоматический выключатель не более : " + Math.Round(tkz.resultTKZ / 3) + " А.");
                MyOpenDocument.ed.WriteMessage("| ------------------------------------------------------------------------------------------------");
                MyOpenDocument.ed.WriteMessage("Расчет выполнен согласно ГОСТ 28249-93");
            }
            else
            {
                MyOpenDocument.ed.WriteMessage("| Добавил дополнительно: Zдоп.= " + rdop + " Ом. " + "Суммарное переходное сопротивление рубильников, автоматов, болтовых соединений и электрической дуги.");
                MyOpenDocument.ed.WriteMessage("| Сопротивление тр-р: " + "R1+jX1=R2+jX2: " + tkz.transformersR + " +j " + tkz.transformersX + " | " + "R0+jX0: " + tkz.transformersR0 + " +j " + tkz.transformersX0 + " (Zпетля= " + transformersPetliya + ")" + " Ом.");
                MyOpenDocument.ed.WriteMessage("| Итоговое сопротивление линии: " + "R1+jX1=R2+jX2: " + tkz.lineR + " +j " + tkz.lineX + " | " + "R0+jX0: " + tkz.lineR0 + " +j " + tkz.lineX0 + " (Zпетля= " + tkz.linePetlia + ")" + " Ом.");
                MyOpenDocument.ed.WriteMessage("| Ток КЗ в конце: " + tkz.resultTKZ + " А.");
                MyOpenDocument.ed.WriteMessage("| Рекомендуемый автоматический выключатель не более : " + Math.Round(tkz.resultTKZ / 3) + " А.");
                MyOpenDocument.ed.WriteMessage("| ------------------------------------------------------------------------------------------------");
                MyOpenDocument.ed.WriteMessage("Расчет выполнен согласно Рекомендации по расчету сопротивления петли \"фаза-нуль\". - М.: Центральное бюро научно-технической информации, 1986.");
            }

            MyOpenDocument.ed.WriteMessage("~~~~~~~~~~~~~~~~~~~~~~");




        }


        public void getMyPathKZ(bool isI1Tkz = true)
        {
            List<string> tempListPoint = new List<string>();

            foreach (PointLine element in listPoint)
            {
                tempListPoint.Add(element.name.ToString());
            }

            //Добавка в начало списка
            tempListPoint.Insert(0, "Выбрать самостоятельно узел");

            string pointKZ = Text.creatPromptKeywordOptions("Введите номер узла КЗ:", tempListPoint, 1);
            if (string.IsNullOrEmpty(pointKZ)) { return; };


            if (pointKZ == "Выбрать_самостоятельно_узел" | pointKZ == "самостоятельно узел" | pointKZ == "Выбрать самостоятельно узел")
            {
                PromptEntityOptions tempPointKZ = new PromptEntityOptions("\nВыберите узел для КЗ: ");
                PromptEntityResult pertempPointKZ = MyOpenDocument.ed.GetEntity(tempPointKZ);

                using (DocumentLock docloc = MyOpenDocument.doc.LockDocument())
                {
                    using (Transaction trAdding = MyOpenDocument.dbCurrent.TransactionManager.StartTransaction())
                    {
                        MText mtext = trAdding.GetObject(pertempPointKZ.ObjectId, OpenMode.ForRead) as MText;
                        pointKZ = mtext.Contents;
                        trAdding.Commit();
                    }
                }
            }



            //Получает данные TKZ
            TKZ tkz = сreatMyTKZ(int.Parse(pointKZ));
            string strResistancetTransformers = Text.creatPromptKeywordOptions("Выберите мощность тр-р с группой соед.: ", BDSQL.searchAllDataInBD(dbFilePath, "transformer", "name"), 1);
            //берем сопротивление в BD по тексту
            tkz.transformersR = BDSQL.searchDataInBD<double>(dbFilePath, "transformer", strResistancetTransformers, "name", "r");
            tkz.transformersX = BDSQL.searchDataInBD<double>(dbFilePath, "transformer", strResistancetTransformers, "name", "x");
            tkz.transformersR0 = BDSQL.searchDataInBD<double>(dbFilePath, "transformer", strResistancetTransformers, "name", "r0");
            tkz.transformersX0 = BDSQL.searchDataInBD<double>(dbFilePath, "transformer", strResistancetTransformers, "name", "x0");

            //Фазное напряжение сети
            double Uline;
            double transformersPetliya;
            double rdop = 0;
            double lineZ = 0;

            if (!isI1Tkz)
            {
                List<string> tempList = BDSQL.searchAllDataInBD(dbFilePath, "voltage", "kV");
                tempList.Insert(0, "Свое");

                string resultPromt = Text.creatPromptKeywordOptions("Выберите ЛИНЕЙНОЕ  напряжение сети.: ", tempList, 2);

                switch (resultPromt)
                {
                    case null:
                        MyOpenDocument.ed.WriteMessage("Отмена");
                        return;
                    case "Свое":


                        PromptDoubleOptions options = new PromptDoubleOptions("Введите свою ЛИНЕЙНУЮ ЭДС:");
                        PromptDoubleResult result = MyOpenDocument.ed.GetDouble(options);
                        if (result.Status == PromptStatus.OK)
                        {
                            MyOpenDocument.ed.WriteMessage("\n\nВы ввели: " + result.Value.ToString());
                            resultPromt = result.Value.ToString();
                        }
                        break;
                    default:
                        // Код, который выполнится, если ни один из случаев не совпадает
                        break;

                }


                Uline = double.Parse(resultPromt);
                //Сумма трех последовательностей по факту тут не петля
                transformersPetliya = Math.Sqrt(Math.Pow((tkz.transformersR), 2) + Math.Pow((tkz.transformersX), 2));
                lineZ = Math.Sqrt(Math.Pow((tkz.lineR), 2) + Math.Pow((tkz.lineX), 2));
                tkz.resultTKZ = Uline / ((transformersPetliya + lineZ) * Math.Sqrt(3));
            }
            else
            {
                //Сопротивление учит. контакты  и т.д   
                rdop = 0.015;
                Uline = 400;
                transformersPetliya = Math.Sqrt(Math.Pow((tkz.transformersR * 2 + tkz.transformersR0), 2) + Math.Pow((tkz.transformersX * 2 + tkz.transformersX0), 2));
                tkz.resultTKZ = Uline / ((transformersPetliya / UserData.coefficientMultiplicity + tkz.linePetlia + rdop) * Math.Sqrt(3));
            }

            StringBuilder text = new StringBuilder();

            //Реверс пути
            tkz.pathPointTKZ.Reverse();
            foreach (PointLine item in tkz.pathPointTKZ)
            {
                text.Append(item.name + " ");

            }

            //Создает PL
            Draw.drawPolyline(tkz.pathPointTKZ, "TKZ_Makarov.D", 256, 2);
            MyOpenDocument.ed.WriteMessage("Линия протекания ТКЗ построена!");

            MyOpenDocument.ed.WriteMessage("~~~~~~~~~~~~~~~~~~~~~~");
            MyOpenDocument.ed.WriteMessage("| Самая нечувствительная точка КЗ: " + tkz.pointTKZ.name + " ");
            MyOpenDocument.ed.WriteMessage("| Путь ТКЗ: " + text + " ");
            MyOpenDocument.ed.WriteMessage("| Длинна ТКЗ: " + tkz.length + " м.");
            MyOpenDocument.ed.WriteMessage("| Линейное напряжение сети: " + Uline + " В.");
            if (!isI1Tkz)
            {
                MyOpenDocument.ed.WriteMessage("| Сопротивление тр-р: " + tkz.transformersR + " +j " + tkz.transformersX + " (Z= " + Math.Sqrt(Math.Pow((tkz.transformersR), 2) + Math.Pow((tkz.transformersX), 2)) + ")" + " Ом.");
                MyOpenDocument.ed.WriteMessage("| Итоговое сопротивление линии: " + tkz.lineR + " +j " + tkz.lineX + " (Z= " + Math.Sqrt(Math.Pow((tkz.lineR), 2) + Math.Pow((tkz.lineX), 2)) + ")" + " Ом.");
                MyOpenDocument.ed.WriteMessage("| Ток КЗ в конце: " + tkz.resultTKZ + " А.");
                //ed.WriteMessage("| Рекомендуемый автоматический выключатель не более : " + Math.Round(tkz.resultTKZ / 3) + " А.");
                MyOpenDocument.ed.WriteMessage("| ------------------------------------------------------------------------------------------------");
                MyOpenDocument.ed.WriteMessage("Расчет выполнен согласно ГОСТ 28249-93");
            }
            else
            {
                MyOpenDocument.ed.WriteMessage("| Добавил дополнительно: Zконт.= " + rdop + " Ом. " + "Суммарное переходное сопротивление рубильников, автоматов, болтовых соединений и электрической дуги.");
                MyOpenDocument.ed.WriteMessage("| Сопротивление тр-р: " + "R1+jX1=R2+jX2: " + tkz.transformersR + " +j " + tkz.transformersX + " | " + "R0+jX0: " + tkz.transformersR0 + " +j " + tkz.transformersX0 + " (Zпетля= " + transformersPetliya + ")" + " Ом.");
                MyOpenDocument.ed.WriteMessage("| Итоговое сопротивление линии: " + "R1+jX1=R2+jX2: " + tkz.lineR + " +j " + tkz.lineX + " | " + "R0+jX0: " + tkz.lineR0 + " +j " + tkz.lineX0 + " (Zпетля= " + tkz.linePetlia + ")" + " Ом.");
                MyOpenDocument.ed.WriteMessage("| Ток КЗ в конце: " + tkz.resultTKZ + " А.");
                MyOpenDocument.ed.WriteMessage("| Рекомендуемый автоматический выключатель не более : " + Math.Round(tkz.resultTKZ / UserData.coefficientMultiplicity) + " А.");
                MyOpenDocument.ed.WriteMessage("| ------------------------------------------------------------------------------------------------");
                MyOpenDocument.ed.WriteMessage("Расчет выполнен согласно Рекомендации по расчету сопротивления петли \"фаза-нуль\". - М.: Центральное бюро научно-технической информации, 1986.");
            }

            MyOpenDocument.ed.WriteMessage("~~~~~~~~~~~~~~~~~~~~~~");

        }


        /*
        //Для проверки АВ, до куда чувсвителен, по самой удаленной точки
        */
        public void getMyPathKZFromAV()
        {
            int nomivalAV = 0;
            PromptIntegerOptions options = new PromptIntegerOptions("Введите номинал автоматического выключателя, для проверки зоны чувствительности: ");
            options.AllowNegative = false;
            options.AllowZero = false;

            PromptIntegerResult result = MyOpenDocument.ed.GetInteger(options);
            if (result.Status == PromptStatus.OK)
            {
                nomivalAV = result.Value;
                MyOpenDocument.ed.WriteMessage("Вы ввели: " + result.Value.ToString());
            }

            //Получает данные TKZ
            TKZ tkz = сreatTKZ(true);
            string strResistancetTransformers = Text.creatPromptKeywordOptions("Выберите мощность тр-р с группой соед.: ", BDSQL.searchAllDataInBD(dbFilePath, "transformer", "name"), 1);

            //берем сопротивление в BD по тексту
            tkz.transformersR = BDSQL.searchDataInBD<double>(dbFilePath, "transformer", strResistancetTransformers, "name", "r");
            tkz.transformersX = BDSQL.searchDataInBD<double>(dbFilePath, "transformer", strResistancetTransformers, "name", "x");
            tkz.transformersR0 = BDSQL.searchDataInBD<double>(dbFilePath, "transformer", strResistancetTransformers, "name", "r0");
            tkz.transformersX0 = BDSQL.searchDataInBD<double>(dbFilePath, "transformer", strResistancetTransformers, "name", "x0");

            //Фазное напряжение сети
            // double Uf = double.Parse(creatPromptKeywordOptions("Выберите Фазное напряжение сети.: ", searchAllDataInBD(dbFilePath, "voltage", "kV"), 1));
            double rdop = 0.015;
            double Uline = 400;
            double transformersPetliya = Math.Sqrt(Math.Pow((tkz.transformersR * 2 + tkz.transformersR0), 2) + Math.Pow((tkz.transformersX * 2 + tkz.transformersX0), 2));
            tkz.resultTKZ = Uline / ((transformersPetliya / 3 + tkz.linePetlia + rdop) * Math.Sqrt(3));



            // /3-это почти эквивалент 5сек
            if (nomivalAV <= tkz.resultTKZ / UserData.coefficientMultiplicity)
            {
                MyOpenDocument.ed.WriteMessage("Ваш автоматический выключатель на " + nomivalAV + " А, защищает всю линую до точки максимальной нечувствительности.");
            }

            else
            {

                TKZ resultTkzDist = new TKZ();
                //Получает данные TKZ
                tkz.pathPointTKZ.Reverse();

                transformersPetliya = Math.Sqrt(Math.Pow((tkz.transformersR * 2 + tkz.transformersR0), 2) + Math.Pow((tkz.transformersX * 2 + tkz.transformersX0), 2));

                foreach (PointLine itemPointTKZ in tkz.pathPointTKZ)
                {
                    if (itemPointTKZ.name != 1)
                    {
                        TKZ tempTKZ = сreatMyTKZ(itemPointTKZ.name);

                        tempTKZ.resultTKZ = Uline / ((transformersPetliya / 3 + tempTKZ.linePetlia + rdop) * Math.Sqrt(3));

                        if (tempTKZ.resultTKZ / UserData.coefficientMultiplicity >= nomivalAV)
                        {
                            resultTkzDist = tempTKZ;
                        }
                    }
                }


                //Реверс
                resultTkzDist.pathPointTKZ.Reverse();

                StringBuilder text = new StringBuilder();
                foreach (PointLine item in resultTkzDist.pathPointTKZ)
                {
                    text.Append(item.name + " ");

                }


                //Создает PL
                Draw.drawPolyline(resultTkzDist.pathPointTKZ, "TKZ_Makarov.D", 256, 2);
                MyOpenDocument.ed.WriteMessage("Линия протекания ТКЗ построена!");

                MyOpenDocument.ed.WriteMessage("~~~~~~~~~~~~~~~~~~~~~~");
                MyOpenDocument.ed.WriteMessage("| Самая нечувствительная точка КЗ: " + resultTkzDist.pointTKZ.name + " ");
                MyOpenDocument.ed.WriteMessage("| Путь ТКЗ: " + text + " ");
                MyOpenDocument.ed.WriteMessage("| Длинна ТКЗ: " + resultTkzDist.length + " м.");
                MyOpenDocument.ed.WriteMessage("| Линейное напряжение сети: " + Uline + " В.");
                MyOpenDocument.ed.WriteMessage("| Добавил дополнительно: Z= " + rdop + " Ом. " + "Суммарное переходное сопротивление рубильников, автоматов, болтовых соединений и электрической дуги.");
                MyOpenDocument.ed.WriteMessage("| Сопротивление тр-р: " + "R1+jX1=R2+jX2: " + tkz.transformersR + " +j " + tkz.transformersX + " | " + "R0+jX0: " + tkz.transformersR0 + " +j " + tkz.transformersX0 + " (Zпетля= " + transformersPetliya + ")" + " Ом.");
                MyOpenDocument.ed.WriteMessage("| Итоговое сопротивление линии: " + "R1+jX1=R2+jX2: " + resultTkzDist.lineR + " +j " + resultTkzDist.lineX + " | " + "R0+jX0: " + resultTkzDist.lineR0 + " +j " + resultTkzDist.lineX0 + " (Zпетля= " + resultTkzDist.linePetlia + ")" + " Ом.");
                MyOpenDocument.ed.WriteMessage("| Ток КЗ в конце: " + resultTkzDist.resultTKZ + " А.");
                MyOpenDocument.ed.WriteMessage("| Рекомендуемый автоматический выключатель не более : " + Math.Round(resultTkzDist.resultTKZ / UserData.coefficientMultiplicity) + " А.");
                MyOpenDocument.ed.WriteMessage("| ------------------------------------------------------------------------------------------------");
                MyOpenDocument.ed.WriteMessage("Расчет выполнен согласно Рекомендации по расчету сопротивления петли \"фаза-нуль\". - М.: Центральное бюро научно-технической информации, 1986.");



            }



        }

        /*
        //Поиск места установки рекоузера в магистрали. 
        */
        public void getLocalREC()
        {
            List<PointLine> masterPointLine = listPowerLine[0].points;
            List<PointLine> saveListMagistral = new List<PointLine>(listPowerLine[0].points);
            List<PointLine> listWhithWeight = new List<PointLine>();

            //Создает список вершин с весом
            foreach (PointLine itemPoint in listPoint)
            {
                if (itemPoint.count > 0)
                {

                    listWhithWeight.Add(itemPoint);
                }
            }

            foreach (PointLine itemPoint in listWhithWeight)
            {
                foreach (PowerLine itemPowerLine in listPowerLine)
                {
                    for (int i = 1; i < itemPowerLine.points.Count(); i++)
                    {
                        if (itemPoint == itemPowerLine.points[i])
                        {
                            PointLine perentPointMagistral = goToParent(itemPowerLine, listPowerLine[0].name).parentPoint;
                            perentPointMagistral.count = perentPointMagistral.count + itemPoint.count;
                            if (itemPoint.isFavorite)
                            {
                                perentPointMagistral.isFavorite = true;
                            }
                        }
                    }
                }
            }

            SaidiSaifi REC = new SaidiSaifi();


            foreach (PointLine itemPoint in masterPointLine)
            {
                List<PointLine> tempLeftPath = new List<PointLine>();
                List<PointLine> tempRightPath = new List<PointLine>();
                double tempLeftDifference = 0;
                double tempRighDifference = 0;
                tempLeftPath = ListPathIntToPoint(findPath(matrixSmej, itemPoint.name - 1, masterPointLine[0].name - 1));
                tempRightPath = ListPathIntToPoint(findPath(matrixSmej, itemPoint.name - 1, masterPointLine.Last().name - 1));

                StringBuilder tempLeftPathText = new StringBuilder();
                foreach (PointLine itemPointLine in tempLeftPath)
                {
                    tempLeftDifference = tempLeftDifference + itemPointLine.count;
                }

                StringBuilder tempRightPathText = new StringBuilder();

                foreach (PointLine itemPointLine in tempRightPath)
                {
                    //Для удаления в правом пути точки из левого пути ( ниже совместный код)
                    if (tempRightPath.IndexOf(itemPointLine) == 0)
                    {
                        continue;
                    }
                    tempRighDifference = tempRighDifference + itemPointLine.count;
                }

                //Проверяет вес и важность поинтов; itemPoint.isFavorite - проверяет в вершине магистрали есть ли важнный потербитель
                if ((tempLeftDifference <= tempRighDifference) || itemPoint.isFavorite)
                {
                    REC.difference = Math.Abs(tempLeftDifference - tempRighDifference);
                    REC.leftPath = tempLeftPath;
                    REC.rightPath = tempRightPath;
                    REC.point = itemPoint;
                    REC.LeftWeight = tempLeftDifference;
                    REC.RighWeight = tempRighDifference;

                    //Для Визуала, для удаления перового поинта 
                    REC.rightPath.RemoveAt(0);

                    foreach (PointLine itemPointLine in REC.leftPath)
                    {
                        tempLeftPathText.Append(itemPointLine.name + " (" + itemPointLine.count + ")" + " ");
                    }


                    foreach (PointLine itemPointLine in REC.rightPath)
                    {

                        tempRightPathText.Append(itemPointLine.name + " (" + itemPointLine.count + ")" + " ");
                    }

                    REC.leftPathText = tempLeftPathText.ToString();
                    REC.righPathText = tempRightPathText.ToString();
                }
                tempLeftDifference = 0;
                tempRighDifference = 0;

            }

            Edge edgeREC = new Edge();
            foreach (Edge element in listEdge)
            {
                if ((REC.leftPath[0] == element.startPoint || REC.leftPath[0] == element.endPoint)
                   && (REC.rightPath[0] == element.startPoint || REC.rightPath[0] == element.endPoint))
                {
                    edgeREC = element;

                }
            }
            //Построить окружность
            Draw.ZoomToEntity(Draw.drawCircle(edgeREC.centerPoint, "Граф_Saidi_Saifi_Makarov.D"), 4);

            MyOpenDocument.ed.WriteMessage("----------");
            MyOpenDocument.ed.WriteMessage("Рекомендуемое место установки REC в ребро №: " + edgeREC.name);
            MyOpenDocument.ed.WriteMessage("Рекомендуемое место установки REC между вершинами № " + edgeREC.startPoint.name + " И " + edgeREC.endPoint.name);
            MyOpenDocument.ed.WriteMessage("Вес левой части: " + REC.LeftWeight + " | " + REC.leftPathText);
            MyOpenDocument.ed.WriteMessage("Вес правой части: " + REC.RighWeight + " | " + REC.righPathText);
            MyOpenDocument.ed.WriteMessage("----------");




            //  masterPointLine = new List<PointLine>(saveListMagistral);

            //Скинуть веса вершин у магистрали
            foreach (var item in masterPointLine)
            {
                item.weightA = 0;
                item.isFavorite = false;
            }


        }

        /*
        //Поиск Падения напряжения. 
       */
        public void getVoltage()
        {
            //Фазное напряжение сети
            string tempUgen = Text.creatPromptKeywordOptions("Выберите напряжение точки генерации сети.: ", BDSQL.searchAllDataInBD(dbFilePath, "voltage", "kV"), 1);
            if (string.IsNullOrEmpty(tempUgen)) { return; };
            double Ugen = double.Parse(tempUgen);

            double Ufashze = Math.Round(Ugen / Math.Sqrt(3), 2);

            listPoint[0].Ua = Ufashze;
            listPoint[0].Ub = Ufashze;
            listPoint[0].Uc = Ufashze;

            using (Transaction trAdding = MyOpenDocument.dbCurrent.TransactionManager.StartTransaction())
            {
                Layer.deleteObjectsOnLayer("Напряжение_Makarov.D");
                trAdding.Commit();
            }

            foreach (var item in listPoint)
            {
                item.Ia = 0;
                item.Ib = 0;
                item.Ic = 0;
                item.tempBoll = false;
            }


            //Резервный список 
            List<PointLine> stateList = new List<PointLine>(listPoint);
            List<PointLine> listWithWeight = new List<PointLine>();
            List<List<PointLine>> tempAllPath = new List<List<PointLine>>();
            List<List<PointLine>> resultAllPath = new List<List<PointLine>>();
            // Создает список вершин с весом
            listWithWeight = GetWeightedVertices(listPoint);


            //Пути до вершин
            foreach (PointLine itemPoint in listWithWeight)
            {
                List<PointLine> path = ListPathIntToPoint(findPath(matrixSmej, itemPoint.name - 1, listPowerLine[0].points[0].name - 1));
                tempAllPath.Add(path);
            }

            tempAllPath.Reverse();



            //Самое важное. Уберает пути которые частично есть 
            foreach (List<PointLine> itemListPoint in tempAllPath)
            {
                if (resultAllPath.Count == 0)
                {
                    resultAllPath.Add(itemListPoint);
                    continue;
                }

                if (!resultAllPath.Any(x => x.Contains(itemListPoint[0])))
                {

                    resultAllPath.Add(itemListPoint);
                }
            }

            //Алгоритм сложения всех токов
            foreach (List<PointLine> itemListPoint in resultAllPath)
            {
                for (int i = 0; i < itemListPoint.Count() - 1; i++)
                {
                    double tempAddIa = 0;
                    double tempAddIb = 0;
                    double tempAddIc = 0;

                    if (!itemListPoint[i + 1].tempBoll)
                    {
                        itemListPoint[i + 1].tempBoll = true;
                        itemListPoint[i + 1].Ia = Math.Round(itemListPoint[i + 1].Ia + itemListPoint[i].Ia, 3);
                        itemListPoint[i + 1].Ib = Math.Round(itemListPoint[i + 1].Ib + itemListPoint[i].Ib, 3);
                        itemListPoint[i + 1].Ic = Math.Round(itemListPoint[i + 1].Ic + itemListPoint[i].Ic, 3);
                    }
                    else
                    {
                        tempAddIa = itemListPoint[0].Ia;
                        itemListPoint[i + 1].Ia += Math.Round(tempAddIa, 2);

                        tempAddIb = itemListPoint[0].Ib;
                        itemListPoint[i + 1].Ib += Math.Round(tempAddIb, 2);

                        tempAddIc = itemListPoint[0].Ic;
                        itemListPoint[i + 1].Ic += Math.Round(tempAddIc, 2);
                    }
                }

                //Построить куда бежит ток
                Draw.drawPolyline(itemListPoint, "Напряжение_Makarov.D", 52, 0.6);
            }


            //Ток добавляем в ребра
            foreach (PointLine itemPoint in listPoint)
            {
                if (itemPoint.Ia > 0 | itemPoint.Ib > 0 | itemPoint.Ic > 0)
                {
                    foreach (Edge itemEdge in listEdge)
                    {
                        if (itemPoint == itemEdge.endPoint)
                        {
                            itemEdge.Ia = itemPoint.Ia;
                            itemEdge.Ib = itemPoint.Ib;
                            itemEdge.Ic = itemPoint.Ic;
                        }

                        //Проверка на критический ток
                        if (itemEdge.Ia > itemEdge.Icrict | itemEdge.Ib > itemEdge.Icrict | itemEdge.Ic > itemEdge.Icrict)
                        {
                            Text.creatText("Напряжение_Makarov.D", itemEdge.centerPoint, "I>Iкрит " + "A: " + itemEdge.Ia + "; " + "B: " + itemEdge.Ib + "; " + "C: " + itemEdge.Ic + " A.", "1", 220, 4);
                        }
                    }
                }
            }


            //Анализирует падения напряжения и отрисовывает Новый алгоритм 
            foreach (Edge itemEdge in listEdge)
            {

                //Отрисовка падения напряжения
                if ((itemEdge.startPoint.Ia > 0 & itemEdge.endPoint.Ia > 0) | (itemEdge.startPoint.Ib > 0 & itemEdge.endPoint.Ib > 0) | (itemEdge.startPoint.Ic > 0 & itemEdge.endPoint.Ic > 0))
                {
                    //Фаза А
                    itemEdge.endPoint.Ua = dropVoltage(itemEdge, "А");

                    Text.creatText("Напряжение_Makarov.D", itemEdge.centerPoint, " ΔUa= " + Math.Round((itemEdge.startPoint.Ua - itemEdge.endPoint.Ua), 2) + " В.", "1", 154, -4);

                    //Фаза В
                    itemEdge.endPoint.Ub = dropVoltage(itemEdge, "В");
                    Text.creatText("Напряжение_Makarov.D", itemEdge.centerPoint, " ΔUb= " + Math.Round((itemEdge.startPoint.Ub - itemEdge.endPoint.Ub), 2) + " В.", "1", 154, -6);

                    //Фаза С
                    itemEdge.endPoint.Uc = dropVoltage(itemEdge, "С");
                    Text.creatText("Напряжение_Makarov.D", itemEdge.centerPoint, " ΔUc= " + Math.Round((itemEdge.startPoint.Uc - itemEdge.endPoint.Uc), 2) + " В.", "1", 154, -8);
                }

                //Отрисовка линейного напряжения
                if (itemEdge.endPoint.Ua > 0 | itemEdge.endPoint.Ub > 0 | itemEdge.endPoint.Uc > 0)
                {
                    //Фаза А
                    if (((Ufashze - itemEdge.endPoint.Ua) / Ufashze * 100) >= 10.0)
                    {
                        double percentA = Math.Round(((Ufashze - itemEdge.endPoint.Ua) / Ufashze * 100), 2);
                        Text.creatText("Напряжение_Makarov.D", itemEdge.endPoint, "Uа= " + itemEdge.endPoint.Ua.ToString() + " В; " + percentA + " %.", "1", 26, -4);
                    }
                    else
                    {
                        double percentA = Math.Round(((Ufashze - itemEdge.endPoint.Ua) / Ufashze * 100), 2);
                        Text.creatText("Напряжение_Makarov.D", itemEdge.endPoint, "Uа= " + itemEdge.endPoint.Ua.ToString() + " В; " + percentA + " %.", "1", 41, -4);
                    }

                    //Фаза В
                    if ((Ufashze - itemEdge.endPoint.Ub) / Ufashze * 100 >= 10.0)
                    {
                        double percentB = Math.Round(((Ufashze - itemEdge.endPoint.Ub) / Ufashze * 100), 2);
                        Text.creatText("Напряжение_Makarov.D", itemEdge.endPoint, "Ub= " + itemEdge.endPoint.Ub.ToString() + " В; " + percentB + " %.", "1", 26, -6);
                    }
                    else
                    {
                        double percentB = Math.Round(((Ufashze - itemEdge.endPoint.Ub) / Ufashze * 100), 2);
                        Text.creatText("Напряжение_Makarov.D", itemEdge.endPoint, "Ub= " + itemEdge.endPoint.Ub.ToString() + " В; " + percentB + " %.", "1", 74, -6);
                    }

                    //Фаза С
                    if (((Ufashze - itemEdge.endPoint.Uc) / Ufashze * 100) >= 10.0)
                    {
                        double percentC = Math.Round(((Ufashze - itemEdge.endPoint.Uc) / Ufashze * 100), 2);
                        Text.creatText("Напряжение_Makarov.D", itemEdge.endPoint, "Uc= " + itemEdge.endPoint.Uc.ToString() + " В; " + percentC + " %.", "1", 26, -8);
                    }
                    else
                    {
                        double percentC = Math.Round(((Ufashze - itemEdge.endPoint.Uc) / Ufashze * 100), 2);
                        Text.creatText("Напряжение_Makarov.D", itemEdge.endPoint, "Uc= " + itemEdge.endPoint.Uc.ToString() + " В; " + percentC + " %.", "1", 22, -8);
                    }

                }
            }

            double dropVoltage(Edge itemEdge, string phase)
            {
                double result = 0;
                double dU = 0;
                double qU = 0;
                switch (phase)
                {
                    case "А":
                        dU = itemEdge.Ia * itemEdge.length * itemEdge.r + itemEdge.length * itemEdge.rN * (itemEdge.Ia - (itemEdge.Ib + itemEdge.Ic) / 2);
                        //Нигде не использую пока что 
                        qU = itemEdge.Ia * itemEdge.length * itemEdge.x + itemEdge.length * itemEdge.xN * (itemEdge.Ia - (itemEdge.Ib + itemEdge.Ic) / 2);
                        result = Math.Round(itemEdge.startPoint.Ua - (dU + 0), 2);
                        return result;
                    case "В":
                        dU = itemEdge.Ib * itemEdge.length * itemEdge.r + itemEdge.length * itemEdge.rN * (itemEdge.Ib - (itemEdge.Ia + itemEdge.Ic) / 2);
                        //Нигде не использую пока что 
                        qU = itemEdge.Ib * itemEdge.length * itemEdge.x + itemEdge.length * itemEdge.xN * (itemEdge.Ib - (itemEdge.Ia + itemEdge.Ic) / 2);
                        result = Math.Round(itemEdge.startPoint.Ub - (dU + 0), 2);
                        return result;
                    case "С":
                        dU = itemEdge.Ic * itemEdge.length * itemEdge.r + itemEdge.length * itemEdge.rN * (itemEdge.Ic - (itemEdge.Ia + itemEdge.Ib) / 2);
                        //Нигде не использую пока что 
                        qU = itemEdge.Ic * itemEdge.length * itemEdge.x + itemEdge.length * itemEdge.xN * (itemEdge.Ic - (itemEdge.Ia + itemEdge.Ib) / 2);
                        result = Math.Round(itemEdge.startPoint.Uc - (dU + 0), 2);
                        return result;
                }
                return result;
            }

            listPoint = new List<PointLine>(stateList);
            OnPropertyChanged(nameof(listPoint));
            OnPropertyChanged(nameof(listEdge));
            OnPropertyChanged(nameof(listPowerLine));
        }





        public void setBD()
        {
            try
            {
                MyOpenDocument.ed.WriteMessage("\n--------------------------------\n");
                MyOpenDocument.ed.WriteMessage("Сохранение веса вершин \n");
                MyOpenDocument.ed.WriteMessage("--------------------------------\n");

                BD bd = new BD();
                //Для восстановления имени линии
                //bd.listPowerLine = listPowerLine;
                //Для восстановления веса Вершин
                bd.listPointLine = listPoint;

                PromptEntityOptions item = new PromptEntityOptions("\nВыберите объект куда сохранить веса вершин: ");
                PromptEntityResult perItem = MyOpenDocument.ed.GetEntity(item);
                if (perItem.Status != PromptStatus.OK) { return; }

                string xmlData = SerializeToXml(bd);
                SaveXmlToXrecord(xmlData, perItem.ObjectId, "Makarov.D");
                MyOpenDocument.ed.WriteMessage("\nСписок успешно сохранен в Xrecord.\n");
            }
            catch (Exception ex)
            {
                MyOpenDocument.ed.WriteMessage(ex.ToString());
                MyOpenDocument.ed.WriteMessage("Что-то пошло не так....");
            }
        }



        public void getBD()

        {
            try
            {
                MyOpenDocument.ed.WriteMessage("\n--------------------------------\n");
                MyOpenDocument.ed.WriteMessage("Восстановление веса вершин \n");
                MyOpenDocument.ed.WriteMessage("--------------------------------\n");
                PromptEntityOptions item = new PromptEntityOptions("\nВыберите объект откуда взять БД: ");
                PromptEntityResult perItem = MyOpenDocument.ed.GetEntity(item);

                if (perItem.Status != PromptStatus.OK) { return; }

                string isLoadNameLine = Text.creatPromptKeywordOptions("Восстановить название линий ?", new List<string> { "Да", "Нет" }, 1);

                List<PointLine> templistPoint = BDShowExtensionDictionaryContents<BD>(perItem.ObjectId, "Makarov.D")?.listPointLine;
                List<PowerLine> templistPowerLine = BDShowExtensionDictionaryContents<BD>(perItem.ObjectId, "Makarov.D")?.listPowerLine;

                foreach (PointLine itemPoint in templistPoint)
                {
                    if (itemPoint.weightA > 0 | itemPoint.weightB > 0 | itemPoint.weightC > 0)
                    {
                        foreach (PointLine item1 in listPoint)
                        {

                            if (itemPoint.name == item1.name)
                            {
                                item1.typeClient = itemPoint.typeClient;
                                item1.cos = itemPoint.cos;
                                //_ потому что иначе криво восстанавливает 
                                item1._weightB = itemPoint._weightB;
                                item1._weightC = itemPoint._weightC;
                                item1.weightA = itemPoint.weightA;
                                item1.count = itemPoint.count;
                                item1.Ko = itemPoint.Ko;
                                //Text.updateTextById(item1.IDText, item1.name + "\\P" + item1.weightA, 66);
                            }

                        }
                    }

                }

                if (isLoadNameLine == "Да")
                {
                    for (int i = 0; i < templistPowerLine.Count(); i++)
                    {
                        listPowerLine[i].name = templistPowerLine[i].name;

                    }
                }

                MyOpenDocument.ed.WriteMessage("\nПолучили данные из БД.\n");
                OnPropertyChanged(nameof(listPoint));
            }
            catch (Exception ex)
            {
                MyOpenDocument.ed.WriteMessage(ex.ToString());
                MyOpenDocument.ed.WriteMessage("Что-то пошло не так....");
            }

        }

        [CommandMethod("Elt", CommandFlags.UsePickSet |
              CommandFlags.Redraw | CommandFlags.Modal)]
        public async void creatFormAsync()
        {

            bool isStart = await _netWork.showUpdateWindows();

            if (isStart)
            {
                return;
            }
            else
            {
                nextFormAsync();
            }

        }




        public void nextFormAsync()
        {
            _myData = new MyData(this);
            //Создает боковое меню
            StartPalet myUserControl = new StartPalet(_myData);
            PaletteSet paletteSet = new PaletteSet("ElectroTools", new Guid("5020f2bc-42b1-4d65-aa80-df455f4bed60"));
            paletteSet.Style = PaletteSetStyles.ShowAutoHideButton | PaletteSetStyles.ShowCloseButton;
            paletteSet.Add("MyPalette", new ElementHost() { Child = myUserControl });
            paletteSet.Visible = true;
            // End. Создает боковое меню


            //Для Windows
            // Сделать окно немодальным
            // FirstForm form = new FirstForm(new MyData(this));
            // form.Show();
            // Сделать окно поверх других окон
            // form.Topmost = true;
            //Отдельное окно Windows
            //Это блокирует окно NCada
            //Application.ShowModalWindow(form);

        }


        List<PointLine> GetWeightedVertices(List<PointLine> stateList)
        {
            List<PointLine> tempList = new List<PointLine>();
            foreach (PointLine itemPoint in stateList)
            {
                if (itemPoint.weightA > 0 | itemPoint.weightB > 0 | itemPoint.weightC > 0)
                {
                    if (itemPoint.typeClient == 1)
                    {
                        itemPoint.Ia = Math.Round(itemPoint.weightA * itemPoint.Ko * itemPoint.count / (0.22 * itemPoint.cos), 2);
                        itemPoint.Ib = Math.Round(itemPoint.weightB * itemPoint.Ko * itemPoint.count / (0.22 * itemPoint.cos), 2);
                        itemPoint.Ic = Math.Round(itemPoint.weightC * itemPoint.Ko * itemPoint.count / (0.22 * itemPoint.cos), 2);
                    }
                    if (itemPoint.typeClient == 3)
                    {
                        itemPoint.Ia = Math.Round(itemPoint.weightA * itemPoint.Ko * itemPoint.count / (Math.Sqrt(3) * 0.38 * itemPoint.cos), 2);
                        itemPoint.Ib = Math.Round(itemPoint.weightA * itemPoint.Ko * itemPoint.count / (Math.Sqrt(3) * 0.38 * itemPoint.cos), 2);
                        itemPoint.Ic = Math.Round(itemPoint.weightA * itemPoint.Ko * itemPoint.count / (Math.Sqrt(3) * 0.38 * itemPoint.cos), 2);
                    }
                    tempList.Add(itemPoint);
                }
            }
            return tempList;
        }
        //Рекурсия для поиска у какой вершине магистрали добавить вес
        PowerLine goToParent(PowerLine line, string searchNamePowerLine)
        {
            if (line.parent.name != searchNamePowerLine)
            {
                return goToParent(line.parent, searchNamePowerLine);
            }
            else
            {
                return line;
            }
        }


        PowerLine creatMagistral(Editor ed, Transaction trAdding, List<PointLine> listPoint, List<Point2d> listPointXY, List<PowerLine> listPowerLine)
        {
            PromptEntityOptions magistral = new PromptEntityOptions("\n\nВыберите магистраль: \n\n");
            PromptEntityResult perMagistral = ed.GetEntity(magistral);
            Polyline Plyline = trAdding.GetObject(perMagistral.ObjectId, OpenMode.ForRead) as Polyline;

            //Приблизить и подсветить
            Draw.ZoomToEntity(perMagistral.ObjectId, 4);
            ed.SetImpliedSelection(new ObjectId[] { perMagistral.ObjectId });

            PowerLine considerPowerLine = new PowerLine();
            int defult = BDSQL.searchAllDataInBD(dbFilePath, "cable", "default").IndexOf("true") + 1;


            considerPowerLine.cable = BDShowExtensionDictionaryContents<Conductor>(perMagistral.ObjectId, "ESMT_LEP_v1.0")?.Name
                ?? Text.creatPromptKeywordOptions("\n\nВыберите марку провода магистрали: ", BDSQL.searchAllDataInBD(dbFilePath, "cable", "name"), defult);
            considerPowerLine.Icrict = BDSQL.searchDataInBD<double>(dbFilePath, "cable", considerPowerLine.cable, "name", "Icrit");
            considerPowerLine.name = "Магистраль";
            considerPowerLine.IDLine = Plyline.ObjectId;
            considerPowerLine.parent = considerPowerLine;
            considerPowerLine.lengthLine = Math.Round(Plyline.Length, 3);

            listPowerLine.Add(considerPowerLine);

            return considerPowerLine;
        }



       



        PowerLine searchPlyline(Editor ed, PowerLine masterLine, Transaction trAdding, List<PointLine> listPoint, List<Point2d> listPointXY, int j)
        {
            Polyline polyline = trAdding.GetObject(masterLine.IDLine, OpenMode.ForRead) as Polyline;

            if (polyline != null)
            {
                List<PowerLine> childrenList = new List<PowerLine>();
                // Для чисто для визуала.
                int k = 1;

                // для себя i можно использовать как узел, от которого произошла отпайка
                // i=1 ; что бы не исать в первой вершине, когда выходит много из одной i=0 -дефолт j
                for (int i = j; i < polyline.NumberOfVertices; i++)
                {
                    // Поиск других полилиний вблизи текущей вершины
                    Point3d searchPoint = new Point3d(polyline.GetPoint2dAt(i).X, polyline.GetPoint2dAt(i).Y, 0);

                    //Филтр полилиний и проверка на замкнутость
                    SelectionFilter acSF = new SelectionFilter
                        (
                        new TypedValue[] 
                            { new TypedValue((int)DxfCode.Start, "LWPOLYLINE")
                             }

                        );


                    //вектор это допусе поиска это поиск окружность

                    Point3d center = new Point3d(polyline.GetPoint2dAt(i).X, polyline.GetPoint2dAt(i).Y, 0);
                    double radius = UserData.searchDistancePL;


                    // Определяем углы прямоугольника, описывающего круг
                    Point3d corner1 = new Point3d(center.X - radius, center.Y - radius, 0);
                    Point3d corner2 = new Point3d(center.X + radius, center.Y + radius, 0);

                    //Тут рамка
                    //PromptSelectionResult acPSR = ed.SelectCrossingWindow(corner1, corner2, acSF);

                    // Создаем многоугольник, приближающий окружность с центром в текущей вершине и радиусом searchDistance
                    Point3dCollection polygonPoints = Draw.createCirclePolygon(searchPoint, UserData.searchDistancePL, 36);


                    //Рисуем зону поиска
                    if (UserData.isDrawZoneSearchPL)
                    using (Transaction tr = MyOpenDocument.dbCurrent.TransactionManager.StartTransaction())
                    {
                        //Draw.drawZoneSearchPLRactangel (corner1, corner2, MyOpenDocument.dbCurrent, tr);
                        Draw.drawZoneSearchPLCircle (polygonPoints, MyOpenDocument.dbCurrent, tr, "Узлы_Saidi_Saifi_Makarov.D");

                            tr.Commit();
                    }

                    //Тут Полигон
                    PromptSelectionResult acPSR = ed.SelectCrossingPolygon(polygonPoints, acSF);


                    //Создаем поинты
                    PointLine point = new PointLine();
                    point.positionPoint = new Point2d(polyline.GetPoint2dAt(i).X, polyline.GetPoint2dAt(i).Y);
                    point.name = listPoint.Count + 1;

                    //Добавляем в общий список поинты и в класс powerLine
                    if (!listPointXY.Contains(point.positionPoint))
                    {
                        listPointXY.Add(point.positionPoint);
                        listPoint.Add(point);
                        masterLine.points.Add(point);
                    }

                    // Проверьте, были ли найдены какие-либо другие полилинии
                    if (acPSR.Status == PromptStatus.OK)
                    {

                        // Пройдите по найденным объектам
                        foreach (SelectedObject acSObj in acPSR.Value)
                        {
                            /*
                            ed.SetImpliedSelection(new ObjectId[] { acSObj.ObjectId });
                            creatPromptKeywordOptions("Заглушка",new List<string>() { "1"},1);
                           */
                            //Отсечь родителя 
                            if ( acSObj.ObjectId != masterLine.IDLine && acSObj.ObjectId != masterLine.parent.IDLine)
                            {

                               //Вытянуть длинну и посмотреть на циклицность.
                                Polyline lengthPolyline = trAdding.GetObject(acSObj.ObjectId, OpenMode.ForWrite) as Polyline;

                                //Если замкнута, пропустить
                                if (lengthPolyline.Closed) { continue; }
                                
                                //Приближаем
                                Draw.ZoomToEntity(acSObj.ObjectId, 4);
                                
                                //Расстояние между точками для проверки соединить их в одну точку  и 5 процентов запаса
                                if (Math.Round(lengthPolyline.GetPoint3dAt(0).DistanceTo(searchPoint), 4) != 0 && Math.Round(lengthPolyline.GetPoint3dAt(0).DistanceTo(searchPoint), 0) <= UserData.searchDistancePL  )
                                {

                                    //Тогда переносим вершину в нужную нам
                                    using (Transaction tr = MyOpenDocument.dbCurrent.TransactionManager.StartTransaction())
                                    {
                                        lengthPolyline.SetPointAt(0, new Point2d(searchPoint.X, searchPoint.Y));
                                        tr.Commit();
                                    }

                                    ed.WriteMessage("Вершина была не очень близко, я ее пододвинул");

                                }

                                //Проверка на цикличность есть Point Perent = last point, то разворачиваем полилинию 
                                int lastVertexIndex = lengthPolyline.NumberOfVertices - 1;
                                Point3d lastPoint = lengthPolyline.GetPoint3dAt(lastVertexIndex);

                                if (lastPoint == searchPoint)
                                {
                                    using (Transaction tr = MyOpenDocument.dbCurrent.TransactionManager.StartTransaction())
                                    {
                                        lengthPolyline.ReverseCurve();
                                        tr.Commit();
                                    }
                                    ed.WriteMessage("Линия была построена не от \"Питания\" к \"Нагрузки\", я ее развернул ");
                                }


                                //Подсветка что выделилось
                                ed.SetImpliedSelection(new ObjectId[] { acSObj.ObjectId });
                                //ed.CurrentUserCoordinateSystem = Matrix3d.Identity; // Сброс координатной системы, если необходимо


                                int defult = BDSQL.searchAllDataInBD(dbFilePath, "cable", "default").IndexOf("true") + 1;
                                PowerLine ChilderLine = new PowerLine();

                                ChilderLine.cable = BDShowExtensionDictionaryContents<Conductor>(acSObj.ObjectId, "ESMT_LEP_v1.0")?.Name
                                 ?? Text.creatPromptKeywordOptions("\n\nВыберите мару провода: ", BDSQL.searchAllDataInBD(dbFilePath, "cable", "name"), defult);


                                ChilderLine.IDLine = acSObj.ObjectId;
                                ChilderLine.parent = masterLine;
                                ChilderLine.lengthLine = Math.Round(lengthPolyline.Length, 3);
                                ChilderLine.Icrict = BDSQL.searchDataInBD<double>(dbFilePath, "cable", ChilderLine.cable, "name", "Icrit");

                                if (masterLine.name != "Магистраль")
                                {
                                    ChilderLine.name = "Отпайка № " + k.ToString() + " от " + masterLine.name; //Можно i, а не К.Будет показывать с какого именно узла
                                }
                                else
                                {
                                    ChilderLine.name = "Отпайка № " + k.ToString();//Можно i, а не К.Будет показывать с какого именно узлаа
                                }
                                /*
                                //Оставь Пригодится
                                ed.WriteMessage("\n Cilder:  " + ChilderLine.name + " | " + " ID: " + ChilderLine.IDLine + " | " + " Родитель: " + ChilderLine.parent.name + " | " + " Длина: " + Math.Round(ChilderLine.lengthLine, 2));
                                */

                                //Добавить в список детей
                                childrenList.Add(ChilderLine);
                                k++;
                            }

                        }
                    }
                }

                //Нужно только для 1ого раза,что б не циклило, когда много отпаек из одной точки				
                j = 1;

                // В сумме три текста, они все связанны
                /*
                if (childrenList.Count != 0)
                {

                    ed.WriteMessage("\n children Кол. : " + childrenList.Count);
                    ed.WriteMessage("\n ~~~~~~~~~~");
                    ed.WriteMessage("\n ");
                    ed.WriteMessage("\n ");
                }*/


                //Прокидывает детей в родителя
                masterLine.children = childrenList;


                //Рекурсия
                foreach (PowerLine line in childrenList)
                {
                    searchPlyline(MyOpenDocument.ed, line, trAdding, listPoint, listPointXY, j);
                }
            }
            return masterLine;
        }



        public class TKZ
        {

            public double transformersX { get; set; }
            public double transformersX0 { get; set; }
            public double transformersR { get; set; }
            public double transformersR0 { get; set; }
            public double length { get; set; }
            public double lineX { get; set; }
            public double lineX0 { get; set; }
            public double lineR { get; set; }
            public double lineR0 { get; set; }
            public double linePetlia { get; set; }

            public double amps { get; set; }
            public double resultTKZ { get; set; }
            public PointLine pointTKZ { get; set; }
            public List<Edge> pathEdgeTKZ { get; set; }
            public List<PointLine> pathPointTKZ { get; set; }

            public TKZ()
            {
                transformersX = 0;
                transformersX0 = 0;
                transformersR = 0;
                transformersR0 = 0;
                lineX = 0;
                lineX0 = 0;
                lineR = 0;
                lineR0 = 0;
                linePetlia = 0;
                amps = 0;
                resultTKZ = 0;
                length = 0;
                pointTKZ = new PointLine();
                pathEdgeTKZ = new List<Edge>();
                pathPointTKZ = new List<PointLine>();
            }

        }




        public class SaidiSaifi
        {
            public PointLine point { get; set; }
            public double difference { get; set; }
            public List<PointLine> leftPath { get; set; }
            public List<PointLine> rightPath { get; set; }
            public string leftPathText { get; set; }
            public string righPathText { get; set; }
            public double LeftWeight { get; set; }
            public double RighWeight { get; set; }
            public Edge Edge { get; set; }


            public SaidiSaifi()
            {
                point = new PointLine();
                difference = 1000000;
                leftPath = new List<PointLine>();
                rightPath = new List<PointLine>();
                leftPathText = "";
                righPathText = "";
                LeftWeight = 0;
                RighWeight = 0;
            }

        }

        [XmlRoot("Conductor")]
        public class Conductor
        {
            [XmlElement("Name")]
            public string Name { get; set; }

            /*
		    [XmlElement("ObjectType")]
		    public string ObjectType { get; set; }
		
		    [XmlElement("Number")]
		    public int Number { get; set; }
		
		    [XmlElement("Length_redef")]
		    public int LengthRedef { get; set; }
		
		    [XmlElement("UseRedefLength")]
		    public bool UseRedefLength { get; set; }
		
		    [XmlElement("AdditionalPercent")]
		    public int AdditionalPercent { get; set; }
		
		    [XmlElement("Multiplier")]
		    public int Multiplier { get; set; }
		
		    [XmlElement("AdditionalLength1")]
		    public int AdditionalLength1 { get; set; }
		
		    [XmlElement("AdditionalLength2")]
		    public int AdditionalLength2 { get; set; }
		
		    [XmlElement("Start")]
		    public string Start { get; set; }
		
		    [XmlElement("End")]
		    public string End { get; set; }
		
		    [XmlArray("Parts")]
		    [XmlArrayItem("Part")]
		    public List<string> Parts { get; set; }
		
		    [XmlArray("Parts_1m")]
		    [XmlArrayItem("Part_1m")]
		    public List<string> Parts1m { get; set; } */

            public Conductor()
            {
                Name = "";
            }
        }

        //Нигде пока не применил 
        [Serializable]
        [XmlRoot("Matrix")]
        public class SerializableMatrix
        {
            [XmlArray("Rows")]
            [XmlArrayItem("Row")]
            public List<int[]> Rows { get; set; }

            public SerializableMatrix()
            {
                Rows = new List<int[]>();
            }

            public SerializableMatrix(int[,] matrix)
            {
                Rows = new List<int[]>();

                int rows = matrix.GetLength(0);
                int cols = matrix.GetLength(1);

                for (int i = 0; i < rows; i++)
                {
                    int[] row = new int[cols];
                    for (int j = 0; j < cols; j++)
                    {
                        row[j] = matrix[i, j];
                    }
                    Rows.Add(row);
                }
            }

            public int[,] ToMatrix()
            {
                if (Rows.Count == 0 || Rows[0].Length == 0)
                    return new int[0, 0];

                int rows = Rows.Count;
                int cols = Rows[0].Length;
                int[,] matrix = new int[rows, cols];

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        matrix[i, j] = Rows[i][j];
                    }
                }

                return matrix;
            }
        }

        //Дополнительные функции 






        //Создание списка всех Powerline
        List<PowerLine> creatListPowerLine(PowerLine masterLine)
        {
            List<PowerLine> tempList = new List<PowerLine>();

            if (masterLine.name == "Магистраль")
            {
                tempList.Add(masterLine);
            }

            foreach (PowerLine line in masterLine.children)
            {
                tempList.Add(line);
                tempList.AddRange(creatListPowerLine(line));
            }
            return tempList;
        }








        //Добавляем вершину родителя
        void addPointPerent(List<PowerLine> masterlist, List<PointLine> listPointXY, Transaction trAdding)
        {
            foreach (PowerLine itemLine in masterlist)
            {

                if (itemLine.name != "Магистраль")
                {
                    Polyline polyline = trAdding.GetObject(itemLine.IDLine, OpenMode.ForRead) as Polyline;
                    Point2d searchPoint = new Point2d(polyline.GetPoint2dAt(0).X, polyline.GetPoint2dAt(0).Y);

                    foreach (PointLine itemPoint in listPointXY)
                    {
                        if (searchPoint.X == itemPoint.positionPoint.X && searchPoint.Y == itemPoint.positionPoint.Y)
                        {
                            itemLine.parentPoint = itemPoint;
                            itemLine.points.Insert(0, itemPoint);
                        }
                    }


                }

            }
        }


        //Добавляем Последней вершины
        void addEndPoint(List<PowerLine> masterlist, List<PointLine> listPointXY, Transaction trAdding)
        {
            foreach (PowerLine itemLine in masterlist)
            {
                itemLine.endPoint = itemLine.points.Last();
                itemLine.endPoint.isLastPoint = true;
            }
        }

        //Создания последних поинтов без отпаек
        void creatListLastPoint()
        {
            foreach (PointLine itemPoint in listPoint)
            {
                if (itemPoint.isLastPoint == true)
                {
                    listLastPoint.Add(itemPoint);
                }
            }

        }







        //Создание листа Ребер
        List<Edge> creatListEdegs(List<PowerLine> masterList)
        {
            List<Edge> tempListEdge = new List<Edge>();

            foreach (PowerLine itemLine in masterList)
            {
                for (int i = 0; i < itemLine.points.Count - 1; i++)
                {
                    Edge newEdge = new Edge
                    {
                        name = tempListEdge.Count() + 1,
                        startPoint = itemLine.points[i],
                        endPoint = itemLine.points[i + 1],
                        length = Math.Round(itemLine.points[i].positionPoint.GetDistanceTo(itemLine.points[i + 1].positionPoint), 4),
                        cable = itemLine.cable,
                        r = BDSQL.searchDataInBD<double>(dbFilePath, "cable", itemLine.cable, "name", "r"),
                        x = BDSQL.searchDataInBD<double>(dbFilePath, "cable", itemLine.cable, "name", "x"),
                        r0 = BDSQL.searchDataInBD<double>(dbFilePath, "cable", itemLine.cable, "name", "r0"),
                        x0 = BDSQL.searchDataInBD<double>(dbFilePath, "cable", itemLine.cable, "name", "x0"),
                        rN = BDSQL.searchDataInBD<double>(dbFilePath, "cable", itemLine.cable, "name", "rN"),
                        xN = BDSQL.searchDataInBD<double>(dbFilePath, "cable", itemLine.cable, "name", "xN"),
                        Ke = BDSQL.searchDataInBD<double>(dbFilePath, "cable", itemLine.cable, "name", "Ke"),
                        Ce = BDSQL.searchDataInBD<double>(dbFilePath, "cable", itemLine.cable, "name", "Ce"),

                        Icrict = BDSQL.searchDataInBD<double>(dbFilePath, "cable", itemLine.cable, "name", "Icrit"),
                        centerPoint = new PointLine
                        {
                            name = int.Parse((itemLine.points[i].name.ToString() + "0" + itemLine.points[i + 1].name.ToString())),
                            positionPoint = new Point2d((itemLine.points[i].positionPoint.X + itemLine.points[i + 1].positionPoint.X) / 2, (itemLine.points[i].positionPoint.Y + itemLine.points[i + 1].positionPoint.Y) / 2)
                        }

                    };

                    tempListEdge.Add(newEdge);


                }

            }

            return tempListEdge;

        }

        //Описать линию ребрами
        public void insertEdgeInPowerLine(List<PowerLine> masterListPowerLine, List<Edge> masterListEdge)
        {
            foreach (PowerLine itemLine in masterListPowerLine)
            {
                for (int i = 0; i < itemLine.points.Count - 1; i++)
                {
                    foreach (Edge itemEdge in masterListEdge)
                    {
                        if (itemLine.points[i] == itemEdge.startPoint && itemLine.points[i + 1] == itemEdge.endPoint)
                        {
                            itemLine.Edges.Add(itemEdge);
                        }

                    }
                }

            }

        }


        //Функция обхода вершин
        List<int> findPath(int[,] adjacencyMatrix, int tempStartPoint, int tempEndPoint)
        {
            bool isRevers = false;
            if (tempStartPoint < tempEndPoint)
            {
                int dif = tempStartPoint;
                tempStartPoint = tempEndPoint;
                tempEndPoint = dif;
                isRevers = true;
            }

            if (tempStartPoint == tempEndPoint)
            {
                return new List<int> { tempStartPoint };

            }

            bool[] visited = new bool[adjacencyMatrix.GetLength(0)];
            List<int> path = new List<int>();
            visited[tempStartPoint] = true;
            if (tempStartPoint == tempEndPoint)
            {

                return new List<int>() { tempStartPoint };

            }


            for (int i = 0; i < visited.Length; i++)
            {

                if (adjacencyMatrix[tempStartPoint, i] == 1 & !visited[i])
                {
                    path = findPath(adjacencyMatrix, i, tempEndPoint);

                    if (path != null)
                    {
                        path.Insert(0, tempStartPoint);

                        if (isRevers)
                        {
                            path.Reverse();
                        }
                        return path;
                    }
                }

            }
            //Заглушка
            return new List<int> { 0 };
        }

        List<PointLine> ListPathIntToPoint(List<int> masterList)
        {

            List<PointLine> tempList = new List<PointLine>();
            foreach (int itemInt in masterList)
            {
                foreach (PointLine itemName in listPoint)
                {
                    if (itemName.name == (itemInt + 1))
                    {

                        tempList.Add(itemName);
                    }
                }
            }
            return tempList;
        }




        //Создание матрицы инцинденций Узел Ветвь 
        int[,] CreatMatrixInc(List<PointLine> masterListPoint, List<Edge> masterListEdge)
        {
            int rows = masterListPoint.Count();
            int columns = masterListEdge.Count();
            int[,] matrix = new int[rows, columns];

            for (int j = 0; j < rows; j++)
            {
                for (int i = 0; i < columns; i++)
                {
                    if (masterListPoint[j] == masterListEdge[i].startPoint)
                    {
                        matrix[j, i] = -1;
                    }

                    if (masterListPoint[j] == masterListEdge[i].endPoint)
                    {
                        matrix[j, i] = 1;
                    }
                }
            }
            return matrix;
        }








        //Создание матрицы смежности узел узел
        int[,] сreatMatrixSmej(List<PointLine> masterListPoint, List<Edge> masterListEdge)
        {
            int size = masterListPoint.Count;
            int[,] matrix = new int[size, size];

            for (int j = 0; j < size; j++)
            {
                for (int i = 0; i < size; i++)
                {
                    if (i != j)
                    {
                        foreach (Edge itemEdge in masterListEdge)
                        {
                            if ((masterListPoint[j] == itemEdge.startPoint) && (masterListPoint[i] == itemEdge.endPoint))
                            {
                                matrix[j, i] = 1;
                            }

                            if ((masterListPoint[j] == itemEdge.endPoint) && (masterListPoint[i] == itemEdge.startPoint))
                            {
                                matrix[j, i] = 1;
                            }

                        }
                    }

                }

            }
            return matrix;
        }




        TKZ сreatTKZ(bool isI1TKZ)
        {
            TKZ info = new TKZ();
            List<Edge> newPathKZEdge = new List<Edge>();
            List<Edge> oldPathKZ = new List<Edge>();
            List<PointLine> newPointPathKZ = new List<PointLine>();
            List<PointLine> oldPointPathKZ = new List<PointLine>();
            double oldData = 0.0;
            double newData = 0.0;
            double oldDist = 0.0;
            double newDist = 0.0;
            double oldlineX = 0.0;
            double newlineX = 0.0;
            double oldlineR = 0.0;
            double newlineR = 0.0;

            double oldlineX0 = 0.0;
            double newlineX0 = 0.0;
            double oldlineR0 = 0.0;
            double newlineR0 = 0.0;

            foreach (PointLine itemPoint in listLastPoint)
            {
                List<int> listInt = findPath(matrixSmej, itemPoint.name - 1, 0);
                newPointPathKZ = ListPathIntToPoint(listInt);

                for (int i = 0; i < newPointPathKZ.Count() - 1; i++)
                {
                    foreach (Edge item in listEdge)
                    {
                        if (((newPointPathKZ[i] == item.startPoint) || ((newPointPathKZ[i] == item.endPoint))) && (((newPointPathKZ[i + 1] == item.startPoint) || ((newPointPathKZ[i + 1] == item.endPoint)))))
                        {
                            newPathKZEdge.Add(item);
                        }
                    }

                    foreach (Edge item in newPathKZEdge)
                    {
                        newlineR = newlineR + (item.r * item.length);
                        newlineX = newlineX + (item.x * item.length);
                        newlineR0 = newlineR0 + (item.r0 * item.length);
                        newlineX0 = newlineX0 + (item.x0 * item.length);

                        if (isI1TKZ)
                        {
                            //Однофазное
                            newData = newData + Math.Sqrt(Math.Pow((2 * (item.r * item.length) + (item.r0 * item.length)), 2) + Math.Pow(2 * (item.x * item.length) + (item.x0 * item.length), 2));
                        }
                        else
                        {
                            //Трехфазное
                            newData = newData + Math.Sqrt(Math.Pow(item.r * item.length, 2) + Math.Pow(item.x * item.length, 2));
                        }


                        newDist = newDist + item.length;
                    }

                    if (newData > oldData)
                    {
                        oldlineX = newlineX;
                        oldlineR = newlineR;
                        oldlineX0 = newlineX0;
                        oldlineR0 = newlineR0;
                        oldData = newData;
                        oldDist = newDist;
                        oldPointPathKZ = newPointPathKZ;
                    }
                    newData = 0.0;
                    newDist = 0.0;
                    newlineX = 0;
                    newlineR = 0;
                    newlineX0 = 0;
                    newlineR0 = 0;

                }
                newPathKZEdge.Clear();
                /*newData = 0.0;
                newDist = 0.0;*/
            }

            info.pathPointTKZ = oldPointPathKZ;
            info.linePetlia = oldData;
            info.lineX = oldlineX;
            info.lineR = oldlineR;
            info.lineX0 = oldlineX0;
            info.lineR0 = oldlineR0;
            info.length = oldDist;
            info.pointTKZ = info.pathPointTKZ[0];
            info.pathEdgeTKZ = oldPathKZ;

            return info;
        }

        TKZ сreatMyTKZ(int Point)
        {
            TKZ info = new TKZ();
            List<Edge> newPathKZEdge = new List<Edge>();
            List<Edge> oldPathKZ = new List<Edge>();
            List<PointLine> newPointPathKZ = new List<PointLine>();
            List<PointLine> oldPointPathKZ = new List<PointLine>();
            double oldData = 0.0;
            double newData = 0.0;
            double oldDist = 0.0;
            double newDist = 0.0;
            double oldlineX = 0.0;
            double newlineX = 0.0;
            double oldlineR = 0.0;
            double newlineR = 0.0;

            double oldlineX0 = 0.0;
            double newlineX0 = 0.0;
            double oldlineR0 = 0.0;
            double newlineR0 = 0.0;


            newPointPathKZ = ListPathIntToPoint(findPath(matrixSmej, Point - 1, 0));

            //Для 1ой вершины
            if (newPointPathKZ.Count() == 1)
            {
                MyOpenDocument.ed.WriteMessage("!Нельзя выбрать точку генерации!");
                return null;
            }


            for (int i = 0; i < newPointPathKZ.Count() - 1; i++)
            {
                foreach (Edge item in listEdge)
                {
                    if (((newPointPathKZ[i] == item.startPoint) || ((newPointPathKZ[i] == item.endPoint))) && (((newPointPathKZ[i + 1] == item.startPoint) || ((newPointPathKZ[i + 1] == item.endPoint)))))
                    {
                        newPathKZEdge.Add(item);
                    }
                }

                foreach (Edge item in newPathKZEdge)
                {
                    newlineR = newlineR + (item.r * item.length);
                    newlineX = newlineX + (item.x * item.length);
                    newlineR0 = newlineR0 + (item.r0 * item.length);
                    newlineX0 = newlineX0 + (item.x0 * item.length);

                    newData = newData + Math.Sqrt(Math.Pow(((2 * item.r * item.length) + (item.r0 * item.length)), 2) + Math.Pow((2 * item.x * item.length) + (item.x0 * item.length), 2));
                    newDist = newDist + item.length;
                }

                if (newData > oldData)
                {
                    oldlineX = newlineX;
                    oldlineR = newlineR;
                    oldlineX0 = newlineX0;
                    oldlineR0 = newlineR0;
                    oldData = newData;
                    oldDist = newDist;
                    oldPointPathKZ = newPointPathKZ;
                }
                newData = 0.0;
                newDist = 0.0;
                newlineX = 0;
                newlineR = 0;
                newlineX0 = 0;
                newlineR0 = 0;

            }
            newPathKZEdge.Clear();
            /*newData = 0.0;
            newDist = 0.0;*/


            info.pathPointTKZ = oldPointPathKZ;
            info.linePetlia = oldData;
            info.lineX = oldlineX;
            info.lineR = oldlineR;
            info.lineX0 = oldlineX0;
            info.lineR0 = oldlineR0;
            info.length = oldDist;
            info.pointTKZ = info.pathPointTKZ[0];
            info.pathEdgeTKZ = oldPathKZ;

            return info;
        }

        //Вставка блоков в вершины
        public void InsertBlockAtVertices()
        {

            bool isNumBlock = false;
            bool isCreatFileExcelCoordinate = false;
            int startNumber = 0;
            string sufBlockName = "";
            string prefBlockName = "";
            List<ObjectId> listObjectID = new List<ObjectId>();
            List<double> listX = new List<double>();
            List<double> listY = new List<double>();


            PromptStringOptions promptForBlockName = new PromptStringOptions("\nВведите название блока [" + UserData.defaultBlock + "]: ");
            promptForBlockName.AllowSpaces = true;
            PromptResult blockNameResult = MyOpenDocument.ed.GetString(promptForBlockName);

            if (blockNameResult.Status != PromptStatus.OK) return;
            string blockName = blockNameResult.StringResult;



            //нужна ли нумерация
            PromptKeywordOptions options = new PromptKeywordOptions("\nБудем нумеровать блок? [Да/Нет] : ");
            options.Keywords.Add("Да");
            options.Keywords.Add("Нет");
            PromptResult resultYesNo = MyOpenDocument.ed.GetKeywords(options);
            if (resultYesNo.Status != PromptStatus.OK) return;

            if (resultYesNo.StringResult == "Да")
            {
                isNumBlock = true;

                //Результат в Excel
                PromptKeywordOptions optionsCreatFileExcelCoordinate = new PromptKeywordOptions("\nВывести результат координат в Excel? [Да/Нет] : ");
                optionsCreatFileExcelCoordinate.Keywords.Add("Да");
                optionsCreatFileExcelCoordinate.Keywords.Add("Нет");
                PromptResult resultCreatFileExcelCoordinateo = MyOpenDocument.ed.GetKeywords(optionsCreatFileExcelCoordinate);
                if (resultCreatFileExcelCoordinateo.Status != PromptStatus.OK) return;
                if (resultCreatFileExcelCoordinateo.StringResult == "Да") isCreatFileExcelCoordinate = true;
            }



            if (isNumBlock)
            {

                //Старт нумерации
                PromptIntegerOptions startNumBlock = new PromptIntegerOptions("\n С какого числа начать нумерацию? [1] ");
                PromptIntegerResult startNumBlockResult = MyOpenDocument.ed.GetInteger(startNumBlock);

                if (startNumBlockResult.Status != PromptStatus.OK) return;
                startNumber = startNumBlockResult.Value;


                //Суффикс
                PromptStringOptions promptSufBlockName = new PromptStringOptions("\nВведите суффикс (или пусто) ? []: ");
                promptSufBlockName.AllowSpaces = true;
                PromptResult sufBlockNameResult = MyOpenDocument.ed.GetString(promptSufBlockName);

                if (sufBlockNameResult.Status != PromptStatus.OK) return;
                sufBlockName = sufBlockNameResult.StringResult;

                //Преффикс

                PromptStringOptions promptPrefBlockName = new PromptStringOptions("\nВведите преффикс (или пусто) ? []: ");
                promptPrefBlockName.AllowSpaces = true;
                PromptResult prefBlockNameResult = MyOpenDocument.ed.GetString(promptPrefBlockName);

                if (prefBlockNameResult.Status != PromptStatus.OK) return;
                prefBlockName = prefBlockNameResult.StringResult;
            }




            using (Transaction tr = MyOpenDocument.dbCurrent.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(MyOpenDocument.dbCurrent.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Проверяем, существует ли блок с введенным именем
                if (!bt.Has(blockName))
                {
                    MyOpenDocument.ed.WriteMessage("\nБлок '" + blockName + "' не найден.");
                    return;
                }

                // Запрашиваем у пользователя выбор полилинии
                PromptEntityOptions peo = new PromptEntityOptions("\nВыберите полилинию:");
                peo.SetRejectMessage("\nМожно выбрать только полилинию.");
                peo.AddAllowedClass(typeof(Polyline), true);
                PromptEntityResult per = MyOpenDocument.ed.GetEntity(peo);

                if (per.Status != PromptStatus.OK) return;

                ObjectId plId = per.ObjectId;

                Polyline pl = tr.GetObject(plId, OpenMode.ForRead) as Polyline;

                // Перебираем вершины полилинии
                for (int i = 0; i < pl.NumberOfVertices; i++)
                {
                    Point3d pt = pl.GetPoint3dAt(i);
                    listX.Add(pl.GetPoint3dAt(i).X);
                    listY.Add(pl.GetPoint3dAt(i).Y);

                    // Создаем новый экземпляр блока
                    using (BlockReference br = new BlockReference(pt, bt[blockName]))
                    {
                        BlockTableRecord ms = tr.GetObject(MyOpenDocument.dbCurrent.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                        ms.AppendEntity(br);
                        tr.AddNewlyCreatedDBObject(br, true);

                        listObjectID.Add(br.ObjectId);


                        if (isNumBlock)
                        {
                            // Получаем определение блока
                            BlockTableRecord blkDef = tr.GetObject(bt[blockName], OpenMode.ForRead) as BlockTableRecord;

                            // Ищем первый атрибут в блоке и изменяем его текст
                            foreach (ObjectId objId in blkDef)
                            {
                                DBObject obj = tr.GetObject(objId, OpenMode.ForRead);
                                if (obj is AttributeDefinition attDef)
                                {
                                    using (AttributeReference attRef = new AttributeReference())
                                    {
                                        // Найден атрибут, устанавливаем новое значение текста
                                        attRef.SetAttributeFromBlock(attDef, br.BlockTransform);
                                        attRef.TextString = sufBlockName + (startNumber + i).ToString() + prefBlockName;
                                        br.AttributeCollection.AppendAttribute(attRef);
                                        tr.AddNewlyCreatedDBObject(attRef, true);
                                        break;
                                    }
                                }
                            }


                        }
                    }
                }



                tr.Commit();

                //куда сохранить
                if (isCreatFileExcelCoordinate)
                {
                    Excel.creatFileExcelCoodrinate(startNumber, listX, listY, per.ObjectId);
                }

                MyOpenDocument.ed.SetImpliedSelection(listObjectID.ToArray());

            }
        }





        public T BDShowExtensionDictionaryContents<T>(ObjectId entityId, string nameDictionary)
        {
            // Проверка входных данных
            if (entityId.IsNull)
                throw new ArgumentNullException("entityId");

            if (string.IsNullOrEmpty(nameDictionary))
                throw new ArgumentNullException("nameDictionary");


            using (DocumentLock doclock = MyOpenDocument.doc.LockDocument())
            {
                using (Transaction tr = MyOpenDocument.dbCurrent.TransactionManager.StartTransaction())
                {
                    try
                    {
                        // Получение объекта
                        Entity entity = tr.GetObject(entityId, OpenMode.ForRead) as Entity;

                        // Проверка объекта
                        if (entity == null)
                            throw new ArgumentException("Entity not found.", "entityId");

                        // Получение словаря расширений
                        DBDictionary extDict = entity.ExtensionDictionary != ObjectId.Null ? tr.GetObject(entity.ExtensionDictionary, OpenMode.ForRead) as DBDictionary : null;

                        // Проверка словаря расширений
                        if (extDict == null)

                            return default(T);
                        //throw new ArgumentException("ExtensionDictionary not found.", "entityId");

                        // Проверка наличия записи
                        if (!extDict.Contains(nameDictionary))
                            return default(T);

                        // Получение записи
                        ObjectId entryId = extDict.GetAt(nameDictionary);
                        DBObject entryObj = tr.GetObject(entryId, OpenMode.ForRead);

                        // Проверка типа записи
                        if (entryObj is Xrecord xRecord)
                        {
                            // Десериализация
                            return DeserializeFromXrecord<T>(xRecord);
                        }
                        else
                        {
                            // Ошибка: Неверный тип записи
                            throw new InvalidOperationException("Entry is not of type Xrecord.");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Логирование ошибки


                        // Возвращение null
                        return default(T);
                    }
                }
            }
        }



        public T DeserializeFromXrecord<T>(Xrecord xRecord)
        {
            try
            {
                if (xRecord == null)
                    throw new ArgumentNullException("xRecord");

                // Получение данных Xrecord
                ResultBuffer data = xRecord.Data;
                if (data == null)
                    throw new ArgumentException("Xrecord does not contain valid data.", "xRecord");

                // Преобразование данных в массив TypedValue
                TypedValue[] values = data.AsArray();

                // Проверка наличия единственного текстового значения
                if (values.Length == 1 && values[0].TypeCode == (int)DxfCode.Text)
                {
                    // Получение XML-данных
                    string xmlData = values[0].Value.ToString();

                    // Десериализация XML
                    return DeserializeFromXml<T>(xmlData);
                }
                else
                {
                    // Ошибка: Неожиданный формат данных
                    throw new ArgumentException("Unexpected data format in Xrecord.", "xRecord");
                }
            }
            catch (InvalidOperationException ex)
            {
                // Логирование ошибки


                // Возвращение null
                return default(T);
            }
            catch (Exception ex)
            {
                // Логирование непредвиденных ошибок


                // Возвращение null
                return default(T);
            }
        }


        public T DeserializeFromXml<T>(string xmlData)
        {
            if (string.IsNullOrEmpty(xmlData))
                throw new ArgumentNullException("xmlData");
            try
            {

                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (StringReader reader = new StringReader(xmlData))
                {
                    return (T)serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                MyOpenDocument.ed.WriteMessage("Error during deserialization: " + ex.Message);
                // Обработайте ошибку в соответствии с вашими потребностями
                throw;
            }
        }


        private static string SerializeToXml<T>(T obj)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                StringWriter writer = new StringWriter();
                serializer.Serialize(writer, obj);

                // Указываем путь к файлу на рабочем столе
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filePath = Path.Combine(desktopPath, "output.xml");

                // Записываем XML-строку в файл
                // File.WriteAllText(filePath, writer.ToString());

                return writer.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сериализации: {ex.Message}");
                return null;
            }

        }

        private void SaveXmlToXrecord(string xmlData, ObjectId entityId, string nameDictionary)
        {
            using (DocumentLock doclock = MyOpenDocument.doc.LockDocument())
            {
                using (Transaction tr = MyOpenDocument.dbCurrent.TransactionManager.StartTransaction())
                {
                    try
                    {
                        Entity entity = tr.GetObject(entityId, OpenMode.ForWrite) as Entity;

                        if (entity != null)
                        {
                            if (entity.ExtensionDictionary == ObjectId.Null)
                            {
                                // Если у сущности еще нет словаря, создаем его
                                entity.CreateExtensionDictionary();
                            }

                            // Получение словаря
                            DBDictionary extDict = tr.GetObject(entity.ExtensionDictionary, OpenMode.ForWrite) as DBDictionary;


                            // Добавление или обновление записи в словаре
                            if (extDict.Contains(nameDictionary))
                            {

                                ObjectId entryIdq = extDict.GetAt(nameDictionary);
                                DBObject entryObj = tr.GetObject(entryIdq, OpenMode.ForWrite);
                                entryObj.UpgradeOpen();
                                entryObj.Erase(true);
                                entryObj.DowngradeOpen();

                            }

                            // Создание Xrecord
                            Xrecord xRecord = new Xrecord();
                            TypedValue tv = new TypedValue((int)DxfCode.Text, xmlData);
                            xRecord.Data = new ResultBuffer(tv);

                            ObjectId entryId = extDict.SetAt(nameDictionary, xRecord);
                            tr.AddNewlyCreatedDBObject(xRecord, true);


                        }

                        tr.Commit();
                    }

                    catch (System.Exception ex)
                    {
                        MyOpenDocument.ed.WriteMessage("Ошибка: {0}\n", ex.Message);
                    }
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Tools_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(listPoint));
        }


        private void DocumentActivatedEventHandler(object sender, DocumentCollectionEventArgs e)
        {
            Document activatedDocument = e.Document;
            Editor ed = activatedDocument.Editor;

            if (ed != this.oldEd)
            {
                //ed.WriteMessage("\nДокумент активирован: {0}", activatedDocument.Name);
                //MessageBox.Show(activatedDocument.Name);
                _myData.isLock = !true;
                _myData.isLoadProcessAnim = true;


                
             

            }
            else
            {
                _myData.isLock = true;
                _myData.isLoadProcessAnim = false;
            }



        }








    }



}