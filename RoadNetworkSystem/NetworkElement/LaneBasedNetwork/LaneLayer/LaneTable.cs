using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.Road;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.Connection;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LinkLayer;
using System;

namespace RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LaneLayer
{
    class LaneTable
    {
        /// <summary>
        /// 规定了数据模型，请不要在其他类中直接读取数据
        /// </summary>
        public const string LaneIDNm = "LaneID";
        public const string PositionNm = "PositionID";
        public const string SerialNm = "Serial";

        public const string ArcIDNm = "ArcID";
        public const string ChangeNm = "Change";
        public const string LeftBoundaryIDNm = "LeftBoundaryID";


        public const string RightBoundaryIDNm = "RightBoundaryID";
        public const string VehClassesNm = "VehClasses";
        public const string LaneClosedNm = "LaneClosed";

        public const string WidthNm = "Width";
        public const string OtherNm = "Other";


        public ITable laneTable;
        public int LaneID;

        public LaneTable(ITable tableConnector, int laneID)
        {
            laneTable = tableConnector;
            LaneID = laneID;
        }

        public IRow GetRow()
        {
            IQueryFilter queryFilter = new QueryFilterClass();
            queryFilter.WhereClause = String.Format("{0}={1}",LaneIDNm, LaneID);
            ICursor cursor = laneTable.Search(queryFilter, false);
            IRow row = cursor.NextRow();
            if (row != null)
            {
                return row;
            }
            else
            {
                return null;
            }
        }

        public LaneEntity GetEntity()
        {
            IRow row = GetRow();
            LaneEntity laneEty = new LaneEntity();
            if (row != null)
            {
                laneEty.LaneID = LaneID;
                if (laneTable.FindField(ChangeNm) > 0)
                    laneEty.Change = Convert.ToString(row.get_Value(laneTable.FindField(ChangeNm)));
                if (laneTable.FindField(PositionNm) > 0)
                    laneEty.Position = Convert.ToInt32(row.get_Value(laneTable.FindField(PositionNm)));
                if (laneTable.FindField(LeftBoundaryIDNm) > 0)
                    laneEty.LeftBoundaryID = Convert.ToInt32(row.get_Value(laneTable.FindField(LeftBoundaryIDNm)));

                if (laneTable.FindField(RightBoundaryIDNm) > 0)
                    laneEty.RightBoundaryID = Convert.ToInt32(row.get_Value(laneTable.FindField(RightBoundaryIDNm)));
                if (laneTable.FindField(ArcIDNm) > 0)
                    laneEty.ArcID = Convert.ToInt32(row.get_Value(laneTable.FindField(ArcIDNm)));
                if (laneTable.FindField(VehClassesNm) > 0)
                    laneEty.VehClasses = Convert.ToString(row.get_Value(laneTable.FindField(VehClassesNm)));

                if (laneTable.FindField(LaneClosedNm) > 0)
                    laneEty.LaneClosed = Convert.ToInt32(row.get_Value(laneTable.FindField(LaneClosedNm)));
                if (laneTable.FindField(OtherNm) > 0)
                    laneEty.LaneClosed = Convert.ToInt32(row.get_Value(laneTable.FindField(OtherNm)));
                if (laneTable.FindField(WidthNm) > 0)
                    laneEty.Width = Convert.ToDouble(row.get_Value(laneTable.FindField(WidthNm)));

            }
            return laneEty;
        }

