using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadNetworkSystem.ElementService.LaneBasedNetwork.NetworkBuilder
{
    public class NextNodeCutInfor
    {
        public Node nextNodeEty { get; set; }
        public int curArcID { get; set; }
        public Arc nextArcEty { get; set; }

        /// <summary>
        /// 逆时针方向第一个Link
        /// </summary>
        public double antiClockAngle { get; set; }


        //截取分为4种情况
        /*
         *(1)相邻Arc都是可通行的 cutType=1
         *(2)当前Arc的可通行，下段Arc是不可通行的 cutType=2
         *(3)当前Arc不可通行，下段rc可通行 cutType=3
         *(4)相邻Arc都是不可通行的 cutType=4
         * 
        */
        public int cutType { get; set; }
    }
}
