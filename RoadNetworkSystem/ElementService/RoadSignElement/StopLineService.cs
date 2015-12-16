using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.DataModel.RoadSign;
using System;

namespace RoadNetworkSystem.NetworkElement.RoadSignElement
{
    class StopLineService
    {

        public const string StopLineIDNm = "StopLineID";
        public const string StyleIDNm = "StyleID";
        public const string NodeIDNm = "NodeID";


        public const string LaneIDNm = "LaneID";
        public const string ArcIDNm = "ArcID";
        public const string OtherNm = "Other";

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
                if (FeaClsStopLine.FindField(StopLineIDNm) > 0)
                    bounEty.StopLineID = Convert.ToInt32(pFeature.get_Value(FeaClsStopLine.FindField(StopLineIDNm)));

                if (FeaClsStopLine.FindField(StyleIDNm) > 0)
                    bounEty.StyleID = Convert.ToInt32(pFeature.get_Value(FeaClsStopLine.FindField(StyleIDNm)));
                if (FeaClsStopLine.FindField(NodeIDNm) > 0)
                    bounEty.NodeID = Convert.ToInt32(pFeature.get_Value(FeaClsStopLine.FindField(NodeIDNm)));

                if (FeaClsStopLine.FindField(LaneIDNm) > 0)
                    bounEty.LaneID = Convert.ToInt32(pFeature.get_Value(FeaClsStopLine.FindField(LaneIDNm)));
                if (FeaClsStopLine.FindField(ArcIDNm) > 0)
                    bounEty.ArcID = Convert.ToInt32(pFeature.get_Value(FeaClsStopLine.FindField(ArcIDNm)));
                if (FeaClsStopLine.FindField(OtherNm) > 0)
                    bounEty.Other = Convert.ToInt32(pFeature.get_Value(FeaClsStopLine.FindField(OtherNm)));
            }
            return bounEty;
        }

        public IFeature CreateStopLine(StopLine bounEty, IPolyline line)
        {
            IFeature stoplineFeature = FeaClsStopLine.CreateFeature();

            if (bounEty.StopLineID > 0)
            {
                if (FeaClsStopLine.FindField(StopLineIDNm) >= 0)
                    stoplineFeature.set_Value(FeaClsStopLine.FindField(StopLineIDNm), bounEty.StopLineID);
            }
            else
            {

                if (FeaClsStopLine.FindField(StopLineIDNm) >= 0)
                    stoplineFeature.set_Value(FeaClsStopLine.FindField(StopLineIDNm), stoplineFeature.OID);
            }

            if (FeaClsStopLine.FindField(StyleIDNm) >= 0)
                stoplineFeature.set_Value(FeaClsStopLine.FindField(StyleIDNm), bounEty.StyleID);


            if (FeaClsStopLine.FindField(NodeIDNm) >= 0)
                stoplineFeature.set_Value(FeaClsStopLine.FindField(NodeIDNm), bounEty.NodeID);


            if (FeaClsStopLine.FindField(ArcIDNm) >= 0)
                stoplineFeature.set_Value(FeaClsStopLine.FindField(ArcIDNm), bounEty.ArcID);


            if (FeaClsStopLine.FindField(LaneIDNm) >= 0)
                stoplineFeature.set_Value(FeaClsStopLine.FindField(LaneIDNm), bounEty.LaneID);

            if (FeaClsStopLine.FindField(OtherNm) >= 0)
                stoplineFeature.set_Value(FeaClsStopLine.FindField(OtherNm), bounEty.Other);

            stoplineFeature.Shape = line;
            stoplineFeature.Store();
            return stoplineFeature;
        }
    }
}
