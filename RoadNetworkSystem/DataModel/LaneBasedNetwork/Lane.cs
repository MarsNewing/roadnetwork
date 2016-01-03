using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.DataModel.LaneBasedNetwork
{
    class Lane
    {
        public const string LaneName = "Lane";
        public const int leftPosition = 1;
        public const int rightPositionOffset = 0;
        public const int LANE_CLOSED = 1;
        public const int LANE_UNCLOSED = 0;

        public const string CHANGE_RIGHT = "Right";
        public const string CHANGE_LEFT = "Left";
        public const string CHANGE_NEITHER = "Neither";
        public const string CHANGE_BOTH = "Both";


        public const double LANE_WEIDTH = 3.5;
        public int LaneID { get; set; }
        public int Position { get; set; }

        public string Change { get; set; }
        public int ArcID { get; set; }
        public int LeftBoundaryID { get; set; }

        public int RightBoundaryID { get; set; }
        public string VehClasses { get; set; }
        public int LaneClosed { get; set; }

        public double Width { get; set; }
        public int Other { get; set; }

        /// <summary>
        /// 复制实体
        /// </summary>
        /// <returns></returns>
        public Lane Copy()
        {
            Lane lane = new Lane();

            lane.LaneID = this.LaneID;
            lane.ArcID = this.ArcID;
            lane.Position = this.Position;

            lane.Change = this.Change;
            lane.RightBoundaryID = this.RightBoundaryID;
            lane.LeftBoundaryID = this.LeftBoundaryID;

            lane.VehClasses = this.VehClasses;
            lane.LaneClosed = this.LaneClosed;
            lane.Width = this.Width;

            lane.Other = this.Other;

            return lane;
        }
    }
}
