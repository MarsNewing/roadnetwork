using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.DataModel.RoadSign;
using System;

namespace RoadNetworkSystem.NetworkElement.RoadSignElement
{
    class StopLineService
    {

        
        public IFeatureClass FeaClsStopLine;
        public int BouddaryID;
        public StopLineService(IFeatureClass pFeaClsStopLine, int stopLineID)
        {
            FeaClsStopLine = pFeaClsStopLine;
            BouddaryID = stopLineID;
        }

        public StopLine GetEntity(IFeature pFeature)
        {
            StopLine bounEty = new StopLine();
            if (pFeature != null)
            {
                if (FeaClsStopLine.FindField(StopLine.StopLineIDNm) > 0)
                    bounEty.StopLineID = Convert.ToInt32(pFeature.get_Value(FeaClsStopLine.FindField(StopLine.StopLineIDNm)));

                if (FeaClsStopLine.FindField(Boundary.STYLEID_NAME) > 0)
                    bounEty.StyleID = Convert.ToInt32(pFeature.get_Value(FeaClsStopLine.FindField(Boundary.STYLEID_NAME)));
                if (FeaClsStopLine.FindField(StopLine.NodeIDNm) > 0)
                    bounEty.NodeID = Convert.ToInt32(pFeature.get_Value(FeaClsStopLine.FindField(StopLine.NodeIDNm)));

                if (FeaClsStopLine.FindField(StopLine.LaneIDNm) > 0)
                    bounEty.LaneID = Convert.ToInt32(pFeature.get_Value(FeaClsStopLine.FindField(StopLine.LaneIDNm)));
                if (FeaClsStopLine.FindField(StopLine.ArcIDNm) > 0)
                    bounEty.ArcID = Convert.ToInt32(pFeature.get_Value(FeaClsStopLine.FindField(StopLine.ArcIDNm)));
                if (FeaClsStopLine.FindField(StopLine.OtherNm) > 0)
                    bounEty.Other = Convert.ToInt32(pFeature.get_Value(FeaClsStopLine.FindField(StopLine.OtherNm)));
            }
            return bounEty;
        }

        public IFeature CreateStopLine(StopLine bounEty, IPolyline line)
        {
            IFeature stoplineFeature = FeaClsStopLine.CreateFeature();

            if (bounEty.StopLineID > 0)
            {
                if (FeaClsStopLine.FindField(StopLine.StopLineIDNm) >= 0)
                    stoplineFeature.set_Value(FeaClsStopLine.FindField(StopLine.StopLineIDNm), bounEty.StopLineID);
            }
            else
            {

                if (FeaClsStopLine.FindField(StopLine.StopLineIDNm) >= 0)
                    stoplineFeature.set_Value(FeaClsStopLine.FindField(StopLine.StopLineIDNm), stoplineFeature.OID);
            }

            if (FeaClsStopLine.FindField(Boundary.STYLEID_NAME) >= 0)
                stoplineFeature.set_Value(FeaClsStopLine.FindField(Boundary.STYLEID_NAME), bounEty.StyleID);


            if (FeaClsStopLine.FindField(StopLine.NodeIDNm) >= 0)
                stoplineFeature.set_Value(FeaClsStopLine.FindField(StopLine.NodeIDNm), bounEty.NodeID);


            if (FeaClsStopLine.FindField(StopLine.ArcIDNm) >= 0)
                stoplineFeature.set_Value(FeaClsStopLine.FindField(StopLine.ArcIDNm), bounEty.ArcID);


            if (FeaClsStopLine.FindField(StopLine.LaneIDNm) >= 0)
                stoplineFeature.set_Value(FeaClsStopLine.FindField(StopLine.LaneIDNm), bounEty.LaneID);

            if (FeaClsStopLine.FindField(StopLine.OtherNm) >= 0)
                stoplineFeature.set_Value(FeaClsStopLine.FindField(StopLine.OtherNm), bounEty.Other);

            stoplineFeature.Shape = line;
            stoplineFeature.Store();
            return stoplineFeature;
        }
    }
}
