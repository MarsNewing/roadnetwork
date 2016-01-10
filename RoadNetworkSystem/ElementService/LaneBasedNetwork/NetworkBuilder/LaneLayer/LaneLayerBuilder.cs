using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.Road;
using RoadNetworkSystem.DataModel.RoadSign;
using RoadNetworkSystem.ElementService.LaneBasedNetwork.NetworkBuilder;
using RoadNetworkSystem.GIS;
using RoadNetworkSystem.GIS.Geometry;
using RoadNetworkSystem.NetworkEditor;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.Connection;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LaneLayer;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LinkLayer;
using RoadNetworkSystem.NetworkElement.RoadSignElement;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace RoadNetworkSystem.NetworkExtraction.LaneBasedNetwork.LaneLayer
{
    class LaneLayerBuilder
    {
        IFeatureClass _pFeaClsLink;
        IFeatureClass _pFeaClsArc;
        IFeatureClass _pFeaClsNode;

        IFeatureClass _pFeaClsLane;
        IFeatureClass _pFeaClsConnector;
        IFeatureClass _pFeaClsKerb;

        IFeatureClass _pFeaClsSurface;
        IFeatureClass _pFeaClsBoundary;
        IFeatureClass _pFeaClsTurnArrow;

        IFeatureClass _pFeaClsStopLine;
        Dictionary<int, int> roadLateralLaneNumPair;

        public LaneLayerBuilder(Dictionary<string, IFeatureClass> feaClsDic)
        {
            _pFeaClsArc = feaClsDic[Arc.ArcFeatureName];
            _pFeaClsBoundary = feaClsDic[Boundary.BoundaryName];
            _pFeaClsConnector = feaClsDic[LaneConnector.ConnectorName];

            _pFeaClsKerb = feaClsDic[Kerb.KerbName];
            _pFeaClsLane = feaClsDic[Lane.LaneName];
            _pFeaClsLink = feaClsDic[Link.LinkName];
            
            _pFeaClsNode = feaClsDic[Node.NodeName];
            _pFeaClsSurface = feaClsDic[Surface.SurfaceName];
            _pFeaClsTurnArrow = feaClsDic[TurnArrow.TurnArrowName];
            _pFeaClsStopLine = feaClsDic[StopLine.StopLineName];

            roadLateralLaneNumPair = new Dictionary<int, int>();
        }

        public void CreateLinkTopologyBatch()
        {
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = "";
            IFeatureCursor cursor = _pFeaClsLink.Search(filter, false);
            IFeature linkFea = cursor.NextFeature();
            while (linkFea != null)
            {
                CreateLinkTopology(linkFea);
                linkFea = cursor.NextFeature();
            }
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
        }

        public void CreateLinkTopology(IFeature linkFea)
        {
            LinkService linkService = new LinkService(_pFeaClsLink, 0);
            LinkMaster linkMstr = linkService.GetEntity(linkFea);
            Link link = new Link();
            link = link.Copy(linkMstr);

            ArcService arcService = new ArcService(_pFeaClsArc, 0);
            Arc sameArc = arcService.GetSameArc(link.ID);
            Arc oppositionArc = arcService.GetOppositionArc(link.ID);

            NodeService fNodeService = new NodeService(_pFeaClsNode, link.FNodeID,null);
            IFeature fNodeFeature = fNodeService.GetFeature();
            
            NodeService tNodeService = new NodeService(_pFeaClsNode, link.TNodeID, null);
            IFeature tNodeFeature = tNodeService.GetFeature();

            if (sameArc != null)
            {
                CreateArcTopology(linkFea, fNodeFeature, tNodeFeature, sameArc);
            }
            if (oppositionArc != null)
            {
                CreateArcTopology(linkFea, fNodeFeature, tNodeFeature, oppositionArc);
            }

            UpdateCenterLine(link.ID);
        }

        public void CreateArcTopology(IFeature linkFea, IFeature fNodeFea, 
            IFeature tNodeFea, Arc arc)
        {
            LinkService linkService = new LinkService(_pFeaClsLink, 0);
            LinkMaster linkMstr = linkService.GetEntity(linkFea);
            Link link = new Link();
            link = link.Copy(linkMstr);

            NodeService nodeService = new NodeService(_pFeaClsNode,0,null);
            NodeMaster fNodeMstr = nodeService.GetNodeMasterEty(fNodeFea);
            Node fNode = new Node();
            fNode = fNode.Copy(fNodeMstr);

            NodeMaster tNodeMstr = nodeService.GetNodeMasterEty(tNodeFea);
            Node tNode = new Node();
            tNode = tNode.Copy(tNodeMstr);


            //  1.-----------------------获取与Link同向的Arc在Arc上游node (preNode) 和 下游node (nextNode)的 截取类型、连通的Arc的实体、与当前Arc的夹角----------------------------------------
            //根据adjLinks,判断要不要截头截尾
            PreNodeCutInforService preNodeCutInforService = new PreNodeCutInforService(_pFeaClsLink, _pFeaClsArc);
            PreNodeCutInfor samePreNodeCutInfor = preNodeCutInforService.GetPreNodeCutInfor(fNode, arc, link);

            if (arc.ArcID == 70)
            {
                int test = 0;
            }
            NextNodeCutInforService nextNodeCutInforService = new NextNodeCutInforService(_pFeaClsLink, _pFeaClsArc);
            NextNodeCutInfor sameNextNodeCutInfor = nextNodeCutInforService.GetNextNodeCutInfor(tNode, arc, link);
            //  2.-----------------------创建与Link同向的车道、边界线、Kerb----------------------------------------


            //创建同向的拓扑
            CreateLaneTopo(linkFea, link.FlowDir, arc, samePreNodeCutInfor, sameNextNodeCutInfor);



            //  3.-----------------------更新同向Arc的上游Arc----------------------------------------
            //相同方向的Arc的上游Arc存在，则更新
            if (samePreNodeCutInfor.preArcEty != null)
            {
                updateArc(samePreNodeCutInfor.preArcEty);
            }
            //  4.-----------------------更新同向Arc的下游Arc----------------------------------------
            //相同方向的Arc的上游Arc存在，则更新
            if (sameNextNodeCutInfor.nextArcEty != null)
            {
                updateArc(sameNextNodeCutInfor.nextArcEty);
            }

            //添加同向的车道为fromlane的连接器,TNode是入口交叉口
            updateConnetorAndArrow(tNode, tNodeFea.ShapeCopy as IPoint, arc, true);
            //添加同向的车道为tolane的连接器,FNode是出口交叉口
            updateConnetorAndArrow(fNode, fNodeFea.ShapeCopy as IPoint, arc, false);


            //  5.-------------------创建同向Arc的Surface---------------------------
            SurfaceService surface = new SurfaceService(_pFeaClsSurface, 0);

            IPolygon gon = new PolygonClass();
            string str = "";
            Surface surfaceEty = new Surface();
            surface.CrateSurfaceShape(_pFeaClsKerb, sameNextNodeCutInfor, tNodeFea, fNodeFea, ref gon, ref str);

            surfaceEty.ArcID = arc.ArcID;
            surfaceEty.ControlIDs = str;
            surfaceEty.SurfaceID = 0;
            surfaceEty.Other = 0;
            if (gon != null)
            {
                surface.CreateSurface(surfaceEty, gon);
            }
        }

        /// <summary>
        /// 生成和更新Connector和TurnArrow
        /// </summary>
        /// <param name="junctionNodeEty"></param>交叉口Node实体
        /// /// <param name="nodePntForLeft"></param>交叉口Node几何
        /// <param name="arcEty"></param>需要更新的Arc
        /// <param name="entranceFlag"></param> ==true,表示Arc是入口Arc；==false，表示Arc是出口Arc
        private void updateConnetorAndArrow(Node junctionNodeEty, IPoint nodePnt, Arc arcEty, bool entranceFlag)
        {

            //arcEty是交叉口入口，那么就获取交叉口的所有出口Arc
            //只更新当前arcEty的所有车道的Arrow，出口车道的导向箭头生成一排直行即可
            if (entranceFlag == true)
            {
                Arc[] exitArcEtys = LogicalConnection.GetNodeExitArcs(_pFeaClsLink, _pFeaClsArc, junctionNodeEty);
                for (int i = 0; i < exitArcEtys.Length; i++)
                {
                    Arc exitArcEty = new Arc();
                    exitArcEty = exitArcEtys[i];

                    //去掉断头路
                    if (exitArcEtys.Length == 1 && (exitArcEty == null || exitArcEty.LinkID == arcEty.LinkID))
                    {
                        return;
                    }
                    if (exitArcEty == null)
                    {
                        continue;
                    }

                    //生成车道连接器，注意，生成前是否要查一下，有没有存在
                    LaneConnectorFeatureService connector = new LaneConnectorFeatureService(_pFeaClsConnector, 0);
                    double angle = PhysicalConnection.GetLinksAngle(arcEty.LinkID, exitArcEty.LinkID, junctionNodeEty);

                    connector.CreateConnectorInArcs(_pFeaClsLane, arcEty, exitArcEty, PhysicalConnection.GetTurningDir(angle), nodePnt);

                    //出口车道的导向箭头生成一排直行即可
                    if (exitArcEtys.Length > 2)
                    {
                        TurnArrowService arrow = new TurnArrowService(_pFeaClsTurnArrow, 0);
                        arrow.createExitArcArrow(_pFeaClsLane, exitArcEty.ArcID);
                    }
                }
                if (exitArcEtys.Length > 2)
                {
                    //更新入口段的导向箭头，暂且生成两排
                    TurnArrowService arrow1 = new TurnArrowService(_pFeaClsTurnArrow, 0);
                    arrow1.createEntranceArcArrow(_pFeaClsNode, _pFeaClsLink, _pFeaClsArc, _pFeaClsLane, _pFeaClsConnector, arcEty.ArcID);
                }

            }
            //arcEty是交叉口出口，那么就获取交叉口的所有入口Arc
            //出口TurnArrow全为直行，只有一排；所有入口的Arc的所有车道的导向箭头要更新
            else
            {
                Arc[] entranceArcEtys = LogicalConnection.GetNodeEntranceArcs(_pFeaClsLink, _pFeaClsArc, junctionNodeEty);
                for (int i = 0; i < entranceArcEtys.Length; i++)
                {
                    Arc entranceArcEty = new Arc();
                    entranceArcEty = entranceArcEtys[i];

                    //去掉断头路
                    if (entranceArcEtys.Length == 1 && (entranceArcEty == null || entranceArcEty.LinkID == arcEty.LinkID))
                    {
                        return;
                    }
                    if (entranceArcEty == null)
                    {
                        continue;
                    }

                    LaneConnectorFeatureService connector = new LaneConnectorFeatureService(_pFeaClsConnector, 0);
                    double angle = PhysicalConnection.GetLinksAngle(entranceArcEty.LinkID, arcEty.LinkID, junctionNodeEty);

                    if (junctionNodeEty.ID == 26)
                    {
                        int test = 0;
                    }

                    connector.CreateConnectorInArcs(_pFeaClsLane, entranceArcEty, arcEty, PhysicalConnection.GetTurningDir(angle), nodePnt);

                    //两路段不生成导向箭头
                    if (entranceArcEtys.Length > 2)
                    {

                        //更新入口段的导向箭头，暂且生成两排
                        TurnArrowService arrow1 = new TurnArrowService(_pFeaClsTurnArrow, 0);
                        arrow1.createEntranceArcArrow(_pFeaClsNode, _pFeaClsLink, _pFeaClsArc, _pFeaClsLane, _pFeaClsConnector, entranceArcEty.ArcID);

                    }
                }

                if (entranceArcEtys.Length > 2)
                {

                    //出口车道的导向箭头生成一排直行即可
                    TurnArrowService arrow = new TurnArrowService(_pFeaClsTurnArrow, 0);
                    arrow.createExitArcArrow(_pFeaClsLane, arcEty.ArcID);
                }
            }
        }


        /// <summary>
        /// 创建属于一个Arc的拓扑数据Lane、Boundary、StopLine、Kerb
        /// </summary>
        /// <param name="linkFea"></param>Link要素
        /// <param name="linkFlowDir"></param>Link的交通流方向
        /// <param name="arcEty"></param>Arc实体
        /// <param name="preNodeCutInfor"></param>Arc上游的截取信息
        /// <param name="nextNodeCutInfor"></param>Arc下游的截取信息
        public void CreateLaneTopo(IFeature linkFea, int linkFlowDir, Arc arcEty, PreNodeCutInfor preNodeCutInfor,
            NextNodeCutInfor nextNodeCutInfor)
        {
            LinkService linkService = new LinkService(_pFeaClsLink, 0);
            LinkMaster linkMstr = linkService.GetEntity(linkFea);
            Link link = new Link();
            link = link.Copy(linkMstr);

            IPolyline refLinkLine = linkFea.ShapeCopy as IPolyline;
            #region ----------------------------1 删除旧的Kerb Surface--------------------------------------------------
            //删除Arc的所有的Kerb
            IFeatureCursor curseorKerb;
            IQueryFilter filterKerb = new QueryFilterClass();
            filterKerb.WhereClause = Kerb.ArcIDNm + " = " + arcEty.ArcID;
            curseorKerb = _pFeaClsSurface.Search(filterKerb, false);
            IFeature pFeaKerb = curseorKerb.NextFeature();
            while (pFeaKerb != null)
            {
                pFeaKerb.Delete();
                pFeaKerb = curseorKerb.NextFeature();
            }

            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(curseorKerb);
            //删除Arc的Surface
            IFeatureCursor curseorSurface;
            IQueryFilter filterSurface = new QueryFilterClass();
            filterSurface.WhereClause = Surface.ArcIDNm + " = " + arcEty.ArcID;
            curseorSurface = _pFeaClsKerb.Search(filterSurface, false);
            IFeature pFeaSurface = curseorSurface.NextFeature();
            while (pFeaSurface != null)
            {
                pFeaSurface.Delete();
                pFeaSurface = curseorSurface.NextFeature();
            }
            //System.Runtime.InteropServices.Marshal.ReleaseComObject(curseorSurface);
            //System.GC.Collect();
            //System.GC.WaitForPendingFinalizers();
            #endregion ----------------------------删除旧的Kerb Surface--------------------------------------------------



            //notice 存在这种情况：y字形交叉口，均为单向，获取下游右侧分支的的起始车道的位置
            int nextLateralLaneNum = 0;
            int preLateralLaneNum = 0;
            if (nextNodeCutInfor.nextArcEty != null)
            {
                nextLateralLaneNum = LogicalConnection.getNextNodeLateralOffsideLanes(_pFeaClsLink, _pFeaClsArc, _pFeaClsNode, 
                    nextNodeCutInfor.nextNodeEty, arcEty.ArcID);
            }
            
            if(preNodeCutInfor.preArcEty != null)
            {
                preLateralLaneNum = LogicalConnection.getPreNodeLateralOffsideLanes(_pFeaClsLink, _pFeaClsArc, _pFeaClsNode, 
                    preNodeCutInfor.preNodeEty, arcEty.ArcID);
            }


            double curWidth= 0;

            if (roadLateralLaneNumPair.ContainsKey(link.RelID))
            {
                curWidth = roadLateralLaneNumPair[link.RelID] * Lane.LANE_WEIDTH;
            }
            else
            {
                int lateralLaneNum = nextLateralLaneNum > preLateralLaneNum ? nextLateralLaneNum : preLateralLaneNum;
                curWidth = lateralLaneNum * Lane.LANE_WEIDTH;
                roadLateralLaneNumPair.Add(link.RelID, lateralLaneNum);
            }


            
            //当前车道右侧边界线
            int curBounID = 0;
            //其一车道的右侧边界线，即当前车道的左侧边界线
            int preBounID = 0;
            IPoint leftStopLinePnt = null;
            IPolyline leftBoundaryLine = null;

            IPoint rightStopLinePnt = null;

            #region ----------------------------2 生成Arc的Lane、Boundary、StopLine、Kerb--------------------------------------------------

            for (int i = Lane.leftPosition; i <= arcEty.LaneNum - Lane.rightPositionOffset; i++)
            {
                #region ++++++++++++++++++++++++ 2.1 删除旧的Lane要素，保留属性++++++++++++++++++++++++

                //先给LaneEntity赋值，因为，后面计算偏移量时要用到车道宽度
                Lane newLaneEty = new Lane();
                newLaneEty.LaneID = LaneFeatureService.GetLaneID(arcEty.ArcID,i);
                newLaneEty.ArcID = arcEty.ArcID;
                newLaneEty.LaneClosed = 0;

                newLaneEty.LeftBoundaryID = 0;
                newLaneEty.Other = 0;
                newLaneEty.Position = i;

                newLaneEty.RightBoundaryID = 0;
                newLaneEty.VehClasses = "1";
                newLaneEty.Width = Lane.LANE_WEIDTH;

                LaneFeatureService lane = new LaneFeatureService(_pFeaClsLane, 0);
                IFeature fatherLaneFea = lane.QueryFeatureBuRule(arcEty.ArcID, i);
                if (fatherLaneFea != null)
                {
                    newLaneEty = lane.GetEntity(fatherLaneFea);
                    fatherLaneFea.Delete();
                    curBounID = newLaneEty.RightBoundaryID;

                    //既然删除了Lane，那就删除以该Lane为起始车道的车道连接器吧
                    IFeatureCursor cursorConn;
                    IQueryFilter filterConn = new QueryFilterClass();
                    filterConn.WhereClause = LaneConnectorFeatureService.fromLaneIDNm + " = " + newLaneEty.LaneID;
                    cursorConn = _pFeaClsConnector.Search(filterConn, false);
                    IFeature feaConn = cursorConn.NextFeature();
                    while (feaConn != null)
                    {
                        feaConn.Delete();
                        feaConn = cursorConn.NextFeature();
                    }

                    filterConn = new QueryFilterClass();
                    filterConn.WhereClause = LaneConnectorFeatureService.toLaneIDNm + " = " + newLaneEty.LaneID;
                    cursorConn = _pFeaClsConnector.Search(filterConn, false);
                    feaConn = cursorConn.NextFeature();
                    while (feaConn != null)
                    {
                        feaConn.Delete();
                        feaConn = cursorConn.NextFeature();
                    }
                    //System.Runtime.InteropServices.Marshal.ReleaseComObject(cursorConn);

                    //System.GC.Collect();
                    //System.GC.WaitForPendingFinalizers();
                }

                #endregion ++++++++++++++++++++++++ 2.1 删除旧的要素，保留属性++++++++++++++++++++++++


                double laneOffset = curWidth + newLaneEty.Width / 2;
                curWidth = curWidth + newLaneEty.Width;
                double preCut = 0;
                double nextCut = 0;


                #region ++++++++++++++++++++++++2.2 根据类型判断截取的大小++++++++++++++++++++++++
                if (preNodeCutInfor.cutType == 1)
                {
                    ArcService preArc = new ArcService(_pFeaClsArc, preNodeCutInfor.preArcEty.ArcID);
                    double preLanesWidth = preArc.GetLanesWidth(_pFeaClsLane, i);
                    preCut = CutHelper.getCutDefualt(curWidth, preLanesWidth, preNodeCutInfor.clockAngle);
                }
                else if (preNodeCutInfor.cutType == 2)
                {
                    preCut = CutHelper.getCutOneway(curWidth, preNodeCutInfor.clockAngle);
                }
                else
                {
                    preCut = 0;
                }
                //尾端截取值
                if (nextNodeCutInfor.cutType == 1)
                {
                    ArcService nextArc = new ArcService(_pFeaClsArc, nextNodeCutInfor.nextArcEty.ArcID);
                    double nextLanesWidth = nextArc.GetLanesWidth(_pFeaClsLane, i);
                    nextCut = CutHelper.getCutDefualt(curWidth, nextLanesWidth, nextNodeCutInfor.antiClockAngle);
                }
                else if (nextNodeCutInfor.cutType == 2)
                {
                    nextCut = CutHelper.getCutOneway(curWidth, nextNodeCutInfor.antiClockAngle);
                }
                else
                {
                    nextCut = 0;
                }

                if (preCut > 0)
                {
                    preCut = 1.5 * preCut;

                }
                else
                {
                    preCut = 0.7 * preCut;
                }

                if (nextCut > 0)
                {
                    nextCut = 1.5 * nextCut;

                }
                else
                {
                    nextCut = 0.7 * nextCut;
                }

                #endregion ++++++++++++++++++++++++2.2 根据类型判断截取的大小++++++++++++++++++++++++

                #region +++++++++++++++++++++++++++++++++++++++++++示意图+++++++++++++++++++++++++++++++++++
                /* 
                 *                           + (TNode)   
                 *                           | 
                 *              preCut  *    |    +  (nextCut)
                 *                      |    |    |
                 *                      |    |    |
                 *                      |    |    |
                 *                      |    |    |
                 *                      |    |    | 
                 *                      |    |    |
                 *                      |    |    |
                 *                      |    |    |
                 *                      |    |    |
                 *             nextCut  +    |    *  (preCut)
                 *                           |
                 *                           * (FNode)   
                 */
                #endregion +++++++++++++++++++++++++++++++++++++++++++示意图+++++++++++++++++++++++++++++++++++


                #region ++++++++++++++++++++++++2.3 获取Lane和Boundary的几何++++++++++++++++++++++++

                IPolyline laneLine = null;
                IPolyline boundryLine = null;
                if (arcEty.FlowDir == 1)
                {
                    laneLine = LineHelper.CreateLineByLRS(refLinkLine, arcEty.FlowDir * laneOffset,
                        refLinkLine.Length * ArcService.ARC_CUT_PERCENTAGE, refLinkLine.Length * ArcService.ARC_CUT_PERCENTAGE);
                    boundryLine = LineHelper.CreateLineByLRS(refLinkLine, arcEty.FlowDir * curWidth, preCut, nextCut);
                }

                else
                {
                    laneLine = LineHelper.CreateLineByLRS(refLinkLine, arcEty.FlowDir * laneOffset,
                        refLinkLine.Length * ArcService.ARC_CUT_PERCENTAGE, refLinkLine.Length * ArcService.ARC_CUT_PERCENTAGE);
                    laneLine.ReverseOrientation();

                    boundryLine = LineHelper.CreateLineByLRS(refLinkLine, arcEty.FlowDir * curWidth, nextCut, preCut);
                }
                #endregion ++++++++++++++++++++++++2.3 获取Lane和Boundary的几何++++++++++++++++++++++++

                #region ++++++++++++++++++++++++ 2.4 生成车道++++++++++++++++++++++++

                IFeature laneFeature = lane.CreateLane(newLaneEty, laneLine);
                #endregion ++++++++++++++++++++++++ 2.4 生成车道++++++++++++++++++++++++

                #region ++++++++++++++++++++++++ 2.5 Link的交通流是双向 或 与当前Arc的交通流方向相同时++++++++++++++++++++++++-----------------
                //当，才生成相应
                if (linkFlowDir == arcEty.FlowDir || linkFlowDir == 0)
                {
                    #region ********************* 2.5.1 创建车道边界线***************************
                    Boundary bounEty = new Boundary();
                    bounEty.BoundaryID = 0;
                    bounEty.Dir = 1;
                    bounEty.StyleID = Boundary.DASHBOUNSTYLE;
                    bounEty.Other = 0;
                    BoundaryService boun = new BoundaryService(_pFeaClsBoundary, 0);

                    //如果原来车道存在，并且车道边界线也存在，那么就删掉
                    if (curBounID > 0)
                    {
                        boun = new BoundaryService(_pFeaClsBoundary, curBounID);
                        IFeature parentBounFea = boun.GetFeature();
                        bounEty = boun.GetEntity(parentBounFea);
                        parentBounFea.Delete();
                    }

                    IFeature bounFea = boun.CreateBoundary(bounEty, boundryLine);
                    curBounID = Convert.ToInt32(bounFea.get_Value(_pFeaClsBoundary.FindField(Boundary.BOUNDARYID_NAME)));

                    //第一个车道先只生成右侧边界线
                    //更新车道的边界线
                    if (i == 0)
                    {

                        preBounID = curBounID;
                        laneFeature.set_Value(_pFeaClsLane.FindField(LaneFeatureService.RightBoundaryIDNm), curBounID);
                        laneFeature.Store();
                    }
                    //其他车道左右侧边界线均可生成
                    else
                    {
                        laneFeature.set_Value(_pFeaClsLane.FindField(LaneFeatureService.LeftBoundaryIDNm), preBounID);
                        laneFeature.set_Value(_pFeaClsLane.FindField(LaneFeatureService.RightBoundaryIDNm), curBounID);
                        laneFeature.Store();
                        preBounID = curBounID;
                    }
                    curBounID = 0;
                    #endregion *********************  2.5.1 创建车道边界线***************************

                    #region ********************* 2.5.2 创建Kerb***************************
                    //编号2、3的kerb
                    if (i == Lane.leftPosition)
                    {
                        KerbService kerb = new KerbService(_pFeaClsKerb, 0);
                        Kerb kerbEty2 = new Kerb();
                        Kerb kerbEty3 = new Kerb();
                        kerbEty2.ArcID = arcEty.ArcID;
                        kerbEty2.KerbID = 0;
                        kerbEty2.Serial = 2;
                        kerbEty2.Other = 0;

                        IPolyline kerbLine = null;

                        //编号2、3的Kerb限定在Node与Node之间
                        if (nextCut < 0)
                        {
                            nextCut = 3;
                        }
                        if (preCut < 0)
                        {
                            preCut = 3;
                        }

                        IPoint pnt2 = null;
                        if (arcEty.FlowDir == 1)
                        {
                            kerbLine = LineHelper.CreateLineByLRS(refLinkLine, 0, preCut, nextCut);
                            pnt2 = kerbLine.FromPoint;
                        }
                        else
                        {
                            kerbLine = LineHelper.CreateLineByLRS(refLinkLine, 0, nextCut, preCut);
                            pnt2 = kerbLine.ToPoint;
                        }

                        kerb.CreateKerb(kerbEty2, pnt2);
                        kerbEty3.ArcID = arcEty.ArcID;
                        kerbEty3.KerbID = 0;
                        kerbEty3.Serial = 3;
                        kerbEty3.Other = 0;

                        IPoint kerb3 = null;
                        if (arcEty.FlowDir == 1)
                        {
                            kerb3 = kerbLine.ToPoint;
                        }
                        else
                        {
                            kerb3 = kerbLine.FromPoint;
                        }
                        kerb.CreateKerb(kerbEty3, kerb3);

                        //第一个车道生成Kerb的线，用来定位停车线的左端点
                        leftBoundaryLine = kerbLine;
                    }

                    //编号0、1的kerb
                    if (i == arcEty.LaneNum - Lane.rightPositionOffset)
                    {
                        KerbService kerb = new KerbService(_pFeaClsKerb, 0);
                        Kerb kerbEty0 = new Kerb();
                        Kerb kerbEty1 = new Kerb();
                        kerbEty0.ArcID = arcEty.ArcID;
                        kerbEty0.KerbID = 0;
                        kerbEty0.Serial = 0;
                        kerbEty0.Other = 0;
                        //修改最右侧的道路边界线的类型
                        bounFea.set_Value(_pFeaClsBoundary.FindField(Boundary.STYLEID_NAME), Boundary.OUTSIDEBOUNSTYLE);
                        bounFea.Store();

                        IPoint pnt0 = null;
                        if (arcEty.FlowDir == 1)
                        {
                            pnt0 = boundryLine.ToPoint;
                        }
                        else
                        {
                            pnt0 = boundryLine.FromPoint;
                        }
                        kerb.CreateKerb(kerbEty0, pnt0);


                        kerbEty1.ArcID = arcEty.ArcID;
                        kerbEty1.KerbID = 0;
                        kerbEty1.Serial = 1;
                        kerbEty1.Other = 0;

                        IPoint pnt1 = null;
                        if (arcEty.FlowDir == 1)
                        {
                            pnt1 = boundryLine.FromPoint;
                        }
                        else
                        {
                            pnt1 = boundryLine.ToPoint;
                        }
                        kerb.CreateKerb(kerbEty1, pnt1);
                    }

                    #endregion ********************* 2.5.2 创建Kerb***************************

                    #region ********************* 2.5.3 创建StopLine***************************

                    try
                    {

                        //路段数大于2时，才生产停车线   
                        if (nextNodeCutInfor.nextNodeEty.AdjIDs.Split('\\').Length > 2)
                        {

                            if (arcEty.FlowDir == 1)
                            {
                                rightStopLinePnt = boundryLine.ToPoint;
                            }
                            else
                            {
                                rightStopLinePnt = boundryLine.FromPoint;
                            }

                            //当前车道的右侧的车道边界线为右侧相邻车道的左侧边界线
                            leftStopLinePnt = PhysicalTopology.GetNearestPointOnLine(rightStopLinePnt, leftBoundaryLine);
                            IPointCollection stoplineClt = new PolylineClass();
                            stoplineClt.AddPoint(leftStopLinePnt);
                            stoplineClt.AddPoint(rightStopLinePnt);
                            IPolyline stopLineLine = stoplineClt as IPolyline;

                            leftBoundaryLine = boundryLine;


                            StopLineService stopLine = new StopLineService(_pFeaClsStopLine, 0);
                            StopLine stopLineEty = new StopLine();
                            stopLineEty.StopLineID = 0;
                            stopLineEty.ArcID = arcEty.ArcID;
                            stopLineEty.NodeID = nextNodeCutInfor.nextNodeEty.ID;
                            stopLineEty.LaneID = newLaneEty.LaneID;
                            stopLineEty.StyleID = StopLine.STOPLINESTYLE;

                            stopLine.CreateStopLine(stopLineEty, stopLineLine);
                        }

                    }
                    catch (Exception ex)
                    {
                        Node temNode2 = nextNodeCutInfor.nextNodeEty;
                    }
                    #endregion ********************* 2.5.3 创建StopLine***************************

                }
                #endregion ---------------------- 2.5 Link的交通流是双向 或 与当前Arc的交通流方向相同时---------------------------------------
            }

            #endregion ----------------------------2 生成Arc的Lane、Boundary、StopLine、Kerb--------------------------------------------------
        }

        /// <summary>
        /// 更新Arc所有的拓扑，用于当前Arc变化后，更新上游或下游的Arc
        /// </summary>
        /// <param name="arcEty"></param>上游或下游Arc实体
        private void updateArc(Arc arcEty)
        {
            try
            {

                int linkID = arcEty.LinkID;
                LinkService link = new LinkService(_pFeaClsLink, linkID);
                IFeature linkFea = link.GetFeature();
                IPolyline curLinkLine = linkFea.Shape as IPolyline;

                LinkMaster linkMstEty = new LinkMaster();
                linkMstEty = link.GetEntity(linkFea);
                Link linkEty = new Link();
                linkEty = linkEty.Copy(linkMstEty);

                Node fNodeEty = new Node();
                NodeService fNode = new NodeService(_pFeaClsNode, linkEty.FNodeID, null);
                NodeMaster nodeMstEty = new NodeMaster();
                IFeature fNodeFea = fNode.GetFeature();
                nodeMstEty = fNode.GetNodeMasterEty(fNodeFea);
                fNodeEty = fNodeEty.Copy(nodeMstEty);

                Node tNodeEty = new Node();
                NodeService tNode = new NodeService(_pFeaClsNode, linkEty.TNodeID, null);
                nodeMstEty = new NodeMaster();
                IFeature tNodeFea = tNode.GetFeature();
                nodeMstEty = tNode.GetNodeMasterEty(tNodeFea);
                tNodeEty = tNodeEty.Copy(nodeMstEty);
                PreNodeCutInforService preNodeCutInforService = new PreNodeCutInforService(_pFeaClsLink,_pFeaClsArc);
                NextNodeCutInforService nextNodeCutInforService = new NextNodeCutInforService(_pFeaClsLink, _pFeaClsArc);
                PreNodeCutInfor preNodeCutInfor = new PreNodeCutInfor();
                NextNodeCutInfor nextNodeCutInfor = new NextNodeCutInfor();

                if (arcEty.FlowDir == 1)
                {

                    preNodeCutInfor = preNodeCutInforService.GetPreNodeCutInfor(fNodeEty, arcEty, linkEty);

                    nextNodeCutInfor = nextNodeCutInforService.GetNextNodeCutInfor(tNodeEty, arcEty, linkEty);

                }
                else
                {
                    preNodeCutInfor = preNodeCutInforService.GetPreNodeCutInfor(tNodeEty, arcEty, linkEty);

                    nextNodeCutInfor = nextNodeCutInforService.GetNextNodeCutInfor(fNodeEty, arcEty, linkEty);
                }


                CreateLaneTopo(linkFea, linkEty.FlowDir, arcEty, preNodeCutInfor, nextNodeCutInfor);

                SurfaceService surface = new SurfaceService(_pFeaClsSurface, 0);

                IPolygon gon = new PolygonClass();
                string str = "";

                if (linkEty.FlowDir == Link.FLOWDIR_SAME || (linkEty.FlowDir == Link.FLOWDIR_DOUBLE && arcEty.FlowDir == 1))
                {
                    surface.CrateSurfaceShape(_pFeaClsKerb, nextNodeCutInfor, tNodeFea, fNodeFea, ref gon, ref str);
                }
                else if (linkEty.FlowDir == Link.FLOWDIR_OPPOSITION ||
                    (linkEty.FlowDir == Link.FLOWDIR_DOUBLE && arcEty.FlowDir == -1))
                {
                    surface.CrateSurfaceShape(_pFeaClsKerb, nextNodeCutInfor, fNodeFea, tNodeFea, ref gon, ref str);
                }

                Surface surfaceEty = new Surface();
                surfaceEty.ArcID = arcEty.ArcID;
                surfaceEty.ControlIDs = str;
                surfaceEty.SurfaceID = 0;
                surfaceEty.Other = 0;
                if (gon != null)
                {
                    surface.CreateSurface(surfaceEty, gon);
                }
                //那就更新CenterLine
                UpdateCenterLine(arcEty.LinkID);

                //那就更新车道连接器吧

                if (arcEty.FlowDir == 1)
                {
                    //添加同向的车道为fromlane的连接器,TNode是入口交叉口
                    updateConnetorAndArrow(tNodeEty, tNodeFea.ShapeCopy as IPoint, arcEty, true);
                    //添加同向的车道为tolane的连接器,FNode是出口交叉口
                    updateConnetorAndArrow(fNodeEty, fNodeFea.ShapeCopy as IPoint, arcEty, false);
                }
                else
                {
                    //添加同向的车道为fromlane的连接器,TNode是入口交叉口
                    updateConnetorAndArrow(fNodeEty, fNodeFea.ShapeCopy as IPoint, arcEty, true);
                    //添加同向的车道为tolane的连接器,FNode是出口交叉口
                    updateConnetorAndArrow(tNodeEty, tNodeFea.ShapeCopy as IPoint, arcEty, false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        public void UpdateCenterLine(int linkID)
        {
            int sameArcID = 0;
            IPoint sameKerb3 = null;
            int oppArcID = 0;
            IPoint oppKerb3 = null;

            IFeature pArcFea = null;

            LinkService link = new LinkService(_pFeaClsLink, linkID);

            IFeature pFeatureLink = link.GetFeature();
            LinkMaster linkMaster = link.GetEntity(pFeatureLink);
            Link linkEty = new Link();
            linkEty = linkEty.Copy(linkMaster);

            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = link.IDNm + " = " + linkID;
            IFeatureCursor cursor = _pFeaClsArc.Search(filter, false);
            pArcFea = cursor.NextFeature();

            //中心线是否被删除掉了，true 删除了；false 没被删除
            bool centerLiuneDltFlag = false;

            while (pArcFea != null)
            {
                int flowDir = Convert.ToInt32(pArcFea.get_Value(_pFeaClsArc.FindField(Arc.FlowDirNm)));
                if (flowDir == 1)
                {
                    sameArcID = Convert.ToInt32(pArcFea.get_Value(_pFeaClsArc.FindField(Arc.ArcIDNm)));


                    if (centerLiuneDltFlag == false)
                    {
                        //删除旧的CenterLine
                        LaneFeatureService lane = new LaneFeatureService(_pFeaClsLane, 0);
                        IFeature laneFea = lane.QueryFeatureBuRule(sameArcID, 0);
                        if (laneFea == null)
                        {
                            pArcFea = cursor.NextFeature();
                            continue;
                        }
                        int centerLineID = Convert.ToInt32(laneFea.get_Value(_pFeaClsLane.FindField(LaneFeatureService.LeftBoundaryIDNm)));
                        BoundaryService boun = new BoundaryService(_pFeaClsBoundary, centerLineID);
                        IFeature bounFea = boun.GetFeature();
                        if (bounFea != null)
                        {
                            bounFea.Delete();
                            centerLiuneDltFlag = true;
                        }
                        else
                        {
                            centerLiuneDltFlag = true;
                        }
                    }

                    if (linkEty.FlowDir == Link.FLOWDIR_DOUBLE || linkEty.FlowDir == Link.FLOWDIR_SAME)
                    {
                        KerbService kerb = new KerbService(_pFeaClsKerb, 0);
                        IFeature kerb3 = kerb.GetKerbByArcAndSerial(sameArcID, 3);
                        sameKerb3 = kerb3.ShapeCopy as IPoint;
                    }

                }
                else
                {
                    if (linkEty.FlowDir == Link.FLOWDIR_DOUBLE || linkEty.FlowDir == Link.FLOWDIR_OPPOSITION)
                    {

                        oppArcID = Convert.ToInt32(pArcFea.get_Value(_pFeaClsArc.FindField(Arc.ArcIDNm)));
                        KerbService kerb = new KerbService(_pFeaClsKerb, 0);
                        IFeature kerb3 = kerb.GetKerbByArcAndSerial(oppArcID, 3);
                        if (kerb3 == null)
                        {
                            pArcFea = cursor.NextFeature();
                            continue;
                        }
                        oppKerb3 = kerb3.ShapeCopy as IPoint;

                        if (centerLiuneDltFlag == false)
                        {
                            //删除旧的CenterLine
                            LaneFeatureService lane = new LaneFeatureService(_pFeaClsLane, 0);
                            IFeature laneFea = lane.QueryFeatureBuRule(oppArcID, 0);
                            int centerLineID = Convert.ToInt32(laneFea.get_Value(_pFeaClsLane.FindField(LaneFeatureService.LeftBoundaryIDNm)));
                            BoundaryService boun = new BoundaryService(_pFeaClsBoundary, centerLineID);
                            IFeature bounFea = boun.GetFeature();
                            if (bounFea != null)
                            {
                                bounFea.Delete();
                                centerLiuneDltFlag = true;
                            }
                            else
                            {
                                centerLiuneDltFlag = true;
                            }
                        }
                    }


                }
                pArcFea = cursor.NextFeature();
            }
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
            //System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);

            IPointCollection col = new PolylineClass();
            if (linkEty.FlowDir != Link.FLOWDIR_DOUBLE)
            {
                col.AddPoint((pFeatureLink.Shape as IPolyline).FromPoint);
                col.AddPoint((pFeatureLink.Shape as IPolyline).ToPoint);
            }
            else
            {
                if (oppKerb3 != null)
                {
                    col.AddPoint(oppKerb3);
                }
                if (sameKerb3 != null)
                {
                    col.AddPoint(sameKerb3);
                }
            }

            IPolyline cneterLine = col as IPolyline;
            Boundary bounEty = new Boundary();
            bounEty.BoundaryID = 0;
            bounEty.Dir = 1;
            if (linkEty.FlowDir != 0)
            {
                bounEty.StyleID = Boundary.OUTSIDEBOUNSTYLE;
            }
            else
            {
                bounEty.StyleID = Boundary.CENTERLINESTYLE;
            }

            bounEty.Other = 0;

            //创建道路中心线
            BoundaryService boun1 = new BoundaryService(_pFeaClsBoundary, 0);
            IFeature bounFea1 = boun1.CreateBoundary(bounEty, cneterLine);
            int bounID = Convert.ToInt32(bounFea1.get_Value(_pFeaClsBoundary.FindField(Boundary.BOUNDARYID_NAME)));

            LaneFeatureService lane1 = new LaneFeatureService(_pFeaClsLane, 0);

            //更新车道的LeftBoundaryID
            IFeature sameLane = lane1.QueryFeatureBuRule(sameArcID, 0);
            if (sameLane != null)
            {
                sameLane.set_Value(_pFeaClsLane.FindField(LaneFeatureService.LeftBoundaryIDNm), bounID);
                sameLane.Store();
            }


            IFeature oppLane = lane1.QueryFeatureBuRule(oppArcID, 0);
            if (oppLane != null)
            {
                oppLane.set_Value(_pFeaClsLane.FindField(LaneFeatureService.LeftBoundaryIDNm), bounID);
                oppLane.Store();
            }


        }


        /// <summary>
        /// 
        /// </summary>
        public void InitLaneBatch()
        {
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = "";
            IFeatureCursor cursor = _pFeaClsArc.Search(filter, false);
            IFeature pFeature = cursor.NextFeature();
            while (pFeature != null)
            {
                ArcService arcService = new ArcService(_pFeaClsArc, 0);
                Arc cursorArc = arcService.GetArcEty(pFeature);
                LinkService linkService = new LinkService(_pFeaClsLink,cursorArc.LinkID);
                IFeature linkFeature = linkService.GetFeature();
                for (int i = Lane.leftPosition; i <= (cursorArc.LaneNum - Lane.rightPositionOffset); i++)
                {
                    Lane lane = new Lane();
                    lane.LaneID = LaneFeatureService.GetLaneID(cursorArc.ArcID, i);
                    lane.ArcID=  cursorArc.ArcID;
                    lane.Position = i;
                    lane.LaneClosed= Lane.LANE_UNCLOSED;
                    if(i == Lane.leftPosition)
                    {
                         
                        if(i ==  cursorArc.LaneNum - Lane.rightPositionOffset)
                        {
                            lane.Change = Lane.CHANGE_NEITHER;
                        }
                        else
                        {
                            lane.Change = Lane.CHANGE_RIGHT;
                        }
                    }

                    else if(i == cursorArc.LaneNum - Lane.rightPositionOffset)
                    {
                        lane.Change = Lane.CHANGE_LEFT;
                    }
                    else
                    {
                        lane.Change = Lane.CHANGE_BOTH;
                    }
                    lane.Width= Lane.LANE_WEIDTH;

                    IPolyline laneLine = LineHelper.CreateLineByLRS(linkFeature.Shape as IPolyline,(lane.Position-0.5)*Lane.LANE_WEIDTH,0,0);

                    LaneFeatureService laneFeatureService = new LaneFeatureService(_pFeaClsLane, lane.LaneID);
                    laneFeatureService.CreateLane(lane, laneLine);

                }
                pFeature = cursor.NextFeature();
            }
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
        }

    }
}
