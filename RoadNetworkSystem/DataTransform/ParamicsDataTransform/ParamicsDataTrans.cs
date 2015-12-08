using System;
using System.Collections.Generic;
using System.Data;
//using System.Linq;
using System.Windows.Forms;
using System.Data.OleDb;
using System.IO;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using RoadNetworkSystem.ADO.Access;

namespace RoadNetworkSystem.ParamicsDataTransform
{
    class ParamicsDataTrans
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
        

        class Node
        {
            public int NodeID;
            public double NodeX;
            public double NodeY;
            public double NodeZ;
            public string NodeType;
        }

        class Link
        {
            public int LinkID;
            public int ArcID;
            public int FNode;
            public int TNode;
            public int LaneNum;

        }

        class AdjLink
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

        private static List<Node> NodeSet = new List<Node>();
        private static List<Link> LinkSet = new List<Link>();
        private static List<AdjLink> AdjLinkSet = new List<AdjLink>();
        private static List<Junction> JunctionSet = new List<Junction>();
        private static List<NextLane> NextLaneSet = new List<NextLane>();
        private static List<LaneNumTrans> LaneTransSet = new List<LaneNumTrans>();

        public static Boolean CreateParamicsdata(string apppath, string databasePath)
        {
            //本函数根据基础路网数据生成Paramics仿真路网文件

            //初始化工作
            IWorkspaceFactory pWSF = new AccessWorkspaceFactoryClass();          //定义IWorkspaceFactory型变量pWSF并初始化
            IWorkspace ws = pWSF.OpenFromFile(databasePath, 0);                      //定义IWorkspace型变量pWS并初始化，从pWSF打开文件赋给pWS

            
            Conn = new OleDbConnection();
            AccessHelper.OpenConnection(databasePath);


            //1.获取当前坐标系，转换为Paramics坐标系
            //NewpFeatClsArc = Setcoorsystem(pFeatClsArc,pFLayer);

            //2.路网数据转换
            DataTrans();

            //3.路网文件输出
            DataOutput(apppath);

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

            string str_sql = "select * from Node ";
            OleDbDataAdapter myDat = new OleDbDataAdapter(str_sql, Conn);
            DataSet ds = new DataSet();
            myDat.Fill(ds, "Node");

            str_sql = "select * from Arc ";
            myDat = new OleDbDataAdapter(str_sql, Conn);
            myDat.Fill(ds, "Arc");

            NodeSet.Clear();
            LinkSet.Clear();
            JunctionSet.Clear();
            AdjLinkSet.Clear();
            NextLaneSet.Clear();
            LaneTransSet.Clear();

            //通过Arc建立Link表,取Arc，Link，起点和终点
            foreach (DataRow arcrow in ds.Tables["Arc"].Rows)
            {
                Link L_link = new Link();
                L_link.LaneNum = Convert.ToInt16(arcrow["LaneNum"]);
                L_link.LinkID = (int)arcrow["LinkID"];
                L_link.ArcID = (int)arcrow["ArcID"];

                string s_Str = "select * from Link where LinkID=" + arcrow["LinkID"].ToString();
                OleDbCommand s_String = new OleDbCommand(s_Str, Conn);
                OleDbDataReader QuesReader = s_String.ExecuteReader();  //新建一个OleDbDataReader
                QuesReader.Read();
                if (Convert.ToInt16(arcrow["Dir"]) == 1)
                {
                    L_link.FNode = (int)QuesReader["FNodeID"];
                    L_link.TNode = (int)QuesReader["TNodeID"];
                }
                else
                {
                    L_link.FNode = (int)QuesReader["TNodeID"];
                    L_link.TNode = (int)QuesReader["FNodeID"];
                }

                LinkSet.Add(L_link);
            }


            //遍历Node表，存储nodeid，邻接link，node类型
            foreach (DataRow noderow in ds.Tables["Node"].Rows)
            {
                Node N_node = new Node();
                N_node.NodeID = (int)noderow["NodeID"];

                //取到每个邻接的link
                adjlink = (string)noderow["AdjLink"];
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
                    AdjLink links = new AdjLink();
                    links.NodeID = (int)noderow["NodeID"];
                    links.Counter = count;
                    links.adj = Adj_Links;
                    AdjLinkSet.Add(links);
                }
                else if (count == 2)
                {
                    LaneNumTrans trans = new LaneNumTrans();
                    trans.NodeID = (int)noderow["NodeID"];
                    trans.LaneTrans = Adj_Links;
                    LaneTransSet.Add(trans);
                }

                //存node的类型
                nodetype = Convert.ToInt16(noderow["NodeType"]);
                if (nodetype == 1)
                    N_node.NodeType = "junction";
                else N_node.NodeType = "roundabout junction";

                //取Node的点坐标
                string s_Str = "select * from Link where FNodeID=" + noderow["NodeID"].ToString();
                OleDbCommand s_String = new OleDbCommand(s_Str, Conn);
                OleDbDataReader QuesReader = s_String.ExecuteReader();
                QuesReader.Read();

                if (!QuesReader.HasRows)
                {
                    s_Str = "select * from Link where TNodeID=" + noderow["NodeID"].ToString();
                    s_String = new OleDbCommand(s_Str, Conn);

                    QuesReader = s_String.ExecuteReader();
                    QuesReader.Read();
                    N_node.NodeX = (double)QuesReader["TNodeX"];
                    N_node.NodeY = (double)QuesReader["TNodeY"];
                    N_node.NodeZ = 0;
                }
                else
                {
                    N_node.NodeX = (double)QuesReader["FNodeX"];
                    N_node.NodeY = (double)QuesReader["FNodeY"];
                    N_node.NodeZ = 0;
                }

                NodeSet.Add(N_node);

            }

