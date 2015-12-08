using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using System.Data.OleDb;
using System.IO;
using RoadNetworkSystem.GIS;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LinkLayer;
using RoadNetworkSystem.GIS.Geometry;
using RoadNetworkSystem.ADO.Access;

namespace RoadNetworkSystem.VissimDataTransform
{
    class VissimDataTrans
    {
        /// <summary>
        /// 此类用于生成Vissim路网数据并按指定格式输出
        /// </summary>
        /// 创建时间   ：2012-5-16
        /// 创建人     ：饶明雷
        /// 

        //private const string VissimCoorsystem = "Xian_1980_3_Degree_GK_CM_114E"; //Vissim路网坐标系
        //private const string TMCoorsystem = "GCS_WGS_1984"; //TM路网坐标系
        //private const string ParamicsCoorsystem = "Xian_1980_3_Degree_GK_CM_114E"; //Paramics路网坐标系
        private const double LANEWIDTH = 3.5; //每车道宽度，用于确定Arc的偏移距离，可根据需要调整
        private static OleDbConnection Conn;
        
        //private IMap m_pMap;

        public static Boolean CreateVissimdata(string apppath, string databasePath, IFeatureLayer pFLayer)
        {
            //本函数根据基础路网数据生成Vissim仿真数据文件

            //初始化工作
            IWorkspaceFactory pWSF = new AccessWorkspaceFactoryClass();          //定义IWorkspaceFactory型变量pWSF并初始化
            IWorkspace ws = pWSF.OpenFromFile(databasePath, 0);                      //定义IWorkspace型变量pWS并初始化，从pWSF打开文件赋给pWS

            
            Conn =AccessHelper.OpenConnection(databasePath);
            

            try
            {
                string strInsert = "DELETE  *   FROM   Vissim_LINKS ";
                OleDbCommand inst = new OleDbCommand(strInsert, Conn);
                inst.ExecuteNonQuery();
            }
            catch
            {
                string strCreatTable = "CREATE TABLE Vissim_LINKS(id LONG, Name TEXT(255), Cost DOUBLE, Gradient DOUBLE, Lanewidth DOUBLE, Length DOUBLE, Numlanes LONG, Surcharge1 DOUBLE, Surcharge2 DOUBLE, Type LONG, FNode TEXT(200), TNode TEXT(200), OverNode MEMO, ArcID LONG)";
                OleDbCommand inst = new OleDbCommand(strCreatTable, Conn);
                inst.ExecuteNonQuery();
            }
            try
            {
                string strInsert = "DELETE  *   FROM   Vissim_Connector ";
                OleDbCommand inst = new OleDbCommand(strInsert, Conn);
                inst.ExecuteNonQuery();
            }
            catch
            {
                string strCreatTable = "CREATE TABLE Vissim_Connector(id LONG, Name TEXT(255), FLink LONG, TLink LONG, FLane TEXT(50), TLane TEXT(50), FPos DOUBLE, TPos DOUBLE, Overnode MEMO)";
                OleDbCommand inst = new OleDbCommand(strCreatTable, Conn);
                inst.ExecuteNonQuery();
            }

            //获取Link和Arc句柄
            IEnumDataset enumDs;
            enumDs = ws.get_Datasets(esriDatasetType.esriDTFeatureClass);        //从工作空间获取数据集

            //定义Link,Node的featureclass
            IFeatureClass pFeatClsLink = null;
            IFeatureClass pFeatClsArc = null;
            //IFeatureClass NewpFeatClsArc = null;
            IFeatureClass featClass = enumDs.Next() as IFeatureClass;         //定义一个 IFeatureClass型变量featClass并初始化，并把enumDs赋给featClass
            while (featClass != null)
            {
                if (featClass.AliasName == "Link")              //如果featClass不为空，当featClass的AliasName与className相同，返回featClass
                {
                    pFeatClsLink = featClass;

                }
                if (featClass.AliasName == "Arc")
                {
                    pFeatClsArc = featClass;
                }
                featClass = enumDs.Next() as IFeatureClass;
            }

            //找不到Link或Node， 返回false
            if (pFeatClsLink == null || pFeatClsArc == null)
            {
                MessageBox.Show("Can't find link or arc!");
                return false;
            }

            //1.获取当前坐标系，转换为Vissim坐标系
            //NewpFeatClsArc = Setcoorsystem(pFeatClsArc,pFLayer);

            //2.根据Arc生成LINKS
            string LINKS = CreateLINKS(pFeatClsArc);

            //3.根据Arc、LaneConnectors生成Connector
            string Connector = CreateConnector(pFeatClsArc);

            //4.合并LINKS、Connector，生成inp文件
            CreateInp(apppath, LINKS, Connector);

            return true;
        }

