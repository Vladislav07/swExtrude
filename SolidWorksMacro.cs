
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Diagnostics;
using System.IO;



namespace StructuralWeldment
{
    public partial class SolidWorksMacro
    {
        public SldWorks swApp;
        ModelDoc2 model;
        ModelDocExtension swModelDocExt;
        SelectionMgr selMgr;
        MathUtility mathUtility;
        FeatureManager fm;
        Weldment profile;
        bool statusSelect;
        bool isCreate;
        public void Main()
        {

            model = (ModelDoc2)swApp.ActiveDoc;
            mathUtility = (MathUtility)swApp.GetMathUtility();
            swModelDocExt = model.Extension;
            fm = model.FeatureManager;
            selMgr = (SelectionMgr)model.SelectionManager;
            SelectData data = selMgr.CreateSelectData();
            data.Mark = 1;
            IEntity selSurg = (IEntity)selMgr.GetSelectedObject6(1, -1);

           // byte[] faceRef = (byte[])swModelDocExt.GetPersistReference3(selSurg);

            

            RefPlane refPlane = (RefPlane)fm.InsertRefPlane(
               (int)swRefPlaneReferenceConstraints_e.swRefPlaneReferenceConstraint_Coincident, 0,
               -1, 0,
               0, 0);

           // int Error;
           // IEntity restored =(IEntity)swModelDocExt.GetObjectByPersistReference3(faceRef,out Error);
            statusSelect = selSurg.Select2(false, 0);
            object[] pointFeature = (object[])fm.InsertReferencePoint(4, 2, 0, 1);
            IFeature p = (IFeature)pointFeature[0];
            model.ClearSelection2(true);

            statusSelect = p.Select2(false, 0);
           
            statusSelect= selSurg.Select2(true, 0);

            statusSelect = model.InsertAxis2(true);
           
            Feature featAxis = (Feature)selMgr.GetSelectedObject6(1, 0);

            if (featAxis == null)
            {
                swApp.SendMsgToUser("Не удалось создать ось по сегменту.");
                return;
            }

            RefAxis axis = featAxis.GetSpecificFeature2() as RefAxis;
            if (axis == null)
            {
                // Иногда нужно получать через Feature.GetDefinition() -> RefAxis; но оставим проверку
                // TODO: при ошибке — заменить на ((RefAxis)featAxis.GetDefinition())
            }
            string nameAxis = featAxis.Name;



            Feature swFeat = (Feature)model.FirstFeature();
            StructuralMemberFeatureData weldData = null;
            while (swFeat != null)
            {
                string tmp = swFeat.GetTypeName();
                if (swFeat.GetTypeName() == "WeldMemberFeat")
                {

                    weldData = (StructuralMemberFeatureData)swFeat.GetDefinition();
                    break;
                }
                else
                {
                    swFeat = (Feature)swFeat.GetNextFeature();
                }

            }

            if (weldData == null)
            {
                swApp.SendMsgToUser("No Structural Member (элемент сварной балки).");
                return;
            }
            // Получаем группы и сегменты и проверяем что ровно одна группа и ровно один сегмент
            weldData.AccessSelections(model, null);
            object[] groups = (object[])weldData.Groups;

            if (groups == null || groups.Length != 1)
            {
                swApp.SendMsgToUser("Элемент должен содержать ровно одну группу.");
                weldData.ReleaseSelectionAccess();
                return;
            }

            StructuralMemberGroup group = (StructuralMemberGroup)groups[0];
            object[] segs = (object[])group.Segments;

            if (segs == null || segs.Length != 1)
            {
                swApp.SendMsgToUser("В группе должен быть ровно один сегмент.");
                weldData.ReleaseSelectionAccess();
                return;
            }


            // Получаем сегмент и линию
            SketchSegment seg = (SketchSegment)segs[0];
            SketchLine line = seg as SketchLine;
            if (line == null)
            {
                swApp.SendMsgToUser("Сегмент должен быть линией.");
                weldData.ReleaseSelectionAccess();
                return;
            }
            // --- 3) Получаем плоскость эскиза, на которой лежит исходный сегмент ---
            Sketch sketch = (Sketch)seg.GetSketch();
            if (sketch == null)
            {
                swApp.SendMsgToUser("Не удалось получить эскиз сегмента.");
                return;
            }

            int swType = (int)swSelectType_e.swSelDATUMPLANES;
            // GetReferenceEntity возвращает ссылку на плоскость эскиза
            RefPlane sketchPlane = sketch.GetReferenceEntity(ref swType) as RefPlane;
            if (sketchPlane == null)
            {
                swApp.SendMsgToUser("Не удалось получить плоскость эскиза сегмента.");
                return;
            }

            weldData.ReleaseSelectionAccess();
            string namePlaneSketch = ((Feature)sketchPlane).Name;
            double angle = group.Angle;
            // --- 4) Создаём новую плоскость: coincident (по оси) + angle относительно sketch plane ---
            model.ClearSelection2(true);

            // выделяем axis и plane по имени (SelectByID2)
            statusSelect = swModelDocExt.SelectByID2(nameAxis, "AXIS", 0, 0, 0, true, 0, null, (int)swSelectOption_e.swSelectOptionDefault);
            statusSelect = swModelDocExt.SelectByID2(namePlaneSketch, "PLANE", 0, 0, 0, true, 1, null, (int)swSelectOption_e.swSelectOptionDefault);

            FeatureManager featMgr = model.FeatureManager;

            // создаём плоскость: Coincident (ось) и Angle (плоскость эскиза) — угол используем из group.Angle
            RefPlane newPlane = (RefPlane)featMgr.InsertRefPlane(
                (int)swRefPlaneReferenceConstraints_e.swRefPlaneReferenceConstraint_Coincident, 0,
                (int)swRefPlaneReferenceConstraints_e.swRefPlaneReferenceConstraint_Angle, angle,
                0, 0);

            if (newPlane == null)
            {
                swApp.SendMsgToUser("Ошибка: не удалось создать опорную плоскость.");
                return;
            }



            MathPoint[] boxPlane = GetBoundingBox(refPlane);  // получение точек обрамляющих с плоскости выделения
          //  double[] pointFirst = { boxPlane[0], boxPlane[1], boxPlane[2] };
           // double[] pointSecond = { boxPlane[3], boxPlane[4], boxPlane[5] };

            model.ClearSelection2(true);
            ((Feature)newPlane).Select(true);
            model.SketchManager.InsertSketch(true);

            Sketch sketchPlane1 = (Sketch)model.SketchManager.ActiveSketch as Sketch;
            Feature sketchFeatureName = (Feature)sketchPlane1;

            MathTransform mt = sketchPlane1.ModelToSketchTransform;

            //MathPoint pointBox1 = (MathPoint)mathUtility.CreatePoint(pointFirst);
            MathPoint point1ToSkecth = (MathPoint)boxPlane[0].MultiplyTransform(mt);
            double[] coordPoint1 = (double[])point1ToSkecth.ArrayData;
            // MathPoint pointBox2 = (MathPoint)mathUtility.CreatePoint(pointSecond);
            MathPoint point2ToSkecth = (MathPoint)boxPlane[1].MultiplyTransform(mt);
            double[] coordPoint2 = (double[])point2ToSkecth.ArrayData;
            Debug.Print(coordPoint2[1].ToString());
            SketchSegment segmentLineCenter = model.SketchManager.CreateLine(coordPoint1[0], coordPoint1[1], coordPoint1[2], coordPoint2[0], coordPoint2[1], coordPoint2[2]);
            model.SketchManager.InsertSketch(true);
            /*

             MathPoint pointBox1 = (MathPoint)mathUtility.CreatePoint(pointLimitedBoxFirst);

             //  Surface surfacePlane = (Surface)newPlane;

             // double[] skPt1 = (double[])surfacePlane.GetClosestPointOn(box[0],box[1],box[2]);
             // double[] skPt2 = (double[])surfacePlane.GetClosestPointOn(p1[0], p1[1], p1[2]);

             //  double[] paramPlane = (double[])surfacePlane.PlaneParams;


             model.ClearSelection2(true);
             ((Feature)newPlane).Select(true);
             model.SketchManager.InsertSketch(true);

             Sketch sketchPlane1 = (Sketch)model.SketchManager.ActiveSketch as Sketch;
             Feature sketchFeatureName = (Feature)sketchPlane1;

             MathTransform mt = sketchPlane1.ModelToSketchTransform;
             MathPoint point1ToSkecth = (MathPoint)pointBox1.MultiplyTransform(mt);
             double[] coordPoint1 = (double[])point1ToSkecth.ArrayData;
             SketchSegment segmentLineCenter = model.SketchManager.CreateLine(p1[0], p1[1], p1[2], p2[0], p2[1], p2[2]);
             segmentLineCenter.ConstructionGeometry = true;
             // model.SketchManager.SetDynamicMirror()
             double[] coordPoint2 = new double[3];
             double[] coordPoint3 = new double[3];
             if (coordPoint1[0] == p1[0])
             {
                 coordPoint2[1] = p1[1] + Math.Abs(coordPoint1[1] - p1[1]);
                 coordPoint2[0] = p1[0];
                 coordPoint3[1] = coordPoint1[1];
                 coordPoint3[0] = coordPoint1[0] + (Math.Abs(coordPoint1[1]) + Math.Abs(coordPoint2[1]));
             }
             else if (coordPoint1[1] == p1[1])
             {
                 coordPoint2[0] = p1[0] + Math.Abs(coordPoint1[0] - p1[0]);
                 coordPoint2[1] = p1[1];
                 coordPoint3[0] = coordPoint1[0];
                 coordPoint3[1] = coordPoint1[1] + Math.Abs(coordPoint1[0]) + Math.Abs(coordPoint2[0]);
             }
             coordPoint2[2] = p1[2];
             coordPoint3[2] = p1[2];
             SketchSegment segmentLine1 = model.SketchManager.CreateLine(coordPoint2[0], coordPoint2[1], coordPoint2[2], coordPoint1[0], coordPoint1[1], coordPoint1[2]);
             segmentLine1 = model.SketchManager.CreateLine(coordPoint1[0], coordPoint1[1], coordPoint1[2], coordPoint3[0], coordPoint3[1], coordPoint3[2]);
             segmentLine1 = model.SketchManager.CreateLine(coordPoint3[0], coordPoint3[1], coordPoint3[2], coordPoint2[0], coordPoint2[1], coordPoint2[2]);
             model.ClearSelection2(true);


             model.SketchManager.InsertSketch(true);
             bool bst = sketchFeatureName.Select(true);
             Feature featExtr = CutUser(model);

             if (featExtr == null)
             {
                 swApp.SendMsgToUser("Предупреждение: не удалось создать Extrude через всё. Проверьте сигнатуры FeatureExtrusion2 в вашей версии API.");
             }
             else
             {
                 swApp.SendMsgToUser("Готово: эскиз построен и (попытка) выполнено выдавливание.");
             }

             // Очистка выделения
             model.ClearSelection2(true);

             */

        }

        private static MathPoint[] GetBoundingBox(IRefPlane refPlane)
        {
            MathPoint[] swMathPoint =new MathPoint[2];
            MathPoint tmp = refPlane.IGetBoundingBox();
            double[] t = (double[])tmp.ArrayData;
           // swMathPoint[1] = (MathPoint)tmp[1];
            return swMathPoint;
        }

       

    }
}
