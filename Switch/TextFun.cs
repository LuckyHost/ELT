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
using Microsoft.Office.Interop.Excel;
using System.Windows.Forms;
using ElectroTools;
using System.Windows.Controls;
using ElectroTools;






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
    public class TextFun
    {
       
        private static Document doc = MyOpenDocument.doc;
        private static Database dbCurrent=MyOpenDocument.dbCurrent;
        private static Editor ed=MyOpenDocument.ed;


        public static void updateColorMtext(ElectroTools.PointLine itemPoint, int ColorIndex)
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

        public static void updateTextById(ObjectId textId, string newText, int colorIndex)
        {
            if (textId == null | newText == null)
            { return; }
            using (DocumentLock doclock = doc.LockDocument())
            {
                using (Transaction tr = dbCurrent.TransactionManager.StartTransaction())
                {
                    try
                    {


                        MText mtextEntity = tr.GetObject(textId, OpenMode.ForWrite) as MText;

                        if (mtextEntity != null)
                        {
                            // Изменяем текст
                            mtextEntity.Contents = newText;

                            // Пример других изменений свойств (необязательно)
                            mtextEntity.ColorIndex = colorIndex; // Например, изменяем цвет текста

                            // Завершаем транзакцию
                            tr.Commit();
                        }
                        else
                        {
                            // Обработка случая, если не удалось получить объект MText
                            ed.WriteMessage("Unable to open MText with ObjectId\n");
                        }
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
    }
}

