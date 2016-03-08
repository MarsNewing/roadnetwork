using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.ADO.Access;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.Road;
using RoadNetworkSystem.GIS;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.Connection;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LinkLayer;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Text;

namespace RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LaneLayer
{
    class LaneFeatureService
    {
        
        /// <summary>
        /// 规定了数据模型，请不要在其他类中直接读取数据
        /// </summary>
        

        public IFeatureClass FeaClsLane;
        public int LaneID;
        private static OleDbConnection _conn;


        public enum LaneChange
        {
            /// <summary>
            /// 均可变向
            /// </summary>
            Both,
            /// <summary>
            /// 左可变
            /// </summary>
            Left,
            /// <summary>
            /// 右可变
            /// </summary>
            Right,
            /// <summary>
            /// 两侧均不可以变
            /// </summary>
            None
        }

        public LaneFeatureService(IFeatureClass pFeaClsLane, int laneID)
        {
            FeaClsLane = pFeaClsLane;
            LaneID = laneID;

            if (_conn == null)
            {
                _conn = AccessHelper.OpenConnection(pFeaClsLane.FeatureDataset.Workspace.PathName);
            }
        }

        public IFeature GetFeature()
        {
            IQueryFilter queryFilter = new QueryFilterClass();
            queryFilter.WhereClause = String.Format("{0}={1}", Lane.LaneIDNm, LaneID);
            IFeatureCursor cursor = FeaClsLane.Search(queryFilter, false);
            IFeature pFeature = cursor.NextFeature();
            if (pFeature != null)
            {
                return pFeature;
            }
            else
            {
                return null;
            }
        }


        public static int GetLaneID(int arcId, int position)
        {
            return arcId * 10 + position;
        }

        public IFeature QueryFeatureBuRule(int arcID,int serial)
        {

            string readStr = String.Format("select * from Lane where ArcID = {0} and [Position] = {1}", arcID, serial);

            
            OleDbCommand readCmd = new OleDbCommand();
            readCmd.CommandText = readStr;
            readCmd.Connection = _conn;
            OleDbDataReader read;
            read = readCmd.ExecuteReader();
            IFeature featureLane = null;
            while (read.Read())
            {
                LaneID = Convert.ToInt32(read[Lane.LaneIDNm]);
                break;
            }
            read.Close();

            featureLane = GetFeature();
            return featureLane;
        }

