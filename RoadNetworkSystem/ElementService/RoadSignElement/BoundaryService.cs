using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.RoadSign;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LaneLayer;
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

        private bool deleteLeftBoundary(IFeature laneFea)
        {
            if (laneFea == null)
            {
                return false;
            }
            int centerLineID = Convert.ToInt32(laneFea.get_Value(laneFea.Fields.FindField(Lane.LeftBoundaryIDNm)));
            BoundaryService boun = new BoundaryService(FeaClsBoundary, centerLineID);
            IFeature bounFea = boun.GetFeature();
            if (bounFea != null)
            {
                bounFea.Delete();
                return true;
            }
            else
            {
                return false;
            }
        }

        public IPolyline GetCenterLineShape(IFeature linkFea,
            Arc sameArc,
            Arc oppositionArc,
            IFeatureClass pFeaClsLane,
            IFeatureClass pFeaClsKerb)
        {

            int linkFlowDir = Convert.ToInt32(linkFea.get_Value(linkFea.Fields.FindField("FlowDir")));

            LaneFeatureService laneService = new LaneFeatureService(pFeaClsLane, 0);

            IPoint sameKerb3 = null;
            IPoint oppKerb3 = null;
            if (sameArc != null)
            {
                IFeature sameLane1Fea = laneService.QueryFeatureBuRule(sameArc.ArcID, Lane.LEFT_POSITION);
                deleteLeftBoundary(sameLane1Fea);
                KerbService kerb = new KerbService(pFeaClsKerb, 0);
                IFeature kerb3 = kerb.GetKerbByArcAndSerial(sameArc.ArcID, 3);
                if(kerb3 != null)
                { sameKerb3 = kerb3.Shape as IPoint; }

            }

            if (oppositionArc != null)
            {
                IFeature oppositionLane1Fea = laneService.QueryFeatureBuRule(oppositionArc.ArcID, Lane.LEFT_POSITION);
                deleteLeftBoundary(oppositionLane1Fea);
                KerbService kerb = new KerbService(pFeaClsKerb, 0);
                IFeature kerb3 = kerb.GetKerbByArcAndSerial(oppositionArc.ArcID, 3);
                if (kerb3 != null)
                {
                    oppKerb3 = kerb3.Shape as IPoint;
                }
            }

            IPointCollection col = new PolylineClass();
            
            if (linkFlowDir != Link.FLOWDIR_DOUBLE)
            {
                col.AddPoint((linkFea.Shape as IPolyline).FromPoint);
                col.AddPoint((linkFea.Shape as IPolyline).ToPoint);
            }
            else
            {
                if (oppKerb3 != null)
                {
                    col.AddPoint(oppKerb3);
                }
                if (sameKerb3 != null)
                {
                    col.AddPoint(sameKerb3);
                }
            }
            IPolyline cneterLine = col as IPolyline;

            return cneterLine;
        }
    }
}
