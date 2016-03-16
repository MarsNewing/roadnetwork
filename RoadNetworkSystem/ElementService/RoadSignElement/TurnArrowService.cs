using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.ADO.Access;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.RoadSign;
using RoadNetworkSystem.GIS;
using RoadNetworkSystem.GIS.GeoDatabase.Dataset;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.Connection;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LaneLayer;
using System;
using System.Collections.Generic;
using System.Data.OleDb;

namespace RoadNetworkSystem.NetworkElement.RoadSignElement
{
    class TurnArrowService
    {
        private double ARROWPOSITION = 8;
        private const int ARROW_COUNT_IN_SHORT_LANE = 2;
        private const int ARROW_COUNT_IN_LONG_LANE = 3;

        private const int LENGHT_LONG_OR_SHORT = 50;

        public const string ArrowIDNm = "ArrowID";
        public const string STYLEID_NAME = "StyleID";
        public const string ArrowTypeNm = "ArrowType";

        public const string SerialNm = "Serial";
        public const string ANGLENm = "ANGLE";
        public const string ArcIDNm = "ArcID";

        public const string LaneIDNm = "LaneID";
        public const string PrecedeArrowsNm = "PrecedeArrows";
        public const string OtherNm = "Other";

        #region --------------------导向箭头的样式---------------------------
        public const int uturnStraStyle = -264;
        public const int uturnLeftStyle = -263;
        public const int uturnStyle = -265;

        public const int leftRightStyle = -262;

        public const int straRightStyle = -261;
        public const int rightStyle = -260;

        public const int straLeftStyle = -259;
        public const int leftStyle = -258;

        public const int straightStyle = -257;

        #endregion --------------------导向箭头的样式---------------------------

        public IFeatureClass FeaClsTurnArrow;
        private IFeatureClass FeaClsTurnLane;
        private IFeatureClass pFeaClsLane;
        private IFeatureClass pFeaClsNode;
        private IFeatureClass pFeaClsLink;
        private IFeatureClass pFeaClsArc;
        private IFeatureClass pFeaClsConnector;
        public int ArrowID;
        public TurnArrowService(IFeatureClass pFeaClsTurnArrow, int arrowID)
        {
            FeaClsTurnArrow = pFeaClsTurnArrow;
            pFeaClsLane = FeatureClassHelper.GetFeaClsInAccess(pFeaClsTurnArrow.FeatureDataset.Workspace.PathName,
                Lane.LaneName);
            pFeaClsNode = FeatureClassHelper.GetFeaClsInAccess(pFeaClsTurnArrow.FeatureDataset.Workspace.PathName,
               Node.NodeName);

            pFeaClsLink = FeatureClassHelper.GetFeaClsInAccess(pFeaClsTurnArrow.FeatureDataset.Workspace.PathName,
                Link.LinkName);
            pFeaClsArc = FeatureClassHelper.GetFeaClsInAccess(pFeaClsTurnArrow.FeatureDataset.Workspace.PathName,
                Arc.ArcFeatureName);
            pFeaClsConnector = FeatureClassHelper.GetFeaClsInAccess(pFeaClsTurnArrow.FeatureDataset.Workspace.PathName,
                LaneConnector.ConnectorName);

            ArrowID = arrowID;
        }

        public IFeature GetFeature(int arrowId)
        {
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = ArrowIDNm + " = " + arrowId.ToString();
            IFeatureCursor cursor = FeaClsTurnArrow.Search(filter, false);
            return cursor.NextFeature();
        }

