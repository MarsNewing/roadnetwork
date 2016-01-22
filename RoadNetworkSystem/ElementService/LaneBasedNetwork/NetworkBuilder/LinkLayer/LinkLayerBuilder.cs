using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.Road;
using RoadNetworkSystem.GIS;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LaneLayer;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LinkLayer;
using RoadNetworkSystem.NetworkExtraction.FreeWay.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.NetworkExtraction.LaneBasedNetwork.LinkLayer
{
    class LinkLayerBuilder
    {
        IFeatureClass _pFeaClsLink;
        IFeatureClass _pFeaClsArc;
        IFeatureClass _pFeaClsNode;

        public LinkLayerBuilder(IFeatureClass pFeaClsLink,IFeatureClass pFeaClsNode,IFeatureClass pFeaClsArc)
        {
            _pFeaClsLink = pFeaClsLink;
            _pFeaClsNode = pFeaClsNode;
            _pFeaClsArc = pFeaClsArc;
        }

        /// <summary>
        /// 线创建Link和Arc
        /// </summary>
        /// <param name="link"></param>
        /// <param name="linkLine"></param>
        /// <param name="sameDirLaneNum"></param>
        /// <param name="oppoDirLaneNum"></param>
        public void createLinkAndArcs(Link link, IPolyline linkLine, int sameDirLaneNum, int oppoDirLaneNum)
        {
            LinkService linkService = new LinkService(_pFeaClsLink, link.ID);
            IFeature linkFeature = linkService.Create(link, linkLine);
            LinkMaster linkMaster = linkService.GetEntity(linkFeature);
            link = link.Copy(linkMaster);
            if (sameDirLaneNum > 0)
            {
                Arc sameArc = new Arc();
                sameArc.LinkID = link.ID;
                sameArc.LaneNum = sameDirLaneNum;
                sameArc.FlowDir = Link.FLOWDIR_SAME;

                ArcService arcService = new ArcService(_pFeaClsArc, 0);
                IPolyline sameArcLine = LineHelper.CreateLineByLRS(linkLine, sameDirLaneNum * Lane.LANE_WEIDTH / 2,
                    linkLine.Length * ArcService.ARC_CUT_PERCENTAGE, linkLine.Length * ArcService.ARC_CUT_PERCENTAGE);
                IFeature sameArcFeature = arcService.CreateArc(sameArc, sameArcLine);

            }


            if (oppoDirLaneNum > 0)
            {
                Arc oppositionArc = new Arc();
                oppositionArc.LinkID = link.ID;
                oppositionArc.LaneNum = oppoDirLaneNum;
                oppositionArc.FlowDir = Link.FLOWDIR_OPPOSITION;

                ArcService arcService = new ArcService(_pFeaClsArc, 0);
                IPolyline oppositionArcLine = LineHelper.CreateLineByLRS(linkLine, -oppoDirLaneNum * Lane.LANE_WEIDTH / 2,
                    linkLine.Length * ArcService.ARC_CUT_PERCENTAGE, linkLine.Length * ArcService.ARC_CUT_PERCENTAGE);

                //转换数字化方向
                oppositionArcLine.ReverseOrientation();
                IFeature oppositionArcFeature = arcService.CreateArc(oppositionArc, oppositionArcLine);
            }
        }

        /// <summary>
        /// 给生成Link 和 Arc的Link层批量生成Node
        /// </summary>
        public void createNodesForLinkAndArc()
        {
            /*
             * 
             * 获取Link端点的坐标点，以及与该点连接的所有的Link
             * 创建Node，获取NodeID
             * 为Link的FNodeID和TNodeID赋值
             * 为Node的创建adj数据
             * 
             */
            Dictionary<string, List<int>> nodeXY_LinkPair = getNodeXYLinkPairs();
            foreach (string item in nodeXY_LinkPair.Keys)
            {
                //创建Node，获取NodeID
                IPoint nodePoint = getPointFromStr(item);
                NodeService nodeService = new NodeService(_pFeaClsNode,0,nodePoint);
                Node node = new Node();
                node.CompositeType = Node.NODE_TYPE_PLANE_INTERSECTION;
                IFeature nodeFeature = nodeService.CreateNode(node);
                NodeMaster nodeMstr = nodeService.GetNodeMasterEty(nodeFeature);
                node = node.Copy(nodeMstr);

                //为Link的FNodeID和TNodeID赋值
                List<int> linkIds = nodeXY_LinkPair[item];
                foreach (int temLinkId in linkIds)
                {
                    LinkService linkService = new LinkService(_pFeaClsLink, temLinkId);
                    IFeature temLinkFea = linkService.GetFeature();
                    LinkMaster linkMstr = linkService.GetEntity(temLinkFea);
                    Link temLink = new Link();
                    temLink = temLink.Copy(linkMstr);
                    IPolyline temLinkLine = temLinkFea.Shape as IPolyline;
                    if (item.Equals(getPointXY(temLinkLine.FromPoint)))
                    {
                        temLink.FNodeID = node.ID;
                        temLinkFea.set_Value(temLinkFea.Fields.FindField(linkService.FNodeIDNm), temLink.FNodeID);
                        temLinkFea.Store();
                    }
                    else
                    {
                        temLink.TNodeID = node.ID;
                        temLinkFea.set_Value(temLinkFea.Fields.FindField(linkService.TNodeIDNm), temLink.TNodeID);
                        temLinkFea.Store();
                    }
                }
                //为Node的创建adj数据
                LinkService linkService1 = new LinkService(_pFeaClsLink, 0);
                nodeService = new NodeService(_pFeaClsNode, node.ID, nodePoint);
                nodeService.CreateAdjData(linkService1);
            }
        }

        private IPoint getPointFromStr(string str)
        {
            if (!str.Contains("_"))
            {
                return null;
            }
            else
            {
                string[] arr = str.Split('_');
                IPoint point = new PointClass();
                point.PutCoords(Convert.ToDouble(arr[0]), Convert.ToDouble(arr[1]));
                return point;
            }
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, List<int>> getNodeXYLinkPairs()
        {
            Dictionary<string, List<int>> nodeXY_LinkPair = new Dictionary<string, List<int>>();

            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = "";
            IFeatureCursor cursor = _pFeaClsLink.Search(filter, false);
            IFeature pFeatureLink = cursor.NextFeature();
            while (pFeatureLink != null)
            {
                IPolyline linkLine = pFeatureLink.Shape as IPolyline;
                string fromPointXY = getPointXY(linkLine.FromPoint);
                string toPointXY = getPointXY(linkLine.ToPoint);
                LinkService linkService = new LinkService(_pFeaClsLink,0);
                int linkId = Convert.ToInt32(pFeatureLink.get_Value(pFeatureLink.Fields.FindField(linkService.IDNm)));
                if (nodeXY_LinkPair.ContainsKey(fromPointXY))
                {
                    nodeXY_LinkPair[fromPointXY].Add(linkId);
                }
                else
                {
                    List<int> linkIds = new List<int>();
                    linkIds.Add(linkId);
                    nodeXY_LinkPair.Add(fromPointXY, linkIds);
                }

                if (nodeXY_LinkPair.ContainsKey(toPointXY))
                {
                    nodeXY_LinkPair[toPointXY].Add(linkId);
                }
                else
                {
                    List<int> linkIds = new List<int>();
                    linkIds.Add(linkId);
                    nodeXY_LinkPair.Add(toPointXY, linkIds);
                }
                
                pFeatureLink = cursor.NextFeature();
            }
            System.GC.Collect();                                         //强制对所有代进行垃圾回收。
            System.GC.WaitForPendingFinalizers();

            return nodeXY_LinkPair;
        }

        private string getPointXY(IPoint point)
        {
            return point.X.ToString()+ "_" +point.Y.ToString();
        }

    }
}
