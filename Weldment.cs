using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StructuralWeldment
{
 
    public   enum ProfileSide
    {
       LeftSide, RightSide
    }

    public enum RotationAxis
    {
        First  = 0,
        Second = 90,
        Third = 180,
        Four = 270
    }

    public class Weldment
    {
        public ProfileSide profileSide { get; set; }
        public RotationAxis rotationAxis { get; set; }
        public double axis { get; set; }
        public int a { get; set; }
        public int b { get; set; }
        public double s { get; set; }
        public double[] pointFirst { get; set; }
        public double[] pointSecond { get; set; }
        public string namePlane { get; set; }

        public Weldment(string templateProfile)
        {
            profileSide = ProfileSide.LeftSide;
            rotationAxis = RotationAxis.First;
            axis = 45;
            string[] parametry = templateProfile.Split('x');
            try
            {
               a = Convert.ToInt16(parametry[0]);
               b = Convert.ToInt16(parametry[1]);
               s = Convert.ToDouble(parametry[2]);
            }
            catch (Exception)
            {
                Debug.Print(templateProfile);
                throw;
            }
           
        }

        public void SetCenterPoits(double[] _pointFirst, double[] _pointSecond )
        {
            pointFirst = _pointFirst;
            pointSecond = _pointSecond;

        }


    }
}
