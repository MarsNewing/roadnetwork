using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using RoadNetworkSystem.ADO.Access;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.RoadSign;
using RoadNetworkSystem.DataModel.SignRule;
using RoadNetworkSystem.GIS;
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
    class RuleSettingDesigner
    {
        private Form1 _frm1;
        OleDbConnection connection;
        public static int BTNHEIGHT = 30;
        public const int LINE_WIDTH = 15;
        public const int ROW_WIDTH = 30;
        public RuleSettingDesigner(Form1 form1) 
        {
            _frm1 = form1;
            connection = AccessHelper.OpenConnection(form1.MdbPath);
        }

        public enum RuleRoadItem
        {
            有向子路段,
            车道
        }


        public enum LaneRule
        {
            通行,
            横向连通,
            转向,
            限高速,
            限低速,
            限高,
            限宽,
            车辆类型
        }


        private void clearParentCtrl()
        {
            WinFormDesigner.ClearPanel(_frm1.panel_Middle);
            WinFormDesigner.ClearPanel(_frm1.panel_Bottom);
            WinFormDesigner.ClearPanel(_frm1.panel_Top);
            //_frm1.groupBox1.Visible = false;
            _frm1.panel_Top.Visible = true;

            _frm1.panel_Top.Visible = true;
            _frm1.panel_Top.AutoScroll = true;
        }

        private void clearTopAndMiddlePanel()
        {
            WinFormDesigner.ClearPanel(_frm1.panel_Middle);
            WinFormDesigner.ClearPanel(_frm1.panel_Top);
            _frm1.panel_Top.Visible = true;
            _frm1.panel_Top.Visible = true;
            _frm1.panel_Top.AutoScroll = true;
        }

        public void SetRuleSettingPlatte()
        {
            clearParentCtrl();
            initPanel();
            _frm1.Resize+=_frm1_Resize;
        }

        void comboBox_Layer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_frm1.comboBox_Layer.SelectedText.Equals(Arc.ArcFeatureName))
            {
                _frm1.button_Add.Tag = 1;
                _frm1.button_Refresh.Tag = 1;
                setArcAtt();
            }
            else
            {
                _frm1.button_Add.Tag = 2;
                _frm1.button_Refresh.Tag = 2;
                setLaneAtt();
            }
        }

        private void initPanel()
        {
            _frm1.groupBox_Arc_Rule_Att = new System.Windows.Forms.GroupBox();
            _frm1.groupBox_Arc_Rule_Att.Visible = true;

            setLaneAtt();
            setBottomPanel();
            setLayerList();
            _frm1.comboBox_Layer.SelectedIndex = 1;
        }

        private void splitMiddlePanel(GroupBox groupBox)
        {
            _frm1.spltCtn_Rule_Selectiont_Att = new SplitContainer();
            _frm1.spltCtn_Rule_Selectiont_Att.Orientation = Orientation.Horizontal;
            _frm1.spltCtn_Rule_Selectiont_Att.SplitterDistance = 40;
            _frm1.spltCtn_Rule_Selectiont_Att.SplitterWidth = 2;
           
            _frm1.spltCtn_Rule_Selectiont_Att.Dock = DockStyle.Fill;

            groupBox.Controls.Add(_frm1.spltCtn_Rule_Selectiont_Att);
        }

        private void clearModifyGroup() 
        {
            if (_frm1.groupBox_Lane_Rule_Modify != null)
            {
                _frm1.groupBox_Lane_Rule_Modify.Controls.Clear();
            }
        }

        #region ------------------- Arc属性设置 -------------------------
        private void setArcAtt() 
        {
            clearTopAndMiddlePanel();

            

            _frm1.groupBox_Arc_Rule_Att = new System.Windows.Forms.GroupBox();
            _frm1.groupBox_Arc_Rule_Att.Visible = true;

            WinFormDesigner.setGroupBoxStyle(_frm1.groupBox_Arc_Rule_Att, "有向子路段属性", new System.Drawing.Point(5, 5),
                System.Windows.Forms.DockStyle.Fill, 100, 0);
            _frm1.panel_Top.Controls.Add(_frm1.groupBox_Arc_Rule_Att);

            _frm1.groupBox_Arc_Rule_Setting = new System.Windows.Forms.GroupBox();
            _frm1.groupBox_Arc_Rule_Setting.Visible = true;

            WinFormDesigner.setGroupBoxStyle(_frm1.groupBox_Arc_Rule_Setting, "规则设定", new System.Drawing.Point(5, 5),
                System.Windows.Forms.DockStyle.Fill, 100, 0);
            _frm1.panel_Middle.Controls.Add(_frm1.groupBox_Arc_Rule_Setting);

            splitMiddlePanel(_frm1.groupBox_Arc_Rule_Setting);
        }

        #endregion ------------------- Arc属性设置 -------------------------

        #region ------------------- Lane属性设置 -------------------------
        private void setLaneAtt()
        {

            clearTopAndMiddlePanel();

            _frm1.groupBox_Lane_Rule_Att = new System.Windows.Forms.GroupBox();
            _frm1.groupBox_Lane_Rule_Att.Visible = true;

            WinFormDesigner.setGroupBoxStyle(_frm1.groupBox_Lane_Rule_Att, "车道属性", new System.Drawing.Point(5, 5),
                System.Windows.Forms.DockStyle.Fill, 100, 0);
            _frm1.panel_Top.Controls.Add(_frm1.groupBox_Lane_Rule_Att);

            _frm1.groupBox_Lane_Rule_Setting = new System.Windows.Forms.GroupBox();
            _frm1.groupBox_Lane_Rule_Setting.Visible = true;

            WinFormDesigner.setGroupBoxStyle(_frm1.groupBox_Lane_Rule_Setting, "规则设定", new System.Drawing.Point(5, 5),
                System.Windows.Forms.DockStyle.Fill, 100, 0);
            _frm1.panel_Middle.Controls.Add(_frm1.groupBox_Lane_Rule_Setting);

            //把规则设定groupbox一分为二
            splitMiddlePanel(_frm1.groupBox_Lane_Rule_Setting);


            //设置选择规则Group
            _frm1.groupBox_Lane_Rule_Selection = new GroupBox();
            int location_X = 10;
            WinFormDesigner.setGroupBoxStyle(_frm1.groupBox_Lane_Rule_Selection,"选择规则",new Point(location_X, 10), DockStyle.Fill,100,100);
            _frm1.spltCtn_Rule_Selectiont_Att.Panel1.Controls.Add(_frm1.groupBox_Lane_Rule_Selection);

            int location_comboBox_Y = 20;
            _frm1.comboBox_Lane_Rule_Selection = new ComboBox();
            _frm1.comboBox_Lane_Rule_Selection.Location = new Point(location_X,location_comboBox_Y);
            _frm1.groupBox_Lane_Rule_Selection.Controls.Add(_frm1.comboBox_Lane_Rule_Selection);
            setLaneRuleList();
            _frm1.comboBox_Lane_Rule_Selection.SelectedIndex = Convert.ToInt32(LaneRule.横向连通);
            _frm1.comboBox_Lane_Rule_Selection.SelectedIndexChanged += comboBox_Lane_Rule_Selection_SelectedIndexChanged;

            //设置修改group
            _frm1.groupBox_Lane_Rule_Modify = new GroupBox();
            WinFormDesigner.setGroupBoxStyle(_frm1.groupBox_Lane_Rule_Modify, "修改", new Point(location_X, 10), DockStyle.Fill, 100, 100);
            _frm1.spltCtn_Rule_Selectiont_Att.Panel2.Controls.Add(_frm1.groupBox_Lane_Rule_Modify);

            //默认设置为横向连通关系修改
            LaternConnection laternConnection = new LaternConnection(_frm1);
            laternConnection.LayoutLaternConnection();
            


        }

        void comboBox_Lane_Rule_Selection_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (_frm1.comboBox_Lane_Rule_Selection.SelectedIndex)
            {
                case (int)LaneRule.通行:
                {
                    break;
                }
                case (int)LaneRule.横向连通:
                {

                    LaternConnection laternConnection = new LaternConnection(_frm1);
                    laternConnection.LayoutLaternConnection();
                    break;
                }
                case (int)LaneRule.转向:
                {
                    initTurnArrowRuleParameters();
                    TurningRule turningRule = new TurningRule(_frm1);
                    turningRule.LayoutTurnarrowRule();
                    break;
                }
                case (int)LaneRule.限高速:
                {
                    break;
                }
                case (int)LaneRule.限低速:
                {
                    break;
                }
                case (int)LaneRule.限高:
                {
                    break;
                }
                case (int)LaneRule.限宽:
                {
                    break;
                }
                case (int)LaneRule.车辆类型:
                {
                    break;
                }
                default:
                {
                    break;
                }
            }
        }


        private void initTurnArrowRuleParameters()
        {
            _frm1.SlctLane_Rule = null;
        }

        private void setLaneRuleList()
        {
            _frm1.comboBox_Lane_Rule_Selection.Items.AddRange(Enum.GetNames(typeof(LaneRule)));
        }

        #endregion  ------------------- Lane属性设置 -------------------------

        private void setBottomPanel()
        {
            _frm1.splitContainer5 = new SplitContainer();
            _frm1.splitContainer5.SplitterDistance = 125;
            _frm1.splitContainer5.Orientation = Orientation.Vertical;
            _frm1.splitContainer5.Dock = DockStyle.Fill;
            _frm1.panel_Bottom.Controls.Add(_frm1.splitContainer5);

            _frm1.button_Add = new Button();
            _frm1.button_Add.Image = Properties.Resources.plus_alt;
            _frm1.button_Add.Width = BTNHEIGHT;
            _frm1.button_Add.Dock = DockStyle.Right;
            _frm1.button_Add.Tag = 2;
            _frm1.button_Add.Click += button_Add_Click;

            _frm1.button_Refresh = new Button();
            _frm1.button_Refresh.Image = Properties.Resources.reload;
            _frm1.button_Refresh.Width = BTNHEIGHT;
            _frm1.button_Refresh.Dock = DockStyle.Right;
            _frm1.button_Refresh.Tag = 2;
            _frm1.button_Refresh.Click += button_Refresh_Click;

            _frm1.splitContainer5.Panel2.Controls.Add(_frm1.button_Add);
            _frm1.splitContainer5.Panel2.Controls.Add(_frm1.button_Refresh);

            

            _frm1.comboBox_Layer = new ComboBox();
            _frm1.comboBox_Layer.Dock = DockStyle.Fill;
            _frm1.comboBox_Layer.Font = new Font(_frm1.comboBox_Layer.Font.Name, _frm1.comboBox_Layer.Font.Size + 5);
            _frm1.splitContainer5.Panel1.Controls.Add(_frm1.comboBox_Layer);
            _frm1.comboBox_Layer.SelectedIndexChanged+=comboBox_Layer_SelectedIndexChanged;

            //保证按钮与图层选择器之间的位置
            _frm1.splitContainer5.SplitterDistance = _frm1.splitContainer5.Width - ((Int32)(2.5 * _frm1.button_Refresh.Width));
           
        }

        void button_Refresh_Click(object sender, EventArgs e)
        {
            if (Convert.ToInt32(_frm1.button_Refresh.Tag) == 2)
            {
                
                //刷新所有的被选中的东东
                IGraphicsContainer pGraphicsContainer = _frm1.axMapControl1.ActiveView as IGraphicsContainer;//把地图的当前view作为图片的容器
                pGraphicsContainer.DeleteAllElements();//显示储存在 IElement 中图形，这样就持久化了。
                _frm1.axMapControl1.Refresh();

                //initValue();
            }
        }

        private void initValue()
        {
            _frm1.SlctArc_Rule=  null;
            _frm1.SlctLane_Rule = null;
        }

        void button_Add_Click(object sender, EventArgs e)
        {
            
            switch (Convert.ToInt32(_frm1.button_Add.Tag))
            {
                case 1:
                    {
                        //选择的是Arc
                        _frm1.ToolBarFlag = false;
                        LayerHelper.ClearSelect(_frm1.axMapControl1);
                        LayerHelper.SelectLayer(_frm1.axMapControl1,Arc.ArcFeatureName);

                        break;
                    }
                case 2:
                    {
                        //选择的是Lane

                        _frm1.ToolBarFlag = false;
                        LayerHelper.ClearSelect(_frm1.axMapControl1);
                        LayerHelper.SelectLayer(_frm1.axMapControl1, Lane.LaneName);

                        break;
                    }
                default :
                    break;
            }

            IGraphicsContainer pGraphicsContainer = _frm1.axMapControl1.ActiveView as IGraphicsContainer;//把地图的当前view作为图片的容器
            pGraphicsContainer.DeleteAllElements();//显示储存在 IElement 中图形，这样就持久化了。
        }

        void _frm1_Resize(object sender, EventArgs e)
        {
            if (_frm1.splitContainer5 != null)
            {
                //保证按钮与图层选择器之间的位置
                _frm1.splitContainer5.SplitterDistance = _frm1.splitContainer5.Width - 2 * _frm1.button_Refresh.Width - 2;
            }
        }

        private void setLayerList()
        {
            List<string> itemList = new List<string>();
            itemList.Add(Arc.ArcFeatureName);
            itemList.Add(Lane.LaneName);
            foreach (string item in itemList)
            {
                _frm1.comboBox_Layer.Items.Add(item);
            }
            
        }

    }
}