        public Lane GetEntity(IFeature pFeature)
        {
            Lane laneEty = new Lane();
            if (pFeature != null)
            {
                if (FeaClsLane.FindField(Lane.LaneIDNm) > 0)
                    laneEty.LaneID = Convert.ToInt32(pFeature.get_Value(FeaClsLane.FindField(Lane.LaneIDNm)));
                
                if (FeaClsLane.FindField(Lane.ChangeNm) > 0)
                    laneEty.Change = Convert.ToString(pFeature.get_Value(FeaClsLane.FindField(Lane.ChangeNm)));
                if (FeaClsLane.FindField(Lane.PositionNm) > 0)
                    laneEty.Position = Convert.ToInt32(pFeature.get_Value(FeaClsLane.FindField(Lane.PositionNm)));
                if (FeaClsLane.FindField(Lane.LeftBoundaryIDNm) > 0)
                    laneEty.LeftBoundaryID = Convert.ToInt32(pFeature.get_Value(FeaClsLane.FindField(Lane.LeftBoundaryIDNm)));

                if (FeaClsLane.FindField(Lane.RightBoundaryIDNm) > 0)
                    laneEty.RightBoundaryID = Convert.ToInt32(pFeature.get_Value(FeaClsLane.FindField(Lane.RightBoundaryIDNm)));
                if (FeaClsLane.FindField(Lane.ArcIDNm) > 0)
                    laneEty.ArcID = Convert.ToInt32(pFeature.get_Value(FeaClsLane.FindField(Lane.ArcIDNm)));
                if (FeaClsLane.FindField(Lane.VehClassesNm) > 0)
                    laneEty.VehClasses = Convert.ToString(pFeature.get_Value(FeaClsLane.FindField(Lane.VehClassesNm)));

                if (FeaClsLane.FindField(Lane.LaneClosedNm) > 0)
                    laneEty.LaneClosed = Convert.ToInt32(pFeature.get_Value(FeaClsLane.FindField(Lane.LaneClosedNm)));
                if (FeaClsLane.FindField(Lane.OtherNm) > 0)
                    laneEty.LaneClosed = Convert.ToInt32(pFeature.get_Value(FeaClsLane.FindField(Lane.OtherNm)));
                if (FeaClsLane.FindField(Lane.WidthNm) > 0)
                    laneEty.Width = Convert.ToDouble(pFeature.get_Value(FeaClsLane.FindField(Lane.WidthNm)));

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
        public void GetLaneLength(ArcService arc, ref IPoint projectPoint, ref double laneLenght)
        {
            
            IFeature arcFeature = arc.GetArcFeature();
            Arc arcEty = arc.GetArcEty(arcFeature);
            
            //对应的LinkID
            int linkID = 0;
            linkID = arcEty.LinkID;
            
            IFeatureClass pFeaClsLink = (arc.FeaClsArc.FeatureDataset.Workspace as IFeatureWorkspace).OpenFeatureClass(Link.LinkName);
            LinkService link = new LinkService(pFeaClsLink,linkID);

            IFeature linkFeature = link.GetFeature();
            LinkMaster linkMasterEty = new LinkMaster();
            linkMasterEty = link.GetEntity(linkFeature);
            Link linkEty = new Link();
            linkEty = linkEty.Copy(linkMasterEty);
            IPolyline preLinkLine = linkFeature.Shape as IPolyline;

            //Arc的前后方的Node的ID，其实就是link的两个端点
            int frontNodeID = 0;
            int behindNodeID = 0;

            LogicalConnection.GetArcCorresponseNodes(arc, link, ref frontNodeID, ref behindNodeID);

            IFeatureClass pFeaClsNode = (arc.FeaClsArc.FeatureDataset.Workspace as IFeatureWorkspace).OpenFeatureClass(Node.NodeName);
            NodeService frontNode = new NodeService(pFeaClsNode, frontNodeID, null);
            NodeService behindNode = new NodeService(pFeaClsNode, frontNodeID, null);


            IFeature frontNodeFeature = frontNode.GetFeature();

            Node frontNodeEty = frontNode.GetNodeMasterEty(frontNodeFeature) as Node;

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
        /// <param name="FeaClsLane"></param>lane表
        /// <param name="updateLaneEty"></param>更新车道实体
        public IFeature CreateLane(Lane updateLaneEty,IPolyline line)
        {

            IFeature laneFeature= FeaClsLane.CreateFeature();

            if (updateLaneEty.LaneID > 0)
            {
                if (FeaClsLane.FindField(Lane.LaneIDNm) >= 0)
                    laneFeature.set_Value(FeaClsLane.FindField(Lane.LaneIDNm), updateLaneEty.LaneID);
            }
            else
            {
                if (FeaClsLane.FindField(Lane.LaneIDNm) >= 0)
                    laneFeature.set_Value(FeaClsLane.FindField(Lane.LaneIDNm), laneFeature.OID);
            }
            if (FeaClsLane.FindField(Lane.PositionNm) >= 0)
                laneFeature.set_Value(FeaClsLane.FindField(Lane.PositionNm), updateLaneEty.Position);


            if (FeaClsLane.FindField(Lane.ChangeNm) >= 0)
                laneFeature.set_Value(FeaClsLane.FindField(Lane.ChangeNm), updateLaneEty.Change);
            if (FeaClsLane.FindField(Lane.ArcIDNm) >= 0)
                laneFeature.set_Value(FeaClsLane.FindField(Lane.ArcIDNm), updateLaneEty.ArcID);
            if (FeaClsLane.FindField(Lane.LeftBoundaryIDNm) >= 0)
                laneFeature.set_Value(FeaClsLane.FindField(Lane.LeftBoundaryIDNm), updateLaneEty.LeftBoundaryID);

            if (FeaClsLane.FindField(Lane.RightBoundaryIDNm) >= 0)
                laneFeature.set_Value(FeaClsLane.FindField(Lane.RightBoundaryIDNm), updateLaneEty.RightBoundaryID);
            if (FeaClsLane.FindField(Lane.VehClassesNm) >= 0)
                laneFeature.set_Value(FeaClsLane.FindField(Lane.VehClassesNm), updateLaneEty.VehClasses);
            if (FeaClsLane.FindField(Lane.LaneClosedNm) >= 0)
                laneFeature.set_Value(FeaClsLane.FindField(Lane.LaneClosedNm), updateLaneEty.LaneClosed);

            if (FeaClsLane.FindField(Lane.WidthNm) >= 0)
                laneFeature.set_Value(FeaClsLane.FindField(Lane.WidthNm), updateLaneEty.Width);
            if (FeaClsLane.FindField(Lane.OtherNm) >= 0)
                laneFeature.set_Value(FeaClsLane.FindField(Lane.OtherNm), updateLaneEty.Other);

            laneFeature.Shape = line;
            laneFeature.Store();

            return laneFeature;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="updateLaneConnEty"></param>
        /// <param name="prelaneID"></param>
        public void UpdateLane(Lane updateLaneConnEty, int prelaneID)
        {
            IFeatureCursor cursor;
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = String.Format("{0} = {1}", Lane.LaneIDNm, prelaneID);

            cursor= FeaClsLane.Search(filter, false);
            IFeature laneFea = cursor.NextFeature();
            
            if (laneFea != null)
            {
                Lane preLaneEty = GetEntity(laneFea);
                if (updateLaneConnEty.ArcID != preLaneEty.ArcID)
                {
                    laneFea.set_Value(FeaClsLane.FindField(Lane.ArcIDNm), updateLaneConnEty.ArcID);
                }
                if (!updateLaneConnEty.Change.Equals(preLaneEty.Change))
                {
                    laneFea.set_Value(FeaClsLane.FindField(Lane.ChangeNm), updateLaneConnEty.Change);
                }
                if (!updateLaneConnEty.LeftBoundaryID.Equals(preLaneEty.LeftBoundaryID))
                {
                    laneFea.set_Value(FeaClsLane.FindField(Lane.LeftBoundaryIDNm), updateLaneConnEty.LeftBoundaryID);
                }


                if (!updateLaneConnEty.RightBoundaryID.Equals(preLaneEty.RightBoundaryID))
                {
                    laneFea.set_Value(FeaClsLane.FindField(Lane.RightBoundaryIDNm), updateLaneConnEty.RightBoundaryID);
                }
                if (updateLaneConnEty.LaneClosed != preLaneEty.LaneClosed)
                {
                    laneFea.set_Value(FeaClsLane.FindField(Lane.LaneClosedNm), updateLaneConnEty.LaneClosed);
                }
                if (!updateLaneConnEty.VehClasses.Equals(preLaneEty.VehClasses))
                {
                    laneFea.set_Value(FeaClsLane.FindField(Lane.VehClassesNm), updateLaneConnEty.VehClasses);
                }

                if (updateLaneConnEty.Width != preLaneEty.Width)
                {
                    laneFea.set_Value(FeaClsLane.FindField(Lane.WidthNm), updateLaneConnEty.Width);
                }
                if (updateLaneConnEty.Other != preLaneEty.Other)
                {
                    laneFea.set_Value(FeaClsLane.FindField(Lane.OtherNm), updateLaneConnEty.Other);
                }

                if (updateLaneConnEty.Position != preLaneEty.Position)
                {
                    laneFea.set_Value(FeaClsLane.FindField(Lane.PositionNm), updateLaneConnEty.Position);
                }

                laneFea.Store();
            }

            
        }

        /// <summary>
        /// 删除
        /// </summary>
        public void DeleteLane()
        {
            IDataset dataset = FeaClsLane as IDataset;
            IWorkspace ws = dataset.Workspace;
            IWorkspaceEdit workspceEdit = ws as IWorkspaceEdit;
            workspceEdit.StartEditing(false);
            workspceEdit.StartEditOperation();

            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = String.Format("LaneID={0}", LaneID);
            IFeatureCursor cursor = FeaClsLane.Search(filter, false);
            cursor.DeleteFeature();
            workspceEdit.StopEditOperation();
            workspceEdit.StopEditing(true);
        }

    

    }
}