        /// <summary>
        /// 获取车道长度
        /// </summary>
        /// <param name="propertySet"></param>
        /// <param name="arcID"></param>
        /// <param name="projectPoint"></param>
        /// <param name="laneLenght"></param>
        public void GetLaneLength(Arc arc, ref IPoint projectPoint, ref double laneLenght)
        {
            
            IFeature arcFeature = arc.GetArcFeature();
            ArcEntity arcEty = arc.GetArcEty(arcFeature);
            
            //对应的LinkID
            int linkID = 0;
            linkID = arcEty.LinkID;
            
            IFeatureClass pFeaClsLink = (arc.FeaClsArc.FeatureDataset.Workspace as IFeatureWorkspace).OpenFeatureClass(LinkEntity.LinkName);
            Link link = new Link(pFeaClsLink,linkID);
            IFeature linkFeature = link.GetFeature();

            LinkMasterEntity linkMasterEty = new LinkMasterEntity();
            linkMasterEty = link.GetEntity(linkFeature);
            LinkEntity linkEty = new LinkEntity();
            linkEty = linkEty.Copy(linkMasterEty);
            
            IPolyline preLinkLine = linkFeature.Shape as IPolyline;

            //Arc的前后方的Node的ID，其实就是link的两个端点
            int frontNodeID = 0;
            int behindNodeID = 0;

            LogicalConnection.GetArcCorresponseNodes(arc, link, ref frontNodeID, ref behindNodeID);

            IFeatureClass pFeaClsNode = (arc.FeaClsArc.FeatureDataset.Workspace as IFeatureWorkspace).OpenFeatureClass(NodeEntity.NodeName);
            Node frontNode = new Node(pFeaClsNode, frontNodeID, null);
            Node behindNode = new Node(pFeaClsNode, frontNodeID, null);


            IFeature frontNodeFeature = frontNode.GetFeature();

            NodeEntity frontNodeEty = frontNode.GetNodeMasterEty(frontNodeFeature) as NodeEntity;

            int frontNodeType = frontNodeEty.NodeType;

            //如果是交叉口Link
            if (frontNodeType == 1)
            {
                //IFeature stopLine = FeatureManager.GetFeatureByID(_pFeaClStopLine, arcID, "RelArcID");

                //IPolyline stoplineLine = stopLine.Shape as IPolyline;
                ////停车线的一个端点
                //IPoint endPoint = stoplineLine.FromPoint;
                //projectPoint = GeoManager.GetNearestPointOnLine(endPoint, preLinkLine);

                //IFeature behindNodeFeature = FeatureManager.GetFeatureByID(_pFeatClsNode, behindNodeID, "NodeID");
                //IPoint behindPoint = behindNodeFeature.Shape as IPoint;
                ////原始车道长
                //laneLenght = GeoManager.GetDistance2Point(projectPoint, behindPoint);
            }
            //非交叉口Link
            else if (frontNodeType == 0)
            {
                laneLenght = preLinkLine.Length;
            }
        }


        /// <summary>
        /// 更新车道数据
        /// </summary>
        /// <param name="laneTable"></param>lane表
        /// <param name="updateLaneEty"></param>更新车道实体
        public void InsertLane(LaneEntity updateLaneEty)
        {
            IDataset dataset = laneTable as IDataset;
            IWorkspace ws = dataset.Workspace;
            IWorkspaceEdit workspceEdit = ws as IWorkspaceEdit;
            workspceEdit.StartEditing(false);
            workspceEdit.StartEditOperation();
            ICursor cursor;
            IRowBuffer newRowBuffer = laneTable.CreateRowBuffer();
            cursor = laneTable.Insert(true);
            IRow newRow = newRowBuffer as IRow;

            if (laneTable.FindField(LaneIDNm) >= 0)
                newRow.set_Value(laneTable.FindField(LaneIDNm), updateLaneEty.LaneID);
            if (laneTable.FindField(PositionNm) >= 0)
                newRow.set_Value(laneTable.FindField(PositionNm), updateLaneEty.Position);

            if (laneTable.FindField(ChangeNm) >= 0)
                newRow.set_Value(laneTable.FindField(ChangeNm), updateLaneEty.Change);
            if (laneTable.FindField(ArcIDNm) >= 0)
                newRow.set_Value(laneTable.FindField(ArcIDNm), updateLaneEty.ArcID);
            if (laneTable.FindField(LeftBoundaryIDNm) >= 0)
                newRow.set_Value(laneTable.FindField(LeftBoundaryIDNm), updateLaneEty.LeftBoundaryID);

            if (laneTable.FindField(RightBoundaryIDNm) >= 0)
                newRow.set_Value(laneTable.FindField(RightBoundaryIDNm), updateLaneEty.RightBoundaryID);
            if (laneTable.FindField(VehClassesNm) >= 0)
                newRow.set_Value(laneTable.FindField(VehClassesNm), updateLaneEty.VehClasses);
            if (laneTable.FindField(LaneClosedNm) >= 0)
                newRow.set_Value(laneTable.FindField(LaneClosedNm), updateLaneEty.LaneClosed);

            if (laneTable.FindField(WidthNm) >= 0)
                newRow.set_Value(laneTable.FindField(WidthNm), updateLaneEty.Width);
            if (laneTable.FindField(OtherNm) >= 0)
                newRow.set_Value(laneTable.FindField(OtherNm), updateLaneEty.Other);


            cursor.InsertRow(newRowBuffer);
            workspceEdit.StopEditOperation();
            workspceEdit.StopEditing(true);

        }