        //1.获取当前坐标系，转换为Vissim坐标系
        //private static IFeatureClass Setcoorsystem(IFeatureClass pFeatClsArc,IFeatureLayer pFeatureLayer)
        //{
        //    IPolyline line;
        //    IFeature pFeatQuery;
        //    ISpatialReference spline;
        //    string Oldnn = "";
        //    IQueryFilter pFilter = new QueryFilterClass();
        //    IFeatureCursor fCursor = pFeatClsArc.Search(pFilter, false);
        //    pFeatQuery = fCursor.NextFeature();
        //    while (pFeatQuery != null)
        //    {
        //        line = pFeatQuery.Shape as IPolyline;
        //        spline = line.SpatialReference;
        //        Oldnn = spline.Name;
        //        break;
        //    }

        //    if (Oldnn == VissimCoorsystem)
        //    {
        //        IFeatureClass NewpFeatClsArc = pFeatureLayer.FeatureClass;
        //        return NewpFeatClsArc;
        //    }
        //    else if (Oldnn == "")
        //    {
        //        ISpatialReferenceFactory pfactory = new SpatialReferenceEnvironmentClass();

        //        //广州处于东经113，遂将投影坐标系（大地坐标系）设定为西安1980三度带esriSRProjCS_Xian1980_3_Degree_GK_CM_114E
        //        IProjectedCoordinateSystem flatref = pfactory.CreateProjectedCoordinateSystem(2383);

        //        IGeometry geo = (IGeometry)pFeatureLayer;
        //        geo.SpatialReference = flatref;

        //        IFeatureClass NewpFeatClsArc = pFeatureLayer.FeatureClass;
        //        return NewpFeatClsArc;
        //    }
        //    else
        //    {
        //        ISpatialReferenceFactory pfactory = new SpatialReferenceEnvironmentClass();

        //        //因为现有的广州地图的地理坐标系（GCS）为WGS1984，所以将地理坐标系设为esriSRGeoCS_WGS1984)
        //        //IGeographicCoordinateSystem earthref = pfactory.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);

        //        //广州处于东经113，遂将投影坐标系（大地坐标系）设定为西安1980三度带esriSRProjCS_Xian1980_3_Degree_GK_CM_114E
        //        IProjectedCoordinateSystem flatref = pfactory.CreateProjectedCoordinateSystem(2383);
        //        IGeometry geo = (IGeometry)pFeatureLayer;
        //        geo.Project(flatref);

        //        IFeatureClass NewpFeatClsArc = pFeatureLayer.FeatureClass;
        //        return NewpFeatClsArc;
        //    }
        //}

