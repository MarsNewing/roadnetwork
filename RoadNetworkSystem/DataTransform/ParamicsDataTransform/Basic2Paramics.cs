using System;
using System.Collections.Generic;
using System.Data;
//using System.Linq;
using System.Windows.Forms;
using System.Data.OleDb;
using System.IO;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LinkLayer;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LaneLayer;
using RoadNetworkSystem.ADO.Access;
namespace RoadNetworkSystem.ParamicsDataTransform
{
    class Basic2Paramics
    {
        /// <summary>
        /// 此类用于生成Paramics路网数据并按指定格式输出
        /// </summary>
        /// 创建时间   ：2012-6-7
        /// 创建人     ：饶明雷
        /// 

        //private const string VissimCoorsystem = "Xian_1980_3_Degree_GK_CM_114E"; //Vissim路网坐标系
        //private const string TMCoorsystem = "GCS_WGS_1984"; //TM路网坐标系
        //private const string ParamicsCoorsystem = "Xian_1980_3_Degree_GK_CM_114E"; //Paramics路网坐标系
        private const double LANEWIDTH = 3.5; //每车道宽度，用于确定Arc的偏移距离，可根据需要调整
        private static OleDbConnection Conn;
        private static double BoundingMax_X = 0, BoundingMax_Y = 0, BoundingMin_X = 0, BoundingMin_Y = 0; //路网区域边界四个角点坐标

        class Node_Para
        {
            public int NodeID;
            public double NodeX;
            public double NodeY;
            public double NodeZ;
            public string NodeType;
        }

        class Link_Para
        {
            public int LinkID;
            public int ArcID;
            public int FNode;
            public int TNode;
            public int LaneNum;

        }

        class AdjLinkIDs
        {
            public int NodeID;
            public int Counter;
            public int[] adj = new int[5];
        }

        class Junction
        {
            public int LinkID;
            public int FNode;
            public int TNode;
            public string[] Out = new string[5];

        }

        class NextLane
        {
            public int FNode;
            public int TNode;
            public int LaneNum;
            public int OutNum;
            public string[,] nextlane = new string[8, 5];
        }

        class LaneNumTrans
        {
            public int NodeID;
            public int[] LaneTrans = new int[2];
        }




        private static List<Node_Para> _nodeSet = new List<Node_Para>();
        private static List<Link_Para> _linkSet = new List<Link_Para>();
        private static List<AdjLinkIDs> _adjLinkSet = new List<AdjLinkIDs>();
        private static List<Junction> _junctionSet = new List<Junction>();
        private static List<NextLane> _nextLaneSet = new List<NextLane>();
        private static List<LaneNumTrans> _laneTransSet = new List<LaneNumTrans>();

        private static IFeatureClass _pFeaClsNode;
        private static IFeatureClass _pFeaClsLink;


