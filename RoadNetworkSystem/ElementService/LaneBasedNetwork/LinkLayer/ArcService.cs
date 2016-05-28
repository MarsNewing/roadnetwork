using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.ADO.Access;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.GIS;
using RoadNetworkSystem.GIS.GeoDatabase.Dataset;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.Connection;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LaneLayer;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Text;
using System.Windows.Forms;

namespace RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LinkLayer
{
    class ArcService
    {

        public const double ARC_CUT_PERCENTAGE = 0.05;


        public struct StrctCrtArc
        {
            /// <summary>
            /// 所属的link
            /// </summary>
            public int LinkID;
            /// <summary>
            /// 距离起点的长度
            /// </summary>
            public double ALen;
            /// <summary>
            /// Arc终点距离起点的长度
            /// </summary>
            public double BLen;
            /// <summary>
            /// 与Link的相对方向，-1相反，1相同
            /// </summary>
            public int Dir;
            /// <summary>
            /// 车道个数
            /// </summary>
            public int LaneNum;
        }

        public IFeatureClass FeaClsArc;

        public int ArcID;

        private static OleDbConnection _conn;
        

        public IFeature ArcFeature;
        public ArcService(IFeatureClass pFeaClsArc,int arcID)
        {
            FeaClsArc = pFeaClsArc;
            ArcID = arcID;
            ArcFeature = GetArcFeature();
            if (_conn == null)
            {
                _conn = AccessHelper.OpenConnection(FeaClsArc.FeatureDataset.Workspace.PathName);
            }
        }


        public static int GetArcID(int linkId, int dir)
        {
            if (dir == Link.FLOWDIR_SAME)
            {
                return linkId * 10;
            }
            else
            {
                return linkId * 10 + 1;
            }
        }

        /// <summary>
        /// 从Link创建出Arc
        /// </summary>
        /// <param name="link"></param>
        /// <param name="arcFlowDir"></param>
        /// <param name="laneNum"></param>
        /// <param name="linkLine"></param>
        /// <returns></returns>
        public Arc CreateArcFromLink(int linkId,
            int arcFlowDir,
            int laneNum,
            IPolyline linkLine)
        {
            Arc arcEty = new Arc();
            #region ++++++++++++++++++++++++++保存Arc+++++++++++++++++++++++++++++++
            arcEty.ArcID = 0;
            arcEty.LinkID = linkId;
            arcEty.FlowDir = arcFlowDir;
            arcEty.LaneNum = laneNum;
            arcEty.Other = 0;


            IPolyline arcLine = LineHelper.CreateLineByLRS(linkLine,
                Lane.LANE_WEIDTH * arcEty.FlowDir * arcEty.LaneNum / 2,
                linkLine.Length*ARC_CUT_PERCENTAGE,
                linkLine.Length * ARC_CUT_PERCENTAGE);

            //获取截头截尾的距离
            ArcService arc = new ArcService(FeaClsArc, 0);

            if (arcFlowDir == Link.FLOWDIR_SAME)
            {
                IFeature sameArcFea = arc.CreateArc(arcEty, arcLine);
                return arc.GetArcEty(sameArcFea);
            }
            else
            {
                arcLine.ReverseOrientation();
                IFeature oppArcFea = arc.CreateArc(arcEty, arcLine);
                return arc.GetArcEty(oppArcFea);
            }
            #endregion ++++++++++++++++++++++++++保存Arc+++++++++++++++++++++++++++++++
        }


        /// <summary>
        /// 生成一个Arc
        /// </summary>
        /// <param name="arc"></param>
        /// <param name="line"></param>
        public IFeature CreateArc(Arc arc, IPolyline line)
        {
            IFeature newFea = FeaClsArc.CreateFeature();
            if (ArcID > 0)
            {
                if (FeaClsArc.FindField(Arc.ArcIDNm) >= 0)
                    newFea.set_Value(FeaClsArc.FindField(Arc.ArcIDNm), arc.ArcID);
            }
            else
            {
                if (FeaClsArc.FindField(Arc.ArcIDNm) >= 0)
                {
                    int arcId = GetArcID(arc.LinkID,arc.FlowDir);                    
                    newFea.set_Value(FeaClsArc.FindField(Arc.ArcIDNm), arcId);
                }
            }
          
            if (FeaClsArc.FindField(Arc.FlowDirNm) >= 0)
                newFea.set_Value(FeaClsArc.FindField(Arc.FlowDirNm), arc.FlowDir);

            if (FeaClsArc.FindField(Arc.LaneNumNm) >= 0)
                newFea.set_Value(FeaClsArc.FindField(Arc.LaneNumNm), arc.LaneNum);

            if (FeaClsArc.FindField(Arc.LinkIDNm) >= 0)
                newFea.set_Value(FeaClsArc.FindField(Arc.LinkIDNm), arc.LinkID);

            if (FeaClsArc.FindField(Arc.OtherNm) >= 0)
                newFea.set_Value(FeaClsArc.FindField(Arc.OtherNm), arc.Other);
            newFea.Shape = line;
            newFea.Store();

            return newFea;
        }


