using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace RoadNetworkSystem.WinForm
{
    class WinFormDesigner
    {

        private static Form1 _frm1;

        public static void ClearPanel(Panel pnel)
        {
            pnel.Controls.Clear();
        }
             
       

        #region -----------------控件设置-----------------------
        /// <summary>
        /// 设置Panel
        /// </summary>
        /// <param name="pnl"></param>
        /// <param name="parentPnl"></param>
        /// <param name="pnt"></param>
        public static void layoutPanel(Panel pnl, Panel parentPnl, Point pnt, DockStyle style,int height)
        {
            pnl.Location = pnt;
            pnl.Height = height;
            pnl.Dock = style;
            pnl.AutoScroll = true;
            parentPnl.Controls.Add(pnl);
        }




        /// <summary>
        /// 设置Label
        /// </summary>
        /// <param name="label"></param>
        /// <param name="str"></param>
        /// <param name="parentPnl"></param>
        /// <param name="pnt"></param>
        public static void layoutLabel(Label label, string str, Control.ControlCollection ctrlCol, System.Drawing.Point pnt,DockStyle style)
        {
            label.Text = str;
            label.Location = pnt;
            label.Dock = style;
            ctrlCol.Add(label);
            label.AutoSize = true;
        }

        public static void layoutListItem(ListBox listbox,Control.ControlCollection ctrlCol,System.Drawing.Point pnt,DockStyle style)
        {
            listbox.Location = pnt;
            listbox.Dock = style;
            ctrlCol.Add(listbox);
            listbox.AutoSize = true;
        }

        public static void layoutRadioBtn(RadioButton radioBtn, string str, Control.ControlCollection ctrlCol, System.Drawing.Point pnt, DockStyle style)
        {
            radioBtn.Text = str;
            radioBtn.Location = pnt;
            radioBtn.Dock = style;
            ctrlCol.Add(radioBtn);
        }


        /// <summary>
        /// 设置textbox
        /// </summary>
        /// <param name="label"></param>
        /// <param name="str"></param>
        /// <param name="ctrlCol"></param>
        /// <param name="pnt"></param>
        public static void layoutTextBox(TextBox textBox,Control.ControlCollection ctrlCol, System.Drawing.Point pnt,DockStyle style)
        {
            textBox.Location = pnt;
            textBox.Dock = style;
            ctrlCol.Add(textBox);
        }



        public static void layoutCheckBox(CheckBox checkBox,string text, Control.ControlCollection ctrlCol, System.Drawing.Point pnt, DockStyle style)
        {
            checkBox.Text = text;
            checkBox.Checked = false;
            checkBox.Location = pnt;
            checkBox.Dock = style;
            ctrlCol.Add(checkBox);
        }


        public static void layoutNumberupdown(NumericUpDown updown, int initNum, Control.ControlCollection ctrlCol, System.Drawing.Point pnt)
        {
            updown.Location = pnt;
            updown.Value = initNum;
            updown.Increment = 1;
            ctrlCol.Add(updown);
        }

        /// <summary>
        /// //设置Combox
        /// </summary>
        /// <param name="combox"></param>
        /// <param name="ctrlCol"></param>
        /// <param name="locPnt"></param>
        /// <param name="itemList"></param>
        public static void layoutComBox(ComboBox combox, Control.ControlCollection ctrlCol, Point locPnt, List<string> itemList,DockStyle style)
        {
            combox.Dock = style;
            combox.Location = locPnt;
            combox.Font=new Font(combox.Font.Name,combox.Font.Size+2,combox.Font.Style);
            foreach (string it in itemList)
            {
                combox.Items.Add(it);
            }
            combox.SelectedIndex = 0;
            ctrlCol.Add(combox);
        }


        /// <summary>
        /// 设置选择框
        /// </summary>
        /// <param name="linkTabControl"></param>
        /// <param name="parentPnl"></param>
        /// <param name="pnt"></param>
        /// <param name="pageList"></param>
        public static void layoutTabControl(TabControl linkTabControl, Panel parentPnl, Point pnt, List<TabPage> pageList)
        {
            linkTabControl.Location = pnt;

            linkTabControl.Dock = DockStyle.Fill;
            linkTabControl.Alignment = TabAlignment.Top;
            foreach (TabPage item in pageList)
            {
                linkTabControl.TabPages.Add(item);
            }
            parentPnl.Controls.Add(linkTabControl);

        }

        /// <summary>
        /// 设置groupBox
        /// </summary>
        /// <param name="atrrGroupBox"></param>
        /// <param name="str"></param>
        /// <param name="parentPnl"></param>
        /// <param name="pnt"></param>
        public static void setGroupBoxStyle(GroupBox atrrGroupBox, string str, 
            Point pnt, DockStyle style,int height,int tabIndex)
        {
            atrrGroupBox.Location = pnt;
            atrrGroupBox.Text = str;
            atrrGroupBox.Dock = style;
            atrrGroupBox.Height = height;
            atrrGroupBox.TabIndex = tabIndex;
            //ctrlCol.Add(atrrGroupBox);
        }


        public static void layoutSplitContainer(SplitContainer spltCtn, Orientation ore, int distance, 
            Panel parentPnl, DockStyle style)
        {
            spltCtn.Orientation = ore;
            spltCtn.SplitterDistance = distance;
            spltCtn.Dock = style;
            parentPnl.Controls.Add(spltCtn);
        }


        public static void layoutRichTextBox(RichTextBox atrrRichTextBox, GroupBox parentGroupBox, Point pnt, DockStyle style)
        {
            atrrRichTextBox.Location = pnt;
            
            atrrRichTextBox.Dock = style;
            parentGroupBox.Controls.Add(atrrRichTextBox);
        }

        public static void layoutButton(Button btn,string text, Control.ControlCollection ctrlCol, Point pnt, DockStyle style, int height, int width)
        {
            btn.Location = pnt;
            btn.Height = height;
            btn.Text = text;
            btn.Width = width;
            btn.AutoSize = true;
            btn.Dock = style;
            ctrlCol.Add(btn);
        }
        #endregion -----------------控件设置-----------------------

    }
}