        public TurnArrow GetEntity(IFeature pFeature)
        {
            TurnArrow bounEty = new TurnArrow();
            if (pFeature != null)
            {
                if (FeaClsTurnArrow.FindField(ArrowIDNm) > 0)
                    bounEty.ArrowID = Convert.ToInt32(pFeature.get_Value(FeaClsTurnArrow.FindField(ArrowIDNm)));
                if (FeaClsTurnArrow.FindField(Boundary.STYLEID_NAME) > 0)
                    bounEty.StyleID = Convert.ToInt32(pFeature.get_Value(FeaClsTurnArrow.FindField(Boundary.STYLEID_NAME)));
                if (FeaClsTurnArrow.FindField(ArrowTypeNm) > 0)
                    bounEty.ArrowType = Convert.ToInt32(pFeature.get_Value(FeaClsTurnArrow.FindField(ArrowTypeNm)));


                if (FeaClsTurnArrow.FindField(SerialNm) > 0)
                    bounEty.ArcID = Convert.ToInt32(pFeature.get_Value(FeaClsTurnArrow.FindField(SerialNm)));
                if (FeaClsTurnArrow.FindField(ANGLENm) > 0)
                    bounEty.ANGLE = Convert.ToDouble(pFeature.get_Value(FeaClsTurnArrow.FindField(ANGLENm)));
                if (FeaClsTurnArrow.FindField(ArcIDNm) > 0)
                    bounEty.ArcID = Convert.ToInt32(pFeature.get_Value(FeaClsTurnArrow.FindField(ArcIDNm)));



                if (FeaClsTurnArrow.FindField(LaneIDNm) > 0)
                    bounEty.LaneID = Convert.ToInt32(pFeature.get_Value(FeaClsTurnArrow.FindField(LaneIDNm)));
                if (FeaClsTurnArrow.FindField(PrecedeArrowsNm) > 0)
                    bounEty.PrecedeArrows = Convert.ToString(pFeature.get_Value(FeaClsTurnArrow.FindField(PrecedeArrowsNm)));
                if (FeaClsTurnArrow.FindField(OtherNm) > 0)
                    bounEty.Other = Convert.ToInt32(pFeature.get_Value(FeaClsTurnArrow.FindField(OtherNm)));
            }
            return bounEty;
        }

        public IFeature CreateArrow(TurnArrow turnArrowEty, IPoint pnt)
        {
            IFeature arrowFeature = FeaClsTurnArrow.CreateFeature();
            

            if (turnArrowEty.ArrowID > 0)
            {
                if (FeaClsTurnArrow.FindField(ArrowIDNm) >= 0)
                    arrowFeature.set_Value(FeaClsTurnArrow.FindField(ArrowIDNm), turnArrowEty.ArrowID);
            }
            else
            {
                if (FeaClsTurnArrow.FindField(ArrowIDNm) >= 0)
                    arrowFeature.set_Value(FeaClsTurnArrow.FindField(ArrowIDNm), arrowFeature.OID);
            }

            if (FeaClsTurnArrow.FindField(Boundary.STYLEID_NAME) >= 0)
                arrowFeature.set_Value(FeaClsTurnArrow.FindField(Boundary.STYLEID_NAME), turnArrowEty.StyleID);
            if (FeaClsTurnArrow.FindField(ArrowTypeNm) >= 0)
                arrowFeature.set_Value(FeaClsTurnArrow.FindField(ArrowTypeNm), turnArrowEty.ArrowType);



            if (FeaClsTurnArrow.FindField(SerialNm) >= 0)
                arrowFeature.set_Value(FeaClsTurnArrow.FindField(SerialNm), turnArrowEty.Serial);
            if (FeaClsTurnArrow.FindField(ANGLENm) >= 0)
                arrowFeature.set_Value(FeaClsTurnArrow.FindField(ANGLENm), turnArrowEty.ANGLE);
            if (FeaClsTurnArrow.FindField(ArcIDNm) >= 0)
                arrowFeature.set_Value(FeaClsTurnArrow.FindField(ArcIDNm), turnArrowEty.ArcID);


            if (FeaClsTurnArrow.FindField(LaneIDNm) >= 0)
                arrowFeature.set_Value(FeaClsTurnArrow.FindField(LaneIDNm), turnArrowEty.LaneID);
            if (FeaClsTurnArrow.FindField(PrecedeArrowsNm) >= 0)
                arrowFeature.set_Value(FeaClsTurnArrow.FindField(PrecedeArrowsNm), turnArrowEty.PrecedeArrows);
            if (FeaClsTurnArrow.FindField(ANGLENm) >= 0)
                arrowFeature.set_Value(FeaClsTurnArrow.FindField(ANGLENm), turnArrowEty.ANGLE);

            arrowFeature.Shape = pnt;
            arrowFeature.Store();

            return arrowFeature;
        }


