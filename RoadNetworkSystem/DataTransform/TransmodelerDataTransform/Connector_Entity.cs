using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.TransmodelerDataTransform
{
    public class Connector_Entity
    {
        //
        private string _link;
        public string LINK
        {
            get { return _link; }
            set { _link = value; }
        }

        //
        private string _lane_pos;
        public string LANE_POS
        {
            get { return _lane_pos; }
            set { _lane_pos = value; }
        }
    }
}
