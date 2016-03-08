using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.GIS.Interactive;
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
        private Lane g_currentLane;
        private Arc g_currentArc;
        private Node g_nextNode;
        private IFeature g_nextNodeFeature;

        private IFeature g_currentLaneFeature;

        Dictionary<CheckBox, IElement> g_connElementMap;
            

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
            g_turnArc = new Dictionary<int, IFeature>();
        }

        private void clearModifyGroup()
        {
            if (g_frm1.groupBox_Lane_Rule_Modify != null)
            {
                g_frm1.groupBox_Lane_Rule_Modify.Controls.Clear();
            }
        }

        //设置导向箭头的界面
        public void LayoutTurnarrowRule()
        {
            clearModifyGroup();
            initConnectionRule();
            if (g_frm1.SlctLane_Rule != null)
            {
                LaneFeatureService laneFeatureService = new LaneFeatureService(g_frm1.FeaClsLane, 0);

                g_currentLaneFeature = g_frm1.SlctLane_Rule;
                g_currentLane = laneFeatureService.GetEntity(g_currentLaneFeature);
                ArcService arcService = new ArcService(g_frm1.FeaClsArc,g_currentLane.ArcID);
                g_currentArc = arcService.GetArcEty(arcService.GetArcFeature());
                g_nextNode = PhysicalConnection.getNextNode(g_frm1.FeaClsLink, g_frm1.FeaClsArc, g_frm1.FeaClsNode, g_currentArc);
                NodeService nodeService = new NodeService(g_frm1.FeaClsNode, g_nextNode.ID, null);
                g_nextNodeFeature = nodeService.GetFeature();

                g_connElementMap = new Dictionary<CheckBox, IElement>();
                List<Arc> leftTurnArcs = new List<Arc>();
                List<Arc> rightTurnArcs = new List<Arc>();
                List<Arc> straightTurnArcs = new List<Arc>();
                List<Arc> uturnTurnArcs = new List<Arc>();
                if (null == g_currentLane)
                {
                    //MessageBox.Show("")
                    return;
                }
                //获取下游的Arc
                
                LogicalConnection.GetTurnTurningArcs(g_currentArc,
                    g_frm1.FeaClsLink,
                    g_frm1.FeaClsArc,
                    g_frm1.FeaClsNode,
                    ref leftTurnArcs,
                    ref rightTurnArcs,
                    ref straightTurnArcs,
                    ref uturnTurnArcs);
                if (leftTurnArcs.Count > 0)
                {
                    arcService = new ArcService(g_frm1.FeaClsArc, leftTurnArcs[0].ArcID);
                    g_turningArcLanes = arcService.getLaneWithinArc();
                    if (null == g_turningArcLanes ||
                        0 == g_turningArcLanes.Count)
                    {
                        return;
                    }
                    setNextLanes(g_turningArcLanes);
                }
            }
        }

        /// <summary>
        /// 设置下游有多少个车道
        /// </summary>
        /// <param name="lanes"></param>
        private void setNextLanes(List<Lane> lanes)
        {
            if (null == lanes ||
                0 == lanes.Count)
            {
                return;
            }

            g_frm1.checkBox_Lane_Rule_Turning_Lane1 = new CheckBox();
            int checkBox_x = g_location_x;
            int checkBox_y1 = g_frm1.label_Lane_Rule_Next_Lane_Serial.Location.Y+RuleSettingDesigner.LINE_WIDTH;
            WinFormDesigner.layoutCheckBox(g_frm1.checkBox_Lane_Rule_Turning_Lane1,
                "车道1",
                g_frm1.groupBox_Lane_Rule_Modify.Controls,
                new System.Drawing.Point(checkBox_x,checkBox_y1),
                DockStyle.None);

            g_frm1.checkBox_Lane_Rule_Turning_Lane1.Click += checkBox_Lane_Rule_Turning_Lane1_Click;

            if (lanes.Count > 1 &&
                lanes.Count <= 2)
            {
                g_frm1.checkBox_Lane_Rule_Turning_Lane2 = new CheckBox();

                int checkBox_y2 = checkBox_y1 + 2 * RuleSettingDesigner.LINE_WIDTH;
                WinFormDesigner.layoutCheckBox(g_frm1.checkBox_Lane_Rule_Turning_Lane2,
                    "车道2",
                    g_frm1.groupBox_Lane_Rule_Modify.Controls,
                    new System.Drawing.Point(checkBox_x, checkBox_y2),
                    DockStyle.None);
                g_frm1.checkBox_Lane_Rule_Turning_Lane2.Click += checkBox_Lane_Rule_Turning_Lane2_Click;
            }
            else if (lanes.Count > 2 &&
                lanes.Count <= 3)
            {

                g_frm1.checkBox_Lane_Rule_Turning_Lane3 = new CheckBox();

                int checkBox_y3 = checkBox_y1 + 2 * 2 * RuleSettingDesigner.LINE_WIDTH;
                WinFormDesigner.layoutCheckBox(g_frm1.checkBox_Lane_Rule_Turning_Lane3,
                    "车道3",
                    g_frm1.groupBox_Lane_Rule_Modify.Controls,
                    new System.Drawing.Point(checkBox_x, checkBox_y3),
                    DockStyle.None);
                g_frm1.checkBox_Lane_Rule_Turning_Lane3.Click += checkBox_Lane_Rule_Turning_Lane3_Click;
            }
            else if (lanes.Count > 3 &&
               lanes.Count <= 4)
            {

                g_frm1.checkBox_Lane_Rule_Turning_Lane4 = new CheckBox();

                int checkBox_y4 = checkBox_y1 + 3 * 2 * RuleSettingDesigner.LINE_WIDTH;
                WinFormDesigner.layoutCheckBox(g_frm1.checkBox_Lane_Rule_Turning_Lane4,
                    "车道4",
                    g_frm1.groupBox_Lane_Rule_Modify.Controls,
                    new System.Drawing.Point(checkBox_x, checkBox_y4),
                    DockStyle.None);

                g_frm1.checkBox_Lane_Rule_Turning_Lane4.Click += checkBox_Lane_Rule_Turning_Lane4_Click;
            }
            else if (lanes.Count > 4 &&
               lanes.Count <= 5)
            {

                g_frm1.checkBox_Lane_Rule_Turning_Lane5 = new CheckBox();

                int checkBox_y5 = checkBox_y1 + 4 * 2 * RuleSettingDesigner.LINE_WIDTH;
                WinFormDesigner.layoutCheckBox(g_frm1.checkBox_Lane_Rule_Turning_Lane5,
                    "车道5",
                    g_frm1.groupBox_Lane_Rule_Modify.Controls,
                    new System.Drawing.Point(checkBox_x, checkBox_y5),
                    DockStyle.None);
                g_frm1.checkBox_Lane_Rule_Turning_Lane5.Click += checkBox_Lane_Rule_Turning_Lane5_Click;
            }
            else if (lanes.Count > 5 &&
               lanes.Count <= 6)
            {

                g_frm1.checkBox_Lane_Rule_Turning_Lane6 = new CheckBox();

                int checkBox_y6 = checkBox_y1 + 5 * 2 * RuleSettingDesigner.LINE_WIDTH;
                WinFormDesigner.layoutCheckBox(g_frm1.checkBox_Lane_Rule_Turning_Lane6,
                    "车道6",
                    g_frm1.groupBox_Lane_Rule_Modify.Controls,
                    new System.Drawing.Point(checkBox_x, checkBox_y6),
                    DockStyle.None);

                g_frm1.checkBox_Lane_Rule_Turning_Lane6.Click += checkBox_Lane_Rule_Turning_Lane6_Click;
            }
            else if (lanes.Count > 6 &&
               lanes.Count <= 7)
            {

                g_frm1.checkBox_Lane_Rule_Turning_Lane7 = new CheckBox();

                int checkBox_y7 = checkBox_y1 + 6 * 2 * RuleSettingDesigner.LINE_WIDTH;
                WinFormDesigner.layoutCheckBox(g_frm1.checkBox_Lane_Rule_Turning_Lane7,
                    "车道7",
                    g_frm1.groupBox_Lane_Rule_Modify.Controls,
                    new System.Drawing.Point(checkBox_x, checkBox_y7),
                    DockStyle.None);
                g_frm1.checkBox_Lane_Rule_Turning_Lane7.Click += checkBox_Lane_Rule_Turning_Lane7_Click;
            }
            else if (lanes.Count > 7 &&
               lanes.Count <= 8)
            {

                g_frm1.checkBox_Lane_Rule_Turning_Lane8 = new CheckBox();

                int checkBox_y8 = checkBox_y1 + 7 * 2 * RuleSettingDesigner.LINE_WIDTH;
                WinFormDesigner.layoutCheckBox(g_frm1.checkBox_Lane_Rule_Turning_Lane8,
                    "车道8",
                    g_frm1.groupBox_Lane_Rule_Modify.Controls,
                    new System.Drawing.Point(checkBox_x, checkBox_y8),
                    DockStyle.None);
                g_frm1.checkBox_Lane_Rule_Turning_Lane8.Click += checkBox_Lane_Rule_Turning_Lane8_Click;
            }
        }

        void checkBox_Lane_Rule_Turning_Lane3_Click(object sender, EventArgs e)
        {
            HighLightConnector(g_frm1.checkBox_Lane_Rule_Turning_Lane3, 2);
        }

        void checkBox_Lane_Rule_Turning_Lane4_Click(object sender, EventArgs e)
        {
            HighLightConnector(g_frm1.checkBox_Lane_Rule_Turning_Lane4, 3);
            
        }

        void checkBox_Lane_Rule_Turning_Lane5_Click(object sender, EventArgs e)
        {
            HighLightConnector(g_frm1.checkBox_Lane_Rule_Turning_Lane5, 4);
        }

        void checkBox_Lane_Rule_Turning_Lane6_Click(object sender, EventArgs e)
        {
            HighLightConnector(g_frm1.checkBox_Lane_Rule_Turning_Lane6, 5);
        }

        void checkBox_Lane_Rule_Turning_Lane7_Click(object sender, EventArgs e)
        {
            HighLightConnector(g_frm1.checkBox_Lane_Rule_Turning_Lane7, 6);
        }

        void checkBox_Lane_Rule_Turning_Lane8_Click(object sender, EventArgs e)
        {
            HighLightConnector(g_frm1.checkBox_Lane_Rule_Turning_Lane8, 7);
        }

        void checkBox_Lane_Rule_Turning_Lane2_Click(object sender, EventArgs e)
        {
            HighLightConnector(g_frm1.checkBox_Lane_Rule_Turning_Lane2, 1);
        }

        void checkBox_Lane_Rule_Turning_Lane1_Click(object sender, EventArgs e)
        {
            HighLightConnector(g_frm1.checkBox_Lane_Rule_Turning_Lane1,0);
        }


        private void HighLightConnector(CheckBox checkBox,int laneIndex)
        {
            if (null == g_turningArcLanes ||
                0 == g_turningArcLanes.Count)
            {
                return;
            }
            LaneConnectorFeatureService laneConnectorService = new LaneConnectorFeatureService(g_frm1.FeaClsConnector, 0);
            LaneFeatureService laneFeatureService = new LaneFeatureService(g_frm1.FeaClsLane, g_currentLane.LaneID);
            IFeature fromLaneFeature = laneFeatureService.GetFeature();
            laneFeatureService = new LaneFeatureService(g_frm1.FeaClsLane, g_turningArcLanes[laneIndex].LaneID);
            IFeature toLaneFeature = laneFeatureService.GetFeature();

            IFeature connFeature = laneConnectorService.GetConnectorByLaneIds(g_currentLane.LaneID,
                g_turningArcLanes[laneIndex].LaneID);

            if (null == connFeature)
            {
                bool isStraight = false;
                if (g_frm1.comBox_Lane_Rule_Next_Arcs.SelectedIndex == Convert.ToInt32(RoadNetworkSystem.DataModel.RoadSign.TurnArrow.TURNING_ITEM.直行))
                {
                    isStraight = true;
                }

                IPolyline line = laneConnectorService.getConnectorShape(fromLaneFeature.Shape as IPolyline,
                    toLaneFeature.Shape as IPolyline,
                    g_nextNodeFeature.Shape as IPoint,
                    isStraight);

                if (checkBox.CheckState == CheckState.Checked)
                {
                    IElement pElement = GeoDisplayHelper.HightLine(g_frm1.axMapControl1,
                        line,
                        255,
                        255,
                        0,
                        2,
                        ESRI.ArcGIS.Display.esriSimpleLineStyle.esriSLSDot);
                    g_connElementMap.Add(checkBox, pElement);
                }
                else
                {
                    if (g_connElementMap.ContainsKey(checkBox))
                    {
                        GeoDisplayHelper.ClearElement(g_frm1.axMapControl1,
                            g_connElementMap[checkBox]);
                        g_connElementMap.Remove(checkBox);
                    }
                }
            }
            else
            {
                IPolyline connLine = connFeature.Shape as IPolyline;
                if (checkBox.CheckState == CheckState.Checked)
                {

                    if (g_connElementMap.ContainsKey(checkBox))
                    {
                        GeoDisplayHelper.ClearElement(g_frm1.axMapControl1,
                            g_connElementMap[checkBox]);
                        g_connElementMap.Remove(checkBox);
                    }
                }
                else
                {
                    IElement pElement = GeoDisplayHelper.HightLine(g_frm1.axMapControl1,
                        connLine,
                        100,
                        100,
                        100,
                        2,
                        ESRI.ArcGIS.Display.esriSimpleLineStyle.esriSLSDot);
                    g_connElementMap.Add(checkBox, pElement);
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

            string[] turningArr = Enum.GetNames(typeof(RoadNetworkSystem.DataModel.RoadSign.TurnArrow.TURNING_ITEM));
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