        public int GetArrowStyle(IFeatureClass pFeaClsConnector,int fromLaneID)
        {
            bool leftFlag = false;
            bool rightFlag = false;
            bool straightFlag = false;
            bool uturnFlag = false;
            IFeatureCursor cursor;
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = LaneConnectorFeatureService.fromLaneIDNm + " = " + fromLaneID.ToString();
            cursor = pFeaClsConnector.Search(filter, false);
            IFeature pFeature = cursor.NextFeature();
            while (pFeature != null)
            {
                string turnDir = Convert.ToString(pFeature.get_Value(pFeaClsConnector.FindField(LaneConnectorFeatureService.TurningDirNm)));
                //直行
                object o = 0;
                if (turnDir.Equals(System.Enum.GetName(typeof(RoadNetworkSystem.DataModel.LaneBasedNetwork.LaneConnector.转向), o)))
                {
                    straightFlag = true;
                }
                o=1;
                if (turnDir.Equals(System.Enum.GetName(typeof(RoadNetworkSystem.DataModel.LaneBasedNetwork.LaneConnector.转向), o)))
                {
                    leftFlag = true;
                }

                o = 2;
                if (turnDir.Equals(System.Enum.GetName(typeof(RoadNetworkSystem.DataModel.LaneBasedNetwork.LaneConnector.转向), o)))
                {
                    rightFlag = true;
                }

                o = 3;
                if (turnDir.Equals(System.Enum.GetName(typeof(RoadNetworkSystem.DataModel.LaneBasedNetwork.LaneConnector.转向), o)))
                {
                    uturnFlag = true;
                }
                pFeature = cursor.NextFeature();
            }

            return getStyleIdByTurningDir(leftFlag, rightFlag, straightFlag, uturnFlag);
        }

        public int GetArrowStyleByDir(List<string> turnDirs)
        {
            bool leftFlag = false;
            bool rightFlag = false;
            bool straightFlag = false;
            bool uturnFlag = false;
            for (int i = 0; i < turnDirs.Count; i++)
            {
                string turnDir = turnDirs[i];
                //直行
                object o = 0;
                if (turnDir.Equals(System.Enum.GetName(typeof(RoadNetworkSystem.DataModel.LaneBasedNetwork.LaneConnector.转向), o)))
                {
                    straightFlag = true;
                }
                o = 1;
                if (turnDir.Equals(System.Enum.GetName(typeof(RoadNetworkSystem.DataModel.LaneBasedNetwork.LaneConnector.转向), o)))
                {
                    leftFlag = true;
                }

                o = 2;
                if (turnDir.Equals(System.Enum.GetName(typeof(RoadNetworkSystem.DataModel.LaneBasedNetwork.LaneConnector.转向), o)))
                {
                    rightFlag = true;
                }

                o = 3;
                if (turnDir.Equals(System.Enum.GetName(typeof(RoadNetworkSystem.DataModel.LaneBasedNetwork.LaneConnector.转向), o)))
                {
                    uturnFlag = true;
                }
            }

            int NewStyleID;

            if (uturnFlag == true && straightFlag == true) NewStyleID = uturnStraStyle;
            else if (uturnFlag == true && leftFlag == true) NewStyleID = uturnLeftStyle;
            else if (uturnFlag == true) NewStyleID = uturnStyle;
            else if (leftFlag == true && rightFlag == true) NewStyleID = leftRightStyle;
            else if (straightFlag == true && rightFlag == true) NewStyleID = straRightStyle;
            else if (rightFlag == true) NewStyleID = rightStyle;
            else if (straightFlag == true && leftFlag == true) NewStyleID = straLeftStyle;
            else if (leftFlag == true) NewStyleID = leftStyle;
            else if (straightFlag == true) NewStyleID = straightStyle;
            else NewStyleID = 0;

            return NewStyleID;

        }