            //------------------建立Juntion表

            int[,] ftnode = new int[5, 2];

            int Lanenumber1, Lanenumber2, LanePosition1, LanePosition2;
            int link1, link2, fnode, tnode;
            int outlist;

            foreach (AdjLink M in AdjLinkSet)
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
                    if ((int)QuesReader["TNodeID"] == nodeid)
                    {
                        fnode = (int)QuesReader["FNodeID"];
                        tnode = (int)QuesReader["TNodeID"];
                        link1 = 1;
                    }
                    else if (Convert.ToInt16(QuesReader["Dir"]) == 2)  //非正向但是双向可通时换一下起终点
                    {
                        tnode = (int)QuesReader["FNodeID"];
                        fnode = (int)QuesReader["TNodeID"];
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

                            s_Str = "select * from Link where LinkID=" + b[t].ToString();
                            s_String = new OleDbCommand(s_Str, Conn);
                            QuesReader = s_String.ExecuteReader();
                            QuesReader.Read();

                            if ((int)QuesReader["FNodeID"] == nodeid)        //是否正向
                                link2 = 1;
                            else if (Convert.ToInt16(QuesReader["Dir"]) == 2) //非正向但是双向可通时换一下起终点
                                link2 = 1;
                            else link2 = 0;

                            //出口可通
                            if (link2 == 1)
                            {

                                //查找fromlink和tolink为进出口的两条link
                                str_sql = "select * from LaneConnectors where fromLinkID=" + M.adj[a].ToString() + " and toLinkID=" + b[t].ToString();
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
                                        s_Str = "select LaneNum from Arc where ArcID=" + dat.Rows[i]["fromArcID"].ToString();
                                        s_String = new OleDbCommand(s_Str, Conn);
                                        Lanenumber1 = Convert.ToInt16(s_String.ExecuteScalar());

                                        lanenext.LaneNum = Lanenumber1;

                                        //查找toarc的车道数
                                        s_Str = "select LaneNum from Arc where ArcID=" + dat.Rows[i]["toArcID"].ToString();
                                        s_String = new OleDbCommand(s_Str, Conn);
                                        Lanenumber2 = Convert.ToInt16(s_String.ExecuteScalar());


                                        //查找当前fromlane的车道位置
                                        s_Str = "select [Position] from Lane where LaneID=" + dat.Rows[i]["fromLaneID"].ToString();
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
                                        s_Str = "select [Position] from Lane where LaneID=" + dat.Rows[i]["toLaneID"].ToString();
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
                        NextLaneSet.Add(lanenext);
                        JunctionSet.Add(junc);
                    }
                }
            }
            //}
            //--------建立Lane表

            foreach (LaneNumTrans M in LaneTransSet)
            {
                foreach (Link N in LinkSet)
                {
                    if (N.TNode == M.NodeID)
                    {
                        if (N.LinkID == M.LaneTrans[0])
                        {
                            str_sql = "select * from LaneConnectors where fromLinkID=" + M.LaneTrans[0].ToString() + " and toLinkID=" + M.LaneTrans[1].ToString();
                        }
                        else
                        {
                            str_sql = "select * from LaneConnectors where fromLinkID=" + M.LaneTrans[1].ToString() + " and toLinkID=" + M.LaneTrans[0].ToString();
                        }

                        DataTable dat = new DataTable();
                        myDat = new OleDbDataAdapter(str_sql, Conn);
                        myDat.Fill(dat);

                        //查找fromarc的车道数
                        string s_Str = "select LaneNum from Arc where ArcID=" + dat.Rows[0]["fromArcID"].ToString();
                        OleDbCommand s_String = new OleDbCommand(s_Str, Conn);
                        Lanenumber1 = Convert.ToInt16(s_String.ExecuteScalar());


                        //查找toarc的车道数
                        s_Str = "select LaneNum from Arc where ArcID=" + dat.Rows[0]["toArcID"].ToString();
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
                            JunctionSet.Add(junc);

                            NextLane lanenext = new NextLane();

                            outlist = 1;
                            for (i = 0; i < dat.Rows.Count; i++)//如果有记录，记录相关车道
                            {
                                //查找fromarc的车道数
                                s_Str = "select LaneNum from Arc where ArcID=" + dat.Rows[i]["fromArcID"].ToString();
                                s_String = new OleDbCommand(s_Str, Conn);
                                Lanenumber1 = Convert.ToInt16(s_String.ExecuteScalar());

                                lanenext.LaneNum = Lanenumber1;

                                //查找toarc的车道数
                                s_Str = "select LaneNum from Arc where ArcID=" + dat.Rows[i]["toArcID"].ToString();
                                s_String = new OleDbCommand(s_Str, Conn);
                                Lanenumber2 = Convert.ToInt16(s_String.ExecuteScalar());

                                lanenext.FNode = N.FNode;
                                lanenext.TNode = N.TNode;
                                lanenext.OutNum = 1;

                                //查找当前fromlane的车道位置
                                s_Str = "select [Position] from Lane where LaneID=" + dat.Rows[i]["fromLaneID"].ToString();
                                s_String = new OleDbCommand(s_Str, Conn);
                                LanePosition1 = Convert.ToInt16(s_String.ExecuteScalar());

                                //求该位置在paramics的编号                           
                                LanePosition1 = Math.Abs(LanePosition1 - Lanenumber1) + 1;

                                //--------------------------------------
                                //查找当前tolane的车道位置
                                s_Str = "select [Position] from Lane where LaneID=" + dat.Rows[i]["toLaneID"].ToString();
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

                            NextLaneSet.Add(lanenext);
                        }
                    }
                }
            }
            //-——————————遍历nextlane，车道的下游车道一定按正序写，不能反过来，只能写范围，如1，3，不能是1,2,3，paramics只能连续设置，再设置下游的各车道的进入量split（流量还没设）
            int max = 0, min = 0;
            foreach (NextLane M in NextLaneSet)
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

            //输出文本Node-----------------------------------------
            string path = app + "SimRoadFile";
            string path1 = app + "SimRoadFile\\Paramics";
            if (Directory.Exists(path))//判断是否存在
            {
                if (!Directory.Exists(path1))
                    Directory.CreateDirectory(path1);
            }
            else
            {
                Directory.CreateDirectory(path);//创建新路径
                Directory.CreateDirectory(path1);
            }

            FileStream fs = new FileStream(path1 + "\\nodes.txt", FileMode.Create, FileAccess.Write);
            //FileStream fs = new FileStream("E: \\nodes.txt", FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.Flush();
            sw.BaseStream.Seek(0, SeekOrigin.Begin);

            foreach (Node M in NodeSet)
            {
                sw.WriteLine("{0,4} {1,8} {2,2} {3,14}{4,2},{5,14}{6,2},{7,8}{8,2}{9,10}", "node", M.NodeID, "at", M.NodeX, "m", M.NodeY, "m", M.NodeZ, "m", M.NodeType);

            }
            sw.Flush();
            sw.Close();


            //输出文本link--------------------------------------------
            fs = new FileStream(path1 + "\\links.txt", FileMode.Create, FileAccess.Write);
            sw = new StreamWriter(fs);

            sw.Flush();
            sw.BaseStream.Seek(0, SeekOrigin.Begin);

            foreach (Link M in LinkSet)
            {
                sw.WriteLine("{0,4}{1,4}{2,4} {3,10}{4,2}", "link", M.FNode, M.TNode, "Category", M.LaneNum);
            }
            sw.Flush();
            sw.Close();

            //输出junction------------------------------------------------------
            fs = new FileStream(path1 + "\\junction.txt", FileMode.Create, FileAccess.Write);
            sw = new StreamWriter(fs);

            sw.Flush();
            sw.BaseStream.Seek(0, SeekOrigin.Begin);
            int i = 0;
            foreach (Junction M in JunctionSet)
            {
                sw.WriteLine("{0,8} {1,2}:{2,2}", "junction", M.FNode, M.TNode);
                i = 1;
                while (M.Out[i] != null)
                {
                    sw.WriteLine("{0,3}{1,2}{2,6} {3,1} ", "out", i, "lanes", M.Out[i]);
                    i++;
                }

            }
            sw.Flush();
            sw.Close();

            //--------------------------------------------
            //输出nextlane------------------------------------------------------
            fs = new FileStream(path1 + "\\nextlane.txt", FileMode.Create, FileAccess.Write);
            sw = new StreamWriter(fs);

            sw.Flush();
            sw.BaseStream.Seek(0, SeekOrigin.Begin);
            int j;
            string sstr;
            foreach (NextLane M in NextLaneSet)
            {
                sw.WriteLine("{0,4} {1,2}:{2,2}", "link", M.FNode, M.TNode);

                for (i = 1; i <= M.LaneNum; i++)
                {
                    sstr = "";
                    for (j = 1; j <= M.OutNum; j++)
                    {
                        if (M.nextlane[i, j] == null)
                            sstr += "0";
                        else
                            sstr += M.nextlane[i, j];

                        sstr += " ";
                    }
                    sw.WriteLine("{0,4}{1,2}{2,5} {3,1} ", "lane", i, "next", sstr);
                }

            }
            sw.Flush();
            sw.Close();
            //--------------------------------------------
            MessageBox.Show("已创建路网文件！" + path1);
        }
    }
}
