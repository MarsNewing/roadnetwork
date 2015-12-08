using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.Road;
using RoadNetworkSystem.EditorTool;
using RoadNetworkSystem.GIS;
using RoadNetworkSystem.NetworkElement.RoadLayer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace RoadNetworkSystem.WinForm.EditTool
{
    class EditToolDesigner
    {
        private static Form1 _frm1;
        private const double LANEWEIDTH = 3.5;
        //左上角
        private const int LEFTX = 10;
        private const int LEFTY = 25;

        
        //行距
        private const int LINEWIDTH = 10;
        //右侧的第一个控件的X坐标
        private static int RIGHTX = 0;
        //textBox的宽度
        private static int _textBoxWidth = 80;

        /// <summary>
        ///按钮的高度
        /// </summary>
        private static int BTNHEIGHT = 15;

        /// <summary>
        /// 选择的图层名
        /// </summary>
        private string selectLayer;


        private int _selectFeaCount;

        public EditToolDesigner(Form1 frm1)
        {
            _frm1 = frm1;
            _frm1.Resize += _frm1_Resize;
            _selectFeaCount = 0;
        }

        private void clearParentCtrl()
        {
            WinFormDesigner.ClearPanel(_frm1.panel_Top);
            WinFormDesigner.ClearPanel(_frm1.panel_Bottom);
            WinFormDesigner.ClearPanel(_frm1.panel_Middle);
            _frm1.panel_Top.Visible = true;

            _frm1.panel_Top.Visible = true;
        }

        private void setPartsHeight()
        {
            _frm1.panel_Top.Height = 25;
        }

        public void SetEditToolPattle()
        {
            clearParentCtrl();
            setPartsHeight();

            
            BTNHEIGHT = _frm1.panel_Top.Height;

            try
            {
                layToolIcon();

                //属性选择框
                _frm1.groupBox_Tool_SltAtt = new System.Windows.Forms.GroupBox();
                WinFormDesigner.setGroupBoxStyle(_frm1.groupBox_Tool_SltAtt, "选择对象属性",
                    new System.Drawing.Point(10, 10), System.Windows.Forms.DockStyle.Top, 200, 0);
                _frm1.panel_Middle.Controls.Add(_frm1.groupBox_Tool_SltAtt);

                //默认选择道路
                layoutRoad();
                if (_frm1.FeaClsRoad == null)
                {
                    DialogResult dr = MessageBox.Show("打开Road图层？", "Warnning", MessageBoxButtons.YesNo);
                    if (dr == DialogResult.Yes)
                    {
                        //选择Road图层
                        _frm1.Wsp = MapComponent.OpenGeoDatabase(_frm1.axMapControl1);
                        _frm1.getAllFeaClses(_frm1.Wsp);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            
        }

        static void _frm1_Resize(object sender, EventArgs e)
        {
            refreshFormLayout();
        }

        private static void refreshFormLayout()
        {
            if (_frm1.comBox_Tool_Layer != null)
            {
                _frm1.comBox_Tool_Layer.Width = _frm1.button_Tool_Add.Location.X;
            }
            if (_frm1.richTextBox_Tool_FirstFea != null)
            {
                _frm1.richTextBox_Tool_FirstFea.Width = _frm1.groupBox_Tool_SltAtt.Width - 2;
            }
            if (_frm1.richTextBox_Tool_SecondFea != null)
            {
                _frm1.richTextBox_Tool_SecondFea.Width = _frm1.groupBox_Tool_SltAtt.Width - 2;
            }
            if (_frm1.richTextBox_Tool_ThirdFea != null)
            {
                _frm1.richTextBox_Tool_ThirdFea.Width = _frm1.groupBox_Tool_SltAtt.Width - 2;
            }
            if (_frm1.richTextBox_Tool_ForthFea != null)
            {
                _frm1.richTextBox_Tool_ForthFea.Width = _frm1.groupBox_Tool_SltAtt.Width - 2;
            }
        }

        public enum 编辑图层
        {
            Road打断,
            Segment打断合并,
            基于SegNode的合并
        }

        private void layToolIcon()
        {
            #region -----------------------------------设置操作按钮------------------------------
            //添加按钮
            _frm1.button_Tool_Add = new System.Windows.Forms.Button();
            WinFormDesigner.layoutButton(_frm1.button_Tool_Add, "", _frm1.panel_Top.Controls, new System.Drawing.Point(1, 1),
                System.Windows.Forms.DockStyle.Right, BTNHEIGHT, BTNHEIGHT);
            _frm1.button_Tool_Add.Image = Properties.Resources.AddTool;
            
            _frm1.button_Tool_Add.Click += button_Tool_Add_Click;


            //重载按钮
            _frm1.button_Tool_Reload = new System.Windows.Forms.Button();
            WinFormDesigner.layoutButton(_frm1.button_Tool_Reload, "", _frm1.panel_Top.Controls, new System.Drawing.Point(1, 1),
                System.Windows.Forms.DockStyle.Right, BTNHEIGHT, BTNHEIGHT);
            _frm1.button_Tool_Reload.Image = Properties.Resources.ReloadTool;
            _frm1.button_Tool_Reload.Click += button_Tool_Reload_Click;

            //合并按钮
            _frm1.button_Tool_Merge = new System.Windows.Forms.Button();
            WinFormDesigner.layoutButton(_frm1.button_Tool_Merge, "", _frm1.panel_Top.Controls, new System.Drawing.Point(1, 1),
                System.Windows.Forms.DockStyle.Right, BTNHEIGHT, BTNHEIGHT);
            _frm1.button_Tool_Merge.Image = Properties.Resources.Merge;
            _frm1.button_Tool_Merge.Click += button_Tool_Merge_Click;


            
            //打断按钮
            _frm1.button_Tool_Break = new System.Windows.Forms.Button();
            WinFormDesigner.layoutButton(_frm1.button_Tool_Break, "", _frm1.panel_Top.Controls, new System.Drawing.Point(1, 1),
                System.Windows.Forms.DockStyle.Right, BTNHEIGHT, BTNHEIGHT);
            _frm1.button_Tool_Break.Image = Properties.Resources.Break;
            _frm1.button_Tool_Break.Click += button_Tool_Break_Click;

            ToolTip toolTip1 = new ToolTip();

            // Set up the delays for the ToolTip.
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            // Force the ToolTip text to be displayed whether or not the form is active.
            toolTip1.ShowAlways = true;

            // Set up the ToolTip text for the Button and Checkbox.
            toolTip1.SetToolTip(_frm1.button_Tool_Break, "打断");
            toolTip1.SetToolTip(_frm1.button_Tool_Merge, "合并");

            toolTip1.SetToolTip(_frm1.button_Tool_Add, "添加");
            toolTip1.SetToolTip(_frm1.button_Tool_Reload, "刷新");

            #endregion -----------------------------------设置操作按钮------------------------------


            //图层选择器
            _frm1.comBox_Tool_Layer = new System.Windows.Forms.ComboBox();
            _frm1.comBox_Tool_Layer.Items.AddRange(System.Enum.GetNames(typeof(编辑图层)));
            _frm1.comBox_Tool_Layer.Font = new Font(_frm1.comBox_Tool_Layer.Font.Name, _frm1.comBox_Tool_Layer.Font.Size + 2, _frm1.comBox_Tool_Layer.Font.Style);
            _frm1.comBox_Tool_Layer.Dock = System.Windows.Forms.DockStyle.Left;
            _frm1.comBox_Tool_Layer.Width = _frm1.button_Tool_Add.Location.X - _frm1.button_Tool_Add.Width-5;
            
            
            _frm1.comBox_Tool_Layer.SelectedIndex = 0;
            _frm1.panel_Top.Controls.Add(_frm1.comBox_Tool_Layer);

            _frm1.comBox_Tool_Layer.SelectedIndexChanged += comBox_Tool_Layer_SelectedIndexChanged;
        }

        void button_Tool_Break_Click(object sender, EventArgs e)
        {
            _frm1.ToolBarFlag = false;
            //两条Road在相交处打断,打断即可，无须修改拓扑关系
            //因为修改后的Road，要重新生成拓扑
            IntersectionTool pIntersectionTool = new IntersectionTool(_frm1);
            if (_frm1.comBox_Tool_Layer.SelectedIndex == 0)
            {
                if (_frm1.FirstRoadFea == null || _frm1.SecondRoadFea == null)
                {
                    MessageBox.Show("请选择两条Road");


                }
                else
                {
                    pIntersectionTool.IntersectionRoad();
                    MessageBox.Show("Road打断成功");
                }
            }
            //两条Segment在相交处打断，并修改拓扑关系。
            //如在Node图层中增加节点，Node表中增加相应的节点信息；
            //在Segment图层中增加节点，Segment表中增加相应的弧段信息
            else if (_frm1.comBox_Tool_Layer.SelectedIndex == 1)
            {

                if (_frm1.FirstSegFea == null || _frm1.SecondSegFea == null)
                {
                    MessageBox.Show("请选择两条Segment");

                }
                else
                {
                    pIntersectionTool.IntersectionSeg();
                    MessageBox.Show("Segment打断成功");
                }
            }
        }

        void button_Tool_Merge_Click(object sender, EventArgs e)
        {
            _frm1.ToolBarFlag = false;
            MergeTool pMergeTool = new MergeTool(_frm1);
            if (_frm1.comBox_Tool_Layer.SelectedIndex == 0)
            {

                if (_frm1.FirstRoadFea == null || _frm1.SecondRoadFea == null)
                {
                    MessageBox.Show("请选择两条Road");

                }
                else
                {
                    pMergeTool.MergeRoad();
                    MessageBox.Show("Road合并成功");
                }

            }
            else if (_frm1.comBox_Tool_Layer.SelectedIndex == 1)
            {
                if (_frm1.FirstSegFea == null || _frm1.SecondSegFea == null)
                {
                    MessageBox.Show("请选择两条Segment");

                }
                else
                {
                    pMergeTool.MergeSegment();
                    MessageBox.Show("Segment合并成功");

                }

            }

        }

        void button_Tool_Reload_Click(object sender, EventArgs e)
        {
            Form_Refresh();
            //所有图层不可选，除非点击添加按钮，才可以选择某一图层。
            LayerHelper.LayerNotSelect(_frm1.axMapControl1);
        }

        //添加页面刷新
        void Form_Refresh()
        {
            //1.清空地图中的鼠标选择
            LayerHelper.ClearSelect(_frm1.axMapControl1);

            //2.清空地图中画的symbol
            IGraphicsContainer pGraphicsContainer = _frm1.axMapControl1.ActiveView as IGraphicsContainer;//把地图的当前view作为图片的容器
            pGraphicsContainer.DeleteAllElements();
            _frm1.axMapControl1.Refresh();

            //3.右侧文字显示区域也要刷新
            switch (_frm1.comBox_Tool_Layer.SelectedIndex)
            {

                case (int)编辑图层.Road打断:
                    {
                        if (_frm1.groupBox_Tool_SltAtt.Controls.Count > 0)
                        {
                            _frm1.groupBox_Tool_SltAtt.Controls.Clear();
                        }

                        layoutRoad();
                        _frm1.SlctRoadIndex_EditTool = 0;
                        _frm1.FirstRoadFea = null;
                        _frm1.SecondRoadFea = null;
                        LayerHelper.SelectLayer(_frm1.axMapControl1, RoadEntity.RoadNm);
                        break;
                    }
                case (int)编辑图层.Segment打断合并:
                    {
                        if (_frm1.groupBox_Tool_SltAtt.Controls.Count > 0)
                        {
                            _frm1.groupBox_Tool_SltAtt.Controls.Clear();
                        }

                        layoutRoad();
                        _frm1.SlctSegmentIndex_EditTool = 0;
                        _frm1.FirstSegFea = null;
                        _frm1.SecondSegFea = null;
                        LayerHelper.SelectLayer(_frm1.axMapControl1, SegmentEntity.SegmentName);
                        break;
                    }
                case (int)编辑图层.基于SegNode的合并:
                    {
                        if (_frm1.groupBox_Tool_SltAtt.Controls.Count > 0)
                        {
                            _frm1.groupBox_Tool_SltAtt.Controls.Clear();
                        }
                        layoutSegmentNode();
                        _frm1.NodeSelected = false;
                        _frm1.SltSegmentNode = null;
                        //选择点图层中，也有用到该全局变量SlctSegmentIndex_EditTool
                        _frm1.SlctSegmentIndex_EditTool = 0;
                        _frm1.FirstSegFea = null;
                        _frm1.SecondSegFea = null;

                        LayerHelper.SelectLayer(_frm1.axMapControl1, SegmentNodeEntity.RoadSegmentNodeName);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

        }

        void button_Tool_Add_Click(object sender, EventArgs e)
        {
            _frm1.ToolBarFlag = false;
            Form_Refresh();
            // 根据用户选择不同图层，将该图层设置为可选
            switch (_frm1.comBox_Tool_Layer.SelectedIndex)
            {

                case (int)编辑图层.Road打断:
                    {
                        LayerHelper.SelectLayer(_frm1.axMapControl1, RoadEntity.RoadNm);
                        break;
                    }
                case (int)编辑图层.Segment打断合并:
                    {
                        LayerHelper.SelectLayer(_frm1.axMapControl1, SegmentEntity.SegmentName);
                        break;
                    }
                case (int)编辑图层.基于SegNode的合并:
                    {
                        LayerHelper.SelectLayer(_frm1.axMapControl1, SegmentNodeEntity.RoadSegmentNodeName);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }


        }

        void comBox_Tool_Layer_SelectedIndexChanged(object sender, EventArgs e)
        {
            Form_Refresh();
            //所有图层不可选，除非点击添加按钮，才可以选择某一图层。
            LayerHelper.LayerNotSelect(_frm1.axMapControl1);

            if (_frm1.comBox_Tool_Layer.SelectedIndex == 1 || _frm1.comBox_Tool_Layer.SelectedIndex == 2)
            {
                if (_frm1.FeaClsSegment == null || _frm1.FeaClsSegNode == null)
                {
                    DialogResult dr = MessageBox.Show("打开Segment和SegmentNode图层？", "Warnning", MessageBoxButtons.YesNo);
                    if (dr == DialogResult.Yes)
                    {
                        //选择Road图层
                        _frm1.Wsp = MapComponent.OpenGeoDatabase(_frm1.axMapControl1);
                        _frm1.getAllFeaClses(_frm1.Wsp);
                        if (_frm1.FeaClsSegment == null)
                        { MessageBox.Show("还是没有Segment..."); }
                        else if (_frm1.FeaClsSegNode == null)
                        {
                            MessageBox.Show("还是没有SegmentNode...");
                        }
                    }
                }
            }
            else
            {
                if (_frm1.FeaClsRoad == null)
                {
                    DialogResult dr = MessageBox.Show("打开Road图层？", "Warnning", MessageBoxButtons.YesNo);
                    if (dr == DialogResult.Yes)
                    {
                        //选择Road图层
                        _frm1.Wsp = MapComponent.OpenGeoDatabase(_frm1.axMapControl1);
                        _frm1.getAllFeaClses(_frm1.Wsp);
                    }
                }
            }


        }


        private void layoutSegmentNode()
        {
            if (_frm1.groupBox_Tool_SltAtt.Controls.Count > 0)
            {
                _frm1.groupBox_Tool_SltAtt.Controls.Clear();
            }
            layoutAttBox("第一个道路", "第二个道路");

            #region 第三段
            int label_third_Y = _frm1.richTextBox_Tool_SecondFea.Location.Y + _frm1.richTextBox_Tool_SecondFea.Height + LINEWIDTH;
            //重置groupbox的高度
            _frm1.groupBox_Tool_SltAtt.Height = 2 * label_third_Y;

            _frm1.label_Tool_ThirdFea = new System.Windows.Forms.Label();
            WinFormDesigner.layoutLabel(_frm1.label_Tool_ThirdFea, "第三个路段", _frm1.groupBox_Tool_SltAtt.Controls,
                new Point(5, label_third_Y), System.Windows.Forms.DockStyle.None);

            int rich_Third_Y = _frm1.label_Tool_ThirdFea.Location.Y + _frm1.label_Tool_ThirdFea.Height + 5;

            _frm1.richTextBox_Tool_ThirdFea = new System.Windows.Forms.RichTextBox();

            WinFormDesigner.layoutRichTextBox(_frm1.richTextBox_Tool_ThirdFea, _frm1.groupBox_Tool_SltAtt,
                new Point(0, rich_Third_Y), System.Windows.Forms.DockStyle.None);
            _frm1.richTextBox_Tool_ThirdFea.Width = _frm1.groupBox_Tool_SltAtt.Width - 2;
            _frm1.richTextBox_Tool_ThirdFea.Height = 50;

            _frm1.richTextBox_Tool_ThirdFea.ForeColor = Color.Pink;

            #endregion 第三段


            #region 第四段
            int label_forth_Y = rich_Third_Y + _frm1.richTextBox_Tool_ThirdFea.Height + 10;
            _frm1.label_Tool_ForthFea = new System.Windows.Forms.Label();
            WinFormDesigner.layoutLabel(_frm1.label_Tool_ForthFea, "第四个路段", _frm1.groupBox_Tool_SltAtt.Controls,
                new System.Drawing.Point(5, label_forth_Y), System.Windows.Forms.DockStyle.None);

            int rich_forth_Y = label_forth_Y + _frm1.label_Tool_ForthFea.Height + 5;
            _frm1.richTextBox_Tool_ForthFea = new RichTextBox();
            WinFormDesigner.layoutRichTextBox(_frm1.richTextBox_Tool_ForthFea, _frm1.groupBox_Tool_SltAtt,
                new Point(0, rich_forth_Y), System.Windows.Forms.DockStyle.None);
            _frm1.richTextBox_Tool_ForthFea.Height = 50;
            _frm1.richTextBox_Tool_ForthFea.Width = _frm1.groupBox_Tool_SltAtt.Width - 2;
            _frm1.richTextBox_Tool_ForthFea.ForeColor = Color.Blue;

            #endregion 第四段
            //重置groupbox的高度
            _frm1.groupBox_Tool_SltAtt.Height = _frm1.richTextBox_Tool_ForthFea.Location.Y + _frm1.richTextBox_Tool_ForthFea.Height + 2;
        }

        private void layoutRoad()
        {



            layoutAttBox("第一个路段", "第二个路段");


        }

        private void layoutAttBox(string labelText1, string labelText2)
        {
            _frm1.label_Tool_FirstFea = new System.Windows.Forms.Label();
            WinFormDesigner.layoutLabel(_frm1.label_Tool_FirstFea, labelText1, _frm1.groupBox_Tool_SltAtt.Controls, new Point(5, 25), System.Windows.Forms.DockStyle.None);

            int rich_first_Y = _frm1.label_Tool_FirstFea.Location.Y + _frm1.label_Tool_FirstFea.Height + 5;

            _frm1.richTextBox_Tool_FirstFea = new System.Windows.Forms.RichTextBox();

            WinFormDesigner.layoutRichTextBox(_frm1.richTextBox_Tool_FirstFea, _frm1.groupBox_Tool_SltAtt, new Point(0, rich_first_Y), System.Windows.Forms.DockStyle.None);
            _frm1.richTextBox_Tool_FirstFea.Width = _frm1.groupBox_Tool_SltAtt.Width - 2;
            _frm1.richTextBox_Tool_FirstFea.Height = 50;

            _frm1.richTextBox_Tool_FirstFea.ForeColor = Color.Red;

            int label_second_Y = rich_first_Y + _frm1.richTextBox_Tool_FirstFea.Height + LINEWIDTH;

            //重置groupbox的高度
            _frm1.groupBox_Tool_SltAtt.Height = 2 * label_second_Y;

            _frm1.label_Tool_SecondFea = new System.Windows.Forms.Label();
            WinFormDesigner.layoutLabel(_frm1.label_Tool_SecondFea, labelText2, _frm1.groupBox_Tool_SltAtt.Controls,
                new System.Drawing.Point(5, label_second_Y), System.Windows.Forms.DockStyle.None);

            int rich_second_Y = label_second_Y + _frm1.label_Tool_SecondFea.Height + 5;
            _frm1.richTextBox_Tool_SecondFea = new RichTextBox();
            WinFormDesigner.layoutRichTextBox(_frm1.richTextBox_Tool_SecondFea, _frm1.groupBox_Tool_SltAtt,
                new Point(0, rich_second_Y), System.Windows.Forms.DockStyle.None);

            _frm1.richTextBox_Tool_SecondFea.Height = 50;

            _frm1.richTextBox_Tool_SecondFea.Width = _frm1.groupBox_Tool_SltAtt.Width - 2;
            _frm1.richTextBox_Tool_SecondFea.ForeColor = Color.Green;
            //重置groupbox的高度
            _frm1.groupBox_Tool_SltAtt.Height = _frm1.richTextBox_Tool_SecondFea.Location.Y + 50;
        }
    }
}