        /// <summary>
        /// 生成交叉口出口Arc段的Lane上的
        /// </summary>
        /// <param name="pFeaClsLane"></param>
        /// <param name="arcID"></param>
        public void CreateExitArcArrow(IFeatureClass pFeaClsLane, int arcID)
        {
            IFeatureCursor cursor;
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = Lane.ArcIDNm + " = " + arcID;
            cursor = pFeaClsLane.Search(filter, false);
            IFeature pFeature = cursor.NextFeature();

            #region ----------------------------计算导向箭头的偏转角-----------------------------------

            IPolyline laneLine = new PolylineClass();
            double angle = 0;
            if (pFeature != null)
            {
                laneLine = pFeature.ShapeCopy as IPolyline;
                
                double dotaX = laneLine.ToPoint.X - laneLine.FromPoint.X;
                double dotaY = laneLine.ToPoint.Y - laneLine.FromPoint.Y;
                
                if (dotaY == 0 && dotaX > 0)
                {
                    angle = 90;
                }
                else if (dotaY == 0 && dotaX < 0)
                {
                    angle = 270;
                }
                else
                {
                    double tanValue = dotaX / dotaY;
                    angle = (Math.Atan(tanValue) * 180 / Math.PI + 360) % 360;
                    if (dotaY < 0)
                    {
                        angle = angle + 180;
                    }
                    if (dotaY < 0)
                    {
 
                    }
                }
            }

            #endregion ----------------------------计算导向箭头的偏转角-----------------------------------

            while (pFeature != null)
            {

                //删掉车道上已经有的
                //删掉车道内部的导向箭头
                IFeatureCursor cursorArrow;
                IQueryFilter filterArrow = new QueryFilterClass();
                filterArrow.WhereClause = TurnArrowService.LaneIDNm + " = " +
                    Convert.ToInt32(pFeature.get_Value(pFeaClsLane.FindField(Lane.LaneIDNm)));
                cursorArrow = FeaClsTurnArrow.Search(filterArrow, false);
                IFeature feaArrow = cursorArrow.NextFeature();
                while (feaArrow != null)
                {
                    feaArrow.Delete();
                    feaArrow = cursorArrow.NextFeature();
                }

                //获取导向箭头的位置
                laneLine = pFeature.ShapeCopy as IPolyline;
                IPoint arrowPnt = LineHelper.CreateLine(laneLine, ARROWPOSITION, 0).FromPoint;

                TurnArrow arrowEty = new TurnArrow();
                arrowEty.ArcID = ArrowID;
                arrowEty.ANGLE = angle;
                arrowEty.ArrowType = 0;
                arrowEty.LaneID = Convert.ToInt32(pFeature.get_Value(pFeaClsLane.FindField(Lane.LaneIDNm)));
                arrowEty.Other = 0;
                arrowEty.PrecedeArrows = "";
                arrowEty.Serial = 0;
                arrowEty.StyleID = straightStyle;

                CreateArrow(arrowEty, arrowPnt);
                pFeature = cursor.NextFeature();
            }
        }


        /// <summary>
        /// 为Arc 批量生成导向箭头
        /// </summary>
        /// <param name="arcID"></param>
        public void CreateEntranceArcArrow(int arcID)
        {
            IFeatureCursor cursor;
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = Lane.ArcIDNm + " = " + arcID;
            cursor = pFeaClsLane.Search(filter, false);
            IFeature pFeature = cursor.NextFeature();
            while (pFeature != null)
            {
                CreateTurnArrowForEntranceLane(pFeature);
                pFeature = cursor.NextFeature();
            }
        }

