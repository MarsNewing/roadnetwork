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
    class LinkConstructor
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


        public LinkConstructor(Dictionary<string, IFeatureClass> feaClsDic)
        {
            _feaClsDic = feaClsDic;

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
            
        }


        public void CreateLinkLayerTopology(IPolyline linkLine,
            ref IFeature fNodeFea,
            ref IFeature tNodeFea,
            int linkRoadType,
            string linkRoadNm,
            int linkFlowDir,
            int sameLaneNum,
            int oppLaneNum,
            ref Arc sameArc,
            ref Arc oppArc,
            ref IFeature createLinkFea)
        {
            if (fNodeFea == null || tNodeFea == null)
            {
                return;
            }

            // ------------------------1--创建Link要素----------------------
            //保存Link
            createLinkFea = saveLink(fNodeFea,
                tNodeFea,
                linkRoadType,
                linkRoadNm,
                linkFlowDir,
                linkLine);

            LinkService linkService = new LinkService(_pFeaClsLink, 0);
            LinkMaster linkMaster = linkService.GetEntity(createLinkFea);
            Link link = new Link();
            link = link.Copy(linkMaster);

            //--------------------------2--更新Link起始Node属性2--------------------
            // 更新FTNode的属性2，并更新_fNodeEty和_tNodeEty
            updateFTNode(ref fNodeFea, ref tNodeFea);

            #region ----------------------------3--创建Link两个方向的Arc要素--------------------------
            if (linkFlowDir == Link.FLOWDIR_DOUBLE || linkFlowDir == Link.FLOWDIR_SAME)
            {
                ArcService arcService = new ArcService(_pFeaClsArc, 0);
                sameArc = arcService.CreateArcFromLink(link.ID, 1, sameLaneNum, linkLine);
            }

            if (linkFlowDir == Link.FLOWDIR_DOUBLE || linkFlowDir == Link.FLOWDIR_OPPOSITION)
            {
                ArcService arcService = new ArcService(_pFeaClsArc, 0);
                oppArc = arcService.CreateArcFromLink(link.ID, -1, oppLaneNum, linkLine);
            }
            #endregion
        }


        public void CreateLaneTopologyForLink(Arc sameArc,
            Arc oppArc,
            IFeature createLinkFea,
            IFeature fNodeFea,
            IFeature tNodeFea)
        {
            LaneLayerBuilder laneLayerFactory = new LaneLayerBuilder(_feaClsDic);
            if (sameArc != null)
            {
                laneLayerFactory.CreateArcTopology(createLinkFea, fNodeFea, tNodeFea, sameArc);
            }

            if (oppArc != null)
            {
                laneLayerFactory.CreateArcTopology(createLinkFea, fNodeFea, tNodeFea, oppArc);
            }

            laneLayerFactory.UpdateCenterLine(createLinkFea, sameArc, oppArc);

        }

        /// <summary>
        /// 创建Link拓扑
        /// 1--创建Link要素
        /// 2--更新Link起始Node属性2
        /// 3--创建Link两个方向的Arc要素
        /// 4--创建Link两个方向Arc的拓扑
        /// </summary>
        /// <param name="linkLine"></param>
        /// <param name="fNodeFea"></param>
        /// <param name="tNodeFea"></param>
        /// <param name="linkRoadType"></param>
        /// <param name="linkRoadNm"></param>
        /// <param name="linkFlowDir"></param>
        /// <param name="sameLaneNum"></param>
        /// <param name="oppLaneNum"></param>
        public void CreateLinkTopo(IPolyline linkLine,
            IFeature fNodeFea,
            IFeature tNodeFea,
            int linkRoadType, 
            string linkRoadNm,
            int linkFlowDir, 
            int sameLaneNum, 
            int oppLaneNum)
        {
            if (fNodeFea == null || tNodeFea == null)
            {
                return;
            }

            // ------------------------1--创建Link要素----------------------
            //保存Link
            IFeature createLinkFea = saveLink(fNodeFea,
                tNodeFea, 
                linkRoadType, 
                linkRoadNm,
                linkFlowDir, 
                linkLine);

            LinkService linkService = new LinkService(_pFeaClsLink, 0);
            LinkMaster linkMaster = linkService.GetEntity(createLinkFea);
            Link link = new Link();
            link = link.Copy(linkMaster);

            //--------------------------2--更新Link起始Node属性2--------------------
            // 更新FTNode的属性2，并更新_fNodeEty和_tNodeEty
            updateFTNode(ref fNodeFea, ref tNodeFea);

            #region ----------------------------3--创建Link两个方向的Arc要素--------------------------
            Arc sameArc = null;
            if (linkFlowDir == Link.FLOWDIR_DOUBLE || linkFlowDir == Link.FLOWDIR_SAME)
            {
                ArcService arcService = new ArcService(_pFeaClsArc, 0);
                sameArc = arcService.CreateArcFromLink(link.ID, 1, sameLaneNum, linkLine);
            }

           
            Arc oppArc = null;
            if (linkFlowDir == Link.FLOWDIR_DOUBLE || linkFlowDir == Link.FLOWDIR_OPPOSITION)
            {
                ArcService arcService = new ArcService(_pFeaClsArc, 0);
                oppArc = arcService.CreateArcFromLink(link.ID, -1, oppLaneNum, linkLine);
            }
            #endregion


            #region ---------------------------- 4--创建Link两个方向Arc的拓扑 --------------------------
            try
            {
                LaneLayerBuilder laneLayerFactory = new LaneLayerBuilder(_feaClsDic);
                if (sameArc != null)
                {
                    laneLayerFactory.CreateArcTopology(createLinkFea, fNodeFea, tNodeFea, sameArc);
                }

                if (oppArc != null)
                {
                    laneLayerFactory.CreateArcTopology(createLinkFea, fNodeFea, tNodeFea, oppArc);
                }

                laneLayerFactory.UpdateCenterLine(createLinkFea,sameArc,oppArc);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            #endregion
        }


        /// <summary>
        /// 打断Link
        /// </summary>
        /// <param name="fatherLinkFea"></param>
        public void SplitLink(IFeature fatherLinkFea,
            double splitLength)
        {
            //保留打断前Link的几何和属性值
            IPolyline fatherLine = fatherLinkFea.ShapeCopy as IPolyline;
            LinkService fatherLinkService = new LinkService(_pFeaClsLink, 0);
            LinkMaster fatherLinkMst = new LinkMaster();
            fatherLinkMst = fatherLinkService.GetEntity(fatherLinkFea);
            Link fatherLink = new Link();
            fatherLink = fatherLink.Copy(fatherLinkMst);

            //获得Link起始和终止结点

            IFeature fNodeFea = null;
            IFeature tNodeFea = null;
            NodeService nodeService = new NodeService(_pFeaClsNode, fatherLink.FNodeID,null);
            fNodeFea = nodeService.GetFeature();

            nodeService = new NodeService(_pFeaClsNode, fatherLink.TNodeID, null);
            tNodeFea = nodeService.GetFeature();


            //保留原始Arc属性
            ArcService arcService = new ArcService(_pFeaClsArc, 0);
            Arc sameArcEty = arcService.GetSameArc(fatherLink.ID);

            Arc oppArcEty = arcService.GetOppositionArc(fatherLink.ID);

            //里删除Link
            deleteLinkTopology(fatherLinkFea);

            

            //创建打断点的Node要素
            IPolyline spltLine = LineHelper.CreateLineByLRS(fatherLine, 0, 0, splitLength);
            IFeature spltNodeFea = createSplitNode(spltLine.ToPoint);


            //创建第一个Link，位于Fnode与spltNode之间
            IPointCollection pntClt = new PolylineClass();
            pntClt.AddPoint(fNodeFea.ShapeCopy as IPoint);
            pntClt.AddPoint(spltNodeFea.ShapeCopy as IPoint);
            IPolyline newLine1 = pntClt as IPolyline;


            //先生成Link层的所有元素，在分别构建各自Arc内部的拓扑
            //生成第一段Link的Link层数据
            Arc arc1Same = null;
            Arc arc1opp = null;
            IFeature link1Fea = null;
            CreateLinkLayerTopology(newLine1,
                ref fNodeFea,
                ref spltNodeFea,
                fatherLink.RoadType,
                fatherLink.RoadName,
                fatherLink.FlowDir,
                sameArcEty.LaneNum,
                oppArcEty.LaneNum,
                ref arc1Same,
                ref arc1opp,
                ref link1Fea);


            //生成第二段Link的Link层数据
            //创建第二个Link，位于spltNode与Tnode之间
            pntClt = new PolylineClass();
            pntClt.AddPoint(spltNodeFea.ShapeCopy as IPoint);
            pntClt.AddPoint(tNodeFea.ShapeCopy as IPoint);
            IPolyline newLine2 = pntClt as IPolyline;

            Arc arc2Same = null;
            Arc arc2Opp = null;
            IFeature link2Fea = null;
            CreateLinkLayerTopology(newLine2, 
                ref spltNodeFea,
                ref tNodeFea, 
                fatherLink.RoadType, 
                fatherLink.RoadName,
                fatherLink.FlowDir, 
                sameArcEty.LaneNum,
                oppArcEty.LaneNum,
                ref arc2Same,
                ref arc2Opp,
                ref link2Fea);

            //生成第一段Lane层拓扑
            CreateLaneTopologyForLink(arc1Same, arc1opp,
                link1Fea, fNodeFea, spltNodeFea);
            //生成第二段Lane层拓扑
            CreateLaneTopologyForLink(arc2Same, arc2Opp,
                link2Fea, spltNodeFea, tNodeFea);

            //CreateLaneTopologyForLink()
        }

        /// <summary>
        /// 更新Node所有属性
        /// </summary>
        /// <param name="fNodeFea"></param>
        /// <param name="tNodeFea"></param>
        private void updateFTNode(ref IFeature fNodeFea,
            ref IFeature tNodeFea)
        {
            //保存FNode属性2
            NodeService nodeService = new NodeService(_pFeaClsNode, 0, null);
            Node fNode = new Node();
            NodeMaster fNodeMasterEty = nodeService.GetNodeMasterEty(fNodeFea);
            fNode = fNode.Copy(fNodeMasterEty);

            //保存FNode属性2
            updateNodeAtt2(fNode.ID, fNodeFea.Shape as IPoint);
            //更新fNodeFea
            nodeService = new NodeService(_pFeaClsNode, fNode.ID, fNodeFea.Shape as IPoint);
            fNodeFea = nodeService.GetFeature();
            string[] adjLinks = nodeService.GetNodeMasterEty(fNodeFea).AdjIDs.Split('\\');
            //相邻路段数大于2，更新NodeType字段
            if (adjLinks.Length > 2)
            {
                fNodeFea.set_Value(_pFeaClsNode.FindField(nodeService.NodeTypeNm), 1);
                fNodeFea.Store();
            }
            
            //保存TNode属性2
            Node tNode = new Node();
            NodeMaster tNodeMasterEty = nodeService.GetNodeMasterEty(tNodeFea);
            tNode = tNode.Copy(tNodeMasterEty);
            //保存FNode属性2
            updateNodeAtt2(tNode.ID, tNodeFea.Shape as IPoint);
            //更新tNodeFea
            nodeService = new NodeService(_pFeaClsNode, tNode.ID, tNodeFea.Shape as IPoint);
            tNodeFea = nodeService.GetFeature();
            if (nodeService.GetNodeMasterEty(tNodeFea).AdjIDs.Split('\\').Length > 2)
            {
                tNodeFea.set_Value(_pFeaClsNode.FindField(nodeService.NodeTypeNm), 1);
                tNodeFea.Store(); 
            }
        }

        /// <summary>
        /// 更新Node的Adj数据
        /// </summary>
        /// <param name="nodeID"></param>
        /// <param name="pnt"></param>
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

            LinkService linkService = new LinkService(_pFeaClsLink, 0);
            NodeService node = new NodeService(_pFeaClsNode, 0, null);

            Link link = new Link();
            link.ID = 0;
            link.RoadType = linkRoadType;
            link.RoadName = linkRoadNm;
            link.FNodeID = Convert.ToInt32(fNodeFea.get_Value(_pFeaClsNode.FindField(node.NodeIDNm)));
            link.TNodeID = Convert.ToInt32(tNodeFea.get_Value(_pFeaClsNode.FindField(node.NodeIDNm)));

            link.FlowDir = linkFlowDir;
            link.RoadLevel = link.RoadType;

            return linkService.Create(link, linkLine);
        }


        /// <summary>
        /// 创建打断点要素
        /// </summary>
        /// <param name="spltPnt"></param>
        /// <returns></returns>
        private IFeature createSplitNode(IPoint spltPnt)
        {
            NodeService spltNode = new NodeService(_pFeaClsNode, 0, spltPnt);
            Node spltNodeEty = new Node();
            spltNodeEty.ID = 0;
            spltNodeEty.AdjIDs = "";
            spltNodeEty.CompositeType = 1;
            spltNodeEty.ConnState = "";
            spltNodeEty.NodeType = 1;
            spltNodeEty.NorthAngles = "";
            spltNodeEty.Other = 0;
            return spltNode.CreateNode(spltNodeEty);
        }

        /// <summary>
        /// 删除Link拓扑
        /// </summary>
        /// <param name="linkFea"></param>
        private void deleteLinkTopology(IFeature linkFea)
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
                filterLane.WhereClause = Lane.ArcIDNm + " = " + arcEty.ArcID;
                cursorLane = _pFeaClsLane.Search(filterLane, false);
                IFeature laneFea = cursorLane.NextFeature();

                //删除道路中心线
                int centerBoundaryID = 0; 
                if (laneFea != null&& centerBoundaryID== 0)
                {
                    Lane laneEty = new Lane();
                    laneEty = lane.GetEntity(laneFea);
                    if (laneEty.Position == Lane.LEFT_POSITION)
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
