using ESRI.ArcGIS.Geodatabase;
using RoadNetworkSystem.ADO.Access;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.RoadSign;
using RoadNetworkSystem.DataModel.SignRule;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LaneLayer;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LinkLayer;
using RoadNetworkSystem.NetworkElement.RoadSignElement;
using RoadNetworkSystem.TrafficRule;
using RoadNetworkSystem.TrafficRule.SignRule;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RoadNetworkSystem.WinForm.RuleSetting
{
    class LaternConnection
    {
        public const string groupBox_Lane_Rule_Modify_Text = "连通状态";

        private Form1 _frm1;
        private static OleDbConnection _conn;
        public LaternConnection(Form1 frm1)
        {

            _frm1 = frm1;
            if (_conn == null)
            {
                _conn = _frm1.Conn;
            }
        }

        private void clearModifyGroup()
        {
            if (_frm1.groupBox_Lane_Rule_Modify != null)
            {
                _frm1.groupBox_Lane_Rule_Modify.Controls.Clear();
            }
        }


        private void clearPanel2()
        {
            if (_frm1.spltCtn_Rule_Setting_Att.Panel2 != null)
            {
                _frm1.spltCtn_Rule_Setting_Att.Panel2.Controls.Clear();
            }
        }


        /// <summary>
        /// 设置横向连通
        /// </summary>
        public void LayoutLaternConnection()
        {
            clearModifyGroup();
            clearPanel2();
            updatePublicControls();
            layoutLaternConnectionControls();
            initConnectionCheckBoxState();
        }

        /// <summary>
        /// 更新公共控件
        /// </summary>
        void updatePublicControls()
        {
            _frm1.groupBox_Lane_Rule_Modify.Text = groupBox_Lane_Rule_Modify_Text;
            _frm1.spltCtn_Rule_Setting_Att.SplitterDistance = 110;
        }

        /// <summary>
        /// 布局横向连通的控件
        /// </summary>
        void layoutLaternConnectionControls()
        {
            int location_x = 10;
            int location_Left_Y = 20;
            _frm1.label_Lane_Rule_Left_Connnection = new Label();
            WinFormDesigner.layoutLabel(_frm1.label_Lane_Rule_Left_Connnection, "左边界线", _frm1.groupBox_Lane_Rule_Modify.Controls,
                new Point(location_x, location_Left_Y), DockStyle.None);

            int location_CheckBox_X = location_x + _frm1.label_Lane_Rule_Left_Connnection.Width + RuleSettingDesigner.ROW_WIDTH;
            _frm1.checkBox_Lane_Rule_Left_Connnection = new CheckBox();
            WinFormDesigner.layoutCheckBox(_frm1.checkBox_Lane_Rule_Left_Connnection, "连通", _frm1.groupBox_Lane_Rule_Modify.Controls,
                new Point(location_CheckBox_X, location_Left_Y - 5), DockStyle.None);


            int location_Right_Y = location_Left_Y + _frm1.label_Lane_Rule_Left_Connnection.Height + RuleSettingDesigner.LINE_WIDTH;
            _frm1.label_Lane_Rule_Right_Connnection = new Label();
            WinFormDesigner.layoutLabel(_frm1.label_Lane_Rule_Right_Connnection, "右边界线", _frm1.groupBox_Lane_Rule_Modify.Controls,
                new Point(location_x, location_Right_Y), DockStyle.None);

            _frm1.checkBox_Lane_Rule_Right_Connnection = new CheckBox();
            WinFormDesigner.layoutCheckBox(_frm1.checkBox_Lane_Rule_Right_Connnection, "连通", _frm1.groupBox_Lane_Rule_Modify.Controls,
                new Point(location_CheckBox_X, location_Right_Y - 5), DockStyle.None);


            int location_Button_Y = _frm1.label_Lane_Rule_Right_Connnection.Location.Y + _frm1.label_Lane_Rule_Right_Connnection.Height + RuleSettingDesigner.LINE_WIDTH;
            _frm1.button_Lane_Rule_Modify = new Button();
            WinFormDesigner.layoutButton(_frm1.button_Lane_Rule_Modify, "修改", _frm1.groupBox_Lane_Rule_Modify.Controls,
                new Point(location_x, location_Button_Y), DockStyle.None,
                RuleSettingDesigner.BTNHEIGHT, RuleSettingDesigner.BTNHEIGHT * 2);

            _frm1.button_Lane_Rule_Modify.Click += button_Lane_Rule_Modify_Click;

        }


        /// <summary>
        /// 初始化横向连通控件（checkbox）的被选中的值
        /// </summary>
        void initConnectionCheckBoxState()
        {
            Lane laneEty = getLaneEty();
            if (laneEty == null)
                return;

            //设置连通状态
            if (laneEty.Change.Equals(Enum.GetName(typeof(LaneFeatureService.LaneChange), LaneFeatureService.LaneChange.Both))
                || laneEty.Change.Equals(Enum.GetName(typeof(LaneFeatureService.LaneChange), LaneFeatureService.LaneChange.Left)))
            {
                _frm1.checkBox_Lane_Rule_Left_Connnection.Checked = true;
            }
            else
            {
                _frm1.checkBox_Lane_Rule_Left_Connnection.Checked = false;
            }

            if (laneEty.Change.Equals(Enum.GetName(typeof(LaneFeatureService.LaneChange), LaneFeatureService.LaneChange.Both))
                || laneEty.Change.Equals(Enum.GetName(typeof(LaneFeatureService.LaneChange), LaneFeatureService.LaneChange.Right)))
            {
                _frm1.checkBox_Lane_Rule_Right_Connnection.Checked = true;
            }
            else
            {
                _frm1.checkBox_Lane_Rule_Right_Connnection.Checked = false;
            }
        }

        private Lane getLaneEty()
        {
            if (_frm1.SlctLane_Rule == null)
            {
                return null;
            }
            LaneFeatureService laneFeature = new LaneFeatureService(_frm1.FeaClsLane, 0);
            return laneFeature.GetEntity(_frm1.SlctLane_Rule);
        }

        void button_Lane_Rule_Modify_Click(object sender, EventArgs e)
        {
            bool leftConnectionFlag = _frm1.checkBox_Lane_Rule_Left_Connnection.Checked;
            bool rightConnectionFlag = _frm1.checkBox_Lane_Rule_Right_Connnection.Checked;


            RuleService ruleService = new RuleService(_conn);
            Rule leftRule = new Rule();
            if (leftConnectionFlag == true)
            {
                leftRule = ruleService.getRruleByIsAccessible(true);
            }
            else
            {
                leftRule = ruleService.getRruleByIsForbidden(true);
            }

            Rule rightRule = new Rule();
            if (rightConnectionFlag == true)
            {
                rightRule = ruleService.getRruleByIsAccessible(true);
            }
            else
            {
                rightRule = ruleService.getRruleByIsForbidden(true);
            }

            //左侧边界线的
            bool isCenterLine_LeftBoundary = false;
            bool isSideLine_LeftBoundary = false;

            bool isSideLine_RightBoundary = false;
            bool isCenterLine_RightBoundary = false;
            Lane laneEty = getLaneEty();

            ArcService arc = new ArcService(_frm1.FeaClsArc, laneEty.ArcID);

            Arc arcEty = arc.GetArcEty(arc.GetArcFeature());
            if (laneEty.Position == 1)
            {
                isCenterLine_LeftBoundary = true;
                if (laneEty.Position == arcEty.LaneNum)
                {
                    isSideLine_RightBoundary = true;
                }
            }

            if (laneEty.Position == arcEty.LaneNum)
            {
                isSideLine_RightBoundary = true;
                if (laneEty.Position == 1)
                {
                    isCenterLine_LeftBoundary = true;
                }
            }

            BoundaryRuleService boundaryRuleService = new BoundaryRuleService(_conn);

            BoundaryService boundary_Left = new BoundaryService(_frm1.FeaClsBoundary, laneEty.LeftBoundaryID);
            BoundaryService boundary_Right = new BoundaryService(_frm1.FeaClsBoundary, laneEty.RightBoundaryID);

            IFeature boundary_Left_Feature = boundary_Left.GetFeature();
            Boundary boundaryEty_Left_Boundary = boundary_Left.GetEntity(boundary_Left_Feature);

            IFeature boundary_Right_Feature = boundary_Right.GetFeature();
            Boundary boundaryEty_Right_Boundary = boundary_Right.GetEntity(boundary_Right_Feature);


            //当前车道的右侧边界线的规则
            BoundaryRule currentBoundaryRule_Right_Boundary = new BoundaryRule();
            currentBoundaryRule_Right_Boundary = boundaryRuleService.getRrule(boundaryEty_Right_Boundary.StyleID);


            //当前车道的左侧边界线的规则
            BoundaryRule currentBoundaryRule_Left_Boundary = new BoundaryRule();
            currentBoundaryRule_Left_Boundary = boundaryRuleService.getRrule(boundaryEty_Left_Boundary.StyleID);

            //合并左侧车道的向右变线规则，和当前车道的向左变向规则
            BoundaryRule newBoundaryRule_Left_Boundary = new BoundaryRule();
            newBoundaryRule_Left_Boundary.IsSideLine = currentBoundaryRule_Left_Boundary.IsSideLine;
            newBoundaryRule_Left_Boundary.IsCenterLine = currentBoundaryRule_Left_Boundary.IsCenterLine;
            newBoundaryRule_Left_Boundary.LeftRuleID = currentBoundaryRule_Left_Boundary.LeftRuleID;
            newBoundaryRule_Left_Boundary.RightRuleID = leftRule.RuleID;

            int leftBoundaryStyleId = boundaryRuleService.getBoundaryStyleId(newBoundaryRule_Left_Boundary);

            //更新左侧边界线
            if (leftBoundaryStyleId != boundaryEty_Left_Boundary.StyleID)
            {
                boundaryEty_Left_Boundary.StyleID = leftBoundaryStyleId;
                boundary_Left.ModifyBoundaryAtt(boundary_Left_Feature, boundaryEty_Left_Boundary);
            }

            //合并右侧车道的向左变线规则，和当前车道的向左变向规则
            BoundaryRule newBoundaryRule_Right_Boundary = new BoundaryRule();
            newBoundaryRule_Right_Boundary.IsSideLine = currentBoundaryRule_Right_Boundary.IsSideLine;
            newBoundaryRule_Right_Boundary.IsCenterLine = currentBoundaryRule_Right_Boundary.IsCenterLine;
            newBoundaryRule_Right_Boundary.LeftRuleID = rightRule.RuleID;
            newBoundaryRule_Right_Boundary.RightRuleID = currentBoundaryRule_Right_Boundary.RightRuleID;
            int rightBoundaryStyleId = boundaryRuleService.getBoundaryStyleId(newBoundaryRule_Right_Boundary);

            //更新右侧侧边界线
            if (rightBoundaryStyleId != boundaryEty_Right_Boundary.StyleID)
            {
                boundaryEty_Right_Boundary.StyleID = rightBoundaryStyleId;
                boundary_Right.ModifyBoundaryAtt(boundary_Right_Feature, boundaryEty_Right_Boundary);
            }
        }
    }
}