        /// <summary>
        /// 为车道创建导向箭头
        /// </summary>
        /// <param name="laneFeature"></param>
        public void CreateTurnArrowForEntranceLane(IFeature laneFeature)
        {

            double angle = GetTurnArrowAngle(laneFeature);


            //删掉车道上已经有的
            //删掉车道内部的导向箭头
            IFeatureCursor cursorArrow;
            IQueryFilter filterArrow = new QueryFilterClass();
            filterArrow.WhereClause = TurnArrowService.LaneIDNm + " = " + 
                Convert.ToInt32(laneFeature.get_Value(laneFeature.Fields.FindField(Lane.LaneIDNm)));
            cursorArrow = FeaClsTurnArrow.Search(filterArrow, false);
            IFeature feaArrow = cursorArrow.NextFeature();
            while (feaArrow != null)
            {
                feaArrow.Delete();
                feaArrow = cursorArrow.NextFeature();
            }

            //获取导向箭头的位置
            IPolyline laneLine = laneFeature.ShapeCopy as IPolyline;
            #region +++++++++++++++++++++++++++++++++先生成Lane起始处的导向箭头+++++++++++++++++++++++++++++++++
            if (ARROWPOSITION > laneLine.Length)
            {
                ARROWPOSITION = 0.3 * laneLine.Length;
            }
            IPoint arrowPnt1 = LineHelper.CreateLine(laneLine, ARROWPOSITION, 0).FromPoint;

            LaneFeatureService lane = new LaneFeatureService(pFeaClsLane, 0);
            Lane laneEty = lane.GetEntity(laneFeature);
            int laneID = laneEty.LaneID;

            //递归获取所有的前方转向
            List<string> turnDirs = LogicalConnection.GetLaneLeadTurnDir(pFeaClsNode, pFeaClsLink,
                pFeaClsArc, pFeaClsConnector, laneEty);


            int preArrowID = 0;
            TurnArrow arrowEty = new TurnArrow();
            arrowEty.ArcID = laneEty.ArcID;
            arrowEty.ANGLE = angle;
            arrowEty.ArrowType = TurnArrow.ARROW_TYPE_GENERAL_DIRECTION;
            arrowEty.LaneID = laneID;
            arrowEty.Other = 0;
            arrowEty.PrecedeArrows = preArrowID.ToString(); ;
            arrowEty.Serial = 1;
            arrowEty.StyleID = GetArrowStyleByDir(turnDirs);

            IFeature arrowFea = CreateArrow(arrowEty, arrowPnt1);
            preArrowID = Convert.ToInt32(arrowFea.get_Value(FeaClsTurnArrow.FindField(ArrowIDNm)));

            #endregion +++++++++++++++++++++++++++++++++先生成Lane起始处的导向箭头+++++++++++++++++++++++++++++++++

            #region +++++++++++++++++++++++++++++++++ 生成Lane末端的导向箭头 +++++++++++++++++++++++++++++++++

            int arrowCountInEnd = ARROW_COUNT_IN_SHORT_LANE - 1;
            if (laneLine.Length > LENGHT_LONG_OR_SHORT)
            {
                arrowCountInEnd = ARROW_COUNT_IN_LONG_LANE - 1;
            }

            for (int i = 1; i <= arrowCountInEnd; i++)
            {
                int toPosition = Convert.ToInt32(ARROWPOSITION + (laneLine.Length - 2 * ARROWPOSITION) * i / arrowCountInEnd);
                IPoint arrowPnt2 = LineHelper.CreateLine(laneLine, 0, toPosition).ToPoint;

                arrowEty = new TurnArrow();
                arrowEty.ArcID = ArrowID;
                arrowEty.ANGLE = angle;
                arrowEty.ArrowType = TurnArrow.ARROW_TYPE_REAL_DIRECTION;
                arrowEty.LaneID = laneID;
                arrowEty.Other = 0;
                arrowEty.PrecedeArrows = preArrowID.ToString();
                arrowEty.Serial = 0;
                arrowEty.StyleID = GetArrowStyleByDir(turnDirs);
                arrowFea = CreateArrow(arrowEty, arrowPnt2);

                preArrowID = Convert.ToInt32(arrowFea.get_Value(FeaClsTurnArrow.FindField(ArrowIDNm)));
            }
            
            update
            #endregion +++++++++++++++++++++++++++++++++ 生成Lane末端的导向箭头 +++++++++++++++++++++++++++++++++

        }

