﻿using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.GIS;
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
        private double _LANEWIDTH = 3.5;

        
        public const string ArcIDNm = "ArcID";
        public const string LaneNumNm = "LaneNum";
        public const string LinkIDNm = "LinkID";

        public const string FlowDirNm = "FlowDir";
        public const string OtherNm = "Other";

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

        public IFeature ArcFeature;
        public ArcService(IFeatureClass pFeaClsArc,int arcID)
        {
            FeaClsArc = pFeaClsArc;
            ArcID = arcID;
            ArcFeature = GetArcFeature();
        }


        /// <summary>
        /// 生成一个Arc
        /// </summary>
        /// <param name="arcEty"></param>
        /// <param name="line"></param>
        public IFeature CreateArc(Arc arcEty, IPolyline line)
        {
            IFeature newFea = FeaClsArc.CreateFeature();
            if (ArcID > 0)
            {
                if (FeaClsArc.FindField(ArcIDNm) >= 0)
                    newFea.set_Value(FeaClsArc.FindField(ArcIDNm), arcEty.ArcID);
            }
            else
            {
                if (FeaClsArc.FindField(ArcIDNm) >= 0)
                    newFea.set_Value(FeaClsArc.FindField(ArcIDNm), newFea.OID);
            }
          
            if (FeaClsArc.FindField(FlowDirNm) >= 0)
                newFea.set_Value(FeaClsArc.FindField(FlowDirNm), arcEty.FlowDir);

            if (FeaClsArc.FindField(LaneNumNm) >= 0)
                newFea.set_Value(FeaClsArc.FindField(LaneNumNm), arcEty.LaneNum);

            if (FeaClsArc.FindField(LinkIDNm) >= 0)
                newFea.set_Value(FeaClsArc.FindField(LinkIDNm), arcEty.LinkID);

            if (FeaClsArc.FindField(OtherNm) >= 0)
                newFea.set_Value(FeaClsArc.FindField(OtherNm), arcEty.Other);
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

            arcShape = LineHelper.CreateLineByLRS(linkLine, curcrtArc.Dir * curcrtArc.LaneNum * _LANEWIDTH / 2, curcrtArc.ALen, curcrtArc.BLen);

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
            filer.WhereClause = String.Format("{0}={1}", ArcIDNm, ArcID);
            cursor = FeaClsArc.Search(filer, false);
            IFeature arcFeature = cursor.NextFeature();
            if (arcFeature != null)
            {
                return arcFeature;
            }
            else
            {
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
            Arc arcEty = new Arc();

            if (FeaClsArc.FindField(ArcIDNm) >= 0)
                arcEty.ArcID = Convert.ToInt32(arcFea.get_Value(FeaClsArc.FindField(ArcIDNm)));

            if (FeaClsArc.FindField(FlowDirNm) >= 0)
                arcEty.FlowDir = Convert.ToInt32(arcFea.get_Value(FeaClsArc.FindField(FlowDirNm)));

            if (FeaClsArc.FindField(LinkIDNm) >= 0)
                arcEty.LinkID = Convert.ToInt32(arcFea.get_Value(FeaClsArc.FindField(LinkIDNm)));

            if (FeaClsArc.FindField(LaneNumNm) >= 0)
                arcEty.LaneNum = Convert.ToInt32(arcFea.get_Value(FeaClsArc.FindField(LaneNumNm)));

            if (FeaClsArc.FindField(OtherNm) >= 0)
                arcEty.Other = Convert.ToInt32(arcFea.get_Value(FeaClsArc.FindField(OtherNm)));

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
        public IFeature GetArcInfo(int linkID, int ArcDir)
        {
            IWorkspace pWs = (FeaClsArc as IDataset).Workspace;
            string mdbPath = pWs.PathName;

            OleDbConnection Conn = new OleDbConnection();
            string conn_str = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + mdbPath + "; Persist Security Info=False";
            Conn = new OleDbConnection(conn_str);
            Conn.Open();
            string readStr = String.Format("select * from {0} where LinkID = {1} and FlowDir = {2}", "Arc", linkID, ArcDir);
            OleDbCommand readCmd = new OleDbCommand();
            readCmd.CommandText = readStr;
            readCmd.Connection = Conn;
            OleDbDataReader read;
            read = readCmd.ExecuteReader();
            IFeature featureArc = null;
            while (read.Read())
            {
                ArcID = Convert.ToInt32(read["ArcID"]);
                break;
            }
            read.Close();
            Conn.Close();
            Conn.Dispose();
            featureArc = GetArcFeature();
            return featureArc;
        }

        public IFeature QueryArcFeatureByRule(string QueryStr)
        {

            IFeatureCursor cursor = null;
            IQueryFilter filer = new QueryFilterClass();
            filer.WhereClause = QueryStr;
            filer.SubFields = "LinkID,FlowDir";
            try
            {

                cursor = FeaClsArc.Search(filer, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());

            }
            IFeature arcFeature=null;
                arcFeature= cursor.NextFeature();
            if (arcFeature != null)
            {
                int arcID = Convert.ToInt32(arcFeature.get_Value(arcFeature.Fields.FindField(ArcIDNm)));

                return arcFeature;
                
            }
            else
            {
                return null;
            }
        }

        public Arc GetArcEtyByRule(string QueryStr)
        {
            IFeature pFeature = QueryArcFeatureByRule(QueryStr);
            Arc arcEty = new Arc();
            arcEty = GetArcEty(pFeature);
            return arcEty;
        }

        /// <summary>
        /// 删除一个Arc实体
        /// </summary>
        /// <param name="arcID"></param>
        public void DeleteArcFea()
        {
            IDataset pDataset = FeaClsArc as IDataset;
            IWorkspace pWS = pDataset.Workspace;
            IWorkspaceEdit pWorkspaceEdit = pWS as IWorkspaceEdit;
            pWorkspaceEdit.StartEditing(false);
            pWorkspaceEdit.StartEditOperation();
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = String.Format("{0}={1}",ArcIDNm ,ArcID);
            IFeatureCursor pFeatureCuror = FeaClsArc.Update(filter, false);
            IFeature arcFeature = pFeatureCuror.NextFeature();

            pFeatureCuror.DeleteFeature();

            pWorkspaceEdit.StopEditOperation();
            pWorkspaceEdit.StopEditing(true);
        }

        public double GetArcWidth(IFeatureClass pFeaClsLane)
        {
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = String.Format("{0} = {1}", ArcService.ArcIDNm, ArcID);
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
                    int lFld = pFeaClsLane.FindField(LaneFeatureService.WidthNm);
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
            filter.WhereClause = String.Format("{0} = {1}", ArcService.ArcIDNm, ArcID);
            IFeatureCursor cursor;
            cursor = pFeaClsLane.Search(filter, false);
            IFeature pFeatureLane = cursor.NextFeature();

            //当前车道的宽度
            double curWidth = 0;
            double cursorIndex = 0;

            //遍历所有的Lane
            while (pFeatureLane != null)
            {
                try
                {
                    int lFld = pFeaClsLane.FindField(LaneFeatureService.WidthNm);
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
    }
}