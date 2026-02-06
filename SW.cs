using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System.Diagnostics;
using System.IO;

namespace StructuralWeldment
{
   public class SW
    {
        public SldWorks swApp;
        ModelDoc2 model;
        ModelDocExtension swModelDocExt;
        SelectionMgr slMg;
        MathUtility mathUtility;
        FeatureManager fm;
        Weldment profile;

        public SW(SldWorks _swApp)
        {
            swApp = _swApp;
            model = (ModelDoc2)swApp.ActiveDoc;
            mathUtility = (MathUtility)swApp.GetMathUtility();
            slMg = (SelectionMgr)model.SelectionManager;
            CheckingIsPart();
            profile=GetWeldment();
            double[] box = GetBoundinBox();
            CreateReferenceAxisForWeldmentBody(box, profile.pointFirst, profile.pointSecond);
        }

        private  Feature CutUser(ModelDoc2 model)
        {
            return ((Feature)(model.FeatureManager.FeatureCut4(false, false, false, 9, 1,
                0.0, 0.0, false, false, false, false,
                0.0, 0.0, false, false, false, false,
                false, true, true, true, true, false, 0, 0, false, false)));
        }

        private void CheckingIsPart()
        {
            if (model == null || model.GetType() != (int)swDocumentTypes_e.swDocPART)
            {
                swApp.SendMsgToUser("Откройте деталь (Part) и запустите макрос.");
                return;
            }
        }

        private  Weldment GetWeldment()
        {
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
                return null;
            }
            // Получаем группы и сегменты и проверяем что ровно одна группа и ровно один сегмент
            weldData.AccessSelections(model, null);
            object[] groups = (object[])weldData.Groups;

            if (groups == null || groups.Length != 1)
            {
                swApp.SendMsgToUser("Элемент должен содержать ровно одну группу.");
                weldData.ReleaseSelectionAccess();
                return null;
            }

            StructuralMemberGroup group = (StructuralMemberGroup)groups[0];
            object[] segs = (object[])group.Segments;

            if (segs == null || segs.Length != 1)
            {
                swApp.SendMsgToUser("В группе должен быть ровно один сегмент.");
                weldData.ReleaseSelectionAccess();
                return null;
            }


            // Получаем сегмент и линию
            SketchSegment seg = (SketchSegment)segs[0];
            SketchLine line = seg as SketchLine;
            if (line == null)
            {
                swApp.SendMsgToUser("Сегмент должен быть линией.");
                weldData.ReleaseSelectionAccess();
                return null;
            }

            // Получаем начало и конец линии (координаты в модели)
            ISketchPoint point1 = (ISketchPoint)line.GetStartPoint2(); // {x,y,z}
            ISketchPoint point2 = (ISketchPoint)line.GetEndPoint2();
            double[] p1 = new double[3];
            double[] p2 = new double[3];
            p1[0] = point1.X;
            p1[1] = point1.Y;
            p1[2] = point1.Z;
            p2[0] = point2.X;
            p2[1] = point2.Y;
            p2[2] = point2.Z;
            // Сохраняем угол (если понадобится)
            double angle = group.Angle;
            weldData.ReleaseSelectionAccess();
            string templatePath = weldData.WeldmentProfilePath;
            string template = Path.GetFileNameWithoutExtension(templatePath);
            string[] parametry = template.Split('x');
            Weldment profile = new Weldment(template);
            profile.SetCenterPoits(p1, p2);
            return profile;
        }

        public  double[] GetBoundinBox()
        {
            PartDoc part = model as PartDoc;
            if (part == null)
            {
                swApp.SendMsgToUser("Ошибка: не удалось привести документ к PartDoc.");
                return null;
            }

            double[] box = null;

            try
            {
                box = part.GetPartBox(true) as double[];
            }
            catch
            {
                swApp.SendMsgToUser("Не удалось получить bounding box детали. Проверьте версию API.");
                return null;
            }


            if (box == null || box.Length < 6)
            {
                swApp.SendMsgToUser("Не удалось получить корректный bounding box.");
                return null;
            }

            return box;

        }

        public void CreateReferenceAxisForWeldmentBody(
              double[] box,
              double[] p1,   // направление (м)
              double[] p2
)
        {


            double xmin = box[0], ymin = box[1], zmin = box[2];
            double xmax = box[3], ymax = box[4], zmax = box[5];

            // ---------------- Центр ----------------
            double cx = 0.5 * (xmin + xmax);
            double cy = 0.5 * (ymin + ymax);
            double cz = 0.5 * (zmin + zmax);

            // ---------------- Направление ----------------
            double dx = p2[0] - p1[0];
            double dy = p2[1] - p1[1];
            double dz = p2[2] - p1[2];

            if (Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz) < 1e-9)
                throw new Exception("Нулевое направление");

            // ---------------- Slab method ----------------
            double tMin = double.NegativeInfinity;
            double tMax = double.PositiveInfinity;




            Slab(xmin, xmax, cx, dx, tMax, tMin);
            Slab(ymin, ymax, cy, dy, tMax, tMin);
            Slab(zmin, zmax, cz, dz, tMax, tMin);

            // ---------------- Конечные точки оси ----------------
            double[] A = { cx + dx , cy + dy , cz + dz};
            double[] B = { cx + dx , cy + dy , cz + dz};

            // ---------------- MathPoint ----------------
            IMathPoint mpA = (IMathPoint)mathUtility.CreatePoint(A);
            IMathPoint mpB = (IMathPoint)mathUtility.CreatePoint(B);

            // ---------------- Reference Axis ----------------
            model.ClearSelection2(true);

            ((IEntity)mpA).Select2(false, -1);
            ((IEntity)mpB).Select2(true, -1);

            model.InsertAxis();
            Feature featAxis = (Feature)slMg.GetSelectedObject6(1, 0);
            if (featAxis == null)
            {
                swApp.SendMsgToUser("Не удалось создать ось по сегменту.");
            }
            
        }

        static void Slab(double min, double max, double c, double d, double tMax, double tMin)
        {
          

            if (Math.Abs(d) < 1e-9)
            {
                if (c < min || c > max)
                    throw new Exception("Линия не пересекает тело");
                return;
            }

            double t1 = (min - c) / d;
            double t2 = (max - c) / d;
            if (t1 > t2)
            {
                double temp = t1;
                t1 = t2;
                t2 = temp;
            }

            tMin = Math.Max(tMin, t1);
            tMax = Math.Min(tMax, t2);
        }
    }
}
