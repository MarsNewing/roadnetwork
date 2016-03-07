using ESRI.ArcGIS.Geodatabase;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.Connection;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LaneLayer;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LinkLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RoadNetworkSystem.WinForm.RuleSetting
{
   
    class TurningRule
    {
        private Form1 g_frm1;
        private static int g_location_x = 10;
        private static int g_location_Left_Y = 20;
        /// <summary>
        /// 某个arc方向的车道信息
        /// </summary>
        private List<Lane> g_turningArcLanes;
            

        /*
         * 1：左转
         * 2：直行
         * 3：右转
         * 4：掉头
         * 
         */
        Dictionary<int, IFeature> g_turnArc;

        public TurningRule(Form1 frm1)
        {
            g_frm1 = frm1;
            g_turningArcLanes = new List<Lane>();
        }

        private void clearModifyGroup()
        {
            if (g_frm1.groupBox_Lane_Rule_Modify != null)
            {
                g_frm1.groupBox_Lane_Rule_Modify.Controls.Clear();
            }
        }

        public void LayoutTurnarrowRule()
        {
            clearModifyGroup();
            initConnectionRule();
            if (g_frm1.SlctLane_Rule != null)
            {
                LaneFeatureService laneFeatureService = new LaneFeatureService(g_frm1.FeaClsLane, 0);

                Lane lane = laneFeatureService.GetEntity(g_frm1.SlctLane_Rule);
                List<Arc> leftTurnArcs = new List<Arc>();
                List<Arc> rightTurnArcs = new List<Arc>();
                List<Arc> straightTurnArcs = new List<Arc>();
                List<Arc> uturnTurnArcs = new List<Arc>();
                if (null == lane)
                {
                    //MessageBox.Show("")
                    return;
                }
                //获取下游的Arc
                ArcService arcService = new ArcService(g_frm1.FeaClsArc, lane.ArcID);
                LogicalConnection.GetTurnTurningArcs(arcService.GetArcEty(arcService.GetArcFeature()),
                    g_frm1.FeaClsLink,
                    g_frm1.FeaClsArc,
                    g_frm1.FeaClsNode,
                    ref leftTurnArcs,
                    ref rightTurnArcs,
                    ref straightTurnArcs,
                    ref uturnTurnArcs);
                if (leftTurnArcs.Count > 0)
                {
                    
                }
            }
        }




        private void getNextArcsFeature()
        {

            //Arc[] arcs = LogicalConnection.GetNodeExitArcs(g_frm1.FeaClsLink,g_frm1.FeaClsArc,);
        }

        public void initConnectionRule()
        {
            g_frm1.label_Lane_Rule_Next_Arcs = new System.Windows.Forms.Label();
            WinFormDesigner.layoutLabel(g_frm1.label_Lane_Rule_Next_Arcs, "下游有向子路段：", g_frm1.groupBox_Lane_Rule_Modify.Controls,
                new System.Drawing.Point(g_location_x, g_location_Left_Y), System.Windows.Forms.DockStyle.None);


            int combox_Y = g_location_Left_Y + RuleSettingDesigner.LINE_WIDTH;
            g_frm1.comBox_Lane_Rule_Next_Arcs = new System.Windows.Forms.ComboBox();

            string[] turningArr = {"左转","直行","右转","掉头"};
            List<string> turningDirection = new List<string>();
            turningDirection.AddRange(turningArr);

            WinFormDesigner.layoutComBox(g_frm1.comBox_Lane_Rule_Next_Arcs, g_frm1.groupBox_Lane_Rule_Modify.Controls,
                new System.Drawing.Point(g_location_x*2, combox_Y), turningDirection, System.Windows.Forms.DockStyle.None);

            int location_Label_Lane_Serial_Y = combox_Y + 3 * RuleSettingDesigner.LINE_WIDTH;
            g_frm1.label_Lane_Rule_Next_Lane_Serial = new System.Windows.Forms.Label();
            WinFormDesigner.layoutLabel(g_frm1.label_Lane_Rule_Next_Lane_Serial, "连接车道：", g_frm1.groupBox_Lane_Rule_Modify.Controls,
                new System.Drawing.Point(g_location_x, location_Label_Lane_Serial_Y), System.Windows.Forms.DockStyle.None);

        }
    }
}
