using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyGroupCenter
{
    [TransactionAttribute(TransactionMode.Manual)]

    public class CopyGroup : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            
            try
            {
                UIDocument uiDoc = commandData.Application.ActiveUIDocument;
                Document doc = uiDoc.Document;

                GroupPickFilter groupPickFilter = new GroupPickFilter();
                Reference reference = uiDoc.Selection.PickObject(ObjectType.Element,groupPickFilter, "Выберите группу объектов");
                Element element = doc.GetElement(reference);
                Group group = element as Group;
                Room room = GetRoomByPoint(doc, groupCenter);
                XYZ roomCenter = GetElementCenter(room);
                XYZ offset = groupCenter - roomCenter;

                // координаты точки центра комнаты
                XYZ coorCenter = GetRoomByPoint(room);
                string coords = "X = " + coorCenter.X.ToString() + "\r\n" + "Y = " + coorCenter.Y.ToString() + "\r\n" + "Z = " + coorCenter.Z.ToString();
                TaskDialog.Show("Координаты центра комнаты вставки", coords);

                //вставка группы
                Transaction transaction = new Transaction(doc);

                transaction.Start();
                
                XYZ groupLocation = groupCenter + offset;
                doc.Create.PlaceGroup(groupLocation, group.GroupType);

                transaction.Commit();
                //


                XYZ point = uiDoc.Selection.PickPoint("Выберите точку");

                Transaction ts = new Transaction(doc);

                ts.Start("Копирование группы объектов");
                doc.Create.PlaceGroup(point, group.GroupType);
                ts.Commit();
            }

            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }

            catch(Exception ex)
            {
                message = ex.Message;   
                return Result.Failed;
            }
           
            return Result.Succeeded;
        }       
    }

    public class GroupPickFilter : ISelectionFilter
    { 
         public bool AllowElement(Element elem)
    {
            if(elem.Category.Id.IntegerValue==(int)BuiltInCategory.OST_IOSModelGroups)
                return true;
            else
                return false;
    }
    public bool AllowReference(Reference r, XYZ p)
    {
        return false;
    }

    public XYZ GetElementCenter (Element elem)
      {
            BoundingBoxXYZ bounding = elem.get_BoundingBox(null);
            XYZ center = (bounding.Max + bounding.Min) / 2;
            return center;
       }

       public Room GetRoomByPoint(Document doc, XYZ point)

        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfCategory(BuiltInCategory.OST_Rooms);
            

            foreach (Element elem in collector)
            {
                Room room = elem as Room;
                if (room != null)
                {
                                    
                    if (room.IsPointInRoom(point))
                    {
                        return room;
                    }
                }
            }
            return null;
        }
    }
}
