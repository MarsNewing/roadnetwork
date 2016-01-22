using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadNetworkSystem.DataModel.Float
{
    class BaseFloatVDT
    {
        public const string BaseFloatVDT_NAME = "BaseFloatVDT";

        public const string VDTID_NAME = "VDTID";
        public const string FTIME_NAME = "FTime";
        public const string TTIME_NAME = "TTime";

        public const string VDT_INDEX_NAME = "VdtIndex";
        public const string LONG_VEH_NAME = "LongVeh";

        public const string DIR_NAME = "Quantity";
        public const string LANEID_NAME = "Velocity";
        public const string LINKID_NAME = "Density";


        public const string LANE1_QUANTITY_NAME = "Lane1Quantity";

        public const string SECTION_QUANTITY_NAME = "SectionQuantity";

        public const string LANE2_QUANTITY_NAME = "Lane2Quantity";
        public const string LANE3_QUANTITY_NAME = "Lane3Quantity";
        public const string LANE4_QUANTITY_NAME = "Lane4Quantity";

        public const string LANE5_QUANTITY_NAME = "Lane5Quantity";
        public const string LANE6_QUANTITY_NAME = "Lane6Quantity";
        public const string LANE7_QUANTITY_NAME = "Lane7Quantity";

        public const string LANE8_QUANTITY_NAME = "Lane8Quantity";



        public int VDTID { get; set; }
        public DateTime FTime { get; set; }
        public DateTime TTime { get; set; }

        public int VdtIndex { get; set; }
        public int LongVeh { get; set; }

        public int Quantity { get; set; }
        public double Velocity { get; set; }
        public double Density { get; set; }

        public int SectionQuantity { get; set; }
        public int Lane1Quantity { get; set; }

        public int Lane2Quantity { get; set; }
        public int Lane3Quantity { get; set; }
        public int Lane4Quantity { get; set; }

        public int Lane5Quantity { get; set; }
        public int Lane6Quantity { get; set; }
        public int Lane7Quantity { get; set; }

        public int Lane8Quantity { get; set; }
    }
}
