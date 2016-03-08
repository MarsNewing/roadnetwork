using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.GIS;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace RoadNetworkSystem.WinForm.NetworkEditor
{
    class NetworkEditorDesigner
    {
        /*
        * ----------------------------------
        * 
        * 
        *        panel2
        * 
        * ----------------------------------
        * 
        * 
        * 
        * 
        *        panel3
        * 
        * 
        * 
        * 
        * 
        * ------------------------------------- 
        *       panel1 
        * -------------------------------------
        */


        /*
         * ----------------------------------
         * 
         * 
         *          groupBox_public_Atrr           
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
         *             spltCtn1 
         * -------------------------------------
         */
        private static Form1 _frm1;
        private static string _selcetLayerNm = "";
        private static int BTNHEIGHT = 30; 

        //鼠标行为
        public enum MouseAction
        {
            Node_Add,
            Node_Move,
            Node_Delete,
            Node_Save,
            Link_Add,
            Link_Move,
            Link_Delete,
            Link_Save
        }

        private static void clearParentCtrl()
        {
            WinFormDesigner.ClearPanel(_frm1.panel_Top);
            WinFormDesigner.ClearPanel(_frm1.panel_Bottom);
            WinFormDesigner.ClearPanel(_frm1.panel_Middle);
            //g_frm1.groupBox1.Visible = false;
            _frm1.panel_Top.Visible = true;

            _frm1.panel_Top.Visible = true;
            _frm1.panel_Top.AutoScroll = true;

            //g_frm1.panel_Bottom.Visible = true;
            //g_frm1.splitContainer5.Visible = false;
        }

        private static void setPartsHeight()
        {
            _frm1.panel_Top.Height = 100;
            _frm1.panel_Bottom.Height = 30;
        }

        //拖放界面
        public static void LayoutNetworkEditor(Form1 frm1)
        {
            _frm1 = frm1;

            setPartsHeight();

            #region 顶部属性部分

            clearParentCtrl();
            setPartsHeight();

            _frm1.groupBox1 = new GroupBox();
            WinFormDesigner.setGroupBoxStyle(_frm1.groupBox1, "选择对象属性", new Point(1, 1), DockStyle.Fill, 90, 0);
            _frm1.panel_Top.Controls.Add(_frm1.groupBox1);
            _frm1.richTextBox1 = new RichTextBox();
            WinFormDesigner.layoutRichTextBox(_frm1.richTextBox1, _frm1.groupBox1, new Point(1, 1), DockStyle.Fill);

            #endregion 顶部属性部分

            //设置底部面板样式
            setBottomPnl();


            //设置底部图层选择条
            setLayerList();


            //设置默认值

            _selcetLayerNm = Node.NodeName;
            NodeEditorDesigner.SetNodePalette(_frm1);
            _frm1.comboBox_Layer.SelectedIndex = 0;

            _frm1.comboBox_Layer.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            _frm1.Resize += _frm1_Resize;
        }

        static void _frm1_Resize(object sender, EventArgs e)
        {
            if (_frm1.splitContainer5 != null)
            {
                //保证按钮与图层选择器之间的位置
                _frm1.splitContainer5.SplitterDistance = _frm1.splitContainer5.Width - 4 * _frm1.button_Delete.Width - 2;
            }
        }


        /// <summary>
        /// 设置底部面板样式
        /// </summary>
        private static void setBottomPnl()
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
            _frm1.button_Add.Tag = 1;

            _frm1.button_Refresh = new Button();
            _frm1.button_Refresh.Image = Properties.Resources.reload;
            _frm1.button_Refresh.Width = BTNHEIGHT;
            _frm1.button_Refresh.Dock = DockStyle.Right;

            _frm1.button_Save = new Button();
            _frm1.button_Save.Image = Properties.Resources.save;
            _frm1.button_Save.Width = BTNHEIGHT;
            _frm1.button_Save.Dock = DockStyle.Right;
            _frm1.button_Save.Tag = 1;

            _frm1.button_Delete = new Button();
            _frm1.button_Delete.Image = Properties.Resources.cross;
            _frm1.button_Delete.Width = BTNHEIGHT;
            _frm1.button_Delete.Dock = DockStyle.Right;

            _frm1.splitContainer5.Panel2.Controls.Add(_frm1.button_Add);
            _frm1.splitContainer5.Panel2.Controls.Add(_frm1.button_Refresh);

            _frm1.splitContainer5.Panel2.Controls.Add(_frm1.button_Save);
            _frm1.splitContainer5.Panel2.Controls.Add(_frm1.button_Delete);


            //保证按钮与图层选择器之间的位置
            _frm1.splitContainer5.SplitterDistance = _frm1.splitContainer5.Width - 4 * _frm1.button_Delete.Width - 2;

            _frm1.comboBox_Layer = new ComboBox();
            _frm1.comboBox_Layer.Dock = DockStyle.Fill;
            _frm1.comboBox_Layer.Font = new Font(_frm1.comboBox_Layer.Font.Name, _frm1.comboBox_Layer.Font.Size + 5);
            _frm1.splitContainer5.Panel1.Controls.Add(_frm1.comboBox_Layer);
        }

        private static void setLayerList()
        {
            List<string> itemList = new List<string>();
            itemList.Add(Node.NodeName);
            itemList.Add(Link.LinkName);
            itemList.Add(Lane.LaneName);
            itemList.Add(LaneConnector.ConnectorName);

            foreach (string item in itemList)
            {
                _frm1.comboBox_Layer.Items.Add(item);
            }
        }



        static void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string str = Convert.ToString(_frm1.comboBox_Layer.SelectedItem);
            _frm1.panel_Middle.Controls.Clear();
            _frm1.richTextBox1.Text = "";

            switch (str)
            {
                case Link.LinkName:
                    {
                        _frm1.button_Save.Tag = 2;
                        _frm1.button_Delete.Tag = 2;
                        _frm1.button_Add.Tag = 2;
                        _frm1.button_Refresh.Tag = 2;
                        _selcetLayerNm = Link.LinkName;

                        LayerHelper.ClearSelect(_frm1.axMapControl1);
                        LayerHelper.SelectLayer(_frm1.axMapControl1, Link.LinkName);

                        LinkEditorDesigner.SetLinkPalette(_frm1);
                        break;
                    }
                case Node.NodeName:
                    {
                        _frm1.button_Save.Tag = 1;
                        _frm1.button_Delete.Tag = 1;
                        _frm1.button_Add.Tag = 1;
                        _frm1.button_Refresh.Tag = 1;
                        _selcetLayerNm = Node.NodeName;

                        LayerHelper.ClearSelect(_frm1.axMapControl1);
                        LayerHelper.SelectLayer(_frm1.axMapControl1, Node.NodeName);

                        NodeEditorDesigner.SetNodePalette(_frm1);
                        break;
                    }
                case Lane.LaneName:
                    {
                        _selcetLayerNm = Lane.LaneName;
                        break;

                    }
                case LaneConnector.ConnectorName:
                    {
                        _selcetLayerNm = LaneConnector.ConnectorName;
                        break;
                    }
                default:
                    {
                        _selcetLayerNm = null;
                        break;
                    }
            }
        }



      


    }
}
