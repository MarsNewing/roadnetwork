using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.RoadSign;
using RoadNetworkSystem.GIS.Interactive;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.Connection;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LaneLayer;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LinkLayer;
using RoadNetworkSystem.NetworkElement.RoadSignElement;
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

        public const string groupBox_Lane_Rule_Modify_Text = "车道转向";

        private Form1 g_frm1;
        private static int g_location_x = 10;
        private static int g_location_Left_Y = 20;
        /// <summary>
        /// 某个arc方向的车道信息
        /// </summary>
        private static List<Lane> g_turningLanesWithinArc;
        private static Lane g_currentLane;
        private static Arc g_currentArc;
        private static Node g_nextNode;
        private static IFeature g_nextNodeFeature;

        private static IFeature g_currentLaneFeature;

        /// <summary>
        /// 保存被选中的checkbox 与 车道连接器几何 的键值对，用于渲染与清除渲染
        /// </summary>
        private static Dictionary<CheckBox, IElement> g_connElementMap;

        /// <summary>
        /// 连通的车道，key：对应 转向的编号，value各个转向的车道列表
        /// </summary>
        private static Dictionary<int, List<Lane>> g_connectedLanesMap;

        private static Dictionary<int, List<Lane>> g_deletedConnectedLanesMap;

        public TurningRule(Form1 frm1)
        {
            
            g_frm1 = frm1;
            GeoDisplayHelper.Refresh(g_frm1.axMapControl1);
            g_turningLanesWithinArc = new List<Lane>();
            g_connectedLanesMap = new Dictionary<int, List<Lane>>();
            g_deletedConnectedLanesMap = new Dictionary<int, List<Lane>>();
        }


        void init()
        {
            g_turningLanesWithinArc = new List<Lane>();
            g_currentLane = new Lane();
            g_currentArc = new Arc();
            
            g_nextNode = new Node();
            g_nextNodeFeature = null;
            g_currentLaneFeature = null;

            g_connElementMap = new Dictionary<CheckBox, IElement>();
            g_connectedLanesMap = new Dictionary<int, List<Lane>>();
            g_deletedConnectedLanesMap = new Dictionary<int, List<Lane>>();
            //刷新地图
            GeoDisplayHelper.Refresh(g_frm1.axMapControl1);
        }

        /// <summary>
        /// 设置导向箭头的界面
        /// </summary>
        public void LayoutTurnarrowRule()
        {
            clearModifyGroup();
            updatePublicControls();
            layoutTurningArcControls();

            clearTurningLaneGroup();
            layoutTurningLanesControls();
        }


        private void clearModifyGroup()
        {
            if (g_frm1.groupBox_Lane_Rule_Modify != null)
            {
                g_frm1.groupBox_Lane_Rule_Modify.Controls.Clear();
            }
        }

        private void clearTurningLaneGroup()
        {
            if (g_frm1.groupBox_Lane_Rule_Turn_Lanes!=null)
            {
                g_frm1.groupBox_Lane_Rule_Turn_Lanes.Controls.Clear();
                //坑爹啊，要释放之前用过的控件
                g_frm1.groupBox_Lane_Rule_Turn_Lanes.Dispose();
                if (g_frm1.checkBox_Lane_Rule_Turning_Lane1 != null)
                {
                    g_frm1.checkBox_Lane_Rule_Turning_Lane1.Dispose();
                }

                if (g_frm1.checkBox_Lane_Rule_Turning_Lane2 != null)
                {
                    g_frm1.checkBox_Lane_Rule_Turning_Lane2.Dispose();
                }

                if (g_frm1.checkBox_Lane_Rule_Turning_Lane3 != null)
                {
                    g_frm1.checkBox_Lane_Rule_Turning_Lane3.Dispose();
                }

                if (g_frm1.checkBox_Lane_Rule_Turning_Lane4 != null)
                {
                    g_frm1.checkBox_Lane_Rule_Turning_Lane4.Dispose();
                }


                if (g_frm1.checkBox_Lane_Rule_Turning_Lane5 != null)
                {
                    g_frm1.checkBox_Lane_Rule_Turning_Lane5.Dispose();
                }


                if (g_frm1.checkBox_Lane_Rule_Turning_Lane6 != null)
                {
                    g_frm1.checkBox_Lane_Rule_Turning_Lane6.Dispose();
                }

                if (g_frm1.checkBox_Lane_Rule_Turning_Lane7 != null)
                {
                    g_frm1.checkBox_Lane_Rule_Turning_Lane7.Dispose();
                }

                if (g_frm1.checkBox_Lane_Rule_Turning_Lane8 != null)
                {
                    g_frm1.checkBox_Lane_Rule_Turning_Lane8.Dispose();
                }
            }
        }

        /// <summary>
        /// 更新与其他规则通用的控件属性
        /// </summary>
        private void updatePublicControls()
        {
            g_frm1.groupBox_Lane_Rule_Modify.Text = groupBox_Lane_Rule_Modify_Text;
            g_frm1.spltCtn_Rule_Setting_Att.SplitterDistance = 60;
        }

        /// <summary>
        /// 转向面板
        /// </summary>
        private void layoutTurningArcControls()
        {   
            int combox_Y = g_location_Left_Y;
            g_frm1.comBox_Lane_Rule_Next_Arcs = new System.Windows.Forms.ComboBox();

            string[] turningArr = Enum.GetNames(typeof(RoadNetworkSystem.DataModel.RoadSign.TurnArrow.TURNING_ITEM));
            List<string> turningDirection = new List<string>();
            turningDirection.AddRange(turningArr);

            WinFormDesigner.layoutComBox(g_frm1.comBox_Lane_Rule_Next_Arcs, g_frm1.groupBox_Lane_Rule_Modify.Controls,
                new System.Drawing.Point(g_location_x * 2, combox_Y), turningDirection, System.Windows.Forms.DockStyle.None);

            g_frm1.comBox_Lane_Rule_Next_Arcs.SelectedIndexChanged += ComBox_Lane_Rule_Next_Arcs_SelectedIndexChanged;
        }
        
        /// <summary>
        /// 转向车道面板
        /// </summary>
        private void layoutTurningLanesControls()
        {
            int location_Label_Lane_Serial_Y = g_frm1.groupBox_Lane_Rule_Modify.Location.Y + g_frm1.groupBox_Lane_Rule_Modify.Height;

            g_frm1.groupBox_Lane_Rule_Turn_Lanes = new GroupBox();
            WinFormDesigner.setGroupBoxStyle(g_frm1.groupBox_Lane_Rule_Turn_Lanes,
                "连接车道：",
                 new System.Drawing.Point(g_location_x, location_Label_Lane_Serial_Y),
                 DockStyle.Fill,
                 40,
                 1);
            g_frm1.spltCtn_Rule_Setting_Att.Panel2.Controls.Add(g_frm1.groupBox_Lane_Rule_Turn_Lanes);



            

            #region
           
            if (g_frm1.SlctLane_Rule != null)
            {
                //test();
                LaneFeatureService laneFeatureService = new LaneFeatureService(g_frm1.FeaClsLane, 0);
                g_currentLaneFeature = g_frm1.SlctLane_Rule;
                g_currentLane = laneFeatureService.GetEntity(g_currentLaneFeature);
                if (null == g_currentLane)
                {
                    return;
                }

                //这里不行呦呦~~~~~~~~~~~~~
                //test();


                ArcService arcService = new ArcService(g_frm1.FeaClsArc, g_currentLane.ArcID);
                g_currentArc = arcService.GetArcEty(arcService.GetArcFeature());

                g_nextNode = PhysicalConnection.getNextNode(g_frm1.FeaClsLink, g_frm1.FeaClsArc, g_frm1.FeaClsNode, g_currentArc);
                NodeService nodeService = new NodeService(g_frm1.FeaClsNode, g_nextNode.ID, null);
                g_nextNodeFeature = nodeService.GetFeature();

                g_connElementMap = new Dictionary<CheckBox, IElement>();
                List<Arc> leftTurnArcs = new List<Arc>();
                List<Arc> rightTurnArcs = new List<Arc>();
                List<Arc> straightTurnArcs = new List<Arc>();
                List<Arc> uturnTurnArcs = new List<Arc>();

                //获取下游的Arc
                LogicalConnection.GetTurnTurningArcs(g_currentArc,
                    g_frm1.FeaClsLink,
                    g_frm1.FeaClsArc,
                    g_frm1.FeaClsNode,
                    ref leftTurnArcs,
                    ref rightTurnArcs,
                    ref straightTurnArcs,
                    ref uturnTurnArcs);

                //获取下游的Arc对应的 ArcService
                arcService = getArcService(leftTurnArcs,
                    straightTurnArcs,
                    rightTurnArcs,
                    uturnTurnArcs);

                g_turningLanesWithinArc = arcService.getLanesWithinArc();
                if (null == g_turningLanesWithinArc ||
                    0 == g_turningLanesWithinArc.Count)
                {
                    return;
                }
                if (g_connectedLanesMap.Count==0)
                {
                    g_connectedLanesMap = getConnectedLanes();
                }
                layoutNextLanesCheckBox(g_turningLanesWithinArc);
            }
            #endregion

            //这里行
            //test();
        }


        private ArcService getArcService(List<Arc> leftTurnArcs,
            List<Arc> straightTurnArcs,
            List<Arc> rightTurnArcs,
            List<Arc> uturnTurnArcs)
        {
            if (g_frm1.comBox_Lane_Rule_Next_Arcs.SelectedIndex == Convert.ToInt32(DataModel.RoadSign.TurnArrow.TURNING_ITEM.左转) &&
                leftTurnArcs.Count > 0)
            {
                return new ArcService(g_frm1.FeaClsArc, leftTurnArcs[0].ArcID);
            }
            else if (g_frm1.comBox_Lane_Rule_Next_Arcs.SelectedIndex == Convert.ToInt32(DataModel.RoadSign.TurnArrow.TURNING_ITEM.直行) &&
                straightTurnArcs.Count > 0)
            {
                return new ArcService(g_frm1.FeaClsArc, straightTurnArcs[0].ArcID);
            }
            else if (g_frm1.comBox_Lane_Rule_Next_Arcs.SelectedIndex == Convert.ToInt32(DataModel.RoadSign.TurnArrow.TURNING_ITEM.右转) &&
                rightTurnArcs.Count > 0)
            {
                return new ArcService(g_frm1.FeaClsArc, rightTurnArcs[0].ArcID);
            }
            else if (g_frm1.comBox_Lane_Rule_Next_Arcs.SelectedIndex == Convert.ToInt32(DataModel.RoadSign.TurnArrow.TURNING_ITEM.掉头) &&
               uturnTurnArcs.Count > 0)
            {
                return new ArcService(g_frm1.FeaClsArc, uturnTurnArcs[0].ArcID);
            }
            else {
                return null;
            }
        }

        private Dictionary<int,List<Lane>> getConnectedLanes()
        {
            LaneConnectorFeatureService connectorService = new LaneConnectorFeatureService(g_frm1.FeaClsConnector, 0);
            if (null == g_currentLane)
            {
                return null;
            }
            return connectorService.GetTurningLaneDictionary(g_currentLane.LaneID);
        }
        
        private void layoutNextLanesCheckBox(List<Lane> lanes)
        {
            if (null == lanes ||
                0 == lanes.Count)
            {
                return;
            }



            g_frm1.checkBox_Lane_Rule_Turning_Lane1 = new CheckBox();
            int checkBox_x = g_location_x;
            int checkBox_y1 = g_location_Left_Y;

            WinFormDesigner.layoutCheckBox(g_frm1.checkBox_Lane_Rule_Turning_Lane1,
                "车道1",
                g_frm1.groupBox_Lane_Rule_Turn_Lanes.Controls,
                new System.Drawing.Point(checkBox_x, checkBox_y1),
                DockStyle.None);

            g_frm1.checkBox_Lane_Rule_Turning_Lane1.Click += checkBox_Lane_Rule_Turning_Lane1_Click;
            setCheckBoxState(g_frm1.checkBox_Lane_Rule_Turning_Lane1,
                Lane.LEFT_POSITION,
                g_connectedLanesMap);


            if (null != g_frm1.button_Lane_Rule_Modify)
            {
                g_frm1.button_Lane_Rule_Modify.Dispose();
            }
            g_frm1.button_Lane_Rule_Modify = new Button();
            WinFormDesigner.layoutButton(g_frm1.button_Lane_Rule_Modify, "修改转向",
                g_frm1.groupBox_Lane_Rule_Turn_Lanes.Controls,
                new System.Drawing.Point(checkBox_x + g_frm1.checkBox_Lane_Rule_Turning_Lane1.Width + 20, checkBox_y1),
                DockStyle.None,
                RuleSettingDesigner.BTNHEIGHT,30);
            g_frm1.button_Lane_Rule_Modify.Click += button_Lane_Rule_Modify_Click;

            if (lanes.Count <= 2)
            {
                g_frm1.checkBox_Lane_Rule_Turning_Lane2 = new CheckBox();

                int checkBox_y2 = checkBox_y1 + 2 * RuleSettingDesigner.LINE_WIDTH;
                WinFormDesigner.layoutCheckBox(g_frm1.checkBox_Lane_Rule_Turning_Lane2,
                    "车道2",
                    g_frm1.groupBox_Lane_Rule_Turn_Lanes.Controls,
                    new System.Drawing.Point(checkBox_x, checkBox_y2),
                    DockStyle.None);
                g_frm1.checkBox_Lane_Rule_Turning_Lane2.Click += checkBox_Lane_Rule_Turning_Lane2_Click;

                setCheckBoxState(g_frm1.checkBox_Lane_Rule_Turning_Lane2,
                Lane.LEFT_POSITION + 1,
                g_connectedLanesMap);

            }
            else if (lanes.Count <= 3)
            {

                g_frm1.checkBox_Lane_Rule_Turning_Lane3 = new CheckBox();

                int checkBox_y3 = checkBox_y1 + 2 * 2 * RuleSettingDesigner.LINE_WIDTH;
                WinFormDesigner.layoutCheckBox(g_frm1.checkBox_Lane_Rule_Turning_Lane3,
                    "车道3",
                    g_frm1.groupBox_Lane_Rule_Turn_Lanes.Controls,
                    new System.Drawing.Point(checkBox_x, checkBox_y3),
                    DockStyle.None);

                setCheckBoxState(g_frm1.checkBox_Lane_Rule_Turning_Lane3,
                Lane.LEFT_POSITION + 2,
                g_connectedLanesMap);

                g_frm1.checkBox_Lane_Rule_Turning_Lane3.Click += checkBox_Lane_Rule_Turning_Lane3_Click;
            }
            else if (lanes.Count <= 4)
            {

                g_frm1.checkBox_Lane_Rule_Turning_Lane4 = new CheckBox();

                int checkBox_y4 = checkBox_y1 + 3 * 2 * RuleSettingDesigner.LINE_WIDTH;
                WinFormDesigner.layoutCheckBox(g_frm1.checkBox_Lane_Rule_Turning_Lane4,
                    "车道4",
                    g_frm1.groupBox_Lane_Rule_Turn_Lanes.Controls,
                    new System.Drawing.Point(checkBox_x, checkBox_y4),
                    DockStyle.None);

                g_frm1.checkBox_Lane_Rule_Turning_Lane4.Click += checkBox_Lane_Rule_Turning_Lane4_Click;

                setCheckBoxState(g_frm1.checkBox_Lane_Rule_Turning_Lane4,
                Lane.LEFT_POSITION + 3,
                g_connectedLanesMap);

            }
            else if (lanes.Count <= 5)
            {

                g_frm1.checkBox_Lane_Rule_Turning_Lane5 = new CheckBox();

                int checkBox_y5 = checkBox_y1 + 4 * 2 * RuleSettingDesigner.LINE_WIDTH;
                WinFormDesigner.layoutCheckBox(g_frm1.checkBox_Lane_Rule_Turning_Lane5,
                    "车道5",
                    g_frm1.groupBox_Lane_Rule_Turn_Lanes.Controls,
                    new System.Drawing.Point(checkBox_x, checkBox_y5),
                    DockStyle.None);
                g_frm1.checkBox_Lane_Rule_Turning_Lane5.Click += checkBox_Lane_Rule_Turning_Lane5_Click;

                setCheckBoxState(g_frm1.checkBox_Lane_Rule_Turning_Lane5,
                Lane.LEFT_POSITION + 5,
                g_connectedLanesMap);

            }
            else if (lanes.Count <= 6)
            {

                g_frm1.checkBox_Lane_Rule_Turning_Lane6 = new CheckBox();

                int checkBox_y6 = checkBox_y1 + 5 * 2 * RuleSettingDesigner.LINE_WIDTH;
                WinFormDesigner.layoutCheckBox(g_frm1.checkBox_Lane_Rule_Turning_Lane6,
                    "车道6",
                    g_frm1.groupBox_Lane_Rule_Turn_Lanes.Controls,
                    new System.Drawing.Point(checkBox_x, checkBox_y6),
                    DockStyle.None);

                g_frm1.checkBox_Lane_Rule_Turning_Lane6.Click += checkBox_Lane_Rule_Turning_Lane6_Click;

                setCheckBoxState(g_frm1.checkBox_Lane_Rule_Turning_Lane6,
                Lane.LEFT_POSITION + 6,
                g_connectedLanesMap);

            }
            else if (lanes.Count <= 7)
            {

                g_frm1.checkBox_Lane_Rule_Turning_Lane7 = new CheckBox();

                int checkBox_y7 = checkBox_y1 + 6 * 2 * RuleSettingDesigner.LINE_WIDTH;
                WinFormDesigner.layoutCheckBox(g_frm1.checkBox_Lane_Rule_Turning_Lane7,
                    "车道7",
                    g_frm1.groupBox_Lane_Rule_Turn_Lanes.Controls,
                    new System.Drawing.Point(checkBox_x, checkBox_y7),
                    DockStyle.None);
                g_frm1.checkBox_Lane_Rule_Turning_Lane7.Click += checkBox_Lane_Rule_Turning_Lane7_Click;

                setCheckBoxState(g_frm1.checkBox_Lane_Rule_Turning_Lane7,
                Lane.LEFT_POSITION + 6,
                g_connectedLanesMap);

            }
            else if (lanes.Count <= 8)
            {

                g_frm1.checkBox_Lane_Rule_Turning_Lane8 = new CheckBox();

                int checkBox_y8 = checkBox_y1 + 7 * 2 * RuleSettingDesigner.LINE_WIDTH;
                WinFormDesigner.layoutCheckBox(g_frm1.checkBox_Lane_Rule_Turning_Lane8,
                    "车道8",
                    g_frm1.groupBox_Lane_Rule_Turn_Lanes.Controls,
                    new System.Drawing.Point(checkBox_x, checkBox_y8),
                    DockStyle.None);
                g_frm1.checkBox_Lane_Rule_Turning_Lane8.Click += checkBox_Lane_Rule_Turning_Lane8_Click;

                setCheckBoxState(g_frm1.checkBox_Lane_Rule_Turning_Lane8,
                Lane.LEFT_POSITION + 7,
                g_connectedLanesMap);

            }

            g_frm1.Refresh();
        }

        
        private void setCheckBoxState(CheckBox checkBox,
            int lanePosition,
            Dictionary<int,List<Lane>> connectedLanes)
        {
            if (isCheckboxLaneConnected(checkBox, lanePosition,g_connectedLanesMap))
            {
                checkBox.Checked = true;
            }
        }

        /// <summary>
        /// check对应的Lane是否在 connectedLaneMap 中
        /// false：不在connectedLaneMap中
        /// true：在connectedLaneMap中
        /// </summary>
        /// <param name="checkBox"></param>
        /// <param name="checkBoxIndex"></param>
        /// <param name="connectedLaneMap"></param>
        /// <returns></returns>
        bool isCheckboxLaneConnected(CheckBox checkBox,
            int checkBoxIndex,
            Dictionary<int,List<Lane>> connectedLaneMap)
        {
            int turningIndex = Convert.ToInt32(g_frm1.comBox_Lane_Rule_Next_Arcs.SelectedIndex);

            if (connectedLaneMap == null ||
                connectedLaneMap.Count == 0)
            {
                return false;
            }

            if (!connectedLaneMap.ContainsKey(turningIndex))
            {
                return false;
            }

            foreach (Lane temLane in connectedLaneMap[turningIndex])
            {
                if (temLane.Position == checkBoxIndex)
                {
                    return true;
                }
            }
            return false;
        }

        private void highLightConnector(CheckBox checkBox, int laneIndex)
        {
            if (null == g_turningLanesWithinArc ||
                0 == g_turningLanesWithinArc.Count)
            {
                return;
            }
            if (g_currentLane == null)
            {
                return;
            }
            LaneConnectorFeatureService laneConnectorService = new LaneConnectorFeatureService(g_frm1.FeaClsConnector, 0);
            LaneFeatureService laneFeatureService = new LaneFeatureService(g_frm1.FeaClsLane, g_currentLane.LaneID);
            IFeature fromLaneFeature = laneFeatureService.GetFeature();
            laneFeatureService = new LaneFeatureService(g_frm1.FeaClsLane, g_turningLanesWithinArc[laneIndex].LaneID);
            IFeature toLaneFeature = laneFeatureService.GetFeature();

            IFeature connFeature = laneConnectorService.GetConnectorByLaneIds(g_currentLane.LaneID,
                g_turningLanesWithinArc[laneIndex].LaneID);

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

        private void addNewConnectedLane(CheckBox checkBox, int checkBoxIndex)
        {
            int turningIndex = Convert.ToInt32(g_frm1.comBox_Lane_Rule_Next_Arcs.SelectedIndex);
            //没有才加入到map中去
            if (!isCheckboxLaneConnected(checkBox, checkBoxIndex, g_connectedLanesMap))
            {
                Lane connectedLane = g_turningLanesWithinArc[checkBoxIndex - 1];
                if (!g_connectedLanesMap.ContainsKey(turningIndex))
                {
                    List<Lane> temList = new List<Lane>();
                    temList.Add(connectedLane);
                    g_connectedLanesMap.Add(turningIndex, temList);
                }
                else
                {
                    g_connectedLanesMap[turningIndex].Add(connectedLane);
                }
            }

            //如果在删除map中，就移除掉
            if (isCheckboxLaneConnected(checkBox, checkBoxIndex, g_deletedConnectedLanesMap))
            {
                Lane connectedLane = g_turningLanesWithinArc[checkBoxIndex - 1];
                removeMap(g_deletedConnectedLanesMap, turningIndex, connectedLane);
            }
        }

        void removeMap(Dictionary<int, List<Lane>> map,int key, Lane lane)
        {
            if (!map.ContainsKey(key))
            {
                return;
            }

            List<Lane> temLaneList = new List<Lane>();
            foreach (Lane temLane in map[key])
            {
                if (temLane.LaneID != lane.LaneID)
                {
                    temLaneList.Add(temLane);
                }
            }
            map.Remove(key);

            if (temLaneList == null ||
                temLaneList.Count == 0)
            {
                return;
            }
            map.Add(key, temLaneList);
        }

        private void removeConnectedLane(CheckBox checkBox, int checkBoxIndex)
        {
            int turningIndex = Convert.ToInt32(g_frm1.comBox_Lane_Rule_Next_Arcs.SelectedIndex);
            //在g_connectedLanesMap中存在,从 g_connectedLanesMap 移到  g_deletedConnectedLanesMap 中去
            if (isCheckboxLaneConnected(checkBox, checkBoxIndex, g_connectedLanesMap))
            {
                Lane connectedLane = g_turningLanesWithinArc[checkBoxIndex - 1];
                //从 isCheckboxLaneConnected 删除
                removeMap(g_connectedLanesMap, turningIndex, connectedLane);
              
                //添加到  g_deletedConnectedLanesMap 中去
                if (!g_deletedConnectedLanesMap.ContainsKey(turningIndex))
                {
                    List<Lane> temList = new List<Lane>();
                    temList.Add(connectedLane);
                    g_deletedConnectedLanesMap.Add(turningIndex, temList);
                }
                else
                {
                    g_deletedConnectedLanesMap[turningIndex].Add(connectedLane);
                }
            }
        }

        private void checkBox_Lane_Rule_Turning_Lane1_Click(object sender, EventArgs e)
        {
            highLightConnector(g_frm1.checkBox_Lane_Rule_Turning_Lane1, 0);

            int checkboxIndex = 1;
            if (g_frm1.checkBox_Lane_Rule_Turning_Lane1.Checked)
            {   
                addNewConnectedLane(g_frm1.checkBox_Lane_Rule_Turning_Lane1, checkboxIndex);
            }
            else
            {
                removeConnectedLane(g_frm1.checkBox_Lane_Rule_Turning_Lane1, checkboxIndex);
            }
        }


        private void checkBox_Lane_Rule_Turning_Lane2_Click(object sender, EventArgs e)
        {
            highLightConnector(g_frm1.checkBox_Lane_Rule_Turning_Lane2, 1);

            int checkboxIndex = 2;
            if (g_frm1.checkBox_Lane_Rule_Turning_Lane2.Checked)
            {
                addNewConnectedLane(g_frm1.checkBox_Lane_Rule_Turning_Lane2, checkboxIndex);
            }
            else
            {
                removeConnectedLane(g_frm1.checkBox_Lane_Rule_Turning_Lane2, checkboxIndex);
            }
        }


        private void checkBox_Lane_Rule_Turning_Lane3_Click(object sender, EventArgs e)
        {
            highLightConnector(g_frm1.checkBox_Lane_Rule_Turning_Lane3, 2);

            int checkboxIndex = 3;
            if (g_frm1.checkBox_Lane_Rule_Turning_Lane3.Checked)
            {
                addNewConnectedLane(g_frm1.checkBox_Lane_Rule_Turning_Lane3, checkboxIndex);
            }
            else
            {
                removeConnectedLane(g_frm1.checkBox_Lane_Rule_Turning_Lane3, checkboxIndex);
            }
        }

        private void checkBox_Lane_Rule_Turning_Lane4_Click(object sender, EventArgs e)
        {
            highLightConnector(g_frm1.checkBox_Lane_Rule_Turning_Lane4, 3);

            int checkboxIndex = 4;
            if (g_frm1.checkBox_Lane_Rule_Turning_Lane4.Checked)
            {
                addNewConnectedLane(g_frm1.checkBox_Lane_Rule_Turning_Lane4, checkboxIndex);
            }
            else
            {
                removeConnectedLane(g_frm1.checkBox_Lane_Rule_Turning_Lane4, checkboxIndex);
            }
        }

        private void checkBox_Lane_Rule_Turning_Lane5_Click(object sender, EventArgs e)
        {
            highLightConnector(g_frm1.checkBox_Lane_Rule_Turning_Lane5, 4);

            int checkboxIndex = 5;
            if (g_frm1.checkBox_Lane_Rule_Turning_Lane5.Checked)
            {
                addNewConnectedLane(g_frm1.checkBox_Lane_Rule_Turning_Lane5, checkboxIndex);
            }
            else
            {
                removeConnectedLane(g_frm1.checkBox_Lane_Rule_Turning_Lane5, checkboxIndex);
            }
        }

        private void checkBox_Lane_Rule_Turning_Lane6_Click(object sender, EventArgs e)
        {
            highLightConnector(g_frm1.checkBox_Lane_Rule_Turning_Lane6, 5);
            int checkboxIndex = 6;
            if (g_frm1.checkBox_Lane_Rule_Turning_Lane6.Checked)
            {
                addNewConnectedLane(g_frm1.checkBox_Lane_Rule_Turning_Lane6, checkboxIndex);
            }
            else
            {
                removeConnectedLane(g_frm1.checkBox_Lane_Rule_Turning_Lane6, checkboxIndex);
            }
        }

        private void checkBox_Lane_Rule_Turning_Lane7_Click(object sender, EventArgs e)
        {
            highLightConnector(g_frm1.checkBox_Lane_Rule_Turning_Lane7, 6);

            int checkboxIndex = 7;
            if (g_frm1.checkBox_Lane_Rule_Turning_Lane7.Checked)
            {
                addNewConnectedLane(g_frm1.checkBox_Lane_Rule_Turning_Lane7, checkboxIndex);
            }
            else
            {
                removeConnectedLane(g_frm1.checkBox_Lane_Rule_Turning_Lane7, checkboxIndex);
            }
        }

        private void checkBox_Lane_Rule_Turning_Lane8_Click(object sender, EventArgs e)
        {
            highLightConnector(g_frm1.checkBox_Lane_Rule_Turning_Lane8, 7);

            int checkboxIndex = 8;
            if (g_frm1.checkBox_Lane_Rule_Turning_Lane8.Checked)
            {
                addNewConnectedLane(g_frm1.checkBox_Lane_Rule_Turning_Lane8, checkboxIndex);
            }
            else
            {
                removeConnectedLane(g_frm1.checkBox_Lane_Rule_Turning_Lane8, checkboxIndex);
            }
        }
        
        private void ComBox_Lane_Rule_Next_Arcs_SelectedIndexChanged(object sender, EventArgs e)
        {
            clearTurningLaneGroup();
            layoutTurningLanesControls();
        }


        /// <summary>
        /// 修改入口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void button_Lane_Rule_Modify_Click(object sender, EventArgs e)
        {
            //g_connectedLanesMap
            //g_deletedConnectedLanesMap
            //更新车道连接器
            LaneConnectorFeatureService laneConnectorService = new LaneConnectorFeatureService(g_frm1.FeaClsConnector, 0);
            laneConnectorService.AddDeleteLaneConnection(g_currentLane,g_connectedLanesMap, g_deletedConnectedLanesMap);

            //更新导向箭头
            TurnArrowService turnArrowService = new TurnArrowService(g_frm1.FeaClsTurnArrow, 0);
            int turnArrowStyleId = turnArrowService.GetArrowStyle(g_frm1.FeaClsConnector, g_currentLane.LaneID);
            TurnArrow turnArrow = new TurnArrow();
            turnArrow.ArcID = g_currentArc.ArcID;
            turnArrow.StyleID = turnArrowStyleId;
            turnArrow.ANGLE = turnArrowService.GetTurnArrowAngle(g_currentLaneFeature);
            turnArrow.LaneID = g_currentLane.LaneID;
            turnArrowService.UpdateTurnArrowInLane(g_currentLane.LaneID, turnArrowStyleId);

            init();
            LayoutTurnarrowRule();

        }

    }
}

