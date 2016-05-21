using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.ADO.Access;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.Road;
using RoadNetworkSystem.ElementService.LaneBasedNetwork.LinkLayer;
using RoadNetworkSystem.GIS;
using RoadNetworkSystem.GIS.GeoDatabase.Dataset;
using RoadNetworkSystem.GIS.GeoDatabase.WorkSpace;
using RoadNetworkSystem.NetworkElement.RoadLayer;
using RoadNetworkSystem.NetworkExtraction.LaneBasedNetwork.DatabaseManager;
using RoadNetworkSystem.NetworkExtraction.LaneBasedNetwork.LinkLayer;
using System;
using System.Data.OleDb;
using System.Windows.Forms;
using RoadNetworkSystem.NetworkExtraction.LaneBasedNetwork.LinkLayer;
using RoadNetworkSystem.WinForm.NetworkExtraction;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LinkLayer;
using RoadNetworkSystem.NetworkExtraction.LaneBasedNetwork.LaneLayer;
using RoadNetworkSystem.DataModel.RoadSign;
using RoadNetworkSystem.ElementService.LaneBasedNetwork.NetworkBuilder.LaneLayer;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.RoadSegmentLayer;

namespace RoadNetworkSystem.NetworkExtraction.Road2BasicRoadNetwork
{
    class Segment2BasicRoadNetwork
    {
        private Form1 _frm1;

        OleDbConnection _conn;
        IFeatureClass _feaClsRoad;
        IFeatureClass _feaClsSegment;
        IFeatureClass _feaClsBreakPoint;

        IFeatureClass _feaClsLink;
        IFeatureClass _feaClsNode;
        IFeatureClass _feaClsArc;

        IFeatureClass _feaClsLane;
        IFeatureClass _feaClsConnector;
        IFeatureClass _feaClsKerb;

        IFeatureClass _feaClsSurface;
        IFeatureClass _feaClsBoundary;
        IFeatureClass _feaClsTurnArrow;

        IFeatureClass _feaClsStopLine;



        public Segment2BasicRoadNetwork(Form1 frm1)
        {
            this._frm1 = frm1;
            this._feaClsRoad = frm1.FeaClsRoad;
            this._feaClsSegment = frm1.FeaClsSegment;
            _conn = _frm1.Conn;
        }


