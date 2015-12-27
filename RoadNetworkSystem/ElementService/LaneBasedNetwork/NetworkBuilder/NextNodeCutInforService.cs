using ESRI.ArcGIS.Geodatabase;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.Road;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.Connection;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LinkLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadNetworkSystem.ElementService.LaneBasedNetwork.NetworkBuilder
{
    class NextNodeCutInforService
    {
        private IFeatureClass _pFeaClsLink;
        private IFeatureClass _pFeaClsArc;
        public NextNodeCutInforService(IFeatureClass pFeaClsLink,IFeatureClass pFeaClsArc)
        {
            _pFeaClsLink = pFeaClsLink;
            _pFeaClsArc = pFeaClsArc;
        }

        /// <summary>
        /// 获取Arc指向的下一个Node位置处的截取信息
        /// </summary>
        /// <param name="nextNodeEty"></param>
        /// <param name="curArcEty"></param>
        /// <param name="curLinkEty"></param>
        /// <returns></returns>
        public NextNodeCutInfor GetNextNodeCutInfor(Node nextNodeEty, Arc curArcEty,
            Link curLinkEty)
        {

            NextNodeCutInfor nextNodeCurInfor = new NextNodeCutInfor();
            nextNodeCurInfor.cutType = -1;
            nextNodeCurInfor.nextArcEty = new Arc();
            nextNodeCurInfor.curArcID = curArcEty.ArcID;

            int cutType = -1;

            Arc nextArcEty = new Arc();
            string[] adjLinkIDs = nextNodeEty.AdjIDs.Split('\\');

            ///单向标志,true表示为单向，false表示双向；
            bool curOneWayFlag = false;

            //下游路段单向标志
            bool nextOneWayFlag = false;

            if (adjLinkIDs.Length > 1)
            {
                #region -------------------------当前路段--------------------------------------

                ///当前Link的信息
                int curLinkID = curArcEty.LinkID;

                int linkFlowDir = curLinkEty.FlowDir;
                //Link的与Arc的FlowDir相同，说明单向
                if (linkFlowDir == curArcEty.FlowDir)
                {
                    curOneWayFlag = true;
                }
                else
                {
                    curOneWayFlag = false;
                }

                #endregion -------------------------当前路段--------------------------------------

                #region -------------------------下游路段--------------------------------------

                ///逆时针方向的第一个Link,下游路段
                int antiClockLinkID = 0;
                ///逆时针方向的夹角
                double antiClockAngle = 0.0;
                PhysicalConnection.GetAntiClockLinkInfor(curLinkID, nextNodeEty, ref antiClockLinkID, ref antiClockAngle);

                LinkService antiClockLink = new LinkService(_pFeaClsLink, antiClockLinkID);
                IFeature antiClockLinkFea = antiClockLink.GetFeature();
                LinkMaster linkMasterEty = new LinkMaster();
                linkMasterEty = antiClockLink.GetEntity(antiClockLinkFea);
                Link antiClockLinkEty = new Link();
                antiClockLinkEty = antiClockLinkEty.Copy(linkMasterEty);

                //下一段Arc
                IFeature nextArcFea = LogicalConnection.GetExitArc(_pFeaClsArc, nextNodeEty.ID, antiClockLinkEty);

                if (nextArcFea == null)
                {
                    nextNodeCurInfor.nextArcEty = null;
                    nextNodeCurInfor.antiClockAngle = 0;
                    return nextNodeCurInfor;
                }

                ArcService nextArc = new ArcService(_pFeaClsArc, 0);

                nextArcEty = nextArc.GetArcEty(nextArcFea);
                nextArc = new ArcService(_pFeaClsArc, nextArcEty.ArcID);

                if (antiClockLinkEty.FlowDir == nextArcEty.FlowDir)
                {
                    nextOneWayFlag = true;
                }
                else
                {
                    nextOneWayFlag = false;
                }

                #endregion -------------------------下游路段--------------------------------------



                if (curOneWayFlag == false && nextOneWayFlag == false)
                {
                    cutType = 1;
                }
                else if (curOneWayFlag = false && nextOneWayFlag == true)
                {
                    cutType = 2;
                }
                else if (curOneWayFlag = true && nextOneWayFlag == false)
                {
                    cutType = 3;
                }

                else if (curOneWayFlag == true && nextOneWayFlag == true)
                {
                    cutType = 4;
                }

                nextNodeCurInfor.nextNodeEty = nextNodeEty;
                nextNodeCurInfor.curArcID = curArcEty.ArcID;
                nextNodeCurInfor.nextArcEty = nextArcEty;
                nextNodeCurInfor.antiClockAngle = antiClockAngle;
                nextNodeCurInfor.cutType = cutType;
            }
            else
            {
                nextNodeCurInfor.nextNodeEty = nextNodeEty;
                nextNodeCurInfor.curArcID = curArcEty.ArcID;
                nextNodeCurInfor.nextArcEty = null;
                nextNodeCurInfor.cutType = 2;
                nextNodeCurInfor.antiClockAngle = 0;
            }

            return nextNodeCurInfor;

        }

    }
}