        public void UpdateLane(LaneEntity updateLaneConnEty, int prelaneID)
        {
            LaneEntity preLaneEty = new LaneEntity();
            preLaneEty = GetEntity();
            IDataset dataset = laneTable as IDataset;
            IWorkspace ws = dataset.Workspace;
            IWorkspaceEdit workspceEdit = ws as IWorkspaceEdit;
            workspceEdit.StartEditing(false);
            workspceEdit.StartEditOperation();
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = String.Format("LaneID={0}", preLaneEty.LaneID);
            ICursor pFeatureCuror1 = laneTable.Search(filter, false);
            IRow modifiedRowArc = pFeatureCuror1.NextRow();

            if (updateLaneConnEty.ArcID != preLaneEty.ArcID)
            {
                modifiedRowArc.set_Value(laneTable.FindField(ArcIDNm), updateLaneConnEty.ArcID);
            }
            if (!updateLaneConnEty.Change.Equals(preLaneEty.Change))
            {
                modifiedRowArc.set_Value(laneTable.FindField(ChangeNm), updateLaneConnEty.Change);
            }
            if (!updateLaneConnEty.LeftBoundaryID.Equals(preLaneEty.LeftBoundaryID))
            {
                modifiedRowArc.set_Value(laneTable.FindField(LeftBoundaryIDNm), updateLaneConnEty.LeftBoundaryID);
            }
            

            if (!updateLaneConnEty.RightBoundaryID.Equals(preLaneEty.RightBoundaryID))
            {
                modifiedRowArc.set_Value(laneTable.FindField(RightBoundaryIDNm), updateLaneConnEty.RightBoundaryID);
            }
            if (updateLaneConnEty.LaneClosed != preLaneEty.LaneClosed)
            {
                modifiedRowArc.set_Value(laneTable.FindField(LaneClosedNm), updateLaneConnEty.LaneClosed);
            }
            if (!updateLaneConnEty.VehClasses.Equals(preLaneEty.VehClasses))
            {
                modifiedRowArc.set_Value(laneTable.FindField(VehClassesNm), updateLaneConnEty.VehClasses);
            }


            if (updateLaneConnEty.Width != preLaneEty.Width)
            {
                modifiedRowArc.set_Value(laneTable.FindField(WidthNm), updateLaneConnEty.Width);
            }
            if (updateLaneConnEty.Other != preLaneEty.Other)
            {
                modifiedRowArc.set_Value(laneTable.FindField(OtherNm), updateLaneConnEty.Other);
            }

            if (updateLaneConnEty.Position != preLaneEty.Position)
            {
                modifiedRowArc.set_Value(laneTable.FindField(PositionNm), updateLaneConnEty.Position);
            }

            modifiedRowArc.Store();

            workspceEdit.StopEditOperation();
            workspceEdit.StopEditing(true);
        }



        public void DeleteLane()
        {
            IDataset dataset = laneTable as IDataset;
            IWorkspace ws = dataset.Workspace;
            IWorkspaceEdit workspceEdit = ws as IWorkspaceEdit;
            workspceEdit.StartEditing(false);
            workspceEdit.StartEditOperation();

            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = String.Format("LaneID={0}", LaneID);
            ICursor cursor = laneTable.Search(filter, false);
            IRow deleteRow = cursor.NextRow();

            deleteRow.Delete();
            workspceEdit.StopEditOperation();
            workspceEdit.StopEditing(true);
        }
    }
}
