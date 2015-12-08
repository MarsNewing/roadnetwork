using RoadNetworkSystem.ADO.Access;
using RoadNetworkSystem.DataModel.SignRule;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadNetworkSystem.TrafficRule.SignRule
{
    class BoundaryRuleService
    {

        private OleDbConnection _connection;
        public BoundaryRuleService(OleDbConnection connection) 
        {
            _connection = connection;
        }
        public BoundaryRule getRrule(int styleId)
        {
            BoundaryRule boundaryRule = null;
            string sql = "select * from " + 
                BoundaryRule.BOUNDARY_RULE_TABLE_NAME+ 
                " where "+ BoundaryRule.STYLEID_NAME + " = " + styleId;
            OleDbCommand cmd = new OleDbCommand(sql, _connection);
            OleDbDataReader reader = cmd.ExecuteReader();
            
            if (reader.Read())
            {
                boundaryRule = new BoundaryRule();
                boundaryRule.StyleID = styleId;
                boundaryRule.LeftRuleID = Convert.ToInt32(reader[BoundaryRule.LEFT_RULEID_NAME]);
                boundaryRule.RightRuleID = Convert.ToInt32(reader[BoundaryRule.RIGHT_RULEID_NAME]);

                boundaryRule.IsCenterLine = Convert.ToBoolean(reader[BoundaryRule.IS_CENTERLINE_NAME]);
                boundaryRule.IsSideLine = Convert.ToBoolean(reader[BoundaryRule.IS_SIDELINE_NAME]);
            }

            return boundaryRule;

        }


        public int getBoundaryStyleId(BoundaryRule boundaryRule)
        {
            string sql = "select * from " +
                           BoundaryRule.BOUNDARY_RULE_TABLE_NAME +
                           " where " + BoundaryRule.LEFT_RULEID_NAME + " = " + boundaryRule.LeftRuleID +
                           " and " + BoundaryRule.RIGHT_RULEID_NAME + " = " + boundaryRule.RightRuleID +
                           " and " + BoundaryRule.IS_CENTERLINE_NAME + " = " + boundaryRule.IsCenterLine +
                           " and " + BoundaryRule.IS_SIDELINE_NAME + " = " + boundaryRule.IsSideLine;
            OleDbCommand cmd = new OleDbCommand(sql, _connection);
            OleDbDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return Convert.ToInt32(reader[BoundaryRule.STYLEID_NAME]);
            }
            else
            {
                return -1;
            }
        }
             
    }
}
