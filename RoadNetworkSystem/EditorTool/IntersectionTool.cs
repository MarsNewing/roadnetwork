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
    class IntersectionTool
    {
        private static Form1 _frm1;
        private static IFeatureClass pSegClass;
        private static IFeatureClass pNodeClass;
        private static IFeatureClass pRoadClass;
        /// <summary>
        /// 打断Road，生成Segment和SegmentNode
        /// </summary>
        /// <param name="pFeaClsRoad"></param>Road要素类
        /// <param name="pFeaCLsSegment"></param>Segment要素类
        /// <param name="pFeaClsSegNode"></param>SegmentNode要素类
        /// <param name="roadFea1"></param>打断的第一个路段
        /// <param name="RoadFea2"></param>打断的第二个路段
        public IntersectionTool(Form1 frm1)
        {
            _frm1 = frm1;
            pSegClass = _frm1.FeaClsSegment;
            pNodeClass = _frm1.FeaClsSegNode;
            pRoadClass = _frm1.FeaClsRoad;

        }

        public void IntersectionRoad()
        {
            IFeature SegFea1 = _frm1.FirstRoadFea;
            IFeature SegFea2 = _frm1.SecondRoadFea;
            IPolyline Line1 = SegFea1.Shape as IPolyline;
            IPolyline Line2 = SegFea2.Shape as IPolyline;
            IPolyline Line11, Line12, Line21, Line22;



            //1.求两条道路的交点
            IPoint pnt = new PointClass();
            pnt = LineHelper.GetIntersectionPoint(Line1, Line2);
            if (pnt == null)
            {
                MessageBox.Show("所选两条道路不相交，请重新选择");

            }
            else
            {
                //2.将两条道路在交点处分割成两段。
                LineHelper.CutLineAtPoint(Line1, pnt, out Line11, out Line12);
                LineHelper.CutLineAtPoint(Line2, pnt, out Line21, out Line22);



                //3.打断后的路段加入到Road图层中
                InsertIntoRoad(Line11, SegFea1);
                InsertIntoRoad(Line12, SegFea1);
                InsertIntoRoad(Line21, SegFea2);
                InsertIntoRoad(Line22, SegFea2);

                //4.在road图层中将原道路删除
                _frm1.FirstRoadFea.Delete();
                _frm1.SecondRoadFea.Delete();
            }
        }

        public void InsertIntoRoad(IPolyline pLine, IFeature pFeat)
        {
            System.GC.Collect();      //强制对所有代进行垃圾回收
            System.GC.WaitForPendingFinalizers();  //挂起当前线程，直到处理终结器队列的线程清空该队列为止

            IFeature Fea1 = pRoadClass.CreateFeature();
            Fea1.Shape = pLine;

            int nRoadID = Fea1.Fields.FindField("RoadID");
            int nRoadName = Fea1.Fields.FindField("RoadName");
            int nRoadType = Fea1.Fields.FindField("RoadType");
            int nObjectID = Fea1.Fields.FindField("ObjectID");
            int nlength = Fea1.Fields.FindField("SHAPE_Length");
            Fea1.set_Value(nRoadID, int.Parse(Fea1.get_Value(nObjectID).ToString()));
            //Fea1.set_Value(nlength, pLine.Length);
            Fea1.set_Value(nRoadName, pFeat.get_Value(nRoadName));
            Fea1.set_Value(nRoadType, pFeat.get_Value(nRoadType));
            Fea1.Store();

        }

        private static void UpdateFeatureValue(IFeature pFeature1, IFeature pFeature2)
        {
            for (int i = 0; i < pFeature2.Fields.FieldCount; i++)
            {
                IField pField = pFeature2.Fields.get_Field(i);
                if (pField.Editable)
                {
                    int nIndex = pFeature1.Fields.FindField(pField.Name);
                    if (nIndex > -1)
                    {
                        pFeature2.set_Value(i, pFeature1.get_Value(nIndex));
                    }
                }
            }
        }

        /// <summary>
        /// 打断Segment，并更新SegmentNode
        /// </summary>
        public void IntersectionSeg()
        {

            IFeature SegFea1 = _frm1.FirstSegFea;
            IFeature SegFea2 = _frm1.SecondSegFea;
            IPolyline Line1 = SegFea1.Shape as IPolyline;
            IPolyline Line2 = SegFea2.Shape as IPolyline;
            IPolyline Line11, Line12, Line21, Line22;
            SegmentService seg = new SegmentService(pSegClass, 0);

            int NewNodeID;
            int nFNode = pSegClass.Fields.FindField(seg.FNodeIDNm);
            int nTNode = pSegClass.Fields.FindField(seg.TNodeIDNm);
            int[] NodeIDs = new int[5];
            NodeIDs[0] = int.Parse(SegFea1.get_Value(nFNode).ToString());
            NodeIDs[1] = int.Parse(SegFea1.get_Value(nTNode).ToString());
            NodeIDs[2] = int.Parse(SegFea2.get_Value(nFNode).ToString());
            NodeIDs[3] = int.Parse(SegFea2.get_Value(nTNode).ToString());
            IPoint[] PointArray = new IPoint[5];
            PointArray[0] = Line1.FromPoint;
            PointArray[1] = Line1.ToPoint;
            PointArray[2] = Line2.FromPoint;
            PointArray[3] = Line2.ToPoint;

            //1.求两条道路的交点
            IPoint pnt = new PointClass();
            pnt = LineHelper.GetIntersectionPoint(Line1, Line2);
            if (pnt == null)
            {
                MessageBox.Show("所选两条道路不相交，请重新选择");

            }
            else
            {
                //2.将两条道路在交点处分割成两段。
                LineHelper.CutLineAtPoint(Line1, pnt, out Line11, out Line12);
                LineHelper.CutLineAtPoint(Line2, pnt, out Line21, out Line22);
                //要判断line11,line12,line21,line22是否为空，为空说明是三叉路口
                if (Line11.Length == 0 || Line12.Length == 0 || Line21.Length == 0 || Line22.Length == 0)
                {
                    //如果是三叉路口，说明交点为Line1，Line2的起点，
                    //该点在节点表已经存在，不需要重新增加节点，但需要修改该节点拓扑关系
                    if ((Line11.Length == 0 || Line12.Length == 0) && (Line21.Length > 0 && Line22.Length > 0))
                    {
                        //得到交点编号
                        if (Line11.Length == 0)
                        {
                            NewNodeID = int.Parse(SegFea1.get_Value(nFNode).ToString());
                        }
                        else
                        {
                            NewNodeID = int.Parse(SegFea1.get_Value(nTNode).ToString());
                        }
                        UpdateNewSeg(seg, SegFea2, Line21, Line22, NewNodeID);
                        //4.原道路删除
                        SegFea2.Delete();
                        //5.将所有segmentid=-1的道路id更新为其ObjectID
                        UpdateSegID(seg);
                        UpdateNodeAttr(NewNodeID, pnt);
                        UpdateNodeAttr(NodeIDs[2], pnt);
                        UpdateNodeAttr(NodeIDs[3], pnt);
                    }
                    else if ((Line21.Length == 0 || Line22.Length == 0) && (Line11.Length > 0 && Line12.Length > 0))
                    {
                        //得到交点编号
                        if (Line21.Length == 0)
                        {
                            NewNodeID = int.Parse(SegFea2.get_Value(nFNode).ToString());
                        }
                        else
                        {
                            NewNodeID = int.Parse(SegFea2.get_Value(nTNode).ToString());
                        }
                        UpdateNewSeg(seg, SegFea1, Line11, Line12, NewNodeID);
                        //4.原道路删除
                        SegFea1.Delete();
                        //5.将所有segmentid=-1的道路id更新为其ObjectID
                        UpdateSegID(seg);
                        UpdateNodeAttr(NewNodeID, pnt);
                        UpdateNodeAttr(NodeIDs[0], pnt);
                        UpdateNodeAttr(NodeIDs[1], pnt);
                    }
                    //Line1，Line2相交于两条道路的起点或终点，则不需打断
                    else
                    {
                        MessageBox.Show("所选两条道路已打断于交点");

                    }
                }
                //四叉路口
                else
                {
                    //要重新产生一个新的节点
                    //获得Node表中节点编号的最大值NodeID。NodeID+1即为新产生点的节点编号

                    NewNodeID = pNodeClass.FeatureCount(null) + 1;

                    //3.将新的路段Line11,Line12,Line21,Line22加入到Segment图层中，
                    //并更新它的各个属性,除了SegmentID

                    UpdateNewSeg(seg, SegFea1, Line11, Line12, NewNodeID);
                    UpdateNewSeg(seg, SegFea2, Line21, Line22, NewNodeID);

                    //4.原道路删除
                    SegFea1.Delete();
                    SegFea2.Delete();

                    //5.将所有segmentid=-1的道路id更新为其ObjectID
                    UpdateSegID(seg);


                    //创建Node节点


                    IFeature NodeFea = CreateNode(pnt, NewNodeID);

                    //更新Line1,Line2首尾四个Node以及新增node的拓扑关系

                    NodeIDs[4] = NewNodeID;
                    PointArray[4] = pnt;
                    int i;
                    for (i = 0; i <= 4; i++)
                    {

                        UpdateNodeAttr(NodeIDs[i], PointArray[i]);


                    }

                }
            }
        }

        /// <summary>
        /// 创建Node
        /// </summary>
        /// <param name="pnt"></param>
        /// <param name="NodeID"></param>
        /// <returns></returns>
        private static IFeature CreateNode(IPoint pnt, int NodeID)
        {
            System.GC.Collect();      //强制对所有代进行垃圾回收
            System.GC.WaitForPendingFinalizers();  //挂起当前线程，直到处理终结器队列的线程清空该队列为止

            IFeature crtPntFeat = pNodeClass.CreateFeature();
            crtPntFeat.Shape = pnt;
            NodeService pNode = new NodeService(pNodeClass, NodeID, pnt);
            if (pNodeClass.FindField(pNode.NodeIDNm) >= 0)
            {
                crtPntFeat.set_Value(pNodeClass.FindField(pNode.NodeIDNm), NodeID);
            }
            crtPntFeat.Store();
            return crtPntFeat;

        }
        
        /// <summary>
        /// 更新被打断道路首尾节点的拓扑关系
        /// </summary>
        /// <param name="nodeID"></param>
        /// <param name="pnt"></param>
        private static void UpdateNodeAttr(int nodeID, IPoint pnt)
        {
            NodeService pNode = new NodeService(pNodeClass, nodeID, pnt);
            SegmentService seg = new SegmentService(pSegClass, 0);
            pNode.CreateAdjData(seg);

        }

        /// <summary>
        /// 更新SegmentID
        /// </summary>
        /// <param name="seg"></param>
        private static void UpdateSegID(SegmentService seg)
        {
            IQueryFilter pQueryFilter = new QueryFilter();
            pQueryFilter.WhereClause = String.Format("{0}={1}", seg.IDNm, -1);
            IFeatureCursor pFeatCursor = pSegClass.Update(pQueryFilter, false);
            IFeature pFeature = pFeatCursor.NextFeature();
            int nObjectID = pSegClass.Fields.FindField("ObjectID");
            int nSegmentID = pSegClass.Fields.FindField(seg.IDNm);

            while (pFeature != null)
            {
                string SegmentID = pFeature.get_Value(nObjectID).ToString();
                pFeature.set_Value(nSegmentID, SegmentID);

                pFeatCursor.UpdateFeature(pFeature);
                pFeature = pFeatCursor.NextFeature();
            }
            pQueryFilter = null;
            pFeatCursor = null;
            pFeature = null;

        }

        /// <summary>
        /// 插入新的Segment
        /// </summary>
        /// <param name="seg"></param>
        /// <param name="pfeature"></param>
        /// <param name="Line11"></param>
        /// <param name="Line12"></param>
        /// <param name="NewNodeID"></param>
        private void InsertIntoSeg(SegmentService seg, IFeature pfeature, IPolyline Line11, IPolyline Line12, int NewNodeID)
        {
            //查找字段的索引，以便赋值
            int nSegmentID = pSegClass.Fields.FindField(seg.IDNm);
            int nFNode = pSegClass.Fields.FindField(seg.FNodeIDNm);
            int nTNode = pSegClass.Fields.FindField(seg.TNodeIDNm);
            int nRoadSegmentID = pSegClass.Fields.FindField(seg.IDNm);
            int nRoadID = pSegClass.Fields.FindField(seg.RelIDNm);
            int nRoadName = pSegClass.Fields.FindField(seg.RoadNameNm);
            int nRoadType = pSegClass.Fields.FindField(seg.RoadTypeNm);
            int nObjectID = pSegClass.Fields.FindField("ObjectID");
            int nLengh = pSegClass.Fields.FindField("SHAPE_Length");

            //获得选中道路的首尾节点编号，路段编号
            int FNodeID1, TNodeID1, SegmentID1;
            GetFTSNodeID(seg, pfeature, out FNodeID1, out TNodeID1, out SegmentID1);

            System.GC.Collect();      //强制对所有代进行垃圾回收
            System.GC.WaitForPendingFinalizers();  //挂起当前线程，直到处理终结器队列的线程清空该队列为止

            IFeature Fea1 = pSegClass.CreateFeature();
            Fea1.Shape = Line11;
            Fea1.set_Value(nSegmentID, Fea1.get_Value(nObjectID));
            Fea1.set_Value(nFNode, FNodeID1);
            Fea1.set_Value(nTNode, NewNodeID);
            Fea1.set_Value(nLengh, Line11.Length);
            Fea1.set_Value(nRoadType, pfeature.get_Value(nRoadType));
            Fea1.set_Value(nRoadName, pfeature.get_Value(nRoadName));
            Fea1.set_Value(nRoadID, pfeature.get_Value(nRoadID));

            Fea1.Store();

            System.GC.Collect();      //强制对所有代进行垃圾回收
            System.GC.WaitForPendingFinalizers();  //挂起当前线程，直到处理终结器队列的线程清空该队列为止

            IFeature Fea2 = pSegClass.CreateFeature();
            Fea2.Shape = Line12;
            Fea2.set_Value(nSegmentID, Fea1.get_Value(nObjectID));
            Fea2.set_Value(nFNode, NewNodeID);
            Fea2.set_Value(nTNode, TNodeID1);
            Fea2.set_Value(nLengh, Line12.Length);
            Fea2.set_Value(nRoadType, pfeature.get_Value(nRoadType));
            Fea2.set_Value(nRoadName, pfeature.get_Value(nRoadName));
            Fea2.set_Value(nRoadID, pfeature.get_Value(nRoadID));
            Fea2.Store();
        }

        /// <summary>
        /// 更新segment的fnodeid和tnodeid
        /// </summary>
        /// <param name="seg"></param>
        /// <param name="pfeature"></param>
        /// <param name="Line11"></param>
        /// <param name="Line12"></param>
        /// <param name="NewNodeID"></param>
        private static void UpdateNewSeg(SegmentService seg, IFeature pfeature, IPolyline Line11, IPolyline Line12, int NewNodeID)
        {
            //查找字段的索引，以便赋值
            int nSegmentID = pSegClass.Fields.FindField(seg.IDNm);
            int nFNode = pSegClass.Fields.FindField(seg.FNodeIDNm);
            int nTNode = pSegClass.Fields.FindField(seg.TNodeIDNm);

            //获得选中道路的首尾节点编号，路段编号
            int FNodeID1, TNodeID1, SegmentID1;
            GetFTSNodeID(seg, pfeature, out FNodeID1, out TNodeID1, out SegmentID1);

            //弧段图层的插入指针,插入Line11

            IFeatureCursor pSegInsert = pSegClass.Insert(true);
            IFeatureBuffer pFeatBuff = null;
            pFeatBuff = pSegClass.CreateFeatureBuffer();

            SetFeatureValue(pfeature, pFeatBuff);
            pFeatBuff.Shape = Line11;
            //新增的segment，其segmentID暂时定为-1
            pFeatBuff.set_Value(nSegmentID, -1);
            pFeatBuff.set_Value(nFNode, FNodeID1);
            pFeatBuff.set_Value(nTNode, NewNodeID);
            pSegInsert.InsertFeature(pFeatBuff);


            //弧段图层的插入指针,插入Line12
            pFeatBuff = pSegClass.CreateFeatureBuffer();

            SetFeatureValue(pfeature, pFeatBuff);
            pFeatBuff.Shape = Line12;
            //新增的segment，其segmentID暂时定为-1
            pFeatBuff.set_Value(nSegmentID, -1);
            pFeatBuff.set_Value(nFNode, NewNodeID);
            pFeatBuff.set_Value(nTNode, TNodeID1);
            pSegInsert.InsertFeature(pFeatBuff);

            pSegInsert.Flush();
            pFeatBuff = null;
            pSegInsert = null;
        }

        /// <summary>
        /// 获得指定feature的首尾节点编号，路段编号
        /// </summary>
        /// <param name="seg"></param>
        /// <param name="pFeature"></param>
        /// <param name="FNodeID"></param>
        /// <param name="TNodeID"></param>
        /// <param name="SegmentID"></param>
        private static void GetFTSNodeID(SegmentService seg, IFeature pFeature, out int FNodeID, out int TNodeID, out int SegmentID)
        {

            //查找字段的索引，以便赋值
            int nSegmentID = pSegClass.Fields.FindField(seg.IDNm);
            int nFNode = pSegClass.Fields.FindField(seg.FNodeIDNm);
            int nTNode = pSegClass.Fields.FindField(seg.TNodeIDNm);
            SegmentID = int.Parse(pFeature.get_Value(nSegmentID).ToString());
            FNodeID = int.Parse(pFeature.get_Value(nFNode).ToString());
            TNodeID = int.Parse(pFeature.get_Value(nTNode).ToString());


        }


        private static void SetFeatureValue(IFeature pFeature, IFeatureBuffer pFeatBuff)
        {
            for (int i = 0; i < pFeatBuff.Fields.FieldCount; i++)
            {
                IField pField = pFeatBuff.Fields.get_Field(i);
                if (pField.Editable)
                {
                    int nIndex = pFeature.Fields.FindField(pField.Name);
                    if (nIndex > -1)
                    {
                        pFeatBuff.set_Value(i, pFeature.get_Value(nIndex));
                    }
                }
            }
        }

    }
}
