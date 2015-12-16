using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.NetworkElement.LaneBasedNetwork.Connection
{
    class PhysicalConnection
    {
        /// <summary>
        /// 获取某个Link逆时针反向的第一个Link的信息（LinkID、夹角）
        /// </summary>
        /// <param name="curLinkID"></param>
        /// <param name="nextNodeEty"></param>
        /// <param name="antiClockLink"></param>
        /// <param name="antiClockAngle"></param>
        public static void GetAntiClockLinkInfor(int curLinkID, Node nextNodeEty, ref int antiClockLink,ref double antiClockAngle)
        {
            int antiClockLinkIndex = -1;

            string[] adjLinkIDs = nextNodeEty.AdjIDs.Split('\\');
            string[] northAngles = nextNodeEty.NorthAngles.Split('\\');
            int curLinkIndex = getLinkIndex(curLinkID,nextNodeEty);
            

            if (curLinkIndex == 0)
            {
                antiClockLinkIndex = adjLinkIDs.Length - 1;
            }
            else
            {
                antiClockLinkIndex = curLinkIndex - 1;
            }
            antiClockLink = Convert.ToInt32(adjLinkIDs[antiClockLinkIndex]);

            double antiClockNorthAngle = Convert.ToDouble(northAngles[antiClockLinkIndex]);
            double curNorthAngle = Convert.ToDouble(northAngles[curLinkIndex]);

            antiClockAngle = (curNorthAngle - antiClockNorthAngle + 360) % 360;

        }


        /// <summary>
        /// 获取某个Link的顺时针方向的第一个LinkID和顺时针夹角
        /// </summary>
        /// <param name="curLinkID"></param>
        /// <param name="preNodeEty"></param>
        /// <param name="clockLink"></param>
        /// <param name="clockAngle"></param>
        public static void GetClockLinkInfor(int curLinkID, Node preNodeEty, ref int clockLink, ref double clockAngle)
        {
            int clockLinkIndex = -1;

            string[] adjLinkIDs = preNodeEty.AdjIDs.Split('\\');
            string[] northAngles = preNodeEty.NorthAngles.Split('\\');
            int curLinkIndex = getLinkIndex(curLinkID, preNodeEty);


            if (curLinkIndex == adjLinkIDs.Length - 1)
            {
                clockLinkIndex = 0;
            }
            else
            {
                clockLinkIndex = curLinkIndex + 1;
            }
            clockLink = Convert.ToInt32(adjLinkIDs[clockLinkIndex]);

            double clockNorthAngle = Convert.ToDouble(northAngles[clockLinkIndex]);
            double curNorthAngle = Convert.ToDouble(northAngles[curLinkIndex]);

            clockAngle = (clockNorthAngle - curNorthAngle + 360) % 360;

        }


        /// <summary>
        /// 获取某个Link的在AdjLink中的编号
        /// </summary>
        /// <param name="linkID"></param>
        /// <param name="nodeEty"></param>
        /// <returns></returns>
        private static int getLinkIndex(int linkID, Node nodeEty)
        {
            string[] adjLinkIDs = nodeEty.AdjIDs.Split('\\');
            int linkIndex = -1;
            for (int i = 0; i < adjLinkIDs.Length; i++)
            {
                int temLinkID = Convert.ToInt32(adjLinkIDs[i]);
                if (temLinkID == linkID)
                {
                    linkIndex = i;
                }
            }
            return linkIndex;
        }

        public static double GetLinksAngle(int fromLinkID, int toLinkID, Node nodeEty)
        {
            double angle = 0;

            string[] adjLinkIDs = nodeEty.AdjIDs.Split('\\');
            string[] adjAngles = nodeEty.NorthAngles.Split('\\');
            int fromLinkIndex = -1;
            int toLinkIndex = -1;
            for (int i = 0; i < adjLinkIDs.Length; i++)
            {
                int temLinkID = Convert.ToInt32(adjLinkIDs[i]);
                if (temLinkID == fromLinkID)
                {
                    fromLinkIndex = i;
                }
                if (temLinkID == toLinkID)
                {
                    toLinkIndex = i;
                }
            }
            double toLinkAngle = Convert.ToDouble(adjAngles[toLinkIndex]);
            double fromLinkAngle = Convert.ToDouble(adjAngles[fromLinkIndex]);

            angle = (toLinkAngle - fromLinkAngle + 360) % 360;

            return angle;
        }
        public static string GetTurningDir(double turningAngle)
        {
            string TurningDir = "";

            if (turningAngle >= 45 && turningAngle < 135)
                TurningDir = "Left";
            else if (turningAngle >= 135 && turningAngle < 225)
                TurningDir = "Straight";
            else if (turningAngle >= 225 && turningAngle < 315)
                TurningDir = "Right";
            else
                TurningDir = "Uturn";
            return TurningDir;
        }
    }
}