        public static Boolean CreateParamicsdata(string apppath, string databasePath)
        {
            //本函数根据基础路网数据生成Paramics仿真路网文件

            //初始化工作
            IWorkspaceFactory pWSF = new AccessWorkspaceFactoryClass();          //定义IWorkspaceFactory型变量pWSF并初始化
            IWorkspace ws = pWSF.OpenFromFile(databasePath, 0);                      //定义IWorkspace型变量pWS并初始化，从pWSF打开文件赋给pWS

            //初始化需要用到的要素类
            _pFeaClsNode = (ws as IFeatureWorkspace).OpenFeatureClass(NodeEntity.NodeName);
            _pFeaClsLink = (ws as IFeatureWorkspace).OpenFeatureClass(LinkEntity.LinkName);
            
            Conn =AccessHelper.OpenConnection(databasePath);


            //1.获取当前坐标系，转换为Paramics坐标系
            //NewpFeatClsArc = Setcoorsystem(pFeatClsArc,pFLayer);

            try
            {

                //2.路网数据转换
                DataTrans();

                //3.路网文件输出
                DataOutput(apppath);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return true;
        }

        private static void DataTrans()
        {
            int i;
            int count;
            int nodetype;
            int nodeid;
            string adjlink;
            int[] b = new int[5];

            string str_sql = "select * from " + NodeEntity.NodeName;
            OleDbDataAdapter myDat = new OleDbDataAdapter(str_sql, Conn);
            DataSet ds = new DataSet();
            myDat.Fill(ds, NodeEntity.NodeName);

            str_sql = "select * from "+ArcEntity.ArcFeatureName;
            myDat = new OleDbDataAdapter(str_sql, Conn);
            myDat.Fill(ds, ArcEntity.ArcFeatureName);

            _nodeSet.Clear();
            _linkSet.Clear();
            _junctionSet.Clear();
            _adjLinkSet.Clear();
            _nextLaneSet.Clear();
            _laneTransSet.Clear();

            Link link = new Link(_pFeaClsLink, 0);

            //通过Arc建立Link表,取Arc，Link，起点和终点
            foreach (DataRow arcrow in ds.Tables[ArcEntity.ArcFeatureName].Rows)
            {
                Link_Para L_link = new Link_Para();
                
                L_link.LaneNum = Convert.ToInt16(arcrow[Arc.LaneNumNm]);
                L_link.LinkID = (int)arcrow[Arc.LinkIDNm];
                L_link.ArcID = (int)arcrow[Arc.ArcIDNm];

                string s_Str = "select * from Link where LinkID=" + arcrow[Arc.LinkIDNm].ToString();
                OleDbCommand s_String = new OleDbCommand(s_Str, Conn);
                OleDbDataReader QuesReader = s_String.ExecuteReader();  //新建一个OleDbDataReader
                QuesReader.Read();
                if (Convert.ToInt16(arcrow[Arc.FlowDirNm]) == 1)
                {
                    L_link.FNode = (int)QuesReader[link.FNodeIDNm];
                    L_link.TNode = (int)QuesReader[link.TNodeIDNm];
                }
                else
                {
                    L_link.FNode = (int)QuesReader[link.TNodeIDNm];
                    L_link.TNode = (int)QuesReader[link.FNodeIDNm];
                }

                _linkSet.Add(L_link);
            }

            Node node = new Node(_pFeaClsNode, 0, null);
            //遍历Node表，存储nodeid，邻接link，node类型
            foreach (DataRow noderow in ds.Tables[NodeEntity.NodeName].Rows)
            {
                Node_Para N_node = new Node_Para();
                N_node.NodeID = (int)noderow[node.NodeIDNm];

                //取到每个邻接的link
                adjlink = (string)noderow[node.AdjIDsNm];
                string[] sArray = adjlink.Split('\\');
                int[] Adj_Links = new int[5];
                count = 0;
                foreach (string mystr in sArray)
                {
                    Adj_Links[count] = Convert.ToInt32(mystr);
                    count++;
                }

                if (count > 2)     //若count数目大于2，说明是交叉口点，是1为边界点，2为直线link的点,存入AdjLinkSet
                {
                    AdjLinkIDs links = new AdjLinkIDs();
                    links.NodeID = (int)noderow["NodeID"];
                    links.Counter = count;
                    links.adj = Adj_Links;
                    _adjLinkSet.Add(links);
                }
                else if (count == 2)
                {
                    LaneNumTrans trans = new LaneNumTrans();
                    trans.NodeID = (int)noderow[node.NodeIDNm];
                    trans.LaneTrans = Adj_Links;
                    _laneTransSet.Add(trans);
                }

                //存node的类型
                nodetype = Convert.ToInt16(noderow[node.NodeTypeNm]);
                if (nodetype == 1)
                    N_node.NodeType = "junction";
                else N_node.NodeType = "roundabout junction";

                //取Node的点坐标
                string s_Str = "select * from Link where "+link.FNodeIDNm+ " =" + noderow[node.NodeIDNm].ToString();
                OleDbCommand s_String = new OleDbCommand(s_Str, Conn);
                OleDbDataReader QuesReader = s_String.ExecuteReader();
                QuesReader.Read();

                if (!QuesReader.HasRows)
                {
                    s_Str = "select * from Link where " + link.TNodeIDNm + " =" + noderow[node.NodeIDNm].ToString();
                    s_String = new OleDbCommand(s_Str, Conn);

                    
                    QuesReader = s_String.ExecuteReader();
                    QuesReader.Read();

                    //获取要素
                    link = new Link(_pFeaClsLink, Convert.ToInt32(QuesReader[link.IDNm]));
                    IFeature linkFea = link.GetFeature();
                    IPolyline linkLine = linkFea.Shape as IPolyline;

                    N_node.NodeX = linkLine.ToPoint.X;
                    N_node.NodeY = linkLine.ToPoint.Y;
                    N_node.NodeZ = 0;
                }
                else
                {
                    //获取要素
                    link = new Link(_pFeaClsLink, Convert.ToInt32(QuesReader[link.IDNm]));
                    IFeature linkFea = link.GetFeature();
                    IPolyline linkLine = linkFea.Shape as IPolyline;

                    N_node.NodeX = linkLine.FromPoint.X;
                    N_node.NodeY = linkLine.FromPoint.Y;
                    N_node.NodeZ = 0;
                }

                _nodeSet.Add(N_node);

            }

            //------------------建立Juntion表

            int[,] ftnode = new int[5, 2];

            int Lanenumber1, Lanenumber2, LanePosition1, LanePosition2;
            int link1, link2, fnode, tnode;
            int outlist;

            foreach (AdjLinkIDs M in _adjLinkSet)
            {
                nodeid = M.NodeID;
                int k = 0;
                count = 0;

                //取一个link后，按从该link的右到左的顺序把其他的link存到b中
                for (int a = 0; a < M.Counter; a++)
                {
                    int y = a;
                    for (i = 0; i < 5; i++) b[i] = 0;
                    k = 0;
                    while (y - 1 >= 0)
                    {
                        b[k++] = M.adj[y - 1]; y--;
                    }
                    y = M.Counter;
                    while (y - 1 > a)
                    {
                        b[k++] = M.adj[y - 1]; y--;
                    }


                    //取各条link为进口道，分别生成junction


                    string s_Str = "select * from Link where LinkID=" + M.adj[a].ToString();
                    OleDbCommand s_String = new OleDbCommand(s_Str, Conn);
                    OleDbDataReader QuesReader = s_String.ExecuteReader();  //新建一个OleDbDataReader
                    QuesReader.Read();


                    fnode = 0;
                    tnode = 0;

                    //进口道的link是否是正向或双向可达，可达才继续生成进口道junction
                    if ((int)QuesReader[link.TNodeIDNm] == nodeid)
                    {
                        fnode = (int)QuesReader[link.FNodeIDNm];
                        tnode = (int)QuesReader[link.TNodeIDNm];
                        link1 = 1;
                    }
                    else if (Convert.ToInt16(QuesReader[link.FlowDirNm]) == 2)  //非正向但是双向可通时换一下起终点
                    {
                        tnode = (int)QuesReader[link.FNodeIDNm];
                        fnode = (int)QuesReader[link.TNodeIDNm];
                        link1 = 1;
                    }
                    else link1 = 0;

                    //进口道可达，寻找出口link，出口正向可通，继续生成junction，否则不产生junction
                    if (link1 != 0)
                    {
                        Junction junc = new Junction();
                        NextLane lanenext = new NextLane();
                        int t;
                        outlist = 1;
                        for (t = 0; t < k; t++)
                        {

                            s_Str = "select * from Link where " + link.IDNm + "=" + b[t].ToString();
                            s_String = new OleDbCommand(s_Str, Conn);
                            QuesReader = s_String.ExecuteReader();
                            QuesReader.Read();

                            if ((int)QuesReader[link.FNodeIDNm] == nodeid)        //是否正向
                                link2 = 1;
                            else if (Convert.ToInt16(QuesReader[link.FlowDirNm]) == 2) //非正向但是双向可通时换一下起终点
                                link2 = 1;
                            else link2 = 0;

                            //出口可通
                            if (link2 == 1)
                            {

                                //查找fromlink和tolink为进出口的两条link
                                str_sql = "select * from " + LaneConnectorEntity.ConnectorName + " where " + LaneConnectorFeature.fromLinkIDNm + "=" + M.adj[a].ToString() + " and " 
                                    + LaneConnectorFeature.toLinkIDNm + "=" + b[t].ToString();
                                DataTable dat = new DataTable();
                                myDat = new OleDbDataAdapter(str_sql, Conn);
                                myDat.Fill(dat);

                                junc.FNode = fnode;
                                junc.TNode = tnode;
                                junc.LinkID = M.adj[a];
                                lanenext.FNode = fnode;
                                lanenext.TNode = tnode;

                                if (dat.Rows.Count != 0)
                                {
                                    //遍历符合条件的记录

                                    for (i = 0; i < dat.Rows.Count; i++)//如果有记录，记录相关车道
                                    {

                                        //查找fromarc的车道数
                                        s_Str = "select " + Arc.LaneNumNm + " from " + ArcEntity.ArcFeatureName + " where " + Arc.ArcIDNm + "="
                                            + dat.Rows[i][LaneConnectorFeature.fromArcIDNm].ToString();

                                        s_String = new OleDbCommand(s_Str, Conn);
                                        Lanenumber1 = Convert.ToInt16(s_String.ExecuteScalar());

                                        lanenext.LaneNum = Lanenumber1;

                                        //查找toarc的车道数
                                        s_Str = "select " + Arc.LaneNumNm + "  from " + ArcEntity.ArcFeatureName + " where " + Arc.ArcIDNm + "=" 
                                            + dat.Rows[i][LaneConnectorFeature.toArcIDNm].ToString();
                                        s_String = new OleDbCommand(s_Str, Conn);
                                        Lanenumber2 = Convert.ToInt16(s_String.ExecuteScalar());


                                        //查找当前fromlane的车道位置
                                        s_Str = "select [Position] from "+LaneEntity.LaneName+" where "+LaneFeature.LaneIDNm+"=" +
                                            dat.Rows[i][LaneConnectorFeature.fromArcIDNm].ToString();

                                        s_String = new OleDbCommand(s_Str, Conn);
                                        LanePosition1 = Convert.ToInt16(s_String.ExecuteScalar());

                                        //求该位置在paramics的编号                           
                                        LanePosition1 = Math.Abs(LanePosition1 - Lanenumber1) + 1;

                                        //查看字符串中是否已存在该车道，主要用于车道一对多的情况的判别
                                        if (junc.Out[outlist] != null)
                                        {
                                            if (junc.Out[outlist].IndexOf(LanePosition1.ToString()) == -1)
                                                junc.Out[outlist] += LanePosition1.ToString() + " ";
                                        }
                                        else
                                            junc.Out[outlist] += LanePosition1.ToString() + " ";

                                        //--------------------------------------
                                        //查找当前tolane的车道位置
                                        s_Str = "select [Position] from  " + LaneEntity.LaneName + " where " + LaneFeature.LaneIDNm + "="
                                            + dat.Rows[i][LaneConnectorFeature.fromArcIDNm].ToString();
                                        s_String = new OleDbCommand(s_Str, Conn);
                                        LanePosition2 = Convert.ToInt16(s_String.ExecuteScalar());

                                        //求该位置在paramics的编号                           
                                        LanePosition2 = Math.Abs(LanePosition2 - Lanenumber2) + 1;

                                        //存储该lane的nextlane
                                        if (lanenext.nextlane[LanePosition1, outlist] == null)
                                            lanenext.nextlane[LanePosition1, outlist] += LanePosition2.ToString();
                                        else
                                        {
                                            //if (lanenext.nextlane[LanePosition1, outlist].IndexOf(LanePosition2.ToString()) != -1)
                                            {
                                                lanenext.nextlane[LanePosition1, outlist] += ",";
                                                lanenext.nextlane[LanePosition1, outlist] += LanePosition2.ToString();
                                            }
                                        }

                                    }
                                    outlist++;
                                }
                                //如果没有，该出口的lane为空,这里默认都设置为1，通过priority文件将其barred
                                else junc.Out[outlist++] = "1";

                            }
                        }
                        lanenext.OutNum = outlist - 1;
                        _nextLaneSet.Add(lanenext);
                        _junctionSet.Add(junc);
                    }
                }
            }
            //}
            //--------建立Lane表

            foreach (LaneNumTrans M in _laneTransSet)
            {
                foreach (Link_Para N in _linkSet)
                {
                    if (N.TNode == M.NodeID)
                    {
                        if (N.LinkID == M.LaneTrans[0])
                        {
                            str_sql = "select * from " + LaneConnectorEntity.ConnectorName + "  where " + LaneConnectorFeature.fromLinkIDNm+ "=" + M.LaneTrans[0].ToString() +
                                " and " + LaneConnectorFeature.toLinkIDNm + "=" + M.LaneTrans[1].ToString();
                        }
                        else
                        {
                            str_sql = "select * from  " + LaneConnectorEntity.ConnectorName + "  where " + LaneConnectorFeature.fromLinkIDNm + "=" + M.LaneTrans[1].ToString() +
                                " and " + LaneConnectorFeature.toLinkIDNm + "=" + M.LaneTrans[0].ToString();
                        }

                        DataTable dat = new DataTable();
                        myDat = new OleDbDataAdapter(str_sql, Conn);
                        myDat.Fill(dat);

                        //查找fromarc的车道数
                        string s_Str = "select "+Arc.LaneNumNm+" from "+ArcEntity.ArcFeatureName+" where "+Arc.ArcIDNm+"=" + dat.Rows[0][LaneConnectorFeature.fromArcIDNm].ToString();
                        OleDbCommand s_String = new OleDbCommand(s_Str, Conn);
                        Lanenumber1 = Convert.ToInt16(s_String.ExecuteScalar());


                        //查找toarc的车道数
                        s_Str = "select  " + Arc.LaneNumNm + " from " + ArcEntity.ArcFeatureName + "  where  " + Arc.ArcIDNm + "=" + dat.Rows[0][LaneConnectorFeature.toArcIDNm].ToString();
                        s_String = new OleDbCommand(s_Str, Conn);
                        Lanenumber2 = Convert.ToInt16(s_String.ExecuteScalar());

                        if (Lanenumber1 != Lanenumber2)
                        {
                            //车道数发生变化处也需要读取这个junction，特别是入口车道数大于出口车道数时一定要
                            Junction junc = new Junction();
                            junc.FNode = N.FNode;
                            junc.TNode = N.TNode;
                            junc.LinkID = N.LinkID;
                            for (i = 1; i <= Lanenumber1; i++) junc.Out[1] += i.ToString() + " ";
                            _junctionSet.Add(junc);

                            NextLane lanenext = new NextLane();

                            outlist = 1;
                            for (i = 0; i < dat.Rows.Count; i++)//如果有记录，记录相关车道
                            {
                                //查找fromarc的车道数
                                s_Str = "select " + Arc.LaneNumNm + " from " + ArcEntity.ArcFeatureName + "  where " + Arc.ArcIDNm + "=" + dat.Rows[i][LaneConnectorFeature.fromArcIDNm].ToString();
                                s_String = new OleDbCommand(s_Str, Conn);
                                Lanenumber1 = Convert.ToInt16(s_String.ExecuteScalar());

                                lanenext.LaneNum = Lanenumber1;

                                //查找toarc的车道数
                                s_Str = "select " + Arc.LaneNumNm + " from " + ArcEntity.ArcFeatureName + " where " + Arc.ArcIDNm + "=" + dat.Rows[i][LaneConnectorFeature.toArcIDNm].ToString();
                                s_String = new OleDbCommand(s_Str, Conn);
                                Lanenumber2 = Convert.ToInt16(s_String.ExecuteScalar());

                                lanenext.FNode = N.FNode;
                                lanenext.TNode = N.TNode;
                                lanenext.OutNum = 1;

                                //查找当前fromlane的车道位置
                                s_Str = "select [Position] from "+LaneEntity.LaneName+" where "+LaneFeature.LaneIDNm+"=" + 
                                    dat.Rows[i][LaneConnectorFeature.fromLaneIDNm].ToString();
                                s_String = new OleDbCommand(s_Str, Conn);
                                LanePosition1 = Convert.ToInt16(s_String.ExecuteScalar());

                                //求该位置在paramics的编号                           
                                LanePosition1 = Math.Abs(LanePosition1 - Lanenumber1) + 1;

                                //--------------------------------------
                                //查找当前tolane的车道位置
                                s_Str = "select [Position] from " + LaneEntity.LaneName + " where " + LaneFeature.LaneIDNm + "=" + 
                                    dat.Rows[i][LaneConnectorFeature.toLaneIDNm].ToString();
                                s_String = new OleDbCommand(s_Str, Conn);
                                LanePosition2 = Convert.ToInt16(s_String.ExecuteScalar());

                                //求该位置在paramics的编号                           
                                LanePosition2 = Math.Abs(LanePosition2 - Lanenumber2) + 1;

                                //存储该lane的nextlane
                                if (lanenext.nextlane[LanePosition1, outlist] == null)
                                    lanenext.nextlane[LanePosition1, outlist] += LanePosition2.ToString();
                                else
                                {
                                    lanenext.nextlane[LanePosition1, outlist] += ",";
                                    lanenext.nextlane[LanePosition1, outlist] += LanePosition2.ToString();
                                }

                            }

                            _nextLaneSet.Add(lanenext);
                        }
                    }
                }
            }
            //-——————————遍历nextlane，车道的下游车道一定按正序写，不能反过来，只能写范围，如1，3，不能是1,2,3，paramics只能连续设置，再设置下游的各车道的进入量split（流量还没设）
            int max = 0, min = 0;
            foreach (NextLane M in _nextLaneSet)
            {
                for (i = 1; i <= M.LaneNum; i++)
                    for (int j = 1; j <= M.OutNum; j++)
                    {
                        if (M.nextlane[i, j] != null)
                        {
                            count = 0;

                            string[] sArray = M.nextlane[i, j].Split(',');

                            foreach (string mystr in sArray)
                            {
                                if (Convert.ToInt32(mystr) != 0)
                                { max = Convert.ToInt32(mystr); min = Convert.ToInt32(mystr); break; }
                            }

                            foreach (string mystr in sArray)
                            {
                                if (Convert.ToInt32(mystr) != 0)
                                {

                                    if (Convert.ToInt32(mystr) > max) max = Convert.ToInt32(mystr);
                                    if (Convert.ToInt32(mystr) < min) min = Convert.ToInt32(mystr);

                                    count++;
                                }
                            }

                            if (count >= 2)
                            {
                                M.nextlane[i, j] = min.ToString() + "," + max.ToString();
                            }

                        }
                    }
            }
            // //-------------------------------------
            Conn.Close();
        }

        private static void DataOutput(string app)
        {

            string OriginDerectory = app + "\\simulationNetwork" + "\\Paramics" + "\\originFolder";//原始文件夹地址
            string TargetDirectory = app + "\\simulationNetwork" + "\\Paramics" + "\\paramicsNetwork";  //目标文件夹地址

            #region 复制路网文件
            if (Directory.Exists(TargetDirectory))
            {
                Directory.Delete(TargetDirectory, true); //若文件夹存在,不管目录是否为空,删除 
                Directory.CreateDirectory(TargetDirectory); //删除后,重新创建文件夹 
            }

            CopyDirectory(OriginDerectory, TargetDirectory);  //复制文件夹


            //  MessageBox.Show("默认文件夹复制完成！");
            System.DateTime currentTime = new System.DateTime();
            currentTime = System.DateTime.Now;
            string strT = currentTime.ToString() + ":默认文件夹复制完成！";
            Application.DoEvents();

            #endregion

            createNodeTxtFile(TargetDirectory, _nodeSet);
            createJunctionTxtFile(TargetDirectory, _junctionSet);
            creatLinkTxtFile(TargetDirectory, _linkSet);
            createNextLaneTxtFile(TargetDirectory, _nextLaneSet);
            modifyFileType(TargetDirectory);
            //--------------------------------------------
            MessageBox.Show("已创建路网文件！" + TargetDirectory);
        }


        public static void CopyDirectory(string sourceDirName, string destDirName)  //复制文件夹
        {
            if (sourceDirName.Substring(sourceDirName.Length - 1) != "\\")
            {
                sourceDirName = sourceDirName + "\\";
            }
            if (destDirName.Substring(destDirName.Length - 1) != "\\")
            {
                destDirName = destDirName + "\\";
            }
            if (Directory.Exists(sourceDirName))
            {
                if (!Directory.Exists(destDirName))
                {
                    Directory.CreateDirectory(destDirName);
                }
                foreach (string item in Directory.GetFiles(sourceDirName))
                {
                    File.Copy(item, destDirName + System.IO.Path.GetFileName(item), true);
                }
                foreach (string item in Directory.GetDirectories(sourceDirName))
                {
                    CopyDirectory(item, destDirName + item.Substring(item.LastIndexOf("\\") + 1));
                }
            }
        }

        /// <summary>
        /// 输出Node文本文件
        /// </summary>
        /// <param name="TargetDirectory"></param>
        /// <param name="NodeSet"></param>
        private static void createNodeTxtFile(string TargetDirectory, List<Node_Para> NodeSet)
        {
            updateBoundary(NodeSet);

            FileStream fs = new FileStream(TargetDirectory + @"\nodes.txt", FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.Flush();
            sw.BaseStream.Seek(0, SeekOrigin.Begin);

            //先输出边界范围大小,在原来角点坐标的基础上扩大一点+ -100，bounding box 有x下界，y下界，x上界，y上界
            //"{0,12} {1,10}{2,2} {3,10}{4,2} {5,10}{6,2} {7,10}{8,2}"，每个输出对应的长度，如第一个字符长度为12，第二个长度为10。。
            //"Bounding Box", Math.Round(BoundingMin_X-100,1), "m", Math.Round(BoundingMin_Y-100,1), "m", Math.Round(BoundingMax_X+100,1), "m",Math.Round(BoundingMax_Y+100,1),"m" 对应前面的9个占位符的输出
            sw.WriteLine("{0,12} {1,10}{2,2} {3,10}{4,2} {5,10}{6,2} {7,10}{8,2}",
                "Bounding Box", Math.Round(BoundingMin_X - 100, 1), "m", Math.Round(BoundingMin_Y - 100, 1),
                "m", Math.Round(BoundingMax_X + 100, 1), "m", Math.Round(BoundingMax_Y + 100, 1), "m");

            foreach (Node_Para M in NodeSet) //遍历NodeSet，输出点编号，x，y坐标和节点类型
            {
                sw.WriteLine("{0,4}   {1,10} {2,2} {3,10}{4,2},{5,11}{6,2},{7,10}{8,2} {9,11}", "node", M.NodeID,
                    "at", Math.Round(M.NodeX, 2), "m", Math.Round(M.NodeY, 2), "m", "0.00", "m", M.NodeType);

            }
            sw.Flush();
            sw.Close();
        }



        /// <summary>
        /// 输出Link文本文件
        /// </summary>
        /// <param name="TargetDirectory"></param>
        /// <param name="LinkSet"></param>
        private static void creatLinkTxtFile(string TargetDirectory, List<Link_Para> LinkSet)
        {
            FileStream fs = new FileStream(TargetDirectory + @"\links.txt", FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.Flush();
            sw.BaseStream.Seek(0, SeekOrigin.Begin);

            int[,] barredlink = new int[LinkSet.Count, LinkSet.Count]; //单向路段link的起点、终点存储数组
            int dirflag; //路段是都单向标识，为1是存在反向道路，为0是单向道路
            int barredcount = 0; //单向车道数目
            foreach (Link_Para M in LinkSet)
            {
                dirflag = 0;
                //输出路段的起点id，终点id，路段类型（设车道类型编号与车道数一样），路段特殊车道信息（有则输出，没有则为空串）
                //sw.WriteLine("{0,4}{1,3}{2,3} {3,8}{4,2}{5,10}", "link", M.FNode, M.TNode, "category", M.LaneNum, M.LaneType);
                sw.WriteLine("{0,4} {1,3} {2,3} {3,8}{4,2}", "link", M.FNode, M.TNode, "category", M.LaneNum);
                foreach (Link_Para H in LinkSet)
                {
                    if (H.FNode == M.TNode && H.TNode == M.FNode)  //针对某一个路段，判断LinkSet中是否存在反向的路段，是则为双向路段，否则为单向
                    { dirflag = 1; break; }
                }
                if (dirflag == 0) //如果道路是单向的，存储不存在的反向道路的起点和终点
                {
                    barredlink[barredcount, 0] = M.TNode;
                    barredlink[barredcount, 1] = M.FNode;
                    barredcount++;  //纪录单向路段的数目
                }

            }

            for (int i = 0; i < barredcount; i++) //因为单向道路在paramics中仍需要输出，即输出为反向道路被禁止
            {
                //输出反向被禁止的路段，反向路段的起点、终点，"barred"
                sw.WriteLine("{0,4}{1,3}{2,3} {3,6}", "link", barredlink[i, 0], barredlink[i, 1], "barred");
            }
            sw.Flush();
            sw.Close();
        }

        /// <summary>
        /// 生成junctionTxt
        /// </summary>
        /// <param name="TargetDirectory"></param>
        /// <param name="junctionSet"></param>
        private static void createJunctionTxtFile(string TargetDirectory, List<Junction> junctionSet)
        {
            FileStream fs = new FileStream(TargetDirectory + @"\junctions.txt", FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.Flush();
            sw.BaseStream.Seek(0, SeekOrigin.Begin);
            string resultpaixu;
            foreach (Junction M in junctionSet) //遍历JunctionSet写入junctions文件
            {
                //进口道的起点和终点
                sw.WriteLine("{0,8} {1,2}:{2,2}", "junction", M.FNode, M.TNode);

                int i = 1;
                while (M.Out[i] != null)
                {

                    string[] sArray = M.Out[i].Split(' ');
                    resultpaixu = sort(sArray);  //出口字符串先按数字从小到大的顺序排列好,再输出
                    //输出到达每个出口对应的进口道车道编号
                    sw.WriteLine("{0,3}{1,2}{2,6} {3,1} ", "out", i, "lanes", resultpaixu);
                    i++;
                }

            }
            sw.Flush();
            sw.Close();
        }


        private static void createNextLaneTxtFile(string TargetDirectory, List<NextLane> NextLaneSet)
        {
            FileStream fs = new FileStream(TargetDirectory + @"\nextlanes.txt", FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.Flush();
            sw.BaseStream.Seek(0, SeekOrigin.Begin);

            sw.WriteLine("Next Lanes");
            int kk;
            string sstr;
            foreach (NextLane M in NextLaneSet) //遍历NextLaneSet，写入nextlanes文件
            {
                //进口道起点终点
                sw.WriteLine("{0,4} {1,2}:{2,2}", "link", M.FNode, M.TNode);

                for (int i = 1; i <= M.LaneNum; i++) //进口道车道数
                {
                    sstr = "";
                    for (kk = 1; kk <= M.OutNum; kk++) //下游出口数
                    {
                        if (M.nextlane[i, kk] == null)  //下游出口不能到达，可到达的下游车道标记为0
                            sstr += "0";
                        else
                            sstr += M.nextlane[i, kk]; //否则纪录下游可到达的车道编号

                        sstr += " ";  //各出口的可到达车道编号串之间通过“ ”分隔
                    }
                    //进口道每个车道和它可到达下游各出口路段的车道编号
                    sw.WriteLine("{0,4}{1,2}{2,5} {3,1} ", "lane", i, "next", sstr);
                }

            }
            sw.Flush();
            sw.Close();
        }


        private static void modifyFileType(string TargetDirectory)
        {
            string[] changefrom = new string[] { "links.txt", "nodes.txt", "junctions.txt", "nextlanes.txt"};//原始文件名
            string[] changeto = new string[] { "links", "nodes", "junctions", "nextlanes"};//修改后的文件名
            for (int i = 0; i < changefrom.Length; i++)
            {
                if (File.Exists(TargetDirectory + @"\" + changeto[i]) == true)
                {
                    File.Delete(TargetDirectory + @"\" + changeto[i]);
                }
                File.Move(TargetDirectory + @"\" + changefrom[i], TargetDirectory + @"\" + changeto[i]);  //将txt类型换成paramics中的类型
            }
        }

        /// <summary>
        /// 排序函数,将数字字符串按从小到大的顺序排列
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string sort(string[] str)  //排序函数,将数字字符串按从小到大的顺序排列
        {
            int temp;
            string resultstr = "";
            int[] a = new int[10];
            int num = 0;

            foreach (string kk in str)//将字符数组中的字符转成整数,存到整数数组a
            {
                if (kk != "")
                { a[num] = Convert.ToInt16(kk); num++; }

            }
            //排序
            for (int i = 0; i < num; i++)
            {
                for (int j = i; j < num; j++)
                {
                    if (a[i] > a[j])
                    {
                        temp = a[i]; a[i] = a[j]; a[j] = temp;
                    }
                }

            }
            //将字符输出写成字符串
            for (int i = 0; i < num; i++)
                resultstr += a[i].ToString() + " ";

            return resultstr;
        }


        /// <summary>
        /// 更新路网界限
        /// </summary>
        /// <param name="NodeSet"></param>
        private static void updateBoundary(List<Node_Para> NodeSet)
        {
            foreach (Node_Para N_node in NodeSet)
            {
                //对比每个点的XY坐标,取到最大最小值，如果是第一个点，直接存入边界值中
                if (BoundingMax_X == 0 && BoundingMax_Y == 0 && BoundingMin_X == 0 && BoundingMin_Y == 0) //一开始先把第一个点作为最大最小值
                {
                    BoundingMax_X = N_node.NodeX; BoundingMax_Y = N_node.NodeY;
                    BoundingMin_X = N_node.NodeX; BoundingMin_Y = N_node.NodeY;
                }
                else//非第一个点的其他点，对比xy坐标,分别判断并获取X,Y的最大和最小，更新边界值
                {
                    if (N_node.NodeX > BoundingMax_X) BoundingMax_X = N_node.NodeX;
                    if (N_node.NodeX < BoundingMin_X) BoundingMin_X = N_node.NodeX;
                    if (N_node.NodeY > BoundingMax_Y) BoundingMax_Y = N_node.NodeY;
                    if (N_node.NodeY < BoundingMin_Y) BoundingMin_Y = N_node.NodeY;
                }
            }
 
        }



    }
}
