
#region Namespaces


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Data.SQLite;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Windows.Forms.Integration;



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
using HostMgd.Windows;

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

    public partial class Tools : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Document doc;
        public Database dbCurrent;
        public Editor ed;
        //Для передачи Lock состояни
        private MyData _myData;
        private NetWork _netWork = new NetWork();
        private string _pathDLLFile;
        private List<PointLine> _listPoint = new List<PointLine>();
        private List<Edge> _listEdge = new List<Edge>();
        private List<PowerLine> _listPowerLine = new List<PowerLine>();


        public Tools()
        {
            this.doc = Application.DocumentManager.MdiActiveDocument;
            this.dbCurrent = Application.DocumentManager.MdiActiveDocument.Database;
            this.ed = Application.DocumentManager.MdiActiveDocument.Editor;
            MyOpenDocument.ed = ed;
            MyOpenDocument.doc = doc;
            MyOpenDocument.dbCurrent = dbCurrent;

            var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var assemblies = new string[]
            {
                "MaterialDesignColors.dll",
                "MaterialDesignThemes.MahApps.dll",
                "MaterialDesignThemes.Wpf.dll",
            };

            foreach (var assembly in assemblies)
            {
                Assembly.LoadFrom(Path.Combine(currentDir, assembly));
            }



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
        private string dbFilePath = "";

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


        /*
        [CommandMethod("фв", CommandFlags.UsePickSet |
                       CommandFlags.Redraw | CommandFlags.Modal)] // название команды, вызываемой в Autocad */



        public  void addInfo()
        {
            //подписывается на обновление чертежа
            Application.DocumentManager.DocumentActivated += DocumentActivatedEventHandler;

            //Обновляем
            doc = Application.DocumentManager.MdiActiveDocument;
            dbCurrent = Application.DocumentManager.MdiActiveDocument.Database;
            ed = Application.DocumentManager.MdiActiveDocument.Editor;

            OnPropertyChanged(nameof(ed));

            //Получить Путь где лежит DLL
            Assembly assembly = Assembly.GetExecutingAssembly();
            pathDLLFile = assembly.Location;

            //Получаем путь для BD
            int lastDelimiterIndex = pathDLLFile.LastIndexOf("\\");
            if (lastDelimiterIndex != -1)
            {
                dbFilePath = pathDLLFile.Substring(0, lastDelimiterIndex).Trim();
            }
            dbFilePath = @dbFilePath + "\\DataBD.db";


            //Нужное
            listPoint.Clear();
            listPowerLine.Clear();
            listPointXY.Clear();
            listEdge.Clear();
            listLastPoint.Clear();
            matrixInc = null; matrixSmej = null; matrixVertexWeight = null; matrixEdjeWeight = null;


            int j = 0;

            int defult = searchAllDataInBD(dbFilePath, "text", "default").IndexOf("true") + 1;
            string sizeTextPoint = creatPromptKeywordOptions("Выберите высотку текста для вершины и ребра", searchAllDataInBD(dbFilePath, "text", "size"), defult);
            if (string.IsNullOrEmpty(sizeTextPoint)) { return; }
            string sizeTextLine = creatPromptKeywordOptions("Выберите высотку текста названия линии", searchAllDataInBD(dbFilePath, "text", "size"), defult);
            if (string.IsNullOrEmpty(sizeTextLine)) { return; }



            //Создание слоев
            creatLayer("Узлы_Saidi_Saifi_Makarov.D", 0, 127, 0, ed, dbCurrent);
            creatLayer("Граф_Saidi_Saifi_Makarov.D", 76, 153, 133, ed, dbCurrent);
            creatLayer("НазванияЛиний_Saidi_Saifi_Makarov.D", 0, 191, 255, ed, dbCurrent);
            creatLayer("Ребра_Saidi_Saifi_Makarov.D", 255, 191, 0, ed, dbCurrent);
            creatLayer("TKZ_Makarov.D", 255, 0, 0, ed, dbCurrent);
            creatLayer("Напряжение_Makarov.D", 0, 255, 63, ed, dbCurrent);


            //Удалить все объекты со слоев
            deleteObjectsOnLayer("Узлы_Saidi_Saifi_Makarov.D", dbCurrent);
            deleteObjectsOnLayer("Граф_Saidi_Saifi_Makarov.D", dbCurrent);
            deleteObjectsOnLayer("НазванияЛиний_Saidi_Saifi_Makarov.D", dbCurrent);
            deleteObjectsOnLayer("Ребра_Saidi_Saifi_Makarov.D", dbCurrent);
            deleteObjectsOnLayer("TKZ_Makarov.D", dbCurrent);
            deleteObjectsOnLayer("Напряжение_Makarov.D", dbCurrent);


            using (DocumentLock docloc = doc.LockDocument())
            {
                using (Transaction trAdding = dbCurrent.TransactionManager.StartTransaction())
                {
                    //Создаем магистраль
                    PowerLine magistralLine = CrearMagistral(ed, trAdding, listPoint, listPointXY, listPowerLine);

                    //Ищем все отпайки у магистрали и у их детей и делает листпоинт
                    searchPlyline(ed, magistralLine, trAdding, listPoint, listPointXY, j);


                    //Собрать весь список PowerLine
                    listPowerLine = creatListPowerLine(magistralLine);

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
                    creatTextFromKnot("Узлы_Saidi_Saifi_Makarov.D", listPoint, sizeTextPoint);

                    //Создает наименования у каждого ребра
                    creatTextFromEdge("Ребра_Saidi_Saifi_Makarov.D", listEdge, sizeTextPoint);

                    //Создает наименования у линии
                    creatTextFromLine("НазванияЛиний_Saidi_Saifi_Makarov.D", listPowerLine, sizeTextLine);

                    // Иначе рабоать не будет				
                    trAdding.Commit();

                }
            }

            OnPropertyChanged(nameof(listPoint));
            OnPropertyChanged(nameof(listEdge));
            OnPropertyChanged(nameof(listPowerLine));

        }


        // Fun Для получения ID
        [CommandMethod("фв2", CommandFlags.UsePickSet |
                       CommandFlags.Redraw | CommandFlags.Modal)] // название команды, вызываемой в Autocad
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


        // Fun Для получения PointLine 
        [CommandMethod("фв3", CommandFlags.UsePickSet |
                       CommandFlags.Redraw | CommandFlags.Modal)] // название команды, вызываемой в Autocad
        public void getPointLine()
        {
            using (Transaction trAdding = dbCurrent.TransactionManager.StartTransaction())
            {
                PromptEntityOptions magistral = new PromptEntityOptions("\n Выберите объект для получения Информации Линии : ");
                PromptEntityResult perMagistral = ed.GetEntity(magistral);
                if (perMagistral.Status != PromptStatus.OK) { return; }
                Polyline Plyline = trAdding.GetObject(perMagistral.ObjectId, OpenMode.ForRead) as Polyline;

                foreach (PowerLine itemLine in listPowerLine)
                {

                    if (itemLine.IDLine == perMagistral.ObjectId)
                    {
                        ed.WriteMessage("\n  ");
                        ed.WriteMessage("~~~~~~~~~~~~~~~~");
                        ed.WriteMessage("\n У выбранного объекта ID:  " + itemLine.IDLine);
                        ed.WriteMessage("\n У выбранного объекта имя:  " + itemLine.name);
                        ed.WriteMessage("\n У выбранного объекта марка провода:  " + itemLine.cable);
                        ed.WriteMessage("\n У выбранного объекта допустимый ток :  " + itemLine.Icrict);
                        ed.WriteMessage("\n У выбранного объекта длинна:  " + itemLine.lengthLine);
                        ed.WriteMessage("\n У выбранного объекта родитель:  " + itemLine.parent.name);
                        ed.WriteMessage("\n Отпайка от вершины родителя:  " + itemLine.parentPoint.name);
                        ed.WriteMessage("\n Последняя вершина объекта:  " + itemLine.endPoint.name);
                        ed.WriteMessage("\n У выбранного объекта детей: " + itemLine.children.Count + " шт.");
                        ed.WriteMessage("Выбранный объект основан на вершинах:  ");

                        StringBuilder text = new StringBuilder();
                        foreach (PointLine itemPoint in itemLine.points)
                        {
                            text.Append(itemPoint.name + " ");
                        }
                        ed.WriteMessage(text.ToString());

                        ed.WriteMessage("Выбранный объект лежит в ребрах:  ");

                        StringBuilder text2 = new StringBuilder();
                        foreach (Edge itemEdge in itemLine.Edges)
                        {
                            text2.Append(itemEdge.name + " ");
                        }
                        ed.WriteMessage(text2.ToString());

                        ed.WriteMessage("~~~~~~~~~~~~~~~~");
                        ed.WriteMessage("\n  ");
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
            using (Transaction trAdding = dbCurrent.TransactionManager.StartTransaction())
            {
                ed.WriteMessage("\n  ");
                ed.WriteMessage("~~~~~~~~~~~~~~~~");
                ed.WriteMessage("СПИСОК ВСЕХ ВЕРШИН: ");

                foreach (PointLine itemPoint in listPoint)
                {

                    ed.WriteMessage(itemPoint.name.ToString() + " " + "weight: " + itemPoint.weight + " " + itemPoint.positionPoint + " I: " + itemPoint.I + " " + itemPoint.isFavorite);
                }
                ed.WriteMessage("~~~~~~~~~~~~~~~~");
                ed.WriteMessage("\n  ");

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
            using (Transaction trAdding = dbCurrent.TransactionManager.StartTransaction())
            {
                ed.WriteMessage("\n  ");
                ed.WriteMessage("~~~~~~~~~~~~~~~~");
                ed.WriteMessage("СПИСОК ВСЕХ СУЩЕСТВУЮЩИХ ЛИНИЙ: ");

                foreach (PowerLine itemLine in listPowerLine)
                {
                    ed.WriteMessage(itemLine.name);
                }

                ed.WriteMessage("~~~~~~~~~~~~~~~~");
                ed.WriteMessage("\n  ");

            }
        }

        // Fun Для получения всех ребер 
        [CommandMethod("фв6", CommandFlags.UsePickSet |
                       CommandFlags.Redraw | CommandFlags.Modal)] // название команды, вызываемой в Autocad
        public void getAllEdeg()
        {
            using (Transaction trAdding = dbCurrent.TransactionManager.StartTransaction())
            {
                ed.WriteMessage("\n  ");
                ed.WriteMessage("~~~~~~~~~~~~~~~~");
                ed.WriteMessage("СПИСОК ВСЕХ РЕБЕР: ");

                foreach (Edge itemEdge in listEdge)
                {
                    ed.WriteMessage(itemEdge.name.ToString() + " | Длина ребра: " + itemEdge.length + " | Марка провода ребра: " + itemEdge.cable + " | Допустимый ток: " + itemEdge.Icrict + " | Протекаемый ток: " + itemEdge.I + " | Data: " + itemEdge.r + " | StartPoint: " + itemEdge.startPoint.name + " |EndPoint: " + itemEdge.endPoint.name);

                    // ed.WriteMessage(itemEdge.name.ToString() + " | Длина ребра: " + itemEdge.length + " | Марка провода: " + itemEdge.cable + " | Data: " + itemEdge.data + " | " + itemEdge.centerPoint.name.ToString() + ": " + itemEdge.centerPoint.positionPoint);
                }
                ed.WriteMessage("~~~~~~~~~~~~~~~~");
                ed.WriteMessage("\n  ");

            }
            OnPropertyChanged(nameof(listEdge));
        }

        // Fun Для получения информации по одному выбранному узлу 
        [CommandMethod("фв7", CommandFlags.UsePickSet |
                       CommandFlags.Redraw | CommandFlags.Modal)] // название команды, вызываемой в Autocad
        public void getInfoPoint()
        {
            using (Transaction trAdding = dbCurrent.TransactionManager.StartTransaction())
            {
                PromptEntityOptions InfoPoint = new PromptEntityOptions("\nВыберите узел для получения информации : ");
                PromptEntityResult perInfoPoint = ed.GetEntity(InfoPoint);
                MText mtext = trAdding.GetObject(perInfoPoint.ObjectId, OpenMode.ForRead) as MText;

                int index = (mtext.Contents).IndexOf('P');

                ed.WriteMessage("\n  ");
                ed.WriteMessage("~~~~~~~~~~~~~~~~");
                ed.WriteMessage("ИНФОРМАЦИЯ ПО УЗЛУ №: ");

                foreach (PointLine itemPoint in listPoint)
                {
                    //Для осечения веса узла
                    if (index > 1)
                    {

                        if (int.Parse((mtext.Contents).Substring(0, index - 1)) == itemPoint.name)
                        {
                            ed.WriteMessage(int.Parse((mtext.Contents).Substring(0, index - 1)) + " | Вес вершины: " + itemPoint.weight + " | " + itemPoint.positionPoint);
                        }
                    }
                    else
                    {
                        if (int.Parse(mtext.Contents) == itemPoint.name)
                        {
                            ed.WriteMessage(itemPoint.name.ToString() + " | Вес вершины: " + itemPoint.weight + " | " + itemPoint.positionPoint);
                        }
                    }



                }

                ed.WriteMessage("~~~~~~~~~~~~~~~~");
                ed.WriteMessage("\n  ");

            }
        }


        // Fun Для получения информации по одному выбранному ребру 
        [CommandMethod("фв8", CommandFlags.UsePickSet |
                       CommandFlags.Redraw | CommandFlags.Modal)] // название команды, вызываемой в Autocad
        public void getInfoEdge()
        {
            using (Transaction trAdding = dbCurrent.TransactionManager.StartTransaction())
            {
                PromptEntityOptions InfoPoint = new PromptEntityOptions("\nВыберите ребро для получения информации : ");
                PromptEntityResult perInfoPoint = ed.GetEntity(InfoPoint);
                MText mtext = trAdding.GetObject(perInfoPoint.ObjectId, OpenMode.ForRead) as MText;

                ed.WriteMessage("\n  ");
                ed.WriteMessage("~~~~~~~~~~~~~~~~");
                ed.WriteMessage("ИНФОРМАЦИЯ ПО РЕБРУ №: " + mtext.Contents);

                foreach (Edge itemEdge in listEdge)
                {

                    if (mtext.Contents == itemEdge.name.ToString())
                    {
                        ed.WriteMessage(itemEdge.name.ToString() + " | Длина ребра: " + itemEdge.length + " | Марка провода ребра: " + itemEdge.cable + " | Допустимый ток: " + itemEdge.Icrict + " | Протекаемый ток: " + itemEdge.I + " | Data: " + itemEdge.r + " | StartPoint: " + itemEdge.startPoint.name + " |EndPoint: " + itemEdge.endPoint.name);
                    }


                }

                ed.WriteMessage("~~~~~~~~~~~~~~~~");
                ed.WriteMessage("\n  ");

            }

        }

        /*
        // Fun Для добавления весов в Point 
        [CommandMethod("фв9", CommandFlags.UsePickSet |
                       CommandFlags.Redraw | CommandFlags.Modal)] // название команды, вызываемой в Autocad
        */
        public void setWeightPoint()
        {
            ed.WriteMessage("\n--------------------------------\n");
            ed.WriteMessage("Запись веса вершин. \n");
            ed.WriteMessage("--------------------------------\n");
            using (Transaction trAdding = dbCurrent.TransactionManager.StartTransaction())
            {

                setDataPoint("Узлы_Saidi_Saifi_Makarov.D", listPoint, dbCurrent, ed, trAdding);


            }
            ed.WriteMessage("Веса вершин успешно внесены. \n");
            OnPropertyChanged(nameof(listPoint));
            OnPropertyChanged(nameof(listEdge));
            OnPropertyChanged(nameof(listPowerLine));
        }


        // Fun Для Создания Пути обхода 
        [CommandMethod("фв10", CommandFlags.UsePickSet |
                       CommandFlags.Redraw | CommandFlags.Modal)] // название команды, вызываемой в Autocad
        public void creatPathPoint()
        {
            int startPoint = 0;
            int endPoint = 0;

            PromptResult result = ed.GetString("С какой вершине начать обходить ?: ");
            if (result.Status == PromptStatus.OK)
            {
                startPoint = int.Parse(result.StringResult) - 1; //+1 что бы билось с визуализацией 
            }

            //Ввод Конечной вершины
            PromptResult result2 = ed.GetString("Конечная вершина обхода ?:");
            if (result2.Status == PromptStatus.OK)
            {
                endPoint = int.Parse(result2.StringResult) - 1;//+1 что бы билось с визуализацией 
            }

            //Создает путь из классов !!
            List<PointLine> path = ListPathIntToPoint(findPath(matrixSmej, startPoint, endPoint));

            ed.WriteMessage("~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            ed.WriteMessage("Путь ОБХОДА :");

            StringBuilder resultPath = new StringBuilder();
            foreach (PointLine item in path)
            {
                resultPath.Append(item.name + " ");
            }
            ed.WriteMessage(resultPath.ToString());
            ed.WriteMessage("~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

        }

        /*
        // Fun Для Создания Пути обхода (выбор автоматического выключателя) КЗ
        [CommandMethod("фв11", CommandFlags.UsePickSet |
                       CommandFlags.Redraw | CommandFlags.Modal)] // название команды, вызываемой в Autocad
        */
        public void getPathKZ(bool isI1Tkz = true)
        {

            //Получает данные TKZ
            TKZ tkz = сreatTKZ(isI1Tkz);

            //Добавляю свой ввод

            string strResistancetTransformers = creatPromptKeywordOptions("Выберите мощность тр-р с группой соед.: ", searchAllDataInBD(dbFilePath, "transformer", "name"), 2);
            if (string.IsNullOrEmpty(strResistancetTransformers)) { return; }

            //берем сопротивление в BD по тексту
            tkz.transformersR = searchDataInBD(dbFilePath, "transformer", strResistancetTransformers, "name", "r");
            tkz.transformersX = searchDataInBD(dbFilePath, "transformer", strResistancetTransformers, "name", "x");
            tkz.transformersR0 = searchDataInBD(dbFilePath, "transformer", strResistancetTransformers, "name", "r0");
            tkz.transformersX0 = searchDataInBD(dbFilePath, "transformer", strResistancetTransformers, "name", "x0");

            //Фазное напряжение сети
            double Uline;
            double transformersPetliya;
            double rdop = 0;
            double lineZ = 0;

            if (!isI1Tkz)
            {
                List<string> tempList = searchAllDataInBD(dbFilePath, "voltage", "kV");
                tempList.Insert(0, "Свое");

                string resultPromt = creatPromptKeywordOptions("Выберите ЛИНЕЙНОЕ  напряжение сети.: ", tempList, 2);

                switch (resultPromt)
                {
                    case null:
                        ed.WriteMessage("Отмена");
                        return;
                    case "Свое":


                        PromptDoubleOptions options = new PromptDoubleOptions("Введите свою ЛИНЕЙНУЮ ЭДС:");
                        PromptDoubleResult result = ed.GetDouble(options);
                        if (result.Status == PromptStatus.OK)
                        {
                            ed.WriteMessage("\n\nВы ввели: " + result.Value.ToString());
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
            creatPL(tkz.pathPointTKZ, "TKZ_Makarov.D", 256, 2);
            ed.WriteMessage("Линия протекания ТКЗ построена!");

            ed.WriteMessage("~~~~~~~~~~~~~~~~~~~~~~");
            ed.WriteMessage("| Самая нечувствительная точка КЗ: " + tkz.pointTKZ.name + " ");
            ed.WriteMessage("| Путь ТКЗ: " + text + " ");
            ed.WriteMessage("| Длинна ТКЗ: " + tkz.length + " м.");
            ed.WriteMessage("| Линейное напряжение сети: " + Uline + " В.");
            if (!isI1Tkz)
            {
                ed.WriteMessage("| Сопротивление тр-р: " + tkz.transformersR + " +j " + tkz.transformersX + " (Z= " + Math.Sqrt(Math.Pow((tkz.transformersR), 2) + Math.Pow((tkz.transformersX), 2)) + ")" + " Ом.");
                ed.WriteMessage("| Итоговое сопротивление линии: " + tkz.lineR + " +j " + tkz.lineX + " (Z= " + Math.Sqrt(Math.Pow((tkz.lineR), 2) + Math.Pow((tkz.lineX), 2)) + ")" + " Ом.");
                ed.WriteMessage("| Ток КЗ в конце: " + tkz.resultTKZ + " А.");
                //ed.WriteMessage("| Рекомендуемый автоматический выключатель не более : " + Math.Round(tkz.resultTKZ / 3) + " А.");
                ed.WriteMessage("| ------------------------------------------------------------------------------------------------");
                ed.WriteMessage("Расчет выполнен согласно ГОСТ 28249-93");
            }
            else
            {
                ed.WriteMessage("| Добавил дополнительно: R1= " + rdop + " Ом. " + "Суммарное переходное сопротивление рубильников, автоматов, болтовых соединений и электрической дуги.");
                ed.WriteMessage("| Сопротивление тр-р: " + "R1+jX1=R2+jX2: " + tkz.transformersR + " +j " + tkz.transformersX + " | " + "R0+jX0: " + tkz.transformersR0 + " +j " + tkz.transformersX0 + " (Zпетля= " + transformersPetliya + ")" + " Ом.");
                ed.WriteMessage("| Итоговое сопротивление линии: " + "R1+jX1=R2+jX2: " + tkz.lineR + " +j " + tkz.lineX + " | " + "R0+jX0: " + tkz.lineR0 + " +j " + tkz.lineX0 + " (Zпетля= " + tkz.linePetlia + ")" + " Ом.");
                ed.WriteMessage("| Ток КЗ в конце: " + tkz.resultTKZ + " А.");
                ed.WriteMessage("| Рекомендуемый автоматический выключатель не более : " + Math.Round(tkz.resultTKZ / 3) + " А.");
                ed.WriteMessage("| ------------------------------------------------------------------------------------------------");
                ed.WriteMessage("Расчет выполнен согласно Рекомендации по расчету сопротивления петли \"фаза-нуль\". - М.: Центральное бюро научно-технической информации, 1986.");
            }

            ed.WriteMessage("~~~~~~~~~~~~~~~~~~~~~~");




        }

        /*
        [CommandMethod("фв12", CommandFlags.UsePickSet |
                       CommandFlags.Redraw | CommandFlags.Modal)] // название команды, вызываемой в Autocad */
        public void getMyPathKZ(bool isI1Tkz = true)
        {
            List<string> tempListPoint = new List<string>();

            foreach (PointLine element in listPoint)
            {
                tempListPoint.Add(element.name.ToString());
            }

            //Добавка в начало списка
            tempListPoint.Insert(0, "Выбрать самостоятельно узел");

            string pointKZ = creatPromptKeywordOptions("Введите номер узла КЗ:", tempListPoint, 1);
            if (string.IsNullOrEmpty(pointKZ) ) { return; };


            if (pointKZ == "Выбрать_самостоятельно_узел" | pointKZ == "самостоятельно узел" | pointKZ == "Выбрать самостоятельно узел")
            {
                PromptEntityOptions tempPointKZ = new PromptEntityOptions("\nВыберите узел для КЗ: ");
                PromptEntityResult pertempPointKZ = ed.GetEntity(tempPointKZ);

                using (DocumentLock docloc = doc.LockDocument())
                {
                    using (Transaction trAdding = dbCurrent.TransactionManager.StartTransaction())
                    {
                        MText mtext = trAdding.GetObject(pertempPointKZ.ObjectId, OpenMode.ForRead) as MText;
                        pointKZ = mtext.Contents;
                        trAdding.Commit();
                    }
                }
            }



            //Получает данные TKZ
            TKZ tkz = сreatMyTKZ(int.Parse(pointKZ));
            string strResistancetTransformers = creatPromptKeywordOptions("Выберите мощность тр-р с группой соед.: ", searchAllDataInBD(dbFilePath, "transformer", "name"), 1);
            //берем сопротивление в BD по тексту
            tkz.transformersR = searchDataInBD(dbFilePath, "transformer", strResistancetTransformers, "name", "r");
            tkz.transformersX = searchDataInBD(dbFilePath, "transformer", strResistancetTransformers, "name", "x");
            tkz.transformersR0 = searchDataInBD(dbFilePath, "transformer", strResistancetTransformers, "name", "r0");
            tkz.transformersX0 = searchDataInBD(dbFilePath, "transformer", strResistancetTransformers, "name", "x0");

            //Фазное напряжение сети
            double Uline;
            double transformersPetliya;
            double rdop = 0;
            double lineZ = 0;

            if (!isI1Tkz)
            {
                List<string> tempList = searchAllDataInBD(dbFilePath, "voltage", "kV");
                tempList.Insert(0, "Свое");

                string resultPromt = creatPromptKeywordOptions("Выберите ЛИНЕЙНОЕ  напряжение сети.: ", tempList, 2);

                switch (resultPromt)
                {
                    case null:
                        ed.WriteMessage("Отмена");
                        return;
                    case "Свое":


                        PromptDoubleOptions options = new PromptDoubleOptions("Введите свою ЛИНЕЙНУЮ ЭДС:");
                        PromptDoubleResult result = ed.GetDouble(options);
                        if (result.Status == PromptStatus.OK)
                        {
                            ed.WriteMessage("\n\nВы ввели: " + result.Value.ToString());
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
            creatPL(tkz.pathPointTKZ, "TKZ_Makarov.D", 256, 2);
            ed.WriteMessage("Линия протекания ТКЗ построена!");

            ed.WriteMessage("~~~~~~~~~~~~~~~~~~~~~~");
            ed.WriteMessage("| Самая нечувствительная точка КЗ: " + tkz.pointTKZ.name + " ");
            ed.WriteMessage("| Путь ТКЗ: " + text + " ");
            ed.WriteMessage("| Длинна ТКЗ: " + tkz.length + " м.");
            ed.WriteMessage("| Линейное напряжение сети: " + Uline + " В.");
            if (!isI1Tkz)
            {
                ed.WriteMessage("| Сопротивление тр-р: " + tkz.transformersR + " +j " + tkz.transformersX + " (Z= " + Math.Sqrt(Math.Pow((tkz.transformersR), 2) + Math.Pow((tkz.transformersX), 2)) + ")" + " Ом.");
                ed.WriteMessage("| Итоговое сопротивление линии: " + tkz.lineR + " +j " + tkz.lineX + " (Z= " + Math.Sqrt(Math.Pow((tkz.lineR), 2) + Math.Pow((tkz.lineX), 2)) + ")" + " Ом.");
                ed.WriteMessage("| Ток КЗ в конце: " + tkz.resultTKZ + " А.");
                //ed.WriteMessage("| Рекомендуемый автоматический выключатель не более : " + Math.Round(tkz.resultTKZ / 3) + " А.");
                ed.WriteMessage("| ------------------------------------------------------------------------------------------------");
                ed.WriteMessage("Расчет выполнен согласно ГОСТ 28249-93");
            }
            else
            {
                ed.WriteMessage("| Добавил дополнительно: Zконт.= " + rdop + " Ом. " + "Суммарное переходное сопротивление рубильников, автоматов, болтовых соединений и электрической дуги.");
                ed.WriteMessage("| Сопротивление тр-р: " + "R1+jX1=R2+jX2: " + tkz.transformersR + " +j " + tkz.transformersX + " | " + "R0+jX0: " + tkz.transformersR0 + " +j " + tkz.transformersX0 + " (Zпетля= " + transformersPetliya + ")" + " Ом.");
                ed.WriteMessage("| Итоговое сопротивление линии: " + "R1+jX1=R2+jX2: " + tkz.lineR + " +j " + tkz.lineX + " | " + "R0+jX0: " + tkz.lineR0 + " +j " + tkz.lineX0 + " (Zпетля= " + tkz.linePetlia + ")" + " Ом.");
                ed.WriteMessage("| Ток КЗ в конце: " + tkz.resultTKZ + " А.");
                ed.WriteMessage("| Рекомендуемый автоматический выключатель не более : " + Math.Round(tkz.resultTKZ / UserData.coefficientMultiplicity) + " А.");
                ed.WriteMessage("| ------------------------------------------------------------------------------------------------");
                ed.WriteMessage("Расчет выполнен согласно Рекомендации по расчету сопротивления петли \"фаза-нуль\". - М.: Центральное бюро научно-технической информации, 1986.");
            }

            ed.WriteMessage("~~~~~~~~~~~~~~~~~~~~~~");

        }


        /*
        //Для проверки АВ, до куда чувсвителен, по самой удаленной точки
        [CommandMethod("фв13", CommandFlags.UsePickSet |
                   CommandFlags.Redraw | CommandFlags.Modal)] // название команды, вызываемой в Autocad */
        public void getMyPathKZFromAV()
        {
            int nomivalAV = 0;
            PromptIntegerOptions options = new PromptIntegerOptions("Введите номинал автоматического выключателя, для проверки зоны чувствительности: ");
            options.AllowNegative = false;
            options.AllowZero = false;

            PromptIntegerResult result = ed.GetInteger(options);
            if (result.Status == PromptStatus.OK)
            {
                nomivalAV = result.Value;
                ed.WriteMessage("Вы ввели: " + result.Value.ToString());
            }

            //Получает данные TKZ
            TKZ tkz = сreatTKZ(true);
            string strResistancetTransformers = creatPromptKeywordOptions("Выберите мощность тр-р с группой соед.: ", searchAllDataInBD(dbFilePath, "transformer", "name"), 1);

            //берем сопротивление в BD по тексту
            tkz.transformersR = searchDataInBD(dbFilePath, "transformer", strResistancetTransformers, "name", "r");
            tkz.transformersX = searchDataInBD(dbFilePath, "transformer", strResistancetTransformers, "name", "x");
            tkz.transformersR0 = searchDataInBD(dbFilePath, "transformer", strResistancetTransformers, "name", "r0");
            tkz.transformersX0 = searchDataInBD(dbFilePath, "transformer", strResistancetTransformers, "name", "x0");

            //Фазное напряжение сети
            // double Uf = double.Parse(creatPromptKeywordOptions("Выберите Фазное напряжение сети.: ", searchAllDataInBD(dbFilePath, "voltage", "kV"), 1));
            double rdop = 0.015;
            double Uline = 400;
            double transformersPetliya = Math.Sqrt(Math.Pow((tkz.transformersR * 2 + tkz.transformersR0), 2) + Math.Pow((tkz.transformersX * 2 + tkz.transformersX0), 2));
            tkz.resultTKZ = Uline / ((transformersPetliya / 3 + tkz.linePetlia + rdop) * Math.Sqrt(3));



            // /3-это почти эквивалент 5сек
            if (nomivalAV <= tkz.resultTKZ / UserData.coefficientMultiplicity)
            {
                ed.WriteMessage("Ваш автоматический выключатель на " + nomivalAV + " А, защищает всю линую до точки максимальной нечувствительности.");
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
                creatPL(resultTkzDist.pathPointTKZ, "TKZ_Makarov.D", 256, 2);
                ed.WriteMessage("Линия протекания ТКЗ построена!");

                ed.WriteMessage("~~~~~~~~~~~~~~~~~~~~~~");
                ed.WriteMessage("| Самая нечувствительная точка КЗ: " + resultTkzDist.pointTKZ.name + " ");
                ed.WriteMessage("| Путь ТКЗ: " + text + " ");
                ed.WriteMessage("| Длинна ТКЗ: " + resultTkzDist.length + " м.");
                ed.WriteMessage("| Линейное напряжение сети: " + Uline + " В.");
                ed.WriteMessage("| Добавил дополнительно: Z= " + rdop + " Ом. " + "Суммарное переходное сопротивление рубильников, автоматов, болтовых соединений и электрической дуги.");
                ed.WriteMessage("| Сопротивление тр-р: " + "R1+jX1=R2+jX2: " + tkz.transformersR + " +j " + tkz.transformersX + " | " + "R0+jX0: " + tkz.transformersR0 + " +j " + tkz.transformersX0 + " (Zпетля= " + transformersPetliya + ")" + " Ом.");
                ed.WriteMessage("| Итоговое сопротивление линии: " + "R1+jX1=R2+jX2: " + resultTkzDist.lineR + " +j " + resultTkzDist.lineX + " | " + "R0+jX0: " + resultTkzDist.lineR0 + " +j " + resultTkzDist.lineX0 + " (Zпетля= " + resultTkzDist.linePetlia + ")" + " Ом.");
                ed.WriteMessage("| Ток КЗ в конце: " + resultTkzDist.resultTKZ + " А.");
                ed.WriteMessage("| Рекомендуемый автоматический выключатель не более : " + Math.Round(resultTkzDist.resultTKZ / UserData.coefficientMultiplicity) + " А.");
                ed.WriteMessage("| ------------------------------------------------------------------------------------------------");
                ed.WriteMessage("Расчет выполнен согласно Рекомендации по расчету сопротивления петли \"фаза-нуль\". - М.: Центральное бюро научно-технической информации, 1986.");



            }



        }

        /*
        //Поиск места установки рекоузера в магистрали. 
        [CommandMethod("фв14", CommandFlags.UsePickSet |
                   CommandFlags.Redraw | CommandFlags.Modal)] // название команды, вызываемой в Autocad */
        public void getLocalREC()
        {
            List<PointLine> masterPointLine = listPowerLine[0].points;
            List<PointLine> saveListMagistral = new List<PointLine>(listPowerLine[0].points);
            List<PointLine> listWhithWeight = new List<PointLine>();

            //Создает список вершин с весом
            foreach (PointLine itemPoint in listPoint)
            {
                if (itemPoint.weight > 0)
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
                            perentPointMagistral.weight = perentPointMagistral.weight + itemPoint.weight;
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
                    tempLeftDifference = tempLeftDifference + itemPointLine.weight;
                }

                StringBuilder tempRightPathText = new StringBuilder();

                foreach (PointLine itemPointLine in tempRightPath)
                {
                    //Для удаления в правом пути точки из левого пути ( ниже совместный код)
                    if (tempRightPath.IndexOf(itemPointLine) == 0)
                    {
                        continue;
                    }
                    tempRighDifference = tempRighDifference + itemPointLine.weight;
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
                        tempLeftPathText.Append(itemPointLine.name + " (" + itemPointLine.weight + ")" + " ");
                    }


                    foreach (PointLine itemPointLine in REC.rightPath)
                    {

                        tempRightPathText.Append(itemPointLine.name + " (" + itemPointLine.weight + ")" + " ");
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
            //Построить окр
            ZoomToEntity(DrawCircle(edgeREC.centerPoint, "Граф_Saidi_Saifi_Makarov.D"), 4);

            ed.WriteMessage("----------");
            ed.WriteMessage("Рекомендуемое место установки REC в ребро №: " + edgeREC.name);
            ed.WriteMessage("Рекомендуемое место установки REC между вершинами № " + edgeREC.startPoint.name + " И " + edgeREC.endPoint.name);
            ed.WriteMessage("Вес левой части: " + REC.LeftWeight + " | " + REC.leftPathText);
            ed.WriteMessage("Вес правой части: " + REC.RighWeight + " | " + REC.righPathText);
            ed.WriteMessage("----------");




            //  masterPointLine = new List<PointLine>(saveListMagistral);

            //Скинуть веса вершин у магистрали
            foreach (var item in masterPointLine)
            {
                item.weight = 0;
                item.isFavorite = false;
            }


        }

        /*
        //Поиск Падения напряжения. 
        [CommandMethod("фв15", CommandFlags.UsePickSet |
                   CommandFlags.Redraw | CommandFlags.Modal)] // название команды, вызываемой в Autocad */
        public void getVoltage()
        {
            //Фазное напряжение сети
            string tempUf = creatPromptKeywordOptions("Выберите напряжение точки генерации сети.: ", searchAllDataInBD(dbFilePath, "voltage", "kV"), 1);
            if (string.IsNullOrEmpty(tempUf)) { return; };
            double Uf = double.Parse(tempUf);
            listPoint[0].tempData = Uf;

            using (Transaction trAdding = dbCurrent.TransactionManager.StartTransaction())
            {
                deleteObjectsOnLayer("Напряжение_Makarov.D", dbCurrent);
                trAdding.Commit();
            }

            foreach (var item in listPoint)
            {
                item.I = 0;
                item.tempBoll = false;
            }


            //Резервный список 
            List<PointLine> stateList = new List<PointLine>(listPoint);
            List<PointLine> listWithWeight = new List<PointLine>();
            List<List<PointLine>> tempAllPath = new List<List<PointLine>>();
            List<List<PointLine>> resultAllPath = new List<List<PointLine>>();
            // Создает список вершин с весом
            listWithWeight = GetWeightedVertices(listPoint);


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
                    double tempAddI = 0;
                    if (itemListPoint[i + 1].tempBoll != true)
                    {
                        itemListPoint[i + 1].tempBoll = true;
                        itemListPoint[i + 1].I = Math.Round(itemListPoint[i + 1].I + itemListPoint[i].I, 3);
                    }
                    else
                    {
                        tempAddI = itemListPoint[0].I;
                        itemListPoint[i + 1].I += Math.Round(tempAddI, 2);
                    }
                }


                /*
                StringBuilder text = new StringBuilder();
                foreach (var item1 in itemListPoint)
                {
                    text.Append(item1.name.ToString() + " ");

                }
                ed.WriteMessage(text.ToString());
                */


                //Построить куда бежит ток
                creatPL(itemListPoint, "Напряжение_Makarov.D", 52, 0.6);
            }


            //Ток добавляем в ребра
            foreach (PointLine itemPoint in listPoint)
            {

                if (itemPoint.I > 0)
                {
                    foreach (Edge itemEdge in listEdge)
                    {
                        if (itemPoint == itemEdge.endPoint)
                        {
                            itemEdge.I = itemPoint.I;
                        }
                        //Проверка на критический ток
                        if (itemEdge.I > itemEdge.Icrict)
                        {
                            creatText("Напряжение_Makarov.D", itemEdge.centerPoint, "I>Iкрит;" + itemEdge.I + " A.", "1", 220, 4);
                        }
                    }
                }
            }

            //Анализирует падения напряжения и отрисовывает
            foreach (Edge itemEdge in listEdge)
            {
                if (itemEdge.startPoint.I > 0 && itemEdge.endPoint.I > 0)
                {
                    creatText("Напряжение_Makarov.D", itemEdge.centerPoint, " ΔU= " + Math.Round((itemEdge.I * (Math.Sqrt(Math.Pow(itemEdge.r, 2) + Math.Pow(itemEdge.x, 2))) * itemEdge.length), 2), "1", 154, -4);

                    if (Math.Round(itemEdge.startPoint.tempData - (itemEdge.I * (Math.Sqrt(Math.Pow(itemEdge.r, 2) + Math.Pow(itemEdge.x, 2))) * itemEdge.length), 2) > 0)
                    {
                        itemEdge.endPoint.tempData = Math.Round(itemEdge.startPoint.tempData - (itemEdge.I * (Math.Sqrt(Math.Pow(itemEdge.r, 2) + Math.Pow(itemEdge.x, 2))) * itemEdge.length), 2);
                    }
                    else
                    {
                        itemEdge.endPoint.tempData = 0;

                    }

                    if (((Uf - Math.Round(itemEdge.startPoint.tempData - (itemEdge.I * (Math.Sqrt(Math.Pow(itemEdge.r, 2) + Math.Pow(itemEdge.x, 2))) * itemEdge.length), 2)) / Uf * 100) >= 10.0)
                    {
                        creatText("Напряжение_Makarov.D", itemEdge.endPoint, "U= " + itemEdge.endPoint.tempData.ToString(), "1", 15, -4);
                    }
                    else
                    {
                        creatText("Напряжение_Makarov.D", itemEdge.endPoint, "U= " + itemEdge.endPoint.tempData.ToString(), "1", 112, -4);
                    }



                }

            }

            listPoint = new List<PointLine>(stateList);
            OnPropertyChanged(nameof(listPoint));
            OnPropertyChanged(nameof(listEdge));
            OnPropertyChanged(nameof(listPowerLine));
        }


        //Получить словари. 
        [CommandMethod("фв16", CommandFlags.UsePickSet |
                   CommandFlags.Redraw | CommandFlags.Modal)] // название команды, вызываемой в Autocad
        public void getSlovari()

        {
            PromptEntityOptions item = new PromptEntityOptions("\nВыберите объект: ");
            PromptEntityResult perItem = ed.GetEntity(item);
            //ShowExtensionDictionaryContents(perItem.ObjectId, "ESMT_LEP_v1.0");
            ShowExtensionDictionaryContents(perItem.ObjectId, "Makarov.D");
            ShowExtensionDictionaryContents(perItem.ObjectId, "ESMT_LEP_v1.0");



        }
        /*
        [CommandMethod("фв17", CommandFlags.UsePickSet |
                 CommandFlags.Redraw | CommandFlags.Modal)] // название команды, вызываемой в Autocad */
        public void setBD()
        {
            try
            {


                ed.WriteMessage("\n--------------------------------\n");
                ed.WriteMessage("Сохранение веса вершин \n");
                ed.WriteMessage("--------------------------------\n");

                BD bd = new BD();
                //bd.listEdge = listEdge;
                //bd.serializableMatrix = new SerializableMatrix(matrixSmej);
                //bd.matrixInc=matrixInc;
                bd.listPowerLine = listPowerLine;
                //Для восстановления веса Вершин
                bd.listPointLine = listPoint;

                PromptEntityOptions item = new PromptEntityOptions("\nВыберите объект куда сохранить веса вершин: ");
                PromptEntityResult perItem = ed.GetEntity(item);
                if (perItem.Status != PromptStatus.OK) { return; }



                string xmlData = SerializeToXml(bd);
                SaveXmlToXrecord(xmlData, perItem.ObjectId, "Makarov.D");
                ed.WriteMessage("\nСписок успешно сохранен в Xrecord.\n");
            }
            catch (Exception ex)
            {
                ed.WriteMessage(ex.ToString());
                ed.WriteMessage("Что-то пошло не так....");
            }
        }


        /*
        //Загружает и обновляет вес вершины
        [CommandMethod("фв18", CommandFlags.UsePickSet |
               CommandFlags.Redraw | CommandFlags.Modal)] // название команды, вызываемой в Autocad  */
        public void getBD()

        {

            try
            {


                ed.WriteMessage("\n--------------------------------\n");
                ed.WriteMessage("Восстановление веса вершин \n");
                ed.WriteMessage("--------------------------------\n");
                PromptEntityOptions item = new PromptEntityOptions("\nВыберите объект откуда взять БД: ");
                PromptEntityResult perItem = ed.GetEntity(item);

                if (perItem.Status != PromptStatus.OK) { return; }

                string isLoadNameLine = creatPromptKeywordOptions("Восстановить название линий ?", new List<string> { "Да", "Нет" }, 1);

                //listPowerLine= BDShowExtensionDictionaryContents(perItem.ObjectId, "Makarov.D").listPowerLine;
                List<PointLine> templistPoint = BDShowExtensionDictionaryContents<BD>(perItem.ObjectId, "Makarov.D").listPointLine;
                List<PowerLine> templistPowerLine = BDShowExtensionDictionaryContents<BD>(perItem.ObjectId, "Makarov.D").listPowerLine;

                foreach (PointLine itemPoint in templistPoint)
                {
                    if (itemPoint.weight > 0)
                    {
                        foreach (PointLine item1 in listPoint)
                        {

                            if (itemPoint.name == item1.name)
                            {
                                item1.weight = itemPoint.weight;
                                item1.cos = itemPoint.cos;
                                item1.typeClient = itemPoint.typeClient;
                                TextFun.updateTextById(item1.IDText, item1.name + "\\P" + item1.weight, 66);
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

                ed.WriteMessage("\nПолучили данные из БД.\n");
                OnPropertyChanged(nameof(listPoint));
            }
            catch (Exception ex)
            {
                ed.WriteMessage(ex.ToString());
                ed.WriteMessage("Что-то пошло не так....");
            }

        }

        [CommandMethod("Elt", CommandFlags.UsePickSet |
              CommandFlags.Redraw | CommandFlags.Modal)]
        public async void  creatFormAsync()
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
                if (itemPoint.weight > 0)
                {

                    if (itemPoint.typeClient == 1)
                    {
                        itemPoint.I = Math.Round(itemPoint.weight / (0.22 * itemPoint.cos), 2);
                    }
                    if (itemPoint.typeClient == 3)
                    {
                        itemPoint.I = Math.Round(itemPoint.weight / (Math.Sqrt(3) * 0.38 * itemPoint.cos), 2);
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


        PowerLine CrearMagistral(Editor ed, Transaction trAdding, List<PointLine> listPoint, List<Point2d> listPointXY, List<PowerLine> listPowerLine)
        {
            PromptEntityOptions magistral = new PromptEntityOptions("\n\nВыберите магистраль: \n\n");
            PromptEntityResult perMagistral = ed.GetEntity(magistral);
            Polyline Plyline = trAdding.GetObject(perMagistral.ObjectId, OpenMode.ForRead) as Polyline;

            ZoomToEntity(perMagistral.ObjectId, 4);

            ed.WriteMessage("\n\nВыберите марку провода магистрали: \n\n");

            PowerLine considerPowerLine = new PowerLine();
            int defult = searchAllDataInBD(dbFilePath, "cable", "default").IndexOf("true") + 1;


            if (ShowExtensionDictionaryContents(perMagistral.ObjectId, "ESMT_LEP_v1.0")["isItem"] == "true")
            {
                considerPowerLine.cable = ShowExtensionDictionaryContents(perMagistral.ObjectId, "ESMT_LEP_v1.0")["nameCable"];
                ed.WriteMessage("\n\nНАЙДЕНЫ ЗНАЧЕНИЯ ИЗ SMARTLINE");
            }
            else
            {
                considerPowerLine.cable = creatPromptKeywordOptions("\n\nВыберите мару провода: ", searchAllDataInBD(dbFilePath, "cable", "name"), defult);
            }
            considerPowerLine.Icrict = searchDataInBD(dbFilePath, "cable", considerPowerLine.cable, "name", "Icrit");
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
                    SelectionFilter acSF = new SelectionFilter(new TypedValue[] { new TypedValue((int)DxfCode.Start, "LWPOLYLINE") });
                    //вектор это допусе поиска
                    PromptSelectionResult acPSR = ed.SelectCrossingWindow(searchPoint, searchPoint + new Vector3d(UserData.searchDistancePL,UserData.searchDistancePL, 0), acSF);
                    //PromptSelectionResult acPSR = ed.SelectCrossingWindow(searchPoint, searchPoint, acSF);

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

                            //Отсечь родителя 
                            if (acSObj.ObjectId != masterLine.IDLine && acSObj.ObjectId != masterLine.parent.IDLine)
                            {

                                //Вытянуть длинну и посмотреть на циклицность.
                                Polyline lengthPolyline = trAdding.GetObject(acSObj.ObjectId, OpenMode.ForWrite) as Polyline;


                                //Расстояние между точками для проверки соединить их в одну точку  и 5 процентов запаса
                                if (Math.Round( lengthPolyline.GetPoint3dAt(0).DistanceTo(searchPoint),0)<=UserData.searchDistancePL | Math.Round(lengthPolyline.GetPoint3dAt(0).DistanceTo(searchPoint), 2) > 0)  
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

                                //Приближаем
                                ZoomToEntity(acSObj.ObjectId, 4);

                                //Подсветка что выделилось
                                ed.SetImpliedSelection(new ObjectId[] { acSObj.ObjectId });
                                ed.CurrentUserCoordinateSystem = Matrix3d.Identity; // Сброс координатной системы, если необходимо


                                int defult = searchAllDataInBD(dbFilePath, "cable", "default").IndexOf("true") + 1;
                                PowerLine ChilderLine = new PowerLine();

                                if (ShowExtensionDictionaryContents(acSObj.ObjectId, "ESMT_LEP_v1.0")["isItem"] == "true")
                                {
                                    ChilderLine.cable = ShowExtensionDictionaryContents(acSObj.ObjectId, "ESMT_LEP_v1.0")["nameCable"];
                                    ed.WriteMessage("\n\nНАЙДЕНЫ ЗНАЧЕНИЯ ИЗ SMARTLINE\n\n");
                                }
                                else
                                {
                                    ChilderLine.cable = creatPromptKeywordOptions("Выберите мару провода: ", searchAllDataInBD(dbFilePath, "cable", "name"), defult);
                                }


                                ChilderLine.IDLine = acSObj.ObjectId;
                                ChilderLine.parent = masterLine;
                                ChilderLine.lengthLine = Math.Round(lengthPolyline.Length, 3);
                                ChilderLine.Icrict = searchDataInBD(dbFilePath, "cable", ChilderLine.cable, "name", "Icrit");

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
                    searchPlyline(ed, line, trAdding, listPoint, listPointXY, j);
                }
            }
            return masterLine;
        }

        private void DocumentInactiveEventHandler(object sender, DocumentCollectionEventArgs e)
        {
            // Этот метод будет вызван, когда документ станет неактивным,
            // что может произойти при закрытии файла
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n\n Закрываю Модуль ElectroTools");
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


        //Создание слоев
        public void creatLayer(string Name, byte ColorR, byte ColorG, byte ColorB, Editor ed, Database dbCurrent)
        {

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


        //Функция создания текста узлов
        void creatTextFromKnot(string nameSearchLayer, List<PointLine> masterPoint, string sizeText)
        {
            using (DocumentLock docloc = doc.LockDocument())
            {

                using (Transaction trAdding = dbCurrent.TransactionManager.StartTransaction())
                {

                    double size;

                    sizeText.Replace(",",".");
                    size = double.Parse(sizeText);
                    sizeText.Replace(".",",");
                    size = double.Parse(sizeText);

                                   


                    // Ищу на какой слой закинуть
                    LayerTable acLyrTbl = trAdding.GetObject(dbCurrent.LayerTableId, OpenMode.ForRead) as LayerTable;

                    ObjectId acLyrId = ObjectId.Null;
                    if (acLyrTbl.Has(nameSearchLayer))
                    {
                        acLyrId = acLyrTbl[nameSearchLayer];
                    }


                    // Для того что бы закинуть текст
                    BlockTable acBlkTbl = trAdding.GetObject(dbCurrent.BlockTableId, OpenMode.ForRead) as BlockTable;

                    BlockTableRecord acBlkTblRec = trAdding.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    foreach (PointLine itemPoint in masterPoint)
                    {
                        // Характеристики текста
                        MText acMText = new MText();
                        acMText.Color = Color.FromColorIndex(ColorMethod.ByAci, 256); ; //Цвето по слою
                        acMText.Contents = itemPoint.name.ToString();
                        acMText.Location = new Point3d(itemPoint.positionPoint.X, itemPoint.positionPoint.Y, 0);
                        acMText.TextHeight = size; //Размер шрифта было size
                                                   //acMText.Height = 5; //Высота чего ?
                        acMText.LayerId = acLyrId;
                        //acMText.Attachment = AttachmentPoint.MiddleCenter; //Центровка текста
                        acBlkTblRec.AppendEntity(acMText);
                        trAdding.AddNewlyCreatedDBObject(acMText, true);

                        //ID Mtext
                        itemPoint.IDText = acMText.ObjectId;
                    }
                    trAdding.Commit();
                }
            }


        }

        //Функция создания текста ребер
        void creatTextFromEdge(string nameSearchLayer, List<Edge> masterEdge, string sizeText)
        {
            double size;
            size = double.Parse(sizeText);

            // Ищу на какой слой закинуть
            using (DocumentLock docloc = doc.LockDocument())
            {

                using (Transaction trAdding = dbCurrent.TransactionManager.StartTransaction())
                {

                    LayerTable acLyrTbl = trAdding.GetObject(dbCurrent.LayerTableId, OpenMode.ForRead) as LayerTable;

                    ObjectId acLyrId = ObjectId.Null;
                    if (acLyrTbl.Has(nameSearchLayer))
                    {
                        acLyrId = acLyrTbl[nameSearchLayer];
                    }

                    // Для того что бы закинуть текст
                    BlockTable acBlkTbl = trAdding.GetObject(dbCurrent.BlockTableId, OpenMode.ForRead) as BlockTable;

                    BlockTableRecord acBlkTblRec = trAdding.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    foreach (Edge itemEdge in masterEdge)
                    {
                        // Характеристики текста
                        MText acMText = new MText();
                        acMText.Color = Color.FromColorIndex(ColorMethod.ByAci, 256); //Цвето по слою
                        acMText.Contents = itemEdge.name.ToString();
                        acMText.Location = new Point3d(itemEdge.centerPoint.positionPoint.X, itemEdge.centerPoint.positionPoint.Y, 0);
                        acMText.TextHeight = size; //Размер шрифта было size
                                                   //acMText.Height = 5; //Высота чего ?
                        acMText.LayerId = acLyrId;
                        acMText.Attachment = AttachmentPoint.MiddleCenter; //Центровка текста
                        acBlkTblRec.AppendEntity(acMText);
                        trAdding.AddNewlyCreatedDBObject(acMText, true);
                        //ID Mtext
                        itemEdge.IDText = acMText.ObjectId;
                    }
                    trAdding.Commit();
                }
            }

        }

        //Функция создания текста линий
        void creatTextFromLine(string nameSearchLayer, List<PowerLine> masterLine, string sizeText)
        {
            using (DocumentLock docloc = doc.LockDocument())
            {

                using (Transaction trAdding = dbCurrent.TransactionManager.StartTransaction())
                {

                    double size;
                    Double.TryParse(sizeText, out size);

                    // Ищу на какой слой закинуть
                    LayerTable acLyrTbl = trAdding.GetObject(dbCurrent.LayerTableId, OpenMode.ForRead) as LayerTable;

                    ObjectId acLyrId = ObjectId.Null;
                    if (acLyrTbl.Has(nameSearchLayer))
                    {
                        acLyrId = acLyrTbl[nameSearchLayer];
                    }

                    // что б стащить цвет
                    // LayerTableRecord acLyrTblRec = trAdding.GetObject(acLyrId, OpenMode.ForRead) as LayerTableRecord;

                    // Для того что бы закинуть текст
                    BlockTable acBlkTbl = trAdding.GetObject(dbCurrent.BlockTableId, OpenMode.ForRead) as BlockTable;

                    BlockTableRecord acBlkTblRec = trAdding.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    foreach (PowerLine itemLine in masterLine)
                    {
                        // Характеристики текста
                        MText acMText = new MText();
                        //acMText.Color = acLyrTblRec.Color; //Цвето по слою принудительно
                        acMText.Color = Color.FromColorIndex(ColorMethod.ByAci, 256);
                        acMText.Contents = itemLine.name.ToString();
                        acMText.Location = new Point3d(itemLine.points[0].positionPoint.X, itemLine.points[0].positionPoint.Y, 0);
                        acMText.TextHeight = size; //Размер шрифта было size
                                                   //acMText.Height = 5; //Высота чего ?
                        acMText.LayerId = acLyrId;
                        acMText.Attachment = AttachmentPoint.MiddleCenter; //Центровка текста
                        acBlkTblRec.AppendEntity(acMText);
                        itemLine.IDText = acMText.ObjectId;
                        trAdding.AddNewlyCreatedDBObject(acMText, true);
                    }
                    trAdding.Commit();
                }
            }


        }

        //Функция создания Мтекста
        void creatText(string nameSearchLayer, PointLine point, string text, string sizeText, short color, int difPosishion)
        {
            using (DocumentLock docloc = doc.LockDocument())
            {

                using (Transaction trAdding = dbCurrent.TransactionManager.StartTransaction())
                {

                    double size;
                    Double.TryParse(sizeText, out size);

                    // Ищу на какой слой закинуть
                    LayerTable acLyrTbl = trAdding.GetObject(dbCurrent.LayerTableId, OpenMode.ForRead) as LayerTable;

                    ObjectId acLyrId = ObjectId.Null;
                    if (acLyrTbl.Has(nameSearchLayer))
                    {
                        acLyrId = acLyrTbl[nameSearchLayer];
                    }

                    // что б стащить цвет
                    // LayerTableRecord acLyrTblRec = trAdding.GetObject(acLyrId, OpenMode.ForRead) as LayerTableRecord;

                    // Для того что бы закинуть текст
                    BlockTable acBlkTbl = trAdding.GetObject(dbCurrent.BlockTableId, OpenMode.ForRead) as BlockTable;

                    BlockTableRecord acBlkTblRec = trAdding.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    // Характеристики текста
                    MText acMText = new MText();
                    //acMText.Color = acLyrTblRec.Color; //Цвето по слою принудительно
                    acMText.Color = Color.FromColorIndex(ColorMethod.ByAci, color);
                    acMText.Contents = text;
                    acMText.Location = new Point3d(point.positionPoint.X, point.positionPoint.Y + difPosishion, 0);
                    acMText.TextHeight = size; //Размер шрифта было size
                                               //acMText.Height = 5; //Высота чего ?
                    acMText.LayerId = acLyrId;
                    acMText.Attachment = AttachmentPoint.MiddleCenter; //Центровка текста
                    acBlkTblRec.AppendEntity(acMText);
                    trAdding.AddNewlyCreatedDBObject(acMText, true);

                    trAdding.Commit();
                }
            }


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


        //Создает текст вершин
        string creatPromptKeywordOptions(string textName, List<string> listOptions, int defaultOptions)
        {
            //Для Acad, если пробел, он берет только первую часть 
            List<string> modifiedListOptions = listOptions.Select(option => option.Replace(" ", "_")).ToList();


            PromptKeywordOptions options = new PromptKeywordOptions(textName);

            foreach (string itemString in modifiedListOptions)
            {
                options.Keywords.Add(itemString);
            }
            options.Keywords.Default = modifiedListOptions[defaultOptions - 1]; // если сам, то -1

            PromptResult result = ed.GetKeywords(options);
            if (result.Status == PromptStatus.OK)
            {
                string selectedKeyword = result.StringResult.Replace("_", " ");
                ed.WriteMessage("\n\nВы выбрали : " + selectedKeyword + "\n\n");
                return selectedKeyword;
            }
            return null;
        }

        public void deleteObjectsOnLayer(string layerNameDelete, Database dbCurrent)
        {

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
                        r = searchDataInBD(dbFilePath, "cable", itemLine.cable, "name", "r"),
                        x = searchDataInBD(dbFilePath, "cable", itemLine.cable, "name", "x"),
                        r0 = searchDataInBD(dbFilePath, "cable", itemLine.cable, "name", "r0"),
                        x0 = searchDataInBD(dbFilePath, "cable", itemLine.cable, "name", "x0"),
                        Ke = searchDataInBD(dbFilePath, "cable", itemLine.cable, "name", "Ke"),
                        Ce = searchDataInBD(dbFilePath, "cable", itemLine.cable, "name", "Ce"),

                        Icrict = searchDataInBD(dbFilePath, "cable", itemLine.cable, "name", "Icrit"),
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
        //Получить веса вершин из  DWG
        public void setDataPoint(string layerName, List<PointLine> masterListPoint, Database dbCurrent, Editor ed, Transaction trAdding)
        {
            PromptSelectionResult res = ed.SelectAll(new SelectionFilter(new TypedValue[]
                      {
                            new TypedValue((int)DxfCode.LayerName, layerName),
                            new TypedValue((int)DxfCode.Start, "MTEXT"),
                  }
              ));
            if (res.Status == PromptStatus.OK)
            {
                ed.SetImpliedSelection(res.Value.GetObjectIds());
            }

            PromptSelectionResult acSSPrompt = ed.GetSelection();

            if (acSSPrompt.Status == PromptStatus.OK)
            {
                SelectionSet acSSet = acSSPrompt.Value;
                BlockTable acBlkTbl = trAdding.GetObject(dbCurrent.BlockTableId, OpenMode.ForRead) as BlockTable;

                foreach (SelectedObject acSSObj in acSSet)
                {
                    if (acSSObj != null)
                    {
                        MText acEnt = trAdding.GetObject(acSSObj.ObjectId, OpenMode.ForRead) as MText;


                        int index = (acEnt.Contents).IndexOf('P');

                        if (index > 0)
                        {

                            //Номер вершины
                            int numberVertex = int.Parse((acEnt.Contents).Substring(0, index - 1));
                            //Вес вершины
                            double weightVertex = double.Parse((acEnt.Contents.Replace(".", ",")).Substring(index + 1));

                            foreach (PointLine masterVertex in masterListPoint)
                            {
                                if (numberVertex == masterVertex.name)
                                {
                                    int indexColor = acEnt.ColorIndex;
                                    masterVertex.weight = weightVertex;
                                    // ed.WriteMessage("indexColor == 201: " + (indexColor == 201).ToString());

                                    // Получение индекса цвета Mtext
                                    if (indexColor == 201)

                                    {
                                        masterVertex.typeClient = 1;
                                    }

                                    else
                                    { masterVertex.typeClient = 3; }

                                }

                            }


                        }

                    }
                }

            }

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

            return null;
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

            int rows = masterListPoint.Count();
            int columns = masterListPoint.Count();
            int[,] matrix = new int[rows, columns];

            for (int j = 0; j < columns + 1; j++)
            {
                for (int i = 0; i < columns + 1; i++)
                {

                    if ((i + 1 < columns) & (j + 1 <= columns))
                    {
                        if ((masterListPoint[j] == masterListEdge[i].startPoint) ^ (masterListPoint[j] == masterListEdge[i].endPoint))
                        {
                            for (int k = 0; k < columns; k++)
                            {
                                if (((masterListPoint[k] == masterListEdge[i].endPoint) ^ (masterListPoint[k] == masterListEdge[i].startPoint)) & (j != k))
                                {
                                    matrix[j, k] = 1;

                                }
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
                newPointPathKZ = ListPathIntToPoint(findPath(matrixSmej, itemPoint.name - 1, 0));

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
                ed.WriteMessage("!Нельзя выбрать точку генерации!");
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

        void creatPL(List<PointLine> masterListPont, string nameLayer, short color, double ConstantWidth)
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

                }
            }
        }


        List<string> searchAllDataInBD(string dbFilePath, string nameTable, string searchColum, string filterColumn = null, string filterValue = null)
        {

            List<string> tempList = new List<string>();
            // Строка подключения к базе данных SQLite
            string connectionString = "Data Source=" + dbFilePath;
            //string connectionString = $"Data Source={dbFilePath};Version=3;";

            // SQL-запрос для выполнения (замените на свой запрос)
            string query;
            if (string.IsNullOrEmpty(filterValue) | string.IsNullOrEmpty(filterColumn))
            {
                query = "SELECT * FROM " + nameTable;
            }
            else
            {
                query = "SELECT * FROM " + nameTable + " WHERE " + filterColumn + " = " + filterValue;
            }


            using (DocumentLock doclock = doc.LockDocument())
            {

                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {



                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {

                                tempList.Add(reader[searchColum].ToString());
                                // Обрабатывайте результаты запроса
                                //ed.WriteMessage(reader[$"{searchColum}"].ToString());
                            }
                        }

                    }
                    connection.Close();
                }
            }

            return tempList;
        }

        double searchDataInBD(string dbFilePath, string nameTable, string searchItem, string searchColum, string gethColum)
        {
            double resultValue = 0;
            // Строка подключения к базе данных SQLite
            string connectionString = "Data Source=" + dbFilePath;
            //string connectionString = $"Data Source={dbFilePath};Version=3;";

            // SQL-запрос для выполнения (замените на свой запрос)
            string query = "SELECT * FROM " + nameTable + " WHERE " + searchColum + "= @searchItem";

            using (DocumentLock doclock = doc.LockDocument())
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@searchItem", searchItem);

                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (double.TryParse(reader[gethColum].ToString(), out resultValue))
                                {
                                    // Вернуть найденное значение из столбца "result"
                                    //ed.WriteMessage(resultValue.ToString());
                                }
                            }

                        }
                        connection.Close();
                    }

                    return resultValue;
                }
            }
        }

        ObjectId DrawCircle(PointLine itemPoint, string nameLayer)
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
        public void ZoomToEntity(ObjectId entityId, double zoomPercent)
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

        public int getColorMtext(PointLine itemPoint)
        {
            int colorIndex = 0;
            using (DocumentLock doclock = doc.LockDocument())
            {
                using (Transaction tr = dbCurrent.TransactionManager.StartTransaction())
                {
                    MText mtext = tr.GetObject(itemPoint.IDText, OpenMode.ForRead) as MText;

                    if (mtext != null)
                    {
                        // Получение индекса цвета Mtext
                        colorIndex = mtext.ColorIndex;


                    }
                    tr.Commit();
                }
            }
            return colorIndex;



        }

        private void updateColorMtext(PointLine itemPoint, int ColorIndex)
        {

            using (DocumentLock doclock = doc.LockDocument())
            {
                using (Transaction tr = dbCurrent.TransactionManager.StartTransaction())
                {
                    try
                    {
                        MText mtext = tr.GetObject(itemPoint.IDText, OpenMode.ForWrite) as MText;

                        if (mtext != null)
                        {
                            // Получение индекса цвета Mtext
                            mtext.ColorIndex = ColorIndex;
                        }
                        tr.Commit();
                    }
                    catch (System.Exception ex)
                    {
                        // Обработка ошибок
                        // ed.WriteMessage("Error updating MText: {0}\n", ex.Message);
                        tr.Abort();
                    }
                }
            }




        }





        Dictionary<string, string> ShowExtensionDictionaryContents(ObjectId entityId, string nameDictionary)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            using (DocumentLock doclock = doc.LockDocument())
            {

                using (Transaction tr = dbCurrent.TransactionManager.StartTransaction())
                {
                    try
                    {
                        // Открываем объект для чтения.
                        Entity entity = tr.GetObject(entityId, OpenMode.ForRead) as Entity;

                        if (entity != null && entity.ExtensionDictionary != ObjectId.Null)
                        {
                            DBDictionary extDict = tr.GetObject(entity.ExtensionDictionary, OpenMode.ForRead) as DBDictionary;

                            if (extDict != null && extDict.Contains(nameDictionary))
                            {
                                ObjectId entryId = extDict.GetAt(nameDictionary);

                                if (!entryId.IsNull)
                                {
                                    DBObject entryObj = tr.GetObject(entryId, OpenMode.ForRead);

                                    Xrecord xRecord = entryObj as Xrecord;

                                    if (xRecord != null)
                                    {
                                        //ed.WriteMessage("ExtensionDictionary contents for entity with ObjectId {0}:\n", entityId.Handle);


                                        /*
                                        // Получаем данные из Xrecord
                                        ResultBuffer data = xRecord.Data;
                                        foreach (TypedValue value in data)
                                        {
                                            ed.WriteMessage("TypedValue: {0}\n", value.Value);
                                        }   */


                                        result["isItem"] = "true";
                                        result["nameCable"] = DeserializeFromXrecord<Conductor>(xRecord).Name;

                                        //ed.WriteMessage("МОЕ: "+DeserializeFromXrecord(xRecord).Name);
                                        return result;
                                    }
                                    else
                                    {
                                        ed.WriteMessage("The entry with key" + nameDictionary + " is not an Xrecord.\n");

                                    }
                                }
                                else
                                {
                                    ed.WriteMessage("Не нашел: " + nameDictionary + " :(.\n");


                                }
                            }
                            else
                            {
                                ed.WriteMessage("\n Отсутствует:  " + nameDictionary + ".\n");


                            }
                        }
                        else
                        {
                            ed.WriteMessage("\n\nНе нашел значения из " + nameDictionary + " :(\n\n");
                            //ed.WriteMessage("Entity is null or does not have an ExtensionDictionary.\n");

                        }
                        //Если пусто
                        result["isItem"] = "false";
                        result["nameCable"] = "";
                        return result;

                    }

                    catch (Exception ex)
                    {
                        ed.WriteMessage("\nОшибка: {0}\n", ex.Message);
                        result["isItem"] = "false";
                        result["nameCable"] = "";
                        return result;
                    }
                    finally
                    {
                        tr.Dispose();

                    }
                }
            }
        }


        T BDShowExtensionDictionaryContents<T>(ObjectId entityId, string nameDictionary)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            using (DocumentLock doclock = doc.LockDocument())
            {
                using (Transaction tr = dbCurrent.TransactionManager.StartTransaction())
                {
                    try
                    {
                        // Открываем объект для чтения.
                        Entity entity = tr.GetObject(entityId, OpenMode.ForRead) as Entity;

                        if (entity != null && entity.ExtensionDictionary != ObjectId.Null)
                        {
                            DBDictionary extDict = tr.GetObject(entity.ExtensionDictionary, OpenMode.ForRead) as DBDictionary;

                            if (extDict != null && extDict.Contains(nameDictionary))
                            {
                                ObjectId entryId = extDict.GetAt(nameDictionary);

                                if (!entryId.IsNull)
                                {
                                    DBObject entryObj = tr.GetObject(entryId, OpenMode.ForRead);

                                    Xrecord xRecord = entryObj as Xrecord;

                                    if (xRecord != null)
                                    {
                                        //ed.WriteMessage("ExtensionDictionary contents for entity with ObjectId {0}:\n", entityId.Handle);
                                        /*
                                        // Получаем данные из Xrecord
                                        ResultBuffer data = xRecord.Data;
                                        foreach (TypedValue value in data)
                                        {
                                            ed.WriteMessage("TypedValue: {0}\n", value.Value);
                                        } */


                                        return DeserializeFromXrecord<T>(xRecord);
                                    }
                                    else
                                    {
                                        ed.WriteMessage("\n The entry with key" + nameDictionary + " is not an Xrecord.\n");

                                    }
                                }
                                else
                                {
                                    ed.WriteMessage(" \n Entry with key" + nameDictionary + " not found in ExtensionDictionary.\n");


                                }
                            }
                            else
                            {
                                ed.WriteMessage("\n ExtensionDictionary with key " + nameDictionary + " not found.\n");


                            }
                        }
                        else
                        {
                            ed.WriteMessage(" \n Не нашел значения из " + nameDictionary + " :(");
                            //ed.WriteMessage("Entity is null or does not have an ExtensionDictionary.\n");

                        }
                        //Если пусто
                        return default(T);

                    }

                    catch (System.Exception ex)
                    {
                        ed.WriteMessage("Error: {0}\n", ex.Message);

                        result["nameCable"] = "";
                        return default(T);
                    }
                    finally
                    {
                        tr.Dispose();

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
                ResultBuffer data = xRecord.Data;
                if (data == null)
                    throw new ArgumentException("Xrecord does not contain valid data.", "xRecord");
                TypedValue[] values = data.AsArray();
                if (values.Length == 1 && values[0].TypeCode == (int)DxfCode.Text)
                {
                    string xmlData = values[0].Value.ToString();
                    return DeserializeFromXml<T>(xmlData);
                }
                else
                {
                    throw new ArgumentException("Unexpected data format in Xrecord.", "xRecord");
                }
            }
            catch (InvalidOperationException ex)
            {
                // Обработка исключения или вывод информации о нем
                ed.WriteMessage("Ошибка десериализации: " + ex.Message);
                ed.WriteMessage($"StackTrace: " + ex.StackTrace);
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
                ed.WriteMessage("Error during deserialization: " + ex.Message);
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
                //File.WriteAllText(filePath, writer.ToString());

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
            using (DocumentLock doclock = doc.LockDocument())
            {
                using (Transaction tr = dbCurrent.TransactionManager.StartTransaction())
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
                        ed.WriteMessage("Ошибка: {0}\n", ex.Message);
                    }
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateData()
        {
            // Пример обновления данных
            listPoint = new List<PointLine> { new PointLine { name = 1, weight = 2.5 } };
            pathDLLFile = "New Path";
        }


        private void DocumentActivatedEventHandler(object sender, DocumentCollectionEventArgs e)
        {
            Document activatedDocument = e.Document;
            Editor ed = activatedDocument.Editor;

            if (ed != this.ed)
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









        /*	 	 
         public class Excel
         {
                //CreadFile();
                OpenFileExcel( CreadFile ( matrixInc ) );


                string CreadFile(int[,] matrix)
                {
                //Первое создани

                Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
                Workbook workbook = excel.Workbooks.Add();

                Worksheet worksheet1 = (Worksheet)workbook.Worksheets.Add();
                worksheet1.Name = "Матрица Инцидентности";

                Worksheet worksheet2 = (Worksheet)workbook.Worksheets.Add();
                worksheet2.Name = "Матрица Смежности";

                Worksheet worksheet4 = (Worksheet)workbook.Worksheets.Add();
                worksheet4.Name = "Матрица Веса Ребер";

                Worksheet worksheet3 = (Worksheet)workbook.Worksheets.Add();
                worksheet3.Name = "Матрица Веса Вершин";



                Range range1 = (Range)worksheet1.Cells[2, 7];
                range1.Value = "Ветви";
                range1.Font.Color = ColorTranslator.ToOle(System.Drawing.Color.Blue);
                range1.Font.Bold = true;

                Range range11 = (Range)worksheet1.Cells[1, 7];
                range11.Value = "Матрица Инцидентности Сети";
                range11.Font.Color = ColorTranslator.ToOle(System.Drawing.Color.Black);
                range11.Font.Bold = true;
                range11.Font.Size = 20;

                Range range2 = (Range)worksheet1.Cells[7, 1];
                range2.Value = "Узлы";
                range2.Font.Color = ColorTranslator.ToOle(System.Drawing.Color.Red);
                range2.Font.Bold = true;




                int up = 2;
                int left = 1;


                for (int row = 1; row <= matrix.GetLength(0); row++)
                {

                    Range range3 = (Range)worksheet1.Cells[up + 1 + row, left + 1];
                    range3.Value = row;
                    range3.Font.Bold = true;
                    range3.HorizontalAlignment = Microsoft.Office.Interop.Excel.Constants.xlCenter;

                }

                for (int col = 1; col <= matrix.GetLength(1); col++)
                {
                    Range range4 = worksheet1.Cells[up + 1, left + 1 + col] as Range;
                    range4.Value = col;
                    range4.Font.Bold = true;
                    range4.HorizontalAlignment = Microsoft.Office.Interop.Excel.Constants.xlCenter;
                }


                for (int j = 0; j < matrix.GetLength(0); j++)
                {
                    for (int i = 0; i < matrix.GetLength(1); i++)
                    {
                        Range range5 = worksheet1.Cells[up + 2 + j, left + 2 + i] as Range;
                        range5.Value = matrix[j, i];


                    }
                }

                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop); //Путь на рабочий стол
                string filePath = Path.Combine(desktopPath, "Матрицы.xlsx"); //Клеим стрингу

                workbook.SaveAs(filePath);
                workbook.Close();
                excel.Quit();

                Marshal.ReleaseComObject(worksheet1);
                Marshal.ReleaseComObject(worksheet2);
                Marshal.ReleaseComObject(worksheet3);
                Marshal.ReleaseComObject(worksheet4);
                Marshal.ReleaseComObject(workbook);
                Marshal.ReleaseComObject(excel);
                return filePath;
                }




            void OpenFileExcel(string path)
            {
                Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
                //Открыть книгу по пути
                Workbook workbook = excel.Workbooks.Open(path);
                excel.Visible = true;
                // Для первого плана
                excel.WindowState = Microsoft.Office.Interop.Excel.XlWindowState.xlMaximized;

                //Активный лист
                //Worksheet ws =workbook.ActiveSheet as Worksheet;

                foreach (Worksheet ws in workbook.Sheets)
                {
                    ws.Rows.RowHeight = 20;
                    ws.Columns.ColumnWidth = 4;
                }

            }

            }*/





    }



}