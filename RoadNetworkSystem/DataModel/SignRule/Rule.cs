using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadNetworkSystem.DataModel.SignRule
{
    class Rule
    {
        public const string RULE_TABLE_NAME = "Rule";

        public int RuleID;
        public int RuleType;
        public int SettingValue;

        public string SettingUnit;
        public bool IsFloor;
        public bool IsCeiling;

        public bool IsAccessible;
        public bool IsForbidden;
        public string ReferenceStandard;

        public string Description;

        public const string RULEID_NAME = "RuleID";
        public const string RULE_TYPE_NAME = "RuleType";
        public const string SETTING_VALUE_NAME = "SettingValue";

        public const string SETTING_UNIT_NAME = "SettingUnit";
        public const string IS_FLOOR_NAME = "IsFloor";
        public const string IS_CEILING_NAME = "IsCeiling";

        public const string IS_ACCESSIBLE_NAME = "IsAccessible";
        public const string IS_FORBIDDEN_NAME = "IsForbidden";
        public const string REFERENCE_STANDARD_NAME = "ReferenceStandard";

        public const string DESCRIPTION_NAME = "Description";

    }
}
