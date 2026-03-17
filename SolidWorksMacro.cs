
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
      
        }

    }
}
