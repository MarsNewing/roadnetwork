using RoadNetworkSystem.NetworkExtraction.FreeWay.Model;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadNetworkSystem.NetworkExtraction.FreeWay.Dao
{
    class ConnectorDao
    {
        OleDbConnection _connection;
        public ConnectorDao(OleDbConnection connection)
        {
            _connection = connection;
        }

        public void InsertLane(Connector connector)
        {
            string str = "Insert into " + Connector.FEATURE_CONNECTOR +
                "(" + Connector.FIELDE_FROM_LANE_ID +
                "," + Connector.FIELDE_TO_LANE_ID +
                "," + Connector.FIELDE_NODE_ID +
                ") values" +
                "(" +connector.FromLaneID.ToString() +
                "," + connector.ToLaneID.ToString() +
                "," + connector.NodeID.ToString() +
                ")";
            OleDbCommand cmd = new OleDbCommand(str,_connection);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }
    }
}
