using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.NetworkExtraction.FreeWay.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadNetworkSystem.NetworkExtraction.FreeWay.Dao
{
    class SegmentDao
    {
        private IFeatureClass _pFeaCls;
        public SegmentDao(IFeatureClass pFeaCls)
        {
            _pFeaCls = pFeaCls;
        }

        public void CreateSegment(Segment segment,IPolyline line)
        {
            IFeature pFeature = _pFeaCls.CreateFeature();
            segment.SegmentID = pFeature.OID;
            pFeature.set_Value(_pFeaCls.FindField(Segment.FIELDE_SEGMENT_ID), segment.SegmentID);
            if (_pFeaCls.FindField(Segment.FIELDE_FSEG_NODE_ID) >= 0)
            {
                pFeature.set_Value(_pFeaCls.FindField(Segment.FIELDE_FSEG_NODE_ID), segment.FSegNodeID);
            }
            if (_pFeaCls.FindField(Segment.FIELDE_TSEG_NODE_ID) >= 0)
            {
                pFeature.set_Value(_pFeaCls.FindField(Segment.FIELDE_TSEG_NODE_ID), segment.TSegNodeID);
            }

            if (_pFeaCls.FindField(Segment.FIELDE_SEG_TYPE) >= 0)
            {
                pFeature.set_Value(_pFeaCls.FindField(Segment.FIELDE_SEG_TYPE), segment.SegType);
            }
            if (_pFeaCls.FindField(Segment.FIELDE_ARC_ID) >= 0 )
            {
                pFeature.set_Value(_pFeaCls.FindField(Segment.FIELDE_ARC_ID), segment.ArcID);
            }
            if (_pFeaCls.FindField(Segment.FIELDE_ARC_SERIAL) >= 0)
            {
                pFeature.set_Value(_pFeaCls.FindField(Segment.FIELDE_ARC_SERIAL), segment.ArcSerial);
            }

            if (_pFeaCls.FindField(Segment.FIELDE_ARC_LANENUM) >= 0)
            {
                pFeature.set_Value(_pFeaCls.FindField(Segment.FIELDE_ARC_LANENUM), segment.LaneNum);
            }
            pFeature.Shape = line;
            pFeature.Store();
        }

        public void updateSegment(Segment segment, IFeature pFeature)
        {
         
            if (_pFeaCls.FindField(Segment.FIELDE_FSEG_NODE_ID) >= 0)
            {
                pFeature.set_Value(_pFeaCls.FindField(Segment.FIELDE_FSEG_NODE_ID), segment.FSegNodeID);
            }
            if (_pFeaCls.FindField(Segment.FIELDE_TSEG_NODE_ID) >= 0)
            {
                pFeature.set_Value(_pFeaCls.FindField(Segment.FIELDE_TSEG_NODE_ID), segment.TSegNodeID);
            }

            if (_pFeaCls.FindField(Segment.FIELDE_SEG_TYPE) >= 0)
            {
                pFeature.set_Value(_pFeaCls.FindField(Segment.FIELDE_SEG_TYPE), segment.SegType);
            }
            if (_pFeaCls.FindField(Segment.FIELDE_ARC_ID) >= 0)
            {
                pFeature.set_Value(_pFeaCls.FindField(Segment.FIELDE_ARC_ID), segment.ArcID);
            }
            if (_pFeaCls.FindField(Segment.FIELDE_ARC_SERIAL) >= 0)
            {
                pFeature.set_Value(_pFeaCls.FindField(Segment.FIELDE_ARC_SERIAL), segment.ArcSerial);
            }

            if (_pFeaCls.FindField(Segment.FIELDE_ARC_LANENUM) >= 0)
            {
                pFeature.set_Value(_pFeaCls.FindField(Segment.FIELDE_ARC_LANENUM), segment.LaneNum);
            }
            pFeature.Store();
        }

        public Segment getSegment(IFeature pFeature)
        {
            Segment segment = new Segment();
            segment.ArcID = Convert.ToInt32(pFeature.get_Value(_pFeaCls.FindField(Segment.FIELDE_ARC_ID)));
            segment.ArcSerial = Convert.ToInt32(pFeature.get_Value(_pFeaCls.FindField(Segment.FIELDE_ARC_SERIAL)));
            segment.FSegNodeID = Convert.ToInt32(pFeature.get_Value(_pFeaCls.FindField(Segment.FIELDE_FSEG_NODE_ID)));

            segment.LaneNum = Convert.ToInt32(pFeature.get_Value(_pFeaCls.FindField(Segment.FIELDE_ARC_LANENUM)));
            segment.SegmentID = Convert.ToInt32(pFeature.get_Value(_pFeaCls.FindField(Segment.FIELDE_SEGMENT_ID)));
            segment.SegType = Convert.ToInt32(pFeature.get_Value(_pFeaCls.FindField(Segment.FIELDE_SEG_TYPE)));

            segment.TSegNodeID = Convert.ToInt32(pFeature.get_Value(_pFeaCls.FindField(Segment.FIELDE_TSEG_NODE_ID)));

            return segment;
        }
    }
}
