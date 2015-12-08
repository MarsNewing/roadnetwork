using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.NetworkElement.RoadLayer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RoadNetworkSystem.NetworkExtraction.LinkMasterExtraction
{
    abstract class LinkMasterLayerFactory 
    {
        /// <summary>
        /// 要素类
        /// </summary>
        private IFeatureClass _pFeaClsRoad;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="pFeaClsRoad"></param>
        public LinkMasterLayerFactory(Form1 frm1)
        {
            _pFeaClsRoad = frm1.FeaClsRoad;

        }


        abstract public void AssembleSegmentLayer(List<NodeInfor> NodeInforList,List<string> forbiddenRuls);

        /// <summary>
        /// 构建拓扑
        /// </summary>
        public void GenerateTopology()
        {
            IFeatureDataset pFeatDt = _pFeaClsRoad.FeatureDataset;
            //数据集为空
            if (pFeatDt == null)
            {
                MessageBox.Show("road图层必须放在数据集下面");
                return;
            }

            IGeoDataset pGeoDt = _pFeaClsRoad as IGeoDataset;

            //采用创建拓扑来自动打断
            ITopologyContainer pTopContainer = pFeatDt as ITopologyContainer;
            //拓扑（Topology）是在同一个要素集（FeatureDataset）下的要素类（Feature Class）之间的拓扑关系的集合。
            ITopology pTopo = null;
            
            try
            {
                //判断之前有没建过拓扑
                pTopo = pTopContainer.get_TopologyByName("RoadNodes");
            }
            catch (Exception)
            {
                //没有，则创建RoadNodes的拓扑
                //string Name,拓扑名
                //double ClusterTolerance,组群容限
                //int maxGeneratedErrorCount, 最大的出错数量
                //string ConfigurationKeyword
                //The CreateTopology method creates a topology with the specified name, cluster tolerance,
                //maximum allowable number of errors to be generated and for ArcSDE, with the supplied configuation keyword. 
                //When a topology is initially created, it is empty with no participating feature classes or rules. 没有特定的要素类和规则
                pTopo = pTopContainer.CreateTopology("RoadNodes", pTopContainer.DefaultClusterTolerance, -1, "");
            }

            //The AddClass method is used to add a feature class to the topology, with the specified weight and ranks. 
             //IClass classToAdd,
             //   double Weight,
             //   int XYRank,坐标精度在要素类上的定义，在拓扑生成的时候，他将控制在哪些要素类向另外哪些要素类进行捕捉，1等级最高
             //   int ZRank,
             //   bool EventNotificationOnValidate
            try
            {
                pTopo.AddClass(_pFeaClsRoad, 5, 30, 1, false);
                pTopo.ValidateTopology(pGeoDt.Extent);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            //Notice!!!
            //Notice!!!
            //Notice!!!
            //Notice!!!
            //通过这种建拓扑的方式，使得_pFeaClsRoad在交叉口点处生成一个控制点
        }

        /// <summary>
        /// 提取交叉口结点,返回结点哈希表
        /// </summary>
        /// <param name="forbiddenRules"></param>列表形式，用5\4这种形式
        /// <returns></returns>
        public Hashtable ExtractJunctionNode(List<string> forbiddenRules)
        {
            Hashtable junctionHst = new Hashtable();
            ///第一步：将road当中道路中心线的所有节点加入hastable中，如果是
            ///交叉口，则IsJunction属性为True，否则为数字化产生的点。


            //节点集合
            IPointCollection pPtCol = null;
            string sPt = string.Empty;
            IPoint pPt = null;
            string RoadTypeSet = string.Empty;
            //查找字段的索引，以便赋值

            

            int roadTypeFldIndex = _pFeaClsRoad.Fields.FindField(Road.RoadTypeNm);
            string strRoadType = string.Empty;
            Boolean isJunction;
            //road图形
            IGeometry pGeometry = null;
            //节点的hashtable,用于去除重复节点
            Hashtable nodeHst = new Hashtable();

            IFeatureCursor pFeatCursor = _pFeaClsRoad.Search(null, true);
            IFeature pFeature = pFeatCursor.NextFeature();
            //遍历所有的Road
            while (pFeature != null)
            {

                pGeometry = pFeature.ShapeCopy;

                //_pFeaClsRoad转为点集
                pPtCol = pGeometry as IPointCollection;
                //RoadType
                strRoadType = pFeature.get_Value(roadTypeFldIndex).ToString();

                //遍历Road上所有的控制点
                for (int i = 0; i < pPtCol.PointCount; i++)
                {
                    pPt = pPtCol.get_Point(i);

                    //坐标
                    sPt = string.Format("{0}_{1}", pPt.X, pPt.Y);

                    //若该节点为道路的起点或终点，也算作交叉口
                    if (i == 0 || i == pPtCol.PointCount - 1)
                    {
                        NodeInfor nodeInfor = new NodeInfor(0, pPt, strRoadType, true);
                        //坐标做key，实体做value
                        if (junctionHst.Contains(sPt) == false)
                        {

                            junctionHst.Add(sPt, nodeInfor);
                        }

                    }
                    else
                    {
                        //判断节点是否已经添加到列表
                        //不再列表里，就添加进去
                        if (nodeHst.Contains(sPt) == false)
                        {
                            NodeInfor nodeInfor = new NodeInfor(0, pPt, strRoadType, false);
                            nodeHst.Add(sPt, nodeInfor);
                        }
                        //若节点在列表中，则要获取它的道路类型，并累加记录，从而得到节点所相连的所有RoadType
                        else
                        {
                            NodeInfor oStruct = nodeHst[sPt] as NodeInfor;
                            string tempS = strRoadType + "\\" + oStruct.RoadTypeSet;
                            oStruct.RoadTypeSet = tempS;
                            oStruct.IsJunction = true;
                        }

                    }
                   
                }
                pFeature = pFeatCursor.NextFeature();
            }

            ///第二步：将交叉口中，不该打断的点排除，不该打断的点的规则如下：

            //规则1：节点由roadType=1与roadtype=3产生的点要剔除
            //规则2：如节点由平面道路与匝道相连，并且相连于匝道的起点或终点，不剔除，否则剔除之。            
            //规则3：如节点由非平面道路与匝道相连，并且相连于匝道的起点或终点，不剔除，否则剔除之。

            foreach (DictionaryEntry item in nodeHst)
            {

                //取出列表中节点信息
                NodeInfor nodeInfor = item.Value as NodeInfor;
                isJunction = nodeInfor.IsJunction;

                //若该节点为交叉口，再按照规则对交叉口进行筛选
                if (isJunction == true)
                {
                    strRoadType = nodeInfor.RoadTypeSet;
                    bool temBool = checkContainedInRule(strRoadType, forbiddenRules);
                    if (temBool == false)
                    {
                        if (junctionHst.Contains(item.Key) == false)
                        {
                            junctionHst.Add(item.Key, item.Value);
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            return junctionHst;
        }

        /// <summary>
        /// 获取_pFeaClsRoad上所有点的信息（几何、所属的所有路段的RoadType、是否为Junction、Node的ID）
        /// </summary>
        /// <returns></returns>
        private Hashtable getNodesInfor()
        {
            Hashtable classifiedNodes = new Hashtable();

            ///第一步：将road当中所有的道路中心线的节点加入hastable中，如果是
            ///交叉口，则IsInterchange属性为True，否则为数字化产生的点。
           

            //节点集合
            IPointCollection pPtCol = null;
            string sPt = string.Empty;
            IPoint pPt = null;
            string RoadTypeSet = string.Empty;
            //查找字段的索引，以便赋值

            
            string strRoadType = string.Empty;
            //road图形
            IGeometry pGeometry = null;
            

            IFeatureCursor pFeatCursor = _pFeaClsRoad.Search(null, true);
            IFeature pFeature = pFeatCursor.NextFeature();

            while (pFeature != null)
            {

                pGeometry = pFeature.ShapeCopy;

                pPtCol = pGeometry as IPointCollection;
                strRoadType = pFeature.get_Value(_pFeaClsRoad.FindField("RoadType")).ToString();
                for (int i = 0; i < pPtCol.PointCount; i++)
                {
                    pPt = pPtCol.get_Point(i);
                    //作为Key
                    sPt = string.Format("{0}_{1}", pPt.X, pPt.Y);

                    #region -------------------判断节点是否已经添加到列表-----------------------
                    //如果不再列表中，添加
                    if (classifiedNodes.Contains(sPt) == false)
                    {
                        //若该节点为道路的起点或终点，也算作交叉口
                        if (i == 0 || i == pPtCol.PointCount - 1)
                        {
                            NodeInfor nodeInfor = new NodeInfor(0, pPt, strRoadType, true);
                            classifiedNodes.Add(sPt, nodeInfor);
                        }
                        else
                        {
                            NodeInfor nodeInfor = new NodeInfor(0, pPt, strRoadType, false);
                            classifiedNodes.Add(sPt, nodeInfor);
                        }
                    }
                    #endregion -------------------判断节点是否已经添加到列表-----------------------

                    #region -------若节点在列表中，则要获取它的道路类型，并累加记录，从而得到节点所相连的所有RoadType-------
                    else
                    {
                        //体现出拓扑构建，在两条_pFeaClsRoad相交处生成控制点的作用
                        //Notice!!!
                        //Notice!!!
                        //Notice!!!
                        //Notice!!!
                        NodeInfor oStruct = classifiedNodes[sPt] as NodeInfor;
                        string tempS = strRoadType + "\\" + oStruct.RoadTypeSet;
                        oStruct.RoadTypeSet = tempS;
                        oStruct.IsJunction = true;
                    }

                    #endregion -------若节点在列表中，则要获取它的道路类型，并累加记录，从而得到节点所相连的所有RoadType-------
                }
                pFeature = pFeatCursor.NextFeature();
            }
            return classifiedNodes;
        }

        /// <summary>
        /// 由前端的交互数据更新已经分类好的控制点，
        /// </summary>
        /// <param name="hstJunction"></param>利用哈希表是引用类型
        /// <param name="NodeInforList"></param>
        /// <returns></returns>
        public void UpdateJunctionNode(Hashtable hstJunction,List<NodeInfor> NodeInforList)
        {
            if (NodeInforList != null)
            {
                foreach (NodeInfor item in NodeInforList)
                {
                    string key = item.Point.X.ToString() + "_" + item.Point.Y.ToString();
                    if (hstJunction.ContainsKey(key))
                    {

                        NodeInfor temNodeInfor = hstJunction[key] as NodeInfor;
                        //输入的结点与分类后的不同，更新分类中的信息
                        //利用引用类型，直接更新

                        if (temNodeInfor.IsJunction != item.IsJunction)
                        {
                            temNodeInfor.IsJunction = item.IsJunction;
                            temNodeInfor.NodeID = item.NodeID;
                            temNodeInfor.RoadTypeSet = item.RoadTypeSet;
                            hstJunction.Remove(key);
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }
            else
            {
                return;
            }
        }

        //创建交叉口结点层
        abstract public void CreateNode(Hashtable hstNode);

        //创建路段层
        abstract public void CreateSegment(Hashtable hstNode);

        /// <summary>
        /// 判断给出的roadtype集中是否包含禁止打断的规则，含有，返回true；不含有，返回false；
        /// </summary>
        /// <param name="roadTypes"></param>
        /// <param name="forbiddenRuls"></param>
        /// <returns></returns>
        public static bool checkContainedInRule(string roadTypes,List<string> forbiddenRuls)
        {

            foreach (string item in forbiddenRuls)
            {
                string[] temStrs = item.Split('\\');
                string[] roadType = roadTypes.Split('\\');
                if (roadType[0].Equals(temStrs[0]))
                {
                    if (roadType[1].Equals(temStrs[1]))
                    {
                        return true;
                    }
                }
                else if(roadType[0].Equals(temStrs[1]))
                {
                    if(roadType[1].Equals(temStrs[0]))
                    {
                        return true;
                    }
                }
                else
                {
                    continue;
                }
            }
            return false;
        }
    }



}
