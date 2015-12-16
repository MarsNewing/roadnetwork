using ESRI.ArcGIS.Geodatabase;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.Road;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.Connection;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LaneLayer;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LinkLayer;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.NetworkEditor
{
    class CutHelper
    {
        public struct NextNodeCutInfor
        {
            public int curArcID;
            public List<double> curCuts;

            public IFeature nextArcFea;
            public List<double> nextCuts;
        }


        public struct PreNodeCutInfor
        {
            public int curArcID;
            public List<double> curCuts;

            public IFeature preArcFea;
            public List<double> preCuts;
        }





        public struct LinkCutInfor
        {
            public IFeature LinkFea;
            public NextNodeCutInfor SameNextNodeCutInfor;
            public PreNodeCutInfor SamePreNodeCutInfor;

            public NextNodeCutInfor OppNextNodeCutInfor;
            public PreNodeCutInfor OppPreNodeCutInfor;

        }

        /*
         * 
         * 
         * 
         *       +    
         *                      *    |    +
         *                      |    |    |
         *                      |    |    |
         *                      |    |    |
         *                      |    |    |
         *            OppPreArc |    |    |SameNextArc
         *                      |    |    |
         *                      |    |    |
         *                      |    |    |
         *                      |    |    |
         *                      +    |    *
         *                           *    
         * 
         *                           + (TNode)   
         *                           | 
         *  OppPreNodeCutInfor  *    |    +  (SameNextNodeCutInfor)
         *                      |    |    |
         *                      |    |    |
         *                      |    |    |
         *                      |    |    |
         *                      |    |    | 
         *                      |    |    |
         *                      |    |    |
         *                      |    |    |
         *                      |    |    |
         * OppNextNodeCutInfor  +    |    *  (SamePreNodeCutInfor)
         *                           |
         *                           * (FNode)   
                    
         *                      *    |    +
         *                      |    |    |
         *                      |    |    |
         *                      |    |    |
         *                      |    |    |
         *          OppNextArc  |    |    | SamePreArc
         *                      |    |    |
         *                      |    |    |
         *                      |    |    |
         *                      |    |    |
         *                      +    |    *
         *                           *    
         * 
         * 
         * 
         * 
         * 
        */



        public static LinkCutInfor GetLinkCutInfor(Link curLinkEty,
            IFeatureClass pFeaClsNode, IFeatureClass pFeaClsLink, IFeatureClass pFeaClsArc, IFeatureClass pFeaClsLane)
        {
            //初始化
            LinkCutInfor linkCutInfor = new LinkCutInfor();
            linkCutInfor.LinkFea = null;
            linkCutInfor.SameNextNodeCutInfor = new NextNodeCutInfor();
            linkCutInfor.SamePreNodeCutInfor = new PreNodeCutInfor();
            linkCutInfor.OppNextNodeCutInfor = new NextNodeCutInfor();
            linkCutInfor.SamePreNodeCutInfor = new PreNodeCutInfor();

            #region ----------------------------Link端点---------------------------------
            int fNodeID = curLinkEty.FNodeID;
            NodeService fNode = new NodeService(pFeaClsNode, fNodeID, null);

            Node fNodeEty = new Node();
            IFeature fNodeFea = fNode.GetFeature();
            NodeMaster nodeMasterEty = fNode.GetNodeMasterEty(fNodeFea);
            fNodeEty = fNodeEty.Copy(nodeMasterEty);



            int tNodeID = curLinkEty.TNodeID;
            NodeService tNode = new NodeService(pFeaClsNode, tNodeID, null);

            Node tNodeEty = new Node();
            IFeature tNodeFea = tNode.GetFeature();
            NodeMaster tnodeMasterEty = tNode.GetNodeMasterEty(tNodeFea);
            tNodeEty = tNodeEty.Copy(tnodeMasterEty);
            #endregion ----------------------------Link端点---------------------------------

            LinkService curLink = new LinkService(pFeaClsLink, curLinkEty.ID);
            

            IFeatureCursor cursor;
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = curLink.IDNm + " = " + curLinkEty.ID;
            cursor = pFeaClsArc.Search(filter, false);
            IFeature pFeature = cursor.NextFeature();
            while(pFeature!=null)
            {
                ArcService arc = new ArcService(pFeaClsArc, 0);
                Arc arcEty = new Arc();
                arcEty = arc.GetArcEty(pFeature);

                int flowDir = arcEty.FlowDir;
                if (flowDir == 1)
                {
                    linkCutInfor.SameNextNodeCutInfor = GetNextNodeCut(curLinkEty, arcEty, tNodeEty,
                        pFeaClsNode, pFeaClsLink, pFeaClsArc, pFeaClsLane);

                    linkCutInfor.SamePreNodeCutInfor = GetPreNodeCut(curLinkEty, arcEty, fNodeEty,
                        pFeaClsNode, pFeaClsLink, pFeaClsArc, pFeaClsLane);
                }

                if (flowDir == -1)
                {
                    linkCutInfor.OppNextNodeCutInfor = GetNextNodeCut(curLinkEty, arcEty, fNodeEty,
                        pFeaClsNode, pFeaClsLink, pFeaClsArc, pFeaClsLane);

                    linkCutInfor.OppPreNodeCutInfor = GetPreNodeCut(curLinkEty, arcEty, tNodeEty,
                        pFeaClsNode, pFeaClsLink, pFeaClsArc, pFeaClsLane);
                }
                pFeature = cursor.NextFeature();
            }

            linkCutInfor.LinkFea = curLink.GetFeature();
            return linkCutInfor;
        }

        /// <summary>
        /// 以Arc为生成单元，获取的车道右侧边界线的截头截尾的大小
        /// </summary>
        /// <param name="laneEty"></param>车道的实体
        /// <param name="nextNodeEty"></param>下游Node实体
        /// <param name="pFeaClsNode"></param>Node要素类
        /// <param name="pFeaClsLink"></param>Link要素类
        /// <param name="pFeaClsArc"></param>Arc要素类
        /// <param name="pFeaClsLane"></param>Lane要素类
        /// <param name="tacCut"></param>返回值，逆时针Node的截取量
        /// <param name="fcCut"></param>返回值，上游顺时针Node的截取量
        public static NextNodeCutInfor GetNextNodeCut(Link curLinkEty, Arc arcEty, Node nextNodeEty,
            IFeatureClass pFeaClsNode,IFeatureClass pFeaClsLink,IFeatureClass pFeaClsArc,IFeatureClass pFeaClsLane)
        {
            
            NextNodeCutInfor nodeCutInfor = new NextNodeCutInfor();
            string[] adjLinkIDs = nextNodeEty.AdjIDs.Split('\\');
            int cutType = -1;

            ///单向标志,true表示为单向，false表示双向；
            bool curOneWayFlag = false;

            //下游路段单向标志
            bool nextOneWayFlag = false;

            if (adjLinkIDs.Length > 1)
            {
                #region -------------------------当前路段--------------------------------------

                ///当前Link的信息
                int curLinkID = arcEty.LinkID;

                int linkFlowDir = curLinkEty.FlowDir;
                //Link的与Arc的FlowDir相同，说明单向
                if (linkFlowDir == arcEty.FlowDir)
                {
                    curOneWayFlag = true;
                }
                else
                {
                    curOneWayFlag = false;
                }

                #endregion -------------------------当前路段--------------------------------------

                /*
             * 
             * 
             *
             * 
            */

                #region -------------------------下游路段--------------------------------------

                ///逆时针方向的第一个Link,下游路段
                int antiClockLinkID = 0;
                ///逆时针方向的夹角
                double antiClockAngle = 0.0;
                PhysicalConnection.GetAntiClockLinkInfor(curLinkID, nextNodeEty, ref antiClockLinkID, ref antiClockAngle);

                LinkService antiClockLink = new LinkService(pFeaClsLink, antiClockLinkID);
                IFeature antiClockLinkFea = antiClockLink.GetFeature();
                LinkMaster linkMasterEty = new LinkMaster();
                linkMasterEty= antiClockLink.GetEntity(antiClockLinkFea);
                Link antiClockLinkEty = new Link();
                antiClockLinkEty = antiClockLinkEty.Copy(linkMasterEty);

                //下一段Arc
                IFeature nextArcFea = LogicalConnection.GetExitArc(pFeaClsArc, nextNodeEty.ID, antiClockLinkEty);
                ArcService nextArc = new ArcService(pFeaClsArc, 0);
                Arc nextArcEty = nextArc.GetArcEty(nextArcFea);
                nextArc = new ArcService(pFeaClsArc, nextArcEty.ArcID);

                if (antiClockLinkEty.FlowDir == nextArcEty.FlowDir)
                {
                    nextOneWayFlag = true;
                }
                else
                {
                    nextOneWayFlag = false;
                }
                #endregion -------------------------下游路段--------------------------------------




                //截取分为三种情况
                /*
                 *(1)相邻Arc都是可通行的 cutType=1
                 *(2)到达Arc是不可通行的 cutType=2
                 *(3)起始Arc不可通行，到达Arc可通行 cutType=3
                 *(3)相邻Arc都是不可通行的 cutType=4
                 * 
                */

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


                ///初始化NodeCutInfor
                nodeCutInfor.nextArcFea = nextArcFea;
                nodeCutInfor.curArcID = arcEty.ArcID;
                nodeCutInfor.curCuts = new List<double>();
                nodeCutInfor.nextCuts = new List<double>();
                List<double> antiClockCuts = new List<double>();
                List<double> clockCuts = new List<double>();

                #region ----------------------遍历当前Arc所有的Lane--------------------------------
                IQueryFilter filter = new QueryFilterClass();
                filter.WhereClause = String.Format("{0} = {1}", ArcService.ArcIDNm, arcEty.ArcID);
                IFeatureCursor cursor;
                cursor = pFeaClsLane.Search(filter, false);
                IFeature pFeatureCurLane = cursor.NextFeature();

                //当前车道的宽度
                double curWidth = 0;
                double nextArcWidth = nextArc.GetArcWidth(pFeaClsLane);

                //遍历所有的Lane
                while (pFeatureCurLane != null)
                {
                    LaneFeatureService laneFea = new LaneFeatureService(pFeaClsLane, 0);
                    Lane temLaneEnty = laneFea.GetEntity(pFeatureCurLane);
                    curWidth = curWidth + temLaneEnty.Width;
                    if (cutType == 1)
                    {
                        double cut = getCutDefualt(curWidth, nextArcWidth, antiClockAngle);
                        antiClockCuts.Add(cut);
                    }
                    else if (cutType == 2)
                    {
                        double cut = getCutOneway(curWidth, antiClockAngle);
                        antiClockCuts.Add(cut);
                    }

                    pFeatureCurLane = cursor.NextFeature();
                }
                #endregion ----------------------遍历当前Arc所有的Lane--------------------------------


                double curLaneWidth = curWidth;
                curWidth = 0;


                #region ----------------------遍历下游Arc所有的Lane--------------------------------
                IQueryFilter filter2 = new QueryFilterClass();
                filter2.WhereClause = String.Format("{0} = {1}", ArcService.ArcIDNm, nextArcEty.ArcID);
                IFeatureCursor cursor2;
                cursor2 = pFeaClsLane.Search(filter2, false);
                IFeature pFeaNextLane = cursor2.NextFeature();
                while (pFeaNextLane != null)
                {
                    LaneFeatureService laneFea = new LaneFeatureService(pFeaClsLane, 0);
                    Lane temLaneEnty = laneFea.GetEntity(pFeatureCurLane);
                    curWidth = curWidth + temLaneEnty.Width;
                    if (cutType == 1)
                    {
                        double cut = getCutDefualt(curWidth, curLaneWidth, antiClockAngle);
                        clockCuts.Add(cut);
                    }
                    else if (cutType == 3)
                    {
                        double cut = getCutOneway(curWidth, antiClockAngle);
                        clockCuts.Add(cut);
                    }

                    pFeaNextLane = cursor2.NextFeature();
                }

                #endregion ----------------------遍历下游Arc所有的Lane--------------------------------

                //PhysicalConnection.GetAntiClockLinkInfor(,)

                nodeCutInfor.curCuts = antiClockCuts;
                nodeCutInfor.nextCuts = clockCuts;
            }
            else
            {

                ///初始化NodeCutInfor
                nodeCutInfor.nextArcFea = null;
                nodeCutInfor.curArcID = arcEty.ArcID;
                nodeCutInfor.curCuts = new List<double>();
                nodeCutInfor.nextCuts = new List<double>();
                List<double> antiClockCuts = new List<double>();
                List<double> clockCuts = new List<double>();
                for (int i = 0; i < arcEty.LaneNum; i++)
                {
                    antiClockCuts.Add(0);
                }

                nodeCutInfor.curCuts = antiClockCuts;
                nodeCutInfor.nextCuts = clockCuts;
            }
          

            return nodeCutInfor;
        }


        public static PreNodeCutInfor GetPreNodeCut(Link curLinkEty, Arc curArcEty, Node preNodeEty,
            IFeatureClass pFeaClsNode, IFeatureClass pFeaClsLink, IFeatureClass pFeaClsArc, IFeatureClass pFeaClsLane)
        {
            PreNodeCutInfor nodeCutInfor = new PreNodeCutInfor();
            
            string[] adjLinkIDs = preNodeEty.AdjIDs.Split('\\');
            int cutType = -1;

            ///单向标志,true表示为单向，false表示双向；
            bool curOneWayFlag = false;

            //上游路段单向标志
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

                /*
             * 
             * 
             *
             * 
            */

                #region -------------------------下游路段--------------------------------------

                ///顺时针方向的第一个Link,上游路段
                int clockLinkID = 0;
                ///逆时针方向的夹角
                double clockAngle = 0.0;
                PhysicalConnection.GetClockLinkInfor(curLinkID, preNodeEty, ref clockLinkID, ref clockAngle);

                LinkService clockLink = new LinkService(pFeaClsLink, clockLinkID);
                IFeature clockLinkFea = clockLink.GetFeature();
                LinkMaster linkMasterEty = new LinkMaster();
                linkMasterEty = clockLink.GetEntity(clockLinkFea);
                Link clockLinkEty = new Link();
                clockLinkEty = clockLinkEty.Copy(linkMasterEty);

                //下一段Arc
                IFeature preArcFea = LogicalConnection.GetEntranceArc(pFeaClsArc, preNodeEty.ID, clockLinkEty);
                ArcService preArc = new ArcService(pFeaClsArc, 0);

                //更新preArc
                Arc preArcEty = preArc.GetArcEty(preArcFea);
                preArc = new ArcService(pFeaClsArc, preArcEty.ArcID);

                if (clockLinkEty.FlowDir == preArcEty.FlowDir)
                {
                    preOneWayFlag = true;
                }
                else
                {
                    preOneWayFlag = false;
                }
                #endregion -------------------------下游路段--------------------------------------




                //截取分为三种情况
                /*
                 *(1)相邻Arc都是可通行的 cutType=1
                 *(2)到达Arc是不可通行的 cutType=2
                 *(3)起始Arc不可通行，到达Arc可通行 cutType=3
                 *(3)相邻Arc都是不可通行的 cutType=4
                 * 
                */

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

                ArcService curArc = new ArcService(pFeaClsArc, curArcEty.ArcID);
                

                ///初始化NodeCutInfor
                nodeCutInfor.preArcFea = preArcFea;
                nodeCutInfor.curArcID = curArcEty.ArcID;
                nodeCutInfor.curCuts = new List<double>();
                nodeCutInfor.preCuts = new List<double>();
                List<double> curCuts = new List<double>();
                List<double> preCuts = new List<double>();

                #region ----------------------遍历当前Arc所有的Lane--------------------------------
                IQueryFilter filter = new QueryFilterClass();
                filter.WhereClause = String.Format("{0} = {1}", ArcService.ArcIDNm, curArcEty.ArcID);
                IFeatureCursor cursor;
                cursor = pFeaClsLane.Search(filter, false);
                IFeature pFeatureCurLane = cursor.NextFeature();

                //当前车道的宽度
                double curWidth = 0;
                
                double preArcWidth = preArc.GetArcWidth(pFeaClsLane);

                //遍历所有的Lane
                while (pFeatureCurLane != null)
                {
                    LaneFeatureService laneFea = new LaneFeatureService(pFeaClsLane, 0);
                    Lane temLaneEnty = laneFea.GetEntity(pFeatureCurLane);
                    curWidth = curWidth + temLaneEnty.Width;
                    if (cutType == 1)
                    {
                        double cut = getCutDefualt(curWidth, preArcWidth, clockAngle);
                        curCuts.Add(cut);
                    }
                    else if (cutType == 2)
                    {
                        double cut = getCutOneway(curWidth, clockAngle);
                        curCuts.Add(cut);
                    }

                    pFeatureCurLane = cursor.NextFeature();
                }
                #endregion ----------------------遍历上游Arc所有的Lane--------------------------------


                double curLaneWidth = curWidth;
                curWidth = 0;


                #region ----------------------遍历下游Arc所有的Lane--------------------------------
                IQueryFilter filter2 = new QueryFilterClass();
                filter2.WhereClause = String.Format("{0} = {1}", ArcService.ArcIDNm, preArcEty.ArcID);
                IFeatureCursor cursor2;
                cursor2 = pFeaClsLane.Search(filter2, false);
                IFeature pFeaNextLane = cursor2.NextFeature();
                while (pFeaNextLane != null)
                {
                    LaneFeatureService laneFea = new LaneFeatureService(pFeaClsLane, 0);
                    Lane temLaneEnty = laneFea.GetEntity(pFeatureCurLane);
                    curWidth = curWidth + temLaneEnty.Width;
                    if (cutType == 1)
                    {
                        double cut = getCutDefualt(curWidth, curLaneWidth, clockAngle);
                        preCuts.Add(cut);
                    }
                    else if (cutType == 3)
                    {
                        double cut = getCutOneway(curWidth, clockAngle);
                        preCuts.Add(cut);
                    }

                    pFeaNextLane = cursor2.NextFeature();
                }

                #endregion ----------------------遍历下游Arc所有的Lane--------------------------------

                //PhysicalConnection.GetAntiClockLinkInfor(,)

                nodeCutInfor.curCuts = curCuts;
                nodeCutInfor.preCuts = preCuts;
            }
            else
            {

                ///初始化NodeCutInfor
                nodeCutInfor.preArcFea = null;
                nodeCutInfor.curArcID = curArcEty.ArcID;
                nodeCutInfor.curCuts = new List<double>();
                nodeCutInfor.preCuts = new List<double>();
                List<double> curCuts = new List<double>();
                List<double> preCuts = new List<double>();
                for (int i = 0; i < curArcEty.LaneNum; i++)
                {
                    curCuts.Add(0);
                }

                nodeCutInfor.curCuts = curCuts;
                nodeCutInfor.preCuts = preCuts;
            }
            return nodeCutInfor;
        }

        public static double getCutDefualt(double curWidth, double adjWidth, double angel)
        {
            double cut = 0.0;
            double angleInArc=angel * Math.PI / 180;
            if (angel == 90)
            {
                cut = adjWidth;
            }
            else if (angel == 180 || angel == 0)
            {
                cut = 0;
            }
            else
            {
                cut = (adjWidth / Math.Sin(angleInArc)) + (curWidth / Math.Tan(angleInArc));
            }
            return cut;
        }

        public static double getCutOneway(double curWidth, double angel)
        {
            double cut = 0.0;
            cut = Math.Tan(angel * Math.PI/180);
            return cut;
        }
    }

    //class 
}
