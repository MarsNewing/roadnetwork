﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.DataModel.RoadSign
{
    class TurnArrow
    {
        /// <summary>
        /// 指示具体方向的ArrowType
        /// </summary>
        public const int ARROW_TYPE_REAL_DIRECTION = 1;

        /// <summary>
        /// 指示前方方向的ArrowType
        /// </summary>
        public const int ARROW_TYPE_GENERAL_DIRECTION = 0;


        public enum TURNING_ITEM
        {
            左转,
            直行,
            右转,
            掉头
        }

        public enum ARROW_TURNING
        {
            左,
            左直,
            左右,
            左掉头,
            直行,
            直掉头,
            直右,
            右,
            掉头
        }



        /// <summary>
        /// 规定了数据模型，请不要在其他类中直接读取数据
        /// </summary>
        public const string ArrowIDNm = "ArrowID";
        public const string STYLEID_NAME = "StyleID";
        public const string ArrowTypeNm = "ArrowType";
        
        public const string SerialNm = "Serial";
        public const string ANGLENm = "ANGLE";
        public const string ArcIDNm = "ArcID";

        public const string LaneIDNm = "LaneID";
        public const string PrecedeArrowsNm = "PrecedeArrows";
        public const string OtherNm = "Other";
        public const string TurnArrowName = "TurnArrow";

        public int ArrowID { get; set; }
        public int StyleID { get; set; }
        public int ArrowType { get; set; }

        public int Serial { get; set; }
        public double ANGLE { get; set; }
        public int ArcID { get; set; }

        public int LaneID { get; set; }
        public string PrecedeArrows { get; set; }
        public int Other { get; set; }

        /// <summary>
        /// 复制实体
        /// </summary>
        /// <returns></returns>
        public TurnArrow Copy()
        {
            TurnArrow ta = new TurnArrow();

            ta.ArrowID = this.ArrowID;
            ta.StyleID = this.StyleID;
            ta.ArrowType = this.ArrowType;

            ta.Serial = this.Serial;
            ta.ANGLE = this.ANGLE;
            ta.ArcID = this.ArcID;

            ta.LaneID = this.LaneID;
            ta.PrecedeArrows = this.PrecedeArrows;
            ta.Other = this.Other;

            return ta;
        }
    }
}
