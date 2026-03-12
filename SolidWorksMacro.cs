
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Diagnostics;
using System.IO;



namespace StructuralWeldment
{
    public partial class SolidWorksMacro
    {
        public  SldWorks swApp;
        private Controler contr;

        public void Main()
        {

            contr = new Controler(swApp);
        


      

          

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
