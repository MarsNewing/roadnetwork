using ESRI.ArcGIS.Geodatabase;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.Road;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.Connection;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LinkLayer;
using System;

namespace RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LaneLayer
{
    class LaneConnectorTableService
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


        private ITable _tableConnector;
        private int _connectorID;

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="tableConnector"></param>ITable 
        /// <param name="connectorID"></param>
        public LaneConnectorTableService(ITable tableConnector,int connectorID)
        {
            _tableConnector = tableConnector;
            _connectorID = connectorID;
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
            ICursor cursor = _tableConnector.Search(queryFilter, false);
            IRow targetRow = cursor.NextRow();
            if (targetRow != null)
            {
                targetLaneEty.ConnectorID = _connectorID;
                targetLaneEty.fromArcID = Convert.ToInt32(targetRow.get_Value(_tableConnector.FindField("fromArcID")));
                targetLaneEty.fromDir = Convert.ToInt32(targetRow.get_Value(_tableConnector.FindField("fromDir")));
                targetLaneEty.fromLaneID = Convert.ToInt32(targetRow.get_Value(_tableConnector.FindField("fromLaneID")));
                targetLaneEty.fromLinkID = Convert.ToInt32(targetRow.get_Value(_tableConnector.FindField("fromLinkID")));
                targetLaneEty.toArcID = Convert.ToInt32(targetRow.get_Value(_tableConnector.FindField("toArcID")));
                targetLaneEty.toDir = Convert.ToInt32(targetRow.get_Value(_tableConnector.FindField("toDir")));
                targetLaneEty.toLaneID = Convert.ToInt32(targetRow.get_Value(_tableConnector.FindField("toLaneID")));
                targetLaneEty.toLinkID = Convert.ToInt32(targetRow.get_Value(_tableConnector.FindField("toLinkID")));
                targetLaneEty.TurningDir = Convert.ToString(targetRow.get_Value(_tableConnector.FindField("TurningDir")));
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
        public void InsertLConnector(LaneConnector updateLaneConnEty)
        {

            IDataset dataset = _tableConnector as IDataset;
            IWorkspace ws = dataset.Workspace;
            IWorkspaceEdit workspceEdit = ws as IWorkspaceEdit;
            workspceEdit.StartEditing(false);
            workspceEdit.StartEditOperation();
            ICursor cursor;
            IRowBuffer newRowBuffer = _tableConnector.CreateRowBuffer();
            cursor = _tableConnector.Insert(true);

            newRowBuffer.set_Value(_tableConnector.FindField("ConnectorID"), updateLaneConnEty.ConnectorID);
            newRowBuffer.set_Value(_tableConnector.FindField("fromArcID"), updateLaneConnEty.fromArcID);
            newRowBuffer.set_Value(_tableConnector.FindField("fromDir"), updateLaneConnEty.fromDir);
            newRowBuffer.set_Value(_tableConnector.FindField("fromLaneID"), updateLaneConnEty.fromLaneID);
            newRowBuffer.set_Value(_tableConnector.FindField("fromLinkID"), updateLaneConnEty.fromLinkID);
            newRowBuffer.set_Value(_tableConnector.FindField("toArcID"), updateLaneConnEty.toArcID);
            newRowBuffer.set_Value(_tableConnector.FindField("toDir"), updateLaneConnEty.toDir);
            newRowBuffer.set_Value(_tableConnector.FindField("toLinkID"), updateLaneConnEty.toLinkID);
            newRowBuffer.set_Value(_tableConnector.FindField("toLaneID"), updateLaneConnEty.toLaneID);
            newRowBuffer.set_Value(_tableConnector.FindField("TurningDir"), updateLaneConnEty.TurningDir);

            cursor.InsertRow(newRowBuffer);
            workspceEdit.StopEditOperation();
            workspceEdit.StopEditing(true);
        }

        /// <summary>
        /// 更新车道连接器
        /// </summary>
        /// <param name="updateLaneConnEty"></param>
        /// <param name="preLaneConnID"></param>
        public void UpdateConnector(LaneConnector updateLaneConnEty, int preLaneConnID)
        {
            LaneConnector preLaneConnEty = GetLaneConnEty(preLaneConnID);

            IDataset dataset = _tableConnector as IDataset;
            IWorkspace ws = dataset.Workspace;
            IWorkspaceEdit workspceEdit = ws as IWorkspaceEdit;
            workspceEdit.StartEditing(false);
            workspceEdit.StartEditOperation();
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = String.Format("{0}={1}",ConnectorIDNm, preLaneConnEty.ConnectorID);
            ICursor pFeatureCuror1 = _tableConnector.Search(filter, false);
            IRow modifiedRowArc = pFeatureCuror1.NextRow();

            if (updateLaneConnEty.TurningDir != preLaneConnEty.TurningDir)
            {
                modifiedRowArc.set_Value(_tableConnector.FindField(TurningDirNm), updateLaneConnEty.TurningDir);
            }
            if (updateLaneConnEty.fromArcID != preLaneConnEty.fromArcID)
            {
                modifiedRowArc.set_Value(_tableConnector.FindField(fromArcIDNm), updateLaneConnEty.fromArcID);
            }
            if (updateLaneConnEty.fromDir != preLaneConnEty.fromDir)
            {
                modifiedRowArc.set_Value(_tableConnector.FindField(fromDirNm), updateLaneConnEty.fromDir);
            }
            if (updateLaneConnEty.fromLaneID != preLaneConnEty.fromLaneID)
            {
                modifiedRowArc.set_Value(_tableConnector.FindField(fromLaneIDNm), updateLaneConnEty.fromLaneID);
            }
            if (updateLaneConnEty.fromLinkID != preLaneConnEty.fromLinkID)
            {
                modifiedRowArc.set_Value(_tableConnector.FindField(fromLinkIDNm), updateLaneConnEty.fromLinkID);
            }
            if (updateLaneConnEty.toArcID != preLaneConnEty.toArcID)
            {
                modifiedRowArc.set_Value(_tableConnector.FindField(toArcIDNm), updateLaneConnEty.toArcID);
            }
            if (updateLaneConnEty.toDir != preLaneConnEty.toDir)
            {
                modifiedRowArc.set_Value(_tableConnector.FindField(toArcIDNm), updateLaneConnEty.toDir);
            }

            if (updateLaneConnEty.toLaneID != preLaneConnEty.toLaneID)
            {
                modifiedRowArc.set_Value(_tableConnector.FindField(toLaneIDNm), updateLaneConnEty.toLaneID);
            }
            if (updateLaneConnEty.toLinkID != preLaneConnEty.toLinkID)
            {
                modifiedRowArc.set_Value(_tableConnector.FindField(toLinkIDNm), updateLaneConnEty.toLinkID);
            }
            modifiedRowArc.Store();

            workspceEdit.StopEditOperation();
            workspceEdit.StopEditing(true);
        }

        /// <summary>
        /// 删除车道连接器
        /// </summary>
        /// <param name="connecttorID"></param>车道连接器的ID
        public void DeleteConnector(int connecttorID)
        {
            IDataset dataset = _tableConnector as IDataset;
            IWorkspace ws = dataset.Workspace;
            IWorkspaceEdit workspceEdit = ws as IWorkspaceEdit;
            workspceEdit.StartEditing(false);
            workspceEdit.StartEditOperation();

            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = String.Format("{0}={1}",ConnectorIDNm, connecttorID);
            ICursor cursor = _tableConnector.Update(filter, false);
            IRow deleteRow = cursor.NextRow();

            deleteRow.Delete();

            workspceEdit.StopEditOperation();
            workspceEdit.StopEditing(true);
        }

        /// <summary>
        /// 按照默认规则生成车道连接器
        /// </summary>
        /// <param name="fromLinkID"></param>
        /// <param name="toLinkID"></param>
        /// <param name="intersctionNode"></param>
        public void CreateConnectorByDefault(LinkService fromLink, LinkService toLink, int intersctionNode)
        {
            int toLinkFNode, toLinkTNode;
            int fromLinkFNode, fromLinkTNode;

            int toLinkID = toLink._linkId;
            int toArcID = 0;
            int toArcDir;
            int toArcLaneNum = 0;

            int fromLinkID = fromLink._linkId;
            int fromArcID = 0;
            int fromArcDir = 0;
            int fromArcLaneNum = 0;

            bool eixtConnectorFlag = false;

            IFeature fromLinkFea = fromLink.GetFeature();
            LinkMaster linkMasterEty = new LinkMaster();
            linkMasterEty = fromLink.GetEntity(fromLinkFea);

            Link fromLinkEty = new Link();
            fromLinkEty = fromLinkEty.Copy(linkMasterEty);

            fromLinkFNode = fromLinkEty.FNodeID;
            fromLinkTNode = fromLinkEty.TNodeID;

            IFeature toLinkFea = toLink.GetFeature();

            linkMasterEty = fromLink.GetEntity(toLinkFea);
            Link toLinkEty = new Link();
            toLinkEty = toLinkEty.Copy(linkMasterEty);
            toLinkFNode = toLinkEty.FNodeID;
            toLinkTNode = toLinkEty.TNodeID;

            if (fromLinkFNode == intersctionNode)
            {
                fromArcDir = -1;
            }
            else if (fromLinkTNode == intersctionNode)
            {
                fromArcDir = 1;
            }
            else
            {
                fromArcDir = 0;
            }

            //获得toArc的Dir
            if (toLinkFNode == intersctionNode)
            {
                toArcDir = 1;
            }
            else if (toLinkTNode == intersctionNode)
            {
                toArcDir = -1;

            }
            else
            {
                toArcDir = 0;
            }

            IWorkspace pWS= fromLink._pFeaClsLink.FeatureDataset.Workspace;
            IFeatureClass pFeaClsArc=(pWS as IFeatureWorkspace).OpenFeatureClass(Arc.ArcFeatureName);
            ArcService arc = new ArcService(pFeaClsArc, -1);

            Arc fromArcEity = new Arc();
            string queryStr = String.Format("{0} = {1} and {2} = {3}", Arc.LinkIDNm, fromLink._linkId, Arc.FlowDirNm, fromArcDir);
            fromArcEity = arc.GetArcEtyByRule(queryStr);

            Arc toArcEity = new Arc();
            queryStr = String.Format("{0} = {1} and {2} = {3}", Arc.LinkIDNm, toLink._linkId, Arc.FlowDirNm, toArcDir);
            toArcEity = arc.GetArcEtyByRule(queryStr);

            string TurningDir = LogicalConnection.GetTurningDir(fromLink, toLink);
            switch (TurningDir)
            {
                case "Right":
                    {
                        //当入口有向子路段的车道数大于等于6时最右两个车道为右转车道；
                        if (fromArcLaneNum >= 5 && toArcLaneNum < 5)
                        {
                            LaneConnector connectorEntity1 = InitConnectorEty(_connectorID, fromLinkID, toLinkID, fromArcID, fromArcID, fromArcDir, toArcDir, fromArcID * 10 + fromArcLaneNum - 1, toArcID * 10 + toArcLaneNum, TurningDir);
                            InsertLConnector(connectorEntity1);
                            _connectorID = _connectorID + 1;
                            if (toArcLaneNum > 2)
                            {
                                LaneConnector connectorEntity2 = InitConnectorEty(_connectorID, fromLinkID, toLinkID, fromArcID, fromArcID, fromArcDir, toArcDir, fromArcID * 10 + fromArcLaneNum, toArcID * 10 + toArcLaneNum, TurningDir);
                                InsertLConnector(connectorEntity2);
                                _connectorID = _connectorID + 1;
                            }

                        }

                        else if (fromArcLaneNum < 5 && toArcLaneNum < 5)
                        {
                            LaneConnector connectorEntity1 = InitConnectorEty(_connectorID, fromLinkID, toLinkID, fromArcID, fromArcID, fromArcDir, toArcDir, fromArcID * 10 + fromArcLaneNum, toArcID * 10 + toArcLaneNum, TurningDir);
                            InsertLConnector(connectorEntity1);
                            _connectorID = _connectorID + 1;
                        }
                        else if (fromArcLaneNum >= 5 && toArcLaneNum >= 5)
                        {
                            LaneConnector connectorEntity1 = InitConnectorEty(_connectorID, fromLinkID, toLinkID, fromArcID, fromArcID, fromArcDir, toArcDir, fromArcID * 10 + fromArcLaneNum - 1, toArcID * 10 + toArcLaneNum - 1, TurningDir);
                            InsertLConnector(connectorEntity1);
                            _connectorID = _connectorID + 1;
                            LaneConnector connectorEntity2 = InitConnectorEty(_connectorID, fromLinkID, toLinkID, fromArcID, fromArcID, fromArcDir, toArcDir, fromArcID * 10 + fromArcLaneNum, toArcID * 10 + toArcLaneNum, TurningDir);
                            InsertLConnector(connectorEntity2);
                            _connectorID = _connectorID + 1;
                        }
                        else
                        {
                            LaneConnector connectorEntity1 = InitConnectorEty(_connectorID, fromLinkID, toLinkID, fromArcID, fromArcID, fromArcDir, toArcDir, fromArcID * 10 + fromArcLaneNum, toArcID * 10 + toArcLaneNum - 1, TurningDir);
                            InsertLConnector(connectorEntity1);
                            _connectorID = _connectorID + 1;
                            if (fromArcLaneNum > 2)
                            {
                                LaneConnector connectorEntity2 = InitConnectorEty(_connectorID, fromLinkID, toLinkID, fromArcID, fromArcID, fromArcDir, toArcDir, fromArcID * 10 + fromArcLaneNum, toArcID * 10 + toArcLaneNum, TurningDir);
                                InsertLConnector(connectorEntity2);
                                _connectorID = _connectorID + 1;
                            }

                        }

                    } break;

                case "Left":
                    {
                        if (fromArcLaneNum >= 5 && toArcLaneNum < 5)
                        {
                            LaneConnector connectorEntity1 = InitConnectorEty(_connectorID, fromLinkID, toLinkID, fromArcID, fromArcID, fromArcDir, toArcDir, fromArcID * 10 + 1, toArcID * 10 + 1, TurningDir);
                            InsertLConnector(connectorEntity1);
                            _connectorID = _connectorID + 1;
                            if (toArcLaneNum > 2)
                            {
                                LaneConnector connectorEntity2 = InitConnectorEty(_connectorID, fromLinkID, toLinkID, fromArcID, fromArcID, fromArcDir, toArcDir, fromArcID * 10 + 2, toArcID * 10 + 1, TurningDir);
                                InsertLConnector(connectorEntity2);
                                _connectorID = _connectorID + 1;
                            }

                        }

                        else if (fromArcLaneNum < 5 && toArcLaneNum < 5)
                        {
                            LaneConnector connectorEntity1 = InitConnectorEty(_connectorID, fromLinkID, toLinkID, fromArcID, fromArcID, fromArcDir, toArcDir, fromArcID * 10 + 1, toArcID * 10 + 1, TurningDir);
                            InsertLConnector(connectorEntity1);
                            _connectorID = _connectorID + 1;
                        }
                        else if (fromArcLaneNum >= 5 && toArcLaneNum >= 5)
                        {
                            LaneConnector connectorEntity1 = InitConnectorEty(_connectorID, fromLinkID, toLinkID, fromArcID, fromArcID, fromArcDir, toArcDir, fromArcID * 10 + 1, toArcID * 10 + 1, TurningDir);
                            InsertLConnector(connectorEntity1);
                            _connectorID = _connectorID + 1;

                            LaneConnector connectorEntity2 = InitConnectorEty(_connectorID, fromLinkID, toLinkID, fromArcID, fromArcID, fromArcDir, toArcDir, fromArcID * 10 + 2, toArcID * 10 + 2, TurningDir);
                            InsertLConnector(connectorEntity2);
                            _connectorID = _connectorID + 1;
                        }
                        else
                        {
                            LaneConnector connectorEntity1 = InitConnectorEty(_connectorID, fromLinkID, toLinkID, fromArcID, fromArcID, fromArcDir, toArcDir, fromArcID * 10 + 1, toArcID * 10 + 1, TurningDir);
                            InsertLConnector(connectorEntity1);
                            _connectorID = _connectorID + 1;

                            LaneConnector connectorEntity2 = InitConnectorEty(_connectorID, fromLinkID, toLinkID, fromArcID, fromArcID, fromArcDir, toArcDir, fromArcID * 10 + 1, toArcID * 10 + 2, TurningDir);
                            InsertLConnector(connectorEntity2);
                            _connectorID = _connectorID + 1;
                        }
                    } break;

                case "Straight":
                    {
                        if (fromArcLaneNum > 2 && fromArcLaneNum == toArcLaneNum)
                        {
                            for (int i = 2; i < fromArcLaneNum; i++)
                            {
                                LaneConnector connectorEntity1 = InitConnectorEty(_connectorID, fromLinkID, toLinkID, fromArcID, fromArcID, fromArcDir, toArcDir, fromArcID * 10 + i, toArcID * 10 + i, TurningDir);
                                InsertLConnector(connectorEntity1);
                                _connectorID = _connectorID + 1;
                            }

                        }
                        if (toArcLaneNum > 2 && fromArcLaneNum > toArcLaneNum)
                        {

                            for (int i = 2; i < fromArcLaneNum; i++)
                            {
                                if (i <= (int)toArcLaneNum / 2)
                                {
                                    LaneConnector connectorEntity1 = InitConnectorEty(_connectorID, fromLinkID, toLinkID, fromArcID, fromArcID, fromArcDir, toArcDir, fromArcID * 10 + i, toArcID * 10 + i, TurningDir);
                                    InsertLConnector(connectorEntity1);
                                    _connectorID = _connectorID + 1;
                                    LaneConnector connectorEntity2 = InitConnectorEty(_connectorID, fromLinkID, toLinkID, fromArcID, fromArcID, fromArcDir, toArcDir, fromArcID * 10 + fromArcLaneNum - i + 1, toArcID * 10 + toArcLaneNum - i + 1, TurningDir);
                                    InsertLConnector(connectorEntity2);
                                    _connectorID = _connectorID + 1;
                                }
                                else if (i > (int)toArcLaneNum / 2 && i < fromArcLaneNum - (int)toArcLaneNum / 2)
                                {
                                    Random rdm = new Random();
                                    int toLaneID = toArcID * 10 + (int)rdm.Next((int)toArcLaneNum / 2, fromArcLaneNum - (int)toArcLaneNum / 2);
                                    LaneConnector connectorEntity1 = InitConnectorEty(_connectorID, fromLinkID, toLinkID, fromArcID, fromArcID, fromArcDir, toArcDir, fromArcID * 10 + i, toLaneID, TurningDir);
                                    _connectorID = _connectorID + 1;
                                }
                            }
                        }
                        if (fromArcLaneNum > 2 && fromArcLaneNum < toArcLaneNum)
                        {

                            for (int i = 2; i < toArcLaneNum; i++)
                            {
                                if (i <= (int)fromArcLaneNum / 2)
                                {
                                    LaneConnector connectorEntity1 = InitConnectorEty(_connectorID, fromLinkID, toLinkID, fromArcID, fromArcID, fromArcDir, toArcDir, fromArcID * 10 + i, toArcID * 10 + i, TurningDir);
                                    InsertLConnector(connectorEntity1);
                                    _connectorID = _connectorID + 1;
                                    LaneConnector connectorEntity2 = InitConnectorEty(_connectorID, fromLinkID, toLinkID, fromArcID, fromArcID, fromArcDir, toArcDir, fromArcID * 10 + fromArcLaneNum - i + 1, toArcID * 10 + toArcLaneNum - i + 1, TurningDir);
                                    InsertLConnector(connectorEntity2);
                                    _connectorID = _connectorID + 1;
                                }
                                else if (i > (int)fromArcLaneNum / 2 && i < fromArcLaneNum - (int)fromArcLaneNum / 2)
                                {
                                    Random rdm = new Random();
                                    int fromLaneID = toArcID * 10 + (int)rdm.Next((int)fromArcLaneNum / 2, toArcLaneNum - (int)fromArcLaneNum / 2);
                                    LaneConnector connectorEntity1 = InitConnectorEty(_connectorID, fromLinkID, toLinkID, fromArcID, fromArcID, fromArcDir, toArcDir, fromLaneID, toArcID * 10 + i, TurningDir);
                                    InsertLConnector(connectorEntity1);
                                    _connectorID = _connectorID + 1;
                                }
                            }
                        }
                        if (fromArcLaneNum <= 2)
                        {

                            for (int i = 1; i < toArcLaneNum; i++)
                            {
                                eixtConnectorFlag = IsExistedConnByLane(fromArcID * 10 + 1, toArcID * 10 + i);
                                if (eixtConnectorFlag == false)
                                {
                                    LaneConnector connectorEntity1 = InitConnectorEty(_connectorID, fromLinkID, toLinkID, fromArcID, fromArcID, fromArcDir, toArcDir, fromArcID * 10 + 1, toArcID * 10 + i, TurningDir);
                                    InsertLConnector(connectorEntity1);
                                    _connectorID = _connectorID + 1;
                                }
                            }

                        }
                        if (toArcLaneNum <= 2)
                        {
                            for (int i = 1; i < fromArcLaneNum; i++)
                            {
                                eixtConnectorFlag = IsExistedConnByLane(fromArcID * 10 + i, toArcID * 10 + 1);
                                if (eixtConnectorFlag == false)
                                {
                                    LaneConnector connectorEntity1 = InitConnectorEty(_connectorID, fromLinkID, toLinkID, fromArcID, fromArcID, fromArcDir, toArcDir, fromArcID * 10 + i, toArcID * 10 + 1, TurningDir);
                                    InsertLConnector(connectorEntity1);
                                    _connectorID = _connectorID + 1;
                                }

                            }

                        }
                    } break;
                case "Uturn":
                    {
                        LaneConnector connectorEntity1 = InitConnectorEty(_connectorID, fromLinkID, toLinkID, fromArcID, fromArcID, fromArcDir, toArcDir, fromArcID * 10 + 1, toArcID * 10 + 1, TurningDir);
                        InsertLConnector(connectorEntity1);
                        _connectorID = _connectorID + 1;
                    } break;
                default:
                    break;
            }
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

            ICursor LaneConnectorCursor1 = _tableConnector.Search(LCFilter1, false);
            IRow LaneConnectorRow1 = LaneConnectorCursor1.NextRow();

            if (LaneConnectorRow1 != null)
            {
                connectorID = Convert.ToInt32(LaneConnectorRow1.get_Value(_tableConnector.FindField("ConnectorID")));
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


    }
}
