using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.DataModel.RoadSign;
using System;

namespace RoadNetworkSystem.NetworkElement.RoadSignElement
{
    class BoundaryService
    {
        
        public IFeatureClass FeaClsBoundary;
        public int BouddaryID;
        public BoundaryService(IFeatureClass pFeaClsBoun, int boundaryID)
        {
            FeaClsBoundary = pFeaClsBoun;
            BouddaryID = boundaryID;
        }

        public IFeature GetFeature()
        {
            IQueryFilter queryFilter = new QueryFilterClass();
            queryFilter.WhereClause = String.Format("{0}={1}", Boundary.BOUNDARYID_NAME, BouddaryID);
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

        public Boundary GetEntity(IFeature pFeature)
        {
            Boundary bounEty = new Boundary();
            if (pFeature != null)
            {
                if (FeaClsBoundary.FindField(Boundary.BOUNDARYID_NAME) > 0)
                    bounEty.BoundaryID = Convert.ToInt32(pFeature.get_Value(FeaClsBoundary.FindField(Boundary.BOUNDARYID_NAME)));

                if (FeaClsBoundary.FindField(Boundary.STYLEID_NAME) > 0)
                    bounEty.StyleID = Convert.ToInt32(pFeature.get_Value(FeaClsBoundary.FindField(Boundary.STYLEID_NAME)));
                if (FeaClsBoundary.FindField(Boundary.DIR_NAME) > 0)
                    bounEty.Dir = Convert.ToInt32(pFeature.get_Value(FeaClsBoundary.FindField(Boundary.DIR_NAME)));
                if (FeaClsBoundary.FindField(Boundary.OTHER_NAME) > 0)
                    bounEty.Other = Convert.ToInt32(pFeature.get_Value(FeaClsBoundary.FindField(Boundary.OTHER_NAME)));
            }
            return bounEty;
        }

        public IFeature CreateBoundary(Boundary bounEty, IPolyline line)
        {
            IFeature bounFeature = FeaClsBoundary.CreateFeature();
            int bounID = 0;

            if (bounEty.BoundaryID > 0)
            {
                bounID = bounEty.BoundaryID;
                if (FeaClsBoundary.FindField(Boundary.BOUNDARYID_NAME) >= 0)
                    bounFeature.set_Value(FeaClsBoundary.FindField(Boundary.BOUNDARYID_NAME), bounEty.BoundaryID);
            }
            else
            {

                if (FeaClsBoundary.FindField(Boundary.BOUNDARYID_NAME) >= 0)
                    bounFeature.set_Value(FeaClsBoundary.FindField(Boundary.BOUNDARYID_NAME), bounFeature.OID);
                bounID = bounFeature.OID;
            }
            if (FeaClsBoundary.FindField(Boundary.STYLEID_NAME) >= 0)
                bounFeature.set_Value(FeaClsBoundary.FindField(Boundary.STYLEID_NAME), bounEty.StyleID);


            if (FeaClsBoundary.FindField(Boundary.DIR_NAME) >= 0)
                bounFeature.set_Value(FeaClsBoundary.FindField(Boundary.DIR_NAME), bounEty.Dir);

            if (FeaClsBoundary.FindField(Boundary.OTHER_NAME) >= 0)
                bounFeature.set_Value(FeaClsBoundary.FindField(Boundary.OTHER_NAME), bounEty.Other);

            bounFeature.Shape = line;
            bounFeature.Store();
            return bounFeature;
        }

        public void ModifyBoundaryAtt(IFeature pFeature,Boundary boundaryEty)
        {
            pFeature.set_Value(pFeature.Fields.FindField(Boundary.STYLEID_NAME), boundaryEty.StyleID);
            pFeature.set_Value(pFeature.Fields.FindField(Boundary.DIR_NAME), boundaryEty.Dir);
            pFeature.set_Value(pFeature.Fields.FindField(Boundary.BOUNDARYID_NAME), boundaryEty.BoundaryID);
            pFeature.Store();
        }
    }
}
