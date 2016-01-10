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
    class PreNodeCutInforService
    {
        private IFeatureClass _pFeaClsLink;
        private IFeatureClass _pFeaClsArc;
        public PreNodeCutInforService(IFeatureClass pFeaClsLink, IFeatureClass pFeaClsArc)
        {
            _pFeaClsLink = pFeaClsLink;
            _pFeaClsArc = pFeaClsArc;
        }

        /// <summary>
        /// 获取Arc上游的Node位置处的截取信息
        /// </summary>
        /// <param name="preNodeEty"></param>
        /// <param name="curArcEty"></param>
        /// <param name="curLinkEty"></param>
        /// <returns></returns>
        public PreNodeCutInfor GetPreNodeCutInfor(Node preNodeEty, Arc curArcEty, Link curLinkEty)
        {

            PreNodeCutInfor preNodeCurInfor = new PreNodeCutInfor();
            preNodeCurInfor.cutType = -1;
            preNodeCurInfor.preArcEty = new Arc();
            preNodeCurInfor.curArcID = curArcEty.ArcID;

            int cutType = -1;

            Arc preArcEty = new Arc();
            string[] adjLinkIDs = preNodeEty.AdjIDs.Split('\\');

            ///单向标志,true表示为单向，false表示双向；
            bool curOneWayFlag = false;

            //下游路段单向标志
            bool preOneWayFlag = false;

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
                int clockLinkID = 0;
                ///逆时针方向的夹角
                double clockAngle = 0.0;
                PhysicalConnection.GetClockLinkInfor(curLinkID, preNodeEty, ref clockLinkID, ref clockAngle);

                LinkService clockLink = new LinkService(_pFeaClsLink, clockLinkID);
                IFeature clockLinkFea = clockLink.GetFeature();
                LinkMaster linkMasterEty = new LinkMaster();
                linkMasterEty = clockLink.GetEntity(clockLinkFea);
                Link clockLinkEty = new Link();
                clockLinkEty = clockLinkEty.Copy(linkMasterEty);

                //上一段Arc
                IFeature preArcFea = LogicalConnection.GetEntranceArc(_pFeaClsArc, preNodeEty.ID, clockLinkEty);
                ArcService preArc = new ArcService(_pFeaClsArc, 0);
                if (preArcFea == null)
                {
                    preNodeCurInfor.preArcEty = null;
                    preNodeCurInfor.clockAngle = 0;
                    preNodeCurInfor.preNodeEty = preNodeEty;
                    return preNodeCurInfor;
                }

                preArcEty = preArc.GetArcEty(preArcFea);
                preArc = new ArcService(_pFeaClsArc, preArcEty.ArcID);

                if (clockLinkEty.FlowDir == preArcEty.FlowDir)
                {
                    preOneWayFlag = true;
                }
                else
                {
                    preOneWayFlag = false;
                }

                #endregion -------------------------下游路段--------------------------------------



                if (curOneWayFlag == false && preOneWayFlag == false)
                {
                    cutType = 1;
                }
                else if (curOneWayFlag = false && preOneWayFlag == true)
                {
                    cutType = 2;
                }
                else if (curOneWayFlag = true && preOneWayFlag == false)
                {
                    cutType = 3;
                }

                else if (curOneWayFlag == true && preOneWayFlag == true)
                {
                    cutType = 4;
                }

                preNodeCurInfor.preNodeEty = preNodeEty;
                preNodeCurInfor.curArcID = curArcEty.ArcID;
                preNodeCurInfor.preArcEty = preArcEty;
                preNodeCurInfor.clockAngle = clockAngle;
                preNodeCurInfor.cutType = cutType;
            }
            else
            {
                preNodeCurInfor.preNodeEty = preNodeEty;
                preNodeCurInfor.curArcID = curArcEty.ArcID;
                preNodeCurInfor.preArcEty = null;
                preNodeCurInfor.cutType = 2;
                preNodeCurInfor.clockAngle = 0;
            }

            return preNodeCurInfor;

        }
    

    }
}
