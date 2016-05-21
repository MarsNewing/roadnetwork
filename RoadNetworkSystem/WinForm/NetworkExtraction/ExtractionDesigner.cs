using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SystemUI;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.Road;
using RoadNetworkSystem.GIS;
using RoadNetworkSystem.GIS.GeoDatabase.Dataset;
using RoadNetworkSystem.GIS.GeoDatabase.WorkSpace;
using RoadNetworkSystem.NetworkEditor;
using RoadNetworkSystem.NetworkElement.RoadLayer;
using RoadNetworkSystem.NetworkExtraction.GuideSignNetwork;
using RoadNetworkSystem.NetworkExtraction.LaneBasedNetwork.SegmentLayer;
using RoadNetworkSystem.NetworkExtraction.LinkMasterExtraction;
using RoadNetworkSystem.NetworkExtraction.Road2BasicRoadNetwork;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace RoadNetworkSystem.WinForm.NetworkExtraction
{
    public class ExtractionDesigner
    {
        private static Form1 _frm1;

        //左上角
        private const int LEFTX = 10;
        private const int LEFTY = 25;
        //行距
        private const int LINEWIDTH = 10;
        //右侧的第一个控件的X坐标
        private static int RIGHTX = 0;

        //
        private const int BTNWIDTH = 25;

        public enum CopyFeatureClassAndTable
        {
            CopyForGuideSignAndSegmentNetowrk,  //  仅仅为生成指路标志路网 或 路段级路网
            CopyForRoad2BasicNetwork            //  仅仅为生成车道级路网
        }

        public int CopyFlag = (int)CopyFeatureClassAndTable.CopyForGuideSignAndSegmentNetowrk;

        /// <summary>
        /// 保存打断规则
        /// </summary>
        private List<string> _ruleList = new List<string>();
        private string _ruleItem="";

        public ExtractionDesigner(Form1 frm1)
        {
            _frm1 = frm1;
        }

        /// <summary>
        /// 用于显现选择对象的颜色
        /// </summary>
        private const string BTN_GUIDESIGN = "提取指路标志路网";
        private const string BTN_SEGMNET = "提取Segment路网";
        private const string BTN_ROAD2BASIC = "提取车道级路网";

        public enum ExtractionType
        {
            指路标志路网,
            路段路网,
            车道级路网
        }

        private void clearParentCtrl()
        {
            WinFormDesigner.ClearPanel(_frm1.panel_Middle);
            WinFormDesigner.ClearPanel(_frm1.panel_Bottom);
            WinFormDesigner.ClearPanel(_frm1.panel_Top);
            //g_frm1.groupBox1.Visible = false;
            _frm1.panel_Top.Visible = true;

            _frm1.panel_Top.Visible = true;
            _frm1.panel_Top.AutoScroll = true;

        }




        /// <summary>
        /// 设置路网提取操作面板
        /// </summary>
        public void SetExtractionPlatte()
        {
            //清空板面
            clearParentCtrl();
            //设置顶端板面设置内容：判断选取的是指路标志路网还是车道级路网
            layoutFunction();

            #region 中间板块

            _frm1.groupBox_extraction_rule = new System.Windows.Forms.GroupBox();
            WinFormDesigner.setGroupBoxStyle(_frm1.groupBox_extraction_rule, "禁止打断规则", new System.Drawing.Point(0, 0), 
                System.Windows.Forms.DockStyle.Top, 200, 0);

           
            _frm1.groupBox_extraction_sclRoad = new System.Windows.Forms.GroupBox();
            WinFormDesigner.setGroupBoxStyle(_frm1.groupBox_extraction_sclRoad, "指定禁止打断交叉口", new System.Drawing.Point(0, 0), 
                System.Windows.Forms.DockStyle.Top, 200, 500);

            //在中间底部加入禁止打断的Road对
            _frm1.panel_Middle.Controls.Add(_frm1.groupBox_extraction_sclRoad);

            //在中间顶端加入禁止打断的设定规则
            _frm1.panel_Middle.Controls.Add(_frm1.groupBox_extraction_rule);

            layoutRule();
            layoutSeleFea();
            #endregion 中间板块

            #region 底端路网提取按钮

            _frm1.groupBox_extraction_operation = new System.Windows.Forms.GroupBox();
            _frm1.groupBox_extraction_operation.Visible = true;
            WinFormDesigner.setGroupBoxStyle(_frm1.groupBox_extraction_operation, "路网提取", new System.Drawing.Point(0, 0),
                System.Windows.Forms.DockStyle.Fill, 60, 60);
            _frm1.panel_Bottom.Controls.Add(_frm1.groupBox_extraction_operation);

            layoutOperation();
            _frm1.panel_Bottom.Height = _frm1.button_extration_extract.Location.Y + _frm1.button_extration_extract.Height + LINEWIDTH;

            #endregion 底端路网提取按钮
        }

        #region ------------------------------设置提取路网的类型------------------------------------------
        //提取功能设计,包含指路标志路网和车道级路网
        private void layoutFunction()
        {
            _frm1.groupBox_extraction_function = new System.Windows.Forms.GroupBox();
            _frm1.groupBox_extraction_function.Visible = true;
            WinFormDesigner.setGroupBoxStyle(_frm1.groupBox_extraction_function, "提取路网类型", new System.Drawing.Point(0, 0), System.Windows.Forms.DockStyle.Fill, 50, 0);
            _frm1.panel_Top.Controls.Add(_frm1.groupBox_extraction_function);

            //设置Label
            _frm1.label_extraction_function = new System.Windows.Forms.Label();
            _frm1.label_extraction_function.Width = 120;
            WinFormDesigner.layoutLabel(_frm1.label_extraction_function, "请选择提取的路网类型", _frm1.groupBox_extraction_function.Controls, new System.Drawing.Point(LEFTX, LEFTY), System.Windows.Forms.DockStyle.None);



            //设置提取的路网类型
            _frm1.comBox_extraction_function = new System.Windows.Forms.ComboBox();
            //g_frm1.comBox_extraction_function.Items.AddRange(Enum.GetNames(typeof(ExtractionType)));
            List<string> items = new List<string>();
            items.AddRange(Enum.GetNames(typeof(ExtractionType)));
            //object o = (object)0;
            //items.Add(Enum.GetName(typeof(ExtractionType), o));
            //o = 1;
            //items.Add(Enum.GetName(typeof(ExtractionType), o));
            int comBox_y = _frm1.label_extraction_function.Location.Y + _frm1.label_extraction_function.Height + LINEWIDTH;
            WinFormDesigner.layoutComBox(_frm1.comBox_extraction_function, _frm1.groupBox_extraction_function.Controls,
                new System.Drawing.Point(LEFTX, comBox_y), items, System.Windows.Forms.DockStyle.None);

            _frm1.panel_Top.Height = _frm1.comBox_extraction_function.Location.Y + _frm1.comBox_extraction_function.Height + LINEWIDTH;

            _frm1.comBox_extraction_function.SelectedIndexChanged += comBox_extraction_function_SelectedIndexChanged;

        }


        void comBox_extraction_function_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (_frm1.comBox_extraction_function.SelectedIndex)
            {
                case (int)ExtractionType.指路标志路网:
                    {
                        _frm1.button_extration_extract.Text = BTN_GUIDESIGN;
                        CopyFlag = (int)ExtractionDesigner.CopyFeatureClassAndTable.CopyForGuideSignAndSegmentNetowrk;
                        testGuideSign();
                        break;
                    }
                case (int)ExtractionType.路段路网:
                    {
                        _frm1.button_extration_extract.Text = BTN_SEGMNET;
                        CopyFlag = (int)ExtractionDesigner.CopyFeatureClassAndTable.CopyForGuideSignAndSegmentNetowrk;
                        testLaneBased();
                        break;
                    }
                case (int)ExtractionType.车道级路网:
                    {
                        _frm1.button_extration_extract.Text = BTN_ROAD2BASIC;
                        CopyFlag = (int)ExtractionDesigner.CopyFeatureClassAndTable.CopyForRoad2BasicNetwork;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        #endregion ------------------------------设置提取路网的类型------------------------------------------

        #region ----------------------选择Road图层-------------------------------

        /// <summary>
        /// 数据管理
        /// </summary>
        //private void layoutData()
        //{
        //    g_frm1.button_extration_road = new System.Windows.Forms.Button();
            
        //    WinFormDesigner.layoutButton(g_frm1.button_extration_road, "选择Road图层", g_frm1.groupBox_extraction_data.Controls, new System.Drawing.Point(LEFTX, LEFTY),
        //        System.Windows.Forms.DockStyle.None, BTNWIDTH, 100);
        //    g_frm1.button_extration_road.Click += button_extration_road_Click;
        //}


        //void button_extration_road_Click(object sender, EventArgs e)
        //{
        //    //清空地图
        //    g_frm1.axMapControl1.ClearLayers();
        //    ICommand pCommand = new ControlsAddDataCommandClass();
        //    pCommand.OnCreate(g_frm1.axMapControl1.Object);
        //    pCommand.OnClick();
        //    IFeatureLayer roadFeaLayer = g_frm1.axMapControl1.Map.Layer[0] as IFeatureLayer;
        //    g_frm1.FeaClsRoad = roadFeaLayer.FeatureClass;
        //}
        #endregion ----------------------选择Road图层-------------------------------

        #region --------------------------禁止打断规则的设定-------------------------

        /// <summary>
        /// 设计禁止打断的规则界面
        /// </summary>
        private void layoutRule()
        {
            #region  +++++++++++++++++++++++++++++++++++++++待选择的规则++++++++++++++++++++++++++++++++++++++++++++
            #region -----------------------------相交第一个路段规则------------------------------------------
            _frm1.label_extraction_type1 = new System.Windows.Forms.Label();
            WinFormDesigner.layoutLabel(_frm1.label_extraction_type1, "第一种道路类型", _frm1.groupBox_extraction_rule.Controls,
                new System.Drawing.Point(LEFTX, LEFTY), System.Windows.Forms.DockStyle.None);

            _frm1.listBox_extraction_type1 = new System.Windows.Forms.ListBox();

            int listbox_type1_x = _frm1.label_extraction_type1.Location.X;
            int listbox_type1_y = _frm1.label_extraction_type1.Location.Y + _frm1.label_extraction_type1.Height + LINEWIDTH;
            WinFormDesigner.layoutListItem(_frm1.listBox_extraction_type1, _frm1.groupBox_extraction_rule.Controls,
                new System.Drawing.Point(listbox_type1_x, listbox_type1_y), System.Windows.Forms.DockStyle.None);


            _frm1.listBox_extraction_type1.Items.AddRange(System.Enum.GetNames(typeof(Link.道路类型)));

            #endregion -----------------------------相交第一个路段规则------------------------------------------
            #region -----------------------------相交第二个路段规则------------------------------------------
            _frm1.label_extraction_type2 = new System.Windows.Forms.Label();
            int label_type2_x = _frm1.groupBox_extraction_rule.Width / 2 + 10;
            WinFormDesigner.layoutLabel(_frm1.label_extraction_type2, "第二种道路类型", _frm1.groupBox_extraction_rule.Controls,
                new System.Drawing.Point(label_type2_x, LEFTY), System.Windows.Forms.DockStyle.None);

            _frm1.listBox_extraction_type2 = new System.Windows.Forms.ListBox();
            WinFormDesigner.layoutListItem(_frm1.listBox_extraction_type2, _frm1.groupBox_extraction_rule.Controls,
                new System.Drawing.Point(label_type2_x, listbox_type1_y), System.Windows.Forms.DockStyle.None);

            _frm1.listBox_extraction_type2.Width = _frm1.groupBox_extraction_rule.Width / 2 - 30;
            _frm1.listBox_extraction_type2.Items.AddRange(System.Enum.GetNames(typeof(Link.道路类型)));



            _frm1.listBox_extraction_type2.Click += listBox_extraction_type2_Click;
            _frm1.listBox_extraction_type1.Click += listBox_extraction_type1_Click;
            #endregion -----------------------------相交第二个路段规则------------------------------------------
            #endregion  +++++++++++++++++++++++++++++++++++++++待选择的规则++++++++++++++++++++++++++++++++++++++++++++


            #region  +++++++++++++++++++++++++++++++++++++++已选择的规则++++++++++++++++++++++++++++++++++++++++++++
            #region -----------------------------已选相交第一个路段规则------------------------------------------
            _frm1.label_extraction_rule1 = new Label();
            int label_rule1_x = LEFTX;
            int label_rule1_y = _frm1.listBox_extraction_type1.Location.Y + _frm1.listBox_extraction_type1.Height + 2 * LINEWIDTH;

            WinFormDesigner.layoutLabel(_frm1.label_extraction_rule1, "已选择的规则1", _frm1.groupBox_extraction_rule.Controls,
                new System.Drawing.Point(label_rule1_x, label_rule1_y), System.Windows.Forms.DockStyle.None);


            _frm1.listBox_extraction_rlue1 = new ListBox();
            int listBox_rule1_x = LEFTX;
            int listBox_rule1_y = label_rule1_y + _frm1.label_extraction_rule1.Height + LINEWIDTH;

            WinFormDesigner.layoutListItem(_frm1.listBox_extraction_rlue1, _frm1.groupBox_extraction_rule.Controls,
                new System.Drawing.Point(listBox_rule1_x, listBox_rule1_y), DockStyle.None);
            #endregion -----------------------------已选相交第一个路段规则------------------------------------------

            #region -----------------------------已选相交第一个路段规则------------------------------------------
            _frm1.label_extraction_rule2 = new Label();
            int label_rule2_x = _frm1.label_extraction_type2.Location.X;
            int label_rule2_y = _frm1.label_extraction_rule1.Location.Y;

            WinFormDesigner.layoutLabel(_frm1.label_extraction_rule2, "已选择的规则2", _frm1.groupBox_extraction_rule.Controls,
                new System.Drawing.Point(label_rule2_x, label_rule2_y), System.Windows.Forms.DockStyle.None);


            _frm1.listBox_extraction_rule2 = new ListBox();
            int listBox_rule2_x = _frm1.listBox_extraction_type2.Location.X;


            WinFormDesigner.layoutListItem(_frm1.listBox_extraction_rule2, _frm1.groupBox_extraction_rule.Controls,
                new System.Drawing.Point(listBox_rule2_x, listBox_rule1_y), DockStyle.None);
            #endregion -----------------------------已选相交第一个路段规则------------------------------------------

            #endregion  +++++++++++++++++++++++++++++++++++++++已选择的规则++++++++++++++++++++++++++++++++++++++++++++

            _frm1.listBox_extraction_type1.Width = _frm1.panel_Top.Width / 2 - 30;

            _frm1.listBox_extraction_rlue1.Width = _frm1.panel_Top.Width / 2 - 30;
            _frm1.listBox_extraction_rule2.Width = _frm1.listBox_extraction_type2.Width;

            //重置高度
            _frm1.groupBox_extraction_rule.Height = _frm1.listBox_extraction_rlue1.Height + LINEWIDTH + listBox_rule1_y;
        }




        void listBox_extraction_type1_Click(object sender, EventArgs e)
        {
            _ruleItem = _frm1.listBox_extraction_type1.SelectedIndex.ToString();
            //add items to rule1
            if (_frm1.listBox_extraction_rlue1.Items.Count == _frm1.listBox_extraction_rule2.Items.Count)
            {
                _frm1.listBox_extraction_rlue1.Items.Add(_frm1.listBox_extraction_type1.SelectedItem);
            }
            else
            {
                if ((_frm1.listBox_extraction_rlue1.Items.Count - _frm1.listBox_extraction_rule2.Items.Count) == 1)
                {
                    MessageBox.Show("请选择第二个规则");
                }
            }

            //设置选择对象的颜色
            _frm1.listBox_extraction_rlue1.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            _frm1.listBox_extraction_rlue1.DrawItem += listBox_extraction_rlue1_DrawItem;
        }

        /// <summary>
        /// 选择项的重绘
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void listBox_extraction_rlue1_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            Brush myBrush = Brushes.Black; //初始化字体颜色=黑色
            //为项设置字体颜色
            if (e.Index % 2 == 0)
            {
                e.Graphics.FillRectangle(Brushes.Yellow, e.Bounds);
                myBrush = Brushes.Black;
            }
            else
            {
                e.Graphics.FillRectangle(Brushes.SkyBlue, e.Bounds);
                myBrush = Brushes.Black;
            }


            e.Graphics.DrawString(_frm1.listBox_extraction_rlue1.Items[e.Index].ToString(), e.Font, myBrush, e.Bounds, null);
            e.DrawFocusRectangle();
        }


        void listBox_extraction_type2_Click(object sender, EventArgs e)
        {
            try
            {
                bool replaceFlag = false;
                if (_ruleItem == "")
                {
                    MessageBox.Show("请先先选择第一个道路类型");
                }
                else
                {
                    string[] rules = _ruleItem.Split('\\');
                    if (rules.Length == 1)
                    {
                        _ruleItem = _ruleItem + "\\" + _frm1.listBox_extraction_type2.SelectedIndex.ToString();

                    }
                     //已经选择了一对规则，但是又选择第二列中规则
                    else
                    {
                        //第二个规则
                        string originalRule = System.Enum.GetName(typeof(Link.道路类型), Convert.ToInt32(rules[1]));
                        //选择的第二个规则与刚刚选择那一对中的第二个不同
                        if (!originalRule.Equals(_frm1.listBox_extraction_type2.SelectedItem.ToString()))
                        {
                            DialogResult dr = MessageBox.Show("确认用 " + _frm1.listBox_extraction_type2.SelectedItem + " 替换 " + originalRule,
                                  "提示", MessageBoxButtons.YesNo);

                            //确认，那就更换吧
                            if (dr == DialogResult.Yes)
                            {
                                replaceFlag = true;
                                //确认要替换，生成新的规则
                                _ruleItem = rules[0] + "\\" + _frm1.listBox_extraction_type2.SelectedIndex.ToString();
                            }
                        }
                    }

                    //现有的规则中是否包含刚刚选择的规则？？？
                    bool existFlag = LinkMasterLayerFactory.checkContainedInRule(_ruleItem, _ruleList);
                    //现有的规则中没有
                    if (existFlag == false)
                    {
                        if (replaceFlag == true)
                        {
                            //确认要替换，先把原有的规则从规则列表中移除
                            _ruleList.Remove(_ruleList[_ruleList.Count - 1]);

                            //可视化列表中的旧规则移除
                            _frm1.listBox_extraction_rule2.Items.RemoveAt(_frm1.listBox_extraction_rule2.Items.Count - 1);
                            //可视化列表中的新规则添加
                            _frm1.listBox_extraction_rule2.Items.Add(_frm1.listBox_extraction_type2.SelectedItem);
                            
                        }
                        else
                        {
                            _frm1.listBox_extraction_rule2.Items.Add(_frm1.listBox_extraction_type2.SelectedItem);
                        }

                        //设置选择对象的颜色
                        _frm1.listBox_extraction_rule2.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
                        _frm1.listBox_extraction_rule2.DrawItem += listBox_extraction_rule2_DrawItem;

                        _ruleList.Add(_ruleItem);
                        _ruleItem = "";
                    }
                    else
                    {
                        string[] temrule = _ruleItem.Split('\\');
                        if (temrule.Length == 2)
                        {
                            string rule1 = System.Enum.GetName(typeof(Link.道路类型), Convert.ToInt32(temrule[0]));
                            string rule2 = System.Enum.GetName(typeof(Link.道路类型), Convert.ToInt32(temrule[1]));
                            _ruleItem = temrule[0];
                            MessageBox.Show(rule1 + "+" + rule2 + "已存在");
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// 重绘
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void listBox_extraction_rule2_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            Brush myBrush = Brushes.Black; //初始化字体颜色=黑色
            //为项设置字体颜色

            if (e.Index % 2 == 0)
            {
                e.Graphics.FillRectangle(Brushes.Yellow, e.Bounds);
                myBrush = Brushes.Black;
            }
            else
            {
                e.Graphics.FillRectangle(Brushes.SkyBlue, e.Bounds);
                myBrush = Brushes.Black;
            }

            e.Graphics.DrawString(_frm1.listBox_extraction_rule2.Items[e.Index].ToString(), e.Font, myBrush, e.Bounds, null);
            e.DrawFocusRectangle();
        }

        #endregion -----------------------禁止打断规则的设定----------------------------

        #region ---------------------------指定禁止打断的交叉道路-----------------------------------

        /// <summary>
        /// 设计指定禁止打断的Road对
        /// </summary>
        private void layoutSeleFea()
        {
            _frm1.button_extaction_AddRoadPair = new Button();
            _frm1.button_extaction_AddRoadFea = new Button();

            WinFormDesigner.layoutButton(_frm1.button_extaction_AddRoadPair, "添加Road对", _frm1.groupBox_extraction_sclRoad.Controls,
                new System.Drawing.Point(LEFTX, LEFTY), DockStyle.None, BTNWIDTH, _frm1.listBox_extraction_type1.Width);
            _frm1.button_extaction_AddRoadPair.Click += button_extaction_AddRoadPair_Click;

            //g_frm1.button_extaction_AddRoadPair.AutoSize = true;

            int btnRoad_x = _frm1.button_extaction_AddRoadPair.Location.X + _frm1.button_extaction_AddRoadPair.Width + LINEWIDTH * 2;

            WinFormDesigner.layoutButton(_frm1.button_extaction_AddRoadFea, "添加Road", _frm1.groupBox_extraction_sclRoad.Controls,
                new System.Drawing.Point(btnRoad_x, LEFTY), DockStyle.None, BTNWIDTH, _frm1.listBox_extraction_type1.Width);
            _frm1.button_extaction_AddRoadFea.Click += button_extaction_AddRoadFea_Click;
            _frm1.button_extaction_AddRoadFea.Enabled = false;


            _frm1.label_extraction_roadPair1 = new Label();
            int label_forbid1_y = _frm1.button_extaction_AddRoadPair.Location.Y + BTNWIDTH + LINEWIDTH;
            WinFormDesigner.layoutLabel(_frm1.label_extraction_roadPair1, "第一个Road", _frm1.groupBox_extraction_sclRoad.Controls,
                new System.Drawing.Point(LEFTX, label_forbid1_y), DockStyle.None);

            _frm1.label_extraction_roadPair2 = new Label();
            int label_forbid2_x = _frm1.button_extaction_AddRoadFea.Location.X;
            WinFormDesigner.layoutLabel(_frm1.label_extraction_roadPair2, "第二个Road", _frm1.groupBox_extraction_sclRoad.Controls,
                new System.Drawing.Point(label_forbid2_x, label_forbid1_y), DockStyle.None);

            _frm1.listBox_extraction_roadPair1 = new ListBox();
            _frm1.listBox_extraction_roadPair2 = new ListBox();

            int road1_listbox_y = _frm1.label_extraction_roadPair1.Location.Y + _frm1.label_extraction_roadPair1.Height + LINEWIDTH;
            WinFormDesigner.layoutListItem(_frm1.listBox_extraction_roadPair1, _frm1.groupBox_extraction_sclRoad.Controls, new System.Drawing.Point(LEFTX, road1_listbox_y), DockStyle.None);
            _frm1.listBox_extraction_roadPair1.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            _frm1.listBox_extraction_roadPair1.DrawItem += listBox_extraction_roadPair1_DrawItem;

            WinFormDesigner.layoutListItem(_frm1.listBox_extraction_roadPair2, _frm1.groupBox_extraction_sclRoad.Controls,
                new System.Drawing.Point(label_forbid2_x, road1_listbox_y), DockStyle.None);
            _frm1.listBox_extraction_roadPair2.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            _frm1.listBox_extraction_roadPair2.DrawItem += listBox_extraction_roadPair2_DrawItem;

            _frm1.listBox_extraction_roadPair2.Width = _frm1.listBox_extraction_type2.Width;
            _frm1.listBox_extraction_roadPair1.Width = _frm1.groupBox_extraction_sclRoad.Width / 2 - 30;

        }

        void listBox_extraction_roadPair1_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            Brush myBrush = Brushes.Black; //初始化字体颜色=黑色
            //为项设置字体颜色

            if (e.Index % 2 == 0)
            {
                e.Graphics.FillRectangle(Brushes.Yellow, e.Bounds);
                myBrush = Brushes.Black;
            }
            else
            {
                e.Graphics.FillRectangle(Brushes.SkyBlue, e.Bounds);
                myBrush = Brushes.Black;
            }

            e.Graphics.DrawString(_frm1.listBox_extraction_roadPair1.Items[e.Index].ToString(), e.Font, myBrush, e.Bounds, null);
            e.DrawFocusRectangle();
        }

        void listBox_extraction_roadPair2_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            Brush myBrush = Brushes.Black; //初始化字体颜色=黑色
            //为项设置字体颜色

            if (e.Index % 2 == 0)
            {
                e.Graphics.FillRectangle(Brushes.Yellow, e.Bounds);
                myBrush = Brushes.Black;
            }
            else
            {
                e.Graphics.FillRectangle(Brushes.SkyBlue, e.Bounds);
                myBrush = Brushes.Black;
            }

            e.Graphics.DrawString(_frm1.listBox_extraction_roadPair2.Items[e.Index].ToString(), e.Font, myBrush, e.Bounds, null);
            e.DrawFocusRectangle();
        }

        //添加第二个
        void button_extaction_AddRoadFea_Click(object sender, EventArgs e)
        {
            _frm1.FeaPairSlctFlag = 1;
            _frm1.ToolBarFlag = false;
            LayerHelper.ClearSelect(_frm1.axMapControl1);
            LayerHelper.SelectLayer(_frm1.axMapControl1, Road.RoadNm);


        }

        //清空上一次的选择，并选择第一个
        void button_extaction_AddRoadPair_Click(object sender, EventArgs e)
        {
            if (_frm1.FeaClsRoad == null)
            {
                MessageBox.Show("请在\"文件-打开\"中打开Road图层");
            }
            else
            {

                _frm1.FeaPairSlctFlag = 0;
                _frm1.ToolBarFlag = false;
                LayerHelper.ClearSelect(_frm1.axMapControl1);
                LayerHelper.SelectLayer(_frm1.axMapControl1, Road.RoadNm);
                IGraphicsContainer pGraphicsContainer = _frm1.axMapControl1.ActiveView as IGraphicsContainer;//把地图的当前view作为图片的容器
                pGraphicsContainer.DeleteAllElements();
            }
        }

        #endregion ---------------------------指定禁止打断的交叉道路-----------------------------------

        #region --------------------------提取路网的操作---------------------------------------
        /// <summary>
        /// 最终提取路网的操作
        /// </summary>
        private void layoutOperation()
        {
            _frm1.button_extration_extract = new Button();
            string str = "";
            if (_frm1.comBox_extraction_function.SelectedIndex == 0)
            {
                str = "提取指路标志路网";
            }
            else
            {
                str = "提取Segment路网";
            }
            WinFormDesigner.layoutButton(_frm1.button_extration_extract, str, _frm1.groupBox_extraction_operation.Controls, new System.Drawing.Point(LEFTX, LEFTY), DockStyle.None, BTNWIDTH, 100);
            _frm1.button_extration_extract.AutoSize = true;
            _frm1.button_extration_extract.Click += button_extration_extract_Click;
        }

        void button_extration_extract_Click(object sender, EventArgs e)
        {
            createRoadNetwork();
        }


        private void createNewDatabase()
        {
            //ISpatialReference spatialReference = spatialReferenceFactory.CreateProjectedCoordinateSystem((int)esriSRProjCS4Type.esriSRProjCS_Xian1980_3_Degree_GK_Zone_38);
            ISpatialReference spatialReference = (_frm1.FeaClsRoad as IGeoDataset).SpatialReference;
            IFeatureDataset pFeaDataset = GeodatabaseHelper.CreateGeoDatabase(spatialReference);
            switch (CopyFlag)
            {
                case (int)CopyFeatureClassAndTable.CopyForGuideSignAndSegmentNetowrk:
                    {
                        try
                        {
                            GeodatabaseHelper.CopyFeaClsToDataset(((IDataset)_frm1.FeaClsRoad).Workspace, pFeaDataset,
                                _frm1.FeaClsRoad.AliasName, "Road");
                            _frm1.UpdateGeoDatabase(pFeaDataset.Workspace.PathName);
                            break;
                        }
                        catch (Exception ex)
                        {
                            
                            break;
                        }
                        //Copy road featureclass to the new geodatabase
                        
                    }

                case (int)CopyFeatureClassAndTable.CopyForRoad2BasicNetwork:
                    {
                        GeodatabaseHelper.CopyFeaClsToDataset(((IDataset)_frm1.FeaClsRoad).Workspace, pFeaDataset,
                            _frm1.FeaClsRoad.AliasName,Road.RoadNm);

                        _frm1.UpdateGeoDatabase(pFeaDataset.Workspace.PathName);
                        break;
                    }
                default:
                    break;
            }
        }


        private void createRoadNetwork()
        {
            //创建geodatabase
            try
            {
                List<NodeInfor> forbidddonBreakRoads = new List<NodeInfor>();
                forbidddonBreakRoads = getNodeInforOfForbidden(_frm1.RoadFeaPairs);

                createNewDatabase();

                if (_frm1.comBox_extraction_function.SelectedIndex == (int)ExtractionType.路段路网)
                {

                    SegmentLayerBuilder segFactory = new SegmentLayerBuilder(_frm1);
                    segFactory.AssembleSegmentLayer(forbidddonBreakRoads, _ruleList);

                }
                else if (_frm1.comBox_extraction_function.SelectedIndex == (int)ExtractionType.指路标志路网)
                {
                    Arc1LayerFactory rsFactory = new Arc1LayerFactory(_frm1);
                    rsFactory.AssembleSegmentLayer(forbidddonBreakRoads, _ruleList);
                }
                else if (_frm1.comBox_extraction_function.SelectedIndex == (int)ExtractionType.车道级路网)
                {

                    SegmentLayerBuilder segFactory = new SegmentLayerBuilder(_frm1);
                    segFactory.AssembleSegmentLayer(forbidddonBreakRoads, _ruleList);
                    //为避免打开conn后，数据库被独占，这样会导致在创建拓扑的时候的报错
                    //因此在创建完拓扑后，在更新Connection

                    //
                    MessageBox.Show("已生成路段路网，请在ArcGIS,标注交通组织中断点");
                    //_frm1.UpdateOleDbConnection(_frm1.MdbPath);
                    //Segment2BasicRoadNetwork road2BasicRoadNetwork = new Segment2BasicRoadNetwork(_frm1);
                    //road2BasicRoadNetwork.Convert2BasicRoadNetwork();
                }
                //删除所有的标注
                IGraphicsContainer graphicsCon = _frm1.axMapControl1.ActiveView as IGraphicsContainer;
                graphicsCon.DeleteAllElements();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// 获取禁止打断的road对间的节点消息(NodeInfor)
        /// </summary>
        /// <param name="roadFeaPairs"></param>
        /// <returns></returns>
        private List<NodeInfor> getNodeInforOfForbidden(List<FeaPair> roadFeaPairs)
        {
            List<NodeInfor> nodeInfors = new List<NodeInfor>();

            foreach (var item in roadFeaPairs)
            {
                Road roadEnt1 = new Road();
                RoadService road = new RoadService(_frm1.FeaClsRoad, 0);
                roadEnt1 = road.GetEntity(item.Fea1);

                Road roadEty2 = new Road();
                roadEty2 = road.GetEntity(item.Fea2);

                IPoint pnt = LineHelper.GetIntersectionPoint(item.Fea1.Shape as IPolyline, item.Fea2.Shape as IPolyline);

                NodeInfor nodeInfor = new NodeInfor(0, pnt, "0\\0", false);
                nodeInfors.Add(nodeInfor);
            }
            return nodeInfors;
        }

        #endregion --------------------------提取路网的操作---------------------------------------


       private static void testLaneBased()
        {
            #region test code and example
            //List<string> feaName=new List<string>();
            //    feaName.Add(RoadEntity.RoadNm);
            //   List<string> strList=new List<string>();
            //   strList.Add("Road");
            //   IFeatureClass pFeatureClsRoad = FeatureClassHelper.GetFeaClsInAccess(g_frm1.MdbPath, strList)[0];
            //   SegmentLayerFactory rsFactory = new SegmentLayerFactory(pFeatureClsRoad);

            //    List<NodeInfor> updateNodeInforList = new List<NodeInfor>();
            //    List<string> forbiddenRuls = new List<string>();

            //    forbiddenRuls.Add("0\\1");
            //    forbiddenRuls.Add("0\\2");
            //    forbiddenRuls.Add("0\\3");
            //    forbiddenRuls.Add("0\\4");
            //    forbiddenRuls.Add("0\\5");
            //    forbiddenRuls.Add("1\\4");
            //    forbiddenRuls.Add("1\\5");
            //    forbiddenRuls.Add("1\\7");
            //    forbiddenRuls.Add("2\\5");
            //    forbiddenRuls.Add("2\\7");
            //    forbiddenRuls.Add("6\\7");
            //    rsFactory.AssembleSegmentLayer(updateNodeInforList, forbiddenRuls);
            #endregion test code and example
        }

       private static void testGuideSign()
       {
           #region test code and example
           //List<string> feaName = new List<string>();
           //feaName.Add(RoadEntity.RoadNm);
           //List<string> strList = new List<string>();
           //strList.Add("Road");
           //IFeatureClass pFeatureClsRoad = FeatureClassHelper.GetFeaClsInAccess(g_frm1.MdbPath, strList)[0];
           //Arc1LayerFactory rsFactory = new Arc1LayerFactory(pFeatureClsRoad);

           //List<NodeInfor> updateNodeInforList = new List<NodeInfor>();
           //List<string> forbiddenRuls = new List<string>();

           //forbiddenRuls.Add("0\\1");
           //forbiddenRuls.Add("0\\2");
           //forbiddenRuls.Add("0\\3");
           //forbiddenRuls.Add("0\\4");
           //forbiddenRuls.Add("0\\5");
           //forbiddenRuls.Add("1\\4");
           //forbiddenRuls.Add("1\\5");
           //forbiddenRuls.Add("1\\7");
           //forbiddenRuls.Add("2\\5");
           //forbiddenRuls.Add("2\\7");
           //forbiddenRuls.Add("6\\7");
           //rsFactory.AssembleSegmentLayer(updateNodeInforList, forbiddenRuls);
           #endregion test code and example

       }

       
    }
}