        public void Convert2BasicRoadNetwork()
        {
            //处理的时候，均默认为双向的
            //多线段转为直线段

            /*
             * 1. Road -> Segment层
             *      利用“路网提取-中心线到路段路网”提取出Segment层
             * 2. Segment层 ->   Link层  
             *      交通组织中断处打断
             *      多线段转为直线段
             *      创建Node
             *      
             * 3. Link层 ->  Lane层
             * 
             */
            
            //1
            //把原始的mdb中的BreakPoint,LaneNumChange移到新的数据库中

            //breakpoint的要素类
            _feaClsBreakPoint = (_frm1.Wsp as IFeatureWorkspace).OpenFeatureClass(BreakPoint.BreakPointName);
            
            //2
            //2.1   创建Link/Node/Arc图层
            _feaClsLink = DatabaseDesigner.CreateLinkClass(_feaClsBreakPoint.FeatureDataset);
            _feaClsNode = DatabaseDesigner.CreateNodeClass(_feaClsBreakPoint.FeatureDataset);
            _feaClsArc = DatabaseDesigner.CreateArcClass(_feaClsBreakPoint.FeatureDataset);

            //2.2   在交通组织变化处打断
            breakSegmentInTrafficDisturb();

            //2.3   多线段转为直线段
            breakSegmentInKinkPoint();

            //2.4   创建Node
            LinkLayerBuilder linkLayerFactory = new LaneBasedNetwork.LinkLayer.LinkLayerBuilder(_feaClsLink, _feaClsNode, _feaClsArc);
            linkLayerFactory.createNodesForLinkAndArc();



            //3   Link层到  Lane层

            _feaClsBoundary = DatabaseDesigner.CreateBoudaryClass(_feaClsBreakPoint.FeatureDataset);
            _feaClsConnector = DatabaseDesigner.CreateConnectorClass(_feaClsBreakPoint.FeatureDataset);
            _feaClsKerb = DatabaseDesigner.CreateKerbClass(_feaClsBreakPoint.FeatureDataset);


            _feaClsLane = DatabaseDesigner.CreateLaneClass(_feaClsBreakPoint.FeatureDataset);
            _feaClsStopLine = DatabaseDesigner.CreateStopLineClass(_feaClsBreakPoint.FeatureDataset);
            _feaClsSurface = DatabaseDesigner.CreateSurfaceClass(_feaClsBreakPoint.FeatureDataset);

            _feaClsTurnArrow = DatabaseDesigner.CreateTurnArrowClass(_feaClsBreakPoint.FeatureDataset);

            Dictionary<string, IFeatureClass> feaClsDic = new Dictionary<string, IFeatureClass>();
            feaClsDic.Add(Arc.ArcFeatureName, _feaClsArc);
            feaClsDic.Add(Boundary.BoundaryName, _feaClsBoundary);
            feaClsDic.Add(LaneConnector.ConnectorName, _feaClsConnector);



            feaClsDic.Add(Node.NodeName, _feaClsNode);
            feaClsDic.Add(Lane.LaneName, _feaClsLane);
            feaClsDic.Add(Link.LinkName, _feaClsLink);

            feaClsDic.Add(Kerb.KerbName, _feaClsKerb);
            feaClsDic.Add(StopLine.StopLineName, _feaClsStopLine);
            feaClsDic.Add(Surface.SurfaceName, _feaClsSurface);

            feaClsDic.Add(TurnArrow.TurnArrowName, _feaClsTurnArrow);

            LaneLayerBatchBuilder laneLayerBatchBilder = new LaneLayerBatchBuilder(feaClsDic);
            laneLayerBatchBilder.CreateLaneOpologyBatch();
            //LaneLayerBuilder laneLayerFactory = new LaneLayerBuilder(feaClsDic);
            
            ////为每个Arc初始化所有的Lane
            //laneLayerFactory.InitLaneBatch();

            //laneLayerFactory.CreateLinkTopologyBatch();
        }


        #region ------------------ 在交通组织中断处打断Segment生成Link --------------------


