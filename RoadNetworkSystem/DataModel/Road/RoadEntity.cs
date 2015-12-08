using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.DataModel.Road
{
    /// <summary>
    /// 道路中心线对象 2014-10-13,22;05
    /// Create by niuzhm
    /// niuzhm@163.com
    /// </summary>
    class RoadEntity
    {
        public const string RoadNm = "Road";

        public int RoadID { get; set; }
        public int RoadType { get; set; }
        public string RoadName { get; set; }

        public int Other { get; set; }

        /// <summary>
        /// 复制实体
        /// </summary>
        /// <returns></returns>
        public RoadEntity Copy()
        {
            RoadEntity rd = new RoadEntity();
            rd.RoadID = this.RoadID;
            rd.RoadName = this.RoadName;
            rd.RoadType = this.RoadType;
            rd.Other = this.Other;
            return rd;
        }

    }
}