        /// <summary>
        /// 获取导向箭头的角度
        /// </summary>
        /// <param name="pFeature"></param>
        /// <returns></returns>
        public int GetTurnArrowAngle(IFeature pFeature)
        {
            IPolyline laneLine = new PolylineClass();
            double angle = 0;
            if (pFeature != null)
            {
                laneLine = pFeature.ShapeCopy as IPolyline;

                double dotaX = laneLine.ToPoint.X - laneLine.FromPoint.X;
                double dotaY = laneLine.ToPoint.Y - laneLine.FromPoint.Y;

                if (dotaY == 0 && dotaX > 0)
                {
                    angle = 90;
                }
                else if (dotaY == 0 && dotaX < 0)
                {
                    angle = 270;
                }
                else
                {
                    double tanValue = dotaX / dotaY;
                    angle = (Math.Atan(tanValue) * 180 / Math.PI + 360) % 360;
                    if (dotaY < 0)
                    {
                        angle = angle + 180;
                    }
                }
            }

            return Convert.ToInt32(angle);
        }
        
        /// <summary>
        /// 更新某个车道的导向箭头
        /// </summary>
        /// <param name="laneId"></param>
        /// <param name="styleId"></param>
        public void UpdateTurnArrowInLane(int laneId, int styleId)
        {
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = LaneIDNm + "=" + laneId;
            IFeatureCursor cursor = FeaClsTurnArrow.Search(filter, false);
            IFeature pFeature = cursor.NextFeature();

            if (null == pFeature)
            {
                LaneFeatureService laneService = new LaneFeatureService(pFeaClsLane, laneId);
                CreateTurnArrowForEntranceLane(laneService.GetFeature());
            }

            //记录车道最上游的导向箭头
            IFeature preArrrow = pFeature;
            while (pFeature != null)
            {
                TurnArrow temTurnArrow = GetEntity(pFeature);
                if (temTurnArrow.ArrowType == TurnArrow.ARROW_TYPE_GENERAL_DIRECTION)
                {
                    break;
                }
                pFeature.set_Value(Convert.ToInt32(pFeature.Fields.FindField(STYLEID_NAME)),
                    styleId);
                pFeature.Store();
                preArrrow = pFeature;
                pFeature = cursor.NextFeature();
            }

            //更新上游导向箭头
            updateUpstreamArrow(preArrrow);
            return;
        }

        
        /// <summary>
        /// 根据下游连接的导向箭头的样式，得到当前导向箭头的样式
        /// </summary>
        /// <param name="arrowsFeature"></param>
        /// <returns></returns>
        private int getTurnArrowStyleByNextArrows(List<IFeature> arrowsFeature)
        {
            if (null == arrowsFeature||
                0 == arrowsFeature.Count)
            {
                return 0;
            }
            List<int> nextStyleIds = new List<int>();
            foreach (IFeature temFeature in arrowsFeature)
            {
                int temArrowId = Convert.ToInt32(temFeature.get_Value(temFeature.Fields.FindField(STYLEID_NAME)));
                nextStyleIds.Add(temArrowId);
            }
            return getStyleIdByStyleIds(nextStyleIds);
        }

        /// <summary>
        /// 获取导向箭头下游的所有的 箭头
        /// </summary>
        /// <param name="arrowId"></param>
        /// <returns></returns>
        private List<IFeature> getNextArrows(int arrowId)
        {
            List<IFeature> arrows = new List<IFeature>();

            OleDbConnection connection = AccessHelper.OpenConnection(FeaClsTurnArrow.FeatureDataset.Workspace.PathName);
            string sql = "Select * from "+TurnArrow.TurnArrowName+
                " Where "+ PrecedeArrowsNm +" Like "+ arrowId;
            OleDbCommand cmd = new OleDbCommand(sql, connection);
            OleDbDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string precedeArrows = Convert.ToString(reader[PrecedeArrowsNm]);

                int objectID = Convert.ToInt32(reader["OBJECTID"]);
                
                if (precedeArrows.Contains(arrowId.ToString()))
                {
                    IFeature temArrowFeature =  FeaClsTurnArrow.GetFeature(objectID);
                    if(temArrowFeature == null)
                    {
                        continue;
                    }
                    arrows.Add(temArrowFeature);
                }
            }
            reader.Close();
            connection.Close();
            connection.Dispose();

