using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadNetworkSystem.DataModel.Float
{
    /// <summary>
    /// 线圈流量
    /// </summary>
    class BaseFloatLDT
    {

        public const string BaseFloatLDT_NAME = "BaseFloatLDT";

        public const string LDTID_NAME = "LDTID";
        public const string LANEID_NAME = "FTime";
        public const string LINKID_NAME = "TTime";

        public const string DIR_NAME = "Quantity";
        public const string VELOCITY_NAME = "Velocity";
        public const string DENSITY_NAME = "Density";
        
        public int LDTID { get; set; }
        public DateTime FTime { get; set; }
        public DateTime TTime { get; set; }

        public int Quantity { get; set; }
        public double Velocity { get; set; }
        public double Density { get; set; }
    }
}