        /// <summary>
        ///创建一个Arc的几何
        /// </summary>
        /// <param name="linkLine"></param>参考线
        /// <param name="curcrtArc"></param>
        /// <returns></returns>
        public IPolyline CreateArcShape(IPolyline linkLine, StrctCrtArc curcrtArc)
        {
            IPolyline arcShape;
            double linkLen = linkLine.Length;

            if (curcrtArc.ALen > linkLen || curcrtArc.BLen > linkLen || Math.Abs(curcrtArc.ALen - curcrtArc.BLen) < 0.1)
                return null;

            if (curcrtArc.ALen > curcrtArc.BLen)
            {
                double tem = curcrtArc.BLen;
                curcrtArc.BLen = curcrtArc.ALen;
                curcrtArc.ALen = tem;
            }

            arcShape = LineHelper.CreateLineByLRS(linkLine, curcrtArc.Dir * curcrtArc.LaneNum * Lane.LANE_WEIDTH / 2, curcrtArc.ALen, curcrtArc.BLen);

            if (curcrtArc.Dir == -1)
            {
                arcShape.ReverseOrientation();
            }
            return arcShape;
        }


        /// <summary>
        /// 获取一个Arc要素
        /// </summary>
        /// <param name="arcID"></param>
        /// <returns></returns>
        public IFeature GetArcFeature()
        {
            IFeatureCursor cursor;
            IQueryFilter filer = new QueryFilterClass();
            filer.WhereClause = String.Format("{0}={1}", Arc.ArcIDNm, ArcID);
            cursor = FeaClsArc.Search(filer, false);
            IFeature arcFeature = cursor.NextFeature();
            if (arcFeature != null)
            {

                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                return arcFeature;
            }
            else
            {

                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                return null;
            }
            
        }

        /// <summary>
        /// 获取Arc实体
        /// </summary>
        /// <param name="arcID"></param>
        /// <returns></returns>
        public Arc GetArcEty(IFeature arcFea)
        {
            if (arcFea == null)
                return null;
            Arc arcEty = new Arc();

            if (FeaClsArc.FindField(Arc.ArcIDNm) >= 0)
                arcEty.ArcID = Convert.ToInt32(arcFea.get_Value(FeaClsArc.FindField(Arc.ArcIDNm)));

            if (FeaClsArc.FindField(Arc.FlowDirNm) >= 0)
                arcEty.FlowDir = Convert.ToInt32(arcFea.get_Value(FeaClsArc.FindField(Arc.FlowDirNm)));

            if (FeaClsArc.FindField(Arc.LinkIDNm) >= 0)
                arcEty.LinkID = Convert.ToInt32(arcFea.get_Value(FeaClsArc.FindField(Arc.LinkIDNm)));

            if (FeaClsArc.FindField(Arc.LaneNumNm) >= 0)
                arcEty.LaneNum = Convert.ToInt32(arcFea.get_Value(FeaClsArc.FindField(Arc.LaneNumNm)));

            if (FeaClsArc.FindField(Arc.OtherNm) >= 0)
                arcEty.Other = Convert.ToInt32(arcFea.get_Value(FeaClsArc.FindField(Arc.OtherNm)));

            return arcEty;
        }


        

        /// <summary>
        /// 判断是否存在要素
        /// </summary>
        /// <param name="ArcID"></param>Arc的ID
        /// <returns></returns>
        public bool ExistArcFeature()
        {
            if (ArcFeature != null)
            {
                return true;
            }
            else
            {
                return false;
            }

        }


        /// <summary>
        /// 获取通过LinkID和Dir获取Arc的相关信息
        /// </summary>
        /// <param name="mdbPath"></param>
        /// <param name="linkID"></param>
        /// <param name="ArcDir"></param>
        /// <param name="storeArcID"></param>
        /// <param name="storeLaneNum"></param>
        public IFeature GetRequiredDirArcFeature(int linkID, int ArcDir)
        {
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = Arc.LinkIDNm + "=" + linkID;

            IFeatureCursor cursor = FeaClsArc.Search(filter, false);
            IFeature temArcFea = cursor.NextFeature();
            while(temArcFea!=null)
            {
                int temDir = Convert.ToInt32(temArcFea.get_Value(temArcFea.Fields.FindField(Arc.FlowDirNm)));
                if (temDir == ArcDir)
                {
                    return temArcFea;
                }

                temArcFea = cursor.NextFeature();
            }
            return null;
        }

