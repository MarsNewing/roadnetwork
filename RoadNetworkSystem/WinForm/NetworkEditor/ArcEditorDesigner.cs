using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace RoadNetworkSystem.WinForm.NetworkEditor
{
    class ArcEditorDesigner
    {
        private static Form1 _frm1;
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
        public static void SetarcPalette(Form1 frm1)
        {
            _frm1 = frm1;

            #region 设置中间编辑面板
            //初始化tabPage_link_core
            _frm1.tabPage_link_core = new TabPage();
            layoutarcCorePage();

            //初始化tabPage_link_Flag
            _frm1.tabPage_link_Flag = new TabPage();
            layoutarcFlagPage();

            List<TabPage> pageList = new List<TabPage>();
            pageList.Add(_frm1.tabPage_link_core);
            pageList.Add(_frm1.tabPage_link_Flag);
            _frm1.tabControl_link = new TabControl();
            WinFormDesigner.layoutTabControl(_frm1.tabControl_link, _frm1.panel_Middle, new Point(1, 1), pageList);

            _frm1.groupBox_Link_BasicAtrr = new GroupBox();
            _frm1.tabPage_link_core.Controls.Add(_frm1.groupBox_Link_BasicAtrr);

            //在Core选项卡中添加一个groupbox,用于管理基本属性

            WinFormDesigner.setGroupBoxStyle(_frm1.groupBox_Link_BasicAtrr, "Basic Attribute", 
                new Point(0, 0), DockStyle.Fill, 150, 0);
            
            // 

            //基础属性编辑框
            setBasicAttGroupBox();


            ////在Core选项卡中添加一个groupbox，用于管理相关操作

            //WinFormDesigner.layoutGroupBox(_frm1.groupBox_Link_Operation, "Operation", _frm1.tabPage_link_core.Controls,
            //    new Point(0, _frm1.groupBox_Link_BasicAtrr.Height), DockStyle.Top, 100,1);


            #endregion 设置中间编辑面板


            _frm1.Resize += _frm1_Resize;
        }

        static void _frm1_Resize(object sender, EventArgs e)
        {
            int textBoxarc_y_x = _frm1.textBox_arc_x.Location.X;
            int textBoxarc_y_y = _frm1.label_arc_y.Location.Y;
            WinFormDesigner.layoutTextBox(_frm1.textBox_link_roadNm, _frm1.groupBox_Link_BasicAtrr.Controls,
                new Point(textBoxarc_y_x, textBoxarc_y_y),
                DockStyle.None);

            int textBoxarc_z_x = _frm1.textBox_arc_x.Location.X;
            int textBoxarc_z_y = _frm1.label_arc_z.Location.Y;
            _frm1.textBox_arc_z = new TextBox();
            WinFormDesigner.layoutTextBox(_frm1.textBox_arc_z, _frm1.groupBox_Link_BasicAtrr.Controls,
                new Point(textBoxarc_z_x, textBoxarc_z_y),
                DockStyle.None);
        }


        /// <summary>
        /// Core选项卡
        /// </summary>
        private static void layoutarcCorePage()
        {
            _frm1.tabPage_link_core.Text = "Core";
            _frm1.tabPage_link_core.AutoScroll = true;


        }


        /// <summary>
        /// Flag选项卡
        /// </summary>
        private static void layoutarcFlagPage()
        {
            _frm1.tabPage_link_Flag.Text = "Flag";
            _frm1.tabPage_link_Flag.AutoScroll = true;


        }

        /// <summary>
        /// 设置中间面板中的Core选项卡中的基本属性
        /// </summary>
        private static void setBasicAttGroupBox()
        {
            //设置arc_X
            _frm1.label_arc_x = new Label();
            WinFormDesigner.layoutLabel(_frm1.label_arc_x, "X坐标", _frm1.groupBox_Link_BasicAtrr.Controls,
                new Point(10, 25), DockStyle.None);

            _frm1.textBox_arc_x = new TextBox();
            WinFormDesigner.layoutTextBox(_frm1.textBox_arc_x, _frm1.groupBox_Link_BasicAtrr.Controls,
                new Point(_frm1.label_arc_x.Location.X, _frm1.label_arc_x.Location.Y),
                DockStyle.Right);


            //设置arc_Y
            _frm1.label_arc_y = new Label();
            int labelarc_y_y = _frm1.label_arc_x.Location.Y + _frm1.label_arc_x.Height + 10;
            WinFormDesigner.layoutLabel(_frm1.label_arc_y, "Y坐标", _frm1.groupBox_Link_BasicAtrr.Controls,
                new Point(10, labelarc_y_y),
                DockStyle.None);

            int textBoxarc_y_x = _frm1.textBox_arc_x.Location.X;
            int textBoxarc_y_y = _frm1.label_arc_y.Location.Y;
            _frm1.textBox_arc_y = new TextBox();
            WinFormDesigner.layoutTextBox(_frm1.textBox_arc_y, _frm1.groupBox_Link_BasicAtrr.Controls,
                new Point(textBoxarc_y_x, textBoxarc_y_y),
                DockStyle.None);


            //设置arc_Y
            _frm1.label_arc_z = new Label();
            int labelarc_z_y = _frm1.label_arc_y.Location.Y + _frm1.label_arc_y.Height + 10;
            WinFormDesigner.layoutLabel(_frm1.label_arc_z, "Y坐标", _frm1.groupBox_Link_BasicAtrr.Controls,
                new Point(10, labelarc_z_y),
                DockStyle.None);

            int textBoxarc_z_x = _frm1.textBox_arc_x.Location.X;
            int textBoxarc_z_y = _frm1.label_arc_z.Location.Y;
            _frm1.textBox_arc_z = new TextBox();
            WinFormDesigner.layoutTextBox(_frm1.textBox_arc_z, _frm1.groupBox_Link_BasicAtrr.Controls,
                new Point(textBoxarc_z_x, textBoxarc_z_y),
                DockStyle.None);
        }
    }
}