        //2.根据Arc生成LINKS
        private static string CreateLINKS(IFeatureClass pFeatClsArc)
        {
            //全选路段，存于pCursor
            IQueryFilter pFilter = new QueryFilterClass();
            IFeatureCursor pCursor = pFeatClsArc.Search(pFilter, false);

            IPoint pPoint1, pPoint2, pPoint3;
            int m = 0, b, c, d, f;
            int num;
            double len = 0.00;

            IFeature pFeatQuery;
            pFeatQuery = pCursor.NextFeature();

            while (pFeatQuery != null)//遍历弧段
            {
                string rodename = "", FNode = "", TNode = "", OverNode = "";
                int arcid, linkid, lanenum;
                m++;
                f = pFeatClsArc.FindField("ArcID");//当前弧段的ArcID
                arcid = Convert.ToInt32(pFeatQuery.get_Value(f));
                b = pFeatClsArc.FindField("LinkID");//当前弧段的LinkID
                linkid = Convert.ToInt32(pFeatQuery.get_Value(b));
                string Str = "select RoadName from Link where LinkID=" + linkid;
                OleDbCommand Com_1 = new OleDbCommand(Str, Conn);
                rodename = Convert.ToString(Com_1.ExecuteScalar());
                c = pFeatClsArc.FindField(Arc.LaneNumNm);//当前弧段的LaneNum
                lanenum = Convert.ToInt32(pFeatQuery.get_Value(c));
                d = pFeatClsArc.FindField("Shape_Length");//当前弧段的弧段长度
                len = Convert.ToDouble(pFeatQuery.get_Value(d));

                //取Arc的控制点
                IPointCollection pPointCollection = pFeatQuery.Shape as IPointCollection;

                //取Arc的端点，存储到pPoint1，pPoint2中
                pPoint1 = pPointCollection.get_Point(0) as IPoint;
                pPoint2 = pPointCollection.get_Point(pPointCollection.PointCount - 1) as IPoint;
                FNode = pPoint1.X + " " + pPoint1.Y;
                TNode = pPoint2.X + " " + pPoint2.Y;
                for (int k = 1; k < pPointCollection.PointCount - 1; k++)
                {
                    pPoint3 = pPointCollection.get_Point(k) as IPoint;
                    OverNode += pPoint3.X + " " + pPoint3.Y + " " + "0.000" + ";";
                }
                OleDbCommand insertCom = new OleDbCommand();
                insertCom.Connection = Conn;
                string str = "insert into Vissim_LINKS(id,Name,Cost,Gradient,Lanewidth,Length,Numlanes,Surcharge1,Surcharge2,Type,FNode,TNode,OverNode,ArcID) Values(" + m + ",'" + rodename + "',0,0," + LANEWIDTH +", "+ len + "," + lanenum + ",0,0,1,'" + FNode.ToString() + "','" + TNode + "','" + OverNode + "'," + arcid + ")";
                insertCom.CommandText = str;
                insertCom.ExecuteNonQuery();

                pFeatQuery = pCursor.NextFeature();
            }
            //输出表格数据到文本,LINKS
            string Str0 = "select max(id) from Vissim_LINKS";
            OleDbCommand Com = new OleDbCommand(Str0, Conn);
            num = (int)Com.ExecuteScalar();
            string links;
            

            links = "\r\n" + "-- Links: --" + "\r\n" + "------------" + "\r\n";
            for (int a = 1; a <= num; a++)
            {
                int n0, n2, n4;
                double n3, n5, n6, n7, n8, n9;
                string n1, n10, n11, n12;
                string Str_1 = "select ArcID from Vissim_LINKS where id=" + a;
                OleDbCommand Com_1 = new OleDbCommand(Str_1, Conn);
                n0 = Convert.ToInt32(Com_1.ExecuteScalar());

                Str_1 = "select Name from Vissim_LINKS where id=" + a;
                Com_1 = new OleDbCommand(Str_1, Conn);
                n1 = Convert.ToString(Com_1.ExecuteScalar());

                Str_1 = "select Type from Vissim_LINKS where id=" + a.ToString();
                Com_1 = new OleDbCommand(Str_1, Conn);
                n2 = Convert.ToInt32(Com_1.ExecuteScalar());

                Str_1 = "select Length from Vissim_LINKS where id=" + a.ToString();
                Com_1 = new OleDbCommand(Str_1, Conn);
                n3 = Convert.ToDouble(Com_1.ExecuteScalar());

                Str_1 = "select Numlanes from Vissim_LINKS where id=" + a.ToString();
                Com_1 = new OleDbCommand(Str_1, Conn);
                n4 = Convert.ToInt32(Com_1.ExecuteScalar());

                Str_1 = "select Lanewidth from Vissim_LINKS where id=" + a.ToString();
                Com_1 = new OleDbCommand(Str_1, Conn);
                n5 = Convert.ToDouble(Com_1.ExecuteScalar());

                Str_1 = "select Gradient from Vissim_LINKS where id=" + a.ToString();
                Com_1 = new OleDbCommand(Str_1, Conn);
                n6 = Convert.ToDouble(Com_1.ExecuteScalar());

                Str_1 = "select Cost from Vissim_LINKS where id=" + a.ToString();
                Com_1 = new OleDbCommand(Str_1, Conn);
                n7 = Convert.ToDouble(Com_1.ExecuteScalar());

                Str_1 = "select Surcharge1 from Vissim_LINKS where id=" + a.ToString();
                Com_1 = new OleDbCommand(Str_1, Conn);
                n8 = Convert.ToDouble(Com_1.ExecuteScalar());

                Str_1 = "select Surcharge2 from Vissim_LINKS where id=" + a.ToString();
                Com_1 = new OleDbCommand(Str_1, Conn);
                n9 = Convert.ToDouble(Com_1.ExecuteScalar());

                Str_1 = "select FNode from Vissim_LINKS where id=" + a.ToString();
                Com_1 = new OleDbCommand(Str_1, Conn);
                n10 = Convert.ToString(Com_1.ExecuteScalar());

                Str_1 = "select TNode from Vissim_LINKS where id=" + a.ToString();
                Com_1 = new OleDbCommand(Str_1, Conn);
                n11 = Convert.ToString(Com_1.ExecuteScalar());

                Str_1 = "select OverNode from Vissim_LINKS where id=" + a.ToString();
                Com_1 = new OleDbCommand(Str_1, Conn);
                object o = Com_1.ExecuteScalar();
                if (o == null)
                    n12 = "";
                else
                    n12 = o.ToString();

                //sw.WriteLine("LINK     " + a + " NAME " + "\"" + n1 + "\"" + " LABEL " + " 0.00 0.00");

                links += "LINK     " + a + " NAME " + "\"" + n1 + "\"" + " LABEL " + " 0.00 0.00" + "\r\n";

                string Lanewidth = "";
                for (int i = 1; i <= n4; i++)
                {
                    Lanewidth += n5 + "  ";
                }
                //sw.WriteLine("  TYPE      " + n2 + " LENGTH  " + n3 + " LANES  " + n4 + " LANE_WIDTH  " + Lanewidth + "GRADIENT 0.00000  " + " COST 0.00000" + " SURCHARGE 0.00000" + " SURCHARGE 0.00000" + " SEGMENT LENGTH  10.000");
                //sw.WriteLine("  FROM  " + n10);

                links += "  TYPE      " + n2 + " LENGTH  " + n3 + " LANES  " + n4 + " LANE_WIDTH  " + Lanewidth + "GRADIENT 0.00000  " + " COST 0.00000" + " SURCHARGE 0.00000" + " SURCHARGE 0.00000" + " SEGMENT LENGTH  10.000" + "\r\n" + "  FROM  " + n10 + "\r\n";

                if (n12 != "")
                {
                    string[] Overnodes = n12.Split(';');
                    string Ovnode = "";
                    for (int n = 0; n < Overnodes.Length - 1; n++)
                    {
                        Ovnode += "  OVER " + Overnodes[n];
                    }
                    //sw.WriteLine(Ovnode);

                    links += Ovnode + "\r\n";
                }
                //sw.WriteLine("  TO    " + n11);
                links += "  TO    " + n11 + "\r\n";
            }
            //sw.Flush();
            //sw.Close();
            return links;
        }

