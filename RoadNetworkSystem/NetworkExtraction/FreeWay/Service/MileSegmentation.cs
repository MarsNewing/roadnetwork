using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.GIS;
using RoadNetworkSystem.GIS.GeoDatabase.Dataset;
using RoadNetworkSystem.GIS.Geometry;
using RoadNetworkSystem.NetworkExtraction.FreeWay.Dao;
using RoadNetworkSystem.NetworkExtraction.FreeWay.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RoadNetworkSystem.NetworkExtraction.FreeWay
{

    /// <summary>
    /// 按照里程桩给Road分段
    /// </summary>
    class MileSegmentation
    {
        private IFeatureClass pFeaClsRoad;
        private IFeatureClass pFeaClsSegment;
        private IFeatureClass pFeaClsSegNode;

        private IFeatureClass pFeaClsNode;
        private IFeatureClass pFeaClsArc;

        /// <summary>
        /// 打断长度
        /// </summary>
        private const int BREAK_LEN = 1000;

        public MileSegmentation(Dictionary<string,IFeatureClass> feaClsDic)
        {
            pFeaClsRoad = feaClsDic[Road.FEATURE_ROAD_NAME];
            pFeaClsSegment = feaClsDic[Segment.FEATURE_SEGMENT_NAME];
            pFeaClsSegNode = feaClsDic[SegNode.FEATURE_SEGNODE_NAME];

            pFeaClsNode = feaClsDic[Node.FEATURE_NODE_NAME];
            pFeaClsArc = feaClsDic[Arc.FEATURE_ARC_NAME];
        }


        /// <summary>
        /// 用里程桩(SegNode)Road打断为Segment，暂时不生成SegNode
        /// </summary>
        public void SegmentRoad()
        {
            if (pFeaClsSegNode == null || pFeaClsRoad == null)
            {
                MessageBox.Show("< BreakRoad2SegmentByMileStone > 请确认加载的有SegNode和Road图层");
                return;
            }

            //遍历所有的SegNode
            #region 按照一公里打断Road
            
            IQueryFilter fileter = new QueryFilterClass();
            fileter.WhereClause = "";
            IFeatureCursor cursor;
            cursor = pFeaClsSegNode.Search(fileter, false);
            IFeature pFeature = cursor.NextFeature();

            //已经处理了的Road，在处理前需要查看是否处理过
            List<int> doneRoadList = new List<int>();


            while (pFeature != null)
            {
                int roadId = Convert.ToInt32(pFeature.get_Value(pFeaClsSegNode.FindField(SegNode.FIELDE_ROAD_ID)));

                if (isRoadDone(doneRoadList, roadId))
                {
                    continue;
                }

                IFeature feaRoad = getRoadFeature(roadId);
                //找到Road
                if (feaRoad == null)
                {
                    MessageBox.Show("< BreakRoad2SegmentByMileStone > 请确认RoadID = " + roadId + " 存在");
                    continue;
                }

              
                breakRoad2SegmentByMileStone(pFeature, feaRoad);

                pFeature = cursor.NextFeature();

            }
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();

            #endregion

        }


        private void breakRoad2SegmentByMileStone(IFeature mileStone,IFeature road)
        {
            //获取流向与里程桩的相对方向，相同1，相反-1
            int increaseMileFlowDirection = Convert.ToInt16(road.get_Value(pFeaClsRoad.FindField(Road.FIELDE_FLOW_DIR)));

            IPoint mileStonePoint = mileStone.Shape as IPoint;
            IPolyline roadLine = road.Shape as IPolyline;
            IPoint correspondingPointOnRoad = PhysicalTopology.GetNearestPointOnLine(mileStonePoint, roadLine);

            IPolyline preLine = new PolylineClass();
            IPolyline postLine = new PolylineClass();
            LineHelper.CutLineAtPoint(roadLine, correspondingPointOnRoad, out preLine, out postLine);


            int preMileStoneNum = Convert.ToInt32(preLine.Length) / BREAK_LEN;
            //打断点位于终点处
            int startEndDirection = - 1;
            createSegments(mileStone, preLine, road, increaseMileFlowDirection, startEndDirection);


            int postMileStoneNum = Convert.ToInt32(postLine.Length) / BREAK_LEN;
            //打断点位于起点处
            startEndDirection = 1;
            createSegments(mileStone, postLine, road, increaseMileFlowDirection, startEndDirection);
        }


        private void createSegments(IFeature segNodeFea,IPolyline line,IFeature roadFeature,
            int increaseMileFlowDirection, int startEndDirection)
        {
            string mileStone = Convert.ToString(segNodeFea.get_Value(pFeaClsSegNode.FindField(SegNode.FIELDE_SEG_NODE_LANEMARK_ID)));
            int segNodeId = Convert.ToInt32(segNodeFea.get_Value(pFeaClsSegNode.FindField(SegNode.FIELDE_SEG_NODE_ID)));


            string[] arr = mileStone.Split(':');
            if (arr.Length !=  2)
                return;
            int mileStoneNum = Convert.ToInt32(line.Length) / BREAK_LEN;
            int miles = Convert.ToInt32(arr[1]);

            double fromDistance = 0;
            double endDistance = 0;
            int temMiles = 0;

            int fSegNodeId = -1;
            int tSegNodeId = -1;

            for (int i = 0; i <= mileStoneNum; i++)
            {

                #region 根据increaseMileFlowDirection，与 startEndDirection 确定 截取量、fsegnodeid和tsegnodeid
                if (increaseMileFlowDirection == 1 && startEndDirection == 1)
                {
                    /*
                     *   increase direction -->
                     * 0 -----------------------------> 
                     * 
                    */
                    temMiles = miles + i + 1;
                    if (i == 0)
                    {
                        fromDistance = 0;
                        endDistance = BREAK_LEN;

                        fSegNodeId = segNodeId;
                        //tsegnode在新建Segment后新建
                    }
                    else if (i == mileStoneNum)
                    {
                        fromDistance = endDistance;
                        endDistance = line.Length;


                        fSegNodeId = tSegNodeId;
                        //tsegnode在新建Segment后新建
                    }
                    else
                    {
                        fromDistance = endDistance;
                        endDistance += BREAK_LEN;

                        fSegNodeId = tSegNodeId;
                        //tsegnode在新建Segment后新建
                    }
                }

                else if (increaseMileFlowDirection == 1 && startEndDirection == -1)
                {
                    /*
                     *   increase direction -->
                     * -----------------------------> 0
                     * 
                    */

                    temMiles = miles - i - 1;
                    if (i == 0)
                    {
                        endDistance = line.Length;
                        fromDistance = endDistance - BREAK_LEN * (i + 1);

                        tSegNodeId = segNodeId;
                        //fsegnode在新建Segment后新建
                    }
                    else if (i == mileStoneNum)
                    {
                        endDistance = fromDistance;
                        fromDistance = 0;

                        tSegNodeId = fSegNodeId;
                        //fsegnode在新建Segment后新建
                    }
                    else
                    {
                        endDistance = fromDistance;
                        fromDistance -= BREAK_LEN;

                        tSegNodeId = fSegNodeId;
                        //fsegnode在新建Segment后新建
                    }
                }
                else if (increaseMileFlowDirection == -1 && startEndDirection == -1)
                {
                    /*
                     *    increase direction -->
                     * 0 <----------------------------- 
                     * 
                    */

                    temMiles = miles + i + 1;
                    if (i == 0)
                    {
                        endDistance = line.Length;
                        fromDistance = endDistance - BREAK_LEN * (i + 1);

                        tSegNodeId = segNodeId;
                        //fsegnode在后续新建segment时新建
                    }
                    else if (i == mileStoneNum)
                    {

                        endDistance = fromDistance;
                        fromDistance = 0;

                        tSegNodeId = fSegNodeId;
                        //fsegnode在后续新建segment时新建

                    }
                    else
                    {
                        endDistance = fromDistance;
                        fromDistance -= BREAK_LEN;

                        tSegNodeId = fSegNodeId;
                        //fsegnode在后续新建segment时新建
                    }
                }
                else if (increaseMileFlowDirection == -1 && startEndDirection == 1)
                {
                    /*
                     *    increase direction -->
                     *  <----------------------------- 0
                     * 
                    */

                    temMiles = miles - i - 1;
                    if (i == 0)
                    {
                        fromDistance = 0;
                        endDistance = BREAK_LEN;

                        fSegNodeId = segNodeId;
                        //fsegnode在新建segment时新建
                    }
                    else if (i == mileStoneNum)
                    {
                        fromDistance = endDistance;
                        endDistance = line.Length;

                        fSegNodeId = tSegNodeId;
                        //fsegnode在新建segment时新建
                    }
                    else
                    {
                        fromDistance = endDistance;
                        endDistance += BREAK_LEN;

                        fSegNodeId = tSegNodeId;
                        //fsegnode在新建segment时新建
                    }
                }

                if (fromDistance < 0)
                {
                    fromDistance = 0;
                }
                #endregion




                double endMeasure = line.Length - endDistance;

                if (endMeasure < 0)
                {
                    endMeasure = 0;
                }

                IPolyline newLine = LineHelper.CreateLine(line, fromDistance, endMeasure);


                if (startEndDirection == 1)
                {
                    //  <----------------------0<------- 0
                    IPoint pnt = newLine.ToPoint;
                    SegNodeDao segNodeDao = new SegNodeDao(pFeaClsSegNode);
                    SegNode segNode = new SegNode();
                    segNode.RoadID = Convert.ToInt32(roadFeature.get_Value(pFeaClsRoad.FindField(Road.FIELDE_ROAD_ID)));
                    segNode.SegNodeLandmark = arr[0] + ":" + temMiles;
                    tSegNodeId = segNodeDao.CreateSegNode(segNode, pnt);
                }
                else
                {
                    //  0<--------0<--------------------- 
                    IPoint pnt = newLine.FromPoint;
                    SegNodeDao segNodeDao = new SegNodeDao(pFeaClsSegNode);
                    SegNode segNode = new SegNode();
                    segNode.RoadID = Convert.ToInt32(roadFeature.get_Value(pFeaClsRoad.FindField(Road.FIELDE_ROAD_ID)));
                    segNode.SegNodeLandmark = arr[0] + ":" + temMiles;
                    fSegNodeId = segNodeDao.CreateSegNode(segNode, pnt);
                }


                SegmentDao segDao = new SegmentDao(pFeaClsSegment);
                Segment seg = new Segment();
                seg.FSegNodeID = fSegNodeId;
                seg.TSegNodeID = tSegNodeId;
                segDao.CreateSegment(seg, newLine);
            }
        }

        /// <summary>
        /// 获取一个Road要素
        /// </summary>
        /// <param name="roadId"></param>
        /// <returns></returns>
        private IFeature getRoadFeature(int roadId)
        {
            IQueryFilter filter2 = new QueryFilterClass();
            filter2.WhereClause = Road.FIELDE_ROAD_ID + "=" + roadId;
            IFeatureCursor cursor2 = pFeaClsRoad.Search(filter2, false);

            IFeature feaRoad = cursor2.NextFeature();
            return feaRoad;
        }

        /// <summary>
        /// 判断一个Road是否打断过，如果没有，加入列表中
        /// </summary>
        /// <param name="doneRoadList"></param>
        /// <param name="roadId"></param>
        /// <returns></returns>
        private bool isRoadDone(List<int> doneRoadList, int roadId)
        {
            if (doneRoadList.Contains(roadId))
            {
                return true;
            }
            else
            {
                doneRoadList.Add(roadId);
                return false;
            }
        }



    }
}
