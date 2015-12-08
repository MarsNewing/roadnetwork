using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.DataModel.GuideSignNetwork;
using RoadNetworkSystem.DataModel.Road;
using RoadNetworkSystem.NetworkElement.GuideSignNetwork;
using RoadNetworkSystem.NetworkElement.RoadLayer;
using RoadNetworkSystem.NetworkExtraction.LaneBasedNetwork.DatabaseManager;
using RoadNetworkSystem.NetworkExtraction.LinkMasterExtraction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RoadNetworkSystem.NetworkExtraction.GuideSignNetwork
{
    class Arc1LayerFactory: LinkMasterLayerFactory
    {
         /// <summary>
        /// 要素类
        /// </summary>
        private IFeatureClass _pFeaClsRoad;
        private IFeatureClass _pFeaClsArc1;
        private IFeatureClass _pFeaClsNode1;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="pFeaClsRoad"></param>
        public Arc1LayerFactory(Form1 frm1)
            : base(frm1)
        {
            _pFeaClsRoad = frm1.FeaClsRoad;
            IFeatureDataset pFeaDs = _pFeaClsRoad.FeatureDataset;
            _pFeaClsArc1 = DatabaseDesigner.CreateArc1Class(pFeaDs);
            _pFeaClsNode1 = DatabaseDesigner.CreateNode1Class(pFeaDs);
            frm1.axMapControl1.Map.ClearLayers();

            IWorkspace WSP = ((IDataset)_pFeaClsRoad).Workspace;
            //添加新建的图层
            IEnumDataset enumDs;
            enumDs = WSP.get_Datasets(esriDatasetType.esriDTFeatureDataset);        //从工作空间获取数据集 }
            IFeatureDataset feaDs = enumDs.Next() as IFeatureDataset;
            IFeatureClassContainer feaClsCtn = feaDs as IFeatureClassContainer;
            IEnumFeatureClass enumFeaCls = feaClsCtn.Classes;
            IFeatureClass featureClass = enumFeaCls.Next();

            while (featureClass != null)
            {
                IFeatureLayer feaLayer = new FeatureLayerClass();
                feaLayer.FeatureClass = featureClass;
                feaLayer.Name = featureClass.AliasName;

                frm1.axMapControl1.Map.AddLayer(feaLayer as ILayer);
                featureClass = enumFeaCls.Next();
            }

        }

        public override void AssembleSegmentLayer(List<NodeInfor> updateNodeInforList,List<string> forbiddenRuls)
        {
            try
            {

                //------------------Step1: 创建拓扑---------------------------
                base.GenerateTopology();

                //------------------Step2: 从Road上，筛选出用于生成交叉口结点的控制点---------------------------
                Hashtable extractionNodeHst = base.ExtractJunctionNode(forbiddenRuls);

                //------------------Step3: 合并从界面端传来需要生成交叉口的控制点与筛选出来的---------------------------
                base.UpdateJunctionNode(extractionNodeHst, updateNodeInforList);

                //------------------Step4: 创建RoadSegment图层---------------------------

                CreateSegment(extractionNodeHst);

                //------------------Step5: 创建RoadSegmentNode图层---------------------------

                CreateNode(extractionNodeHst);

                MessageBox.Show("成果生成指路标志路网");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        public override void CreateNode(Hashtable hstNode)
        {
            foreach (DictionaryEntry nodeDic in hstNode)
            {
                //那么就创建一个吧
                Node1Entity node1Ety = new Node1Entity();
                NodeInfor nodeInfor=nodeDic.Value as NodeInfor;
                node1Ety.ID = nodeInfor.NodeID;

                #region <<<<<<<<<<判断是否是平面交叉口>>>>>>>>>>>>>>>>>>>
                string[] roadTypeArr = nodeInfor.RoadTypeSet.Split('\\');
                if (roadTypeArr.Length <= 1)
                {
                    node1Ety.NodeType = 1;
                }
                else
                {
                    //6 表示为匝道，表示为非平面交叉口(前提是：已经确认该结点是交叉口结点)
                    if (nodeInfor.RoadTypeSet.Contains("6") == true)
                    {
                        node1Ety.NodeType = 2;
                    }
                }
                #endregion <<<<<<<<<<判断是否是平面交叉口>>>>>>>>>>>>>>>>>>>

                node1Ety.CompositeType = 1;
                Node1 rdSegNode = new Node1(_pFeaClsNode1, node1Ety.ID, nodeInfor.Point);
                rdSegNode.CreateNode(node1Ety);
                Arc1 rs = new Arc1(_pFeaClsArc1, 0);
                rdSegNode.CreateAdjData(rs);
                
            }
           
        }

        public override void CreateSegment(Hashtable hstNode)
        {
            //Hashtable filterNodeHst = hstNode.Clone() as Hashtable;

            IFeatureCursor pRoadCursor;

            IQueryFilter pFilter = new QueryFilterClass();
            pFilter.WhereClause = "";
            pRoadCursor = _pFeaClsRoad.Search(pFilter, false);

            IFeature pFeature = pRoadCursor.NextFeature();

            int roadSegID = 1;
            int roadSegNodeID = 1;

            int fNodeID = 0;
            int tnodeID = 0;



            //遍历所有的Road要素
            while (pFeature != null)
            {
                //获取road实体
                RoadEntity rdEty = new RoadEntity();
                Road road = new Road(_pFeaClsRoad, 0);
                rdEty = road.GetEntity(pFeature);
                //Road的几何
                IPolyline roadLine = pFeature.Shape as IPolyline;
                IPointCollection col=new Polyline();
                col = roadLine as IPointCollection;
                IPointCollection segCol = new Polyline();
                IPoint pnt34 = col.Point[0];
                segCol.AddPoint(col.Point[0]);

                #region 更新Road的第一点的ID；
                string key1 = String.Format("{0}_{1}", col.Point[0].X, col.Point[0].Y);
                
                try
                {
                    NodeInfor nodeInfor1 = hstNode[key1] as NodeInfor;
                    //如果没有创建过Node，nodeInfor1.NodeID == 0
                    //创建Road的第一个Node
                    if (nodeInfor1.NodeID == 0)
                    {
                        nodeInfor1.NodeID = roadSegNodeID;
                        hstNode[key1] = nodeInfor1;
                        fNodeID = roadSegNodeID;
                        roadSegNodeID = roadSegNodeID + 1;
                    }
                    else
                    {
                        fNodeID = nodeInfor1.NodeID;
                    }
                }
                catch 
                {
                    MessageBox.Show("Road的ObjectID是："+pFeature.OID.ToString());
                }
                


                #endregion

                //从编号为1的点开始，加入collection,当出现交叉口结点的时候，用new新建segCol
                for (int i = 1; i < col.PointCount; i++)
                {
                    IPoint pnt=col.Point[i];
                    segCol.AddPoint(pnt);
                    string key = String.Format("{0}_{1}", pnt.X, pnt.Y);
                    NodeInfor temNodeInfor = hstNode[key] as NodeInfor;



                    //#region----------------为交叉口结点--------------
                    //1. 合成新的RoadSegment
                    if (temNodeInfor != null)
                    {
                        //如果还没有生成过交叉口，
                        if (temNodeInfor.NodeID == 0)
                        {
                            temNodeInfor.NodeID = roadSegNodeID;
                            hstNode[key] = temNodeInfor;
                            tnodeID = roadSegNodeID;

                            roadSegNodeID = roadSegNodeID + 1;                       
                        }
                        else
                        {
                            tnodeID = temNodeInfor.NodeID;
                        }
                        //如果已经生成交叉口，获取其ID，

                        #region----------------创建新的Segment--------------
                            IPolyline newSegLine = segCol as IPolyline;

                            //新建实体，并应用与要素生成
                            Arc1Entity arc1Ety = new Arc1Entity();
                            arc1Ety.RelID = rdEty.RoadID;
                            arc1Ety.Other = rdEty.RoadID;
                            arc1Ety.RoadType = rdEty.RoadType;
                            arc1Ety.RoadName = rdEty.RoadName;
                            arc1Ety.ID = roadSegID;
                            arc1Ety.RoadLevel = rdEty.RoadType;
                            arc1Ety.FlowDir = 2;
                            arc1Ety.FNodeID = fNodeID;
                            arc1Ety.TNodeID = tnodeID;

                            //创建新的Roadsegment
                            Arc1 arc1 = new Arc1(_pFeaClsArc1, roadSegID);
                            arc1.Create(arc1Ety, newSegLine);
                        #endregion----------------创建新的Segment---------------


                            fNodeID = tnodeID;

                        //roadSegmentID自加，保证唯一性
                        roadSegID = roadSegID + 1;

                        //重新新建segCol，用于生成下一段RoadSegment
                        segCol = new Polyline();
                        segCol.AddPoint(pnt);
                    }

                }
                pFeature = pRoadCursor.NextFeature();
                System.GC.Collect();      //强制对所有代进行垃圾回收
                System.GC.WaitForPendingFinalizers();  //挂起当前线程，直到处理终结器队列的线程清空该队列为止
            }

        }
    }
}
