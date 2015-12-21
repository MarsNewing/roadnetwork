﻿using ESRI.ArcGIS.Geodatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadNetworkSystem.WinForm.RuleSetting
{
    class TurningRule
    {
        private Form1 _frm1;
        private static int location_x = 10;
        private static int location_Left_Y = 20;
            

        /*
         * 1：左转
         * 2：直行
         * 3：右转
         * 4：掉头
         * 
         */
        Dictionary<int, IFeature> turnArc;

        public TurningRule(Form1 frm1)
        {
            _frm1 = frm1;
        }

        private void clearModifyGroup()
        {
            if (_frm1.groupBox_Lane_Rule_Modify != null)
            {
                _frm1.groupBox_Lane_Rule_Modify.Controls.Clear();
            }
        }

        public void LayoutTurnarrowRule()
        {
            clearModifyGroup();

            initConnectionRule();
        }

        public void initConnectionRule()
        {
            _frm1.label_Next_Arcs = new System.Windows.Forms.Label();
            WinFormDesigner.layoutLabel(_frm1.label_Next_Arcs, "下游有向子路段", _frm1.groupBox_Lane_Rule_Modify.Controls,
                new System.Drawing.Point(location_x, location_Left_Y), System.Windows.Forms.DockStyle.None);


            int combox_Y = location_Left_Y + RuleSettingDesigner.LINE_WIDTH;
            _frm1.comBox_Next_Arcs = new System.Windows.Forms.ComboBox();
            WinFormDesigner.layoutComBox(_frm1.comBox_Next_Arcs, _frm1.groupBox_Lane_Rule_Modify.Controls,
                new System.Drawing.Point(location_x, combox_Y), new List<string>(), System.Windows.Forms.DockStyle.None);

            int location_Label_Lane_Serial_Y = combox_Y + 2 * RuleSettingDesigner.LINE_WIDTH;
            _frm1.label_Next_Lane_Serial = new System.Windows.Forms.Label();
            WinFormDesigner.layoutLabel(_frm1.label_Next_Lane_Serial, "连接车道", _frm1.groupBox_Lane_Rule_Modify.Controls,
                new System.Drawing.Point(location_x, location_Label_Lane_Serial_Y), System.Windows.Forms.DockStyle.None);
        }
    }
}
