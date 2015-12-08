using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.Road;
using RoadNetworkSystem.DataModel.RoadSign;
using RoadNetworkSystem.GIS;
using RoadNetworkSystem.GIS.Geometry;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.Connection;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LaneLayer;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LinkLayer;
using RoadNetworkSystem.NetworkElement.RoadSignElement;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RoadNetworkSystem.NetworkEditor.EditorFlow
{
    class SegmentConstructor
    {
        private IFeatureClass _pFeaClsNode;
        private IFeatureClass _pFeaClsLink;
        private IFeatureClass _pFeaClsArc;


        private IFeatureClass _pFeaClsLane;
        private IFeatureClass _pFeaClsConnector;
        private IFeatureClass _pFeaClsBoundary;

        private IFeatureClass _pFeaClsKerb;
        private IFeatureClass _pFeaClsTurnArrow;
        private IFeatureClass _pFeaClsStopLine;

        private IFeatureClass _pFeaClsSurface;


        private IFeature _crtLinkFea;
        private LinkEntity _linkEty ;


        private IFeature _fNodeFea;
        private NodeEntity _fNodeEty;
        private NodeEntity _tNodeEty;
        private IFeature _tNodeFea;

        private ArcEntity _sameArcEty;
        private IFeature _sameArcFea;

        private ArcEntity _oppArcEty;
        private IFeature _oppArcFea;

        private const double LANEWEIDTH = 3.5;
        private const double INITFCUR = 5;
        private const double INITTCUT = 10;

        private const int STOPLINESTYLE = -247;
        private const int DASHBOUNSTYLE = -225;
        private const int SOLIDBOUNSTYLE = -227;
        private const int OUTSIDEBOUNSTYLE = 227;
        private const int TURNARROWSTYLE = 227;
        private const int CENTERLINESTYLE = 241;


        public SegmentConstructor(Dictionary<string, IFeatureClass> feaClsDic, IFeature fNodeFea, IFeature tNodeFea)
        {
            _linkEty = new LinkEntity();
            _sameArcEty = new ArcEntity();
            _oppArcEty = new ArcEntity();

            _pFeaClsArc = feaClsDic[ArcEntity.ArcFeatureName];
            _pFeaClsBoundary = feaClsDic[BoundaryEntity.BoundaryName];
            _pFeaClsConnector=feaClsDic[LaneConnectorEntity.ConnectorName];

            _pFeaClsKerb=feaClsDic[KerbEntity.KerbName];
            _pFeaClsLane = feaClsDic[LaneEntity.LaneName];
            _pFeaClsLink = feaClsDic[LinkEntity.LinkName];

            _pFeaClsNode = feaClsDic[NodeEntity.NodeName];
            _pFeaClsStopLine = feaClsDic[StopLineEntity.StopLineName];
            _pFeaClsTurnArrow = feaClsDic[TurnArrowEntity.TurnArrowName];
            _pFeaClsSurface = feaClsDic[SurfaceEntity.SurfaceName];


            _fNodeFea = fNodeFea;
            _tNodeFea = tNodeFea;
        }

        public void CreateLinkTopo(IPolyline linkLine,IFeature fNodeFea,IFeature tNodeFea,int linkRoadType, string linkRoadNm,
            int linkFlowDir, int sameLaneNum, int oppLaneNum)
        {
            _linkEty = new LinkEntity();
            _sameArcEty = new ArcEntity();
            _oppArcEty = new ArcEntity();

            //保存Link
            IFeature crtLinkFea = saveLink(fNodeFea,tNodeFea, linkRoadType, linkRoadNm, linkFlowDir, linkLine);

            //更新FTNode的属性2，并更新_fNodeEty和_tNodeEty
            updateFTNode(fNodeFea,tNodeFea);

            saveArc(linkFlowDir, 1, sameLaneNum, linkLine);
            saveArc(linkFlowDir, -1, oppLaneNum, linkLine);


            try
            {
    //  1.-----------------------获取与Link同向的Arc在Arc上游node (preNode) 和 下游node (nextNode)的 截取类型、连通的Arc的实体、与当前Arc的夹角----------------------------------------
                //根据adjLinks,判断要不要截头截尾
                PreNodeCutInfor samePreNodeCutInfor = getPreNodeCutInfor(_fNodeEty, _sameArcEty, _linkEty);

                NextNodeCutInfor sameNextNodeCutInfor = getNextNodeCutInfor(_tNodeEty, _sameArcEty, _linkEty);
    //  2.-----------------------创建与Link同向的车道、边界线、Kerb----------------------------------------
                
                //创建同向的拓扑
                createLaneTopo(crtLinkFea, linkFlowDir, _sameArcEty, samePreNodeCutInfor, sameNextNodeCutInfor);

                

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
                updateConnetorAndArrow(_tNodeEty, tNodeFea.ShapeCopy as IPoint, _sameArcEty, true);
                //添加同向的车道为tolane的连接器,FNode是出口交叉口
                updateConnetorAndArrow(_fNodeEty, fNodeFea.ShapeCopy as IPoint, _sameArcEty, false);


    //  5.-------------------创建同向Arc的Surface---------------------------
                Surface surface = new Surface(_pFeaClsSurface, 0);

                IPolygon gon = new PolygonClass();
                string str = "";
                surface.CrateSurfaceShape(_pFeaClsKerb, sameNextNodeCutInfor, tNodeFea, fNodeFea,ref gon,ref str);


                SurfaceEntity surfaceEty = new SurfaceEntity();
                surfaceEty.ArcID = _sameArcEty.ArcID;
                surfaceEty.ControlIDs = str;
                surfaceEty.SurfaceID = 0;
                surfaceEty.Other = 0;
                surface.CreateSurface(surfaceEty, gon);


    //  6.-----------------------获取与Link反向的Arc在Arc上游node (preNode) 和 下游node (nextNode)的 截取类型、连通的Arc的实体、与当前Arc的夹角----------------------------------------
                PreNodeCutInfor oppPreNodeCutInfor = getPreNodeCutInfor(_tNodeEty, _oppArcEty, _linkEty);

                NextNodeCutInfor oppNextNodeCutInfor = getNextNodeCutInfor(_fNodeEty, _oppArcEty, _linkEty);

                //创建反的拓扑
                createLaneTopo(crtLinkFea, linkFlowDir, _oppArcEty, oppPreNodeCutInfor, oppNextNodeCutInfor);


              
    //  7.-----------------------更新反向Arc的上游Arc----------------------------------------
                //相反方向的Arc的上游Arc存在，则更新
                if (oppPreNodeCutInfor.preArcEty != null)
                {
                    updateArc(oppPreNodeCutInfor.preArcEty);
                }


    //  8.-----------------------更新反向Arc的下游Arc----------------------------------------
                //相反方向的Arc的下游Arc存在，则更新
                if (oppNextNodeCutInfor.nextArcEty != null)
                {
                    updateArc(oppNextNodeCutInfor.nextArcEty);
                }

                //添加反向的车道为fromlane的连接器,FNode是入口交叉口
                updateConnetorAndArrow(_fNodeEty, fNodeFea.ShapeCopy as IPoint, _oppArcEty, true);
                //添加反向的车道为tolane的连接器,TNode是出口交叉口
                updateConnetorAndArrow(_tNodeEty, tNodeFea.ShapeCopy as IPoint, _oppArcEty, false);


    //  9.-----------------------创建反向Arc的Surface----------------------------------------
                surface.CrateSurfaceShape(_pFeaClsKerb, oppNextNodeCutInfor, fNodeFea, tNodeFea, ref gon, ref str);
                surfaceEty = new SurfaceEntity();
                surfaceEty.ArcID = _oppArcEty.ArcID;
                surfaceEty.ControlIDs = str;
                surfaceEty.SurfaceID = 0;
                surfaceEty.Other = 0;
                surface.CreateSurface(surfaceEty, gon);

    //  10.-----------------------创建当前Link的CenterLine----------------------------------------
                updateCenterLine(_sameArcEty.LinkID);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void updateFTNode(IFeature fNodeFea, IFeature tNodeFea)
        {
            //保存FNode属性2
            Node node = new Node(_pFeaClsNode, 0, null);
            _fNodeEty = new NodeEntity();
            NodeMasterEntity fNodeMasterEty = node.GetNodeMasterEty(fNodeFea);
            _fNodeEty = _fNodeEty.Copy(fNodeMasterEty);
            //保存FNode属性2
            updateNodeAtt2(_fNodeEty.ID, fNodeFea.Shape as IPoint);

            //更新fNodeFea
            node = new Node(_pFeaClsNode, _fNodeEty.ID, fNodeFea.Shape as IPoint);
            fNodeFea = node.GetFeature();

            ///更新fNodeEty
            fNodeMasterEty = node.GetNodeMasterEty(fNodeFea);
            _fNodeEty = _fNodeEty.Copy(fNodeMasterEty);
            string[] adjLinks = _fNodeEty.AdjIDs.Split('\\');
            //相邻路段数大于2，更新NodeType字段
            if (adjLinks.Length > 2)
            {
                fNodeFea.set_Value(_pFeaClsNode.FindField(node.NodeTypeNm), 1);
            }



            //保存TNode属性2
            _tNodeEty = new NodeEntity();
            NodeMasterEntity tNodeMasterEty = node.GetNodeMasterEty(tNodeFea);
            _tNodeEty = _tNodeEty.Copy(tNodeMasterEty);
            //保存FNode属性2
            updateNodeAtt2(_tNodeEty.ID, tNodeFea.Shape as IPoint);
            //更新tNodeFea
            node = new Node(_pFeaClsNode, _tNodeEty.ID, tNodeFea.Shape as IPoint);
            tNodeFea = node.GetFeature();

            ///更新_tNodeEty
            tNodeMasterEty = node.GetNodeMasterEty(tNodeFea);
            _tNodeEty = _tNodeEty.Copy(tNodeMasterEty);
        }

        private void updateNodeAtt2(int nodeID, IPoint pnt)
        {
            Node rdSegNode = new Node(_pFeaClsNode, nodeID, pnt);
            Link rs = new Link(_pFeaClsLink, 0);
            rdSegNode.CreateAdjData(rs);
        }

        private void updateCenterLine(int linkID)
        {
            int sameArcID = 0;
            IPoint sameKerb3 = null;
            int oppArcID = 0;
            IPoint oppKerb3 = null;

            IFeature pArcFea = null;

            Link link=new Link(_pFeaClsLink,linkID);
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = link.IDNm + " = " + linkID;
            IFeatureCursor cursor = _pFeaClsArc.Search(filter, false);
            pArcFea=cursor.NextFeature();

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
                        LaneFeature lane = new LaneFeature(_pFeaClsLane, 0);
                        IFeature laneFea = lane.QueryFeatureBuRule(sameArcID, 0);
                        int centerLineID = Convert.ToInt32(laneFea.get_Value(_pFeaClsLane.FindField(LaneFeature.LeftBoundaryIDNm)));
                        Boundary boun = new Boundary(_pFeaClsBoundary, centerLineID);
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


                    Kerb kerb = new Kerb(_pFeaClsKerb, 0);
                    IFeature kerb3 = kerb.GetKerbByArcAndSerial(sameArcID, 3);
                    sameKerb3 = kerb3.ShapeCopy as IPoint;
                }
                else
                {
                    oppArcID = Convert.ToInt32(pArcFea.get_Value(_pFeaClsArc.FindField(Arc.ArcIDNm)));
                    Kerb kerb = new Kerb(_pFeaClsKerb, 0);
                    IFeature kerb3 = kerb.GetKerbByArcAndSerial(oppArcID, 3);
                    oppKerb3 = kerb3.ShapeCopy as IPoint;

                    if (centerLiuneDltFlag == false)
                    {
                        //删除旧的CenterLine
                        LaneFeature lane = new LaneFeature(_pFeaClsLane, 0);
                        IFeature laneFea = lane.QueryFeatureBuRule(oppArcID, 0);
                        int centerLineID = Convert.ToInt32(laneFea.get_Value(_pFeaClsLane.FindField(LaneFeature.LeftBoundaryIDNm)));
                        Boundary boun = new Boundary(_pFeaClsBoundary, centerLineID);
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
                pArcFea = cursor.NextFeature();

            }
            //System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);

            IPointCollection col = new PolylineClass();
            col.AddPoint(oppKerb3);
            col.AddPoint(sameKerb3);

            IPolyline cneterLine = col as IPolyline;
            BoundaryEntity bounEty = new BoundaryEntity();
            bounEty.BoundaryID = 0;
            bounEty.Dir = 1;
            bounEty.StyleID = CENTERLINESTYLE;
            bounEty.Other = 0;

            //创建道路中心线
            Boundary boun1 = new Boundary(_pFeaClsBoundary, 0);
            IFeature bounFea1 = boun1.CreateBoundary(bounEty, cneterLine);
            int bounID = Convert.ToInt32(bounFea1.get_Value(_pFeaClsBoundary.FindField(Boundary.BoundaryIDNm)));

            LaneFeature lane1=new LaneFeature(_pFeaClsLane,0);

            //更新车道的LeftBoundaryID
            IFeature sameLane = lane1.QueryFeatureBuRule(sameArcID, 0);
            if(sameLane!=null)
            {
                sameLane.set_Value(_pFeaClsLane.FindField(LaneFeature.LeftBoundaryIDNm), bounID);
                sameLane.Store();
            }


            IFeature oppLane = lane1.QueryFeatureBuRule(oppArcID, 0);
            if (oppLane != null)
            {
                oppLane.set_Value(_pFeaClsLane.FindField(LaneFeature.LeftBoundaryIDNm), bounID);
                oppLane.Store();
            }


        }

        /// <summary>
        /// 保存Link
        /// </summary>
        /// <param name="linkRoadType"></param>
        /// <param name="linkRoadNm"></param>
        /// <param name="linkFlowDir"></param>
        private IFeature saveLink(IFeature fNodeFea,IFeature tNodeFea,int linkRoadType,string linkRoadNm,int linkFlowDir,IPolyline linkLine)
        {
            Link link = new Link(_pFeaClsLink, 0);
            Node node = new Node(_pFeaClsNode, 0, null);

            _linkEty.ID = 0;
            _linkEty.RoadType = linkRoadType;
            _linkEty.RoadName = linkRoadNm;
            _linkEty.FNodeID = Convert.ToInt32(fNodeFea.get_Value(_pFeaClsNode.FindField(node.NodeIDNm)));
            _linkEty.TNodeID = Convert.ToInt32(tNodeFea.get_Value(_pFeaClsNode.FindField(node.NodeIDNm)));

            _linkEty.FlowDir = linkFlowDir;
            _linkEty.RoadLevel = _linkEty.RoadType;

            IFeature crtLinkFea = link.Create(_linkEty, linkLine);
            _linkEty.ID = Convert.ToInt32(crtLinkFea.get_Value(_pFeaClsLink.FindField(link.IDNm)));

            return crtLinkFea;
        }

        private void saveArc(int linkFlowDir, int arcFlowDir,int laneNum,IPolyline linkLine)
        {
            ArcEntity arcEty = new ArcEntity();
            #region ++++++++++++++++++++++++++保存Arc+++++++++++++++++++++++++++++++
            arcEty.ArcID = 0;
            Link link = new Link(_pFeaClsLink, 0);

            arcEty.LinkID = _linkEty.ID;

            ///_flowDir已经赋值，用checkbox的状态

            arcEty.FlowDir = arcFlowDir;
            arcEty.LaneNum = laneNum;

            arcEty.Other = 0;


            IPolyline arcLine = LineHelper.CreateLineByLRS(linkLine, LANEWEIDTH * arcEty.FlowDir * arcEty.LaneNum / 2,
                INITFCUR, INITTCUT);

            //获取截头截尾的距离
            Arc arc = new Arc(_pFeaClsArc, 0);

            if (arcFlowDir == 1)
            {
                _sameArcFea = arc.CreateArc(arcEty, arcLine);
                _sameArcEty = arc.GetArcEty(_sameArcFea);
            }
            else
            {
                arcLine.ReverseOrientation();
                _oppArcFea = arc.CreateArc(arcEty, arcLine);
                _oppArcEty = arc.GetArcEty(_oppArcFea);
            }
            #endregion ++++++++++++++++++++++++++保存Arc+++++++++++++++++++++++++++++++

            //先生成Arc，对于Lane和相应的标志标线的生成，就开始考虑截头截尾
            
         
        }

        /// <summary>
        /// 更新Arc所有的拓扑，用于当前Arc变化后，更新上游或下游的Arc
        /// </summary>
        /// <param name="arcEty"></param>上游或下游Arc实体
        private void updateArc(ArcEntity arcEty)
        {
            try
            {

                int linkID = arcEty.LinkID;
                Link link = new Link(_pFeaClsLink, linkID);
                IFeature linkFea = link.GetFeature();
                IPolyline curLinkLine = linkFea.Shape as IPolyline;

                LinkMasterEntity linkMstEty = new LinkMasterEntity();
                linkMstEty = link.GetEntity(linkFea);
                LinkEntity linkEty = new LinkEntity();
                linkEty = linkEty.Copy(linkMstEty);

                NodeEntity fNodeEty = new NodeEntity();
                Node fNode = new Node(_pFeaClsNode, linkEty.FNodeID, null);
                NodeMasterEntity nodeMstEty = new NodeMasterEntity();
                IFeature fNodeFea = fNode.GetFeature();
                nodeMstEty = fNode.GetNodeMasterEty(fNodeFea);
                fNodeEty = fNodeEty.Copy(nodeMstEty);

                NodeEntity tNodeEty = new NodeEntity();
                Node tNode = new Node(_pFeaClsNode, linkEty.TNodeID, null);
                nodeMstEty = new NodeMasterEntity();
                IFeature tNodeFea = tNode.GetFeature();
                nodeMstEty = tNode.GetNodeMasterEty(tNodeFea);
                tNodeEty = tNodeEty.Copy(nodeMstEty);
                PreNodeCutInfor preNodeCutInfor = new PreNodeCutInfor();
                NextNodeCutInfor nextNodeCutInfor = new NextNodeCutInfor();

                if (arcEty.FlowDir == 1)
                {

                    preNodeCutInfor = getPreNodeCutInfor(fNodeEty, arcEty, linkEty);

                    nextNodeCutInfor = getNextNodeCutInfor(tNodeEty, arcEty, linkEty);
                }
                else
                {
                    preNodeCutInfor = getPreNodeCutInfor(tNodeEty, arcEty, linkEty);

                    nextNodeCutInfor = getNextNodeCutInfor(fNodeEty, arcEty, linkEty);
                }
                createLaneTopo(linkFea, linkEty.FlowDir, arcEty, preNodeCutInfor, nextNodeCutInfor);

                Surface surface = new Surface(_pFeaClsSurface, 0);

                IPolygon gon = new PolygonClass();
                string str = "";

                if (arcEty.FlowDir == 1)
                {
                    surface.CrateSurfaceShape(_pFeaClsKerb, nextNodeCutInfor, tNodeFea, fNodeFea, ref gon, ref str);
                }
                else
                {
                    surface.CrateSurfaceShape(_pFeaClsKerb, nextNodeCutInfor, fNodeFea, tNodeFea, ref gon, ref str);
                }

                SurfaceEntity surfaceEty = new SurfaceEntity();
                surfaceEty.ArcID = arcEty.ArcID;
                surfaceEty.ControlIDs = str;
                surfaceEty.SurfaceID = 0;
                surfaceEty.Other = 0;

                surface.CreateSurface(surfaceEty, gon);

                //那就更新CenterLine
                updateCenterLine(arcEty.LinkID);

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

        /// <summary>
        /// 打断Link
        /// </summary>
        /// <param name="fatherLinkFea"></param>
        public void SplitLink(IFeature fatherLinkFea)
        {

            IPolyline fatherLine=fatherLinkFea.ShapeCopy as IPolyline;
            IPolyline spltLine = LineHelper.CreateLineByLRS(fatherLine, 0, 0, fatherLine.Length / 2);
            IPoint spltPnt = spltLine.ToPoint;

            Node spltNode = new Node(_pFeaClsNode, 0, spltPnt);
            NodeEntity spltNodeEty = new NodeEntity();
            spltNodeEty.ID = 0;
            spltNodeEty.AdjIDs = "";
            spltNodeEty.CompositeType = 1;
            spltNodeEty.ConnState = "";
            spltNodeEty.NodeType = 1;
            spltNodeEty.NorthAngles = "";
            spltNodeEty.Other = 0;
            IFeature spltNodeFea = spltNode.CreateNode(spltNodeEty);

            Link spltLink = new Link(_pFeaClsLink, 0);
            LinkMasterEntity linkMstEty = new LinkMasterEntity();
            linkMstEty = spltLink.GetEntity(fatherLinkFea);

            LinkEntity spltLinkEty = new LinkEntity();
            spltLinkEty = spltLinkEty.Copy(linkMstEty);




            int fNodeId = spltLinkEty.FNodeID;
            int tNodeId = spltLinkEty.TNodeID;

            spltNode = new Node(_pFeaClsNode, fNodeId, null);
            IFeature fNodeFea = spltNode.GetFeature();


            spltNode = new Node(_pFeaClsNode, tNodeId, null);
            IFeature tNodeFea = spltNode.GetFeature();

            ArcEntity sameArcEty = new ArcEntity();
            ArcEntity oppArcEty = new ArcEntity();

            Arc arc = new Arc(_pFeaClsArc, 0);
            IFeatureCursor cursor;
            IQueryFilter fitler = new QueryFilterClass();
            fitler.WhereClause = spltLink.IDNm + " = " + spltLinkEty.ID;
            cursor = _pFeaClsArc.Search(fitler, false);
            IFeature pFeaArc = cursor.NextFeature();
            while (pFeaArc != null)
            {
                int flowDir = Convert.ToInt32(pFeaArc.get_Value(_pFeaClsArc.FindField(Arc.FlowDirNm)));
                if (flowDir == 1)
                {
                    sameArcEty = arc.GetArcEty(pFeaArc);
                }
                else
                {
                    oppArcEty = arc.GetArcEty(pFeaArc);
                }
                pFeaArc = cursor.NextFeature();
            }
            //System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);

            //在这里删除Link
            deleteLink(fatherLinkFea);

            IPointCollection pntClt = new PolylineClass();
            pntClt.AddPoint(fNodeFea.ShapeCopy as IPoint);
            pntClt.AddPoint(spltNodeFea.ShapeCopy as IPoint);
            IPolyline newLine1 = pntClt as IPolyline;

            //创建第一个Link，位于Fnode与spltNode之间
            CreateLinkTopo(newLine1,fNodeFea,spltNodeFea, spltLinkEty.RoadType, spltLinkEty.RoadName, spltLinkEty.FlowDir, sameArcEty.LaneNum, oppArcEty.LaneNum);


            pntClt = new PolylineClass();
            pntClt.AddPoint(spltNodeFea.ShapeCopy as IPoint);
            pntClt.AddPoint(tNodeFea.ShapeCopy as IPoint);
            IPolyline newLine2 = pntClt as IPolyline;

            //创建第二个Link，位于spltNode与Tnode之间
            CreateLinkTopo(newLine2,spltNodeFea,tNodeFea, spltLinkEty.RoadType, spltLinkEty.RoadName, spltLinkEty.FlowDir, sameArcEty.LaneNum, oppArcEty.LaneNum);

        }

        private void deleteLink(IFeature linkFea)
        {
            Link link = new Link(_pFeaClsLink, 0);
            int linkID = Convert.ToInt32(linkFea.get_Value(_pFeaClsLink.FindField(link.IDNm)));
            linkFea.Delete();

            Arc arc = new Arc(_pFeaClsArc, 0);
            IFeatureCursor cursor;
            IQueryFilter fitler = new QueryFilterClass();
            fitler.WhereClause = link.IDNm + " = " + linkID;
            cursor = _pFeaClsArc.Search(fitler, false);
            IFeature pFeaArc = cursor.NextFeature();
            while (pFeaArc != null)
            {
                ArcEntity arcEty = arc.GetArcEty(pFeaArc);
                pFeaArc.Delete();

                //删除Arc层次上的东东
                IFeatureCursor cursorKerb;
                IQueryFilter filterKerb = new QueryFilterClass();
                filterKerb.WhereClause = Kerb.ArcIDNm + " = " + arcEty.ArcID;
                cursorKerb = _pFeaClsKerb.Search(filterKerb, false);
                IFeature kerbFea = cursorKerb.NextFeature();
                while (kerbFea != null)
                {
                    kerbFea.Delete();
                    kerbFea = cursorKerb.NextFeature();
                }


                IFeatureCursor cursorSurface;
                IQueryFilter filterSurface= new QueryFilterClass();
                filterSurface.WhereClause = Surface.ArcIDNm + " = " + arcEty.ArcID;
                cursorSurface = _pFeaClsSurface.Search(filterSurface, false);
                IFeature surfaceFea = cursorSurface.NextFeature();
                while (surfaceFea != null)
                {
                    surfaceFea.Delete();
                    surfaceFea = cursorSurface.NextFeature();
                }

                //System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);

                //删除车道层次的东东
                LaneFeature lane = new LaneFeature(_pFeaClsLane, 0);
                IFeatureCursor cursorLane;
                IQueryFilter filterLane = new QueryFilterClass();
                filterLane.WhereClause = LaneFeature.ArcIDNm + " = " + arcEty.ArcID;
                cursorLane = _pFeaClsLane.Search(filterLane, false);
                IFeature laneFea = cursorLane.NextFeature();

                //删除道路中心线
                int centerBoundaryID = 0; 
                if (laneFea != null&& centerBoundaryID== 0)
                {
                    LaneEntity laneEty = new LaneEntity();
                    laneEty = lane.GetEntity(laneFea);
                    if (laneEty.Position == 0)
                    {
                        centerBoundaryID = laneEty.LeftBoundaryID;
                        Boundary boun = new Boundary(_pFeaClsBoundary, centerBoundaryID);
                        IFeature bounFea = boun.GetFeature();
                        if (bounFea != null)
                        {
                            bounFea.Delete();
                        }
                    }
                }
                while (laneFea != null)
                {
                    LaneEntity laneEty = new LaneEntity();
                    laneEty = lane.GetEntity(laneFea);
                    laneFea.Delete();

                    int rigntBounID = laneEty.RightBoundaryID;
                    Boundary boun = new Boundary(_pFeaClsBoundary, rigntBounID);
                    IFeature bounFea = boun.GetFeature();

                    if (bounFea != null)
                    {
                        bounFea.Delete();    
                    }
                    


                    laneFea = cursorLane.NextFeature();
                }
                pFeaArc = cursor.NextFeature();
            }
            //System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
        }

        /// <summary>
        /// 生成和更新Connector和TurnArrow
        /// </summary>
        /// <param name="junctionNodeEty"></param>交叉口Node实体
        /// /// <param name="nodePnt"></param>交叉口Node几何
        /// <param name="arcEty"></param>需要更新的Arc
        /// <param name="entranceFlag"></param> ==true,表示Arc是入口Arc；==false，表示Arc是出口Arc
        private void updateConnetorAndArrow(NodeEntity junctionNodeEty, IPoint nodePnt,ArcEntity arcEty, bool entranceFlag)
        {

            //arcEty是交叉口入口，那么就获取交叉口的所有出口Arc
            //只更新当前arcEty的所有车道的Arrow，出口车道的导向箭头生成一排直行即可
            if (entranceFlag == true)
            {
                ArcEntity[] exitArcEtys = LogicalConnection.GetNodeExitArcs(_pFeaClsLink, _pFeaClsArc, junctionNodeEty);
                for (int i = 0; i < exitArcEtys.Length; i++)
                {
                    ArcEntity exitArcEty = new ArcEntity();
                    exitArcEty = exitArcEtys[i];

                    //去掉断头路
                    if (exitArcEtys.Length == 1 && exitArcEty.LinkID == arcEty.LinkID)
                    {
                        return;
                    }

                    //生成车道连接器，注意，生成前是否要查一下，有没有存在
                    LaneConnectorFeature connector = new LaneConnectorFeature(_pFeaClsConnector, 0);
                    double angle = PhysicalConnection.GetLinksAngle(arcEty.LinkID, exitArcEty.LinkID, junctionNodeEty);

                    connector.CreateConnectorInArcs(_pFeaClsLane, arcEty, exitArcEty, PhysicalConnection.GetTurningDir(angle),nodePnt);
                    
                    //出口车道的导向箭头生成一排直行即可
                    if (exitArcEtys.Length > 2)
                    {
                        TurnArrow arrow = new TurnArrow(_pFeaClsTurnArrow, 0);
                        arrow.createExitArcArrow(_pFeaClsLane, exitArcEty.ArcID);
                    }
                }
                if (exitArcEtys.Length > 2)
                {
                    //更新入口段的导向箭头，暂且生成两排
                    TurnArrow arrow1 = new TurnArrow(_pFeaClsTurnArrow, 0);
                    arrow1.createEntranceArcArrow(_pFeaClsNode, _pFeaClsLink, _pFeaClsArc, _pFeaClsLane, _pFeaClsConnector, arcEty.ArcID);
                }

            }
            //arcEty是交叉口出口，那么就获取交叉口的所有入口Arc
            //出口TurnArrow全为直行，只有一排；所有入口的Arc的所有车道的导向箭头要更新
            else
            {
                ArcEntity[] entranceArcEtys = LogicalConnection.GetNodeEntranceArcs(_pFeaClsLink, _pFeaClsArc, junctionNodeEty);
                for (int i = 0; i < entranceArcEtys.Length; i++)
                {
                    ArcEntity entranceArcEty = new ArcEntity();
                    entranceArcEty = entranceArcEtys[i];

                    //去掉断头路
                    if (entranceArcEtys.Length == 1 && entranceArcEty.LinkID == arcEty.LinkID)
                    {
                        return;
                    }
                    LaneConnectorFeature connector = new LaneConnectorFeature(_pFeaClsConnector, 0);
                    double angle = PhysicalConnection.GetLinksAngle(entranceArcEty.LinkID, arcEty.LinkID, junctionNodeEty);

                    connector.CreateConnectorInArcs(_pFeaClsLane, entranceArcEty, arcEty, PhysicalConnection.GetTurningDir(angle),nodePnt);

                    //两路段不生成导向箭头
                    if (entranceArcEtys.Length > 2)
                    {

                        //更新入口段的导向箭头，暂且生成两排
                        TurnArrow arrow1 = new TurnArrow(_pFeaClsTurnArrow, 0);
                        arrow1.createEntranceArcArrow(_pFeaClsNode, _pFeaClsLink, _pFeaClsArc, _pFeaClsLane, _pFeaClsConnector, entranceArcEty.ArcID);

                    }
                }

                if (entranceArcEtys.Length > 2)
                {

                    //出口车道的导向箭头生成一排直行即可
                    TurnArrow arrow = new TurnArrow(_pFeaClsTurnArrow, 0);
                    arrow.createExitArcArrow(_pFeaClsLane, arcEty.ArcID);
                }
            }
        }




        /// <summary>
        /// 创建属于一个Arc的拓扑数据
        /// </summary>
        /// <param name="linkFea"></param>Link要素
        /// <param name="linkFlowDir"></param>Link的交通流方向
        /// <param name="arcEty"></param>Arc实体
        /// <param name="preNodeCutInfor"></param>Arc上游的截取信息
        /// <param name="nextNodeCutInfor"></param>Arc下游的截取信息
        private void createLaneTopo(IFeature linkFea, int linkFlowDir,ArcEntity arcEty,PreNodeCutInfor preNodeCutInfor,
            NextNodeCutInfor nextNodeCutInfor)
        {
            
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
            //System.Runtime.InteropServices.Marshal.ReleaseComObject(curseorKerb);
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
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            #endregion ----------------------------删除旧的Kerb Surface--------------------------------------------------

            double curWidth = 0;

            //当前车道右侧边界线
            int curBounID = 0;
            //其一车道的右侧边界线，即当前车道的左侧边界线
            int preBounID = 0;
            IPoint leftStopLinePnt = null;
            IPolyline leftBoundaryLine = null;

            IPoint rightStopLinePnt = null;

            #region ----------------------------2 生成Arc的Lane、Boundary、StopLine、Kerb--------------------------------------------------

            for (int i = 0; i < arcEty.LaneNum; i++)
            {
                #region ++++++++++++++++++++++++ 2.1 删除旧的Lane要素，保留属性++++++++++++++++++++++++

                //先给LaneEntity赋值，因为，后面计算偏移量时要用到车道宽度
                LaneEntity newLaneEty = new LaneEntity();
                newLaneEty.LaneID = arcEty.ArcID * 10 + (i + 1);
                newLaneEty.ArcID = arcEty.ArcID;
                newLaneEty.LaneClosed = 0;

                newLaneEty.LeftBoundaryID = 0;
                newLaneEty.Other = 0;
                newLaneEty.Position = i;

                newLaneEty.RightBoundaryID = 0;
                newLaneEty.VehClasses = "1";
                newLaneEty.Width = LANEWEIDTH;

                LaneFeature lane = new LaneFeature(_pFeaClsLane, 0);
                IFeature fatherLaneFea = lane.QueryFeatureBuRule(arcEty.ArcID, i);
                if (fatherLaneFea != null)
                {
                    newLaneEty = lane.GetEntity(fatherLaneFea);
                    fatherLaneFea.Delete();
                    curBounID = newLaneEty.RightBoundaryID;

                    //既然删除了Lane，那就删除以该Lane为起始车道的车道连接器吧
                    IFeatureCursor cursorConn;
                    IQueryFilter filterConn = new QueryFilterClass();
                    filterConn.WhereClause = LaneConnectorFeature.fromLaneIDNm + " = " + newLaneEty.LaneID;
                    cursorConn = _pFeaClsConnector.Search(filterConn, false);
                    IFeature feaConn = cursorConn.NextFeature();
                    while (feaConn != null)
                    {
                        feaConn.Delete();
                        feaConn = cursorConn.NextFeature();
                    }

                    filterConn = new QueryFilterClass();
                    filterConn.WhereClause = LaneConnectorFeature.toLaneIDNm + " = " + newLaneEty.LaneID;
                    cursorConn = _pFeaClsConnector.Search(filterConn, false);
                    feaConn = cursorConn.NextFeature();
                    while (feaConn != null)
                    {
                        feaConn.Delete();
                        feaConn = cursorConn.NextFeature();
                    }
                    //System.Runtime.InteropServices.Marshal.ReleaseComObject(cursorConn);
                }

                #endregion ++++++++++++++++++++++++ 2.1 删除旧的要素，保留属性++++++++++++++++++++++++


                double laneOffset = curWidth + newLaneEty.Width / 2;
                curWidth = curWidth + newLaneEty.Width;
                double preCut = 0;
                double nextCut = 0;


                #region ++++++++++++++++++++++++2.2 根据类型判断截取的大小++++++++++++++++++++++++
                if (preNodeCutInfor.cutType == 1)
                {
                    Arc preArc = new Arc(_pFeaClsArc, preNodeCutInfor.preArcEty.ArcID);
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
                    Arc nextArc = new Arc(_pFeaClsArc,nextNodeCutInfor.nextArcEty.ArcID);
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
                    laneLine = LineHelper.CreateLineByLRS(refLinkLine, arcEty.FlowDir * laneOffset, preCut, nextCut);
                    boundryLine = LineHelper.CreateLineByLRS(refLinkLine, arcEty.FlowDir * curWidth, preCut, nextCut);
                }

                else
                {
                    laneLine = LineHelper.CreateLineByLRS(refLinkLine, arcEty.FlowDir * laneOffset, nextCut, preCut);
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
                            BoundaryEntity bounEty = new BoundaryEntity();
                            bounEty.BoundaryID = 0;
                            bounEty.Dir = 1;
                            bounEty.StyleID = DASHBOUNSTYLE;
                            bounEty.Other = 0;
                            Boundary boun = new Boundary(_pFeaClsBoundary, 0);

                        //如果原来车道存在，并且车道边界线也存在，那么就删掉
                            if (curBounID > 0)
                            {
                                boun = new Boundary(_pFeaClsBoundary, curBounID);
                                IFeature parentBounFea = boun.GetFeature();
                                bounEty = boun.GetEntity(parentBounFea);
                                parentBounFea.Delete();
                            }

                            IFeature bounFea = boun.CreateBoundary(bounEty, boundryLine);
                            curBounID = Convert.ToInt32(bounFea.get_Value(_pFeaClsBoundary.FindField(Boundary.BoundaryIDNm)));

                            //第一个车道先只生成右侧边界线
                            //更新车道的边界线
                            if (i == 0)
                            {
                                
                                preBounID = curBounID;
                                laneFeature.set_Value(_pFeaClsLane.FindField(LaneFeature.RightBoundaryIDNm), curBounID);
                                laneFeature.Store();
                            }
                            //其他车道左右侧边界线均可生成
                            else
                            {
                                laneFeature.set_Value(_pFeaClsLane.FindField(LaneFeature.LeftBoundaryIDNm), preBounID);
                                laneFeature.set_Value(_pFeaClsLane.FindField(LaneFeature.RightBoundaryIDNm), curBounID);
                                laneFeature.Store();
                                preBounID = curBounID;
                            }
                            curBounID = 0;
                        #endregion *********************  2.5.1 创建车道边界线***************************

                        #region ********************* 2.5.2 创建Kerb***************************
                            //编号2、3的kerb
                            if (i == 0)
                            {
                                Kerb kerb = new Kerb(_pFeaClsKerb, 0);
                                KerbEntity kerbEty2 = new KerbEntity();
                                KerbEntity kerbEty3 = new KerbEntity();
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
                            if (i == arcEty.LaneNum - 1)
                            {
                                Kerb kerb = new Kerb(_pFeaClsKerb, 0);
                                KerbEntity kerbEty0 = new KerbEntity();
                                KerbEntity kerbEty1 = new KerbEntity();
                                kerbEty0.ArcID = arcEty.ArcID;
                                kerbEty0.KerbID = 0;
                                kerbEty0.Serial = 0;
                                kerbEty0.Other = 0;
                                //修改最右侧的道路边界线的类型
                                bounFea.set_Value(_pFeaClsBoundary.FindField(Boundary.StyleIDNm), OUTSIDEBOUNSTYLE);
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
                            

                                StopLine stopLine = new StopLine(_pFeaClsStopLine, 0);
                                StopLineEntity stopLineEty = new StopLineEntity();
                                stopLineEty.StopLineID = 0;
                                stopLineEty.ArcID = arcEty.ArcID;
                                stopLineEty.NodeID = nextNodeCutInfor.nextNodeEty.ID;
                                stopLineEty.LaneID = newLaneEty.LaneID;
                                stopLineEty.StyleID = STOPLINESTYLE;

                                stopLine.CreateStopLine(stopLineEty, stopLineLine);
                            }

                        #endregion ********************* 2.5.3 创建StopLine***************************

                    }
                #endregion ---------------------- 2.5 Link的交通流是双向 或 与当前Arc的交通流方向相同时---------------------------------------

            }

            #endregion ----------------------------2 生成Arc的Lane、Boundary、StopLine、Kerb--------------------------------------------------
        }


        public struct NextNodeCutInfor
        {
            public NodeEntity nextNodeEty;
            public int curArcID;
            public ArcEntity nextArcEty;

            /// <summary>
            /// 逆时针方向第一个Link
            /// </summary>
            public double antiClockAngle;


            //截取分为4种情况
            /*
             *(1)相邻Arc都是可通行的 cutType=1
             *(2)当前Arc的可通行，下段Arc是不可通行的 cutType=2
             *(3)当前Arc不可通行，下段rc可通行 cutType=3
             *(4)相邻Arc都是不可通行的 cutType=4
             * 
            */
            public int cutType;
        }


        public struct PreNodeCutInfor
        {
            public NodeEntity preNodeEty;
            public int curArcID;
            public ArcEntity preArcEty;

            /// <summary>
            /// 逆时针方向第一个Link
            /// </summary>
            public double clockAngle;


            //截取分为4种情况
            /*
             *(1)相邻Arc都是可通行的 cutType=1
             *(2)当前Arc可通行，前一段Arc不可通行 cutType=2
             *(3)当前Arc不可通行，前一段Arc不可通行 cutType=3
             *(4)相邻Arc都是不可通行的 cutType=4
             * 
            */
            public int cutType;
        }

        /// <summary>
        /// 获取Arc指向的下一个Node位置处的截取信息
        /// </summary>
        /// <param name="nextNodeEty"></param>
        /// <param name="curArcEty"></param>
        /// <param name="curLinkEty"></param>
        /// <returns></returns>
        private NextNodeCutInfor getNextNodeCutInfor(NodeEntity nextNodeEty, ArcEntity curArcEty, LinkEntity curLinkEty)
        {

            NextNodeCutInfor nextNodeCurInfor = new NextNodeCutInfor();
            nextNodeCurInfor.cutType = -1;
            nextNodeCurInfor.nextArcEty = new ArcEntity();
            nextNodeCurInfor.curArcID=curArcEty.ArcID;

            int cutType=-1;

            ArcEntity nextArcEty = new ArcEntity();
             string[] adjLinkIDs = nextNodeEty.AdjIDs.Split('\\');

            ///单向标志,true表示为单向，false表示双向；
            bool curOneWayFlag = false;

            //下游路段单向标志
            bool nextOneWayFlag = false;

            if (adjLinkIDs.Length > 1)
            {
                #region -------------------------当前路段--------------------------------------

                ///当前Link的信息
                int curLinkID = curArcEty.LinkID;

                int linkFlowDir = curLinkEty.FlowDir;
                //Link的与Arc的FlowDir相同，说明单向
                if (linkFlowDir == curArcEty.FlowDir)
                {
                    curOneWayFlag = true;
                }
                else
                {
                    curOneWayFlag = false;
                }

                #endregion -------------------------当前路段--------------------------------------

                #region -------------------------下游路段--------------------------------------

                ///逆时针方向的第一个Link,下游路段
                int antiClockLinkID = 0;
                ///逆时针方向的夹角
                double antiClockAngle = 0.0;
                PhysicalConnection.GetAntiClockLinkInfor(curLinkID, nextNodeEty, ref antiClockLinkID, ref antiClockAngle);

                Link antiClockLink = new Link(_pFeaClsLink, antiClockLinkID);
                IFeature antiClockLinkFea = antiClockLink.GetFeature();
                LinkMasterEntity linkMasterEty = new LinkMasterEntity();
                linkMasterEty = antiClockLink.GetEntity(antiClockLinkFea);
                LinkEntity antiClockLinkEty = new LinkEntity();
                antiClockLinkEty = antiClockLinkEty.Copy(linkMasterEty);

                //下一段Arc
                IFeature nextArcFea = LogicalConnection.GetExitArc(_pFeaClsArc, nextNodeEty.ID, antiClockLinkEty);
                Arc nextArc = new Arc(_pFeaClsArc, 0);

                nextArcEty = nextArc.GetArcEty(nextArcFea);
                nextArc = new Arc(_pFeaClsArc, nextArcEty.ArcID);

                if (antiClockLinkEty.FlowDir == nextArcEty.FlowDir)
                {
                    nextOneWayFlag = true;
                }
                else
                {
                    nextOneWayFlag = false;
                }

                #endregion -------------------------下游路段--------------------------------------
                


                if (curOneWayFlag == false && nextOneWayFlag == false)
                {
                    cutType = 1;
                }
                else if (curOneWayFlag = false && nextOneWayFlag == true)
                {
                    cutType = 2;
                }
                else if (curOneWayFlag = true && nextOneWayFlag == false)
                {
                    cutType = 3;
                }

                else if (curOneWayFlag == true && nextOneWayFlag == true)
                {
                    cutType = 4;
                }

                nextNodeCurInfor.nextNodeEty = nextNodeEty;
                nextNodeCurInfor.curArcID = curArcEty.ArcID;
                nextNodeCurInfor.nextArcEty = nextArcEty;
                nextNodeCurInfor.antiClockAngle = antiClockAngle;
                nextNodeCurInfor.cutType = cutType;
            }
            else
            {
                nextNodeCurInfor.nextNodeEty = nextNodeEty;
                nextNodeCurInfor.curArcID = curArcEty.ArcID;
                nextNodeCurInfor.nextArcEty = null;
                nextNodeCurInfor.cutType = 2;
                nextNodeCurInfor.antiClockAngle = 0;
            }

            return nextNodeCurInfor;

        }


        /// <summary>
        /// 获取Arc上游的Node位置处的截取信息
        /// </summary>
        /// <param name="preNodeEty"></param>
        /// <param name="curArcEty"></param>
        /// <param name="curLinkEty"></param>
        /// <returns></returns>
         private PreNodeCutInfor getPreNodeCutInfor(NodeEntity preNodeEty, ArcEntity curArcEty, LinkEntity curLinkEty)
        {

            PreNodeCutInfor preNodeCurInfor = new PreNodeCutInfor();
            preNodeCurInfor.cutType = -1;
            preNodeCurInfor.preArcEty = new ArcEntity();
            preNodeCurInfor.curArcID=curArcEty.ArcID;

            int cutType=-1;

            ArcEntity preArcEty = new ArcEntity();
             string[] adjLinkIDs = preNodeEty.AdjIDs.Split('\\');

            ///单向标志,true表示为单向，false表示双向；
            bool curOneWayFlag = false;

            //下游路段单向标志
            bool preOneWayFlag = false;

            if (adjLinkIDs.Length > 1)
            {
                #region -------------------------当前路段--------------------------------------

                ///当前Link的信息
                int curLinkID = curArcEty.LinkID;

                int linkFlowDir = curLinkEty.FlowDir;
                //Link的与Arc的FlowDir相同，说明单向
                if (linkFlowDir == curArcEty.FlowDir)
                {
                    curOneWayFlag = true;
                }
                else
                {
                    curOneWayFlag = false;
                }

                #endregion -------------------------当前路段--------------------------------------

                #region -------------------------下游路段--------------------------------------

                ///逆时针方向的第一个Link,下游路段
                int clockLinkID = 0;
                ///逆时针方向的夹角
                double clockAngle = 0.0;
                PhysicalConnection.GetClockLinkInfor(curLinkID, preNodeEty, ref clockLinkID, ref clockAngle);

                Link clockLink = new Link(_pFeaClsLink, clockLinkID);
                IFeature clockLinkFea = clockLink.GetFeature();
                LinkMasterEntity linkMasterEty = new LinkMasterEntity();
                linkMasterEty = clockLink.GetEntity(clockLinkFea);
                LinkEntity clockLinkEty = new LinkEntity();
                clockLinkEty = clockLinkEty.Copy(linkMasterEty);

                //下一段Arc
                IFeature preArcFea = LogicalConnection.GetEntranceArc(_pFeaClsArc, preNodeEty.ID, clockLinkEty);
                Arc preArc = new Arc(_pFeaClsArc, 0);
                if (preArcFea != null)
                { }

                preArcEty = preArc.GetArcEty(preArcFea);
                preArc = new Arc(_pFeaClsArc, preArcEty.ArcID);

                if (clockLinkEty.FlowDir == preArcEty.FlowDir)
                {
                    preOneWayFlag = true;
                }
                else
                {
                    preOneWayFlag = false;
                }

                #endregion -------------------------下游路段--------------------------------------
                


                if (curOneWayFlag == false && preOneWayFlag == false)
                {
                    cutType = 1;
                }
                else if (curOneWayFlag = false && preOneWayFlag == true)
                {
                    cutType = 2;
                }
                else if (curOneWayFlag = true && preOneWayFlag == false)
                {
                    cutType = 3;
                }

                else if (curOneWayFlag == true && preOneWayFlag == true)
                {
                    cutType = 4;
                }

                preNodeCurInfor.preNodeEty = preNodeEty;
                preNodeCurInfor.curArcID = curArcEty.ArcID;
                preNodeCurInfor.preArcEty = preArcEty;
                preNodeCurInfor.clockAngle = clockAngle;
                preNodeCurInfor.cutType = cutType;
            }
            else
            {
                preNodeCurInfor.preNodeEty = preNodeEty;
                preNodeCurInfor.curArcID = curArcEty.ArcID;
                preNodeCurInfor.preArcEty = null;
                preNodeCurInfor.cutType = 2;
                preNodeCurInfor.clockAngle = 0;
            }

            return preNodeCurInfor;

        }
    
    }
}
