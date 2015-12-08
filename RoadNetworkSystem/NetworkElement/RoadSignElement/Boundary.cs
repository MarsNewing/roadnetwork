﻿using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.DataModel.RoadSign;
using System;

namespace RoadNetworkSystem.NetworkElement.RoadSignElement
{
    class Boundary
    {
        /// <summary>
        /// 规定了数据模型，请不要在其他类中直接读取数据
        /// </summary>
        public const string BoundaryIDNm = "BoundaryID";
        public const string StyleIDNm = "StyleID";
        public const string DirNm = "Dir";

        public const string OtherNm = "Other";
        public IFeatureClass FeaClsBoundary;
        public int BouddaryID;
        public Boundary(IFeatureClass pFeaClsBoun, int boundaryID)
        {
            FeaClsBoundary = pFeaClsBoun;
            BouddaryID = boundaryID;
        }

        public IFeature GetFeature()
        {
            IQueryFilter queryFilter = new QueryFilterClass();
            queryFilter.WhereClause = String.Format("{0}={1}", BoundaryIDNm, BouddaryID);
            IFeatureCursor cursor = FeaClsBoundary.Search(queryFilter, false);
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

        public BoundaryEntity GetEntity(IFeature pFeature)
        {
            BoundaryEntity bounEty = new BoundaryEntity();
            if (pFeature != null)
            {
                if (FeaClsBoundary.FindField(BoundaryIDNm) > 0)
                    bounEty.BoundaryID = Convert.ToInt32(pFeature.get_Value(FeaClsBoundary.FindField(BoundaryIDNm)));

                if (FeaClsBoundary.FindField(StyleIDNm) > 0)
                    bounEty.StyleID = Convert.ToInt32(pFeature.get_Value(FeaClsBoundary.FindField(StyleIDNm)));
                if (FeaClsBoundary.FindField(DirNm) > 0)
                    bounEty.Dir = Convert.ToInt32(pFeature.get_Value(FeaClsBoundary.FindField(DirNm)));
                if (FeaClsBoundary.FindField(OtherNm) > 0)
                    bounEty.Other = Convert.ToInt32(pFeature.get_Value(FeaClsBoundary.FindField(OtherNm)));
            }
            return bounEty;
        }

        public IFeature CreateBoundary(BoundaryEntity bounEty, IPolyline line)
        {
            IFeature bounFeature = FeaClsBoundary.CreateFeature();
            int bounID = 0;

            if (bounEty.BoundaryID > 0)
            {
                bounID = bounEty.BoundaryID;
                if (FeaClsBoundary.FindField(BoundaryIDNm) >= 0)
                    bounFeature.set_Value(FeaClsBoundary.FindField(BoundaryIDNm), bounEty.BoundaryID);
            }
            else
            {

                if (FeaClsBoundary.FindField(BoundaryIDNm) >= 0)
                    bounFeature.set_Value(FeaClsBoundary.FindField(BoundaryIDNm), bounFeature.OID);
                bounID = bounFeature.OID;
            }
            if (FeaClsBoundary.FindField(StyleIDNm) >= 0)
                bounFeature.set_Value(FeaClsBoundary.FindField(StyleIDNm), bounEty.StyleID);


            if (FeaClsBoundary.FindField(DirNm) >= 0)
                bounFeature.set_Value(FeaClsBoundary.FindField(DirNm), bounEty.Dir);

            if (FeaClsBoundary.FindField(OtherNm) >= 0)
                bounFeature.set_Value(FeaClsBoundary.FindField(OtherNm), bounEty.Other);

            bounFeature.Shape = line;
            bounFeature.Store();
            return bounFeature;
        }

        public void ModifyBoundaryAtt(IFeature pFeature,BoundaryEntity boundaryEty)
        {
            pFeature.set_Value(pFeature.Fields.FindField(BoundaryEntity.STYLEID_NAME), boundaryEty.StyleID);
            pFeature.set_Value(pFeature.Fields.FindField(BoundaryEntity.DIR_NAME), boundaryEty.Dir);
            pFeature.set_Value(pFeature.Fields.FindField(BoundaryEntity.BOUNDARYID_NAME), boundaryEty.BoundaryID);
            pFeature.Store();
        }
    }
}
