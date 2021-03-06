﻿using ESRI.ArcGIS.Geodatabase;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.Road;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.Connection;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LaneLayer;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LinkLayer;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.NetworkEditor
{
    class CutHelper
    {
        const int tolerance = 15;
        public static double getCutDefualt(double curWidth, double adjWidth, double angel)
        {
            double cut = 0.0;
            double angleInArc=angel * Math.PI / 180;
            if (angel > (90 - tolerance) && angel < (90 + tolerance))
            {
                cut = adjWidth;
            }
            else if ((angel > (180-tolerance)) || angel < tolerance)
            {
                cut = 0;
            }
            else
            {
                cut = (adjWidth / Math.Sin(angleInArc)) + (curWidth / Math.Tan(angleInArc));
            }
            return cut;
        }

        public static double getCutOneway(double curWidth, double angel)
        {
            double cut = 0.0;
            cut = Math.Tan(angel * Math.PI/180);
            return cut;
        }
    }

   

    
}
