using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.RoadSign;
using RoadNetworkSystem.GIS;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.Connection;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LaneLayer;
using System;
using System.Collections.Generic;

namespace RoadNetworkSystem.NetworkElement.RoadSignElement
{
    class TurnArrow
    {
        private double ARROWPOSITION = 8;
        public const string ArrowIDNm = "ArrowID";
        public const string StyleIDNm = "StyleID";
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
        public int ArrowID;
        public TurnArrow(IFeatureClass pFeaClsTurnArrow, int arrowID)
        {
            FeaClsTurnArrow = pFeaClsTurnArrow;
            ArrowID = arrowID;
        }

        public TurnArrowEntity GetEntity(IFeature pFeature)
        {
            TurnArrowEntity bounEty = new TurnArrowEntity();
            if (pFeature != null)
            {
                if (FeaClsTurnArrow.FindField(ArrowIDNm) > 0)
                    bounEty.ArrowID = Convert.ToInt32(pFeature.get_Value(FeaClsTurnArrow.FindField(ArrowIDNm)));
                if (FeaClsTurnArrow.FindField(StyleIDNm) > 0)
                    bounEty.StyleID = Convert.ToInt32(pFeature.get_Value(FeaClsTurnArrow.FindField(StyleIDNm)));
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

        public IFeature CreateArrow(TurnArrowEntity turnArrowEty, IPoint pnt)
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

            if (FeaClsTurnArrow.FindField(StyleIDNm) >= 0)
                arrowFeature.set_Value(FeaClsTurnArrow.FindField(StyleIDNm), turnArrowEty.StyleID);
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
            filter.WhereClause = LaneConnectorFeature.fromLaneIDNm + " = " + fromLaneID.ToString();
            cursor = pFeaClsConnector.Search(filter, false);
            IFeature pFeature = cursor.NextFeature();
            while (pFeature != null)
            {
                string turnDir = Convert.ToString(pFeature.get_Value(pFeaClsConnector.FindField(LaneConnectorFeature.TurningDirNm)));
                //直行
                object o = 0;
                if (turnDir.Equals(System.Enum.GetName(typeof(RoadNetworkSystem.DataModel.LaneBasedNetwork.LaneConnectorEntity.转向), o)))
                {
                    straightFlag = true;
                }
                o=1;
                if (turnDir.Equals(System.Enum.GetName(typeof(RoadNetworkSystem.DataModel.LaneBasedNetwork.LaneConnectorEntity.转向), o)))
                {
                    leftFlag = true;
                }

                o = 2;
                if (turnDir.Equals(System.Enum.GetName(typeof(RoadNetworkSystem.DataModel.LaneBasedNetwork.LaneConnectorEntity.转向), o)))
                {
                    rightFlag = true;
                }

                o = 3;
                if (turnDir.Equals(System.Enum.GetName(typeof(RoadNetworkSystem.DataModel.LaneBasedNetwork.LaneConnectorEntity.转向), o)))
                {
                    uturnFlag = true;
                }
                pFeature = cursor.NextFeature();
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
                if (turnDir.Equals(System.Enum.GetName(typeof(RoadNetworkSystem.DataModel.LaneBasedNetwork.LaneConnectorEntity.转向), o)))
                {
                    straightFlag = true;
                }
                o = 1;
                if (turnDir.Equals(System.Enum.GetName(typeof(RoadNetworkSystem.DataModel.LaneBasedNetwork.LaneConnectorEntity.转向), o)))
                {
                    leftFlag = true;
                }

                o = 2;
                if (turnDir.Equals(System.Enum.GetName(typeof(RoadNetworkSystem.DataModel.LaneBasedNetwork.LaneConnectorEntity.转向), o)))
                {
                    rightFlag = true;
                }

                o = 3;
                if (turnDir.Equals(System.Enum.GetName(typeof(RoadNetworkSystem.DataModel.LaneBasedNetwork.LaneConnectorEntity.转向), o)))
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
        public void createExitArcArrow(IFeatureClass pFeaClsLane, int arcID)
        {
            IFeatureCursor cursor;
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = LaneFeature.ArcIDNm + " = " + arcID;
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
                filterArrow.WhereClause = TurnArrow.LaneIDNm + " = " + Convert.ToInt32(pFeature.get_Value(pFeaClsLane.FindField(LaneFeature.LaneIDNm)));
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

                TurnArrowEntity arrowEty = new TurnArrowEntity();
                arrowEty.ArcID = ArrowID;
                arrowEty.ANGLE = angle;
                arrowEty.ArrowType = 0;
                arrowEty.LaneID = Convert.ToInt32(pFeature.get_Value(pFeaClsLane.FindField(LaneFeature.LaneIDNm)));
                arrowEty.Other = 0;
                arrowEty.PrecedeArrows = "";
                arrowEty.Serial = 0;
                arrowEty.StyleID = straightStyle;

                CreateArrow(arrowEty, arrowPnt);
                pFeature = cursor.NextFeature();
            }
        }



        public void createEntranceArcArrow(IFeatureClass pFeaClsNode,IFeatureClass pFeaClsLink,
            IFeatureClass pFeaClsArc,IFeatureClass pFeaClsLane,IFeatureClass pFeaClsConnector, int arcID)
        {
            IFeatureCursor cursor;
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = LaneFeature.ArcIDNm + " = " + arcID;
            cursor = pFeaClsLane.Search(filter, false);
            IFeature pFeature = cursor.NextFeature();
            int preArrowID = 0;

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
                }
            }
            #endregion ----------------------------计算导向箭头的偏转角-----------------------------------

            while (pFeature != null)
            {
                //删掉车道上已经有的
                //删掉车道内部的导向箭头
                IFeatureCursor cursorArrow;
                IQueryFilter filterArrow = new QueryFilterClass();
                filterArrow.WhereClause = TurnArrow.LaneIDNm + " = " + Convert.ToInt32(pFeature.get_Value(pFeaClsLane.FindField(LaneFeature.LaneIDNm)));
                cursorArrow = FeaClsTurnArrow.Search(filterArrow, false);
                IFeature feaArrow = cursorArrow.NextFeature();
                while (feaArrow != null)
                {
                    feaArrow.Delete();
                    feaArrow = cursorArrow.NextFeature();
                }

                //获取导向箭头的位置
                laneLine = pFeature.ShapeCopy as IPolyline;
                #region +++++++++++++++++++++++++++++++++先生成Lane起始处的导向箭头+++++++++++++++++++++++++++++++++
                if (ARROWPOSITION > laneLine.Length)
                {
                    ARROWPOSITION = 0.3 * laneLine.Length;
                }
                IPoint arrowPnt1 = LineHelper.CreateLine(laneLine, ARROWPOSITION, 0).FromPoint;

                LaneFeature lane = new LaneFeature(pFeaClsLane, 0);
                LaneEntity laneEty = lane.GetEntity(pFeature);
                int laneID = laneEty.LaneID;

                //递归获取所有的前方转向
                List<string> turnDirs = LogicalConnection.GetLaneLeadNodeTurnDir(pFeaClsNode, pFeaClsLink,
                    pFeaClsArc, pFeaClsConnector, laneEty);


                TurnArrowEntity arrowEty = new TurnArrowEntity();
                arrowEty.ArcID = ArrowID;
                arrowEty.ANGLE = angle;
                arrowEty.ArrowType = 0;
                arrowEty.LaneID = laneID;
                arrowEty.Other = 0;
                arrowEty.PrecedeArrows = preArrowID.ToString(); ;
                arrowEty.Serial = 1;

                arrowEty.StyleID = GetArrowStyleByDir(turnDirs);

                IFeature arrowFea = CreateArrow(arrowEty, arrowPnt1);

                preArrowID = Convert.ToInt32(arrowFea.get_Value(FeaClsTurnArrow.FindField(ArrowIDNm)));

                #endregion +++++++++++++++++++++++++++++++++先生成Lane起始处的导向箭头+++++++++++++++++++++++++++++++++


                #region +++++++++++++++++++++++++++++++++先生成Lane起始处的导向箭头+++++++++++++++++++++++++++++++++
                if (ARROWPOSITION > laneLine.Length)
                {
                    ARROWPOSITION = 0.3 * laneLine.Length;
                }
                IPoint arrowPnt2 = LineHelper.CreateLine(laneLine, 0, ARROWPOSITION).ToPoint;

                arrowEty = new TurnArrowEntity();
                arrowEty.ArcID = ArrowID;
                arrowEty.ANGLE = angle;
                arrowEty.ArrowType = 0;
                arrowEty.LaneID = laneID;
                arrowEty.Other = 0;
                arrowEty.PrecedeArrows = "";
                arrowEty.Serial = 0;

                arrowEty.StyleID = GetArrowStyleByDir(turnDirs);

                CreateArrow(arrowEty, arrowPnt2);

                #endregion +++++++++++++++++++++++++++++++++先生成Lane起始处的导向箭头+++++++++++++++++++++++++++++++++

                pFeature = cursor.NextFeature();
            }
        }



    }
}
