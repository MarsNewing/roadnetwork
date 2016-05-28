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

        const int MIN_UNCUT_LINK_LENGTH = 40;

        /// <summary>
        /// key:SegmentID
        /// value: LaneNum(单向Y交叉口，偏移车道数)
        /// </summary>
        Dictionary<int, int> _roadLateralLaneNumPair;

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

            _roadLateralLaneNumPair = new Dictionary<int, int>();
        }
        
        /// <summary>
        /// 创建Arc的
        /// 1--获取当前Arc的关联Arc(上游顺时针Arc，下游逆时针Arc)
        /// 2--
        /// </summary>
        /// <param name="linkFea"></param>
        /// <param name="fNodeFea"></param>
        /// <param name="tNodeFea"></param>
        /// <param name="arc"></param>
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
            PreNodeCutInfor preNodeCutInfo = null;
            NextNodeCutInfor nextNodeCutInfo = null;
            getRelatedArcs(fNode, 
                tNode,
                arc,
                link,
                ref preNodeCutInfo,
                ref nextNodeCutInfo);

            
            //  2.-----------------------创建与Link同向的车道、边界线、Kerb----------------------------------------


            //创建的拓扑
            CreateLaneTopologyInArc(linkFea, link.FlowDir, arc, preNodeCutInfo, nextNodeCutInfo);



            //  3.-----------------------更新Arc的上游Arc----------------------------------------
            //相同方向的Arc的上游Arc存在，则更新
            if (arc.FlowDir == Link.FLOWDIR_SAME &&
                preNodeCutInfo.preArcEty != null)
            {
                updateArc(preNodeCutInfo.preArcEty);
            }

            if (arc.FlowDir == Link.FLOWDIR_OPPOSITION &&
                preNodeCutInfo.preArcEty != null)
            {
                updateArc(preNodeCutInfo.preArcEty);
            }
            //  4.-----------------------更新Arc的下游Arc----------------------------------------
            //相同方向的Arc的上游Arc存在，则更新
            if (arc.FlowDir == Link.FLOWDIR_SAME &&
                nextNodeCutInfo.nextArcEty != null)
            {
                updateArc(nextNodeCutInfo.nextArcEty);
            }

            if (arc.FlowDir == Link.FLOWDIR_OPPOSITION &&
                nextNodeCutInfo.nextArcEty != null)
            {
                updateArc(nextNodeCutInfo.nextArcEty);
            }

            //  5.-------------------更新当前arc的前后端车道连接器---------------------------
            if (arc.FlowDir == Link.FLOWDIR_SAME)
            {
                //添加同向的车道为fromlane的连接器,TNode是入口交叉口
                updateConnetorAndArrow(tNode, tNodeFea.ShapeCopy as IPoint, arc, true);
                //添加同向的车道为tolane的连接器,FNode是出口交叉口
                updateConnetorAndArrow(fNode, fNodeFea.ShapeCopy as IPoint, arc, false);
            }
            else
            {
                //添加同向的车道为fromlane的连接器,TNode是入口交叉口
                updateConnetorAndArrow(tNode, tNodeFea.ShapeCopy as IPoint, arc, false);
                //添加同向的车道为tolane的连接器,FNode是出口交叉口
                updateConnetorAndArrow(fNode, fNodeFea.ShapeCopy as IPoint, arc, true);

            }




            //  6.-------------------创建Arc的Surface---------------------------
            SurfaceService surface = new SurfaceService(_pFeaClsSurface, 0);

            IPolygon gon = new PolygonClass();
            string str = "";
            Surface surfaceEty = new Surface();
            if (arc.FlowDir == Link.FLOWDIR_SAME)
            {
                surface.CrateSurfaceShape(_pFeaClsKerb, nextNodeCutInfo, tNodeFea, fNodeFea, ref gon, ref str);
            }
            else
            {
                surface.CrateSurfaceShape(_pFeaClsKerb, nextNodeCutInfo,  fNodeFea, tNodeFea, ref gon, ref str);
            }
            

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
        /// 创建属于一个Arc的拓扑数据Lane、Boundary、StopLine、Kerb
        /// </summary>
        /// <param name="linkFea"></param>Link要素
        /// <param name="linkFlowDir"></param>Link的交通流方向
        /// <param name="arc"></param>Arc实体
        /// <param name="preNodeCutInfor"></param>Arc上游的截取信息
        /// <param name="nextNodeCutInfor"></param>Arc下游的截取信息
        public void CreateLaneTopologyInArc(IFeature linkFea,
            int linkFlowDir,
            Arc arc,
            PreNodeCutInfor preNodeCutInfor,
            NextNodeCutInfor nextNodeCutInfor)
        {
            
            LinkService linkService = new LinkService(_pFeaClsLink, 0);
            LinkMaster linkMstr = linkService.GetEntity(linkFea);
            Link link = new Link();
            link = link.Copy(linkMstr);

            IPolyline refLinkLine = linkFea.ShapeCopy as IPolyline;

            #region ----------------------------1 删除旧的Kerb Surface--------------------------------------------------
            //删除Arc的所有的Kerb
            KerbService kerbService = new KerbService(_pFeaClsKerb, 0);
            kerbService.DeleteKerbsInArc(arc.ArcID);

            //删除Arc的Surface
            SurfaceService surfaceService = new SurfaceService(_pFeaClsSurface, 0);
            surfaceService.DeleteSurfaceInArc(arc.ArcID);

            #endregion ----------------------------删除旧的Kerb Surface--------------------------------------------------

           
            double curWidth = getOffsetLaneNumInSingleDirectoinY(nextNodeCutInfor,
                preNodeCutInfor,
                arc,
                link);

            //当前车道右侧边界线
            int curBounID = 0;
            //其一车道的右侧边界线，即当前车道的左侧边界线
            int preBounID = 0;
            IPolyline leftBoundaryLine = null;


            //2 生成Arc的Lane、Boundary、StopLine、Kerb

            for (int i = Lane.LEFT_POSITION; i <= arc.LaneNum - Lane.rightPositionOffset; i++)
            {
                //2.1 删除旧的Lane要素，保留属性
                Lane newLaneEty = deleteOldLane(arc.ArcID, i);

                //2.2 根据类型判断截取的大小
                double preCut = 0;
                double nextCut = 0;


                //double laneOffset = curWidth + newLaneEty.Width / 2;
                //curWidth = curWidth + newLaneEty.Width;
                getPreNextCut(preNodeCutInfor, nextNodeCutInfor, newLaneEty, curWidth,
                    ref preCut, ref nextCut);

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

                //2.3 获取Lane和Boundary的几何
                IPolyline laneLine = null;
                IPolyline boundaryLine = null;

                createLaneBoundaryShape(ref curWidth, arc, refLinkLine,
                    newLaneEty, preCut, nextCut, preNodeCutInfor, nextNodeCutInfor,
                    ref laneLine, ref boundaryLine);

                //2.4 创建Lane
                LaneFeatureService laneService = new LaneFeatureService(_pFeaClsLane, newLaneEty.LaneID);
                IFeature laneFeature = laneService.CreateLane(newLaneEty, laneLine);
                newLaneEty = laneService.GetEntity(laneFeature);
                
                //  2.5 创建Lane相关的标线(车行道分界线，控制点，停止线)
                if (linkFlowDir == arc.FlowDir 
                    || linkFlowDir == Link.FLOWDIR_DOUBLE)
                {
                    // 2.5.1 创建车行道分界线
                    preBounID = createBoundary(newLaneEty,
                        arc,
                        preBounID, 
                        curBounID,
                        boundaryLine,
                        laneFeature);

                    // 2.5.2 创建控制点
                    createKerb(newLaneEty,
                        arc,
                        preCut,
                        nextCut,
                        refLinkLine,
                        boundaryLine);



                    // 2.5.3 创建停止线
                    if (newLaneEty.Position == Lane.LEFT_POSITION)
                    {
                        leftBoundaryLine = refLinkLine;
                    }
                    createStopLine(nextNodeCutInfor, leftBoundaryLine, boundaryLine,
                        newLaneEty, arc, refLinkLine);
                    leftBoundaryLine = boundaryLine;

                }
            }
        }


        public void UpdateCenterLine(IFeature linkFeature, Arc sameArc, Arc oppArc)
        {
            BoundaryService boundaryService = new BoundaryService(_pFeaClsBoundary, 0);

            IPolyline cneterLine = boundaryService.GetCenterLineShape(linkFeature,
                sameArc,
                oppArc,
                _pFeaClsLane,
                _pFeaClsKerb);

            Boundary bounEty = new Boundary();
            bounEty.BoundaryID = 0;
            bounEty.Dir = 1;

            int linkFlowDir = Convert.ToInt32(linkFeature.get_Value(linkFeature.Fields.FindField("FlowDir")));
            if (linkFlowDir != Link.FLOWDIR_DOUBLE)
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
            IFeature sameLane = lane1.QueryFeatureBuRule(sameArc.ArcID, 0);
            if (sameLane != null)
            {
                sameLane.set_Value(_pFeaClsLane.FindField(Lane.LeftBoundaryIDNm), bounID);
                sameLane.Store();
            }

            IFeature oppLane = lane1.QueryFeatureBuRule(oppArc.ArcID, 0);
            if (oppLane != null)
            {
                oppLane.set_Value(_pFeaClsLane.FindField(Lane.LeftBoundaryIDNm), bounID);
                oppLane.Store();
            }

        }


        /// <summary>
        /// 获取单向Y字形交叉口中，偏离处主线的出口（入口）Link的偏移车道数
        /// </summary>
        private double getOffsetLaneNumInSingleDirectoinY(NextNodeCutInfor nextNodeCutInfor,
            PreNodeCutInfor preNodeCutInfor,
            Arc arcEty,
            Link link)
        {
            //notice 存在这种情况：y字形交叉口，均为单向，获取下游右侧分支的的起始车道的位置
            int nextLateralLaneNum = 0;
            int preLateralLaneNum = 0;
            if (nextNodeCutInfor.nextArcEty != null)
            {
                nextLateralLaneNum = LogicalConnection.getNextNodeLateralOffsideLanes(_pFeaClsLink,
                    _pFeaClsArc,
                    _pFeaClsNode,
                    nextNodeCutInfor.nextNodeEty,
                    arcEty.ArcID);
            }

            if (preNodeCutInfor.preArcEty != null)
            {
                preLateralLaneNum = LogicalConnection.getPreNodeLateralOffsideLanes(_pFeaClsLink, _pFeaClsArc, _pFeaClsNode,
                    preNodeCutInfor.preNodeEty, arcEty.ArcID);
            }


            double curWidth = 0;
            if (_roadLateralLaneNumPair.ContainsKey(link.RelID))
            {
                curWidth = _roadLateralLaneNumPair[link.RelID] * Lane.LANE_WEIDTH;
            }
            else
            {
                int lateralLaneNum = nextLateralLaneNum > preLateralLaneNum ? nextLateralLaneNum : preLateralLaneNum;
                curWidth = lateralLaneNum * Lane.LANE_WEIDTH;
                _roadLateralLaneNumPair.Add(link.RelID, lateralLaneNum);
            }

            return curWidth;
        }


        /// <summary>
        /// 获取上下游关联的Arc
        /// </summary>
        /// <param name="fNode"></param>
        /// <param name="tNode"></param>
        /// <param name="arc"></param>
        /// <param name="link"></param>
        /// <param name="preNodeCutInfo"></param>
        /// <param name="nextNodeCutInfo"></param>
        private void getRelatedArcs(Node fNode,
            Node tNode,
            Arc arc,
            Link link,
            ref PreNodeCutInfor preNodeCutInfo,
            ref NextNodeCutInfor nextNodeCutInfo)
        {
            if (arc.FlowDir == Link.FLOWDIR_SAME)
            {
                PreNodeCutInforService preNodeCutInforService = new PreNodeCutInforService(_pFeaClsLink, _pFeaClsArc);
                preNodeCutInfo = preNodeCutInforService.GetPreNodeCutInfor(fNode, arc, link);

                NextNodeCutInforService nextNodeCutInforService = new NextNodeCutInforService(_pFeaClsLink, _pFeaClsArc);
                nextNodeCutInfo = nextNodeCutInforService.GetNextNodeCutInfor(tNode, arc, link);
                if (tNode.AdjIDs.Split('\\').Length >= 2 && nextNodeCutInfo.nextArcEty == null)
                {
                    int test = 0;
                    nextNodeCutInforService = new NextNodeCutInforService(_pFeaClsLink, _pFeaClsArc);
                    nextNodeCutInfo = nextNodeCutInforService.GetNextNodeCutInfor(tNode, arc, link);
                    if (tNode.AdjIDs.Split('\\').Length >= 2 && nextNodeCutInfo.nextArcEty == null)
                    {
                        test = 0;
                    }
                }


                if (fNode.AdjIDs.Split('\\').Length >= 2 && preNodeCutInfo.preArcEty == null)
                {
                    int test = 0;
                    preNodeCutInforService = new PreNodeCutInforService(_pFeaClsLink, _pFeaClsArc);
                    preNodeCutInfo = preNodeCutInforService.GetPreNodeCutInfor(fNode, arc, link);

                }

            }
            else
            {

                PreNodeCutInforService preNodeCutInforService = new PreNodeCutInforService(_pFeaClsLink, _pFeaClsArc);
                preNodeCutInfo = preNodeCutInforService.GetPreNodeCutInfor(tNode, arc, link);

                NextNodeCutInforService nextNodeCutInforService = new NextNodeCutInforService(_pFeaClsLink, _pFeaClsArc);
                nextNodeCutInfo = nextNodeCutInforService.GetNextNodeCutInfor(fNode, arc, link);


                if (tNode.AdjIDs.Split('\\').Length >= 2 && preNodeCutInfo.preArcEty == null)
                {
                    int test = 0;
                    preNodeCutInforService = new PreNodeCutInforService(_pFeaClsLink, _pFeaClsArc);
                    preNodeCutInfo = preNodeCutInforService.GetPreNodeCutInfor(tNode, arc, link);

                    if (tNode.AdjIDs.Split('\\').Length >= 2 && preNodeCutInfo.preArcEty == null)
                    {
                        test = 0;

                    }
                }


                if (fNode.AdjIDs.Split('\\').Length >= 2 && nextNodeCutInfo.nextArcEty == null)
                {
                    int test = 0;
                    nextNodeCutInforService = new NextNodeCutInforService(_pFeaClsLink, _pFeaClsArc);
                    nextNodeCutInfo = nextNodeCutInforService.GetNextNodeCutInfor(fNode, arc, link);

                }

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
                        arrow.CreateExitArcArrow(_pFeaClsLane, exitArcEty.ArcID);
                    }
                }
                if (exitArcEtys.Length > 2)
                {
                    //更新入口段的导向箭头，暂且生成两排
                    TurnArrowService arrow1 = new TurnArrowService(_pFeaClsTurnArrow, 0);
                    arrow1.CreateEntranceArcArrow(arcEty.ArcID);
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
                        arrow1.CreateEntranceArcArrow(entranceArcEty.ArcID);

                    }
                }

                if (entranceArcEtys.Length > 2)
                {

                    //出口车道的导向箭头生成一排直行即可
                    TurnArrowService arrow = new TurnArrowService(_pFeaClsTurnArrow, 0);
                    arrow.CreateExitArcArrow(_pFeaClsLane, arcEty.ArcID);
                }
            }
        }

        private void createLaneBoundaryShape(ref double curWidth,
            Arc arcEty,
            IPolyline refLinkLine,
            Lane newLaneEty,
            double preCut,
            double nextCut,
            PreNodeCutInfor preNodeCutInfor,
            NextNodeCutInfor nextNodeCutInfor,
            ref IPolyline laneLine,
            ref IPolyline boundryLine)
        {
            double laneOffset = curWidth + newLaneEty.Width / 2;
            curWidth += newLaneEty.Width;

            double fCut = 0;
            double tCut = 0;
            if (arcEty.FlowDir == Link.FLOWDIR_SAME)
            {
                fCut = preCut;
                tCut = nextCut;
            }
            else
            {
                fCut = nextCut;
                tCut = preCut;
            }

            if (refLinkLine.Length < MIN_UNCUT_LINK_LENGTH)
            {
                laneLine = LineHelper.CreateLineByLRS(refLinkLine, arcEty.FlowDir * laneOffset, 0, 0);
                boundryLine = LineHelper.CreateLineByLRS(refLinkLine, arcEty.FlowDir * curWidth, 0, 0);
            }
            else
            {
                laneLine = LineHelper.CreateLineByLRS(refLinkLine, arcEty.FlowDir * laneOffset,
                fCut + 5, tCut + 5);
                boundryLine = LineHelper.CreateLineByLRS(refLinkLine, arcEty.FlowDir * curWidth, fCut, tCut);
            }

            if (newLaneEty.Position == arcEty.LaneNum)
            {
                createBoundaryForTwoLegs(preNodeCutInfor,
                    nextNodeCutInfor,
                    arcEty.FlowDir,
                    ref boundryLine);
            }

            if (arcEty.FlowDir == Link.FLOWDIR_OPPOSITION)
            {
                laneLine.ReverseOrientation();
                boundryLine.ReverseOrientation();
            }
        }


        /// <summary>
        /// 删除旧的车道，以及与之连接的车道连接器
        /// </summary>
        /// <param name="arcId"></param>
        /// <param name="lanePosition"></param>
        /// <returns></returns>
        private Lane deleteOldLane(int arcId, int lanePosition)
        {
            //先给LaneEntity赋值，因为，后面计算偏移量时要用到车道宽度
            Lane newLaneEty = new Lane();
            newLaneEty.LaneID = LaneFeatureService.GetLaneID(arcId, lanePosition);
            newLaneEty.ArcID = arcId;
            newLaneEty.LaneClosed = 0;

            newLaneEty.LeftBoundaryID = 0;
            newLaneEty.Other = 0;
            newLaneEty.Position = lanePosition;

            newLaneEty.RightBoundaryID = 0;
            newLaneEty.VehClasses = "1";
            newLaneEty.Width = Lane.LANE_WEIDTH;

            LaneFeatureService lane = new LaneFeatureService(_pFeaClsLane, 0);
            IFeature fatherLaneFea = lane.QueryFeatureBuRule(arcId, lanePosition);
            if (fatherLaneFea != null)
            {
                newLaneEty = lane.GetEntity(fatherLaneFea);
                fatherLaneFea.Delete();

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
            }
            return newLaneEty;
        }


        private void getPreNextCut(PreNodeCutInfor preNodeCutInfor,
            NextNodeCutInfor nextNodeCutInfor,
            Lane lane,
            double curWidth,
            ref double preCut,
            ref double nextCut)
        {
            double laneOffset = curWidth + lane.Width / 2;
            curWidth = curWidth + lane.Width;
            if (preNodeCutInfor.cutType == 1)
            {
                ArcService preArc = new ArcService(_pFeaClsArc, preNodeCutInfor.preArcEty.ArcID);
                double preLanesWidth = 0;
                if (!preNodeCutInfor.preNodeEty.IsTwoLegsNode())
                {
                    preLanesWidth = preArc.GetLanesWidth(_pFeaClsLane, preNodeCutInfor.preArcEty.LaneNum);
                }
                else
                {
                    preLanesWidth = preArc.GetLanesWidth(_pFeaClsLane, lane.Position);
                }

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
                double nextLanesWidth = 0;
                if (!nextNodeCutInfor.nextNodeEty.IsTwoLegsNode())
                {
                    nextLanesWidth = nextArc.GetLanesWidth(_pFeaClsLane, nextNodeCutInfor.nextArcEty.LaneNum);
                }
                else
                {
                    nextLanesWidth = nextArc.GetLanesWidth(_pFeaClsLane, lane.Position);
                }
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
                preCut = 1.2 * preCut;

            }
            else
            {
                preCut = 0.7 * preCut;
            }

            if (nextCut > 0)
            {
                nextCut = 1.2 * nextCut;

            }
            else
            {
                nextCut = 0.7 * nextCut;
            }
        }


        private int createBoundary(Lane lane,
            Arc arc,
            int preBounID,
            int curBounID,
            IPolyline boundryLine,
            IFeature laneFeature)
        {
            #region ********************* 2.5.1 创建车道边界线***************************
            Boundary bounEty = new Boundary();
            bounEty.BoundaryID = 0;
            bounEty.Dir = 1;
            if (lane.Position == arc.LaneNum - Lane.rightPositionOffset)
            {
                bounEty.StyleID = Boundary.OUTSIDEBOUNSTYLE;
            }
            else
            {
                if (LogicalConnection.IsArcEntranceInJunction(_pFeaClsArc, _pFeaClsLink, _pFeaClsNode, arc))
                {
                    bounEty.StyleID = Boundary.SOLIDBOUNSTYLE;
                }
                else
                {
                    bounEty.StyleID = Boundary.DASHBOUNSTYLE;
                }
            }

            bounEty.Other = 0;
            BoundaryService boun = new BoundaryService(_pFeaClsBoundary, 0);

            //如果原来车道存在，并且车道边界线也存在，那么就删掉
            if (lane.RightBoundaryID > 0)
            {
                boun = new BoundaryService(_pFeaClsBoundary, lane.RightBoundaryID);
                IFeature parentBounFea = boun.GetFeature();
                parentBounFea.Delete();
            }



            IFeature bounFea = boun.CreateBoundary(bounEty, boundryLine);
            curBounID = Convert.ToInt32(bounFea.get_Value(_pFeaClsBoundary.FindField(Boundary.BOUNDARYID_NAME)));


            //第一个车道先只生成右侧边界线
            //更新车道的边界线
            if (lane.Position == Lane.LEFT_POSITION)
            {

                preBounID = curBounID;
                laneFeature.set_Value(_pFeaClsLane.FindField(Lane.RightBoundaryIDNm), curBounID);
                laneFeature.Store();
            }
            //其他车道左右侧边界线均可生成
            else
            {
                laneFeature.set_Value(_pFeaClsLane.FindField(Lane.LeftBoundaryIDNm), preBounID);
                laneFeature.set_Value(_pFeaClsLane.FindField(Lane.RightBoundaryIDNm), curBounID);
                laneFeature.Store();
                preBounID = curBounID;
            }
            return curBounID;
            #endregion *********************  2.5.1 创建车道边界线***************************
        }



        private void createKerb(Lane lane, Arc arcEty,
            double preCut, double nextCut,
            IPolyline refLinkLine,
            IPolyline boundaryLine)
        {
            //编号2、3的kerb
            if (lane.Position == Lane.LEFT_POSITION)
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
                    nextCut = 5;
                }
                if (preCut < 0)
                {
                    preCut = 5;
                }

                IPoint pnt2 = null;
                if (arcEty.FlowDir == 1)
                {
                    kerbLine = LineHelper.CreateLineByLRS(refLinkLine, 0, preCut, nextCut);
                }
                else
                {
                    kerbLine = LineHelper.CreateLineByLRS(refLinkLine, 0, nextCut, preCut);
                    kerbLine.ReverseOrientation();
                }
                pnt2 = kerbLine.FromPoint;

                kerb.CreateKerb(kerbEty2, pnt2);
                kerbEty3.ArcID = arcEty.ArcID;
                kerbEty3.KerbID = 0;
                kerbEty3.Serial = 3;
                kerbEty3.Other = 0;

                IPoint kerb3 = null;
                kerb3 = kerbLine.ToPoint;
                kerb.CreateKerb(kerbEty3, kerb3);

            }

            //编号0、1的kerb
            if (lane.Position == arcEty.LaneNum - Lane.rightPositionOffset)
            {

                KerbService kerb = new KerbService(_pFeaClsKerb, 0);
                Kerb kerbEty0 = new Kerb();
                Kerb kerbEty1 = new Kerb();
                kerbEty0.ArcID = arcEty.ArcID;
                kerbEty0.KerbID = 0;
                kerbEty0.Serial = 0;
                kerbEty0.Other = 0;

                IPoint pnt0 = null;
                pnt0 = boundaryLine.ToPoint;
                kerb.CreateKerb(kerbEty0, pnt0);


                kerbEty1.ArcID = arcEty.ArcID;
                kerbEty1.KerbID = 0;
                kerbEty1.Serial = 1;
                kerbEty1.Other = 0;

                IPoint pnt1 = null;
                pnt1 = boundaryLine.FromPoint;
                kerb.CreateKerb(kerbEty1, pnt1);
            }
        }


        /// <summary>
        /// 创建车道的停止线
        /// </summary>
        /// <param name="nextNodeCutInfor"></param>
        /// <param name="leftBoundaryLine"></param>
        /// <param name="rightBoundary"></param>
        /// <param name="newLaneEty"></param>
        /// <param name="arcEty"></param>
        /// <param name="refLinkLine"></param>
        private void createStopLine(NextNodeCutInfor nextNodeCutInfor,
            IPolyline leftBoundaryLine,
            IPolyline rightBoundary,
            Lane newLaneEty,
            Arc arcEty,
            IPolyline refLinkLine)
        {
            IPoint leftStopLinePnt = null;

            IPoint rightStopLinePnt = null;
            try
            {
                //路段数大于2时，才生产停车线   
                if (LogicalConnection.IsArcEntranceInJunction(_pFeaClsArc,_pFeaClsLink,_pFeaClsNode,arcEty))
                {
                    rightStopLinePnt = rightBoundary.ToPoint;
                    //当前车道的右侧的车道边界线为右侧相邻车道的左侧边界线
                    leftStopLinePnt = PhysicalTopology.GetNearestPointOnLine(rightStopLinePnt, leftBoundaryLine);
                    IPointCollection stoplineClt = new PolylineClass();
                    stoplineClt.AddPoint(leftStopLinePnt);
                    stoplineClt.AddPoint(rightStopLinePnt);
                    IPolyline stopLineLine = stoplineClt as IPolyline;

                    leftBoundaryLine = rightBoundary;


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
            }
        }


        /// <summary>
        /// 为2个直接相连接的Link最外层车行道分界线创建几何
        /// </summary>
        /// <param name="preNodeCutInfor"></param>
        /// <param name="nextNodeCutInfor"></param>
        /// <param name="boundryLine"></param>
        private void createBoundaryForTwoLegs(PreNodeCutInfor preNodeCutInfor,
            NextNodeCutInfor nextNodeCutInfor,
            int arcFlowDir,
            ref IPolyline boundryLine)
        {
            //为展宽流出现阶段
            int preCutTolerance = 0;
            if (preNodeCutInfor.preNodeEty.IsTwoLegsNode())
            {
                preCutTolerance = 5;
            }

            int nextCutTolerance = 0;
            if (nextNodeCutInfor.nextNodeEty.IsTwoLegsNode())
            {
                nextCutTolerance = 5;
            }

            

            if (preCutTolerance != 0 || nextCutTolerance != 0)
            {
                if (arcFlowDir == Link.FLOWDIR_SAME)
                {
                    boundryLine = LineHelper.CreateLineByLRS(boundryLine, 0, preCutTolerance, nextCutTolerance);
                }
                else
                {
                    boundryLine = LineHelper.CreateLineByLRS(boundryLine, 0,  nextCutTolerance, preCutTolerance);
                }
            }

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


                PreNodeCutInfor preNodeCutInfor = new PreNodeCutInfor();
                NextNodeCutInfor nextNodeCutInfor = new NextNodeCutInfor();
                getRelatedArcs(fNodeEty, tNodeEty, arcEty, linkEty, ref preNodeCutInfor, ref nextNodeCutInfor);

                CreateLaneTopologyInArc(linkFea, linkEty.FlowDir, arcEty, preNodeCutInfor, nextNodeCutInfor);

                SurfaceService surface = new SurfaceService(_pFeaClsSurface, 0);

                IPolygon gon = new PolygonClass();
                string str = "";

                if (linkEty.FlowDir == Link.FLOWDIR_SAME || (linkEty.FlowDir == Link.FLOWDIR_DOUBLE && arcEty.FlowDir == Link.FLOWDIR_SAME))
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
                if (arcEty.FlowDir == Link.FLOWDIR_SAME)
                {
                    ArcService arcService = new ArcService(_pFeaClsArc, 0);
                    Arc oppArc = arcService.GetOppositionArc(arcEty.LinkID);
                    UpdateCenterLine(linkFea, arcEty, oppArc);
                }
                else
                {

                    ArcService arcService = new ArcService(_pFeaClsArc, 0);
                    Arc sameArc = arcService.GetOppositionArc(arcEty.LinkID);
                    UpdateCenterLine(linkFea, sameArc, arcEty);
                }
                

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


        
        
    }
}