        /// <summary>
        /// 在交通组织中断处打断
        /// </summary>
        private void breakSegmentInTrafficDisturb()
        {
            //BreakPoint,LaneNumChange

            List<int> untagRoads = new List<int>();

            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = "";
            IFeatureCursor query = _feaClsSegment.Search(filter, false);
            IFeature pFeatureSegment = query.NextFeature();

            while (pFeatureSegment != null)
            {
                
                SegmentService segmentService = new SegmentService(_feaClsSegment, 0);
                LinkMaster linkMaster = segmentService.GetEntity(pFeatureSegment);
                Segment segment = new Segment();
                segment = segment.Copy(linkMaster);

                if (segment.ID == 6 ||
                    segment.ID == 8 ||
                    segment.ID == 9 ||
                    segment.ID == 10)
                {
                    int test = 10;
                }


                int flowDir = Link.FLOWDIR_DOUBLE;
               
                //遍历属于同一个Road的各个LaneNumChange
                string sql = "Select * from " + LaneNumChange.LaneNumChangeName +
                    " where " + LaneNumChange.SegmentID_Name + " = " + segment.ID;
                OleDbCommand cmd = new OleDbCommand(sql, _conn);
                OleDbDataReader reader = cmd.ExecuteReader();

                bool tagedFlag = false;
                while (reader.Read())
                {
                    tagedFlag = true;
                    int doneFlag = Convert.ToInt32(reader[LaneNumChange.DoneFlag_Name]);
                    if (doneFlag == LaneNumChange.DONEFLAG_DONE)
                    {
                        continue;
                    }
                    int fromBreakPointId  = -1;
                    if(reader[LaneNumChange.FromBreakPointID_Name] != DBNull.Value)
                    {
                        fromBreakPointId = Convert.ToInt32(reader[LaneNumChange.FromBreakPointID_Name]);
                    }
                    
                    int toBreakPointId  = -1;
                    if (reader[LaneNumChange.ToBreakPointID_Name] != DBNull.Value)
                    {
                        toBreakPointId = Convert.ToInt32(reader[LaneNumChange.ToBreakPointID_Name]);
                    }
                    int laneNum = Convert.ToInt32(reader[LaneNumChange.LaneNum_Name]);
                    int breakPointFlowDir = Convert.ToInt32(reader[LaneNumChange.FlowDir_Name]);
                    int laneNumChangeId = Convert.ToInt32(reader[LaneNumChange.LaneNumChangeID_Name]);

                    
                    LaneNumChange currentLaneNumChange = new LaneNumChange();
                    currentLaneNumChange.LaneNumChangeID = laneNumChangeId;
                    currentLaneNumChange.DoneFlag = LaneNumChange.DONEFLAG_DONE;
                    currentLaneNumChange.FromBreakPointID = fromBreakPointId;

                    currentLaneNumChange.ToBreakPointID = toBreakPointId;
                    currentLaneNumChange.LaneNum = laneNum;
                    currentLaneNumChange.FlowDir = breakPointFlowDir;
                    currentLaneNumChange.SegmentID = segment.ID;

                    LaneNumChangeService laneNumChange = new LaneNumChangeService(_conn);
                    int oppositionDir = (currentLaneNumChange.FlowDir == Link.FLOWDIR_SAME?Link.FLOWDIR_OPPOSITION:Link.FLOWDIR_SAME);
                    LaneNumChange oppositionLaneNumChange = laneNumChange.GetOppositeDirectionLaneNumChange(segment.ID,oppositionDir,
                        fromBreakPointId, toBreakPointId);

                    int oppositionLaneNum = -1;
                    flowDir = breakPointFlowDir;
                    if (oppositionLaneNumChange != null)
                    {
                        oppositionLaneNum = oppositionLaneNumChange.LaneNum;
                        flowDir = Link.FLOWDIR_DOUBLE;
                    }



                    if (!isRoadFlowDirValid(flowDir,currentLaneNumChange, oppositionLaneNumChange))
                    {
                        //MessageBox.Show("Road objectid = " + pFeatureRoad.OID + " LaneNumChange 不匹配"); 
                    }
                    LaneNumChangeService laneNumChangeService = new LaneNumChangeService(_conn);
                    bool isSameDirectionFlag = laneNumChangeService.isCurrentLaneNumChangeSameDirection(pFeatureSegment.Shape as IPolyline,
                        currentLaneNumChange,_feaClsBreakPoint);
                    Link link = new Link();
                    link.FlowDir = flowDir;
                    link.RoadName = segment.RoadName;
                    link.RoadType = segment.RoadType;
                    link.RelID = segment.ID;

                    IPolyline linkLine = createLinkPolylineForTrafficDisturb(fromBreakPointId,toBreakPointId,pFeatureSegment,isSameDirectionFlag);
                    if (linkLine == null)
                    {
                        continue;
                    }

                    LinkLayerBuilder linkLayerFactory = new LinkLayerBuilder(_feaClsLink,_feaClsNode,_feaClsArc);
                    if (isSameDirectionFlag)
                    {
                        linkLayerFactory.createLinkAndArcs(link, linkLine, laneNum, oppositionLaneNum);
                    }
                    else 
                    {
                        linkLayerFactory.createLinkAndArcs(link, linkLine, oppositionLaneNum, laneNum);
                    }


                    currentLaneNumChange.DoneFlag = LaneNumChange.DONEFLAG_DONE;
                    laneNumChangeService.UpdateLaneNumChangeDoneFlag(currentLaneNumChange);
                    if (oppositionLaneNumChange != null)
                    {
                        oppositionLaneNumChange.DoneFlag = LaneNumChange.DONEFLAG_DONE;
                        laneNumChangeService.UpdateLaneNumChangeDoneFlag(oppositionLaneNumChange);
                    }
                }
                reader.Close();
                reader.Dispose();
                if (!tagedFlag)
                {
                    untagRoads.Add(segment.ID);
                }
                pFeatureSegment = query.NextFeature();
            }

        }


