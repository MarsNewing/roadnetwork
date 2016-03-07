using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.RoadSign;
using RoadNetworkSystem.GIS;
using RoadNetworkSystem.NetworkEditor.EditorFlow;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RoadNetworkSystem.WinForm.NetworkEditor
{
    class LinkEditorDesigner
    {
        private static Form1 _frm1;
        private const double LANEWEIDTH = 3.5;
        private const int LANE_NUM_DEFUAL = 3;
        private const bool IS_ONE_WAY = true;
        
        //左上角
        private const int LEFTX = 10;
        private const int LEFTY = 25;
        //行距
        private const int LINEWIDTH = 10;
        //右侧的第一个控件的X坐标
        private static int RIGHTX = 0;
        //textBox的宽度
        private static int _textBoxWidth = 80;

        #region --------------------拓扑数据-------------------------------

        //只保存两个点
        private static int _slctPntIndex = 0;


        private static Link _crtLineEty = new Link();

        //Arc相关的
        
        private static Arc _sameArcEty=new Arc();
        private static Arc _oppArcEty=new Arc();
        private static IFeature _sameArcFea=null;
        private static IFeature _oppArcFea=null;

        ///Link的FlowDir字段值，由两个CheckBox决定
        private static int _flowDir = 0;


        private const double INITFCUR = 5;
        private const double INITTCUT = 10;

        #endregion --------------------拓扑数据-------------------------------

        private static int _mouseFlag = -1;



        /*
         * ----------------------------------
         * 
         * 
         *        panel_public_Bottom
         * 
         * ----------------------------------
         * 
         * 
         * 
         * 
         *        panel_public_Middle
         * 
         * 
         * 
         * 
         * 
         * ------------------------------------- 
         *        panel_public_Bottom 
         * -------------------------------------
         */


        /*
         * ----------------------------------
         * 
         * 
         *          groupBox1  richTextBox1           
         * 
         * ----------------------------------
         * 
         * 
         * 
         * 
         *                    
         *             tabControl_link
         * 
         * 
         * 
         * 
         * ------------------------------------- 
         *             splitContainer4 
         * -------------------------------------
         */

        /// <summary>
        /// 初始化
        /// </summary>
        private static void initValue()
        {
            _frm1.SlctPntIndex = 0;
            _frm1.FNodeFea=null;
            _frm1.TNodeFea=null;
            _frm1.CrttLinkFea=null;
            _frm1.CrtLine=null;
            _crtLineEty = null;
            _sameArcEty=new Arc();
            _oppArcEty=new Arc();
            _sameArcFea=null;
            _oppArcFea=null;
            
        }

        //拖拽界面设计
        public static void SetLinkPalette(Form1 frm1)
        {
            _frm1 = frm1;
            _flowDir = Link.FLOWDIR_DOUBLE;

            #region 设置中间编辑面板
            //初始化tabPage_link_core
            _frm1.tabPage_link_core = new TabPage();
            layoutLinkCorePage();

            //初始化tabPage_link_Flag
            _frm1.tabPage_link_Flag = new TabPage();
            layoutLinkFlagPage();

            List<TabPage> pageList = new List<TabPage>();
            pageList.Add(_frm1.tabPage_link_core);
            pageList.Add(_frm1.tabPage_link_Flag);
            _frm1.tabControl_link = new TabControl();
            WinFormDesigner.layoutTabControl(_frm1.tabControl_link, _frm1.panel_Middle, new System.Drawing.Point(1, 1), pageList);

            _frm1.groupBox_Link_BasicAtrr = new GroupBox();
            _frm1.groupBox_Link_Operation = new GroupBox();
            _frm1.groupBox_Link_Flow = new GroupBox();

            _frm1.tabPage_link_core.Controls.Add(_frm1.groupBox_Link_Operation);
            _frm1.tabPage_link_core.Controls.Add(_frm1.groupBox_Link_Flow);
            _frm1.tabPage_link_core.Controls.Add(_frm1.groupBox_Link_BasicAtrr);

            //在Core选项卡中添加一个groupbox,用于管理基本属性

            WinFormDesigner.setGroupBoxStyle(_frm1.groupBox_Link_BasicAtrr, "Basic Attribute",
                new System.Drawing.Point(0, 0), DockStyle.Top, 150, 0);

            
            //基础属性选择卡
            setBasicAttGroupBox();

            //MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM

            //交通流设置，单双向设置
            WinFormDesigner.setGroupBoxStyle(_frm1.groupBox_Link_Flow, "Flow", 
               new System.Drawing.Point(0, _frm1.groupBox_Link_BasicAtrr.Height), DockStyle.Top, 150, 1);
            //基础属性选择卡
            setFlowGroupBox();


            //在Core选项卡中添加一个groupbox，用于管理相关操作

            int groupBoxOperation_Y = _frm1.groupBox_Link_BasicAtrr.Height + _frm1.groupBox_Link_Flow.Height;
            WinFormDesigner.setGroupBoxStyle(_frm1.groupBox_Link_Operation, "Operation", 
                new System.Drawing.Point(0, groupBoxOperation_Y), DockStyle.Fill, 100, 2);

           

            //基础属性选择卡
            setOperationGroupBox();

            #endregion 设置中间编辑面板


            //事件相应
            _frm1.Resize += _frm1_Resize;
            _frm1.button_Add.Click += button_Add_Click;
            _frm1.button_Save.Click += button_Save_Click;
            _frm1.button_Refresh.Click += button_Refresh_Click;
            _frm1.upDown_link_SameLaneMun.ValueChanged += upDown_link_SameLaneMun_ValueChanged;
            _frm1.upDown_link_OppLaneMun.ValueChanged += upDown_link_OppLaneMun_ValueChanged;
            _frm1.button_link_split.Click += button_link_split_Click;
        }

        static void button_link_split_Click(object sender, EventArgs e)
        {
            Dictionary<string, IFeatureClass> feaClsDic = new Dictionary<string, IFeatureClass>();
            feaClsDic.Add(Node.NodeName, _frm1.FeaClsNode);
            feaClsDic.Add(Link.LinkName, _frm1.FeaClsLink);
            feaClsDic.Add(Arc.ArcFeatureName, _frm1.FeaClsArc);

            feaClsDic.Add(Lane.LaneName, _frm1.FeaClsLane);
            feaClsDic.Add(LaneConnector.ConnectorName, _frm1.FeaClsConnector);
            feaClsDic.Add(Boundary.BoundaryName, _frm1.FeaClsBoundary);

            feaClsDic.Add(StopLine.StopLineName, _frm1.FeaClsStopLine);
            feaClsDic.Add(Kerb.KerbName, _frm1.FeaClsKerb);
            feaClsDic.Add(TurnArrow.TurnArrowName, _frm1.FeaClsTurnArrow);

            feaClsDic.Add(Surface.SurfaceName, _frm1.FeaClsSurface);

            if (_frm1.CrttLinkFea != null)
            {
                SegmentConstructor segContruct = new SegmentConstructor(feaClsDic, _frm1.FNodeFea, _frm1.TNodeFea);
                segContruct.SplitLink(_frm1.CrttLinkFea);
            }
        }

        #region -----------------------拓扑更新--------------------------------

      

        #endregion -----------------------拓扑更新--------------------------------



        #region----------------------事件相应--------------------------

        static void button_Save_Click(object sender, EventArgs e)
        {
            if (Convert.ToInt32(_frm1.button_Save.Tag) == 2 && _frm1.CrtLine != null)
            {
                Dictionary<string, IFeatureClass> feaClsDic = new Dictionary<string, IFeatureClass>();
                feaClsDic.Add(Node.NodeName, _frm1.FeaClsNode);
                feaClsDic.Add(Link.LinkName, _frm1.FeaClsLink);
                feaClsDic.Add(Arc.ArcFeatureName, _frm1.FeaClsArc);

                feaClsDic.Add(Lane.LaneName, _frm1.FeaClsLane);
                feaClsDic.Add(LaneConnector.ConnectorName, _frm1.FeaClsConnector);
                feaClsDic.Add(Boundary.BoundaryName, _frm1.FeaClsBoundary);

                feaClsDic.Add(StopLine.StopLineName, _frm1.FeaClsStopLine);
                feaClsDic.Add(Kerb.KerbName, _frm1.FeaClsKerb);
                feaClsDic.Add(TurnArrow.TurnArrowName, _frm1.FeaClsTurnArrow);

                feaClsDic.Add(Surface.SurfaceName, _frm1.FeaClsSurface);

                SegmentConstructor segContruct = new SegmentConstructor(feaClsDic,_frm1.FNodeFea, _frm1.TNodeFea);
                int roadType=_frm1.comboBox_Layer.SelectedIndex + 1;
                string roadName=_frm1.textBox_link_roadNm.Text;
                int sameLaneNum=Convert.ToInt32(_frm1.upDown_link_SameLaneMun.Value);
                int oppLaneNum=Convert.ToInt32(_frm1.upDown_link_OppLaneMun.Value);

                segContruct.CreateLinkTopo(_frm1.CrtLine, _frm1.FNodeFea, _frm1.TNodeFea, roadType, roadName, 
                    _flowDir, sameLaneNum, oppLaneNum);
               
                //刷新地图
                _frm1.axMapControl1.Refresh();

                //保存完后，默认选择Link图层
                LayerHelper.ClearSelect(_frm1.axMapControl1);
                LayerHelper.SelectLayer(_frm1.axMapControl1, Link.LinkName);

                //结点清空
                _frm1.FNodeFea = null;
                _frm1.TNodeFea = null;
            }
        }


        

        static void upDown_link_OppLaneMun_ValueChanged(object sender, EventArgs e)
        {
            _oppArcEty.LaneNum = Convert.ToInt32(_frm1.upDown_link_OppLaneMun.Value);
        }

        static void upDown_link_SameLaneMun_ValueChanged(object sender, EventArgs e)
        {
            _sameArcEty.LaneNum = Convert.ToInt32(_frm1.upDown_link_SameLaneMun.Value);
        }

        static void button_Refresh_Click(object sender, EventArgs e)
        {
            if (Convert.ToInt32(_frm1.button_Refresh.Tag) == 2)
            {
                //刷新后，默认选择Link图层
                LayerHelper.ClearSelect(_frm1.axMapControl1);
                LayerHelper.SelectLayer(_frm1.axMapControl1, Link.LinkName);

                //刷新所有的被选中的东东
                IGraphicsContainer pGraphicsContainer = _frm1.axMapControl1.ActiveView as IGraphicsContainer;//把地图的当前view作为图片的容器
                pGraphicsContainer.DeleteAllElements();//显示储存在 IElement 中图形，这样就持久化了。
                _frm1.axMapControl1.Refresh();

                initValue();
            }
        }
      

        static void button_Add_Click(object sender, EventArgs e)
        {
            if (Convert.ToInt32(_frm1.button_Add.Tag) == 2)
            {
                _frm1.ToolBarFlag = false;
                LayerHelper.ClearSelect(_frm1.axMapControl1);
                LayerHelper.SelectLayer(_frm1.axMapControl1, Node.NodeName);

                IGraphicsContainer pGraphicsContainer = _frm1.axMapControl1.ActiveView as IGraphicsContainer;//把地图的当前view作为图片的容器
                pGraphicsContainer.DeleteAllElements();//显示储存在 IElement 中图形，这样就持久化了。
                _frm1.SlctPntIndex = 0;
                
            }
        }

        //当窗体形状发生变化时，相对位置的控件需要重新定位
        static void _frm1_Resize(object sender, EventArgs e)
        {
            if (Convert.ToString(_frm1.comboBox_Layer.SelectedItem).Equals(Link.LinkName))
            {

                RIGHTX = _frm1.comBox_link_roadType.Location.X;

                WinFormDesigner.layoutTextBox(_frm1.textBox_link_roadNm, _frm1.groupBox_Link_BasicAtrr.Controls,
                    new System.Drawing.Point(RIGHTX, _frm1.label_link_roadNm.Location.Y - 5),
                    DockStyle.None);

                WinFormDesigner.layoutTextBox(_frm1.textBox_link_roadNm, _frm1.groupBox_Link_BasicAtrr.Controls,
                   new System.Drawing.Point(RIGHTX, _frm1.label_link_roadNm.Location.Y - 5),
                   DockStyle.None);

                //flow
                //same
                WinFormDesigner.layoutLabel(_frm1.label_link_SameFlow, "车道数", _frm1.groupBox_Link_Flow.Controls,
                new System.Drawing.Point(RIGHTX, _frm1.checkBox_link_SameFlow.Location.Y + 5), DockStyle.None);

                int sameLaneNum_y = _frm1.checkBox_link_SameFlow.Location.Y + _frm1.checkBox_link_SameFlow.Height + LINEWIDTH;
                WinFormDesigner.layoutNumberupdown(_frm1.upDown_link_SameLaneMun, LANE_NUM_DEFUAL, _frm1.groupBox_Link_Flow.Controls,
                    new System.Drawing.Point(RIGHTX + 5, sameLaneNum_y));

                //Opp
                int oppFlowCheckBox_y = sameLaneNum_y + _frm1.upDown_link_SameLaneMun.Height + LINEWIDTH + 5;
                WinFormDesigner.layoutLabel(_frm1.label_link_OppFlow, "车道数", _frm1.groupBox_Link_Flow.Controls,
                    new System.Drawing.Point(RIGHTX, oppFlowCheckBox_y + 5), DockStyle.None);

                int oppLaneNum_y = _frm1.label_link_OppFlow.Location.Y + _frm1.label_link_OppFlow.Height + LINEWIDTH;
                WinFormDesigner.layoutNumberupdown(_frm1.upDown_link_OppLaneMun, LANE_NUM_DEFUAL, _frm1.groupBox_Link_Flow.Controls,
                    new System.Drawing.Point(RIGHTX + 5, oppLaneNum_y));

                if (IS_ONE_WAY)
                {
                    _frm1.checkBox_link_OppFlow.Checked = false;
                }

                _frm1.groupBox_Link_BasicAtrr.Height = _frm1.label_link_roadNm.Location.Y + _frm1.label_link_roadNm.Height + LINEWIDTH;


            }
        }


        static void checkBox_link_OppFlow_CheckStateChanged(object sender, EventArgs e)
        {
            if (_frm1.checkBox_link_OppFlow.Checked == false)
            {
                _frm1.upDown_link_OppLaneMun.Enabled = false;
                _flowDir = 1;
                if (_frm1.checkBox_link_SameFlow.Checked == false)
                {

                    DialogResult dr = MessageBox.Show("确定路段设为关闭？", "Warning", MessageBoxButtons.YesNo);
                    if (dr == DialogResult.Yes)
                    {
                        _frm1.groupBox_Link_BasicAtrr.Enabled = false;
                    }
                }
            }
            else
            {
                _frm1.upDown_link_OppLaneMun.Enabled = true;
                _frm1.groupBox_Link_BasicAtrr.Enabled = true;
            }
        }

        static void checkBox_link_SameFlow_CheckStateChanged(object sender, EventArgs e)
        {
            if (_frm1.checkBox_link_SameFlow.Checked == false)
            {
                _frm1.upDown_link_SameLaneMun.Enabled = false;
                _flowDir = -1;
                if (_frm1.checkBox_link_SameFlow.Checked == false)
                {

                    DialogResult dr = MessageBox.Show("确定路段设为关闭？", "Warning", MessageBoxButtons.YesNo);
                    if (dr == DialogResult.Yes)
                    {
                        _frm1.groupBox_Link_BasicAtrr.Enabled = false;
                    }
                }
            }
            else
            {
                _frm1.upDown_link_SameLaneMun.Enabled = true;

            }
        }

        #endregion----------------------事件相应--------------------------


        #region----------------------界面设计--------------------------
        /// <summary>
        /// 设置中间面板中的Core选项卡中的基本属性
        /// </summary>
        private static void setBasicAttGroupBox()
        {
            //设置道路类型
            _frm1.label_link_roadType = new Label();
            WinFormDesigner.layoutLabel(_frm1.label_link_roadType, "道路类型", _frm1.groupBox_Link_BasicAtrr.Controls,
                new System.Drawing.Point(LEFTX, LEFTY), DockStyle.None);
            //g_frm1.label_link_roadType.Width = Convert.ToInt32(g_frm1.label_link_roadType.Font.Size * 5);

            _frm1.comBox_link_roadType = new ComboBox();
            setRoadTypeCombox();

            //给定右侧控件的X坐标
            RIGHTX = _frm1.comBox_link_roadType.Location.X;

            //设置道路名称
            _frm1.label_link_roadNm = new Label();
            WinFormDesigner.layoutLabel(_frm1.label_link_roadNm, "道路名", _frm1.groupBox_Link_BasicAtrr.Controls,
                new System.Drawing.Point(LEFTX, _frm1.label_link_roadType.Location.Y + _frm1.label_link_roadType.Height + LINEWIDTH),
                DockStyle.None);
            //g_frm1.label_link_roadNm.Width = Convert.ToInt32(g_frm1.label_link_roadNm.Font.Size * 4);


            _frm1.textBox_link_roadNm = new TextBox();
            WinFormDesigner.layoutTextBox(_frm1.textBox_link_roadNm, _frm1.groupBox_Link_BasicAtrr.Controls,
                new System.Drawing.Point(RIGHTX, _frm1.label_link_roadNm.Location.Y - 5),
                DockStyle.None);
            _frm1.textBox_link_roadNm.Width = _textBoxWidth;
            _frm1.groupBox_Link_BasicAtrr.Height = _frm1.label_link_roadNm.Location.Y + _frm1.label_link_roadNm.Height + LINEWIDTH;
        }

        private static void setFlowGroupBox()
        {
            #region  ---------------同向------------------------------
            //同向交通流设置，
            _frm1.checkBox_link_SameFlow = new CheckBox();
            WinFormDesigner.layoutCheckBox(_frm1.checkBox_link_SameFlow, "同向交通流", _frm1.groupBox_Link_Flow.Controls,
                new System.Drawing.Point(LEFTX, LEFTY), DockStyle.None);
            _frm1.checkBox_link_SameFlow.Checked = true;
            _frm1.checkBox_link_SameFlow.CheckStateChanged += checkBox_link_SameFlow_CheckStateChanged;

            //同向车道数设置
            _frm1.label_link_SameFlow = new Label();
            WinFormDesigner.layoutLabel(_frm1.label_link_SameFlow, "车道数", _frm1.groupBox_Link_Flow.Controls,
                new System.Drawing.Point(RIGHTX, _frm1.checkBox_link_SameFlow.Location.Y + 5), DockStyle.None);
            //g_frm1.label_link_SameFlow.Width = Convert.ToInt32(g_frm1.label_link_SameFlow.Font.Size * 4);
            

            _frm1.upDown_link_SameLaneMun = new NumericUpDown();
            int sameLaneNum_y = _frm1.checkBox_link_SameFlow.Location.Y + _frm1.checkBox_link_SameFlow.Height + LINEWIDTH;
            WinFormDesigner.layoutNumberupdown(_frm1.upDown_link_SameLaneMun, LANE_NUM_DEFUAL, _frm1.groupBox_Link_Flow.Controls,
                new System.Drawing.Point(RIGHTX + 5, sameLaneNum_y));
            _frm1.upDown_link_SameLaneMun.Width = _textBoxWidth;

            #endregion  ---------------同向------------------------------


            #region  ---------------反向------------------------------
            //反向交通流设置，
            _frm1.checkBox_link_OppFlow = new CheckBox();
            int oppFlowCheckBox_y = sameLaneNum_y + _frm1.upDown_link_SameLaneMun.Height + LINEWIDTH + 5;
            WinFormDesigner.layoutCheckBox(_frm1.checkBox_link_OppFlow, "反向交通流", _frm1.groupBox_Link_Flow.Controls,
                new System.Drawing.Point(LEFTX, oppFlowCheckBox_y), DockStyle.None);
            _frm1.checkBox_link_OppFlow.Checked = true;
            _frm1.checkBox_link_OppFlow.CheckStateChanged += checkBox_link_OppFlow_CheckStateChanged;

            //反向车道数设置
            _frm1.label_link_OppFlow = new Label();
            WinFormDesigner.layoutLabel(_frm1.label_link_OppFlow, "车道数", _frm1.groupBox_Link_Flow.Controls,
                new System.Drawing.Point(RIGHTX, oppFlowCheckBox_y + 5), DockStyle.None);
            //g_frm1.label_link_OppFlow.Width = Convert.ToInt32(g_frm1.label_link_OppFlow.Font.Size * 4);

            //同向车道数，
            _frm1.upDown_link_OppLaneMun = new NumericUpDown();
            int oppLaneNum_y = _frm1.label_link_OppFlow.Location.Y + _frm1.label_link_OppFlow.Height + LINEWIDTH;
            WinFormDesigner.layoutNumberupdown(_frm1.upDown_link_OppLaneMun, LANE_NUM_DEFUAL, _frm1.groupBox_Link_Flow.Controls,
                new System.Drawing.Point(RIGHTX + 5, oppLaneNum_y));
            _frm1.upDown_link_OppLaneMun.Width = _textBoxWidth;

            if (IS_ONE_WAY)
            {
                _frm1.checkBox_link_OppFlow.Checked = false;
            }

            #endregion  ---------------反向------------------------------

            _frm1.groupBox_Link_Flow.Height = _frm1.upDown_link_OppLaneMun.Location.Y + _frm1.upDown_link_OppLaneMun.Height + 10;

        }

        /// <summary>
        /// 设置操作框
        /// </summary>
        private static void setOperationGroupBox()
        {
            _frm1.button_link_split = new Button();
            WinFormDesigner.layoutButton(_frm1.button_link_split, "打断", _frm1.groupBox_Link_Operation.Controls,
                new System.Drawing.Point(_frm1.label_link_roadType.Location.X, 30), DockStyle.None, 20, 50);

            _frm1.button_link_merge = new Button();
            int btn_merge_X = _frm1.label_link_roadType.Location.X + _frm1.button_link_split.Width + 20;
            WinFormDesigner.layoutButton(_frm1.button_link_merge, "合并", _frm1.groupBox_Link_Operation.Controls,
                new System.Drawing.Point(btn_merge_X, 30), DockStyle.None, 20, 50);

            _frm1.groupBox_Link_Operation.Height = _frm1.button_link_split.Location.Y + _frm1.button_link_split.Height + 30;
        }




        /// <summary>
        /// 道路类型选项条
        /// </summary>
        private static void setRoadTypeCombox()
        {
            _frm1.comBox_link_roadType.Location = new System.Drawing.Point(_frm1.label_link_roadType.Location.X + 50,
                _frm1.label_link_roadType.Location.Y - 2);

            _frm1.comBox_link_roadType.Dock = DockStyle.Right;
            _frm1.comBox_link_roadType.Font = new Font(_frm1.comBox_link_roadType.Font.Name, _frm1.label_link_roadType.Font.Size, _frm1.comBox_link_roadType.Font.Style);
            _frm1.comBox_link_roadType.Width = Convert.ToInt32(_frm1.comBox_link_roadType.Font.Size * 7);
            _textBoxWidth = _frm1.comBox_link_roadType.Width;
            try
            {

                _frm1.comBox_link_roadType.Items.AddRange(System.Enum.GetNames(typeof(Link.道路类型)));
                _frm1.groupBox_Link_BasicAtrr.Controls.Add(_frm1.comBox_link_roadType);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// Core选项卡
        /// </summary>
        private static void layoutLinkCorePage()
        {
            _frm1.tabPage_link_core.Text = "Core";
            _frm1.tabPage_link_core.AutoScroll = true;


        }


        /// <summary>
        /// Flag选项卡
        /// </summary>
        private static void layoutLinkFlagPage()
        {
            _frm1.tabPage_link_Flag.Text = "Flag";
            _frm1.tabPage_link_Flag.AutoScroll = true;
        }
        #endregion----------------------界面设计--------------------------


        //手打界面设置
        /*  
         * 
        #region -----------------路网编辑-----------------------
        /// <summary>
        /// 设置Link的界面
        /// </summary>
        public static void SetLinkPalette(Form1 frm1)
        {
            g_frm1 = frm1;

            g_frm1.fileLabel = new Label();
            WinFormDesigner.layoutLabel(g_frm1.fileLabel, "Fuck", g_frm1.panel_public_Middle, new System.Drawing.Point(35, 30));

            g_frm1.tabControl_link = new TabControl();
            List<TabPage> pageList = new List<TabPage>();
            //初始化tabPage_link_core
            g_frm1.tabPage_link_core = new TabPage();
            //初始化tabPage_link_Flag
            g_frm1.tabPage_link_Flag = new TabPage();
            layoutLinkCorePage();
            layoutLinkFlagPage();
            pageList.Add(g_frm1.tabPage_link_core);
            pageList.Add(g_frm1.tabPage_link_Flag);



            WinFormDesigner.layoutTabControl(g_frm1.tabControl_link, g_frm1.panel_public_Middle, new System.Drawing.Point(4, 4), pageList);
            g_frm1.tabControl_link.SelectedIndex = 0;
            g_frm1.tabControl_link.TabIndex = 0;
        }


        private static void layoutLinkCorePage()
        {
            g_frm1.tabPage_link_core.Text = "Core";
            g_frm1.tabPage_link_core.AutoScroll = true;
            
            
        }



        private static void layoutLinkFlagPage()
        {
            g_frm1.tabPage_link_core.Text = "Flag";
            g_frm1.tabPage_link_Flag.AutoScroll = true;
        }




        #endregion -----------------路网编辑-----------------------
        
        */

    }
}
