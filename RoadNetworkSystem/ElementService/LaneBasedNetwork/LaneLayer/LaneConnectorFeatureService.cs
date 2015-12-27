using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.Road;
using RoadNetworkSystem.GIS;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.Connection;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LinkLayer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LaneLayer
{
    class LaneConnectorFeatureService
    {
          /// <summary>
        /// 规定了数据模型，请不要在其他类中直接读取数据
        /// </summary>
        public const string ConnectorIDNm = "ConnectorID";
        public const string fromLaneIDNm = "fromLaneID";
        public const string toLaneIDNm = "toLaneID";

        public const string TurningDirNm = "TurningDir";
        public const string fromArcIDNm = "fromArcID";
        public const string toArcIDNm = "toArcID";


        public const string fromLinkIDNm = "fromLinkID";
        public const string fromDirNm = "fromDir";
        public const string toLinkIDNm = "toLinkID";

        public const string toDirNm = "toDir";
        public const string OtherNm = "Other";

        private int _connectorID = 0;
        private IFeatureClass _pFeaClsConnector;

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="pFeaClsConnector"></param>ITable 
        /// <param name="connectorID"></param>
        public LaneConnectorFeatureService(IFeatureClass pFeaClsConnector, int connectorID)
        {
            _pFeaClsConnector = pFeaClsConnector;
            _connectorID = connectorID;
        }

        /// <summary>
        /// 获取车道连接器
        /// </summary>
        /// <returns></returns>车道连接器实体
        public LaneConnector GetLaneConnEty(IFeature targetFea)
        {
            LaneConnector targetLaneEty = new LaneConnector();
           
            if (targetFea != null)
            {
                targetLaneEty.ConnectorID = _connectorID;
                targetLaneEty.fromArcID = Convert.ToInt32(targetFea.get_Value(_pFeaClsConnector.FindField(fromArcIDNm)));
                targetLaneEty.fromDir = Convert.ToInt32(targetFea.get_Value(_pFeaClsConnector.FindField(fromDirNm)));
                targetLaneEty.fromLaneID = Convert.ToInt32(targetFea.get_Value(_pFeaClsConnector.FindField(fromLaneIDNm)));
                targetLaneEty.fromLinkID = Convert.ToInt32(targetFea.get_Value(_pFeaClsConnector.FindField(fromLinkIDNm)));
                targetLaneEty.toArcID = Convert.ToInt32(targetFea.get_Value(_pFeaClsConnector.FindField(toArcIDNm)));
                targetLaneEty.toDir = Convert.ToInt32(targetFea.get_Value(_pFeaClsConnector.FindField(toDirNm)));
                targetLaneEty.toLaneID = Convert.ToInt32(targetFea.get_Value(_pFeaClsConnector.FindField(toLaneIDNm)));
                targetLaneEty.toLinkID = Convert.ToInt32(targetFea.get_Value(_pFeaClsConnector.FindField(toLinkIDNm)));
                targetLaneEty.TurningDir = Convert.ToString(targetFea.get_Value(_pFeaClsConnector.FindField(TurningDirNm)));
            }
            else
            {
                targetLaneEty = null;
            }
            return targetLaneEty;
        }

        /// <summary>
        /// 获取车道连接器
        /// </summary>
        /// <returns></returns>车道连接器实体
        public LaneConnector GetLaneConnEty(int connectorID)
        {
            LaneConnector targetLaneEty = new LaneConnector();
            IQueryFilter queryFilter = new QueryFilterClass();
            queryFilter.WhereClause = String.Format("ConnectorID ={0}", connectorID);
            IFeatureCursor cursor = _pFeaClsConnector.Search(queryFilter, false);
            IFeature targetFea = cursor.NextFeature();
            if (targetFea != null)
            {
                targetLaneEty.ConnectorID = _connectorID;
                targetLaneEty.fromArcID = Convert.ToInt32(targetFea.get_Value(_pFeaClsConnector.FindField(fromArcIDNm)));
                targetLaneEty.fromDir = Convert.ToInt32(targetFea.get_Value(_pFeaClsConnector.FindField(fromDirNm)));
                targetLaneEty.fromLaneID = Convert.ToInt32(targetFea.get_Value(_pFeaClsConnector.FindField(fromLaneIDNm)));
                targetLaneEty.fromLinkID = Convert.ToInt32(targetFea.get_Value(_pFeaClsConnector.FindField(fromLinkIDNm)));
                targetLaneEty.toArcID = Convert.ToInt32(targetFea.get_Value(_pFeaClsConnector.FindField(toArcIDNm)));
                targetLaneEty.toDir = Convert.ToInt32(targetFea.get_Value(_pFeaClsConnector.FindField(toDirNm)));
                targetLaneEty.toLaneID = Convert.ToInt32(targetFea.get_Value(_pFeaClsConnector.FindField(toLaneIDNm)));
                targetLaneEty.toLinkID = Convert.ToInt32(targetFea.get_Value(_pFeaClsConnector.FindField(toLinkIDNm)));
                targetLaneEty.TurningDir = Convert.ToString(targetFea.get_Value(_pFeaClsConnector.FindField(TurningDirNm)));
            }
            else
            {
                targetLaneEty = null;
            }
            return targetLaneEty;
        }

        /// <summary>
        /// 插入一条车道连接器
        /// </summary>
        /// <param name="updateLaneConnEty"></param>
        public void InsertLConnector(LaneConnector updateLaneConnEty,IPolyline connLine)
        {
            //由fromLaneID和toLaneID判断车道连接器是否存在，存在=true，不存在=false
            bool exitConntectorFlag = false;
            exitConntectorFlag = IsExistedConnByLane(updateLaneConnEty.fromLaneID, updateLaneConnEty.toLaneID);
            //不存在才生成
            if (exitConntectorFlag == false)
            {

                IFeature newFeature = _pFeaClsConnector.CreateFeature();
                if (updateLaneConnEty.ConnectorID > 0)
                {

                    newFeature.set_Value(_pFeaClsConnector.FindField("ConnectorID"), updateLaneConnEty.ConnectorID);
                }
                else
                {
                    newFeature.set_Value(_pFeaClsConnector.FindField("ConnectorID"), newFeature.OID);
                }
                newFeature.set_Value(_pFeaClsConnector.FindField("fromArcID"), updateLaneConnEty.fromArcID);
                newFeature.set_Value(_pFeaClsConnector.FindField("fromDir"), updateLaneConnEty.fromDir);
                newFeature.set_Value(_pFeaClsConnector.FindField("fromLaneID"), updateLaneConnEty.fromLaneID);
                newFeature.set_Value(_pFeaClsConnector.FindField("fromLinkID"), updateLaneConnEty.fromLinkID);
                newFeature.set_Value(_pFeaClsConnector.FindField("toArcID"), updateLaneConnEty.toArcID);
                newFeature.set_Value(_pFeaClsConnector.FindField("toDir"), updateLaneConnEty.toDir);
                newFeature.set_Value(_pFeaClsConnector.FindField("toLinkID"), updateLaneConnEty.toLinkID);
                newFeature.set_Value(_pFeaClsConnector.FindField("toLaneID"), updateLaneConnEty.toLaneID);
                newFeature.set_Value(_pFeaClsConnector.FindField("TurningDir"), updateLaneConnEty.TurningDir);

                newFeature.Shape = connLine;
                newFeature.Store();

            }
        }

        /// <summary>
        /// 更新车道连接器
        /// </summary>
        /// <param name="updateLaneConnEty"></param>
        /// <param name="preLaneConnID"></param>
        public void UpdateConnector(LaneConnector updateLaneConnEty, int preLaneConnID)
        {
            LaneConnector preLaneConnEty = GetLaneConnEty(preLaneConnID);

         
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = String.Format("{0}={1}",ConnectorIDNm, preLaneConnEty.ConnectorID);
            IFeatureCursor pFeatureCuror1 = _pFeaClsConnector.Search(filter, false);
            IFeature modifiedFea = pFeatureCuror1.NextFeature();

            if (updateLaneConnEty.TurningDir != preLaneConnEty.TurningDir)
            {
                modifiedFea.set_Value(_pFeaClsConnector.FindField(TurningDirNm), updateLaneConnEty.TurningDir);
            }
            if (updateLaneConnEty.fromArcID != preLaneConnEty.fromArcID)
            {
                modifiedFea.set_Value(_pFeaClsConnector.FindField(fromArcIDNm), updateLaneConnEty.fromArcID);
            }
            if (updateLaneConnEty.fromDir != preLaneConnEty.fromDir)
            {
                modifiedFea.set_Value(_pFeaClsConnector.FindField(fromDirNm), updateLaneConnEty.fromDir);
            }
            if (updateLaneConnEty.fromLaneID != preLaneConnEty.fromLaneID)
            {
                modifiedFea.set_Value(_pFeaClsConnector.FindField(fromLaneIDNm), updateLaneConnEty.fromLaneID);
            }
            if (updateLaneConnEty.fromLinkID != preLaneConnEty.fromLinkID)
            {
                modifiedFea.set_Value(_pFeaClsConnector.FindField(fromLinkIDNm), updateLaneConnEty.fromLinkID);
            }
            if (updateLaneConnEty.toArcID != preLaneConnEty.toArcID)
            {
                modifiedFea.set_Value(_pFeaClsConnector.FindField(toArcIDNm), updateLaneConnEty.toArcID);
            }
            if (updateLaneConnEty.toDir != preLaneConnEty.toDir)
            {
                modifiedFea.set_Value(_pFeaClsConnector.FindField(toArcIDNm), updateLaneConnEty.toDir);
            }

            if (updateLaneConnEty.toLaneID != preLaneConnEty.toLaneID)
            {
                modifiedFea.set_Value(_pFeaClsConnector.FindField(toLaneIDNm), updateLaneConnEty.toLaneID);
            }
            if (updateLaneConnEty.toLinkID != preLaneConnEty.toLinkID)
            {
                modifiedFea.set_Value(_pFeaClsConnector.FindField(toLinkIDNm), updateLaneConnEty.toLinkID);
            }
            modifiedFea.Store();

        }

        /// <summary>
        /// 删除车道连接器
        /// </summary>
        /// <param name="connecttorID"></param>车道连接器的ID
        public void DeleteConnector(int connecttorID)
        {
         
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = String.Format("{0}={1}",ConnectorIDNm, connecttorID);
            IFeatureCursor cursor = _pFeaClsConnector.Update(filter, false);
            IFeature deleteRow = cursor.NextFeature();

            deleteRow.Delete();

        }


        public void CreateConnectorInNode(Node nodeEty, IFeatureClass pFeaClsLink, IFeatureClass pFeaClsArc)
        {
 
        }

        /// <summary>
        /// 默认规则生成两个Arc间的车道连接器
        /// </summary>
        /// <param name="pFeaClsLane"></param>
        /// <param name="fromArcEty"></param>
        /// <param name="toArcEty"></param>
        /// <param name="TurningDir"></param>
        /// <param name="nodePnt"></param>
        public void CreateConnectorInArcs(IFeatureClass pFeaClsLane,Arc fromArcEty, 
            Arc toArcEty, string TurningDir,IPoint nodePnt)
        {
            if (fromArcEty == null || toArcEty == null)
            {
                return;
            }
            else
            {

                int toLinkID = toArcEty.LinkID;
                int toArcID = toArcEty.ArcID;
                int toArcDir = toArcEty.FlowDir;
                int toArcLaneNum = toArcEty.LaneNum;
                int toLanePosition = -1;
                int toLaneID;
                IFeature toLaneFea = null;
                IPolyline toLine = null;

                int fromLinkID = fromArcEty.LinkID;
                int fromArcID = fromArcEty.ArcID;
                int fromArcDir = fromArcEty.FlowDir;
                int fromArcLaneNum = fromArcEty.LaneNum;
                int fromLanePostition = -1;
                int fromLaneID;
                IPolyline fromLine = null;
                IFeature fromLaneFea = null;

                IPolyline bezierLine = null;
                LaneFeatureService lane = new LaneFeatureService(pFeaClsLane, 0);
                switch (TurningDir)
                {


                    #region -----------------------------------Right-------------------------------------
                    case "Right":
                        {

                            if (fromArcLaneNum >= 5)
                            {
                                if (toArcLaneNum < 2)
                                {
                                    fromLanePostition = fromArcLaneNum - 1;
                                    toLanePosition = toArcLaneNum - 1;

                                    fromLaneFea = lane.QueryFeatureBuRule(fromArcID, fromLanePostition);
                                    fromLaneID = Convert.ToInt32(fromLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                    fromLine = fromLaneFea.ShapeCopy as IPolyline;

                                    toLaneFea = lane.QueryFeatureBuRule(toArcID, toLanePosition);
                                    toLaneID = Convert.ToInt32(toLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                    toLine = toLaneFea.ShapeCopy as IPolyline;
                                    bezierLine = GetConnectorShape(fromLine, toLine, null, false);

                                    LaneConnector connectorEntity1 = InitConnectorEty(0, fromLinkID, toLinkID, fromArcID, toArcID,
                                        fromArcDir, toArcDir, fromLaneID, toLaneID, TurningDir);

                                    InsertLConnector(connectorEntity1, bezierLine);

                                }
                                else
                                {
                                    fromLanePostition = fromArcLaneNum - 1;
                                    toLanePosition = toArcLaneNum - 1;

                                    fromLaneFea = lane.QueryFeatureBuRule(fromArcID, fromLanePostition);
                                    fromLaneID = Convert.ToInt32(fromLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                    fromLine = fromLaneFea.ShapeCopy as IPolyline;

                                    toLaneFea = lane.QueryFeatureBuRule(toArcID, toLanePosition);
                                    toLaneID = Convert.ToInt32(toLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                    toLine = toLaneFea.ShapeCopy as IPolyline;
                                    bezierLine = GetConnectorShape(fromLine, toLine, null, false);

                                    LaneConnector connectorEntity1 = InitConnectorEty(0, fromLinkID, toLinkID, fromArcID, toArcID,
                                        fromArcDir, toArcDir, fromLaneID, toLaneID, TurningDir);

                                    InsertLConnector(connectorEntity1, bezierLine);


                                    fromLanePostition = fromArcLaneNum - 2;
                                    toLanePosition = toArcLaneNum - 2;

                                    fromLaneFea = lane.QueryFeatureBuRule(fromArcID, fromLanePostition);
                                    fromLaneID = Convert.ToInt32(fromLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                    fromLine = fromLaneFea.ShapeCopy as IPolyline;

                                    toLaneFea = lane.QueryFeatureBuRule(toArcID, toLanePosition);
                                    toLaneID = Convert.ToInt32(toLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                    toLine = toLaneFea.ShapeCopy as IPolyline;
                                    bezierLine = GetConnectorShape(fromLine, toLine, null, false);

                                    LaneConnector connectorEntity2 = InitConnectorEty(0, fromLinkID, toLinkID, fromArcID, toArcID,
                                        fromArcDir, toArcDir, fromLaneID, toLaneID, TurningDir);

                                    InsertLConnector(connectorEntity2, bezierLine);

                                }
                            }
                            else
                            {
                                fromLanePostition = fromArcLaneNum - 1;
                                toLanePosition = toArcLaneNum - 1;

                                fromLaneFea = lane.QueryFeatureBuRule(fromArcID, fromLanePostition);
                                fromLaneID = Convert.ToInt32(fromLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                fromLine = fromLaneFea.ShapeCopy as IPolyline;

                                toLaneFea = lane.QueryFeatureBuRule(toArcID, toLanePosition);
                                toLaneID = Convert.ToInt32(toLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                toLine = toLaneFea.ShapeCopy as IPolyline;
                                bezierLine = GetConnectorShape(fromLine, toLine, null, false);

                                LaneConnector connectorEntity2 = InitConnectorEty(0, fromLinkID, toLinkID, fromArcID, toArcID,
                                    fromArcDir, toArcDir, fromLaneID, toLaneID, TurningDir);

                                InsertLConnector(connectorEntity2, bezierLine);

                            }

                        } break;
                    #endregion -----------------------------------Right-------------------------------------

                    #region -----------------------------------Left-------------------------------------
                    case "Left":
                        {

                            if (fromArcLaneNum >= 5)
                            {
                                if (toArcLaneNum >= 2)
                                {
                                    fromLanePostition = 0;
                                    toLanePosition = 0;

                                    fromLaneFea = lane.QueryFeatureBuRule(fromArcID, fromLanePostition);
                                    fromLaneID = Convert.ToInt32(fromLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                    fromLine = fromLaneFea.ShapeCopy as IPolyline;

                                    toLaneFea = lane.QueryFeatureBuRule(toArcID, toLanePosition);
                                    toLaneID = Convert.ToInt32(toLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                    toLine = toLaneFea.ShapeCopy as IPolyline;
                                    bezierLine = GetConnectorShape(fromLine, toLine,nodePnt,false);

                                    LaneConnector connectorEntity2 = InitConnectorEty(0, fromLinkID, toLinkID, fromArcID, toArcID,
                                        fromArcDir, toArcDir, fromLaneID, toLaneID, TurningDir);

                                    InsertLConnector(connectorEntity2, bezierLine);


                                    fromLanePostition = 1;
                                    toLanePosition = 1;

                                    fromLaneFea = lane.QueryFeatureBuRule(fromArcID, fromLanePostition);
                                    fromLaneID = Convert.ToInt32(fromLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                    fromLine = fromLaneFea.ShapeCopy as IPolyline;

                                    toLaneFea = lane.QueryFeatureBuRule(toArcID, toLanePosition);
                                    toLaneID = Convert.ToInt32(toLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                    toLine = toLaneFea.ShapeCopy as IPolyline;
                                    bezierLine = GetConnectorShape(fromLine, toLine, nodePnt, false);

                                    LaneConnector connectorEntity = InitConnectorEty(0, fromLinkID, toLinkID, fromArcID, toArcID,
                                        fromArcDir, toArcDir, fromLaneID, toLaneID, TurningDir);

                                    InsertLConnector(connectorEntity, bezierLine);

                                }
                                else
                                {
                                    fromLanePostition = 0;
                                    toLanePosition = 0;

                                    fromLaneFea = lane.QueryFeatureBuRule(fromArcID, fromLanePostition);
                                    fromLaneID = Convert.ToInt32(fromLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                    fromLine = fromLaneFea.ShapeCopy as IPolyline;

                                    toLaneFea = lane.QueryFeatureBuRule(toArcID, toLanePosition);
                                    toLaneID = Convert.ToInt32(toLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                    toLine = toLaneFea.ShapeCopy as IPolyline;
                                    bezierLine = GetConnectorShape(fromLine, toLine, nodePnt, false);

                                    LaneConnector connectorEntity = InitConnectorEty(0, fromLinkID, toLinkID, fromArcID, toArcID,
                                        fromArcDir, toArcDir, fromLaneID, toLaneID, TurningDir);

                                    InsertLConnector(connectorEntity, bezierLine);
                                }
                            }
                            else
                            {
                                fromLanePostition = 0;
                                toLanePosition = 0;

                                fromLaneFea = lane.QueryFeatureBuRule(fromArcID, fromLanePostition);
                                fromLaneID = Convert.ToInt32(fromLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                fromLine = fromLaneFea.ShapeCopy as IPolyline;

                                toLaneFea = lane.QueryFeatureBuRule(toArcID, toLanePosition);
                                toLaneID = Convert.ToInt32(toLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                toLine = toLaneFea.ShapeCopy as IPolyline;
                                bezierLine = GetConnectorShape(fromLine, toLine, nodePnt, false);

                                LaneConnector connectorEntity = InitConnectorEty(0, fromLinkID, toLinkID, fromArcID, toArcID,
                                    fromArcDir, toArcDir, fromLaneID, toLaneID, TurningDir);

                                InsertLConnector(connectorEntity, bezierLine);
                            }
                        } break;
                    #endregion -----------------------------------Left-------------------------------------

                    #region -----------------------------------Straight-------------------------------------
                    case "Straight":
                        {

                            #region ************************************ fromArcLaneNum > 2 **********************************
                            if (fromArcLaneNum > 2)
                            {
                                if (toArcLaneNum > 2)
                                {
                                    #region %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% fromArcLaneNum == toArcLaneNum %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
                                    if (fromArcLaneNum == toArcLaneNum)
                                    {

                                        for (int i = 1; i < fromArcLaneNum - 1; i++)
                                        {
                                            fromLanePostition = i;
                                            toLanePosition = i;

                                            fromLaneFea = lane.QueryFeatureBuRule(fromArcID, fromLanePostition);
                                            if (fromLaneFea == null)
                                            {
                                                return;
                                            }
                                            fromLaneID = Convert.ToInt32(fromLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                            fromLine = fromLaneFea.ShapeCopy as IPolyline;

                                            toLaneFea = lane.QueryFeatureBuRule(toArcID, toLanePosition);
                                            if (toLaneFea == null)
                                            {
                                                return;
                                            }
                                            toLaneID = Convert.ToInt32(toLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                            toLine = toLaneFea.ShapeCopy as IPolyline;
                                            bezierLine = GetConnectorShape(fromLine, toLine, null, true);

                                            LaneConnector connectorEntity = InitConnectorEty(0, fromLinkID, toLinkID, fromArcID, toArcID,
                                                fromArcDir, toArcDir, fromLaneID, toLaneID, TurningDir);

                                            InsertLConnector(connectorEntity, bezierLine);
                                        }

                                    }
                                    #endregion %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% fromArcLaneNum == toArcLaneNum %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

                                    #region %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% fromArcLaneNum > toArcLaneNum %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
                                    else if (fromArcLaneNum > toArcLaneNum)
                                    {

                                        for (int i = 1; i < fromArcLaneNum - 1; i++)
                                        {
                                            if (i <= (int)toArcLaneNum / 2)
                                            {
                                                fromLanePostition = i;
                                                toLanePosition = i;

                                                fromLaneFea = lane.QueryFeatureBuRule(fromArcID, fromLanePostition);
                                                if (fromLaneFea == null)
                                                {
                                                    return;
                                                }
                                                fromLaneID = Convert.ToInt32(fromLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                                fromLine = fromLaneFea.ShapeCopy as IPolyline;

                                                toLaneFea = lane.QueryFeatureBuRule(toArcID, toLanePosition);
                                                if (toLaneFea == null)
                                                {
                                                    return;
                                                }
                                                toLaneID = Convert.ToInt32(toLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                                toLine = toLaneFea.ShapeCopy as IPolyline;
                                                bezierLine = GetConnectorShape(fromLine, toLine, null, true);

                                                LaneConnector connectorEntity = InitConnectorEty(0, fromLinkID, toLinkID, fromArcID, toArcID,
                                                    fromArcDir, toArcDir, fromLaneID, toLaneID, TurningDir);

                                                InsertLConnector(connectorEntity, bezierLine);


                                                fromLanePostition = fromArcLaneNum - i - 1;
                                                toLanePosition = toArcLaneNum - i - 1;

                                                fromLaneFea = lane.QueryFeatureBuRule(fromArcID, fromLanePostition);
                                                if (fromLaneFea == null)
                                                {
                                                    return;
                                                }
                                                fromLaneID = Convert.ToInt32(fromLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                                fromLine = fromLaneFea.ShapeCopy as IPolyline;

                                                if (toLaneFea == null)
                                                {
                                                    return;
                                                }
                                                toLaneFea = lane.QueryFeatureBuRule(toArcID, toLanePosition);
                                                toLaneID = Convert.ToInt32(toLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                                toLine = toLaneFea.ShapeCopy as IPolyline;
                                                bezierLine = GetConnectorShape(fromLine, toLine, null, true);

                                                LaneConnector connectorEntity1 = InitConnectorEty(0, fromLinkID, toLinkID, fromArcID, toArcID,
                                                    fromArcDir, toArcDir, fromLaneID, toLaneID, TurningDir);

                                                InsertLConnector(connectorEntity1, bezierLine);
                                            }

                                            else if (i >= ((int)toArcLaneNum / 2) && i <= (fromArcLaneNum - (int)(toArcLaneNum / 2) - 1))
                                            {
                                                Random rdm = new Random();

                                                fromLanePostition = i;
                                                toLanePosition = (int)rdm.Next((int)toArcLaneNum / 2, (fromArcLaneNum - (int)(toArcLaneNum / 2) - 1));

                                                fromLaneFea = lane.QueryFeatureBuRule(fromArcID, fromLanePostition);
                                                if (fromLaneFea == null)
                                                {
                                                    return;
                                                }
                                                fromLaneID = Convert.ToInt32(fromLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                                fromLine = fromLaneFea.ShapeCopy as IPolyline;

                                                toLaneFea = lane.QueryFeatureBuRule(toArcID, toLanePosition);
                                                if (toLaneFea == null)
                                                {
                                                    return;
                                                }
                                                toLaneID = Convert.ToInt32(toLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                                toLine = toLaneFea.ShapeCopy as IPolyline;
                                                bezierLine = GetConnectorShape(fromLine, toLine, null, true);

                                                LaneConnector connectorEntity1 = InitConnectorEty(0, fromLinkID, toLinkID, fromArcID, toArcID,
                                                    fromArcDir, toArcDir, fromLaneID, toLaneID, TurningDir);

                                                InsertLConnector(connectorEntity1, bezierLine);
                                            }
                                        }
                                    }
                                    #endregion %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% fromArcLaneNum > toArcLaneNum %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%


                                    #region %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% fromArcLaneNum < toArcLaneNum %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
                                    else
                                    {
                                        for (int i = 1; i < toArcLaneNum - 1; i++)
                                        {
                                            if (i <= ((int)(fromArcLaneNum / 2) - 1))
                                            {
                                                fromLanePostition = i;
                                                toLanePosition = i;

                                                fromLaneFea = lane.QueryFeatureBuRule(fromArcID, fromLanePostition);
                                                if (fromLaneFea == null)
                                                {
                                                    return;
                                                }
                                                fromLaneID = Convert.ToInt32(fromLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                                fromLine = fromLaneFea.ShapeCopy as IPolyline;

                                                toLaneFea = lane.QueryFeatureBuRule(toArcID, toLanePosition);
                                                if (toLaneFea == null)
                                                {
                                                    return;
                                                }
                                                toLaneID = Convert.ToInt32(toLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                                toLine = toLaneFea.ShapeCopy as IPolyline;
                                                bezierLine = GetConnectorShape( fromLine, toLine,null,true);

                                                LaneConnector connectorEntity = InitConnectorEty(0, fromLinkID, toLinkID, fromArcID, toArcID,
                                                    fromArcDir, toArcDir, fromLaneID, toLaneID, TurningDir);

                                                InsertLConnector(connectorEntity, bezierLine);


                                                fromLanePostition = fromArcLaneNum - i - 1;
                                                toLanePosition = toArcLaneNum - i - 1;

                                                fromLaneFea = lane.QueryFeatureBuRule(fromArcID, fromLanePostition);
                                                if (fromLaneFea == null)
                                                {
                                                    return;
                                                }
                                                fromLaneID = Convert.ToInt32(fromLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                                fromLine = fromLaneFea.ShapeCopy as IPolyline;

                                                toLaneFea = lane.QueryFeatureBuRule(toArcID, toLanePosition);
                                                if (toLaneFea == null)
                                                {
                                                    return;
                                                }
                                                toLaneID = Convert.ToInt32(toLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                                toLine = toLaneFea.ShapeCopy as IPolyline;
                                                bezierLine = GetConnectorShape( fromLine, toLine,null,true);

                                                LaneConnector connectorEntity1 = InitConnectorEty(0, fromLinkID, toLinkID, fromArcID, toArcID,
                                                    fromArcDir, toArcDir, fromLaneID, toLaneID, TurningDir);

                                                InsertLConnector(connectorEntity1, bezierLine);


                                            }
                                            else if (i >= ((int)fromArcLaneNum / 2) && i <= (toArcLaneNum - (int)(fromArcLaneNum / 2) - 1))
                                            {
                                                Random rdm = new Random();

                                                fromLanePostition = (int)rdm.Next((int)fromArcLaneNum / 2, toArcLaneNum - (int)(fromArcLaneNum / 2) - 1);
                                                toLanePosition = i;

                                                fromLaneFea = lane.QueryFeatureBuRule(fromArcID, fromLanePostition);
                                                if (fromLaneFea == null)
                                                {
                                                    return;
                                                }

                                                fromLaneID = Convert.ToInt32(fromLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                                fromLine = fromLaneFea.ShapeCopy as IPolyline;

                                                toLaneFea = lane.QueryFeatureBuRule(toArcID, toLanePosition);
                                                if (toLaneFea == null)
                                                {
                                                    return;
                                                }

                                                toLaneID = Convert.ToInt32(toLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                                toLine = toLaneFea.ShapeCopy as IPolyline;
                                                bezierLine = GetConnectorShape(fromLine, toLine, null, true);

                                                LaneConnector connectorEntity1 = InitConnectorEty(0, fromLinkID, toLinkID, fromArcID, toArcID,
                                                    fromArcDir, toArcDir, fromLaneID, toLaneID, TurningDir);

                                                InsertLConnector(connectorEntity1, bezierLine);
                                            }
                                        }
                                    }
                                    #endregion %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% fromArcLaneNum < toArcLaneNum %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
                                }
                                else
                                {
                                    for (int i = 1; i < fromArcLaneNum - 1; i++)
                                    {
                                        fromLanePostition = fromArcLaneNum - 1;
                                        toLanePosition = i;

                                        fromLaneFea = lane.QueryFeatureBuRule(fromArcID, fromLanePostition);
                                        if (fromLaneFea == null)
                                        {
                                            return;
                                        }
                                        fromLaneID = Convert.ToInt32(fromLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                        fromLine = fromLaneFea.ShapeCopy as IPolyline;

                                        toLaneFea = lane.QueryFeatureBuRule(toArcID, toLanePosition);
                                        if (toLaneFea == null)
                                        {
                                            return;
                                        }
                                        toLaneID = Convert.ToInt32(toLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                        toLine = toLaneFea.ShapeCopy as IPolyline;
                                        bezierLine = GetConnectorShape(fromLine, toLine, null, true);

                                        LaneConnector connectorEntity = InitConnectorEty(0, fromLinkID, toLinkID, fromArcID, toArcID,
                                            fromArcDir, toArcDir, fromLaneID, toLaneID, TurningDir);

                                        InsertLConnector(connectorEntity, bezierLine);
                                    }
                                }
                            }
                            #endregion ************************************ fromArcLaneNum > 2 **********************************

                            #region ************************************ fromArcLaneNum<=2 **********************************
                            else
                            {
                                if (toArcLaneNum > 2)
                                {
                                    for (int i = 1; i < toArcLaneNum - 1; i++)
                                    {
                                        fromLanePostition = fromArcLaneNum - 1;
                                        toLanePosition = i;

                                        fromLaneFea = lane.QueryFeatureBuRule(fromArcID, fromLanePostition);
                                        if (fromLaneFea == null)
                                        {
                                            return;
                                        }
                                        fromLaneID = Convert.ToInt32(fromLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                        fromLine = fromLaneFea.ShapeCopy as IPolyline;

                                        toLaneFea = lane.QueryFeatureBuRule(toArcID, toLanePosition);
                                        if(toLaneFea == null)
                                        {
                                            return;
                                        }
                                        toLaneID = Convert.ToInt32(toLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                        toLine = toLaneFea.ShapeCopy as IPolyline;
                                        bezierLine = GetConnectorShape(fromLine, toLine, null, true);

                                        LaneConnector connectorEntity = InitConnectorEty(0, fromLinkID, toLinkID, fromArcID, toArcID,
                                            fromArcDir, toArcDir, fromLaneID, toLaneID, TurningDir);

                                        InsertLConnector(connectorEntity, bezierLine);
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < toArcLaneNum - 1; i++)
                                    {
                                        fromLanePostition = i;
                                        toLanePosition = i;

                                        fromLaneFea = lane.QueryFeatureBuRule(fromArcID, fromLanePostition);
                                        if (fromLaneFea == null)
                                        {
                                            return;
                                        }
                                        fromLaneID = Convert.ToInt32(fromLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                        fromLine = fromLaneFea.ShapeCopy as IPolyline;

                                        toLaneFea = lane.QueryFeatureBuRule(toArcID, toLanePosition);
                                        if (toLaneFea == null)
                                        {
                                            return;
                                        }

                                        toLaneID = Convert.ToInt32(toLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                        toLine = toLaneFea.ShapeCopy as IPolyline;
                                        bezierLine = GetConnectorShape(fromLine, toLine, null, true);

                                        LaneConnector connectorEntity = InitConnectorEty(0, fromLinkID, toLinkID, fromArcID, toArcID,
                                            fromArcDir, toArcDir, fromLaneID, toLaneID, TurningDir);

                                        InsertLConnector(connectorEntity, bezierLine);
                                    }
                                }
                            }
                            #endregion ************************************ fromArcLaneNum<=2 **********************************
                        } break;
                    #endregion -----------------------------------Straight-------------------------------------

                    case "Uturn":
                        {
                            if (fromArcLaneNum > 2 && toArcLaneNum > 2)
                            {

                                fromLanePostition = 0;
                                toLanePosition = 0;

                                fromLaneFea = lane.QueryFeatureBuRule(fromArcID, fromLanePostition);
                                fromLaneID = Convert.ToInt32(fromLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                fromLine = fromLaneFea.ShapeCopy as IPolyline;

                                toLaneFea = lane.QueryFeatureBuRule(toArcID, toLanePosition);
                                toLaneID = Convert.ToInt32(toLaneFea.get_Value(pFeaClsLane.FindField(LaneFeatureService.LaneIDNm)));
                                toLine = toLaneFea.ShapeCopy as IPolyline;
                                bezierLine = GetConnectorShape(fromLine, toLine, null,true);

                                LaneConnector connectorEntity = InitConnectorEty(0, fromLinkID, toLinkID, fromArcID, toArcID,
                                    fromArcDir, toArcDir, fromLaneID, toLaneID, TurningDir);

                                InsertLConnector(connectorEntity, bezierLine);
                            }
                        } break;
                    default:
                        break;
                }
            }
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromLine"></param>起始车道的几何
        /// <param name="toLine"></param>终止车道的几何
        /// <param name="nodePnt"></param> Node 的几何，当该值为空值，获取两车道交点为中间控制点
        /// <param name="straightFlag"></param>
        /// <returns></returns>
        public IPolyline GetConnectorShape(IPolyline fromLine, IPolyline toLine,IPoint nodePnt,bool straightFlag)
        {
            IPolyline bezier = new PolylineClass();
            if (straightFlag == true)
            {
                IPointCollection col = new PolylineClass();

                col.AddPoint(fromLine.ToPoint);
                col.AddPoint(toLine.FromPoint);
                bezier = col as IPolyline;

            }
            else
            {
                IPolyline toExtendLine1 = new PolylineClass();
                toExtendLine1 = LineHelper.CreateLine(toLine, -20, -20);

                IPolyline toExtendLine2 = new PolylineClass();
                toExtendLine2 = LineHelper.CreateLine(fromLine, -20, -20);

                IPoint pnt1 = new PointClass();
                IPoint pnt2 = new PointClass();
                IPoint pntMiddle = new PointClass();
                IRay ray = new RayClass();
                IVector3D vector = new Vector3DClass();

                pnt1 = fromLine.ToPoint;

                pnt2 = toLine.FromPoint;

                //nodePnt是交叉口结点的几何，
                //左转时，认为中间控制点为NodePoint
                if (nodePnt != null)
                {
                    pntMiddle = nodePnt;
                }
                else
                {
                    pntMiddle = LineHelper.GetIntersectionPoint(toExtendLine1, toExtendLine2);
                }

                bezier = LineHelper.DrawBezier(pnt1, pntMiddle, pnt2);
            }
            
            
            
            return bezier;

        }


        public bool IsExistedConnByLane(int fromLaneID, int toLaneID)
        {
            bool existedFlag = false;
            int connectorID = GetConnectorIdByLane(fromLaneID, toLaneID);
            if (connectorID != -1)
            {
                existedFlag = true;
            }
            else
            {
                existedFlag = false;
            }
            return existedFlag;
        }

        public int GetConnectorIdByLane(int fromLaneID, int toLaneID)
        {
            int connectorID = -1;

            IQueryFilter LCFilter1 = new QueryFilterClass();
            LCFilter1.WhereClause = String.Format("fromLaneID={0} AND toLaneID={1}", fromLaneID, toLaneID);

            IFeatureCursor LaneConnectorCursor1 = _pFeaClsConnector.Search(LCFilter1, false);
            IFeature LaneConnectorRow1 = LaneConnectorCursor1.NextFeature();

            if (LaneConnectorRow1 != null)
            {
                connectorID = Convert.ToInt32(LaneConnectorRow1.get_Value(_pFeaClsConnector.FindField("ConnectorID")));
            }

            return connectorID;
        }

        public LaneConnector InitConnectorEty(int connectorID, int fromLinkID, int toLinkID, int fromArcID, int toArcID, int fromArcDir, int toArcDir, int fromLaneID, int toLaneID, string TurningDir)
        {
            LaneConnector connectorEntity = new LaneConnector();
            connectorEntity.ConnectorID = connectorID;
            connectorEntity.fromLinkID = fromLinkID;
            connectorEntity.toLinkID = toLinkID;
            connectorEntity.fromArcID = fromArcID;
            connectorEntity.toArcID = toArcID;
            connectorEntity.fromDir = fromArcDir;
            connectorEntity.toDir = toArcDir;
            connectorEntity.fromLaneID = fromLaneID;
            connectorEntity.toLaneID = toLaneID;
            connectorEntity.TurningDir = TurningDir;
            return connectorEntity;
        }


        //tlink = Convert.ToInt32(Com.ExecuteScalar());
        //Com.Dispose();

        //for (int a = 10000; a <= num; a++)
        //{
        //    int n2, n3, n6, n7;
        //    double n4, n8;
        //    string n1, n5;
        //    string Str_1 = "select Name from Vissim_Connector where id=" + a;
        //    OleDbCommand Com_1 = new OleDbCommand(Str_1, Conn);
        //    n1 = Convert.ToString(Com_1.ExecuteScalar());

        //    Str_1 = "select FLink from Vissim_Connector where id=" + a;
        //    Com_1 = new OleDbCommand(Str_1, Conn);
        //    n2 = Convert.ToInt32(Com_1.ExecuteScalar());

        //    Str_1 = "select FLane from Vissim_Connector where id=" + a;
        //    Com_1 = new OleDbCommand(Str_1, Conn);
        //    n3 = Convert.ToInt32(Com_1.ExecuteScalar());

        //    Str_1 = "select FPos from Vissim_Connector where id=" + a;
        //    Com_1 = new OleDbCommand(Str_1, Conn);
        //    n4 = Convert.ToDouble(Com_1.ExecuteScalar());

        //    Str_1 = "select Overnode from Vissim_Connector where id=" + a;
        //    Com_1 = new OleDbCommand(Str_1, Conn);
        //    object o = Com_1.ExecuteScalar();
        //    if (o == null)
        //        n5 = "";
        //    else
        //        n5 = o.ToString();

        //    Str_1 = "select TLink from Vissim_Connector where id=" + a;
        //    Com_1 = new OleDbCommand(Str_1, Conn);
        //    n6 = Convert.ToInt32(Com_1.ExecuteScalar());

        //    Str_1 = "select TLane from Vissim_Connector where id=" + a;
        //    Com_1 = new OleDbCommand(Str_1, Conn);
        //    n7 = Convert.ToInt32(Com_1.ExecuteScalar());

        //    Str_1 = "select TPos from Vissim_Connector where id=" + a;
        //    Com_1 = new OleDbCommand(Str_1, Conn);
        //    n8 = Convert.ToDouble(Com_1.ExecuteScalar());

        //    //sw.WriteLine("CONNECTOR " + a + " NAME " + "\"" + n1 + "\"" + " LABEL  0.00 0.00");
        //    connectors += "CONNECTOR " + a + " NAME " + "\"" + n1 + "\"" + " LABEL  0.00 0.00" + "\r\n";

        //    //sw.WriteLine("  FROM LINK " + n2 + " LANES " + n3 + " AT " + n4);
        //    connectors += "  FROM LINK " + n2 + " LANES " + n3 + " AT " + n4 + "\r\n";

        //    if (n5 != "")
        //    {
        //        string[] Overnodes = n5.Split(';');
        //        string Ovnode = "";
        //        for (int n = 0; n < Overnodes.Length; n++)
        //        {
        //            Ovnode += "  OVER " + Overnodes[n];
        //        }
        //        //sw.WriteLine(Ovnode);
        //        connectors += Ovnode + "\r\n";
        //    }
        //    if (n8 == 0.00)
        //    {
        //        //sw.WriteLine("  TO LINK " + n6 + " LANES " + n7 + " AT 0.000 " + " ALL");
        //        connectors += "  TO LINK " + n6 + " LANES " + n7 + " AT 0.000 " + " ALL" + "\r\n";
        //    }
        //    else
        //    {
        //        //sw.WriteLine("  TO LINK " + n6 + " LANES " + n7 + " AT 0.100 " + " ALL");
        //        connectors += "  TO LINK " + n6 + " LANES " + n7 + " AT 0.100 " + " ALL" + "\r\n";
        //    }
        //    //sw.WriteLine("  DX_EMERG_STOP 5.000 DX_LANE_CHANGE 200.000");
        //    //sw.WriteLine("  GRADIENT 0.00000  COST 0.00000  SURCHARGE 0.00000  SURCHARGE 0.00000");
        //    //sw.WriteLine("  SEGMENT LENGTH 10.000 ANIMATION");

        //    connectors += "  DX_EMERG_STOP 5.000 DX_LANE_CHANGE 200.000" + "\r\n" + "  GRADIENT 0.00000  COST 0.00000  SURCHARGE 0.00000  SURCHARGE 0.00000" + "\r\n" + "  SEGMENT LENGTH 10.000 ANIMATION" + "\r\n";
        //}
        ////sw.Flush();
        ////sw.Close();
        //Conn.Close();

    }
}