        /// <summary>
        /// 根据frombreakPoint和toBreakPoint得到Link的几何
        /// </summary>
        /// <param name="fromBreakPointId"></param>
        /// <param name="toBreakPointId"></param>
        /// <param name="roadFeature"></param>
        /// <param name="breakPointRoadLineSameFlag"></param>
        /// <returns></returns>
        private IPolyline createLinkPolylineForTrafficDisturb(int fromBreakPointId, int toBreakPointId, 
            IFeature roadFeature,bool breakPointRoadLineSameFlag)
        {
            IPolyline roadLine = roadFeature.Shape as IPolyline;
            IPoint fromBreakPointPoint = new PointClass();
            IPoint toBreakPointPoint = new PointClass();

            BreakPointService breakPointService = new BreakPointService(_feaClsBreakPoint, 0);
            breakPointService.getBreakPointPoints(roadLine, fromBreakPointId, toBreakPointId, breakPointRoadLineSameFlag,
                ref fromBreakPointPoint, ref toBreakPointPoint);

            if (roadLine.Length == 0)
            {
                return null;
            }

            if (breakPointRoadLineSameFlag)
            {
                return LineHelper.CutPolylineByPointsOnLine(roadLine, fromBreakPointPoint, toBreakPointPoint);
            }
            else
            {
                return LineHelper.CutPolylineByPointsOnLine(roadLine, toBreakPointPoint, fromBreakPointPoint);
            }

        }


        /// <summary>
        /// 判断road的FlowDir字段值是否合法
        /// </summary>
        /// <param name="roadFlowDir"></param>
        /// <param name="currentLaneChange"></param>
        /// <param name="oppositionLaneChange"></param>
        /// <returns></returns>
        private bool isRoadFlowDirValid(int roadFlowDir, LaneNumChange currentLaneChange, 
            LaneNumChange oppositionLaneChange)
        {
            if (roadFlowDir == Link.FLOWDIR_DOUBLE)
            {
                if (currentLaneChange != null && oppositionLaneChange != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (currentLaneChange == null || oppositionLaneChange == null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private IPolyline createLinkPolylineForTrafficDisturb(int fromBreakPointId, int toBreakPointId, 
            IFeature roadFeature)
        {
            BreakPointService fromBreakPoint = new BreakPointService(_feaClsBreakPoint, fromBreakPointId);
            IFeature fromBreakPointFeature = fromBreakPoint.getBreakPointFeature();

            BreakPointService toBreakPoint = new BreakPointService(_feaClsBreakPoint, toBreakPointId);
            IFeature toBreakPointFeature = toBreakPoint.getBreakPointFeature();

            IPolyline roadLine = roadFeature.Shape as IPolyline;
            if (roadLine.Length == 0)
            {
                return null;
            }
            return LineHelper.CutPolylineByPointsOnLine(roadLine, fromBreakPointFeature.Shape as IPoint, toBreakPointFeature.Shape as IPoint);
        }

        #endregion ------------------ 在交通组织中断处打断Segment生成Link --------------------

        #region ------------------ 把长弧度polyline转换为line --------------------
        /// <summary>
        /// 多线段转为直线段
        /// </summary>
        private void breakSegmentInKinkPoint()
        {
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = "";
            IFeatureCursor cursor = _feaClsLink.Search(filter, false);
            IFeature pFeatureLink = cursor.NextFeature();
            while (pFeatureLink != null)
            {
                LinkService linkService = new LinkService(_feaClsLink, 0);
                LinkMaster linkMstr = linkService.GetEntity(pFeatureLink);

                Link link = new Link();
                link = link.Copy(linkMstr);

                if (link.ID == 19)
                {
                    int test = 0;
                }

                List<IPolyline> lines = LineHelper.ConvertPolyline2Lines(pFeatureLink.Shape as IPolyline);
                if (lines.Count > 1) 
                {
                    linkService.breakLinkIntoLinksWithoutNodeUpdate(pFeatureLink, lines, _feaClsArc);
                }
                
                pFeatureLink = cursor.NextFeature();
            }
        }

        #endregion ------------------ 把长弧度polyline转换为line --------------------


    }
}
