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
   public static class Calculator
    {

        public static double[][] GetCentetPonts(this Weldment wd, double[] box)
        {
            double[] p1 = wd.pointFirst;
            double[] p2 = wd.pointSecond;

            double xmin = box[0], ymin = box[1], zmin = box[2];
            double xmax = box[3], ymax = box[4], zmax = box[5];

            double cx = 0.5 * (xmin + xmax);
            double cy = 0.5 * (ymin + ymax);
            double cz = 0.5 * (zmin + zmax);

            double dx = p2[0] - p1[0];
            double dy = p2[1] - p1[1];
            double dz = p2[2] - p1[2];

            if (Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz) < 1e-9)
                throw new Exception("Нулевое направление");

            double tMin = double.NegativeInfinity;
            double tMax = double.PositiveInfinity;

            Slab(xmin, xmax, cx, dx, ref tMin, ref tMax);
            Slab(ymin, ymax, cy, dy, ref tMin, ref tMax);
            Slab(zmin, zmax, cz, dz, ref tMin, ref tMax);

            if (tMin > tMax)
                throw new Exception("Нет пересечения");

            double[] A = { cx + dx * tMin, cy + dy * tMin, cz + dz * tMin };
            double[] B = { cx + dx * tMax, cy + dy * tMax, cz + dz * tMax };
            A = CleanPoint(A);
            B = CleanPoint(B);
            return new[] { A, B };
        }


        private static void Slab(
            double min,
            double max,
            double c,
            double d,
            ref double tMin,
            ref double tMax)
        {
            if (Math.Abs(d) < 1e-9)
            {
                if (c < min || c > max)
                    throw new Exception("Линия вне slab");
                return;
            }

            double t1 = (min - c) / d;
            double t2 = (max - c) / d;

            if (t1 > t2)
            {
                double tmp = t1;
                t1 = t2;
                t2 = tmp;
            }

            tMin = Math.Max(tMin, t1);
            tMax = Math.Min(tMax, t2);
        }

        static double[] CleanPoint(double[] p, double eps = 1e-9)
        {
            return new[]
            {
                Math.Abs(p[0]) < eps ? 0.0 : p[0],
                Math.Abs(p[1]) < eps ? 0.0 : p[1],
                Math.Abs(p[2]) < eps ? 0.0 : p[2]
            };
        }


        public static double GetOffsetPlane(){

			return 0;
		}

		

		public static double[] GetPointsThirdExtrude(this Weldment wd)
        {

			return null;
		}

        public static double[] GetPointsFirstExtrude(this Weldment wd, bool Start)
        {
            double fx = wd.pointFirst[0];
            double fy = wd.pointFirst[1];
            double fz = wd.pointFirst[2];
            double sx = wd.pointSecond[0];
            double sy = wd.pointSecond[1];
            double sz = wd.pointSecond[2];
            double a_mm = wd.a/2;
            double e_deg = wd.axis;
            double a = a_mm / 1000.0;
            double e = e_deg * Math.PI / 180.0;

            double[] F = { fx, fy, fz };
            double[] S = { sx, sy, sz };

            double[] axis = Normalize(Sub(S, F));

            double[] Z = { 0, 0, 1 };
            double[] X = { 1, 0, 0 };

            double[] refV =
                Math.Abs(Dot(axis, Z)) < 0.95 ? Z : X;

            double[] v = Normalize(Cross(axis, refV));

            double[] Base = Start ? F : S;

            double[] P1 = Add(Base, Scale(v, a));
            double[] P2 = Add(Base, Scale(v, -a));

            double d = 2 * a / Math.Tan(e);

            double[] P3 = Add(P1, Scale(axis, d));


            return new double[]
            {
                P1[0],P1[1],P1[2],
                P2[0],P2[1],P2[2],
                P3[0],P3[1],P3[2]
            };
        }
        static double[] Add(double[] a, double[] b)
        {
            return new[] { a[0] + b[0], a[1] + b[1], a[2] + b[2] };
        }

        static double[] Sub(double[] a, double[] b)
        {
            return new[] { a[0] - b[0], a[1] - b[1], a[2] - b[2] };
        }

        static double[] Scale(double[] a, double s)
        {
            return new[] { a[0] * s, a[1] * s, a[2] * s };
        }

        static double Dot(double[] a, double[] b)
        {
            return a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
        }

        static double[] Cross(double[] a, double[] b)
        {
            return new[]{
        a[1]*b[2]-a[2]*b[1],
        a[2]*b[0]-a[0]*b[2],
        a[0]*b[1]-a[1]*b[0]};
        }

        static double[] Normalize(double[] v)
        {
            double l = Math.Sqrt(Dot(v, v));
            return new[] { v[0] / l, v[1] / l, v[2] / l };
        }

        public static double[] GetPointsSecondExtrude(this Weldment wd, double[] Points)
        {
            double[] P1 = new double[] { Points[0], Points[1], Points[2] };
            double[] P2 = new double[] { Points[3], Points[4], Points[5] };
            double[] P3 = new double[] { Points[6], Points[7], Points[8] };
            double s = wd.s / 1000.0;

            // L1: направление P1P3
            double[] d1 = Normalize(Sub(P3, P1));
            if (Dot(d1, Sub(P3, P2)) < 0) d1 = Scale(d1, -1);

            // Q1 = P2
            double[] Q1 = P2;

            // Q2 на L1
            double[] Q2 = Add(P2, Scale(d1, s));

            // Направление P1->P2
            double[] v_base = Sub(P1, P2);

            // Линия P2P3
            double[] d3 = Sub(P3, P2);

            // Нормаль плоскости треугольника
            double[] n = Normalize(Cross(Sub(P2, P1), Sub(P3, P1)));

            // Проекция v_base на плоскость, перпендикулярную d3
            double[] v_proj = Sub(v_base, Scale(n, Dot(v_base, n)));
            v_proj = Normalize(v_proj);

            // Проверка направления в сторону P1
            if (Dot(v_proj, Sub(P1, Q2)) < 0)
                v_proj = Scale(v_proj, -1);

            // Решаем пересечение
            double denom = Dot(Cross(v_proj, d3), n);
            if (Math.Abs(denom) < 1e-9)
                throw new Exception("Линии почти параллельны");

            double t = Dot(Cross(Sub(P2, Q2), d3), n) / denom;

            double[] Q3 = Add(Q2, Scale(v_proj, t));

            return new double[]
            {
                Q1[0],Q1[1],Q1[2],
                Q2[0],Q2[1],Q2[2],
                Q3[0],Q3[1],Q3[2]
            };
        }

    }
}
