using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.Road;
using RoadNetworkSystem.DataModel.RoadSign;
using RoadNetworkSystem.ElementService.LaneBasedNetwork.NetworkBuilder;
using RoadNetworkSystem.GIS;
using RoadNetworkSystem.GIS.Geometry;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.Connection;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LaneLayer;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LinkLayer;
using RoadNetworkSystem.NetworkElement.RoadSignElement;
using RoadNetworkSystem.NetworkExtraction.LaneBasedNetwork.LaneLayer;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RoadNetworkSystem.NetworkEditor.EditorFlow
{
    class SegmentConstructor
    {
        private Dictionary<string, IFeatureClass> _feaClsDic;
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
        private Link _linkEty ;


        private IFeature _fNodeFea;
        private Node _fNodeEty;
        private Node _tNodeEty;
        private IFeature _tNodeFea;

        private Arc _sameArcEty;
        private IFeature _sameArcFea;

        private Arc _oppArcEty;
        private IFeature _oppArcFea;

        private const double INITFCUR = 5;
        private const double INITTCUT = 10;

        
        

        public SegmentConstructor(Dictionary<string, IFeatureClass> feaClsDic, IFeature fNodeFea, IFeature tNodeFea)
        {
            _feaClsDic = feaClsDic;
            _linkEty = new Link();
            _sameArcEty = new Arc();
            _oppArcEty = new Arc();

            _pFeaClsArc = feaClsDic[Arc.ArcFeatureName];
            _pFeaClsBoundary = feaClsDic[Boundary.BoundaryName];
            _pFeaClsConnector=feaClsDic[LaneConnector.ConnectorName];

            _pFeaClsKerb=feaClsDic[Kerb.KerbName];
            _pFeaClsLane = feaClsDic[Lane.LaneName];
            _pFeaClsLink = feaClsDic[Link.LinkName];

            _pFeaClsNode = feaClsDic[Node.NodeName];
            _pFeaClsStopLine = feaClsDic[StopLine.StopLineName];
            _pFeaClsTurnArrow = feaClsDic[TurnArrow.TurnArrowName];
            _pFeaClsSurface = feaClsDic[Surface.SurfaceName];


            _fNodeFea = fNodeFea;
            _tNodeFea = tNodeFea;
        }

        public void CreateLinkTopo(IPolyline linkLine,IFeature fNodeFea,IFeature tNodeFea,int linkRoadType, string linkRoadNm,
            int linkFlowDir, int sameLaneNum, int oppLaneNum)
        {
            if (fNodeFea == null || tNodeFea == null)
            {
                //MessageBox.Show("确认fnode 和 tnode 为非空");
                return;
            }
            _linkEty = new Link();
            _sameArcEty = null;
            _oppArcEty = null;

            //保存Link
            IFeature crtLinkFea = saveLink(fNodeFea,tNodeFea, linkRoadType, linkRoadNm, linkFlowDir, linkLine);

            //更新FTNode的属性2，并更新_fNodeEty和_tNodeEty
            updateFTNode(fNodeFea, tNodeFea);
            if (linkFlowDir == Link.FLOWDIR_DOUBLE || linkFlowDir == Link.FLOWDIR_SAME)
            {
                saveArc(linkFlowDir, 1, sameLaneNum, linkLine);
            }

            
            if (linkFlowDir == Link.FLOWDIR_DOUBLE || linkFlowDir == Link.FLOWDIR_OPPOSITION)
            {
                saveArc(linkFlowDir, -1, oppLaneNum, linkLine);
            }


            try
            {
                LaneLayerFactory laneLayerFactory = new LaneLayerFactory(_feaClsDic);
                if (_sameArcEty != null)
                {
                    laneLayerFactory.CreateArcTopology(_crtLinkFea, fNodeFea, tNodeFea, _sameArcEty);
                    
                    //  1.-----------------------获取与Link同向的Arc在Arc上游node (preNode) 和 下游node (nextNode)的 截取类型、连通的Arc的实体、与当前Arc的夹角----------------------------------------
                    //根据adjLinks,判断要不要截头截尾
                    //PreNodeCutInforService preNodeCutInforService = new PreNodeCutInforService(_pFeaClsLink, _pFeaClsArc);
                    //PreNodeCutInfor samePreNodeCutInfor = preNodeCutInforService.GetPreNodeCutInfor(_fNodeEty, _sameArcEty, _linkEty);

                    //NextNodeCutInforService nextNodeCutInforService = new NextNodeCutInforService(_pFeaClsLink, _pFeaClsArc);
                    //NextNodeCutInfor sameNextNodeCutInfor = nextNodeCutInforService.GetNextNodeCutInfor(_tNodeEty, _sameArcEty, _linkEty);
                    ////  2.-----------------------创建与Link同向的车道、边界线、Kerb----------------------------------------


                    ////创建同向的拓扑
                    //createLaneTopo(crtLinkFea, linkFlowDir, _sameArcEty, samePreNodeCutInfor, sameNextNodeCutInfor);



                    ////  3.-----------------------更新同向Arc的上游Arc----------------------------------------
                    ////相同方向的Arc的上游Arc存在，则更新
                    //if (samePreNodeCutInfor.preArcEty != null)
                    //{
                    //    updateArc(samePreNodeCutInfor.preArcEty);
                    //}
                    ////  4.-----------------------更新同向Arc的下游Arc----------------------------------------
                    ////相同方向的Arc的上游Arc存在，则更新
                    //if (sameNextNodeCutInfor.nextArcEty != null)
                    //{
                    //    updateArc(sameNextNodeCutInfor.nextArcEty);
                    //}

                    ////添加同向的车道为fromlane的连接器,TNode是入口交叉口
                    //updateConnetorAndArrow(_tNodeEty, tNodeFea.ShapeCopy as IPoint, _sameArcEty, true);
                    ////添加同向的车道为tolane的连接器,FNode是出口交叉口
                    //updateConnetorAndArrow(_fNodeEty, fNodeFea.ShapeCopy as IPoint, _sameArcEty, false);


                    ////  5.-------------------创建同向Arc的Surface---------------------------
                    //SurfaceService surface = new SurfaceService(_pFeaClsSurface, 0);

                    //IPolygon gon = new PolygonClass();
                    //string str = "";
                    //Surface surfaceEty = new Surface();
                    //surface.CrateSurfaceShape(_pFeaClsKerb, sameNextNodeCutInfor, tNodeFea, fNodeFea,
                    //    ref gon, ref str);

                    //surfaceEty.ArcID = _sameArcEty.ArcID;
                    //surfaceEty.ControlIDs = str;
                    //surfaceEty.SurfaceID = 0;
                    //surfaceEty.Other = 0;
                    //surface.CreateSurface(surfaceEty, gon);

                }


                if (_oppArcEty != null)
                {

                    
                    laneLayerFactory.CreateArcTopology(_crtLinkFea, fNodeFea, tNodeFea, _oppArcEty);
                    

                    ////  6.-----------------------获取与Link反向的Arc在Arc上游node (preNode) 和 下游node (nextNode)的 截取类型、连通的Arc的实体、与当前Arc的夹角----------------------------------------
                    //PreNodeCutInforService preNodeCutInforService = new PreNodeCutInforService(_pFeaClsLink, _pFeaClsArc);
                    //PreNodeCutInfor oppPreNodeCutInfor = preNodeCutInforService.GetPreNodeCutInfor(_tNodeEty, _oppArcEty, _linkEty);

                    //NextNodeCutInforService nextNodeCutInforService = new NextNodeCutInforService(_pFeaClsLink, _pFeaClsArc);
                    //NextNodeCutInfor oppNextNodeCutInfor = nextNodeCutInforService.GetNextNodeCutInfor(_fNodeEty, _oppArcEty, _linkEty);

                    ////创建反的拓扑
                    //createLaneTopo(crtLinkFea, linkFlowDir, _oppArcEty, oppPreNodeCutInfor, oppNextNodeCutInfor);



                    ////  7.-----------------------更新反向Arc的上游Arc----------------------------------------
                    ////相反方向的Arc的上游Arc存在，则更新
                    //if (oppPreNodeCutInfor.preArcEty != null)
                    //{
                    //    updateArc(oppPreNodeCutInfor.preArcEty);
                    //}


                    ////  8.-----------------------更新反向Arc的下游Arc----------------------------------------
                    ////相反方向的Arc的下游Arc存在，则更新
                    //if (oppNextNodeCutInfor.nextArcEty != null)
                    //{
                    //    updateArc(oppNextNodeCutInfor.nextArcEty);
                    //}

                    ////添加反向的车道为fromlane的连接器,FNode是入口交叉口
                    //updateConnetorAndArrow(_fNodeEty, fNodeFea.ShapeCopy as IPoint, _oppArcEty, true);
                    ////添加反向的车道为tolane的连接器,TNode是出口交叉口
                    //updateConnetorAndArrow(_tNodeEty, tNodeFea.ShapeCopy as IPoint, _oppArcEty, false);


                    ////  9.-----------------------创建反向Arc的Surface----------------------------------------
                    //SurfaceService surface = new SurfaceService(_pFeaClsSurface, 0);

                    //IPolygon gon = new PolygonClass();
                    //string str = "";
                    //Surface surfaceEty = new Surface();

                    //surface.CrateSurfaceShape(_pFeaClsKerb, oppNextNodeCutInfor, fNodeFea, tNodeFea, ref gon, ref str);
                    //surfaceEty = new Surface();
                    //surfaceEty.ArcID = _oppArcEty.ArcID;
                    //surfaceEty.ControlIDs = str;
                    //surfaceEty.SurfaceID = 0;
                    //surfaceEty.Other = 0;
                    //surface.CreateSurface(surfaceEty, gon);
                }

    //  10.-----------------------创建当前Link的CenterLine----------------------------------------
                laneLayerFactory.UpdateCenterLine(_sameArcEty.LinkID);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void updateFTNode(IFeature fNodeFea, IFeature tNodeFea)
        {
            //保存FNode属性2
            NodeService node = new NodeService(_pFeaClsNode, 0, null);
            _fNodeEty = new Node();
            NodeMaster fNodeMasterEty = node.GetNodeMasterEty(fNodeFea);
            _fNodeEty = _fNodeEty.Copy(fNodeMasterEty);
            //保存FNode属性2
            updateNodeAtt2(_fNodeEty.ID, fNodeFea.Shape as IPoint);

            //更新fNodeFea
            node = new NodeService(_pFeaClsNode, _fNodeEty.ID, fNodeFea.Shape as IPoint);
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
            _tNodeEty = new Node();
            NodeMaster tNodeMasterEty = node.GetNodeMasterEty(tNodeFea);
            _tNodeEty = _tNodeEty.Copy(tNodeMasterEty);
            //保存FNode属性2
            updateNodeAtt2(_tNodeEty.ID, tNodeFea.Shape as IPoint);
            //更新tNodeFea
            node = new NodeService(_pFeaClsNode, _tNodeEty.ID, tNodeFea.Shape as IPoint);
            tNodeFea = node.GetFeature();

            ///更新_tNodeEty
            tNodeMasterEty = node.GetNodeMasterEty(tNodeFea);
            _tNodeEty = _tNodeEty.Copy(tNodeMasterEty);
        }

        private void updateNodeAtt2(int nodeID, IPoint pnt)
        {
            NodeService rdSegNode = new NodeService(_pFeaClsNode, nodeID, pnt);
            LinkService rs = new LinkService(_pFeaClsLink, 0);
            rdSegNode.CreateAdjData(rs);
        }

        

        /// <summary>
        /// 保存Link
        /// </summary>
        /// <param name="linkRoadType"></param>
        /// <param name="linkRoadNm"></param>
        /// <param name="linkFlowDir"></param>
        private IFeature saveLink(IFeature fNodeFea,IFeature tNodeFea,int linkRoadType,string linkRoadNm,int linkFlowDir,IPolyline linkLine)
        {
            if (fNodeFea == null || tNodeFea == null)
            {
                return null;
            }

            LinkService link = new LinkService(_pFeaClsLink, 0);
            NodeService node = new NodeService(_pFeaClsNode, 0, null);


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
            Arc arcEty = new Arc();
            #region ++++++++++++++++++++++++++保存Arc+++++++++++++++++++++++++++++++
            arcEty.ArcID = 0;
            LinkService link = new LinkService(_pFeaClsLink, 0);

            arcEty.LinkID = _linkEty.ID;

            ///_flowDir已经赋值，用checkbox的状态

            arcEty.FlowDir = arcFlowDir;
            arcEty.LaneNum = laneNum;

            arcEty.Other = 0;


            IPolyline arcLine = LineHelper.CreateLineByLRS(linkLine, Lane.LANE_WEIDTH * arcEty.FlowDir * arcEty.LaneNum / 2,
                INITFCUR, INITTCUT);

            //获取截头截尾的距离
            ArcService arc = new ArcService(_pFeaClsArc, 0);

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
        /// 打断Link
        /// </summary>
        /// <param name="fatherLinkFea"></param>
        public void SplitLink(IFeature fatherLinkFea)
        {

            IPolyline fatherLine=fatherLinkFea.ShapeCopy as IPolyline;
            IPolyline spltLine = LineHelper.CreateLineByLRS(fatherLine, 0, 0, fatherLine.Length / 2);
            IPoint spltPnt = spltLine.ToPoint;

            NodeService spltNode = new NodeService(_pFeaClsNode, 0, spltPnt);
            Node spltNodeEty = new Node();
            spltNodeEty.ID = 0;
            spltNodeEty.AdjIDs = "";
            spltNodeEty.CompositeType = 1;
            spltNodeEty.ConnState = "";
            spltNodeEty.NodeType = 1;
            spltNodeEty.NorthAngles = "";
            spltNodeEty.Other = 0;
            IFeature spltNodeFea = spltNode.CreateNode(spltNodeEty);

            LinkService spltLink = new LinkService(_pFeaClsLink, 0);
            LinkMaster linkMstEty = new LinkMaster();
            linkMstEty = spltLink.GetEntity(fatherLinkFea);

            Link spltLinkEty = new Link();
            spltLinkEty = spltLinkEty.Copy(linkMstEty);




            int fNodeId = spltLinkEty.FNodeID;
            int tNodeId = spltLinkEty.TNodeID;

            spltNode = new NodeService(_pFeaClsNode, fNodeId, null);
            IFeature fNodeFea = spltNode.GetFeature();


            spltNode = new NodeService(_pFeaClsNode, tNodeId, null);
            IFeature tNodeFea = spltNode.GetFeature();

            Arc sameArcEty = new Arc();
            Arc oppArcEty = new Arc();

            ArcService arc = new ArcService(_pFeaClsArc, 0);
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

            //System.GC.Collect();
            //System.GC.WaitForPendingFinalizers();
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
            LinkService link = new LinkService(_pFeaClsLink, 0);
            int linkID = Convert.ToInt32(linkFea.get_Value(_pFeaClsLink.FindField(link.IDNm)));
            linkFea.Delete();

            ArcService arc = new ArcService(_pFeaClsArc, 0);
            IFeatureCursor cursor;
            IQueryFilter fitler = new QueryFilterClass();
            fitler.WhereClause = link.IDNm + " = " + linkID;
            cursor = _pFeaClsArc.Search(fitler, false);
            IFeature pFeaArc = cursor.NextFeature();
            while (pFeaArc != null)
            {
                Arc arcEty = arc.GetArcEty(pFeaArc);
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
                filterSurface.WhereClause = Arc.ArcIDNm + " = " + arcEty.ArcID;
                cursorSurface = _pFeaClsSurface.Search(filterSurface, false);
                IFeature surfaceFea = cursorSurface.NextFeature();
                while (surfaceFea != null)
                {
                    surfaceFea.Delete();
                    surfaceFea = cursorSurface.NextFeature();
                }

                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                //System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);

                //删除车道层次的东东
                LaneFeatureService lane = new LaneFeatureService(_pFeaClsLane, 0);
                IFeatureCursor cursorLane;
                IQueryFilter filterLane = new QueryFilterClass();
                filterLane.WhereClause = LaneFeatureService.ArcIDNm + " = " + arcEty.ArcID;
                cursorLane = _pFeaClsLane.Search(filterLane, false);
                IFeature laneFea = cursorLane.NextFeature();

                //删除道路中心线
                int centerBoundaryID = 0; 
                if (laneFea != null&& centerBoundaryID== 0)
                {
                    Lane laneEty = new Lane();
                    laneEty = lane.GetEntity(laneFea);
                    if (laneEty.Position == 0)
                    {
                        centerBoundaryID = laneEty.LeftBoundaryID;
                        BoundaryService boun = new BoundaryService(_pFeaClsBoundary, centerBoundaryID);
                        IFeature bounFea = boun.GetFeature();
                        if (bounFea != null)
                        {
                            bounFea.Delete();
                        }
                    }
                }
                while (laneFea != null)
                {
                    Lane laneEty = new Lane();
                    laneEty = lane.GetEntity(laneFea);
                    laneFea.Delete();

                    int rigntBounID = laneEty.RightBoundaryID;
                    BoundaryService boun = new BoundaryService(_pFeaClsBoundary, rigntBounID);
                    IFeature bounFea = boun.GetFeature();

                    if (bounFea != null)
                    {
                        bounFea.Delete();    
                    }
                    


                    laneFea = cursorLane.NextFeature();
                }
                pFeaArc = cursor.NextFeature();
            }

            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            //System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
        }





    }
}
