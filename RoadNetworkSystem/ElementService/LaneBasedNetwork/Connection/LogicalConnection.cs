using ESRI.ArcGIS.Geodatabase;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.Road;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LaneLayer;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LinkLayer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace RoadNetworkSystem.NetworkElement.LaneBasedNetwork.Connection
{
    class LogicalConnection
    {
        /// <summary>
        /// 返回一个Arc通往各个方向的Arc列表，
        /// </summary>
        /// <param name="fromArc"></param>
        /// <param name="pFeaClsLink"></param>
        /// <param name="pFeaClsArc"></param>
        /// <param name="pFeaClsNode"></param>
        /// <param name="leftTurnArcs"></param>
        /// <param name="rightTurnArcs"></param>
        /// <param name="straightTurnArcs"></param>
        /// <param name="uturnTurnArcs"></param>
        public static void GetTurnTurningArcs(Arc fromArc,
            IFeatureClass pFeaClsLink, IFeatureClass pFeaClsArc, IFeatureClass pFeaClsNode,
            ref List<Arc> leftTurnArcs, ref List<Arc> rightTurnArcs,
            ref List<Arc> straightTurnArcs, ref List<Arc> uturnTurnArcs)
        {
            Node nextNode = PhysicalConnection.getNextNode(pFeaClsLink, pFeaClsArc, pFeaClsNode, fromArc);
            Arc[] exitArcs = GetNodeExitArcs(pFeaClsLink, pFeaClsArc, nextNode);
            foreach(Arc temArc in exitArcs)
            {
                if (temArc == null)
                {
                    continue;
                }
                double angle = PhysicalConnection.GetLinksAngle(fromArc.LinkID, temArc.LinkID, nextNode);
                string turnDir = PhysicalConnection.GetTurningDir(angle);
                switch (turnDir)
                {
                    case LaneConnector.TURNING_LEFT:
                        {
                            leftTurnArcs.Add(temArc);
                            break;
                        }
                    case LaneConnector.TURNING_RIGHT:
                        {
                            rightTurnArcs.Add(temArc);
                            break;
                        }
                    case LaneConnector.TURNING_STRAIGHT:
                        {
                            straightTurnArcs.Add(temArc);
                            break;
                        }
                    case LaneConnector.CHANGE_UTURN:
                        {
                            uturnTurnArcs.Add(temArc);
                            break;
                        }
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 获取Arc行驶方向前后Node
        /// </summary>
        /// <param name="arc"></param>arc对象
        /// <param name="link"></param>link对象
        /// <param name="nextNode"></param>前方NodeID
        /// <param name="preNode"></param>后方NodeID
        public static void GetArcCorresponseNodes(ArcService arc, LinkService link, ref int nextNode, ref int preNode)
        {
            Arc arcEty = new Arc();
            arcEty = arc.GetArcEty(arc.ArcFeature);

            Link linkEty = new Link();
            IFeature linkFea = link.GetFeature();
            LinkMaster linkMasterEty = new LinkMaster();
            linkMasterEty = link.GetEntity(linkFea);
            linkEty = linkEty.Copy(linkMasterEty);
  

            if (arcEty.FlowDir == 1)
            {
                nextNode = linkEty.TNodeID;
                preNode = linkEty.FNodeID;
            }
            else if (arcEty.FlowDir == -1)
            {
                nextNode = linkEty.FNodeID;
                preNode = linkEty.TNodeID;
            }
        }

        /// <summary>
        /// 获取两条Link的相对转向
        /// </summary>
        /// <param name="fromLink"></param>起始Link
        /// <param name="toLink"></param>终止Link
        /// <returns></returns>
        public static string GetTurningDir(LinkService fromLink, LinkService toLink)
        {
            //返回的Link的相对方向
            string TurningDir = "";
            //交叉口NodeID
            int junctionNodeID = 0;

            IFeature fromLinkFea = fromLink.GetFeature();

            LinkMaster linkMasterEty = new LinkMaster();
            linkMasterEty = fromLink.GetEntity(fromLinkFea);
            Link fromLinkEty = new Link();

            fromLinkEty = fromLinkEty.Copy(linkMasterEty);

            
            IFeature toLinkFea = toLink.GetFeature();
            linkMasterEty = toLink.GetEntity(toLinkFea);
            Link toLinkEty = new Link();
            toLinkEty = toLinkEty.Copy(linkMasterEty);

            int fromLinkID = fromLink.Id;
            int toLinkID = toLink.Id;

            if (fromLinkID != toLinkID)
            {

                //      <-------------* <-------------
                //        ---------->     --------->
                if (fromLinkEty.FNodeID == toLinkEty.TNodeID)
                {
                    junctionNodeID = fromLinkEty.FNodeID;
                }
                //      <-------------* ------------->
                //        ---------->     --------->
                else if (fromLinkEty.FNodeID == toLinkEty.FNodeID)
                {
                    junctionNodeID = fromLinkEty.FNodeID;
                }
                //      ------------->* ------------->
                //        ---------->     --------->
                else if (fromLinkEty.TNodeID == toLinkEty.FNodeID)
                {
                    junctionNodeID = toLinkEty.TNodeID;
                }
                //      ------------->* <-------------
                //        ---------->     --------->
                else
                {
                    junctionNodeID = toLinkEty.TNodeID;
                }
            }

            //当车道连接器起点、终点属于同一Link
            if (fromLinkID == toLinkID)
            {
                TurningDir = "UTurn";
            }
            else
            {
                IFeatureClass pFeaClsNode = (fromLink._pFeaClsLink.FeatureDataset.Workspace as IFeatureWorkspace).OpenFeatureClass(Node.NodeName);
                NodeService junctionNode = new NodeService(pFeaClsNode, junctionNodeID, null);
                IFeature junctionNodeFea = junctionNode.GetFeature();
                //读取Node的实体
                Node junNodeEty = junctionNode.GetNodeMasterEty(junctionNodeFea) as Node;

                string AdjLink = junNodeEty.AdjIDs;
                string AdjNorthAngle = junNodeEty.NorthAngles;

                string[] arrLink = AdjLink.Split('\\');
                string[] arrNorthAngle = AdjNorthAngle.Split('\\');


                //计算角度
                int i, fromsub = -1, tosub = -1;
                for (i = 0; i <= arrLink.GetUpperBound(0); i++)
                {
                    if (Convert.ToInt64(arrLink[i]) == fromLinkID)
                        fromsub = i;
                    else if (Convert.ToInt64(arrLink[i]) == toLinkID)
                        tosub = i;
                }

                if (fromsub == -1 || tosub == -1)
                {
                    MessageBox.Show("Can't Link: " + fromLinkID.ToString() + " or " + toLinkID.ToString() + " in Node: " + junctionNodeID.ToString());
                    return null;
                }

                int turningAngle = (Convert.ToInt16(arrNorthAngle[tosub]) - Convert.ToInt16(arrNorthAngle[fromsub]) + 360) % 360;
                TurningDir = PhysicalConnection.GetTurningDir(turningAngle);

            }
            return TurningDir;   //返回TurningDir
        }

        /// <summary>
        /// 获取给定交叉口和出口Link的出口Arc
        /// </summary>
        /// <param name="pFeaClsArc"></param>
        /// <param name="junctionNode"></param>
        /// <param name="exitLinkEty"></param>
        /// <returns></returns>
        public static IFeature GetExitArc(IFeatureClass pFeaClsArc, int junctionNode, Link exitLinkEty)
        {

            int nextArcFlowDir = 0;

            //FNode与nextNode相同，NextNode的出口Arc与NextLink方向相同
            
            ///  (preNode)*-----------------------------*(Junction)-------------nextLink----------------->*
            ///               ----------------------->                ----------------------->
            if (exitLinkEty.FNodeID == junctionNode)
            {
                nextArcFlowDir = 1;
            }

            ///  (preNode)*-----------------------------*(NextNode)<------------------------------*
            ///               ----------------------->                ----------------------->
            else
            {
                nextArcFlowDir = -1;
            }

            ArcService arc = new ArcService(pFeaClsArc, 0);
            //通过LinkID和FlowDir,查询Arc


            return arc.GetArcInfo(exitLinkEty.ID, nextArcFlowDir);

             
        }

        /// <summary>
        /// 获取y字交叉口（分支出口）右侧出口的偏离车道数
        /// </summary>
        /// <param name="pFeaClsLink"></param>
        /// <param name="pFeaClsArc"></param>
        /// <param name="pFeaClsNode"></param>
        /// <param name="preNode"></param>
        /// <param name="currentArcId"></param>
        /// <returns></returns>
        public static int getPreNodeLateralOffsideLanes(IFeatureClass pFeaClsLink,IFeatureClass pFeaClsArc,IFeatureClass pFeaClsNode, 
            Node preNode, int currentArcId)
        {
            Arc[] entranceArcs = GetNodeEntranceArcs(pFeaClsLink, pFeaClsArc, preNode);
            List<Arc> entranceArcList = filterInvalidArcs(entranceArcs);

            ArcService arcService = new ArcService(pFeaClsArc, currentArcId);
            IFeature arcFeature = arcService.GetArcFeature();
            Arc currentArc = arcService.GetArcEty(arcFeature);

            //上游没有Arc,或者当前为双向的
            if (entranceArcList == null || entranceArcList.Count == 0 || (!isOnewayArc(currentArc, pFeaClsLink)))
            {
                return 0;
            }
            else
            {
                if (entranceArcList.Count == 1)
                {
                    int entranceArcId = entranceArcList[0].ArcID;

                    Arc[] exitArcs = GetNodeExitArcs(pFeaClsLink, pFeaClsArc, preNode);
                    List<Arc> exitArcList = filterInvalidArcs(exitArcs);
                    if (exitArcList.Count == 1)
                    {
                        return 0;
                    }


                    int clockLinkId = 0;
                    double clockAngle = 0;
                    PhysicalConnection.GetClockLinkInfor(entranceArcList[0].LinkID, preNode, ref clockLinkId, ref clockAngle);

                    Arc leftestArc = getRequiredArcByLinkId(exitArcList, clockLinkId);
                    int lateralLaneNum = leftestArc.LaneNum;
                    int loop = 0;
                    while (clockLinkId != currentArc.LinkID && loop < 5)
                    {
                        PhysicalConnection.GetClockLinkInfor(clockLinkId, preNode, ref clockLinkId, ref clockAngle);
                        Arc cursorArc = getRequiredArcByLinkId(exitArcList, clockLinkId);
                        if (cursorArc.ArcID != currentArc.ArcID)
                        {
                            lateralLaneNum += cursorArc.LaneNum;
                        }
                        loop++;
                    }
                    return lateralLaneNum;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// 过滤掉非法的Arc
        /// </summary>
        /// <param name="arcs"></param>
        /// <returns></returns>
        public static List<Arc> filterInvalidArcs(Arc[] arcs)
        {
            List<Arc> arcList = new List<Arc>();
            foreach (Arc arc in arcs)
            {
                if (arc != null)
                {
                    arcList.Add(arc);
                }
            }

            return arcList;
        }


        /// <summary>
        /// 获取y字交叉口（分支入口）右侧入口的偏离车道数
        /// </summary>
        /// <param name="pFeaClsLink"></param>
        /// <param name="pFeaClsArc"></param>
        /// <param name="pFeaClsNode"></param>
        /// <param name="nextNode"></param>
        /// <param name="currentArcId"></param>
        /// <returns></returns>
        public static int getNextNodeLateralOffsideLanes(IFeatureClass pFeaClsLink, IFeatureClass pFeaClsArc, IFeatureClass pFeaClsNode, 
            Node nextNode, int currentArcId)
        {

            Arc[] exitArcs = GetNodeExitArcs(pFeaClsLink, pFeaClsArc, nextNode);
            List<Arc> exitArcList = filterInvalidArcs(exitArcs);

            ArcService arcService = new ArcService(pFeaClsArc, currentArcId);
            IFeature arcFeature = arcService.GetArcFeature();
            Arc currentArc = arcService.GetArcEty(arcFeature);

            //上游没有Arc,或者当前为双向的
            if (exitArcList == null || exitArcList.Count == 0 || (!isOnewayArc(currentArc, pFeaClsLink)))
            {
                return 0;
            }
            else
            {
                if (exitArcList.Count == 1)
                {
                    int entranceArcId = exitArcList[0].ArcID;


                    Arc[] entranceArcs = GetNodeEntranceArcs(pFeaClsLink, pFeaClsArc, nextNode);
                    List<Arc> entranceArcList = filterInvalidArcs(entranceArcs);

                    if (entranceArcList.Count == 1)
                    {
                        return 0;
                    }

                    int antiCclockLinkId = 0;
                    double antiClockAngle = 0;

                    PhysicalConnection.GetAntiClockLinkInfor(exitArcList[0].LinkID, nextNode, ref antiCclockLinkId, ref antiClockAngle);

                    Arc leftestArc = getRequiredArcByLinkId(entranceArcList, antiCclockLinkId);
                    int lateralLaneNum = leftestArc.LaneNum;
                    int loop = 0;
                    while (antiCclockLinkId != currentArc.LinkID && loop < 5)
                    {
                        PhysicalConnection.GetAntiClockLinkInfor(antiCclockLinkId, nextNode, ref antiCclockLinkId, ref antiClockAngle);
                        Arc cursorArc = getRequiredArcByLinkId(entranceArcList, antiCclockLinkId);
                        if (cursorArc.ArcID != currentArc.ArcID)
                        {
                            lateralLaneNum += cursorArc.LaneNum;
                        }
                        loop++;
                    }
                    return lateralLaneNum;
                }
                else
                {
                    return 0;
                }
            }
        }


        private static Arc getRequiredArcByLinkId(List<Arc> arcs, int linkId)
        {
            foreach (Arc temArc in arcs)
            {
                if (temArc.LinkID == linkId)
                {
                    return temArc;
                }
            }
            return null;
        }

        /// <summary>
        /// 判断Arc是否为单向
        /// </summary>
        /// <param name="currentArc"></param>
        /// <param name="pFeaClsLink"></param>
        /// <returns></returns>
        public static bool isOnewayArc(Arc currentArc,IFeatureClass pFeaClsLink)
        {
            LinkService linkService = new LinkService(pFeaClsLink, currentArc.LinkID);
            IFeature linkFea = linkService.GetFeature();
            LinkMaster linkMaster = linkService.GetEntity(linkFea);
            Link link = new Link();
            link = link.Copy(linkMaster);
            if (link.FlowDir == Link.FLOWDIR_OPPOSITION || link.FlowDir == Link.FLOWDIR_SAME)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 获取给定交叉口和入口Link的入口Arc
        /// </summary>
        /// <param name="pFeaClsArc"></param>
        /// <param name="junctionNode"></param>
        /// <param name="entranceLinkEty"></param>
        /// <returns></returns>
        public static IFeature GetEntranceArc(IFeatureClass pFeaClsArc, int junctionNode, Link entranceLinkEty)
        {
            int preArcFlowDir = 0;

            //FNode与nextNode相同，NextNode的出口Arc与NextLink方向相同
            /// *--------------preLik---------------*(Junction)------------curLink------------------>*
            ///           ----------------------->                ----------------------->
            if (entranceLinkEty.TNodeID == junctionNode)
            {
                preArcFlowDir = 1;
            }

            ///  (preNode)*-----------------------------*(NextNode)<------------------------------*
            ///               ----------------------->                ----------------------->
            else
            {
                preArcFlowDir = -1;
            }

            ArcService arc = new ArcService(pFeaClsArc, 0);
            //通过LinkID和FlowDir,查询Arc
            //string queryStr = String.Format("{0} = {1} & {2} = {3}", Arc.LinkIDNm, preLinkEty.ID, Arc.FlowDirNm, preArcFlowDir);
            //string queryStr = Arc.LinkIDNm + "=" + preLinkEty.ID + "AND" + Arc.FlowDirNm + "=" + preArcFlowDir;
            //nextArcFeature = arc.QueryArcFeatureByRule(queryStr);

            return arc.GetArcInfo(entranceLinkEty.ID, preArcFlowDir);
        }


        public static Arc[] GetNodeEntranceArcs(IFeatureClass pFeaClsLink,IFeatureClass pFeaClsArc,Node junctionEty)
        {
            
            string[] adjLinks = junctionEty.AdjIDs.Split('\\');
            Arc[] entranceArcEtys = new Arc[adjLinks.Length];
            for (int i = 0; i < adjLinks.Length; i++)
            {
                int temLinkID = Convert.ToInt32(adjLinks[i]);

                LinkService link = new LinkService(pFeaClsLink, temLinkID);
                IFeature linkFea = link.GetFeature();
                LinkMaster linkMstr = new LinkMaster();
                linkMstr = link.GetEntity(linkFea);
                Link linkEty = new Link();
                linkEty = linkEty.Copy(linkMstr);
                
                Arc temArc = new Arc();
                IFeature temArcFea = GetEntranceArc(pFeaClsArc, junctionEty.ID, linkEty);

                if (temArcFea == null)
                {
                    continue;
                }
                else
                {
                    ArcService arc = new ArcService(pFeaClsArc, 0);
                    temArc = arc.GetArcEty(temArcFea);
                    entranceArcEtys[i] = new Arc();
                    entranceArcEtys[i] = temArc.Copy();
                }
            }

                return entranceArcEtys;
        }


        public static Arc[] GetNodeExitArcs(IFeatureClass pFeaClsLink, IFeatureClass pFeaClsArc, Node junctionEty)
        {

            string[] adjLinks = junctionEty.AdjIDs.Split('\\');
            Arc[] exitArcEtys = new Arc[adjLinks.Length];
            for (int i = 0; i < adjLinks.Length; i++)
            {
                int temLinkID = Convert.ToInt32(adjLinks[i]);

                LinkService link = new LinkService(pFeaClsLink, temLinkID);
                IFeature linkFea = link.GetFeature();
                LinkMaster linkMstr = new LinkMaster();
                linkMstr = link.GetEntity(linkFea);
                Link linkEty = new Link();
                linkEty = linkEty.Copy(linkMstr);

                Arc temArc = new Arc();
                IFeature temArcFea = GetExitArc(pFeaClsArc, junctionEty.ID, linkEty);

                if (temArcFea == null)
                {
                    continue;
                }
                else
                {
                    ArcService arc = new ArcService(pFeaClsArc, 0);
                    temArc = arc.GetArcEty(temArcFea);

                    exitArcEtys[i] = new Arc();
                    exitArcEtys[i] = temArc.Copy();
                }
            }

            return exitArcEtys;
        }


        /// <summary>
        /// 获取直接方向
        /// </summary>
        /// <param name="pFeaClsNode"></param>
        /// <param name="pFeaClsLink"></param>
        /// <param name="pFeaClsArc"></param>
        /// <param name="pFeaClsConnector"></param>
        /// <param name="laneEty"></param>
        /// <returns></returns>
        public static List<string> GetLaneLeadTurnDir(IFeatureClass pFeaClsNode, IFeatureClass pFeaClsLink,
            IFeatureClass pFeaClsArc, IFeatureClass pFeaClsConnector, Lane laneEty)
        {
            List<string> leadingDir = new List<string>();
            LaneConnectorFeatureService laneConnectorService= new LaneConnectorFeatureService(pFeaClsConnector,0);

            List<LaneConnector> connectors = laneConnectorService.GetConnectorByFromLane(laneEty.LaneID);
            if (connectors == null)
            {
                return null;
            }

            foreach (LaneConnector connector in connectors)
            {
                if(leadingDir.Contains(connector.TurningDir))
                {
                    continue;
                }
                leadingDir.Add(connector.TurningDir);
            }

            return leadingDir;
        }

        /// <summary>
        /// 获取一个车道的通过前方路段后，在前方交叉口处的所有可能转向
        /// </summary>
        /// <param name="pFeaClsNode"></param>
        /// <param name="pFeaClsLink"></param>
        /// <param name="pFeaClsArc"></param>
        /// <param name="pFeaClsConnector"></param>
        /// <param name="laneEty"></param>
        /// <returns></returns>
        public static List<string> GetLaneLeadNodeTurnDir(IFeatureClass pFeaClsNode, IFeatureClass pFeaClsLink,
            IFeatureClass pFeaClsArc, IFeatureClass pFeaClsConnector,Lane laneEty)
        {
            List<string> leadingDir = new List<string>();
            
            int laneID=laneEty.LaneID;
            int arcID = laneEty.ArcID;
            ArcService arc = new ArcService(pFeaClsArc, arcID);
            IFeature arcFea = arc.GetArcFeature();
            int linkID = Convert.ToInt32(arcFea.get_Value(pFeaClsArc.FindField(Arc.LinkIDNm)));
            LinkService link=new LinkService(pFeaClsLink,linkID);
            int nextNodeID=0;
            int preNodeID=0;
            LogicalConnection.GetArcCorresponseNodes(arc, link, ref nextNodeID, ref preNodeID);

            //获取从当前路段到前方交叉口的所有的LinkID
            linkIDs = new List<int>();
            //递归获取到达前方交叉口前的所有经过的Link
            linkIDs = getEntranceLinkInSeg(pFeaClsLink, pFeaClsNode, linkID, nextNodeID);
            //最后一个是交叉口Link
            int entranceLinkID = linkIDs[linkIDs.Count - 1];
            //递归获取所有的转向
            leadingDir = getLaneTurnDirInNode(pFeaClsConnector, laneID, linkID, entranceLinkID,0);
            _turnDirInNode = new List<string>();

            return leadingDir;
        }

        private static List<string> _turnDirInNode = new List<string>();

        private const int MAX_RECURSION_TIMES = 4;

        /// <summary>
        /// 递归获取一个车道在前方交叉口的所有转向
        /// </summary>
        /// <param name="pFeaClsConn"></param>车道连接器要素类
        /// <param name="laneID"></param>当前车道的ID
        /// <param name="cursorLinkID"></param>游标车道，初始值为当前车道的LinkID
        /// <param name="entranceLinkID"></param>交叉口入口LinkID
        /// <returns></returns>
        private static List<string> getLaneTurnDirInNode(IFeatureClass pFeaClsConn,int laneID,int cursorLinkID, int entranceLinkID,int recursionTime)
        {

            if (cursorLinkID == entranceLinkID)
            {
                IFeatureCursor cursorConnector;
                IQueryFilter filterConn = new QueryFilterClass();

                filterConn.WhereClause = LaneConnectorFeatureService.fromLaneIDNm + " = " + laneID;
                cursorConnector = pFeaClsConn.Search(filterConn, false);
                IFeature feaConn = cursorConnector.NextFeature();
                while (feaConn != null && recursionTime < MAX_RECURSION_TIMES)
                {
                    LaneConnector connEty = new LaneConnector();
                    LaneConnectorFeatureService laneConn = new LaneConnectorFeatureService(pFeaClsConn, 0);
                    connEty = laneConn.GetLaneConnEty(feaConn);
                    //没有加入过，则加入
                    if (_turnDirInNode.Contains(connEty.TurningDir) == false)
                    {
                        _turnDirInNode.Add(connEty.TurningDir);
                    }
                    feaConn = cursorConnector.NextFeature();
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                }
                //System.Runtime.InteropServices.Marshal.ReleaseComObject(cursorConnector);
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
            }
            else
            {


                while (cursorLinkID != entranceLinkID && recursionTime < MAX_RECURSION_TIMES)
                {
                    IFeatureCursor cursorConnector;
                    IQueryFilter filterConn = new QueryFilterClass();

                    filterConn.WhereClause = LaneConnectorFeatureService.fromLaneIDNm + " = " + laneID;
                    cursorConnector = pFeaClsConn.Search(filterConn, false);
                    IFeature feaConn = cursorConnector.NextFeature();
                    while (feaConn != null && recursionTime < MAX_RECURSION_TIMES)
                    {
                        LaneConnector connEty = new LaneConnector();
                        LaneConnectorFeatureService laneConn = new LaneConnectorFeatureService(pFeaClsConn, 0);
                        connEty = laneConn.GetLaneConnEty(feaConn);

                        //更新游标
                        cursorLinkID = connEty.fromLinkID;
                        laneID = connEty.fromLaneID;
                        if (cursorLinkID == entranceLinkID)
                        {
                            //没有加入过，则加入
                            if (_turnDirInNode.Contains(connEty.TurningDir) == false)
                            {
                                _turnDirInNode.Add(connEty.TurningDir);
                            }
                        }
                        ++recursionTime;
                        //递归啦
                        getLaneTurnDirInNode(pFeaClsConn, laneID, cursorLinkID, entranceLinkID, recursionTime);
                        feaConn = cursorConnector.NextFeature();
                        System.GC.Collect();
                        System.GC.WaitForPendingFinalizers();
                    }
                    //System.Runtime.InteropServices.Marshal.ReleaseComObject(cursorConnector);
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                }
            }
            return _turnDirInNode;
        }

        private static List<int> linkIDs = new List<int>();
        /// <summary>
        /// 递归获取从一个Link到交叉口处的所有LinkID
        /// </summary>
        /// <param name="pFeaClsLink"></param>
        /// <param name="pFeaClsNode"></param>
        /// <param name="linkID"></param>
        /// <param name="nextNodeID"></param>
        /// <returns></returns>
        public static List<int> getEntranceLinkInSeg(IFeatureClass pFeaClsLink,IFeatureClass pFeaClsNode,int linkID,int nextNodeID)
        {
           
            bool entranceLinkFlag = false;

            while (entranceLinkFlag == false)
            {
                Link linkEty = new Link();
                LinkService link = new LinkService(pFeaClsLink, linkID);
                IFeature pFeature = link.GetFeature();
                LinkMaster linkMstEty = new LinkMaster();
                linkMstEty = link.GetEntity(pFeature);
                linkEty = linkEty.Copy(linkMstEty);

                Node nodeEty = new Node();
                NodeService node = new NodeService(pFeaClsNode, nextNodeID, null);
                NodeMaster nodeMstrEty = new NodeMaster();
                IFeature pFeatureNode = node.GetFeature();
                nodeMstrEty = node.GetNodeMasterEty(pFeatureNode);
                nodeEty = nodeEty.Copy(nodeMstrEty);

                linkIDs.Add(linkID);
                string[] adjLinks=nodeEty.AdjIDs.Split('\\');
                if (adjLinks.Length>2)
                {
                    break;
                }
                else
                {
                    //更新LinkID
                    if (adjLinks.Length == 2)
                    {
                        if (adjLinks[0].Equals(linkID.ToString()))
                        {
                            linkID = Convert.ToInt32(adjLinks[1]);
                        }
                        else
                        {
                            linkID = Convert.ToInt32(adjLinks[0]);
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                //更新nextNodeID
                if (nextNodeID == linkEty.FNodeID)
                {
                    nextNodeID = linkEty.TNodeID;
                }
                else
                {
                    nextNodeID = linkEty.FNodeID;
                }

                getEntranceLinkInSeg(pFeaClsLink, pFeaClsNode, linkID, nextNodeID);

            }
            return linkIDs;
        }
    }
}
