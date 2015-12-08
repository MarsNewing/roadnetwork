﻿using ESRI.ArcGIS.Geodatabase;
using RoadNetworkSystem.DataModel.Road;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.NetworkElement.RoadLayer
{
    class Road
    {
        /// <summary>
        /// 规定了数据模型，请不要在其他类中直接读取数据
        /// </summary>
        public const string RoadIDNm = "RoadID";
        public const string RoadTypeNm = "RoadType";
        public const string RoadNameNm = "RoadName";
        public const string OtherNm = "Other";

        public IFeatureClass FeaClsRoad;
        public int RoadID;
        public IFeature FeaRoad;

        public Road(IFeatureClass pFeaCls,int roadID)
        {
            FeaClsRoad = pFeaCls;
            RoadID = roadID;
            FeaRoad = GetFeature();
        }

        public IFeature GetFeature()
        {
            IFeatureCursor cursor;
            IQueryFilter filer = new QueryFilterClass();
            filer.WhereClause = String.Format("{0}={1}", RoadIDNm, RoadID);
            cursor = FeaClsRoad.Search(filer, false);
            IFeature nodeFeature = cursor.NextFeature();
            if (nodeFeature != null)
            {
                return nodeFeature;
            }
            else
            {
                return null;
            }
        }


        public RoadEntity GetEntity(IFeature FeaRoad)
        {
            RoadEntity rsEty = new RoadEntity();

            try
            {
                if (FeaRoad.Fields.FindField(RoadIDNm) >= 0)
                    rsEty.RoadID = Convert.ToInt32(FeaRoad.get_Value(FeaRoad.Fields.FindField(RoadIDNm)));

                if (FeaRoad.Fields.FindField(RoadNameNm) >= 0)
                    rsEty.RoadName = Convert.ToString(FeaRoad.get_Value(FeaRoad.Fields.FindField(RoadNameNm)));
                if (FeaRoad.Fields.FindField(RoadTypeNm) >= 0)
                    rsEty.RoadType = Convert.ToInt32(FeaRoad.get_Value(FeaRoad.Fields.FindField(RoadTypeNm)));

                //if (FeaRoad.Fields.FindField(OtherNm) >= 0)
                //    rsEty.Other = Convert.ToInt32(FeaRoad.get_Value(FeaRoad.Fields.FindField(OtherNm)));

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return rsEty;
        }
    }
}