        //3.根据Arc、LaneConnectors生成Connector
        private static string CreateConnector(IFeatureClass pFeatClsArc)
        {
            OleDbCommand cmd = new OleDbCommand();
            OleDbDataReader reader;
            string strSql, str;
            strSql = "select * from LaneConnectors order by ConnectorID";
            cmd.CommandText = strSql;
            cmd.Connection = Conn;
            reader = cmd.ExecuteReader();

            int ConnectorID, fromArcID, toArcID, fromLaneID, toLaneID, flanepos, tlanepos, fnumlane, tnumlane, flane = 0, tlane = 0, flink, tlink;
            double len, FPos, TPos;
            string OverNode = "", rodename = "";
            while (reader.Read())
            {
                ConnectorID = (int)reader["ConnectorID"];
                fromArcID = (int)reader["fromArcID"];
                toArcID = (int)reader["toArcID"];
                fromLaneID = (int)reader["fromLaneID"];
                toLaneID = (int)reader["toLaneID"];
                string Str0 = "select Length from Vissim_LINKS where ArcID=" + fromArcID;
                OleDbCommand Com_1 = new OleDbCommand(Str0, Conn);
                len = Convert.ToDouble(Com_1.ExecuteScalar());
                Com_1.Dispose();

                Str0 = "select id from Vissim_LINKS where ArcID=" + fromArcID;
                Com_1 = new OleDbCommand(Str0, Conn);
                flink = Convert.ToInt32(Com_1.ExecuteScalar());
                Com_1.Dispose();
                Str0 = "select id from Vissim_LINKS where ArcID=" + toArcID;
                Com_1 = new OleDbCommand(Str0, Conn);
                tlink = Convert.ToInt32(Com_1.ExecuteScalar());
                Com_1.Dispose();

                Str0 = "select Numlanes from Vissim_LINKS where ArcID=" + fromArcID;
                Com_1 = new OleDbCommand(Str0, Conn);
                fnumlane = Convert.ToInt32(Com_1.ExecuteScalar());
                Com_1.Dispose();
                Str0 = "select Numlanes from Vissim_LINKS where ArcID=" + toArcID;
                Com_1 = new OleDbCommand(Str0, Conn);
                tnumlane = Convert.ToInt32(Com_1.ExecuteScalar());
                Com_1.Dispose();
                Str0 = "select [Position] from Lane where LaneID=" + fromLaneID;
                Com_1 = new OleDbCommand(Str0, Conn);
                flanepos = Convert.ToInt32(Com_1.ExecuteScalar());
                Com_1.Dispose();
                Str0 = "select [Position] from Lane where LaneID=" + toLaneID;
                Com_1 = new OleDbCommand(Str0, Conn);
                tlanepos = Convert.ToInt32(Com_1.ExecuteScalar());
                FPos = len - 0.100;
                TPos = 0.100;
                int m = 9999 + ConnectorID;
                int k = 0, j = 0;
                for (int i = fnumlane; i >= 1; i--)
                {
                    k++;
                    if (flanepos == i)
                        flane = k;
                }
                for (int i = tnumlane; i >= 1; i--)
                {
                    j++;
                    if (tlanepos == i)
                        tlane = j;
                }
                OverNode = SearchOverNode(pFeatClsArc, fromArcID, toArcID, fnumlane, tnumlane, flanepos, tlanepos);
                OleDbCommand insertCom = new OleDbCommand();
                insertCom.Connection = Conn;
                str = "insert into Vissim_Connector(id,Name,FLink,TLink,FLane,TLane,FPos,TPos,Overnode) Values(" + m + ",'" + rodename + "'," + flink + "," + tlink + ",'" + flane + "','" + tlane + "'," + FPos + "," + TPos + ",'" + OverNode + "')";
                insertCom.CommandText = str;
                insertCom.ExecuteNonQuery();
            }
            //输出表格数据到文本,Connectors
            string Str = "select max(id) from Vissim_Connector";
            OleDbCommand Com = new OleDbCommand(Str, Conn);
            int num = Convert.ToInt32(Com.ExecuteScalar());

            string connectors;
            //FileStream fs = new FileStream("F:\\项目\\863\\temp数据\\new\\Vissim\\Connectors.txt", FileMode.Create, FileAccess.Write);
            //StreamWriter sw = new StreamWriter(fs);
            //sw.Flush();
            //sw.BaseStream.Seek(0, SeekOrigin.Begin);
            //sw.WriteLine();
            //sw.WriteLine();
            //sw.WriteLine("-- Connectors: --");
            //sw.WriteLine("------------");

            connectors = "\r\n" + "\r\n" + "-- Connectors: --" + "\r\n" + "------------" + "\r\n";    //string str="第一行" + Environment.NewLine + "第二行";
            for (int a = 10000; a <= num; a++)
            {
                int n2, n3, n6, n7;
                double n4, n8;
                string n1, n5;
                string Str_1 = "select Name from Vissim_Connector where id=" + a;
                OleDbCommand Com_1 = new OleDbCommand(Str_1, Conn);
                n1 = Convert.ToString(Com_1.ExecuteScalar());

                Str_1 = "select FLink from Vissim_Connector where id=" + a;
                Com_1 = new OleDbCommand(Str_1, Conn);
                n2 = Convert.ToInt32(Com_1.ExecuteScalar());

                Str_1 = "select FLane from Vissim_Connector where id=" + a;
                Com_1 = new OleDbCommand(Str_1, Conn);
                n3 = Convert.ToInt32(Com_1.ExecuteScalar());

                Str_1 = "select FPos from Vissim_Connector where id=" + a;
                Com_1 = new OleDbCommand(Str_1, Conn);
                n4 = Convert.ToDouble(Com_1.ExecuteScalar());

                Str_1 = "select Overnode from Vissim_Connector where id=" + a;
                Com_1 = new OleDbCommand(Str_1, Conn);
                object o = Com_1.ExecuteScalar();
                if (o == null)
                    n5 = "";
                else
                    n5 = o.ToString();

                Str_1 = "select TLink from Vissim_Connector where id=" + a;
                Com_1 = new OleDbCommand(Str_1, Conn);
                n6 = Convert.ToInt32(Com_1.ExecuteScalar());

                Str_1 = "select TLane from Vissim_Connector where id=" + a;
                Com_1 = new OleDbCommand(Str_1, Conn);
                n7 = Convert.ToInt32(Com_1.ExecuteScalar());

                Str_1 = "select TPos from Vissim_Connector where id=" + a;
                Com_1 = new OleDbCommand(Str_1, Conn);
                n8 = Convert.ToDouble(Com_1.ExecuteScalar());

                //sw.WriteLine("CONNECTOR " + a + " NAME " + "\"" + n1 + "\"" + " LABEL  0.00 0.00");
                connectors += "CONNECTOR " + a + " NAME " + "\"" + n1 + "\"" + " LABEL  0.00 0.00" + "\r\n";

                //sw.WriteLine("  FROM LINK " + n2 + " LANES " + n3 + " AT " + n4);
                connectors += "  FROM LINK " + n2 + " LANES " + n3 + " AT " + n4 + "\r\n";

                if (n5 != "")
                {
                    string[] Overnodes = n5.Split(';');
                    string Ovnode = "";
                    for (int n = 0; n < Overnodes.Length; n++)
                    {
                        Ovnode += "  OVER " + Overnodes[n];
                    }
                    //sw.WriteLine(Ovnode);
                    connectors += Ovnode + "\r\n";
                }
                if (n8 == 0.00)
                {
                    //sw.WriteLine("  TO LINK " + n6 + " LANES " + n7 + " AT 0.000 " + " ALL");
                    connectors += "  TO LINK " + n6 + " LANES " + n7 + " AT 0.000 " + " ALL" + "\r\n";
                }
                else
                {
                    //sw.WriteLine("  TO LINK " + n6 + " LANES " + n7 + " AT 0.100 " + " ALL");
                    connectors += "  TO LINK " + n6 + " LANES " + n7 + " AT 0.100 " + " ALL" + "\r\n";
                }
                //sw.WriteLine("  DX_EMERG_STOP 5.000 DX_LANE_CHANGE 200.000");
                //sw.WriteLine("  GRADIENT 0.00000  COST 0.00000  SURCHARGE 0.00000  SURCHARGE 0.00000");
                //sw.WriteLine("  SEGMENT LENGTH 10.000 ANIMATION");

                connectors += "  DX_EMERG_STOP 5.000 DX_LANE_CHANGE 200.000" + "\r\n" + "  GRADIENT 0.00000  COST 0.00000  SURCHARGE 0.00000  SURCHARGE 0.00000" + "\r\n" + "  SEGMENT LENGTH 10.000 ANIMATION" + "\r\n";
            }
            //sw.Flush();
            //sw.Close();
            Conn.Close();
            return connectors;
            //MessageBox.Show("F:\\项目\\863\\temp数据\\new\\Vissim\\Links.txt,Connectors.txt");
        }

