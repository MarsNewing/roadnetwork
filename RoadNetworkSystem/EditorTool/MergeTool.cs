using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.GIS;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LinkLayer;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.RoadSegmentLayer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace RoadNetworkSystem.EditorTool
{
    class MergeTool
    {
        private static Form1 _frm1;
        private static IFeatureClass pSegClass;
        private static IFeatureClass pNodeClass;
        private static IFeatureClass pRoadClass;
        /// <summary>
        ///合并Segment
        /// </summary>
        /// <param name="pFeaClsRoad"></param>Road要素类
        /// <param name="pFeaCLsSegment"></param>Segment要素类
        /// <param name="pFeaClsSegNode"></param>SegmentNode要素类
        /// <param name="segmentFea1"></param>打断的第一个路段
        /// <param name="segmentFea2"></param>打断的第二个路段
        /// <param name="segNodeFea"></param>和并处的SegmentNode要素
        public MergeTool(Form1 frm1)
        {
            _frm1 = frm1;
            pSegClass = _frm1.FeaClsSegment;
            pNodeClass = _frm1.FeaClsSegNode;
            pRoadClass = _frm1.FeaClsRoad;

        }
        public void MergeRoad()
        {
            IFeature SegFea1 = _frm1.FirstRoadFea;
            IFeature SegFea2 = _frm1.SecondRoadFea;
            IPolyline Line1 = SegFea1.Shape as IPolyline;
            IPolyline Line2 = SegFea2.Shape as IPolyline;

            //1.求两条道路的交点
            IPoint pnt = new PointClass();
            pnt = LineHelper.GetIntersectionPoint(Line1, Line2);
            if (pnt == null)
            {
                MessageBox.Show("所选两条Road不相交，请重新选择");

            }
            else
            {
                IPolyline Line = new PolylineClass();
                Line = LineHelper.MergeLine(Line1, Line2);
                //暂时用Line1的各种属性更新合并之后的Line
                //但长度还未考虑
                IntersectionTool pIntersectionTool = new IntersectionTool(_frm1);
                pIntersectionTool.InsertIntoRoad(Line, SegFea1);
                _frm1.FirstRoadFea.Delete();
                _frm1.SecondRoadFea.Delete();
            }

        }
        public void MergeSegment()
        {
            IFeature SegFea1 = _frm1.FirstSegFea;
            IFeature SegFea2 = _frm1.SecondSegFea;
            IPolyline Line1 = SegFea1.Shape as IPolyline;
            IPolyline Line2 = SegFea2.Shape as IPolyline;

            //1.求两条道路的交点
            IPoint pnt = new PointClass();
            pnt = LineHelper.GetIntersectionPoint(Line1, Line2);
            if (pnt == null)
            {
                MessageBox.Show("所选两条Segment不相交，请重新选择");

            }
            else
            {
                IPolyline Line = new PolylineClass();
                Line = LineHelper.MergeLine(Line1, Line2);
                InsertIntoSeg(Line, SegFea1);
                _frm1.FirstSegFea.Delete();
                _frm1.SecondSegFea.Delete();

                //删掉交点。
                int NodeID = SearchNodeID(pnt);
                if (NodeID > 0)
                {
                    DeletNode(NodeID);
                }

                //更新合并后的Line首尾节点的拓扑关系
                IPointCollection pPtCol = Line as IPointCollection;
                int FNodeID = SearchNodeID(pPtCol.get_Point(0));
                int TNodeID = SearchNodeID(pPtCol.get_Point(pPtCol.PointCount - 1));

                UpdateNodeAttr(FNodeID, pPtCol.get_Point(0));
                UpdateNodeAttr(TNodeID, pPtCol.get_Point(pPtCol.PointCount - 1));
            }


        }
        //更新被打断道路首尾节点的拓扑关系
        private void UpdateNodeAttr(int nodeID, IPoint pnt)
        {
            Node pNode = new Node(pNodeClass, nodeID, pnt);
            Segment seg = new Segment(pSegClass, 0);
            pNode.CreateAdjData(seg);

        }
        private void InsertIntoSeg(IPolyline pLine, IFeature pFeat)
        {
            System.GC.Collect();      //强制对所有代进行垃圾回收
            System.GC.WaitForPendingFinalizers();  //挂起当前线程，直到处理终结器队列的线程清空该队列为止

            IFeature Fea1 = pSegClass.CreateFeature();
            Fea1.Shape = pLine;

            Segment seg = new Segment(pSegClass, 0);
            int nRoadSegmentID = pSegClass.Fields.FindField(seg.IDNm);
            int nFNode = pSegClass.Fields.FindField(seg.FNodeIDNm);
            int nTNode = pSegClass.Fields.FindField(seg.TNodeIDNm);
            int nRoadID = pSegClass.Fields.FindField(seg.RelIDNm);
            int nRoadName = pSegClass.Fields.FindField(seg.RoadNameNm);
            int nRoadType = pSegClass.Fields.FindField(seg.RoadTypeNm);
            int nObjectID = Fea1.Fields.FindField("ObjectID");
            int nLengh = Fea1.Fields.FindField("SHAPE_Length");

            Fea1.set_Value(nRoadSegmentID, int.Parse(Fea1.get_Value(nObjectID).ToString()));

            IPointCollection pPtCol = pLine as IPointCollection;
            int FNodeID = SearchNodeID(pPtCol.get_Point(0));
            int TNodeID = SearchNodeID(pPtCol.get_Point(pPtCol.PointCount - 1));
            Fea1.set_Value(nFNode, FNodeID);
            Fea1.set_Value(nTNode, TNodeID);
            Fea1.set_Value(nLengh, pLine.Length);
            Fea1.set_Value(nRoadType, pFeat.get_Value(nRoadType));
            Fea1.set_Value(nRoadName, pFeat.get_Value(nRoadName));
            Fea1.set_Value(nRoadID, pFeat.get_Value(nRoadID));

            Fea1.Store();


        }
        private int SearchNodeID(IPoint pnt)
        {
            IFeatureCursor pFeatCursor = pNodeClass.Search(null, true);
            IFeature pFeature = pFeatCursor.NextFeature();
            int nNodeID = pFeature.Fields.FindField("NodeID");
            int NodeID = 0;
            while (pFeature != null)
            {
                IPoint pPoint = pFeature.Shape as IPoint;
                if (pnt.X.ToString("#0.0000") == pPoint.X.ToString("#0.0000") && (pnt.Y.ToString("#0.0000") == pPoint.Y.ToString("#0.0000")))
                {
                    NodeID = int.Parse(pFeature.get_Value(nNodeID).ToString());
                    break;

                }
                pFeature = pFeatCursor.NextFeature();
            }
            pFeatCursor = null;
            pFeature = null;

            return NodeID;

        }
        private void DeletNode(int NodeID)
        {
            IQueryFilter pQueryFilter = new QueryFilter();
            pQueryFilter.WhereClause = String.Format("{0}={1}", "NodeID", NodeID);
            IFeatureCursor pFeatCursor = pNodeClass.Search(pQueryFilter, false);
            IFeature pFeature = pFeatCursor.NextFeature();
            pFeature.Delete();
            pQueryFilter = null;
            pFeatCursor = null;

        }
       
    }
}
