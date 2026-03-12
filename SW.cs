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

      

        private void CheckingIsPart()
        {
            if (model == null || model.GetType() != (int)swDocumentTypes_e.swDocPART)
            {
                swApp.SendMsgToUser("Откройте деталь (Part) и запустите макрос.");
                return;
            }
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

	public static class Calculator {


		 static Calculator(){

		}

	

		public static double GetOffsetPlane(){

			return 0;
		}

		public static double[] GetPointsFirstExtrude(this Weldment wd){

			return null;
		}

		public static double[] GetPointsSecondExtrude(this Weldment wd)
        {

			return null;
		}

		public static double[] GetPointsThirdExtrude(this Weldment wd)
        {

			return null;
		}

		/// 
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="c"></param>
		/// <param name="d"></param>
		/// <param name="tMax"></param>
		/// <param name="tMin"></param>
		private static void Slab(double min, double max, double c, double d, double tMax, double tMin){

		}

	}//end Calculator
}
