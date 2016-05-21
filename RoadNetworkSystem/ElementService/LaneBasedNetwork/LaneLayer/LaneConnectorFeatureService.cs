using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.Road;
using RoadNetworkSystem.ElementService.LaneBasedNetwork.NetworkBuilder;
using RoadNetworkSystem.GIS;
using RoadNetworkSystem.GIS.GeoDatabase.Dataset;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.Connection;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LinkLayer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LaneLayer
{

    /// <summary>
    /// 记录一个Lane与一个Arc多个车道的连通情况
    /// </summary>
    public struct LaneArcConnectedInfo
    {
        public Lane FromLane;
        public List<Lane> ToLanes;
        public string Dir;
    }
    class LaneConnectorFeatureService
    {
        /// <summary>
        /// 规定了数据模型，请不要在其他类中直接读取数据
        /// </summary>
        public const string ConnectorIDNm = "ConnectorID";
        public const string fromLaneIDNm = "fromLaneID";
        public const string toLaneIDNm = "toLaneID";

        public const string TurningDirNm = "TurningDir";
        public const string fromArcIDNm = "fromArcID";
        public const string toArcIDNm = "toArcID";

        public const string fromLinkIDNm = "fromLinkID";
        public const string fromDirNm = "fromDir";
        public const string toLinkIDNm = "toLinkID";

        public const string toDirNm = "toDir";
        public const string OtherNm = "Other";

        private bool CREATE_UTURN_FLAG = false;

        struct ArcLaneConnectorsPosition
        {

            /*

                ----------------------------            ----------------------------
                ----fromLaneLeftPosition----  ------>   ----------------------------
                ----fromLaneRightPosition---  ------>   ----------------------------
                ----------------------------            ----------------------------

            */

            /// <summary>
            /// 一个方向的车道连接器，在起始Arc中，最左侧的车道的Position
            /// </summary>
            public int fromLaneLeftPosition;

            /// <summary>
            /// 一个方向的车道连接器，在起始Arc中，最右侧的车道的Position
            /// </summary>
            public int fromLaneRightPosition;
        }
        private int _connectorID = 0;
        private IFeatureClass _pFeaClsConnector;
        private IFeatureClass _pFeaClsLane;
        private IFeatureClass _pFeaClsArc;
        private IFeatureClass _pFeaClsLink;
        private IFeatureClass _pFeaClsNode;

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="pFeaClsConnector"></param>ITable 
        /// <param name="connectorID"></param>
        public LaneConnectorFeatureService(IFeatureClass pFeaClsConnector, int connectorID)
        {
            _pFeaClsConnector = pFeaClsConnector;
            _pFeaClsLane = FeatureClassHelper.GetFeaClsInAccess(_pFeaClsConnector.FeatureDataset.Workspace.PathName,
                Lane.LaneName);
            _pFeaClsArc = FeatureClassHelper.GetFeaClsInAccess(_pFeaClsConnector.FeatureDataset.Workspace.PathName,
                Arc.ArcFeatureName);

            _pFeaClsLink = FeatureClassHelper.GetFeaClsInAccess(_pFeaClsConnector.FeatureDataset.Workspace.PathName,
                Link.LinkName);

            _pFeaClsNode = FeatureClassHelper.GetFeaClsInAccess(_pFeaClsConnector.FeatureDataset.Workspace.PathName,
                Node.NodeName);
            _connectorID = connectorID;
        }

        /// <summary>
        /// 获取车道连接器
        /// </summary>
        /// <returns></returns>车道连接器实体
        public LaneConnector GetLaneConnEty(IFeature targetFea)
        {
            LaneConnector targetLaneEty = new LaneConnector();

            if (targetFea != null)
            {
                targetLaneEty.ConnectorID = _connectorID;
                targetLaneEty.fromArcID = Convert.ToInt32(targetFea.get_Value(_pFeaClsConnector.FindField(fromArcIDNm)));
                targetLaneEty.fromDir = Convert.ToInt32(targetFea.get_Value(_pFeaClsConnector.FindField(fromDirNm)));
                targetLaneEty.fromLaneID = Convert.ToInt32(targetFea.get_Value(_pFeaClsConnector.FindField(fromLaneIDNm)));
                targetLaneEty.fromLinkID = Convert.ToInt32(targetFea.get_Value(_pFeaClsConnector.FindField(fromLinkIDNm)));
                targetLaneEty.toArcID = Convert.ToInt32(targetFea.get_Value(_pFeaClsConnector.FindField(toArcIDNm)));
                targetLaneEty.toDir = Convert.ToInt32(targetFea.get_Value(_pFeaClsConnector.FindField(toDirNm)));
                targetLaneEty.toLaneID = Convert.ToInt32(targetFea.get_Value(_pFeaClsConnector.FindField(toLaneIDNm)));
                targetLaneEty.toLinkID = Convert.ToInt32(targetFea.get_Value(_pFeaClsConnector.FindField(toLinkIDNm)));
                targetLaneEty.TurningDir = Convert.ToString(targetFea.get_Value(_pFeaClsConnector.FindField(TurningDirNm)));
            }
            else
            {
                targetLaneEty = null;
            }
            return targetLaneEty;
        }

        /// <summary>
        /// 获取车道连接器
        /// </summary>
        /// <returns></returns>车道连接器实体
        public LaneConnector GetLaneConnEty(int connectorID)
        {
            LaneConnector targetLaneEty = new LaneConnector();
            IQueryFilter queryFilter = new QueryFilterClass();
            queryFilter.WhereClause = String.Format("ConnectorID ={0}", connectorID);
            IFeatureCursor cursor = _pFeaClsConnector.Search(queryFilter, false);
            IFeature targetFea = cursor.NextFeature();
            if (targetFea != null)
            {
                targetLaneEty.ConnectorID = _connectorID;
                targetLaneEty.fromArcID = Convert.ToInt32(targetFea.get_Value(_pFeaClsConnector.FindField(fromArcIDNm)));
                targetLaneEty.fromDir = Convert.ToInt32(targetFea.get_Value(_pFeaClsConnector.FindField(fromDirNm)));
                targetLaneEty.fromLaneID = Convert.ToInt32(targetFea.get_Value(_pFeaClsConnector.FindField(fromLaneIDNm)));
                targetLaneEty.fromLinkID = Convert.ToInt32(targetFea.get_Value(_pFeaClsConnector.FindField(fromLinkIDNm)));
                targetLaneEty.toArcID = Convert.ToInt32(targetFea.get_Value(_pFeaClsConnector.FindField(toArcIDNm)));
                targetLaneEty.toDir = Convert.ToInt32(targetFea.get_Value(_pFeaClsConnector.FindField(toDirNm)));
                targetLaneEty.toLaneID = Convert.ToInt32(targetFea.get_Value(_pFeaClsConnector.FindField(toLaneIDNm)));
                targetLaneEty.toLinkID = Convert.ToInt32(targetFea.get_Value(_pFeaClsConnector.FindField(toLinkIDNm)));
                targetLaneEty.TurningDir = Convert.ToString(targetFea.get_Value(_pFeaClsConnector.FindField(TurningDirNm)));
            }
            else
            {
                targetLaneEty = null;
            }
            return targetLaneEty;
        }

        /// <summary>
        /// 插入一条车道连接器
        /// </summary>
        /// <param name="updateLaneConnEty"></param>
        public void InsertLConnector(LaneConnector updateLaneConnEty, IPolyline connLine)
        {
            //由fromLaneID和toLaneID判断车道连接器是否存在，存在=true，不存在=false
            bool exitConntectorFlag = false;
            exitConntectorFlag = IsExistedConnByLane(updateLaneConnEty.fromLaneID, updateLaneConnEty.toLaneID);
            //不存在才生成
            if (exitConntectorFlag == false)
            {

                IFeature newFeature = _pFeaClsConnector.CreateFeature();
                if (updateLaneConnEty.ConnectorID > 0)
                {

                    newFeature.set_Value(_pFeaClsConnector.FindField("ConnectorID"), updateLaneConnEty.ConnectorID);
                }
                else
                {
                    newFeature.set_Value(_pFeaClsConnector.FindField("ConnectorID"), newFeature.OID);
                }
                newFeature.set_Value(_pFeaClsConnector.FindField("fromArcID"), updateLaneConnEty.fromArcID);
                newFeature.set_Value(_pFeaClsConnector.FindField("fromDir"), updateLaneConnEty.fromDir);
                newFeature.set_Value(_pFeaClsConnector.FindField("fromLaneID"), updateLaneConnEty.fromLaneID);
                newFeature.set_Value(_pFeaClsConnector.FindField("fromLinkID"), updateLaneConnEty.fromLinkID);
                newFeature.set_Value(_pFeaClsConnector.FindField("toArcID"), updateLaneConnEty.toArcID);
                newFeature.set_Value(_pFeaClsConnector.FindField("toDir"), updateLaneConnEty.toDir);
                newFeature.set_Value(_pFeaClsConnector.FindField("toLinkID"), updateLaneConnEty.toLinkID);
                newFeature.set_Value(_pFeaClsConnector.FindField("toLaneID"), updateLaneConnEty.toLaneID);
                newFeature.set_Value(_pFeaClsConnector.FindField("TurningDir"), updateLaneConnEty.TurningDir);

                newFeature.Shape = connLine;
                newFeature.Store();

            }
        }

        /// <summary>
        /// 更新车道连接器
        /// </summary>
        /// <param name="updateLaneConnEty"></param>
        /// <param name="preLaneConnID"></param>
        public void UpdateConnector(LaneConnector updateLaneConnEty, int preLaneConnID)
        {
            LaneConnector preLaneConnEty = GetLaneConnEty(preLaneConnID);


            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = String.Format("{0}={1}", ConnectorIDNm, preLaneConnEty.ConnectorID);
            IFeatureCursor pFeatureCuror1 = _pFeaClsConnector.Search(filter, false);
            IFeature modifiedFea = pFeatureCuror1.NextFeature();

            if (updateLaneConnEty.TurningDir != preLaneConnEty.TurningDir)
            {
                modifiedFea.set_Value(_pFeaClsConnector.FindField(TurningDirNm), updateLaneConnEty.TurningDir);
            }
            if (updateLaneConnEty.fromArcID != preLaneConnEty.fromArcID)
            {
                modifiedFea.set_Value(_pFeaClsConnector.FindField(fromArcIDNm), updateLaneConnEty.fromArcID);
            }
            if (updateLaneConnEty.fromDir != preLaneConnEty.fromDir)
            {
                modifiedFea.set_Value(_pFeaClsConnector.FindField(fromDirNm), updateLaneConnEty.fromDir);
            }
            if (updateLaneConnEty.fromLaneID != preLaneConnEty.fromLaneID)
            {
                modifiedFea.set_Value(_pFeaClsConnector.FindField(fromLaneIDNm), updateLaneConnEty.fromLaneID);
            }
            if (updateLaneConnEty.fromLinkID != preLaneConnEty.fromLinkID)
            {
                modifiedFea.set_Value(_pFeaClsConnector.FindField(fromLinkIDNm), updateLaneConnEty.fromLinkID);
            }
            if (updateLaneConnEty.toArcID != preLaneConnEty.toArcID)
            {
                modifiedFea.set_Value(_pFeaClsConnector.FindField(toArcIDNm), updateLaneConnEty.toArcID);
            }
            if (updateLaneConnEty.toDir != preLaneConnEty.toDir)
            {
                modifiedFea.set_Value(_pFeaClsConnector.FindField(toArcIDNm), updateLaneConnEty.toDir);
            }

            if (updateLaneConnEty.toLaneID != preLaneConnEty.toLaneID)
            {
                modifiedFea.set_Value(_pFeaClsConnector.FindField(toLaneIDNm), updateLaneConnEty.toLaneID);
            }
            if (updateLaneConnEty.toLinkID != preLaneConnEty.toLinkID)
            {
                modifiedFea.set_Value(_pFeaClsConnector.FindField(toLinkIDNm), updateLaneConnEty.toLinkID);
            }
            modifiedFea.Store();

        }

        /// <summary>
        /// 删除车道连接器
        /// </summary>
        /// <param name="connecttorID"></param>车道连接器的ID
        public void DeleteConnector(int connecttorID)
        {

            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = String.Format("{0}={1}", ConnectorIDNm, connecttorID);
            IFeatureCursor cursor = _pFeaClsConnector.Update(filter, false);
            IFeature deleteRow = cursor.NextFeature();

            deleteRow.Delete();

        }

        /// <summary>
        /// 创建一个结点处的Arc间的车道连接器
        /// </summary>
        /// <param name="nodeEty"></param>
        /// <param name="pFeaClsLink"></param>
        /// <param name="pFeaClsArc"></param>
        public void CreateConnectorInNode(IFeature nodeFea, IFeatureClass pFeaClsNode, IFeatureClass pFeaClsLink, IFeatureClass pFeaClsArc)
        {
            NodeService nodeService = new NodeService(pFeaClsNode, 0, null);
            NodeMaster nodeMaster = nodeService.GetNodeMasterEty(nodeFea);
            Node node = new Node();
            node = node.Copy(nodeMaster);


        }


        public IFeature GetConnectorByLaneIds(int fromLaneId, int toLaneId)
        {
            IQueryFilter LCFilter1 = new QueryFilterClass();
            LCFilter1.WhereClause = String.Format("fromLaneID={0} AND toLaneID={1}", fromLaneId, toLaneId);

            IFeatureCursor LaneConnectorCursor1 = _pFeaClsConnector.Search(LCFilter1, false);
            IFeature LaneConnectorRow1 = LaneConnectorCursor1.NextFeature();
            return LaneConnectorRow1;

        }

        /// <summary>
        /// 判断两个车道间是否存在车道连接器
        /// </summary>
        /// <param name="fromLaneID"></param>
        /// <param name="toLaneID"></param>
        /// <returns></returns>
        public bool IsExistedConnByLane(int fromLaneID, int toLaneID)
        {
            bool existedFlag = false;
            int connectorID = GetConnectorIdByLane(fromLaneID, toLaneID);
            if (connectorID != -1)
            {
                existedFlag = true;
            }
            else
            {
                existedFlag = false;
            }
            return existedFlag;
        }

        /// <summary>
        /// 通过起始车道和终止车道的查找车道连接器
        /// </summary>
        /// <param name="fromLaneID"></param>
        /// <param name="toLaneID"></param>
        /// <returns></returns>
        public int GetConnectorIdByLane(int fromLaneID, int toLaneID)
        {
            int connectorID = -1;

            IFeature LaneConnectorRow1 = GetConnectorByLaneIds(fromLaneID, toLaneID);
            if (LaneConnectorRow1 != null)
            {
                connectorID = Convert.ToInt32(LaneConnectorRow1.get_Value(_pFeaClsConnector.FindField("ConnectorID")));
            }

            return connectorID;
        }

        /// <summary>
        /// 通过起始车道和终止车道的查找车道连接器
        /// </summary>
        /// <param name="fromLaneID"></param>
        /// <param name="toLaneID"></param>
        /// <returns></returns>
        public List<LaneConnector> GetConnectorByFromLane(int fromLaneID)
        {
            List<LaneConnector> toLaneIDs = new List<LaneConnector>();

            IQueryFilter LCFilter1 = new QueryFilterClass();
            LCFilter1.WhereClause = String.Format("{0}={1}", fromLaneIDNm, fromLaneID);

            IFeatureCursor LaneConnectorCursor1 = _pFeaClsConnector.Search(LCFilter1, false);
            IFeature LaneConnectorRow1 = LaneConnectorCursor1.NextFeature();

            LaneConnectorFeatureService laneConnectorFeatureService = new LaneConnectorFeatureService(_pFeaClsConnector, 0);
            while (LaneConnectorRow1 != null)
            {
                LaneConnector laneConnector = laneConnectorFeatureService.GetLaneConnEty(LaneConnectorRow1);
                if (laneConnector != null)
                {
                    toLaneIDs.Add(laneConnector);
                }
                LaneConnectorRow1 = LaneConnectorCursor1.NextFeature();
            }

            return toLaneIDs;
        }


        public Dictionary<int, List<Lane>> GetTurningLaneDictionary(int fromLaneId)
        {

            Dictionary<int, List<Lane>> turningLanes = new Dictionary<int, List<Lane>>();
            List<LaneConnector> connectors = GetConnectorByFromLane(fromLaneId);

            List<Lane> leftLanes = null;
            List<Lane> rightLanes = null;
            List<Lane> straightLanes = null;
            List<Lane> uturnLanes = null;
            foreach (LaneConnector temConnector in connectors)
            {
                LaneFeatureService laneFeatureService = new LaneFeatureService(_pFeaClsLane, temConnector.toLaneID);
                Lane temLane = laneFeatureService.GetEntity(laneFeatureService.GetFeature());
                if (temConnector.TurningDir.Equals(LaneConnector.TURNING_LEFT))
                {
                    if (leftLanes == null)
                    {
                        leftLanes = new List<Lane>();
                    }
                    leftLanes.Add(temLane);
                }
                else if (temConnector.TurningDir.Equals(LaneConnector.TURNING_RIGHT))
                {
                    if (rightLanes == null)
                    {
                        rightLanes = new List<Lane>();
                    }
                    rightLanes.Add(temLane);
                }
                else if (temConnector.TurningDir.Equals(LaneConnector.TURNING_STRAIGHT))
                {
                    if (straightLanes == null)
                    {
                        straightLanes = new List<Lane>();
                    }
                    straightLanes.Add(temLane);
                }
                else if (temConnector.TurningDir.Equals(LaneConnector.TURNING_UTURN))
                {
                    if (uturnLanes == null)
                    {
                        uturnLanes = new List<Lane>();
                    }
                    uturnLanes.Add(temLane);
                }
                else
                {
                    continue;
                }
            }

            if (null != leftLanes)
            {
                turningLanes.Add(Convert.ToInt32(DataModel.RoadSign.TurnArrow.TURNING_ITEM.左转),
                    leftLanes);
            }

            if (null != straightLanes)
            {
                turningLanes.Add(Convert.ToInt32(DataModel.RoadSign.TurnArrow.TURNING_ITEM.直行),
                    straightLanes);
            }

            if (null != rightLanes)
            {
                turningLanes.Add(Convert.ToInt32(DataModel.RoadSign.TurnArrow.TURNING_ITEM.右转),
                    rightLanes);
            }


            if (null != uturnLanes)
            {
                turningLanes.Add(Convert.ToInt32(DataModel.RoadSign.TurnArrow.TURNING_ITEM.右转),
                    uturnLanes);
            }


            return turningLanes;
        }

        /// <summary>
        /// 默认规则生成两个Arc间的车道连接器
        /// </summary>
        /// <param name="pFeaClsLane"></param>
        /// <param name="fromArcEty"></param>
        /// <param name="toArcEty"></param>
        /// <param name="TurningDir"></param>
        /// <param name="nodePntForLeft"></param>
        public void CreateConnectorInArcs(IFeatureClass pFeaClsLane, Arc fromArcEty,
            Arc toArcEty, string TurningDir, IPoint nodePnt)
        {
            if (fromArcEty == null || toArcEty == null)
            {
                return;
            }
            else
            {

                IFeatureClass pFeaClsNode = (pFeaClsLane.FeatureDataset.Workspace as IFeatureWorkspace).OpenFeatureClass(Node.NodeName);
                IFeatureClass pFeaClsLink = (pFeaClsLane.FeatureDataset.Workspace as IFeatureWorkspace).OpenFeatureClass(Link.LinkName);
                IFeatureClass pFeaClsArc = (pFeaClsLane.FeatureDataset.Workspace as IFeatureWorkspace).OpenFeatureClass(Arc.ArcFeatureName);


                //
                ArcLaneConnectorsPosition arcLaneConnectorsPosition = getArcLanePosition(pFeaClsLink, pFeaClsArc, pFeaClsNode,
                    fromArcEty, toArcEty);


                switch (TurningDir)
                {
                    case LaneConnector.TURNING_RIGHT:
                        {
                            createRightConnectors(pFeaClsLane, fromArcEty, toArcEty,arcLaneConnectorsPosition);
                        } break;
                    case LaneConnector.TURNING_LEFT:
                        {
                            createLeftConnector(pFeaClsLane, fromArcEty, toArcEty, nodePnt,arcLaneConnectorsPosition);
                        } break;
                    case LaneConnector.TURNING_STRAIGHT:
                        {
                            createStraightConnectors(pFeaClsLane, fromArcEty, toArcEty,arcLaneConnectorsPosition);
                        } break;

                    case LaneConnector.TURNING_UTURN:
                        {
                            if (CREATE_UTURN_FLAG)
                            {
                                createUturnConnectors(pFeaClsLane, fromArcEty, toArcEty);
                            }
                        } break;
                    default:
                        break;
                }
            }
        }


        public void AddDeleteLaneConnection(Lane fromLane,
            Dictionary<int,List<Lane>> connectedLanes,
            Dictionary<int,List<Lane>> removedConnectors)
        {
            if(fromLane == null ||
                (connectedLanes == null && 
                removedConnectors == null))
            {
                return;
            }
            ArcService arcService = new ArcService(_pFeaClsArc, fromLane.ArcID);
            Arc fromArc = arcService.GetArcEty(arcService.GetArcFeature());

            
            Node nextNode = PhysicalConnection.getNextNode(_pFeaClsLink,_pFeaClsArc,_pFeaClsNode,fromArc);
            NodeService nodeService = new NodeService(_pFeaClsNode,nextNode.ID,null);
            IFeature nextNodeFeature = nodeService.GetFeature();

            if (connectedLanes != null)
            {
                foreach (int turningIndex in Enum.GetValues(typeof(RoadNetworkSystem.DataModel.RoadSign.TurnArrow.TURNING_ITEM)))
                {
                    if (!connectedLanes.ContainsKey(turningIndex))
                    {
                        continue;
                    }
                    foreach (Lane temLane in connectedLanes[turningIndex])
                    {
                        LaneConnector laneConnector = new LaneConnector();
                        laneConnector.fromLaneID = fromLane.LaneID;
                        laneConnector.fromArcID = fromLane.ArcID;
                        laneConnector.fromLinkID = fromArc.LinkID;
                        laneConnector.fromDir = fromArc.FlowDir;
                        laneConnector.TurningDir = laneConnector.GetTurnDir(turningIndex);
                        LaneFeatureService laneFeatureService = new LaneFeatureService(_pFeaClsLane,fromLane.LaneID);
                        IFeature fromLaneFeature = laneFeatureService.GetFeature();


                        ArcService arcService1 = new ArcService(_pFeaClsArc, temLane.ArcID);
                        Arc toArc = arcService1.GetArcEty(arcService1.GetArcFeature());
                        laneConnector.toLaneID = temLane.LaneID;
                        laneConnector.toArcID = temLane.ArcID;
                        laneConnector.toDir = toArc.FlowDir;
                        laneConnector.toLinkID = toArc.LinkID;

                        laneFeatureService = new LaneFeatureService(_pFeaClsLane,temLane.LaneID);
                        IFeature toLaneFeature = laneFeatureService.GetFeature();

                        bool isStraight = (turningIndex == Convert.ToInt32(RoadNetworkSystem.DataModel.RoadSign.TurnArrow.TURNING_ITEM.直行)?true:false);
                        IPolyline connectorLine = getConnectorShape(fromLaneFeature.Shape as IPolyline,
                            toLaneFeature.Shape as IPolyline,
                            nextNodeFeature.Shape as IPoint,
                            isStraight);

                        InsertLConnector(laneConnector, connectorLine);

                    }
                }
            }


            if (removedConnectors != null)
            {
                foreach (int turningIndex in Enum.GetValues(typeof(RoadNetworkSystem.DataModel.RoadSign.TurnArrow.TURNING_ITEM)))
                {
                    if (!removedConnectors.ContainsKey(turningIndex))
                    {
                        continue;
                    }
                    foreach (Lane lane in removedConnectors[turningIndex])
                    {
                        LaneConnector laneConnector = new LaneConnector();
                        laneConnector.fromLaneID = fromLane.LaneID;
                        laneConnector.fromArcID = fromLane.ArcID;
                        laneConnector.fromLaneID = fromArc.LinkID;
                        laneConnector.fromDir = fromArc.FlowDir;


                        laneConnector.toLaneID = lane.LaneID;
                        int connectorId = GetConnectorIdByLane(fromLane.LaneID,lane.LaneID);
                        if(connectorId > 0)
                        {
                            DeleteConnector(connectorId);
                        }
                    }
                }
            }
            return;
        }


        #region private

        /// <summary>
        /// 获取车道连接器起始Arc的到终止Arc，在fromArc中车道连接器的设置leftPosition与rightPosition
        /// </summary>
        /// <param name="pFeaClsLink"></param>
        /// <param name="pFeaClsArc"></param>
        /// <param name="pFeaClsNode"></param>
        /// <param name="fromArc"></param>
        /// <param name="toArc"></param>
        /// <returns></returns>
        private ArcLaneConnectorsPosition getArcLanePosition(IFeatureClass pFeaClsLink, IFeatureClass pFeaClsArc, IFeatureClass pFeaClsNode,
            Arc fromArc, Arc toArc)
        {
            ArcLaneConnectorsPosition arcLaneConnectorsPosition = new ArcLaneConnectorsPosition();

            List<Arc> clockArcs = PhysicalConnection.GetClockArcs(pFeaClsLink, pFeaClsArc, pFeaClsNode, fromArc, toArc);
            List<Arc> antiClockArcs = PhysicalConnection.GetAntiClockArcs(pFeaClsLink, pFeaClsArc, pFeaClsNode, fromArc, toArc);


            List<Arc> leftTurnArcs = new List<Arc>();
            List<Arc> rightTurnArcs = new List<Arc>();
            List<Arc> straightTurnArcs = new List<Arc>();
            List<Arc> uturnTurnArcs = new List<Arc>();


            LogicalConnection.GetTurnTurningArcs(fromArc, pFeaClsLink, pFeaClsArc, pFeaClsNode,
                ref leftTurnArcs, ref rightTurnArcs, ref straightTurnArcs, ref uturnTurnArcs);

            //到达最左侧的Arc
            if (clockArcs == null || clockArcs.Count == 0)
            {
                arcLaneConnectorsPosition.fromLaneLeftPosition = Lane.LEFT_POSITION;
                //判断下游的是否是单向
                if (LogicalConnection.isOnewayArc(toArc, pFeaClsLink))
                {
                    if (fromArc.LaneNum >= toArc.LaneNum)
                    {
                        arcLaneConnectorsPosition.fromLaneRightPosition = toArc.LaneNum;
                    }
                    else
                    {

                        arcLaneConnectorsPosition.fromLaneRightPosition = fromArc.LaneNum;
                    }
                }
                else
                {
                    if (fromArc.LaneNum <= 2)
                    {
                        arcLaneConnectorsPosition.fromLaneRightPosition = Lane.LEFT_POSITION;
                    }
                    else if (fromArc.LaneNum < 5)
                    {
                        //@1@  2 - 4个车道，从只有第一个车道是到最左侧的Arc
                        arcLaneConnectorsPosition.fromLaneRightPosition = Lane.LEFT_POSITION;
                    }
                    else
                    {
                        //@2@ 多于5个车道，从第3个车道到倒数第2个车道均可到达 中间现有Arc
                        arcLaneConnectorsPosition.fromLaneRightPosition = Lane.LEFT_POSITION + 1;
                    }
                }

                if (arcLaneConnectorsPosition.fromLaneLeftPosition > 0 && arcLaneConnectorsPosition.fromLaneRightPosition > 0)
                {
                    if ((rightTurnArcs.Count == 0 && straightTurnArcs.Count == 0||
                        (rightTurnArcs.Count == 0 && leftTurnArcs.Count == 0))||
                        rightTurnArcs.Count == 0 && leftTurnArcs.Count == 0)
                    {
                        arcLaneConnectorsPosition.fromLaneRightPosition = fromArc.LaneNum - Lane.rightPositionOffset;
                    }
                    return arcLaneConnectorsPosition;
                }
            }


            //到达最右侧的Arc
            if (antiClockArcs == null || antiClockArcs.Count == 0)
            {
                arcLaneConnectorsPosition.fromLaneRightPosition = fromArc.LaneNum;

                //判断下游的是否是单向
                if (LogicalConnection.isOnewayArc(toArc, pFeaClsLink))
                {
                    if (fromArc.LaneNum >= toArc.LaneNum)
                    {
                        //5 -> 3,2
                        arcLaneConnectorsPosition.fromLaneLeftPosition = fromArc.LaneNum - toArc.LaneNum;
                        if (arcLaneConnectorsPosition.fromLaneLeftPosition == 0)
                        {
                            arcLaneConnectorsPosition.fromLaneLeftPosition = Lane.LEFT_POSITION ;
                        }
                    }
                    else
                    {
                        arcLaneConnectorsPosition.fromLaneRightPosition = Lane.LEFT_POSITION;
                    }
                }
                else
                {
                    if (fromArc.LaneNum <= 2)
                    {
                        arcLaneConnectorsPosition.fromLaneLeftPosition = Lane.LEFT_POSITION;
                    }
                    else if (fromArc.LaneNum < 5)
                    {
                        //@1@  2 - 4个车道，从只有最右侧的车道 连接到 最右侧的Arc
                        arcLaneConnectorsPosition.fromLaneLeftPosition = fromArc.LaneNum - Lane.rightPositionOffset;
                    }
                    else
                    {
                        //@2@ 多于5个车道，从只有最右侧的车道 连接到 最右侧的Arc
                        arcLaneConnectorsPosition.fromLaneRightPosition = fromArc.LaneNum - Lane.rightPositionOffset;
                    }
                }
                if (arcLaneConnectorsPosition.fromLaneLeftPosition > 0 && arcLaneConnectorsPosition.fromLaneRightPosition > 0)
                {
                    if ((rightTurnArcs.Count == 0 && straightTurnArcs.Count == 0 ||
                        (rightTurnArcs.Count == 0 && leftTurnArcs.Count == 0)) ||
                        rightTurnArcs.Count == 0 && leftTurnArcs.Count == 0)
                    {
                        arcLaneConnectorsPosition.fromLaneLeftPosition = Lane.LEFT_POSITION;
                    }
                    return arcLaneConnectorsPosition;
                }
            }

            //到达中间Arc
            if (clockArcs != null && antiClockArcs != null)
            {
                if (fromArc.LaneNum <= 2)
                {
                    arcLaneConnectorsPosition.fromLaneLeftPosition = Lane.LEFT_POSITION;
                }
                else if (fromArc.LaneNum < 5)
                {
                    //@1@ 2 - 4个车道，从第2个车道到倒数第2个车道均可到达 中间现有Arc
                    arcLaneConnectorsPosition.fromLaneLeftPosition = Lane.LEFT_POSITION + 1;
                    arcLaneConnectorsPosition.fromLaneRightPosition = fromArc.LaneNum - Lane.rightPositionOffset - 1;
                }
                else
                {
                    //@2@ 多于5个车道，从第3个车道到倒数第2个车道均可到达 中间现有Arc
                    arcLaneConnectorsPosition.fromLaneLeftPosition = Lane.LEFT_POSITION + 2;
                    arcLaneConnectorsPosition.fromLaneRightPosition = fromArc.LaneNum - Lane.rightPositionOffset - 1;
                }

            }
            return arcLaneConnectorsPosition;
        }

        /// <summary>
        /// 通过设定车道连接器的起始车道位置，终止车道的位置，生成车道连接器
        /// </summary>
        /// <param name="fromArc"></param>
        /// <param name="toArc"></param>
        /// <param name="fromLanePostition"></param>
        /// <param name="toLanePosition"></param>
        /// <param name="pFeaClsLane"></param>
        /// <param name="nodePntForLeft"></param>
        /// <param name="isStraignt"></param>
        /// <param name="turningDir"></param>
        private void createConnectorByFromTOPosition(Arc fromArc, Arc toArc,
            int fromLanePostition, int toLanePosition, IFeatureClass pFeaClsLane,
            IPoint nodePntForLeft, bool isStraignt, string turningDir)
        {

            LaneFeatureService laneService = new LaneFeatureService(pFeaClsLane, 0);
            int fromArcID = fromArc.ArcID;

            IFeature fromLaneFea = laneService.QueryFeatureBuRule(fromArcID, fromLanePostition);
            if (fromLaneFea == null)
            {
                return;
            }
            int fromLaneID = Convert.ToInt32(fromLaneFea.get_Value(pFeaClsLane.FindField(Lane.LaneIDNm)));
            IPolyline fromLine = fromLaneFea.ShapeCopy as IPolyline;

            int toArcID = toArc.ArcID;
            IFeature toLaneFea = laneService.QueryFeatureBuRule(toArcID, toLanePosition);
            if (toLaneFea == null)
            {
                return;
            }
            int toLaneID = Convert.ToInt32(toLaneFea.get_Value(pFeaClsLane.FindField(Lane.LaneIDNm)));
            IPolyline toLine = toLaneFea.ShapeCopy as IPolyline;
            IPolyline bezierLine = getConnectorShape(fromLine, toLine, nodePntForLeft, isStraignt);

            LaneConnector connectorEntity = initConnectorEty(0, fromArc.LinkID, toArc.LinkID, fromArcID, toArcID,
                fromArc.FlowDir, toArc.FlowDir, fromLaneID, toLaneID, turningDir);

            InsertLConnector(connectorEntity, bezierLine);
        }
      

        /// <summary>
        /// 创建左转车道连接器
        /// </summary>
        /// <param name="pFeaClsLane"></param>
        /// <param name="fromArc"></param>
        /// <param name="toArc"></param>
        /// <param name="nodePnt"></param>
        /// <param name="arcLaneConnectorsPosition"></param>
        private void createLeftConnector(IFeatureClass pFeaClsLane, Arc fromArc, Arc toArc, IPoint nodePnt,
            ArcLaneConnectorsPosition arcLaneConnectorsPosition)
        {
            if (arcLaneConnectorsPosition.fromLaneLeftPosition <= 0 || 
                arcLaneConnectorsPosition.fromLaneRightPosition <= 0)
            {
                return;
            }
            int fromLanePostition = 0;
            int toLanePosition = 0;
            for(int i = arcLaneConnectorsPosition.fromLaneLeftPosition ; 
                i<= arcLaneConnectorsPosition.fromLaneRightPosition;i++)
            {
                fromLanePostition = i;
                for(int j = Lane.LEFT_POSITION; j <= toArc.LaneNum;j++)
                {
                    toLanePosition = j;
                    createConnectorByFromTOPosition(fromArc,toArc,fromLanePostition,toLanePosition,
                        pFeaClsLane,nodePnt,false,LaneConnector.TURNING_LEFT);
                }
            }
        }

        /// <summary>
        /// 创建直行的车道连接器
        /// </summary>
        /// <param name="pFeaClsLane"></param>
        /// <param name="fromArc"></param>
        /// <param name="toArc"></param>
        /// <param name="arcLaneConnectorsPosition"></param>
        private void createStraightConnectors(IFeatureClass pFeaClsLane, Arc fromArc, Arc toArc, 
            ArcLaneConnectorsPosition arcLaneConnectorsPosition)
        {
            if (arcLaneConnectorsPosition.fromLaneLeftPosition <= 0 || 
                arcLaneConnectorsPosition.fromLaneRightPosition <= 0)
            {
                return;
            }
            int fromLanePostition = 0;
            int toLanePosition = 0;
            for(int i = arcLaneConnectorsPosition.fromLaneLeftPosition ; 
                i<= arcLaneConnectorsPosition.fromLaneRightPosition;i++)
            {
                fromLanePostition = i;
                for(int j = Lane.LEFT_POSITION; j <= toArc.LaneNum;j++)
                {
                    toLanePosition = j;
                    createConnectorByFromTOPosition(fromArc,toArc,fromLanePostition,toLanePosition,
                        pFeaClsLane,null,true,LaneConnector.TURNING_STRAIGHT);
                }
            }
        }

        /// <summary>
        /// 创建右行的车道连接器
        /// </summary>
        /// <param name="pFeaClsLane"></param>
        /// <param name="fromArc"></param>
        /// <param name="toArc"></param>
        /// <param name="arcLaneConnectorsPosition"></param>
        private void createRightConnectors(IFeatureClass pFeaClsLane, Arc fromArc, Arc toArc,
            ArcLaneConnectorsPosition arcLaneConnectorsPosition)
        {
            if (arcLaneConnectorsPosition.fromLaneLeftPosition <= 0 || 
                arcLaneConnectorsPosition.fromLaneRightPosition <= 0)
            {
                return;
            }
            int fromLanePostition = 0;
            int toLanePosition = 0;
            for(int i = arcLaneConnectorsPosition.fromLaneLeftPosition ; 
                i<= arcLaneConnectorsPosition.fromLaneRightPosition;i++)
            {
                fromLanePostition = i;
                for(int j = Lane.LEFT_POSITION; j <= toArc.LaneNum;j++)
                {
                    toLanePosition = j;
                    createConnectorByFromTOPosition(fromArc,toArc,fromLanePostition,toLanePosition,
                        pFeaClsLane,null,false,LaneConnector.TURNING_RIGHT);
                }
            }
        }

      
        /// <summary>
        /// 创建掉头的车道连接器
        /// </summary>
        /// <param name="pFeaClsLane"></param>
        /// <param name="fromArc"></param>
        /// <param name="toArc"></param>
        private void createUturnConnectors(IFeatureClass pFeaClsLane, Arc fromArc, Arc toArc)
        {
            LaneFeatureService lane = new LaneFeatureService(pFeaClsLane, 0);
            int toLinkID = toArc.LinkID;
            int toArcID = toArc.ArcID;
            int toArcDir = toArc.FlowDir;
            int toArcLaneNum = toArc.LaneNum;
            int toLanePosition = -1;
            int toLaneID;
            IFeature toLaneFea = null;
            IPolyline toLine = null;

            int fromLinkID = fromArc.LinkID;
            int fromArcID = fromArc.ArcID;
            int fromArcDir = fromArc.FlowDir;
            int fromArcLaneNum = fromArc.LaneNum;
            int fromLanePostition = -1;
            int fromLaneID;
            IPolyline fromLine = null;
            IFeature fromLaneFea = null;

            IPolyline bezierLine = null;
            if (fromArcLaneNum > 2 && toArcLaneNum > 2)
            {

                fromLanePostition = Lane.LEFT_POSITION;
                toLanePosition = Lane.LEFT_POSITION;

                fromLaneFea = lane.QueryFeatureBuRule(fromArcID, fromLanePostition);
                fromLaneID = Convert.ToInt32(fromLaneFea.get_Value(pFeaClsLane.FindField(Lane.LaneIDNm)));
                fromLine = fromLaneFea.ShapeCopy as IPolyline;

                toLaneFea = lane.QueryFeatureBuRule(toArcID, toLanePosition);
                toLaneID = Convert.ToInt32(toLaneFea.get_Value(pFeaClsLane.FindField(Lane.LaneIDNm)));
                toLine = toLaneFea.ShapeCopy as IPolyline;
                bezierLine = getConnectorShape(fromLine, toLine, null, true);


                LaneConnector connectorEntity = initConnectorEty(0, fromLinkID, toLinkID, fromArcID, toArcID,
                    fromArcDir, toArcDir, fromLaneID, toLaneID, LaneConnector.TURNING_UTURN);

                InsertLConnector(connectorEntity, bezierLine);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromLine"></param>起始车道的几何
        /// <param name="toLine"></param>终止车道的几何
        /// <param name="nodePntForLeft"></param> Node 的几何，当该值为空值，获取两车道交点为中间控制点
        /// <param name="straightFlag"></param>
        /// <returns></returns>
        public IPolyline getConnectorShape(IPolyline fromLine, IPolyline toLine,IPoint nodePnt,bool straightFlag)
        {
            IPolyline bezier = new PolylineClass();
            if (straightFlag == true)
            {
                IPointCollection col = new PolylineClass();
                col.AddPoint(fromLine.ToPoint);
                col.AddPoint(toLine.FromPoint);
                bezier = col as IPolyline;

            }
            else
            {
                IPolyline toExtendLine1 = new PolylineClass();
                toExtendLine1 = LineHelper.CreateLine(toLine, -20, -20);

                IPolyline toExtendLine2 = new PolylineClass();
                toExtendLine2 = LineHelper.CreateLine(fromLine, -20, -20);

                IPoint pnt1 = new PointClass();
                IPoint pnt2 = new PointClass();
                IPoint pntMiddle = new PointClass();
                IRay ray = new RayClass();
                IVector3D vector = new Vector3DClass();

                pnt1 = fromLine.ToPoint;

                pnt2 = toLine.FromPoint;

                //nodePnt是交叉口结点的几何，
                //左转时，认为中间控制点为NodePoint
                if (nodePnt != null)
                {
                    pntMiddle = nodePnt;
                }
                else
                {
                    pntMiddle = LineHelper.GetIntersectionPoint(toExtendLine1, toExtendLine2);
                }

                bezier = LineHelper.DrawBezier(pnt1, pntMiddle, pnt2);
            }


            if (bezier == null)
            {
                IPointCollection col = new PolylineClass();
                col.AddPoint(fromLine.ToPoint);
                col.AddPoint(toLine.FromPoint);
                bezier = col as IPolyline;
            }
            return bezier;

        }


        /// <summary>
        /// 创建一个车道连接器对象
        /// </summary>
        /// <param name="connectorID"></param>
        /// <param name="fromLinkID"></param>
        /// <param name="toLinkID"></param>
        /// <param name="fromArcID"></param>
        /// <param name="toArcID"></param>
        /// <param name="fromArcDir"></param>
        /// <param name="toArcDir"></param>
        /// <param name="fromLaneID"></param>
        /// <param name="toLaneID"></param>
        /// <param name="TurningDir"></param>
        /// <returns></returns>
        private LaneConnector initConnectorEty(int connectorID, int fromLinkID, int toLinkID, int fromArcID,
            int toArcID, int fromArcDir, int toArcDir, int fromLaneID, int toLaneID, string TurningDir)
        {
            LaneConnector connectorEntity = new LaneConnector();
            connectorEntity.ConnectorID = connectorID;
            connectorEntity.fromLinkID = fromLinkID;
            connectorEntity.toLinkID = toLinkID;
            connectorEntity.fromArcID = fromArcID;
            connectorEntity.toArcID = toArcID;
            connectorEntity.fromDir = fromArcDir;
            connectorEntity.toDir = toArcDir;
            connectorEntity.fromLaneID = fromLaneID;
            connectorEntity.toLaneID = toLaneID;
            connectorEntity.TurningDir = TurningDir;
            return connectorEntity;
        }



        #endregion private
    }
}