        public Arc GetRequiredDirArc(int linkId, int arcDir)
        {
            IFeature arcFea = GetRequiredDirArcFeature(linkId, arcDir);
            return GetArcEty(arcFea);
                 
        }

        public Arc GetSameArc(int linkId)
        {
            return GetRequiredDirArc(linkId, Link.FLOWDIR_SAME);
        }

        public Arc GetOppositionArc(int linkId)
        {
            return GetRequiredDirArc(linkId, Link.FLOWDIR_OPPOSITION);
        }
        

        /// <summary>
        /// 删除一个Arc实体
        /// </summary>
        /// <param name="arcID"></param>
        public void DeleteArcFea(int arcId)
        {
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = String.Format("{0}={1}",Arc.ArcIDNm , arcId);
            IFeatureCursor pFeatureCuror = FeaClsArc.Update(filter, false);
            IFeature arcFeature = pFeatureCuror.NextFeature();
            arcFeature.Delete();
        }

        public double GetArcWidth(IFeatureClass pFeaClsLane)
        {
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = String.Format("{0} = {1}", Arc.ArcIDNm, ArcID);
            IFeatureCursor cursor;
            cursor = pFeaClsLane.Search(filter, false);
            IFeature pFeatureLane = cursor.NextFeature();

            //当前车道的宽度
            double curWidth = 0;
            //遍历所有的Lane
            while (pFeatureLane != null)
            {
                try
                {
                    int lFld = pFeaClsLane.FindField(Lane.WidthNm);
                    double temLaneWidth = Convert.ToDouble(pFeatureLane.get_Value(lFld));
                    curWidth = curWidth + temLaneWidth;
                    pFeatureLane = cursor.NextFeature();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

            return curWidth;
        }


        public double GetLanesWidth(IFeatureClass pFeaClsLane,int serial)
        {
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = String.Format("{0} = {1}", Arc.ArcIDNm, ArcID);
            IFeatureCursor cursor;
            cursor = pFeaClsLane.Search(filter, false);
            IFeature pFeatureLane = cursor.NextFeature();

            if (pFeatureLane == null)
            {
                return Lane.LANE_WEIDTH * serial;
            }

            //当前车道的宽度
            double curWidth = 0;
            double cursorIndex = Lane.LEFT_POSITION;

            //遍历所有的Lane
            while (pFeatureLane != null)
            {
                try
                {
                    int lFld = pFeaClsLane.FindField(Lane.WidthNm);
                    double temLaneWidth = Convert.ToDouble(pFeatureLane.get_Value(lFld));
                    if (cursorIndex <= serial)
                    {
                        curWidth = curWidth + temLaneWidth;
                    }
                    else
                    {
                        break;
                    }
                    cursorIndex++;
                    pFeatureLane = cursor.NextFeature();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();

            return curWidth;
        }

        /// <summary>
        /// 找到Arc内部的所有的Lane
        /// </summary>
        /// <returns></returns>
        public List<Lane> getLanesWithinArc()
        {
            List<Lane> lanes = new List<Lane>();
            IFeature pFeatureArc = GetArcFeature();
            IFeatureClass pFeaClsLane = FeatureClassHelper.GetFeaClsInAccess(FeaClsArc.FeatureDataset.Workspace.PathName,Lane.LaneName);
            if(null == pFeaClsLane)
            {
                return null;
            }
            if(null == pFeatureArc)
            {
                return null;
            }
            Arc arc = GetArcEty(pFeatureArc);

            for (int i = Lane.LEFT_POSITION; i <= (arc.LaneNum - Lane.rightPositionOffset); i++)
            {
                int LaneId = LaneFeatureService.GetLaneID(arc.ArcID, i);
                LaneFeatureService laneFeatureService = new LaneFeatureService(pFeaClsLane, LaneId);
                IFeature pTemFeature = laneFeatureService.GetFeature();
                if(null == pTemFeature)
                {
                    continue;
                }
                Lane temLane = laneFeatureService.GetEntity(pTemFeature);
                if (null == temLane)
                {
                    continue;
                }

                lanes.Add(temLane);
            }

            if (null == lanes ||
                0 == lanes.Count)
            {
                return null;
            }
            return lanes;
        }
    }
}