        //4.合并LINKS、Connector，生成inp文件
        private static void CreateInp(string app, string L, string C)
        {
            //byte[] b1 = System.Text.Encoding.UTF8.GetBytes(myString);
            //byte[] b2 = System.Text.Encoding.ASCII.GetBytes(myString);

            //string pi = "\u03a0";
            //byte[] ascii = System.Text.Encoding.ASCII.GetBytes(pi);
            //byte[] utf8 = System.Text.Encoding.UTF8.GetBytes(pi);

            //Console.WriteLine(ascii.Length); //will print 1
            //Console.WriteLine(utf8.Length); //will print 2
            //Console.WriteLine(System.Text.Encoding.ASCII.GetString(ascii)); //will print

            string LaddC = L + "\r\n" + C;
            byte[] utf8 = System.Text.Encoding.UTF8.GetBytes(LaddC);

            FileStream _file = new FileStream("Vissim_data1.txt", FileMode.Open, FileAccess.Read, FileShare.Read);
            //MemoryStream _memfile = new MemoryStream();
            byte[] utf8data1 = new byte[(int)_file.Length]; //_memfile.GetBuffer();
            //从_file当前流段的位置,一直到结尾.注意: 是从0开始的.而不流当前的位置,否则报错.   
            _file.Read(utf8data1, 0, (int)_file.Length);
            _file.Close();
            _file.Dispose();

            FileStream _file2 = new FileStream("Vissim_data2.txt", FileMode.Open, FileAccess.Read, FileShare.Read);
            //MemoryStream _memfile2 = new MemoryStream();
            byte[] utf8data2 = new byte[(int)_file2.Length];
            _file2.Read(utf8data2, 0, (int)_file2.Length);
            _file2.Close();
            _file2.Dispose();

            int len = utf8.Length + utf8data1.Length + utf8data2.Length;
            byte[] utf8vissim = new byte[len];
            for (int i = 0; i < utf8data1.Length; i++)
            {
                utf8vissim[i] = utf8data1[i];
            }
            for (int i = 0; i < utf8.Length; i++)
            {
                utf8vissim[i + utf8data1.Length] = utf8[i];
            }
            for (int i = 0; i < utf8data2.Length; i++)
            {
                utf8vissim[i + utf8data1.Length + utf8.Length] = utf8data2[i];
            }

            string path = app + "SimRoadFile";
            string path1 = app + "SimRoadFile\\Vissim";
            //string path2 = app + "SimRoadFile\\TM";
            //string path3 = app + "SimRoadFile\\Paramics";
            if (Directory.Exists(path))//判断是否存在
            {
                if (!Directory.Exists(path1))
                    Directory.CreateDirectory(path1);
                //if (!Directory.Exists(path2))
                //    Directory.CreateDirectory(path2);
                //if (!Directory.Exists(path3))
                //    Directory.CreateDirectory(path3);
            }
            else
            {
                Directory.CreateDirectory(path);//创建新路径
                Directory.CreateDirectory(path1);
                //Directory.CreateDirectory(path2);
                //Directory.CreateDirectory(path3);
            }

            FileStream fs = new FileStream(path1 + "\\Vissim.inp", FileMode.Create, FileAccess.Write);
            fs.Write(utf8vissim, 0, utf8vissim.Length);
            fs.Close();
            MessageBox.Show(path1 + "\\Vissim.inp");
        }

