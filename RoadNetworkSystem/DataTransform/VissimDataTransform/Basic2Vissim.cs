using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using System.Data.OleDb;
using System.IO;
using RoadNetworkSystem.GIS;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.ADO.Access;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LinkLayer;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LaneLayer;
using RoadNetworkSystem.GIS.Geometry;

namespace RoadNetworkSystem.VissimDataTransform
{
    class Basic2Vissim
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
        private static OleDbConnection _conn;
        
        //private IMap m_pMap;
        private static IFeatureClass _pFeaClsNode;
        private static IFeatureClass _pFeaClsLink;
        private static IFeatureClass _pFeaClsArc;

        private const int FIRSTLANEPOS = 0;
        private static string _vissimNetworkPath = "";

        public Basic2Vissim(OleDbConnection conn)
        {
            _conn = conn;
        }

        public Boolean CreateVissimdata(string apppath, string databasePath)
        {

            _vissimNetworkPath = apppath + "\\simulationNetwork\\Vissim";
            //本函数根据基础路网数据生成Vissim仿真数据文件

            //初始化工作
            IWorkspaceFactory pWSF = new AccessWorkspaceFactoryClass();          //定义IWorkspaceFactory型变量pWSF并初始化
            IWorkspace ws = pWSF.OpenFromFile(databasePath, 0);                      //定义IWorkspace型变量pWS并初始化，从pWSF打开文件赋给pWS



            //初始化需要用到的要素类
            _pFeaClsNode = (ws as IFeatureWorkspace).OpenFeatureClass(Node.NodeName);
            _pFeaClsLink = (ws as IFeatureWorkspace).OpenFeatureClass(Link.LinkName);
            _pFeaClsArc = (ws as IFeatureWorkspace).OpenFeatureClass(Arc.ArcFeatureName);

            //找不到Link或Node， 返回false
            if (_pFeaClsLink == null || _pFeaClsArc == null)
            {
                MessageBox.Show("Can't find link or arc!");
                return false;
            }


            #region 创建中间表 Vissim_LINKS 和 Vissim_Connector

            try
            {
                string strInsert = "DELETE  *   FROM   Vissim_LINKS ";
                OleDbCommand inst = new OleDbCommand(strInsert, _conn);
                inst.ExecuteNonQuery();
            }
            catch
            {
                string strCreatTable = "CREATE TABLE Vissim_LINKS(id LONG, Name TEXT(255), Cost DOUBLE, Gradient DOUBLE, Lanewidth DOUBLE, Length DOUBLE, Numlanes LONG, Surcharge1 DOUBLE, Surcharge2 DOUBLE, Type LONG, FNode TEXT(200), TNode TEXT(200), OverNode MEMO, ArcID LONG)";
                OleDbCommand inst = new OleDbCommand(strCreatTable, _conn);
                inst.ExecuteNonQuery();
            }
            try
            {
                string strInsert = "DELETE  *   FROM   Vissim_Connector ";
                OleDbCommand inst = new OleDbCommand(strInsert, _conn);
                inst.ExecuteNonQuery();
            }
            catch
            {
                string strCreatTable = "CREATE TABLE Vissim_Connector(id LONG, Name TEXT(255), FLink LONG, TLink LONG, FLane TEXT(50), TLane TEXT(50), FPos DOUBLE, TPos DOUBLE, Overnode MEMO)";
                OleDbCommand inst = new OleDbCommand(strCreatTable, _conn);
                inst.ExecuteNonQuery();
            }

            #endregion 创建中间表

            //1.获取当前坐标系，转换为Vissim坐标系
            //NewpFeatClsArc = Setcoorsystem(_pFeaClsArc,pFLayer);

            try
            {

                //2.根据Arc生成LINKS
                string LINKS = CreateLINKS(_pFeaClsArc);

                //3.根据Arc、LaneConnectors生成Connector
                string Connector = CreateConnector(_pFeaClsArc);

                //4.合并LINKS、Connector，生成inp文件
                CreateInp(apppath, LINKS, Connector);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return true;
        }

        #region 
        //1.获取当前坐标系，转换为Vissim坐标系
        //private static IFeatureClass Setcoorsystem(IFeatureClass _pFeaClsArc,IFeatureLayer pFeatureLayer)
        //{
        //    IPolyline line;
        //    IFeature pFeatQuery;
        //    ISpatialReference spline;
        //    string Oldnn = "";
        //    IQueryFilter pFilter = new QueryFilterClass();
        //    IFeatureCursor fCursor = _pFeaClsArc.Search(pFilter, false);
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

        #endregion

        //2.根据Arc生成LINKS
        private static string CreateLINKS(IFeatureClass _pFeaClsArc)
        {
            //全选路段，存于pCursor
            IQueryFilter pFilter = new QueryFilterClass();
            IFeatureCursor pCursor = _pFeaClsArc.Search(pFilter, false);

            IPoint pPoint1, pPoint2, pPoint3;
            int m = 0, b, c, d, f;
            int num;
            double len = 0.00;

            IFeature pFeatQuery;
            pFeatQuery = pCursor.NextFeature();


            LinkService link = new LinkService(_pFeaClsLink, 0);
            while (pFeatQuery != null)//遍历弧段
            {
                string rodename = "", FNode = "", TNode = "", OverNode = "";
                int arcid, linkid, lanenum;
                m++;
                f = _pFeaClsArc.FindField(Arc.ArcIDNm);//当前弧段的ArcID
                arcid = Convert.ToInt32(pFeatQuery.get_Value(f));
                b = _pFeaClsArc.FindField(Arc.LinkIDNm);//当前弧段的LinkID
                linkid = Convert.ToInt32(pFeatQuery.get_Value(b));

                string Str = "select " + link.RoadNameNm + " from " + Link.LinkName + " where " + link.IDNm + "=" + linkid;

                OleDbCommand Com_1 = new OleDbCommand(Str, _conn);
                rodename = Convert.ToString(Com_1.ExecuteScalar());
                c = _pFeaClsArc.FindField(Arc.LaneNumNm);//当前弧段的LaneNum
                lanenum = Convert.ToInt32(pFeatQuery.get_Value(c));

                //当前弧段的弧段长度
                len = (pFeatQuery.Shape as IPolyline).Length;

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
                insertCom.Connection = _conn;
                string str = "insert into Vissim_LINKS(id,Name,Cost,Gradient,Lanewidth,Length,Numlanes,Surcharge1,Surcharge2,Type,FNode,TNode,OverNode,ArcID) Values(" + m + ",'" + rodename + "',0,0," + LANEWIDTH +", "+ len + "," + lanenum + ",0,0,1,'" + FNode.ToString() + "','" + TNode + "','" + OverNode + "'," + arcid + ")";
                insertCom.CommandText = str;
                insertCom.ExecuteNonQuery();

                pFeatQuery = pCursor.NextFeature();
            }
            //输出表格数据到文本,LINKS
            string Str0 = "select max(id) from Vissim_LINKS";
            OleDbCommand Com = new OleDbCommand(Str0, _conn);
            num = (int)Com.ExecuteScalar();
            string links;
            

            links = "\r\n" + "-- Links: --" + "\r\n" + "------------" + "\r\n";
            for (int a = 1; a <= num; a++)
            {
                int n0, n2, n4;
                double n3, n5, n6, n7, n8, n9;
                string n1, n10, n11, n12;
                string Str_1 = "select ArcID from Vissim_LINKS where id=" + a;
                OleDbCommand Com_1 = new OleDbCommand(Str_1, _conn);
                n0 = Convert.ToInt32(Com_1.ExecuteScalar());

                Str_1 = "select Name from Vissim_LINKS where id=" + a;
                Com_1 = new OleDbCommand(Str_1, _conn);
                n1 = Convert.ToString(Com_1.ExecuteScalar());

                Str_1 = "select Type from Vissim_LINKS where id=" + a.ToString();
                Com_1 = new OleDbCommand(Str_1, _conn);
                n2 = Convert.ToInt32(Com_1.ExecuteScalar());

                Str_1 = "select Length from Vissim_LINKS where id=" + a.ToString();
                Com_1 = new OleDbCommand(Str_1, _conn);
                n3 = Convert.ToDouble(Com_1.ExecuteScalar());

                Str_1 = "select Numlanes from Vissim_LINKS where id=" + a.ToString();
                Com_1 = new OleDbCommand(Str_1, _conn);
                n4 = Convert.ToInt32(Com_1.ExecuteScalar());

                Str_1 = "select Lanewidth from Vissim_LINKS where id=" + a.ToString();
                Com_1 = new OleDbCommand(Str_1, _conn);
                n5 = Convert.ToDouble(Com_1.ExecuteScalar());

                Str_1 = "select Gradient from Vissim_LINKS where id=" + a.ToString();
                Com_1 = new OleDbCommand(Str_1, _conn);
                n6 = Convert.ToDouble(Com_1.ExecuteScalar());

                Str_1 = "select Cost from Vissim_LINKS where id=" + a.ToString();
                Com_1 = new OleDbCommand(Str_1, _conn);
                n7 = Convert.ToDouble(Com_1.ExecuteScalar());

                Str_1 = "select Surcharge1 from Vissim_LINKS where id=" + a.ToString();
                Com_1 = new OleDbCommand(Str_1, _conn);
                n8 = Convert.ToDouble(Com_1.ExecuteScalar());

                Str_1 = "select Surcharge2 from Vissim_LINKS where id=" + a.ToString();
                Com_1 = new OleDbCommand(Str_1, _conn);
                n9 = Convert.ToDouble(Com_1.ExecuteScalar());

                Str_1 = "select FNode from Vissim_LINKS where id=" + a.ToString();
                Com_1 = new OleDbCommand(Str_1, _conn);
                n10 = Convert.ToString(Com_1.ExecuteScalar());

                Str_1 = "select TNode from Vissim_LINKS where id=" + a.ToString();
                Com_1 = new OleDbCommand(Str_1, _conn);
                n11 = Convert.ToString(Com_1.ExecuteScalar());

                Str_1 = "select OverNode from Vissim_LINKS where id=" + a.ToString();
                Com_1 = new OleDbCommand(Str_1, _conn);
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
        private static string CreateConnector(IFeatureClass _pFeaClsArc)
        {
            OleDbCommand cmd = new OleDbCommand();
            OleDbDataReader reader;
            string strSql, str;
            strSql = "select * from " + LaneConnector.ConnectorName + " order by " + LaneConnectorFeatureService.ConnectorIDNm;
            cmd.CommandText = strSql;
            cmd.Connection = _conn;
            reader = cmd.ExecuteReader();

            int ConnectorID, fromArcID, toArcID, fromLaneID, toLaneID, flanepos, tlanepos, fnumlane, tnumlane, flane = 0, tlane = 0, flink, tlink;
            double len, FPos, TPos;
            string OverNode = "", rodename = "";
            while (reader.Read())
            {
                ConnectorID = (int)reader[LaneConnectorFeatureService.ConnectorIDNm];
                fromArcID = (int)reader[LaneConnectorFeatureService.fromArcIDNm];
                toArcID = (int)reader[LaneConnectorFeatureService.toArcIDNm];
                fromLaneID = (int)reader[LaneConnectorFeatureService.fromLaneIDNm];
                toLaneID = (int)reader[LaneConnectorFeatureService.toLaneIDNm];
                string Str0 = "select Length from Vissim_LINKS where ArcID=" + fromArcID;
                OleDbCommand Com_1 = new OleDbCommand(Str0, _conn);
                len = Convert.ToDouble(Com_1.ExecuteScalar());
                Com_1.Dispose();

                Str0 = "select id from Vissim_LINKS where ArcID=" + fromArcID;
                Com_1 = new OleDbCommand(Str0, _conn);
                flink = Convert.ToInt32(Com_1.ExecuteScalar());
                Com_1.Dispose();
                Str0 = "select id from Vissim_LINKS where ArcID=" + toArcID;
                Com_1 = new OleDbCommand(Str0, _conn);
                tlink = Convert.ToInt32(Com_1.ExecuteScalar());
                Com_1.Dispose();

                Str0 = "select Numlanes from Vissim_LINKS where ArcID=" + fromArcID;
                Com_1 = new OleDbCommand(Str0, _conn);
                fnumlane = Convert.ToInt32(Com_1.ExecuteScalar());
                Com_1.Dispose();

                Str0 = "select Numlanes from Vissim_LINKS where ArcID=" + toArcID;
                Com_1 = new OleDbCommand(Str0, _conn);
                tnumlane = Convert.ToInt32(Com_1.ExecuteScalar());
                Com_1.Dispose();

                Str0 = "select [Position] from Lane where LaneID=" + fromLaneID;
                Com_1 = new OleDbCommand(Str0, _conn);
                flanepos = Convert.ToInt32(Com_1.ExecuteScalar());
                Com_1.Dispose();

                Str0 = "select [Position] from Lane where LaneID=" + toLaneID;
                Com_1 = new OleDbCommand(Str0, _conn);
                tlanepos = Convert.ToInt32(Com_1.ExecuteScalar());

                FPos = len - 0.100;
                TPos = 0.100;


                int m = 9999 + ConnectorID;
                int k = 0, j = 0;
                for (int i = fnumlane - 1; i >= FIRSTLANEPOS; i--)
                {
                    k++;
                    if (flanepos == i)
                        flane = k;
                }
                for (int i = tnumlane - 1; i >= FIRSTLANEPOS; i--)
                {
                    j++;
                    if (tlanepos == i)
                        tlane = j;
                }
                OverNode = SearchOverNode(_pFeaClsArc, fromArcID, toArcID, fnumlane, tnumlane, flanepos, tlanepos);
                OleDbCommand insertCom = new OleDbCommand();
                insertCom.Connection = _conn;
                str = "insert into Vissim_Connector(id,Name,FLink,TLink,FLane,TLane,FPos,TPos,Overnode) Values(" + m + ",'" + rodename + "'," + flink + "," + tlink + ",'" + flane + "','" + tlane + "'," + FPos + "," + TPos + ",'" + OverNode + "')";
                insertCom.CommandText = str;
                insertCom.ExecuteNonQuery();
            }
            //输出表格数据到文本,Connectors
            string Str = "select max(id) from Vissim_Connector";
            OleDbCommand Com = new OleDbCommand(Str, _conn);
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



            Str = "select * from Vissim_Connector order by id";
            Com = new OleDbCommand(Str, _conn);
            OleDbDataReader read;
            read = Com.ExecuteReader();
            while (read.Read())
            {
                int n2, n3, n6, n7;
                double n4, n8;
                string n1, n5;
                int a = Convert.ToInt32(read["id"]);
                n1 = Convert.ToString(read["Name"]);
                n2 = Convert.ToInt32(read["FLink"]);
                n3 = Convert.ToInt32(read["FLane"]);
                n4 = Convert.ToDouble(read["FPos"]);
                object o= read["Overnode"];
                if (o != null)
                {
                    n5 = Convert.ToString(o);
                }
                else
                {
                    n5 = "";
                }
                n6 = Convert.ToInt32(read["TLink"]);
                n7 = Convert.ToInt32(read["TLane"]);
                n8 = Convert.ToDouble(read["TPos"]);
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


            //tlink = Convert.ToInt32(Com.ExecuteScalar());
            //Com.Dispose();

            //for (int a = 10000; a <= num; a++)
            //{
            //    int n2, n3, n6, n7;
            //    double n4, n8;
            //    string n1, n5;
            //    string Str_1 = "select Name from Vissim_Connector where id=" + a;
            //    OleDbCommand Com_1 = new OleDbCommand(Str_1, Conn);
            //    n1 = Convert.ToString(Com_1.ExecuteScalar());

            //    Str_1 = "select FLink from Vissim_Connector where id=" + a;
            //    Com_1 = new OleDbCommand(Str_1, Conn);
            //    n2 = Convert.ToInt32(Com_1.ExecuteScalar());

            //    Str_1 = "select FLane from Vissim_Connector where id=" + a;
            //    Com_1 = new OleDbCommand(Str_1, Conn);
            //    n3 = Convert.ToInt32(Com_1.ExecuteScalar());

            //    Str_1 = "select FPos from Vissim_Connector where id=" + a;
            //    Com_1 = new OleDbCommand(Str_1, Conn);
            //    n4 = Convert.ToDouble(Com_1.ExecuteScalar());

            //    Str_1 = "select Overnode from Vissim_Connector where id=" + a;
            //    Com_1 = new OleDbCommand(Str_1, Conn);
            //    object o = Com_1.ExecuteScalar();
            //    if (o == null)
            //        n5 = "";
            //    else
            //        n5 = o.ToString();

            //    Str_1 = "select TLink from Vissim_Connector where id=" + a;
            //    Com_1 = new OleDbCommand(Str_1, Conn);
            //    n6 = Convert.ToInt32(Com_1.ExecuteScalar());

            //    Str_1 = "select TLane from Vissim_Connector where id=" + a;
            //    Com_1 = new OleDbCommand(Str_1, Conn);
            //    n7 = Convert.ToInt32(Com_1.ExecuteScalar());

            //    Str_1 = "select TPos from Vissim_Connector where id=" + a;
            //    Com_1 = new OleDbCommand(Str_1, Conn);
            //    n8 = Convert.ToDouble(Com_1.ExecuteScalar());

            //    //sw.WriteLine("CONNECTOR " + a + " NAME " + "\"" + n1 + "\"" + " LABEL  0.00 0.00");
            //    connectors += "CONNECTOR " + a + " NAME " + "\"" + n1 + "\"" + " LABEL  0.00 0.00" + "\r\n";

            //    //sw.WriteLine("  FROM LINK " + n2 + " LANES " + n3 + " AT " + n4);
            //    connectors += "  FROM LINK " + n2 + " LANES " + n3 + " AT " + n4 + "\r\n";

            //    if (n5 != "")
            //    {
            //        string[] Overnodes = n5.Split(';');
            //        string Ovnode = "";
            //        for (int n = 0; n < Overnodes.Length; n++)
            //        {
            //            Ovnode += "  OVER " + Overnodes[n];
            //        }
            //        //sw.WriteLine(Ovnode);
            //        connectors += Ovnode + "\r\n";
            //    }
            //    if (n8 == 0.00)
            //    {
            //        //sw.WriteLine("  TO LINK " + n6 + " LANES " + n7 + " AT 0.000 " + " ALL");
            //        connectors += "  TO LINK " + n6 + " LANES " + n7 + " AT 0.000 " + " ALL" + "\r\n";
            //    }
            //    else
            //    {
            //        //sw.WriteLine("  TO LINK " + n6 + " LANES " + n7 + " AT 0.100 " + " ALL");
            //        connectors += "  TO LINK " + n6 + " LANES " + n7 + " AT 0.100 " + " ALL" + "\r\n";
            //    }
            //    //sw.WriteLine("  DX_EMERG_STOP 5.000 DX_LANE_CHANGE 200.000");
            //    //sw.WriteLine("  GRADIENT 0.00000  COST 0.00000  SURCHARGE 0.00000  SURCHARGE 0.00000");
            //    //sw.WriteLine("  SEGMENT LENGTH 10.000 ANIMATION");

            //    connectors += "  DX_EMERG_STOP 5.000 DX_LANE_CHANGE 200.000" + "\r\n" + "  GRADIENT 0.00000  COST 0.00000  SURCHARGE 0.00000  SURCHARGE 0.00000" + "\r\n" + "  SEGMENT LENGTH 10.000 ANIMATION" + "\r\n";
            //}
            ////sw.Flush();
            ////sw.Close();
            //Conn.Close();
            return connectors;
            //MessageBox.Show("F:\\项目\\863\\temp数据\\new\\Vissim\\Links.txt,Connectors.txt");
        }

        //4.合并LINKS、Connector，生成inp文件
        private static void CreateInp(string app, string Links, string Conns)
        {
            //byte[] b1 = System.Text.Encoding.UTF8.GetBytes(myString);
            //byte[] b2 = System.Text.Encoding.ASCII.GetBytes(myString);

            //string pi = "\u03a0";
            //byte[] ascii = System.Text.Encoding.ASCII.GetBytes(pi);
            //byte[] utf8 = System.Text.Encoding.UTF8.GetBytes(pi);

            //Console.WriteLine(ascii.Length); //will print 1
            //Console.WriteLine(utf8.Length); //will print 2
            //Console.WriteLine(System.Text.Encoding.ASCII.GetString(ascii)); //will print

            string LaddC = Links + "\r\n" + Conns;
            byte[] utf8 = System.Text.Encoding.UTF8.GetBytes(LaddC);

            string data1Path = _vissimNetworkPath + "\\Vissim_data1.txt";
            FileStream _file = new FileStream(data1Path, FileMode.Open, FileAccess.Read, FileShare.Read);
            //MemoryStream _memfile = new MemoryStream();
            byte[] utf8data1 = new byte[(int)_file.Length]; //_memfile.GetBuffer();
            //从_file当前流段的位置,一直到结尾.注意: 是从0开始的.而不流当前的位置,否则报错.   
            _file.Read(utf8data1, 0, (int)_file.Length);
            _file.Close();
            _file.Dispose();

            string data2Path = _vissimNetworkPath + "\\Vissim_data2.txt";
            FileStream _file2 = new FileStream(data2Path, FileMode.Open, FileAccess.Read, FileShare.Read);
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

            string path = _vissimNetworkPath + "\\TransferNetwork";
            //string path2 = app + "SimRoadFile\\TM";
            //string path3 = app + "SimRoadFile\\Paramics";
            if (Directory.Exists(path)==false)//判断是否存在
            {
                Directory.CreateDirectory(path);//创建新路径
                //if (!Directory.Exists(path2))
                //    Directory.CreateDirectory(path2);
                //if (!Directory.Exists(path3))
                //    Directory.CreateDirectory(path3);
            }
            FileStream fs = new FileStream(path + "\\Vissim.inp", FileMode.Create, FileAccess.Write);
            fs.Write(utf8vissim, 0, utf8vissim.Length);
            fs.Close();
            MessageBox.Show(path + "\\Vissim.inp");
        }

        private static string SearchOverNode(IFeatureClass _pFeaClsArc, int fromArcID, int toArcID, int fnumlane, int tnumlane, int flanepos, int tlanepos)
        {
            //此函数用于生成连接器的中间点
            try
            {
                string OverNode = "";

                IPoint pPoint1, pPoint2;
                double pPoint3X, pPoint3Y, pPoint3Z, pPoint4X, pPoint4Y, pPoint4Z, pPoint5X, pPoint5Y, pPoint5Z;
                IPolyline line;
                double Measure;
                IFeature pFeatQuery;
                List<IPoint> fPoint = new List<IPoint>();
                List<IPoint> tPoint = new List<IPoint>();

                string Str = "select OBJECTID from Arc where ArcID=" + fromArcID;
                OleDbCommand Com_1 = new OleDbCommand(Str, _conn);
                int FOBID = Convert.ToInt32(Com_1.ExecuteScalar());
                Str = "select OBJECTID from Arc where ArcID=" + toArcID;
                Com_1 = new OleDbCommand(Str, _conn);
                int TOBID = Convert.ToInt32(Com_1.ExecuteScalar());
                Str = "select Shape_Length from Arc where ArcID=" + fromArcID;
                Com_1 = new OleDbCommand(Str, _conn);
                double len = Convert.ToDouble(Com_1.ExecuteScalar());
                Str = "select Lanewidth from Vissim_LINKS where ArcID=" + fromArcID;
                Com_1 = new OleDbCommand(Str, _conn);
                double lanwidth = Convert.ToDouble(Com_1.ExecuteScalar());

                pFeatQuery = _pFeaClsArc.GetFeature(FOBID);
                line = pFeatQuery.Shape as IPolyline;       //获取对应的中心线（首）
                Measure = len - 0.100;
                fPoint = SearchPoint(line, fnumlane, lanwidth, Measure);

                pFeatQuery = _pFeaClsArc.GetFeature(TOBID);
                line = pFeatQuery.Shape as IPolyline;       //获取对应的中心线（尾）
                Measure = 0.100;
                tPoint = SearchPoint(line, tnumlane, lanwidth, Measure);

                pPoint1 = fPoint[flanepos] as IPoint;
                pPoint2 = tPoint[tlanepos] as IPoint;

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
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return "";
            }
            
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
