using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.swpublished;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace StructuralWeldment
{
    [ComVisibleAttribute(true)]
    public class clsPropMgr : PropertyManagerPage2Handler9
    {
        public event Action<double> rotate;
        public event Action action;
        public event Action<double> angle;

        PropertyManagerPage2 pm_Page;
        PropertyManagerPageGroup pm_Group;
        PropertyManagerPageLabel labelRotationTitle;
        PropertyManagerPageNumberbox labelRotation;
        PropertyManagerPageButton btnRotationCycle;
        PropertyManagerPageLabel labelAngleTitle;
        PropertyManagerPageNumberbox numAngle;
        PropertyManagerPageOption radioRight;
        PropertyManagerPageOption radioLeft;

        const int GroupID = 1;
        const int LabelRotationTitleID = 2;
        const int LabelRotationID = 3;
        const int BtnRotationID = 4;
        const int LabelAngleTitleID = 5;
        const int NumAngleID = 6;
        const int RadioRightID = 7;
        const int RadioLeftID = 8;

        public bool IsOpen { get; internal set; }
     

        public void Show()

        {

            pm_Page.Show2(0);

        }

        public clsPropMgr(SldWorks swApp)
        {
            this.swApp = swApp;
            this.model = (ModelDoc2)swApp.ActiveDoc;

            int longerrors = 0;
            string pageTitle = "Extrude Control";
            int options = (int)swPropertyManagerPageOptions_e.swPropertyManagerOptions_OkayButton|
                (int)swPropertyManagerPageOptions_e.swPropertyManagerOptions_CancelButton|
                (int)swPropertyManagerPageOptions_e.swPropertyManagerOptions_PreviewButton;
            int longErrors = 0;

            pm_Page = (PropertyManagerPage2)swApp.CreatePropertyManagerPage(pageTitle, options, this, ref longErrors);

            if (longErrors != (int)swPropertyManagerPageStatus_e.swPropertyManagerPage_Okay)
            {
                System.Windows.Forms.MessageBox.Show("Failed to create PropertyManager page.");
                return;
            }

            // --- Добавляем группу ---
            pm_Group = (PropertyManagerPageGroup)pm_Page.AddGroupBox(GroupID, "Rotation Settings",
                (int)swAddGroupBoxOptions_e.swGroupBoxOptions_Visible + (int)swAddControlOptions_e.swControlOptions_Enabled);

            short leftAlign = (short)swPropertyManagerPageControlLeftAlign_e.swControlAlign_LeftEdge;
            int ctrlOptions = (int)swAddControlOptions_e.swControlOptions_Visible + (int)swAddControlOptions_e.swControlOptions_Enabled;

          
            // --- Label "Rotation" ---
            labelRotationTitle = (PropertyManagerPageLabel)pm_Group.AddControl2(LabelRotationTitleID,
                (int)swPropertyManagerPageControlType_e.swControlType_Label, "Rotation", leftAlign, ctrlOptions, "");

            // --- Label текущего значения ---
            labelRotation = (PropertyManagerPageNumberbox)pm_Group.AddControl2(LabelRotationID,
                (int)swPropertyManagerPageControlType_e.swControlType_Numberbox, "Rotation", leftAlign, ctrlOptions, "");
            labelRotation.SetRange2((int)swNumberboxUnitType_e.swNumberBox_Angle, -90 * Math.PI / 180.0, 360 * Math.PI / 180.0, false, 90 * Math.PI / 180.0, 90 * Math.PI / 180.0, 90 * Math.PI / 180.0);
            labelRotation.Value = 0;
            // --- Кнопка для циклической смены значения ---
            btnRotationCycle = (PropertyManagerPageButton)pm_Group.AddControl2(BtnRotationID,
                (int)swPropertyManagerPageControlType_e.swControlType_Button, "Cycle Rotation", leftAlign, ctrlOptions, "Click to change rotation");
            btnRotationCycle.Caption = "Togle";

            // --- Label "Angle" ---
            labelAngleTitle = (PropertyManagerPageLabel)pm_Group.AddControl2(LabelAngleTitleID,
                (int)swPropertyManagerPageControlType_e.swControlType_Label, "Angle", leftAlign, ctrlOptions, "");

            // --- Числовое поле для угла 5..85 ---
            numAngle = (PropertyManagerPageNumberbox)pm_Group.AddControl2(NumAngleID,
                (int)swPropertyManagerPageControlType_e.swControlType_Numberbox, "Angle Value", leftAlign, ctrlOptions, "Set angle");
            numAngle.SetRange2((int)swNumberboxUnitType_e.swNumberBox_Angle, 5.0 * Math.PI / 180.0, 85.0 * Math.PI / 180.0, false, 1 * Math.PI / 180.0, 5 * Math.PI / 180.0, 0.1 * Math.PI / 180.0);
            numAngle.Value = 45* Math.PI / 180.0;

            // --- Радиокнопки "Right" и "Left" ---
            radioRight = (PropertyManagerPageOption)pm_Group.AddControl2(RadioRightID,
                (int)swPropertyManagerPageControlType_e.swControlType_Option, "Right", leftAlign, ctrlOptions, "");

            radioLeft = (PropertyManagerPageOption)pm_Group.AddControl2(RadioLeftID,
                (int)swPropertyManagerPageControlType_e.swControlType_Option, "Left", leftAlign, ctrlOptions, "");

            // Сделать их взаимно исключающимися
            radioRight.Caption = "Rigth";
            radioLeft.Caption="Left";


            //Make sure that the page was created properly 
            if (longerrors == (int)swPropertyManagerPageStatus_e.swPropertyManagerPage_Okay)

            {

              

            }

            else

            {

                //If the page is not created 
                System.Windows.Forms.MessageBox.Show("An error occurred while attempting to create the PropertyManager page.");

            }

        }

        SldWorks swApp;
        ModelDoc2 model;

        public void ShowPreview()
        {
     
        }

        Body2 CreateCutBody()
        {
            Modeler modeler = (Modeler)swApp.GetModeler();

            double[] p = { 0, 0, 0 };
            double[] v1 = { 1, 0, 0 };
            double[] v2 = { 0, 1, 0 };

            Surface surf =
                (Surface)modeler.CreatePlanarSurface2(p, v1, v2);

            double[] uv = { -0.05, 0.05, -0.05, 0.05 };

            Body2 sheet =
                (Body2)modeler.CreateSheetFromSurface(surf, uv);

            return sheet;
        }


        #region IPropertyManagerPage2Handler9 Members  
        public void AfterActivation()
        {
            IsOpen = true;
        }

        public void AfterClose()
        {
            IsOpen = false;
        }

        public int OnActiveXControlCreated(int Id, bool Status)
        {
            throw new NotImplementedException();
        }

        public void OnButtonPress(int Id)
        {
            if (Id == 4)
            {
                labelRotation.Value = labelRotation.Value + 90 * Math.PI / 180.0;
                if (labelRotation.Value > 270 * Math.PI / 180.0) labelRotation.Value = 0;
            }
        }

        public void OnCheckboxCheck(int Id, bool Checked)
        {
            throw new NotImplementedException();
        }

        public void OnClose(int reason)
        {
            System.Diagnostics.Debug.Print("Close reason: " + reason);

            if (reason == (int)swPropertyManagerPageCloseReasons_e.swPropertyManagerPageClose_Okay)
            {
                action.Invoke();
            }
        }

        public void OnComboboxEditChanged(int Id, string Text)
        {
            throw new NotImplementedException();
        }

        public void OnComboboxSelectionChanged(int Id, int Item)
        {
            throw new NotImplementedException();
        }

        public void OnGainedFocus(int Id)
        {
            //throw new NotImplementedException();
        }

        public void OnGroupCheck(int Id, bool Checked)
        {
            throw new NotImplementedException();
        }

        public void OnGroupExpand(int Id, bool Expanded)
        {
            
        }

        public bool OnHelp()
        {
            throw new NotImplementedException();
        }

        public bool OnKeystroke(int Wparam, int Message, int Lparam, int Id)
        {
            throw new NotImplementedException();
        }

        public void OnListboxRMBUp(int Id, int PosX, int PosY)
        {
            throw new NotImplementedException();
        }

        public void OnListboxSelectionChanged(int Id, int Item)
        {
            throw new NotImplementedException();
        }

        public void OnLostFocus(int Id)
        {
            //throw new NotImplementedException();
        }

        public bool OnNextPage()
        {
            throw new NotImplementedException();
        }

        public void OnNumberboxChanged(int Id, double Value)
        {
          
        }

        public void OnNumberBoxTrackingCompleted(int Id, double Value)
        {
            //throw new NotImplementedException();
        }

        public void OnOptionCheck(int Id)
        {
            throw new NotImplementedException();
        }

        public void OnPopupMenuItem(int Id)
        {
            throw new NotImplementedException();
        }

        public void OnPopupMenuItemUpdate(int Id, ref int retval)
        {
            throw new NotImplementedException();
        }

        public bool OnPreview()
        {
            Body2 body = CreateCutBody();

            body.Display3(
                model,
                (int)swTempBodySelectOptions_e.swTempBodySelectable,
               -1);

            return true;
        }

        public bool OnPreviousPage()
        {
            throw new NotImplementedException();
        }

        public void OnRedo()
        {
            throw new NotImplementedException();
        }

        public void OnSelectionboxCalloutCreated(int Id)
        {
            throw new NotImplementedException();
        }

        public void OnSelectionboxCalloutDestroyed(int Id)
        {
            throw new NotImplementedException();
        }

        public void OnSelectionboxFocusChanged(int Id)
        {
            throw new NotImplementedException();
        }

        public void OnSelectionboxListChanged(int Id, int Count)
        {
            throw new NotImplementedException();
        }

        public void OnSliderPositionChanged(int Id, double Value)
        {
            throw new NotImplementedException();
        }

        public void OnSliderTrackingCompleted(int Id, double Value)
        {
            throw new NotImplementedException();
        }

        public bool OnSubmitSelection(int Id, object Selection, int SelType, ref string ItemText)
        {
            throw new NotImplementedException();
        }

        public bool OnTabClicked(int Id)
        {
            throw new NotImplementedException();
        }

        public void OnTextboxChanged(int Id, string Text)
        {
            throw new NotImplementedException();
        }

        public void OnUndo()
        {
            throw new NotImplementedException();
        }

        public void OnWhatsNew()
        {
            throw new NotImplementedException();
        }

        public int OnWindowFromHandleControlCreated(int Id, bool Status)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
