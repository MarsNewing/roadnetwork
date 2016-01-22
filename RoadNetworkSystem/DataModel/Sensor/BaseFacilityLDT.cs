using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadNetworkSystem.DataModel.Sensor
{
    /// <summary>
    /// 线圈模型
    /// </summary>
    class BaseFacilityLDT
    {
        public const string BaseFacilityLDT_Name = "BaseFacilityLDT";
        public const string LDTID_NAME = "LDTID";
        public const string LANEID_NAME = "LaneID";
        public const string LINKID_NAME = "LinkID";

        public const string DIR_NAME = "Dir";
        public const string POS_NAME = "Pos";
        public const string OFFSET_NAME = "Offset";

        public const string CREATE_DATE_NAME = "CreateDate";
        public const string MAINTAIN_DATE_NAME = "MaintainDate";
        public const string STATUS_NAME = "Status";

        

        public int LDTID { get; set; }
        public int LANEID { get; set; }
        public int LinkID { get; set; }

        public int Dir { get; set; }
        public double Pos { get; set; }
        public double Offset { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime MaintainDate { get; set; }
        public string Status { get; set; }
    }
}