            return arrows;
        }

        /// <summary>
        /// 更新车道上游那个车道的导向箭头
        /// </summary>
        /// <param name="firstArrowInNextLane"></param>
        private void updateUpstreamArrow(IFeature firstArrowInNextLane)
        {
            if (firstArrowInNextLane == null)
            {
                return;
            }
            TurnArrow arrow = GetEntity(firstArrowInNextLane);
            if (arrow == null)
            {
                return;
            }

            if (arrow.PrecedeArrows == "")
            {
                return;
            }

            string[] precedeArrowArray = arrow.PrecedeArrows.Split('\\');

            foreach (string temPrecedeArrowId in precedeArrowArray)
            {
                int id = Convert.ToInt32(temPrecedeArrowId);
                if (id <= 0)
                {
                    continue;
                }

                TurnArrowService temTurnArrowService = new TurnArrowService(FeaClsTurnArrow, id);
                IFeature pFeature = temTurnArrowService.GetFeature(id);
                TurnArrow temArrow = GetEntity(pFeature);

                List<IFeature> nextArrowsFeature = getNextArrows(id);

                if (null == nextArrowsFeature ||
                    0 == nextArrowsFeature.Count)
                {
                    continue;
                }

                //上游的样式,上游车道可能也有好几个导向箭头
                int temPrecedeStyle = getTurnArrowStyleByNextArrows(nextArrowsFeature);
                UpdateTurnArrowInLane(temArrow.LaneID, temPrecedeStyle);
            }
        }


        private int getStyleIdByTurningDir(bool leftFlag,
            bool rightFlag,
            bool straightFlag,
            bool uturnFlag)
        {
            int NewStyleID = 0;
            if (uturnFlag == true && straightFlag == true) NewStyleID = uturnStraStyle;
            else if (uturnFlag == true && leftFlag == true) NewStyleID = uturnLeftStyle;
            else if (uturnFlag == true) NewStyleID = uturnStyle;
            else if (leftFlag == true && rightFlag == true) NewStyleID = leftRightStyle;
            else if (straightFlag == true && rightFlag == true) NewStyleID = straRightStyle;
            else if (rightFlag == true) NewStyleID = rightStyle;
            else if (straightFlag == true && leftFlag == true) NewStyleID = straLeftStyle;
            else if (leftFlag == true) NewStyleID = leftStyle;
            else if (straightFlag == true) NewStyleID = straightStyle;
            else NewStyleID = 0;
            return NewStyleID;
        }

        private int getStyleIdByStyleIds(List<int> styleIds)
        {
            bool leftFlag = false;
            bool rightFlag = false;
            bool straightFlag = false;
            bool uturnFlag = false;

            if (styleIds.Contains(uturnLeftStyle)) { uturnFlag = true; }
            if (styleIds.Contains(leftStyle)) { leftFlag = true; }
            if (styleIds.Contains(rightStyle)) { rightFlag = true; }
            if (styleIds.Contains(straightStyle)) { straightFlag = true; }
            if (styleIds.Contains(leftRightStyle)) { leftFlag = true; rightFlag = true; }
            if (styleIds.Contains(straLeftStyle)) { leftFlag = true; straightFlag = true; }
            if (styleIds.Contains(uturnLeftStyle)) { leftFlag = true; uturnFlag = true; }
            if (styleIds.Contains(straRightStyle)) { straightFlag = true;rightFlag = true; }
            if (styleIds.Contains(uturnStraStyle)) { straightFlag = true;uturnFlag = true; }

            return getStyleIdByTurningDir(leftFlag,
                rightFlag,
                straightFlag,
                uturnFlag);
        }

    }
}
