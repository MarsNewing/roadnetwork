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
using RoadNetworkSystem.WinForm.NetworkExtraction;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RoadNetworkSystem.NetworkExtraction.Road2BasicRoadNetwork
{
    class Road2BasicRoadNetwork
    {
        private Form1 _frm1;

        IFeatureClass _feaClsRoad;
        IFeatureClass _feaClsSegment;
        IFeatureClass _feaClsBreakPoint;

        IFeatureClass _feaClsLink;
        IFeatureClass _feaClsNode;
        IFeatureClass _feaClsArc;

        public Road2BasicRoadNetwork(Form1 frm1)
        {
            this._frm1 = frm1;
            this._feaClsRoad = frm1.FeaClsRoad;
            this._feaClsSegment = frm1.FeaClsSegment;
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

            breakSegmentInKinkPoint();
        }


        

        /// <summary>
        /// 在交通组织中断处打断
        /// </summary>
        private void breakSegmentInTrafficDisturb()
        {
            //BreakPoint,LaneNumChange
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = "";
            IFeatureCursor query = _feaClsRoad.Search(filter, false);
            IFeature pFeatureRoad = query.NextFeature();
            while (pFeatureRoad != null)
            {
                RoadService roadService = new RoadService(_feaClsRoad,0);

                Road road = roadService.GetEntity(pFeatureRoad);


                int flowDir = Convert.ToInt32(pFeatureRoad.get_Value(pFeatureRoad.Fields.FindField(RoadNetworkSystem.DataModel.Road.Road.FlowDirName)));
               
                OleDbConnection conn = AccessHelper.OpenConnection(_frm1.Wsp.PathName);
                string sql = "Select * from " + LaneNumChange.LaneNumChangeName +
                    " where " + LaneNumChange.RoadID_Name + " = " + Convert.ToInt32(pFeatureRoad.get_Value(pFeatureRoad.Fields.FindField(LaneNumChange.RoadID_Name))) +
                    " and  " + LaneNumChange.DoneFlag_Name + " = " + LaneNumChange.DONEFLAG_UNDO;
                OleDbCommand cmd = new OleDbCommand(sql, conn);
                OleDbDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
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
                    LaneNumChange currentLaneNumChange = new LaneNumChange();
                    currentLaneNumChange.DoneFlag = LaneNumChange.DONEFLAG_DONE;
                    currentLaneNumChange.FromBreakPointID = fromBreakPointId;
                    currentLaneNumChange.ToBreakPointID = toBreakPointId;
                    currentLaneNumChange.LaneNum = laneNum;
                    currentLaneNumChange.FlowDir = breakPointFlowDir;


                    LaneNumChangeService laneNumChange = new LaneNumChangeService(conn);
                    LaneNumChange oppositionLaneNumChange = laneNumChange.GetOppositeDirectionLaneNumChange(fromBreakPointId, toBreakPointId);
                    int oppositionLaneNum = -1;
                    if (oppositionLaneNumChange != null)
                    {
                        oppositionLaneNum = oppositionLaneNumChange.LaneNum;
                    }

                    if (!isRoadFlowDirValid(flowDir,currentLaneNumChange, oppositionLaneNumChange))
                    {
                        MessageBox.Show("Road objectid = " + pFeatureRoad.OID + " LaneNumChange 不匹配"); 
                    }

                    bool isSameDirectionFlag = isCurrentLaneNumChangeSameDirection(pFeatureRoad.Shape as IPolyline, currentLaneNumChange);
                    Link link = new Link();
                    link.FlowDir = flowDir;
                    link.RoadName = road.RoadName;
                    link.RoadType = road.RoadType;
                    IPolyline linkLine = createLinkPolylineForTrafficDisturb(fromBreakPointId,toBreakPointId,pFeatureRoad,isSameDirectionFlag);

                    LinkLayerFactory linkLayerFactory = new LinkLayerFactory(_feaClsLink,_feaClsNode,_feaClsArc);
                    if (isSameDirectionFlag)
                    {
                        linkLayerFactory.createLinkAndArcs(link, linkLine, laneNum, oppositionLaneNum);
                    }
                    else 
                    {
                        linkLayerFactory.createLinkAndArcs(link, linkLine, oppositionLaneNum, laneNum);
                    }
                }
                reader.Close();
                reader.Dispose();

                pFeatureRoad = query.NextFeature();
            }
        }


        /// <summary>
        /// 判断LaneNumChange表示的方向与Road的数字化方向是否相同
        /// </summary>
        /// <param name="roadLine"></param>
        /// <param name="currentLaneNumChange"></param>
        /// <returns></returns>
        private bool isCurrentLaneNumChangeSameDirection(IPolyline roadLine ,LaneNumChange currentLaneNumChange)
        {
            IPoint fromPointPoint = new PointClass();


            IPoint toPointPoint = new PointClass();
            getBreakPointPoints(roadLine, currentLaneNumChange.FromBreakPointID, currentLaneNumChange.ToBreakPointID, currentLaneNumChange.FlowDir == Link.FLOWDIR_SAME?true:false,
                ref fromPointPoint, ref toPointPoint);

            double vector_x_LaneNumChange = toPointPoint.X - fromPointPoint.X ;
            double vector_y_LaneNumChange = toPointPoint.Y - fromPointPoint.Y;

            double vector_x_Road = roadLine.ToPoint.X - roadLine.FromPoint.X;
            double vector_y_Road = roadLine.ToPoint.Y - roadLine.FromPoint.Y;

            double product = vector_x_LaneNumChange * vector_x_Road + vector_y_LaneNumChange * vector_y_Road;
            if (product > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        private void getBreakPointPoints(IPolyline roadLine, int fromBreakPointId, int toBreakPointId,bool breakPointRoadLineSameFlag ,
            ref IPoint fromBreakPointPoint,ref IPoint toBreakPointPoint)
        {
            if (fromBreakPointId > 0)
            {

                BreakPointService fromBreakPoint = new BreakPointService(_feaClsBreakPoint, fromBreakPointId);
                IFeature fromBreakPointFeature = fromBreakPoint.getBreakPointFeature();
                fromBreakPointPoint = fromBreakPointFeature.Shape as IPoint;
            }
            else
            {
                if (breakPointRoadLineSameFlag)
                {
                    fromBreakPointPoint = roadLine.FromPoint;
                }
                else
                {
                    fromBreakPointPoint = roadLine.ToPoint;
                }
            }

            
            if (toBreakPointId > 0)
            {
                BreakPointService toBreakPoint = new BreakPointService(_feaClsBreakPoint, toBreakPointId);
                IFeature toBreakPointFeature = toBreakPoint.getBreakPointFeature();
                toBreakPointPoint = toBreakPointFeature.Shape as IPoint;
            }
            else
            {
                if (breakPointRoadLineSameFlag)
                {
                    toBreakPointPoint = roadLine.ToPoint;
                }
                else
                {
                    toBreakPointPoint = roadLine.FromPoint;
                }
            }
            
        }


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


            getBreakPointPoints(roadLine, fromBreakPointId, toBreakPointId, breakPointRoadLineSameFlag,
                ref fromBreakPointPoint, ref toBreakPointPoint);
            
            return LineHelper.CutPolylineByPointsOnLine(roadLine, fromBreakPointPoint, toBreakPointPoint);
        }


        /// <summary>
        /// 多线段转为直线段
        /// </summary>
        private void breakSegmentInKinkPoint()
        { 
        }
             

    }
}
