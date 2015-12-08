using RoadNetworkSystem.NetworkExtraction.FreeWay.Model;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadNetworkSystem.NetworkExtraction.FreeWay.Dao
{
    class LaneDao
    {
        OleDbConnection _connection;
        public LaneDao(OleDbConnection connection)
        {
            _connection = connection;
        }

        public void InsertLane(Lane lane)
        {
            string str = "Insert into " + Lane.FEATURE_LANE +
                "(" + Lane.FIELDE_SEGMENT_ID +
                "," + Lane.FIELDE_TYPE +
                "," + Lane.FIELDE_SERIAL +
                ") values" +
                "(" + lane.SegmentID.ToString() +
                "," + lane.Type.ToString() +
                "," + lane.Serial.ToString() +
                ")";
            OleDbCommand cmd = new OleDbCommand(str,_connection);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }


        public Lane SearchLane(int segmentId, int serial)
        {
            string sql = "Select * from " + "["+Lane.FEATURE_LANE  +"]"+
                " where " +
                "[" + Lane.FIELDE_SEGMENT_ID + "]" +
                " = " + segmentId +
                " and " +
                "[" + Lane.FIELDE_SERIAL + "]"+
                " = " + serial;
            OleDbCommand cmd = new OleDbCommand(sql, _connection);
            OleDbDataReader reader = cmd.ExecuteReader();

            Lane lane = new Lane();
            if (reader.Read())
            {
                lane.Serial = serial;
                lane.SegmentID = segmentId;
                lane.LaneID = Convert.ToInt32(reader[Lane.FIELDE_LANE_ID]);
            }
            return lane;
        }



    }
}
