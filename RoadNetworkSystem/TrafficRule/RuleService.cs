using RoadNetworkSystem.ADO.Access;
using RoadNetworkSystem.DataModel.SignRule;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadNetworkSystem.TrafficRule
{
    class RuleService
    {
        private OleDbConnection _connection;
        public RuleService(OleDbConnection connection) 
        {
            _connection = connection;
        }
        public RoadNetworkSystem.DataModel.SignRule.Rule getRrule(int ruleId)
        {
            RoadNetworkSystem.DataModel.SignRule.Rule rule = null;
            
            string sql = "select * from " + 
                RoadNetworkSystem.DataModel.SignRule.Rule.RULE_TABLE_NAME+ 
                " where "+RoadNetworkSystem.DataModel.SignRule.Rule.RULEID_NAME + " = " + ruleId;
            OleDbCommand cmd = new OleDbCommand(sql, _connection);
            OleDbDataReader reader = cmd.ExecuteReader();
            
            if (reader.Read())
            {
                rule = new DataModel.SignRule.Rule();
                rule.RuleID = ruleId;
                rule.RuleType = Convert.ToInt32(reader[Rule.RULE_TYPE_NAME]);
                rule.SettingValue = Convert.ToInt32(reader[Rule.SETTING_VALUE_NAME]);
                rule.SettingUnit = Convert.ToString(reader[Rule.SETTING_UNIT_NAME]);

                rule.IsFloor = Convert.ToBoolean(reader[Rule.IS_FLOOR_NAME]);
                rule.IsCeiling = Convert.ToBoolean(reader[Rule.IS_CEILING_NAME]);
                rule.IsAccessible = Convert.ToBoolean(reader[Rule.IS_ACCESSIBLE_NAME]);

                rule.IsForbidden = Convert.ToBoolean(reader[Rule.IS_FORBIDDEN_NAME]);
                rule.ReferenceStandard = Convert.ToString(reader[Rule.REFERENCE_STANDARD_NAME]);
                rule.Description = Convert.ToString(reader[Rule.DESCRIPTION_NAME]);
            }

            return rule;

        }

        public Rule getRruleByIsAccessible(bool accessibleFlag)
        {
            RoadNetworkSystem.DataModel.SignRule.Rule rule = null;

            string sql = "select * from " +
                RoadNetworkSystem.DataModel.SignRule.Rule.RULE_TABLE_NAME +
                " where " + RoadNetworkSystem.DataModel.SignRule.Rule.IS_ACCESSIBLE_NAME + " = " + accessibleFlag;
            OleDbCommand cmd = new OleDbCommand(sql, _connection);
            OleDbDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                rule = new DataModel.SignRule.Rule();
                rule.RuleID = Convert.ToInt32(reader[Rule.IS_ACCESSIBLE_NAME]);
                rule.RuleType = Convert.ToInt32(reader[Rule.RULE_TYPE_NAME]);
                rule.SettingValue = Convert.ToInt32(reader[Rule.SETTING_VALUE_NAME]);
                rule.SettingUnit = Convert.ToString(reader[Rule.SETTING_UNIT_NAME]);

                rule.IsFloor = Convert.ToBoolean(reader[Rule.IS_FLOOR_NAME]);
                rule.IsCeiling = Convert.ToBoolean(reader[Rule.IS_CEILING_NAME]);
                rule.IsAccessible = Convert.ToBoolean(reader[Rule.IS_ACCESSIBLE_NAME]);

                rule.IsForbidden = Convert.ToBoolean(reader[Rule.IS_FORBIDDEN_NAME]);
                rule.ReferenceStandard = Convert.ToString(reader[Rule.REFERENCE_STANDARD_NAME]);
                rule.Description = Convert.ToString(reader[Rule.DESCRIPTION_NAME]);
            }

            return rule;

        }

        /// <summary>
        /// 是否被禁止
        /// </summary>
        /// <param name="forbidden"></param>禁止为true
        /// <returns></returns>
        public Rule getRruleByIsForbidden(bool forbidden)
        {
            RoadNetworkSystem.DataModel.SignRule.Rule rule = null;

            string sql = "select * from " +
                RoadNetworkSystem.DataModel.SignRule.Rule.RULE_TABLE_NAME +
                " where " + RoadNetworkSystem.DataModel.SignRule.Rule.IS_FORBIDDEN_NAME + " = " + forbidden;
            OleDbCommand cmd = new OleDbCommand(sql, _connection);
            OleDbDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                rule = new DataModel.SignRule.Rule();
                rule.RuleID = Convert.ToInt32(reader[Rule.RULEID_NAME]);
                rule.RuleType = Convert.ToInt32(reader[Rule.RULE_TYPE_NAME]);
                rule.SettingValue = Convert.ToInt32(reader[Rule.SETTING_VALUE_NAME]);
                rule.SettingUnit = Convert.ToString(reader[Rule.SETTING_UNIT_NAME]);

                rule.IsFloor = Convert.ToBoolean(reader[Rule.IS_FLOOR_NAME]);
                rule.IsCeiling = Convert.ToBoolean(reader[Rule.IS_CEILING_NAME]);
                rule.IsAccessible = Convert.ToBoolean(reader[Rule.IS_ACCESSIBLE_NAME]);

                rule.IsForbidden = Convert.ToBoolean(reader[Rule.IS_FORBIDDEN_NAME]);
                rule.ReferenceStandard = Convert.ToString(reader[Rule.REFERENCE_STANDARD_NAME]);
                rule.Description = Convert.ToString(reader[Rule.DESCRIPTION_NAME]);
            }

            return rule;

        }
    }
}