        private static string SearchOverNode(IFeatureClass pFeatClsArc, int fromArcID, int toArcID, int fnumlane, int tnumlane, int flanepos, int tlanepos)
        {
            //此函数用于生成连接器的中间点
            string OverNode = "";

            IPoint pPoint1, pPoint2;
            double pPoint3X, pPoint3Y, pPoint3Z, pPoint4X, pPoint4Y, pPoint4Z, pPoint5X, pPoint5Y, pPoint5Z;
            IPolyline line;
            double Measure;
            IFeature pFeatQuery;
            List<IPoint> fPoint = new List<IPoint>();
            List<IPoint> tPoint = new List<IPoint>();

            string Str = "select OBJECTID from Arc where ArcID=" + fromArcID;
            OleDbCommand Com_1 = new OleDbCommand(Str, Conn);
            int FOBID = Convert.ToInt32(Com_1.ExecuteScalar());
            Str = "select OBJECTID from Arc where ArcID=" + toArcID;
            Com_1 = new OleDbCommand(Str, Conn);
            int TOBID = Convert.ToInt32(Com_1.ExecuteScalar());
            Str = "select Shape_Length from Arc where ArcID=" + fromArcID;
            Com_1 = new OleDbCommand(Str, Conn);
            double len = Convert.ToDouble(Com_1.ExecuteScalar());
            Str = "select Lanewidth from Vissim_LINKS where ArcID=" + fromArcID;
            Com_1 = new OleDbCommand(Str, Conn);
            double lanwidth = Convert.ToDouble(Com_1.ExecuteScalar());

            pFeatQuery = pFeatClsArc.GetFeature(FOBID);
            line = pFeatQuery.Shape as IPolyline;       //获取对应的中心线（首）
            Measure = len - 0.100;
            fPoint = SearchPoint(line, fnumlane, lanwidth, Measure);

            pFeatQuery = pFeatClsArc.GetFeature(TOBID);
            line = pFeatQuery.Shape as IPolyline;       //获取对应的中心线（尾）
            Measure = 0.100;
            tPoint = SearchPoint(line, tnumlane, lanwidth, Measure);

            pPoint1 = fPoint[flanepos - 1] as IPoint;
            pPoint2 = tPoint[tlanepos - 1] as IPoint;

            pPoint3X = (pPoint1.X + pPoint2.X) / 2;
            pPoint3Y = (pPoint1.Y + pPoint2.Y) / 2;
            pPoint3Z = (pPoint1.Z + pPoint2.Z) / 2;

            pPoint4X = (pPoint1.X + pPoint3X) / 2;
            pPoint4Y = (pPoint1.Y + pPoint3Y) / 2;
            pPoint4Z = (pPoint1.Z + pPoint3Z) / 2;

            pPoint5X = (pPoint2.X + pPoint3X) / 2;
            pPoint5Y = (pPoint2.Y + pPoint3Y) / 2;
            pPoint5Z = (pPoint2.Z + pPoint3Z) / 2;

            OverNode = pPoint1.X + " " + pPoint1.Y + " " + "0.000" + ";" + pPoint4X + " " + pPoint4Y + " " + "0.000" + ";" + pPoint3X + " " + pPoint3Y + " " + "0.000" + ";" + pPoint5X + " " + pPoint5Y + " " + "0.000" + ";" + pPoint2.X + " " + pPoint2.Y + " " + "0.000";

            return OverNode;
        }

