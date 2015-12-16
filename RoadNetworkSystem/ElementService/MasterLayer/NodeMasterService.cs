using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.DataModel.Road;
using RoadNetworkSystem.GIS;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace RoadNetworkSystem.NetworkElement.MasterLayer
{
    class NodeMasterService
    {

        #region ++++++++++++++++定义Node的属性字段名，其他地方需要，只能在这里取+++++++++++++++++++++++++

        /// <summary>
        /// 规定了数据模型，请不要在其他类中直接读取数据
        /// </summary>
        public string NodeIDNm;
        public string CompositeTypeNm;
        public string NodeTypeNm;

        public string AdjIDsNm;
        public string NorthAnglesNm;
        public string ConnStateNm;

        public string OtherNm;

        #endregion ++++++++++++++++定义Node的属性字段名，其他地方需要，只能在这里取+++++++++++++++++++++++++


        public IFeatureClass FeaClsNode;
        public IFeature NodeMasterFea;
        public int Id;

        private IPoint _pnt;
        

        public struct DataInfo
        {
            public int LinkID;  //弧段ID
            public double X;    //弧段的邻近控制点坐标
            public double Y;
        }

        public NodeMasterService(IFeatureClass pFeaClsSegment, int nodeID, IPoint pnt)
        {
            if (pFeaClsSegment != null)
            {
                FeaClsNode = pFeaClsSegment;
                Id = nodeID;
                _pnt = pnt;

            }
        }

        public IFeature GetFeature()
        {
            IFeatureCursor cursor;
            IQueryFilter filer = new QueryFilterClass();
            filer.WhereClause = String.Format("{0}={1}", NodeIDNm, Id);
            cursor = FeaClsNode.Search(filer, false);
            IFeature nodeFeature = cursor.NextFeature();
            System.GC.Collect();                                         //强制对所有代进行垃圾回收。
            System.GC.WaitForPendingFinalizers(); 
            if (nodeFeature != null)
            {
                return nodeFeature;
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// 已知ID，想要用这个，需要先通过ID获取Feature，然后作为输入参数
        /// </summary>
        /// <param name="nodeFea"></param>
        /// <returns></returns>
        public NodeMaster GetNodeMasterEty(IFeature nodeFea)
        {
            NodeMaster nodeMaster = new NodeMaster();
            try
            {
                if (FeaClsNode.FindField(NodeIDNm) >= 0)
                    nodeMaster.ID = Convert.ToInt32(nodeFea.get_Value(FeaClsNode.FindField(NodeIDNm)));
                

                if (FeaClsNode.FindField(CompositeTypeNm) >= 0)
                    nodeMaster.CompositeType = Convert.ToInt32(nodeFea.get_Value(FeaClsNode.FindField(CompositeTypeNm)));
                if (FeaClsNode.FindField(NodeTypeNm) >= 0)
                    nodeMaster.NodeType = Convert.ToInt32(nodeFea.get_Value(FeaClsNode.FindField(NodeTypeNm)));
                if (FeaClsNode.FindField(AdjIDsNm) >= 0)
                    nodeMaster.AdjIDs = Convert.ToString(nodeFea.get_Value(FeaClsNode.FindField(AdjIDsNm)));

                if (FeaClsNode.FindField(NorthAnglesNm) >= 0)
                    nodeMaster.NorthAngles = Convert.ToString(nodeFea.get_Value(FeaClsNode.FindField(NorthAnglesNm)));
                if (FeaClsNode.FindField(ConnStateNm) >= 0)
                    nodeMaster.ConnState = Convert.ToString(nodeFea.get_Value(FeaClsNode.FindField(ConnStateNm)));
                if (FeaClsNode.FindField(OtherNm) >= 0)
                    nodeMaster.Other = Convert.ToInt32(nodeFea.get_Value(FeaClsNode.FindField(OtherNm)));

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            return nodeMaster;
        }


        /// <summary>
        /// 创建Node，如果Id>0,则直接赋值，否则赋为OID
        /// </summary>
        /// <param name="nodeMasterEty"></param>
        public IFeature CreateNode(NodeMaster nodeMasterEty)
        {
            //该函数实现在_pFeaCls创建点pnt，并令其ID为NodeID
            int lFld;
            IFeature crtPntFeat;

            ////“不能再打开其他表了”的原始提示上分析，问题就是要素类的表被打开太多次了，最终的解决方案，强制一次垃圾回收
            System.GC.Collect();      //强制对所有代进行垃圾回收
            System.GC.WaitForPendingFinalizers();  //挂起当前线程，直到处理终结器队列的线程清空该队列为止

            crtPntFeat = FeaClsNode.CreateFeature();
            crtPntFeat.Shape = _pnt;

            if (FeaClsNode.FindField(NodeIDNm) >= 0)
                if (nodeMasterEty.ID > 0)
                {
                    crtPntFeat.set_Value(FeaClsNode.FindField(NodeIDNm), nodeMasterEty.ID);
                }
                else
                {
                    crtPntFeat.set_Value(FeaClsNode.FindField(NodeIDNm), crtPntFeat.OID);
                }
            if (FeaClsNode.FindField(CompositeTypeNm) >= 0)
                crtPntFeat.set_Value(FeaClsNode.FindField(CompositeTypeNm), nodeMasterEty.CompositeType);
            if (FeaClsNode.FindField(NodeTypeNm) >= 0)
                crtPntFeat.set_Value(FeaClsNode.FindField(NodeTypeNm), nodeMasterEty.NodeType);
            if (FeaClsNode.FindField(OtherNm) >= 0)
                crtPntFeat.set_Value(FeaClsNode.FindField(OtherNm), nodeMasterEty.Other);

            crtPntFeat.Store();
            return crtPntFeat;
        }

        public void CreateAdjData(LinkMasterService seg)
        {
            IFeature pFeatureNode = GetFeature();



            //获取相邻LInk的信息（ID, 领接点的坐标）
            List<DataInfo> linkDataInf = new List<DataInfo>();
            linkDataInf = GetAdjInfo(seg);
            //获取_node相邻的Link以及其与正北方向的夹角
            Dictionary<int, int> adjDic = new Dictionary<int, int>();
            adjDic = GetAdjData(linkDataInf);

            string adjIDs = "";
            string adjAngles = "";
            foreach (var tem in adjDic)
            {
                adjIDs = adjIDs + tem.Key.ToString() + '\\';
                adjAngles = adjAngles + tem.Value.ToString() + '\\';
            }
            if (adjIDs.Length > 0)
                adjIDs = adjIDs.Substring(0, adjIDs.Length - 1);
            if (adjAngles.Length > 0)
                adjAngles = adjAngles.Substring(0, adjAngles.Length - 1);


            pFeatureNode.set_Value(FeaClsNode.FindField(AdjIDsNm), adjIDs);
            pFeatureNode.set_Value(FeaClsNode.FindField(NorthAnglesNm), adjAngles);
            pFeatureNode.Store();
        }

        private Dictionary<int, int> GetAdjData(List<DataInfo> linkInforList)
        {
            double X = _pnt.X;
            double Y = _pnt.Y;
            Dictionary<int, int> adjDic = new Dictionary<int, int>();
            //计算从正北向开始的邻近弧段 
            int i, tmpsub, tmpminangle;
            Boolean tmpBln;
            double PIang;
            int ang;
            int[] northAngle = new int[linkInforList.Count];     //用于存储正北向夹角

            //1。为每一Link计算正北向夹角，并存储于northAngle中
            for (i = 0; i < linkInforList.Count; i++)
            {
                if ((linkInforList[i].Y - Y == 0) && (linkInforList[i].X - X) > 0)
                    northAngle[i] = 90;                                         //对异常情况作特别处理
                else if ((linkInforList[i].Y - Y == 0) && (linkInforList[i].X - X) < 0)
                    northAngle[i] = 270;                                        //对异常情况作特别处理
                else
                {
                    PIang = Math.Atan((linkInforList[i].X - X) / (linkInforList[i].Y - Y));       //计算弧度
                    ang = (int)(PIang * 180 / 3.14 + 0.5);                                              //获取角度

                    //分情况赋值
                    if (linkInforList[i].Y - Y > 0)
                        northAngle[i] = (ang + 360) % 360;   //北向角在(0,180)之间
                    else
                        northAngle[i] = ang + 180;     //北向角在(180,360)之间
                }
            }

            //2。按从正北向开始顺时针旋转的先后顺序将数据存储入adjLinkAngle中

            do
            {
                //本循环每次都找出最小的正北向角，并把相关信息存储入tmpAngle和tmpLink中
                tmpBln = false;         //控制循环是否结束的变量
                tmpsub = -1;
                tmpminangle = 370; //定义一个最大的角度

                //找出最小角
                for (i = 0; i < northAngle.Length; i++)
                {
                    if (northAngle[i] < tmpminangle)
                    {
                        tmpminangle = northAngle[i];
                        tmpsub = i;
                        tmpBln = true;

                    }
                }

                try
                {

                    if (tmpsub > -1)
                    {
                        
                        

                        if (adjDic.ContainsKey(linkInforList[tmpsub].LinkID) == false)
                        {
                            adjDic.Add(linkInforList[tmpsub].LinkID, northAngle[tmpsub]);
                        }

                        northAngle[tmpsub] = 370;
                        tmpsub = -1;

                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("生成NodeID为"+Id.ToString()+"的相邻数据时出错/n"+ex.ToString());
                }
                    
                
            } while (tmpBln);

            return adjDic;
        }

        private List<DataInfo> GetAdjInfo(LinkMasterService seg)
        {
            List<DataInfo> listLinkData = new List<DataInfo>();
            IFeatureCursor fCursorLink;
            IQueryFilter pFilter = new QueryFilterClass();
            pFilter.WhereClause = String.Format("{0} = {1}", seg.FNodeIDNm, Id);
            fCursorLink = seg.FeaClsLinkMaster.Search(pFilter, false);
            //起点
            IFeature pFeatLink = fCursorLink.NextFeature();
            int lFld = 0;
            while (pFeatLink != null)
            {
                DataInfo linkData = new DataInfo();

                lFld = fCursorLink.FindField(seg.IDNm);
                linkData.LinkID = Convert.ToInt32(pFeatLink.get_Value(lFld)); //记录下对应的弧段ID

                IPolyline line1 = pFeatLink.Shape as IPolyline;

                IPointCollection pntClo1 = line1 as IPointCollection;

                IPoint fromPoint = pntClo1.get_Point(1);

                linkData.X = fromPoint.X;
                linkData.Y = fromPoint.Y;

                listLinkData.Add(linkData);                                     //存储入listLinkData中

                pFeatLink = fCursorLink.NextFeature();
            }

            #region 以node为终点的link
            pFilter.WhereClause = String.Format("{0} = {1}", seg.TNodeIDNm, Id);
            fCursorLink = seg.FeaClsLinkMaster.Search(pFilter, false);
            pFeatLink = fCursorLink.NextFeature();
            while (pFeatLink != null)
            {
                DataInfo linkData = new DataInfo();
                lFld = fCursorLink.FindField(seg.IDNm);
                linkData.LinkID = Convert.ToInt32(pFeatLink.get_Value(lFld)); //记录下对应的LinkID

                IPolyline line2 = pFeatLink.Shape as IPolyline;

                IPointCollection pntClo2 = line2 as IPointCollection;

                IPoint toPoint = pntClo2.get_Point(pntClo2.PointCount - 2);

                linkData.X = toPoint.X;
                linkData.Y = toPoint.Y;
                listLinkData.Add(linkData);
                pFeatLink = fCursorLink.NextFeature();
            }
            #endregion

            return listLinkData;
        }
    }
}
