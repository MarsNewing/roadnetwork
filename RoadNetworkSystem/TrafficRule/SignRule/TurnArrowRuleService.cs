
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadNetworkSystem.TrafficRule.SignRule
{
    class TurnArrowRuleService
    {
        OleDbConnection _conn;
        public TurnArrowRuleService(OleDbConnection conn)
        {
            this._conn = conn;
        }
    }
}
