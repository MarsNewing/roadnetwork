using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.Road;
using RoadNetworkSystem.DataModel.RoadSign;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.Connection;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LaneLayer;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LinkLayer;
using RoadNetworkSystem.NetworkElement.RoadSignElement;
using RoadNetworkSystem.NetworkExtraction.LaneBasedNetwork.LaneLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadNetworkSystem.ElementService.LaneBasedNetwork.NetworkBuilder.LaneLayer
{
    class LaneLayerBatchBuilder
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

        Dictionary<string, IFeatureClass> _feaClsDic;
        public LaneLayerBatchBuilder(Dictionary<string, IFeatureClass> feaClsDic)
        {
            _feaClsDic = feaClsDic;
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

        public void CreateLaneOpologyBatch()
        {
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = "";

            IFeatureCursor cursor = _pFeaClsArc.Search(filter,false);
            IFeature arcFeaCursor = cursor.NextFeature();
            while (arcFeaCursor != null)
            {
                ArcService arcService = new ArcService(_pFeaClsArc, 0);
                Arc arcCursor = arcService.GetArcEty(arcFeaCursor);

                LinkService linkService = new LinkService(_pFeaClsLink, arcCursor.LinkID);
                IFeature linkFea = linkService.GetFeature();
                if (linkFea == null)
                {
                    arcFeaCursor.Delete();
                    arcFeaCursor = cursor.NextFeature();
                    continue;

                }
                LinkMaster linkMstr = linkService.GetEntity(linkFea);
                Link link = new Link();
                link = link.Copy(linkMstr);

                NodeService nodeService = new NodeService(_pFeaClsNode, link.FNodeID, null);
                
                IFeature fNodeFeature = nodeService.GetFeature();
                NodeMaster fNodeMstr = nodeService.GetNodeMasterEty(fNodeFeature);
                Node fNode = new Node();
                fNode = fNode.Copy(fNodeMstr);

                nodeService = new NodeService(_pFeaClsNode, link.TNodeID, null);
                IFeature tNodeFeature = nodeService.GetFeature();
                NodeMaster tNodeMstr = nodeService.GetNodeMasterEty(tNodeFeature);
                Node tNode = new Node();
                tNode = tNode.Copy(tNodeMstr);

                if (arcCursor.ArcID == 51)
                {
                    int test = 0;
                }

                PreNodeCutInforService preNodeCutInforService = new PreNodeCutInforService(_pFeaClsLink, _pFeaClsArc);
                NextNodeCutInforService nextNodeCutInforService = new NextNodeCutInforService(_pFeaClsLink, _pFeaClsArc);
                PreNodeCutInfor preNodeCutInfor;
                NextNodeCutInfor nextNodeCutInfor;
                if (arcCursor.FlowDir == Link.FLOWDIR_SAME)
                {
                    preNodeCutInfor = preNodeCutInforService.GetPreNodeCutInfor(fNode, arcCursor, link);
                    nextNodeCutInfor = nextNodeCutInforService.GetNextNodeCutInfor(tNode, arcCursor, link);
                }
                else
                {
                    preNodeCutInfor = preNodeCutInforService.GetPreNodeCutInfor(tNode, arcCursor, link);
                    nextNodeCutInfor = nextNodeCutInforService.GetNextNodeCutInfor(fNode, arcCursor, link);
                }



                //生成Lane、Kerb、Boundary、StopLine
                LaneLayerBuilder laneLayerBuilder = new LaneLayerBuilder(_feaClsDic);
                laneLayerBuilder.CreateLaneTopo(linkFea, link.FlowDir, arcCursor, preNodeCutInfor, nextNodeCutInfor);
                
                arcFeaCursor = cursor.NextFeature();

            }

            CreateLaneConnectorBatch();
        }


        private void CreateLaneConnectorBatch()
        {
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = "";

            IFeatureCursor cursor = _pFeaClsArc.Search(filter,false);
            IFeature arcFeaCursor = cursor.NextFeature();
            while (arcFeaCursor != null)
            {
                ArcService arcService = new ArcService(_pFeaClsArc, 0);
                Arc arcCursor = arcService.GetArcEty(arcFeaCursor);

                if (arcCursor.ArcID == 61|| arcCursor.ArcID == 51)
                {
                    int test = 0;
                }

                LinkService linkService = new LinkService(_pFeaClsLink, arcCursor.LinkID);
                IFeature linkFea = linkService.GetFeature();
                LinkMaster linkMstr = linkService.GetEntity(linkFea);
                Link link = new Link();
                link = link.Copy(linkMstr);

                NodeService nodeService = new NodeService(_pFeaClsNode, link.FNodeID, null);
                
                IFeature fNodeFeature = nodeService.GetFeature();
                NodeMaster fNodeMstr = nodeService.GetNodeMasterEty(fNodeFeature);
                Node fNode = new Node();
                fNode = fNode.Copy(fNodeMstr);

                nodeService = new NodeService(_pFeaClsNode, link.TNodeID, null);
                IFeature tNodeFeature = nodeService.GetFeature();
                NodeMaster tNodeMstr = nodeService.GetNodeMasterEty(tNodeFeature);
                Node tNode = new Node();
                tNode = tNode.Copy(tNodeMstr);
                Node preNode = null;
                Node nextNode = null;
                if (arcCursor.FlowDir == Link.FLOWDIR_SAME)
                {
                    preNode = fNode;
                    nextNode = tNode;
                }
                else
                {
                    preNode = tNode;
                    nextNode = fNode;
                }
                
                PreNodeCutInforService preNodeCutInforService = new PreNodeCutInforService(_pFeaClsLink, _pFeaClsArc);
                PreNodeCutInfor preNodeCutInfor = preNodeCutInforService.GetPreNodeCutInfor(preNode, arcCursor, link);

                NextNodeCutInforService nextNodeCutInforService = new NextNodeCutInforService(_pFeaClsLink, _pFeaClsArc);
                NextNodeCutInfor nextNodeCutInfor = nextNodeCutInforService.GetNextNodeCutInfor(nextNode, arcCursor, link);

                IFeature nextNodeFea;
                IFeature preNodeFea;
                if(fNode.ID ==nextNodeCutInfor.nextNodeEty.ID)
                {
                    nextNodeFea = fNodeFeature;
                    preNodeFea = tNodeFeature;
                }
                else
                {
                    nextNodeFea = tNodeFeature;
                    preNodeFea = fNodeFeature;
                }

                //生成车道连接器
                Arc[] exitArcs = LogicalConnection.GetNodeExitArcs(_pFeaClsLink, _pFeaClsArc, nextNodeCutInfor.nextNodeEty);
                foreach (Arc temArc in exitArcs)
                {
                    if (temArc == null)
                    {
                        continue;
                    }

                    double angle = PhysicalConnection.GetLinksAngle(arcCursor.LinkID,temArc.LinkID,nextNodeCutInfor.nextNodeEty);
                    string turningDir= PhysicalConnection.GetTurningDir(angle);

                    LaneConnectorFeatureService laneConnectorFeatureService = new LaneConnectorFeatureService(_pFeaClsConnector, 0);
                    laneConnectorFeatureService.CreateConnectorInArcs(_pFeaClsLane, arcCursor, temArc, turningDir,nextNodeFea.Shape as IPoint);
                }

                //更新入口段的导向箭头，暂且生成两排
                TurnArrowService arrow1 = new TurnArrowService(_pFeaClsTurnArrow, 0);
                arrow1.CreateEntranceArcArrow(arcCursor.ArcID);
                
                //生成Surface
                SurfaceService surface = new SurfaceService(_pFeaClsSurface, 0);
                IPolygon gon = new PolygonClass();
                string str = "";
                Surface surfaceEty = new Surface();
                surface.CrateSurfaceShape(_pFeaClsKerb, nextNodeCutInfor, nextNodeFea, preNodeFea, ref gon, ref str);

                surfaceEty.ArcID = arcCursor.ArcID;
                surfaceEty.ControlIDs = str;
                surfaceEty.SurfaceID = 0;
                surfaceEty.Other = 0;
                if (gon != null)
                {
                    surface.CreateSurface(surfaceEty, gon);
                }


                Arc sameArc;
                Arc oppArc;
                //生产道路中间分隔线
                if (arcCursor.FlowDir == Link.FLOWDIR_SAME)
                {
                    sameArc = arcCursor;
                    ArcService temArcService = new ArcService(_pFeaClsArc, 0);
                    oppArc = temArcService.GetOppositionArc(sameArc.LinkID);
                }
                else
                {
                    oppArc = arcCursor;
                    ArcService temArcService = new ArcService(_pFeaClsArc, 0);
                    sameArc = temArcService.GetOppositionArc(oppArc.LinkID);
                }

                LaneLayerBuilder laneLayerBuilder = new LaneLayerBuilder(_feaClsDic);
                laneLayerBuilder.UpdateCenterLine(linkFea,sameArc,oppArc);
                arcFeaCursor = cursor.NextFeature();
                
            }
        }
        
    }
}
