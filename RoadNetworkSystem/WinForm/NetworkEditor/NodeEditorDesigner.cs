using AxESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using IntersectionModel.GIS;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.GIS;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LinkLayer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace RoadNetworkSystem.WinForm
{
    class NodeEditorDesigner
    {
        private static Form1 _frm1;

        private const int LINEWEIGTH = 15;

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
        public static void SetNodePalette(Form1 frm1)
        {
            _frm1 = frm1;
            #region ---------------------------------设置中间编辑面板---------------------------------
            //初始化tabPage_link_core
            _frm1.tabPage_link_core = new TabPage();
            layoutNodeCorePage();

            //初始化tabPage_link_Flag
            _frm1.tabPage_link_Flag = new TabPage();
            layoutNodeFlagPage();

            List<TabPage> pageList = new List<TabPage>();
            pageList.Add(_frm1.tabPage_link_core);
            pageList.Add(_frm1.tabPage_link_Flag);
            _frm1.tabControl_link = new TabControl();
            WinFormDesigner.layoutTabControl(_frm1.tabControl_link, _frm1.panel_Middle, new System.Drawing.Point(1, 1), pageList);

            _frm1.groupBox_node_BasicAtrr = new GroupBox();
            _frm1.tabPage_link_core.Controls.Add(_frm1.groupBox_node_BasicAtrr);

            //在Core选项卡中添加一个groupbox,用于管理基本属性

            WinFormDesigner.setGroupBoxStyle(_frm1.groupBox_node_BasicAtrr, "Basic Attribute",
                new System.Drawing.Point(0, 0), DockStyle.Fill, 150, 0);
            //MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM


            //基础属性编辑框
            setBasicAttGroupBox();
            _frm1.Resize += _frm1_Resize;

            #endregion ---------------------------------设置中间编辑面板---------------------------------


            #region --------------------------------交互----------------------------------
            //添加Node

            _frm1.button_Add.Click += button_Add_Click;
            //删除Node

            _frm1.button_Delete.Click += button_Delete_Click;
            //保存Node

            _frm1.button_Save.Click += button_Save_Click;

            _frm1.button_Refresh.Click += button_Refresh_Click;


            _frm1.axMapControl1.OnKeyDown += axMapControl1_OnKeyDown;
            #endregion --------------------------------交互----------------------------------


        }

        static void button_Refresh_Click(object sender, EventArgs e)
        {
            if (Convert.ToInt32(_frm1.button_Refresh.Tag) == 1)
            {
                //刷新后，默认选择Link图层
                LayerHelper.ClearSelect(_frm1.axMapControl1);
                LayerHelper.SelectLayer(_frm1.axMapControl1, NodeEntity.NodeName);

                //刷新所有的被选中的东东
                IGraphicsContainer pGraphicsContainer = _frm1.axMapControl1.ActiveView as IGraphicsContainer;//把地图的当前view作为图片的容器
                pGraphicsContainer.DeleteAllElements();//显示储存在 IElement 中图形，这样就持久化了。
                _frm1.axMapControl1.Refresh();

                _frm1.CrtPnt = null;
            }
        }

        static void button_Save_Click(object sender, EventArgs e)
        {
            if (Convert.ToInt32(_frm1.button_Save.Tag) == 1)
            {

                _frm1.axMapControl1.MousePointer = esriControlsMousePointer.esriPointerArrow;
                //不准添加了
                _mouseFlag = (int)RoadNetworkSystem.WinForm.NetworkEditor.NetworkEditorDesigner.MouseAction.Node_Save;

                if (_frm1.CrtPnt != null)
                {

                    //保存Node
                    Node node = new Node(_frm1.FeaClsNode, 0, _frm1.CrtPnt);
                    //MessageBox.Show((_frm1.FeaClsNode as IDataset).Workspace.PathName);
                    NodeEntity nodeEty = new NodeEntity();
                    nodeEty.ID = 0;
                    
                    //默认是非交叉口结点，但是，在加入路段后，更新Adj数据后，需要更新这个字段
                    nodeEty.NodeType = 0;
                    nodeEty.CompositeType = 1;
                    nodeEty.Other = 0;
                    try
                    {
                        node.CreateNode(nodeEty);
                        _frm1.CrtPnt = null;
                        _frm1.axMapControl1.Refresh();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }

            }
        }

        static void button_Delete_Click(object sender, EventArgs e)
        {
            _frm1.axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
            _mouseFlag = (int)RoadNetworkSystem.WinForm.NetworkEditor.NetworkEditorDesigner.MouseAction.Node_Delete;

        }

        static void axMapControl1_OnKeyDown(object sender, IMapControlEvents2_OnKeyDownEvent e)
        {
            //if (e.keyCode == (int)Keys.ControlKey)
            //{
            //    MessageBox.Show("you press ctrl");
            //}
        }

        static void button_Add_Click(object sender, EventArgs e)
        {
            
            if (Convert.ToInt32(_frm1.button_Add.Tag) == 1)
            {
                LayerHelper.ClearSelect(_frm1.axMapControl1);
                _frm1.axMapControl1.MousePointer = esriControlsMousePointer.esriPointerPencil;
                _mouseFlag = (int)RoadNetworkSystem.WinForm.NetworkEditor.NetworkEditorDesigner.MouseAction.Node_Add;
                _frm1.ToolBarFlag = false;
            }
        }




        static void _frm1_Resize(object sender, EventArgs e)
        {
            if (Convert.ToString(_frm1.comboBox_Layer.SelectedItem).Equals(NodeEntity.NodeName))
            {

                int textBoxNode_y_x = _frm1.textBox_node_x.Location.X;
                int textBoxNode_y_y = _frm1.label_node_y.Location.Y;

                WinFormDesigner.layoutTextBox(_frm1.textBox_node_y, _frm1.groupBox_node_BasicAtrr.Controls,
                    new System.Drawing.Point(textBoxNode_y_x, textBoxNode_y_y),
                    DockStyle.None);


                int textBoxNode_z_x = _frm1.textBox_node_x.Location.X;
                int textBoxNode_z_y = _frm1.label_node_z.Location.Y + 3;

                WinFormDesigner.layoutTextBox(_frm1.textBox_node_z, _frm1.groupBox_node_BasicAtrr.Controls,
                    new System.Drawing.Point(textBoxNode_z_x, textBoxNode_z_y),
                    DockStyle.None);
            }
        }




        /// <summary>
        /// Core选项卡
        /// </summary>
        private static void layoutNodeCorePage()
        {
            _frm1.tabPage_link_core.Text = "Core";
            _frm1.tabPage_link_core.AutoScroll = true;


        }


        /// <summary>
        /// Flag选项卡
        /// </summary>
        private static void layoutNodeFlagPage()
        {
            _frm1.tabPage_link_Flag.Text = "Flag";
            _frm1.tabPage_link_Flag.AutoScroll = true;


        }

        /// <summary>
        /// 设置中间面板中的Core选项卡中的基本属性
        /// </summary>
        private static void setBasicAttGroupBox()
        {
            //设置Node_X
            _frm1.label_node_x = new Label();
            _frm1.label_node_y = new Label();
            _frm1.label_node_z = new Label();

            _frm1.groupBox_node_BasicAtrr.Controls.Add(_frm1.label_node_z);
            _frm1.groupBox_node_BasicAtrr.Controls.Add(_frm1.label_node_y);
            _frm1.groupBox_node_BasicAtrr.Controls.Add(_frm1.label_node_x);

            _frm1.textBox_node_x = new TextBox();
            _frm1.textBox_node_y = new TextBox();
            _frm1.textBox_node_z = new TextBox();

            _frm1.groupBox_node_BasicAtrr.Controls.Add(_frm1.textBox_node_z);
            _frm1.groupBox_node_BasicAtrr.Controls.Add(_frm1.textBox_node_y);
            _frm1.groupBox_node_BasicAtrr.Controls.Add(_frm1.textBox_node_x);


            WinFormDesigner.layoutLabel(_frm1.label_node_x, "X坐标", _frm1.groupBox_node_BasicAtrr.Controls,
                new System.Drawing.Point(10, 25), DockStyle.None);
            _frm1.label_node_x.Width = Convert.ToInt32(_frm1.label_node_x.Font.Size * 4);


            WinFormDesigner.layoutTextBox(_frm1.textBox_node_x, _frm1.groupBox_node_BasicAtrr.Controls,
                new System.Drawing.Point(_frm1.label_node_x.Location.X, _frm1.label_node_x.Location.Y + 3),
                DockStyle.Right);
            

            //设置Node_Y

            int labelNode_y_y = _frm1.label_node_x.Location.Y + _frm1.label_node_x.Height + LINEWEIGTH;
            WinFormDesigner.layoutLabel(_frm1.label_node_y, "Y坐标", _frm1.groupBox_node_BasicAtrr.Controls,
                new System.Drawing.Point(10, labelNode_y_y),
                DockStyle.None);
            _frm1.label_node_y.Width = Convert.ToInt32(_frm1.label_node_y.Font.Size * 4);


            int textBoxNode_y_x = _frm1.textBox_node_x.Location.X;
            int textBoxNode_y_y = _frm1.label_node_y.Location.Y;

            WinFormDesigner.layoutTextBox(_frm1.textBox_node_y, _frm1.groupBox_node_BasicAtrr.Controls,
                new System.Drawing.Point(textBoxNode_y_x, textBoxNode_y_y),
                DockStyle.None);
            


            //设置Node_z

            int labelNode_z_y = _frm1.label_node_y.Location.Y + _frm1.label_node_y.Height + LINEWEIGTH;
            WinFormDesigner.layoutLabel(_frm1.label_node_z, "Z坐标", _frm1.groupBox_node_BasicAtrr.Controls,
                new System.Drawing.Point(10, labelNode_z_y),
                DockStyle.None);
            _frm1.label_node_z.Width = Convert.ToInt32(_frm1.label_node_z.Font.Size * 4);


            int textBoxNode_z_x = _frm1.textBox_node_x.Location.X;
            int textBoxNode_z_y = _frm1.label_node_z.Location.Y + 3;

            WinFormDesigner.layoutTextBox(_frm1.textBox_node_z, _frm1.groupBox_node_BasicAtrr.Controls,
                new System.Drawing.Point(textBoxNode_z_x, textBoxNode_z_y),
                DockStyle.None);
        }

        //private static void setNodeTypeCombox()
        //{

        //}

        //private static void setOperationGroupBox()
        //{
        //    _frm1.button_link_split = new Button();
        //    WinFormDesigner.layoutButton(_frm1.button_link_split, "打断", _frm1.groupBox_Link_Operation.Controls,
        //        new System.Drawing.Point(_frm1.label_link_roadType.Location.X, 30), DockStyle.None, 20, 50);

        //    _frm1.button_link_merge = new Button();
        //    int btn_merge_X = _frm1.label_link_roadType.Location.X + _frm1.button_link_split.Width + 20;
        //    WinFormDesigner.layoutButton(_frm1.button_link_merge, "合并", _frm1.groupBox_Link_Operation.Controls,
        //        new System.Drawing.Point(btn_merge_X, 30), DockStyle.None, 20, 50);

        //    _frm1.groupBox_Link_Operation.Height = _frm1.button_link_split.Location.Y + _frm1.button_link_split.Height + 30;
        //}

    }
}
