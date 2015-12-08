using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadNetworkSystem.DataModel.SignRule
{
    class BoundaryRule
    {

        public const string BOUNDARY_RULE_TABLE_NAME = "Boundary_Rule";

        public int StyleID;
        public int LeftRuleID;
        public int RightRuleID;

        public bool IsCenterLine;
        public bool IsSideLine;

        public const string STYLEID_NAME = "StyleID";
        public const string LEFT_RULEID_NAME = "LeftRuleID";
        public const string RIGHT_RULEID_NAME = "RightRuleID";

        public const string IS_CENTERLINE_NAME = "IsCenterLine";
        public const string IS_SIDELINE_NAME = "IsSideLine";
    }
}