        private static List<IPoint> SearchPoint(IPolyline line, int numlane, double lanwidth, double Measure)
        {
            //此函数用于生成连接器的起点、终点
            double Offset;
            IPoint P;
            List<IPoint> Point = new List<IPoint>();
            if (numlane == 1)
            {
                Offset = 0.00;
                P = PointHelper.CreatPointFromLineByLRS(line, Offset, Measure);
                Point.Add(P);
            }
            if (numlane == 2)
            {
                Offset = -lanwidth * 0.5;
                P = PointHelper.CreatPointFromLineByLRS(line, Offset, Measure);
                Point.Add(P);
                Offset = lanwidth * 0.5;
                P = PointHelper.CreatPointFromLineByLRS(line, Offset, Measure);
                Point.Add(P);
            }
            if (numlane == 3)
            {
                Offset = -lanwidth;
                P = PointHelper.CreatPointFromLineByLRS(line, Offset, Measure);
                Point.Add(P);
                Offset = 0.00;
                P = PointHelper.CreatPointFromLineByLRS(line, Offset, Measure);
                Point.Add(P);
                Offset = lanwidth;
                P = PointHelper.CreatPointFromLineByLRS(line, Offset, Measure);
                Point.Add(P);
            }
            if (numlane == 4)
            {
                Offset = -lanwidth * 1.5;
                P = PointHelper.CreatPointFromLineByLRS(line, Offset, Measure);
                Point.Add(P);
                Offset = -lanwidth * 0.5;
                P = PointHelper.CreatPointFromLineByLRS(line, Offset, Measure);
                Point.Add(P);
                Offset = lanwidth * 0.5;
                P = PointHelper.CreatPointFromLineByLRS(line, Offset, Measure);
                Point.Add(P);
                Offset = lanwidth * 1.5;
                P = PointHelper.CreatPointFromLineByLRS(line, Offset, Measure);
                Point.Add(P);
            }
            if (numlane == 5)
            {
                Offset = -lanwidth * 2;
                P = PointHelper.CreatPointFromLineByLRS(line, Offset, Measure);
                Point.Add(P);
                Offset = -lanwidth;
                P = PointHelper.CreatPointFromLineByLRS(line, Offset, Measure);
                Point.Add(P);
                Offset = 0.00;
                P = PointHelper.CreatPointFromLineByLRS(line, Offset, Measure);
                Point.Add(P);
                Offset = lanwidth;
                P = PointHelper.CreatPointFromLineByLRS(line, Offset, Measure);
                Point.Add(P);
                Offset = lanwidth * 2;
                P = PointHelper.CreatPointFromLineByLRS(line, Offset, Measure);
                Point.Add(P);
            }
            if (numlane == 6)
            {
                Offset = -lanwidth * 2.5;
                P = PointHelper.CreatPointFromLineByLRS(line, Offset, Measure);
                Point.Add(P);
                Offset = -lanwidth * 1.5;
                P = PointHelper.CreatPointFromLineByLRS(line, Offset, Measure);
                Point.Add(P);
                Offset = -lanwidth * 0.5;
                P = PointHelper.CreatPointFromLineByLRS(line, Offset, Measure);
                Point.Add(P);
                Offset = lanwidth * 0.5;
                P = PointHelper.CreatPointFromLineByLRS(line, Offset, Measure);
                Point.Add(P);
                Offset = lanwidth * 1.5;
                P = PointHelper.CreatPointFromLineByLRS(line, Offset, Measure);
                Point.Add(P);
                Offset = lanwidth * 2.5;
                P = PointHelper.CreatPointFromLineByLRS(line, Offset, Measure);
                Point.Add(P);
            }
            if (numlane == 7)
            {
                Offset = -lanwidth * 3;
                P = PointHelper.CreatPointFromLineByLRS(line, Offset, Measure);
                Point.Add(P);
                Offset = -lanwidth * 2;
                P = PointHelper.CreatPointFromLineByLRS(line, Offset, Measure);
                Point.Add(P);
                Offset = -lanwidth;
                P = PointHelper.CreatPointFromLineByLRS(line, Offset, Measure);
                Point.Add(P);
                Offset = 0.00;
                P = PointHelper.CreatPointFromLineByLRS(line, Offset, Measure);
                Point.Add(P);
                Offset = lanwidth;
                P = PointHelper.CreatPointFromLineByLRS(line, Offset, Measure);
                Point.Add(P);
                Offset = lanwidth * 2;
                P = PointHelper.CreatPointFromLineByLRS(line, Offset, Measure);
                Point.Add(P);
                Offset = lanwidth * 3;
                P = PointHelper.CreatPointFromLineByLRS(line, Offset, Measure);
                Point.Add(P);
            }
            return Point;
        }
    }
}
