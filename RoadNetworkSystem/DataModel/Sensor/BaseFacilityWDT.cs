using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadNetworkSystem.DataModel.Sensor
{
    class BaseFacilityWDT
    {

        public const string BaseFacilityWDT_NAME = "BaseFacilityWDT";

        public const string WDTID_NAME = "WDTID";
        public const string LINKID_NAME = "LinkID";
        public const string DIR_NAME = "Dir";

        public const string POS_NAME = "Pos";
        public const string OFFSET_NAME = "Offset";
        public const string CREATE_DATE_NAME = "CreateDate";

        public const string MAINTAIN_DATE_NAME = "MaintainDate";
        public const string STATUS_NAME = "Status";
        public const string LANE1_NAME = "Lane1ID";

        public const string LANE2_NAME = "Lane2ID";
        public const string LANE3_NAME = "Lane3ID";
        public const string LANE4_NAME = "Lane4ID";

        public const string LANE5_NAME = "Lane5ID";
        public const string LANE6_NAME = "Lane6ID";
        public const string LANE7_NAME = "Lane7ID";

        public const string LANE8_NAME = "Lane8ID";

        public int WDTID { get; set; }
        public int LinkID { get; set; }
        public int Dir { get; set; }

        public double Pos { get; set; }
        public double Offset { get; set; }
        public DateTime CreateDate { get; set; }

        public DateTime MaintainDate { get; set; }
        public string Status { get; set; }
        public int Lane1ID { get; set; }

        public int Lane2ID { get; set; }
        public int Lane3ID { get; set; }
        public int Lane4ID { get; set; }

        public int Lane5ID { get; set; }
        public int Lane6ID { get; set; }
        public int Lane7ID { get; set; }

        public int Lane8ID { get; set; }
    }
}
