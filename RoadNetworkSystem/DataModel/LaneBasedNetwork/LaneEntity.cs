﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.DataModel.LaneBasedNetwork
{
    class LaneEntity
    {
        public const string LaneName = "Lane";

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
        public LaneEntity Copy()
        {
            LaneEntity lane = new LaneEntity();

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
