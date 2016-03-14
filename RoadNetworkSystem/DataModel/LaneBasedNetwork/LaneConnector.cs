using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.DataModel.LaneBasedNetwork
{
    class LaneConnector
    {
        public const string ConnectorName = "LaneConnector";

        public const string TURNING_RIGHT = "Right";
        public const string TURNING_LEFT = "Left";
        public const string TURNING_STRAIGHT = "Straight";
        public const string TURNING_UTURN = "UTurn";

        public int ConnectorID { get; set; }
        public int fromLaneID { get; set; }
        public int toLaneID { get; set; }

        public string TurningDir { get; set; }
        public int fromArcID { get; set; }
        public int toArcID { get; set; }

        public int fromLinkID { get; set; }
        public int fromDir { get; set; }
        public int toLinkID { get; set; }


        public int toDir { get; set; }
        public int Other { get; set; }


        public string GetTurnDir(int turningIndex)
        {

            if (turningIndex == Convert.ToInt32(RoadNetworkSystem.DataModel.RoadSign.TurnArrow.TURNING_ITEM.左转))
            {
                return TURNING_LEFT;
            }
            else if (turningIndex == Convert.ToUInt32(RoadNetworkSystem.DataModel.RoadSign.TurnArrow.TURNING_ITEM.直行))
            {
                return TURNING_STRAIGHT;
            }
            else if (turningIndex == Convert.ToUInt32(RoadNetworkSystem.DataModel.RoadSign.TurnArrow.TURNING_ITEM.右转))
            {
                return TURNING_RIGHT;
            }
            else if (turningIndex == Convert.ToUInt32(RoadNetworkSystem.DataModel.RoadSign.TurnArrow.TURNING_ITEM.掉头))
            {
                return TURNING_UTURN;
            }
            else
            {
                return "";
            }

        }

        /// <summary>
        /// 复制实体
        /// </summary>
        /// <returns></returns>
        public LaneConnector Copy()
        {
            LaneConnector connector = new LaneConnector();

            connector.ConnectorID = this.ConnectorID;
            connector.fromLaneID = this.fromLaneID;
            connector.toLaneID = this.toLaneID;

            connector.TurningDir = this.TurningDir;
            connector.toArcID = this.toArcID;
            connector.fromArcID = this.fromArcID;

            connector.fromLinkID = this.fromLinkID;
            connector.fromDir = this.fromDir;
            connector.toLinkID = this.toLinkID;

            connector.toDir = this.toDir;
            connector.Other = this.Other;

            return connector;
        }

        public enum 转向
        {
            /// <summary>
            /// 直行
            /// </summary>
            Straight = 0,
            /// <summary>
            /// 左转
            /// </summary>
            Left = 1,
            /// <summary>
            /// 右转
            /// </summary>
            Right = 2,
            /// <summary>
            /// 掉头
            /// </summary>
            UTurn = 3
        }
    }
}
