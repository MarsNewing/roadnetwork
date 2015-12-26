using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using System.Data.OleDb;
using System.Collections;
using System.IO;
using System.Xml;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.Road;
using RoadNetworkSystem.ADO.Access;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LinkLayer;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LaneLayer;

namespace RoadNetworkSystem.TransmodelerDataTransform
{
    class Basic2TransModeler
    {
        /// <summary>
        /// 此类用于生成Transmodeler路网数据并按指定格式输出
        /// </summary>
        /// 创建时间   ：2012-5-22
        /// 创建人     ：饶明雷
        /// 

        //private const string VissimCoorsystem = "Xian_1980_3_Degree_GK_Zone_38"; //Vissim路网坐标系
        //private const string TMCoorsystem = "GCS_WGS_1984"; //TM路网坐标系
        //private const string ParamicsCoorsystem = "Xian_1980_3_Degree_GK_Zone_38"; //Paramics路网坐标系
        private const double LANEWIDTH = 3.5; //每车道宽度，用于确定Arc的偏移距离，可根据需要调整
        private static OleDbConnection Conn;
        

        public static Boolean CreateTransmodelerdata(string apppath, string databasePath)
        {
            //本函数根据基础路网数据生成Transmodeler仿真数据文件

            FileStream fileStream = new FileStream(apppath + "\\simulation database.smp", FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] utf8data1 = new byte[(int)fileStream.Length]; //_memfile.GetBuffer();
            //从_file当前流段的位置,一直到结尾.注意: 是从0开始的.而不流当前的位置,否则报错.   
            fileStream.Read(utf8data1, 0, (int)fileStream.Length);
            fileStream.Close();
            fileStream.Dispose();


            string path = apppath + "\\TransferNetwork";
            //string path2 = app + "SimRoadFile\\TM";
            //string path3 = app + "SimRoadFile\\Paramics";
            if (Directory.Exists(path) == false)//判断是否存在
            {
                Directory.CreateDirectory(path);//创建新路径
                //if (!Directory.Exists(path2))
                //    Directory.CreateDirectory(path2);
                //if (!Directory.Exists(path3))
                //    Directory.CreateDirectory(path3);
            }
            FileStream fs = new FileStream(path + "\\Vissim.inp", FileMode.Create, FileAccess.Write);
            fs.Write(utf8data1, 0, utf8data1.Length);
            fs.Close();

            //初始化工作
            IWorkspaceFactory pWSF = new AccessWorkspaceFactoryClass();          //定义IWorkspaceFactory型变量pWSF并初始化
            IWorkspace ws = pWSF.OpenFromFile(databasePath, 0);                      //定义IWorkspace型变量pWS并初始化，从pWSF打开文件赋给pWS

            
            Conn = AccessHelper.OpenConnection(databasePath);
            

            try
            {
                string strInsert = "DELETE  *   FROM   Transmodeler_Nodes ";
                OleDbCommand inst = new OleDbCommand(strInsert, Conn);
                inst.ExecuteNonQuery();
            }
            catch
            {
                string strCreatTable = "CREATE TABLE Transmodeler_Nodes(ID LONG, [Longitude] LONG, [Latitude] LONG, [Approach Links] LONG, [Departure Links] LONG, [External] TEXT(50), [Fidelity] TEXT(50), [Control Type] TEXT(50), [Signalized Delay] TEXT(50), [Unsignalized Delay] TEXT(50))";
                OleDbCommand inst = new OleDbCommand(strCreatTable, Conn);
                inst.ExecuteNonQuery();
            }
            try
            {
                string strInsert = "DELETE  *   FROM   Transmodeler_Lines ";
                OleDbCommand inst = new OleDbCommand(strInsert, Conn);
                inst.ExecuteNonQuery();
            }
            catch
            {
                string strCreatTable = "CREATE TABLE Transmodeler_Lines(ID LONG, Type TEXT(50), ANode LONG, BNode LONG, Points MEMO)";
                OleDbCommand inst = new OleDbCommand(strCreatTable, Conn);
                inst.ExecuteNonQuery();
            }
            try
            {
                string strInsert = "DELETE  *   FROM   Transmodeler_Links ";
                OleDbCommand inst = new OleDbCommand(strInsert, Conn);
                inst.ExecuteNonQuery();
            }
            catch
            {
                string strCreatTable = "CREATE TABLE Transmodeler_Links(ID LONG, Dir LONG, AB TEXT(50), BA TEXT(50), ANode LONG, BNode LONG, Superlink LONG, Length LONG, Segments LONG, Type TEXT(50), Priority LONG, Access_AB TEXT(50), Access_BA TEXT(50), Control_AB TEXT(50), Control_BA TEXT(50), Name TEXT(50), Class TEXT(50), Disabled_AB TEXT(50), Disabled_BA TEXT(50), Queue_AB LONG, Queue_BA LONG)";
                OleDbCommand inst = new OleDbCommand(strCreatTable, Conn);
                inst.ExecuteNonQuery();
            }
            try
            {
                string strInsert = "DELETE  *   FROM   Transmodeler_Lanes ";
                OleDbCommand inst = new OleDbCommand(strInsert, Conn);
                inst.ExecuteNonQuery();
            }
            catch
            {
                string strCreatTable = "CREATE TABLE Transmodeler_Lanes(ID LONG, Segment LONG, Dir LONG, [Position] LONG, Side TEXT(50), Turns TEXT(50), Auxiliary TEXT(50), Merged TEXT(50), Merging TEXT(50), Exit TEXT(50), Dropped TEXT(50), Parking TEXT(50), Width DOUBLE, Shoulder TEXT(50), [Change] TEXT(50), Barrier TEXT(50), ETC TEXT(50), HOV TEXT(50), Transit TEXT(50), Truck TEXT(50), [User A] TEXT(50), [User B] TEXT(50), HOT TEXT(50), Density LONG, Speed LONG, ReLaneID LONG)";
                OleDbCommand inst = new OleDbCommand(strCreatTable, Conn);
                inst.ExecuteNonQuery();
            }
            try
            {
                string strInsert = "DELETE  *   FROM   Transmodeler_LC ";
                OleDbCommand inst = new OleDbCommand(strInsert, Conn);
                inst.ExecuteNonQuery();
            }
            catch
            {
                string strCreatTable = "CREATE TABLE Transmodeler_LC(ID COUNTER, UL LONG, DL LONG, Dir TEXT(50), Length LONG, Connectivity LONG)";
                OleDbCommand inst = new OleDbCommand(strCreatTable, Conn);
                inst.ExecuteNonQuery();
            }
            try
            {
                string strInsert = "DELETE  *   FROM   Transmodeler_Segments ";
                OleDbCommand inst = new OleDbCommand(strInsert, Conn);
                inst.ExecuteNonQuery();
            }
            catch
            {
                string strCreatTable = "CREATE TABLE Transmodeler_Segments(ID LONG, Dir TEXT(50), AB TEXT(50), BA TEXT(50), Link LONG, [Position] LONG, Length LONG, Lanes_AB LONG, Lanes_BA LONG, [Fidelity] TEXT(50), [Tunnel] TEXT(50), [Grade] DOUBLE, Parking_AB TEXT(50), Parking_BA TEXT(50), Density_AB LONG, Density_BA LONG, Speed_AB LONG, Speed_BA LONG, [K/Kjam_AB] LONG, [K/Kjam_BA] LONG)";
                OleDbCommand inst = new OleDbCommand(strCreatTable, Conn);
                inst.ExecuteNonQuery();
            }


            IFeatureWorkspace feaWs = ws as IFeatureWorkspace;


            //定义Link,Node的featureclass
            IFeatureClass pFeatClsLink = feaWs.OpenFeatureClass(Link.LinkName);

            IFeatureClass pFeatClsNode = feaWs.OpenFeatureClass(Node.NodeName);


            try
            {

                //1.获取当前坐标系，转换为TM坐标系
                //NewpFeatClsNode = SetcoorsystemPoint(pFeatClsNode, pFeatClsLink, pFLayer1, pFLayer2);
                //NewpFeatClsLink = SetcoorsystemLine(pFeatClsNode, pFeatClsLink, pFLayer1, pFLayer2);
                //2.根据已有数据生成TM数据表
                CreateTMData(pFeatClsNode, pFeatClsLink);

                //3.根据数据表生成路网文件
                btnCreateFromMDB(apppath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            return true;
        }
        //1.获取当前坐标系，转换为TM坐标系，点
        //private static IPoint SetcoorsystemPoint(IPoint pNode)
        //{
        //    ISpatialReference spoint;
        //    string Oldnn = "";
        //    int Oldss;
        //    IQueryFilter pFilter = new QueryFilterClass();
        //    spoint = pNode.SpatialReference;
        //    Oldnn = spoint.Name;
        //    Oldss = spoint.FactoryCode;

        //    ISpatialReferenceFactory pfactory = new SpatialReferenceEnvironmentClass();
        //    //广州处于东经113，遂将投影坐标系（大地坐标系）设定为西安1980三度带Xian_1980_3_Degree_GK_Zone_38
        //    IProjectedCoordinateSystem flatref = pfactory.CreateProjectedCoordinateSystem(Oldss);
        //    //因为现有的广州地图的地理坐标系（GCS）为WGS1984，所以将地理坐标系设为esriSRGeoCS_WGS1984)
        //    IGeographicCoordinateSystem earthref = pfactory.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);

        //    double NodeX = pNode.X;
        //    double NodeY = pNode.Y;

        //    IPoint pt = new PointClass();
        //    pt.PutCoords(NodeX, NodeY);
        //    IGeometry geo = (IGeometry)pt;
        //    geo.SpatialReference = flatref;
        //    geo.Project(earthref);
        //    double NodeX1 = pt.X;
        //    double NodeY1 = pt.Y;

        //    return pt;
        //}
        
        ////1.获取当前坐标系，转换为TM坐标系，线
        //private static IPolyline SetcoorsystemLine(IPolyline pLink)
        //{
        //    ISpatialReference spline;
        //    string Oldnn = "";
        //    int Oldss;
        //    IQueryFilter pFilter = new QueryFilterClass();
        //    spline = pLink.SpatialReference;
        //    Oldnn = spline.Name;
        //    Oldss = spline.FactoryCode;

        //    ISpatialReferenceFactory pfactory = new SpatialReferenceEnvironmentClass();
        //    //广州处于东经113，遂将投影坐标系（大地坐标系）设定为西安1980三度带Xian_1980_3_Degree_GK_Zone_38
        //    IProjectedCoordinateSystem flatref = pfactory.CreateProjectedCoordinateSystem(Oldss);
        //    //因为现有的广州地图的地理坐标系（GCS）为WGS1984，所以将地理坐标系设为esriSRGeoCS_WGS1984)
        //    IGeographicCoordinateSystem earthref = pfactory.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);

        //    IPolyline pl = new PolylineClass();
        //    IGeometry geo = (IGeometry)pl;
        //    geo.SpatialReference = flatref;
        //    geo.Project(earthref);

        //    double len = pl.Length;
        //    return pl;
        //}

        ////根据端点ALen、BLen截取link，生成截断后的线段返回
        //private static IPolyline CutLine(IPolyline link, double ALen, double BLen)
        //{
        //    //根据端点ALen、BLen截取link，生成截断后的线段返回
        //    //注意，这里ALen,  BLen的单位为米

        //    double linkLen = link.Length;

        //    if (ALen > linkLen || BLen > linkLen || Math.Abs(ALen - BLen) < 0.1)
        //        return null;

        //    //声明newPartIndex1、newPartIndex2、newSegmentIndex1、newSegmentIndex2为整型变量
        //    int newPartIndex1;
        //    int newPartIndex2;
        //    int newSegmentIndex1;
        //    int newSegmentIndex2;
        //    //声明SplitHappened为布尔型变量
        //    bool SplitHappened;
        //    //定义缺省变量
        //    object o = System.Type.Missing;

        //    //使得ALen<BLen
        //    if (ALen > BLen)
        //    {
        //        double tmp = ALen;
        //        ALen = BLen;
        //        BLen = tmp;
        //    }

        //    //获取切断首尾断弧段
        //    newSegmentIndex1 = 0;       //若为起点则不加结点
        //    if (Math.Abs(ALen) > 0.1)
        //        link.SplitAtDistance(ALen, false, false, out SplitHappened, out newPartIndex1, out newSegmentIndex1);

        //    newSegmentIndex2 = -1;
        //    if (Math.Abs(BLen - link.Length) > 0.1)       //若为端点则不加结点
        //        link.SplitAtDistance(BLen, false, false, out SplitHappened, out newPartIndex2, out newSegmentIndex2);

        //    IPointCollection pPtCol = link as IPointCollection;

        //    int i;
        //    //要先截取尾部，再截取首部，否则会出错
        //    //若不为终点则截取
        //    if (newSegmentIndex2 != -1)
        //        for (i = pPtCol.PointCount - 1; i > newSegmentIndex2; i--)
        //        {
        //            pPtCol.RemovePoints(i, 1);
        //        }

        //    for (i = 0; i <= newSegmentIndex1 - 1; i++)
        //    {
        //        pPtCol.RemovePoints(0, 1);
        //    }

        //    IPolyline line = pPtCol as IPolyline;
        //    return line;
        //}

        private static void CreateTMData(IFeatureClass pFeatClsNode, IFeatureClass pFeatClsLink)
        {
            //此段生成Transmodeler_Nodes的相关数据，存储于Transmodeler_Nodes表中
            ArrayList listLinkDataF = new ArrayList();
            ArrayList listLinkDataT = new ArrayList();
            IQueryFilter pFilter = new QueryFilterClass();
            IFeature pFeatLink;
            IFeatureCursor fCursorLink;
            IFeature pFeatPoint;
            IFeatureCursor fCursorPoint = pFeatClsNode.Search(pFilter, false);

            int lFldNodeID = pFeatClsNode.FindField("NodeID");
            int lFld;
            int NodeID, linkid;
            double NodeX, NodeY;
            pFeatPoint = fCursorPoint.NextFeature();
            while (pFeatPoint != null)
            {
                //每次循环都加把所有以NodeID为端点的弧段加入listLinkData
                listLinkDataF.Clear();
                listLinkDataT.Clear();
                //1。先记录当前结点信息，包括ID，经纬度坐标X,Y
                NodeID = Convert.ToInt32(pFeatPoint.get_Value(lFldNodeID));
                IPoint tmpPoint = pFeatPoint.Shape as IPoint;
                NodeX = tmpPoint.X;
                NodeY = tmpPoint.Y;

                // 坐标系转换
                //IPoint Newpt = SetcoorsystemPoint(tmpPoint);
                NodeX = tmpPoint.X;
                NodeY = tmpPoint.Y;

                //.2。找出所有以当前Node为起点的Link，把LinkID存储入listLinkData
                pFilter.WhereClause = "FNodeID=" + NodeID;
                fCursorLink = pFeatClsLink.Search(pFilter, false);
                pFeatLink = fCursorLink.NextFeature();
                while (pFeatLink != null)
                {
                    lFld = fCursorLink.FindField("LinkID");
                    linkid = Convert.ToInt32(pFeatLink.get_Value(lFld)); //记录下对应的LinkID
                    listLinkDataF.Add(linkid);                                     //存储入listLinkData中
                    pFeatLink = fCursorLink.NextFeature();
                }
                //.3。找出所有以当前Node为终点的Link，把LinkID存储入listLinkData
                pFilter.WhereClause = "TNodeID=" + NodeID;
                fCursorLink = pFeatClsLink.Search(pFilter, false);
                pFeatLink = fCursorLink.NextFeature();
                while (pFeatLink != null)
                {
                    lFld = fCursorLink.FindField("LinkID");
                    linkid = Convert.ToInt32(pFeatLink.get_Value(lFld)); //记录下对应的LinkID
                    listLinkDataT.Add(linkid);                                     //存储入listLinkData中
                    pFeatLink = fCursorLink.NextFeature();
                }
                long Longitude = 0, Latitude = 0, Applinks = 0, Deplinks = 0;
                string External = "No", Fidelity = "Micro";
                //Longitude = Convert.ToInt32(NodeX * 100000);
                //Latitude = Convert.ToInt32(NodeY * 100000);

                Longitude = Convert.ToInt32(NodeX);
                Latitude = Convert.ToInt32(NodeY);
                Applinks = listLinkDataT.Count;
                Deplinks = listLinkDataF.Count;
                OleDbCommand insertCom = new OleDbCommand();
                insertCom.Connection = Conn;
                string str = "insert into Transmodeler_Nodes(ID,[Longitude],[Latitude],[Approach Links],[Departure Links],[External],[Fidelity]) Values(" + NodeID + "," + Longitude + "," + Latitude + "," + Applinks + "," + Deplinks + ",'" + External + "','" + Fidelity + "')";
                insertCom.CommandText = str;
                insertCom.ExecuteNonQuery();
                pFeatPoint = fCursorPoint.NextFeature();
            }

            //////////
            //此段生成Transmodeler_Lines、Transmodeler_Links、Transmodeler_Segments的相关数据，存储于Transmodeler_Lines、Transmodeler_Links、Transmodeler_Segments表中
            /////////
            IQueryFilter pFilter1 = new QueryFilterClass();
            fCursorLink = pFeatClsLink.Search(pFilter1, false);
            IPoint pPoint1, pPoint2;
            int m = 0, a, b, c, d, f;

            pFeatLink = fCursorLink.NextFeature();

            while (pFeatLink != null)//遍历弧段
            {
                //坐标系转换，不进行线要素坐标转换，转换之后的控制点
                //IPolyline tmpPline = pFeatLink.Shape as IPolyline;
                //IPolyline Newpl = SetcoorsystemLine(tmpPline);

                string OverNode = "";
                long ANode, BNode;
                int dir, len, lanesAB, lanesBA;
                m++;

                LinkService link = new LinkService(pFeatClsLink, 0);
                LinkMaster linkMstrEty = link.GetEntity(pFeatLink);

                Link linkEty = new Link();
                linkEty = linkEty.Copy(linkMstrEty);




                linkid = linkEty.ID;

                len = Convert.ToInt32((pFeatLink.Shape as IPolyline).Length);


                ANode = linkEty.FNodeID;
                
                BNode = linkEty.TNodeID;

                dir = linkEty.FlowDir;
                if (dir == 2)
                    dir = 0;

                string Str_1 = "select LaneNum from " + Arc.ArcFeatureName + " where " + Arc.LinkIDNm + "=" + linkid + " and " + Arc.FlowDirNm + " =1";
                OleDbCommand Com_1 = new OleDbCommand(Str_1, Conn);
                lanesAB = Convert.ToInt32(Com_1.ExecuteScalar());

                Str_1 = "select LaneNum from " + Arc.ArcFeatureName + " where " + Arc.LinkIDNm + "=" + linkid + " and " + Arc.FlowDirNm + " =-1";
                Com_1 = new OleDbCommand(Str_1, Conn);
                lanesBA = Convert.ToInt32(Com_1.ExecuteScalar());

                ////生成Lines数据时，考虑到仿真路网的美观性，首先对Link进行首尾截取，截取距离取30m
                //double ALen = 30.0;
                //double BLen = (double)len-30.0;
                //IPolyline Newlink = CutLine(pFeatLink.Shape as IPolyline, ALen, BLen);

                ////取Link的控制点(截断后)
                //IPointCollection pPointCollection = Newlink as IPointCollection;

                //取Link的控制点
                IPointCollection pPointCollection = pFeatLink.Shape as IPointCollection;

                //取Link的起点，存储到pPoint1中
                pPoint1 = pPointCollection.get_Point(0) as IPoint;
                //对控制点的坐标进行转换
                //pPoint1 = SetcoorsystemPoint(pPoint1);

                OverNode = (long)(pPoint1.X * 1000000) + "," + (long)(pPoint1.Y * 1000000) + "," + 0 + ";";
                for (int k = 1; k < pPointCollection.PointCount; k++)
                {
                    pPoint2 = pPointCollection.get_Point(k) as IPoint;

                    //对控制点的坐标进行转换
                    //pPoint2 = SetcoorsystemPoint(pPoint2);

                    OverNode += ((long)(pPoint2.X * 1000000) - (long)(pPoint1.X * 1000000)) + "," + ((long)(pPoint2.Y * 1000000) - (long)(pPoint1.Y * 1000000)) + "," + 0 + ";";
                }
                OleDbCommand insertCom = new OleDbCommand();
                insertCom.Connection = Conn;
                string str = "insert into Transmodeler_Lines(ID,Type,ANode,BNode,Points) Values(" + linkid + ",'polyline'," + ANode + "," + BNode + ",'" + OverNode + "')";
                insertCom.CommandText = str;
                insertCom.ExecuteNonQuery();

                str = "insert into Transmodeler_Links(ID,Dir,ANode,BNode,Segments,Type,Priority) Values(" + linkid + "," + dir + "," + ANode + "," + BNode + ",1,'street',1)";
                insertCom.CommandText = str;
                insertCom.ExecuteNonQuery();

                str = "insert into Transmodeler_Segments(ID,Dir,Link,[Position],Length,Lanes_AB,Lanes_BA,Fidelity,Tunnel,Parking_AB,Parking_BA) Values(" + m + "," + dir + "," + linkid + ",0," + len + "," + lanesAB + "," + lanesBA + ",'Micro','No','None','None')";
                insertCom.CommandText = str;
                insertCom.ExecuteNonQuery();

                pFeatLink = fCursorLink.NextFeature();
            }

            //////////
            //此段生成Transmodeler_Lanes的相关数据，存储于Transmodeler_Lanes表中
            /////////

            OleDbCommand cmd = new OleDbCommand();
            OleDbDataReader reader;
            string strSql = "select * from Lane order by LaneID";
            cmd.CommandText = strSql;
            cmd.Connection = Conn;
            reader = cmd.ExecuteReader();
            int n = 0, laneid, lanearcid, lanepos, laneseg, lanedir, lanenum;
            string laneside, lanechange;
            while (reader.Read())
            {



                n++;
                laneid = Convert.ToInt32(reader[LaneFeatureService.LaneIDNm]);
                lanearcid = Convert.ToInt32(reader[LaneFeatureService.ArcIDNm]);
                lanepos = Convert.ToInt32(reader[LaneFeatureService.PositionNm]);
                //lanepos = lanepos - 1;钮中铭修改于20150109,定义Position从0开始结束
                lanechange = Convert.ToString(reader[LaneFeatureService.ChangeNm]);
                string Str_1 = "select " + Arc.LinkIDNm + " from " + Arc.ArcFeatureName + " where " + Arc.ArcIDNm + "=" + lanearcid;
                OleDbCommand Com_1 = new OleDbCommand(Str_1, Conn);
                laneseg = Convert.ToInt32(Com_1.ExecuteScalar());
                Str_1 = "select ID from Transmodeler_Segments where Link=" + laneseg;
                Com_1 = new OleDbCommand(Str_1, Conn);
                laneseg = Convert.ToInt32(Com_1.ExecuteScalar());
                Str_1 = "select " + Arc.FlowDirNm + " from " + Arc.ArcFeatureName + " where " + Arc.ArcIDNm + "=" + lanearcid;
                Com_1 = new OleDbCommand(Str_1, Conn);
                lanedir = Convert.ToInt32(Com_1.ExecuteScalar());
                Str_1 = "select " + Arc.LaneNumNm + " from " + Arc.ArcFeatureName + " where " + Arc.ArcIDNm + "=" + lanearcid;
                Com_1 = new OleDbCommand(Str_1, Conn);
                lanenum = Convert.ToInt32(Com_1.ExecuteScalar());
                if (lanepos == 0)
                    laneside = "Left";
                else if (lanepos == (lanenum - 1))
                    laneside = "Right";
                else
                    laneside = "";
                string strSql1 = string.Format("select * from " + LaneConnector.ConnectorName + " where " + LaneConnectorFeatureService.fromLaneIDNm + " = {0:G}", laneid);
                OleDbCommand cmdDB = new OleDbCommand();
                OleDbDataReader readDB;
                cmdDB.CommandText = strSql1;
                cmdDB.Connection = Conn;
                readDB = cmdDB.ExecuteReader();
                string turndir = "";
                ArrayList listlaneturns = new ArrayList();
                if (readDB.HasRows)
                {
                    while (readDB.Read())
                    {
                        turndir = Convert.ToString(readDB[LaneConnectorFeatureService.TurningDirNm]);
                        listlaneturns.Add(turndir);
                    }
                    if ((listlaneturns.Count == 1) && (Convert.ToString(listlaneturns[0]) == "Left"))
                        turndir = "L";
                    else if ((listlaneturns.Count == 1) && (Convert.ToString(listlaneturns[0]) == "Straight"))
                        turndir = "T";
                    else if ((listlaneturns.Count == 1) && (Convert.ToString(listlaneturns[0]) == "Right"))
                        turndir = "R";
                    else if ((listlaneturns.Count == 2) && (!listlaneturns.Contains("Left")))
                        turndir = "TR";
                    else if ((listlaneturns.Count == 2) && (!listlaneturns.Contains("Right")))
                        turndir = "LT";
                    else if ((listlaneturns.Count == 2) && (!listlaneturns.Contains("Straight")))
                        turndir = "LR";
                    else
                        turndir = "LTR";
                }
                else
                    turndir = "";
                readDB.Close();
                cmdDB.Dispose();
                OleDbCommand insertCom = new OleDbCommand();
                insertCom.Connection = Conn;
                string str = "insert into Transmodeler_Lanes(ID,Segment,Dir,[Position],Side,Turns,Merged,Exit,Dropped,Parking,Width,Shoulder,[Change],ReLaneID) Values(" + n + "," + laneseg + "," + lanedir + "," + lanepos + ",'" + laneside + "','" + turndir + "','No','No','No','No'," + LANEWIDTH + ",'No','" + lanechange + "'," + laneid + ")";
                insertCom.CommandText = str;
                insertCom.ExecuteNonQuery();
            }
            reader.Close();
            cmd.Dispose();

            //////////
            //此段生成Transmodeler_LC的相关数据，存储于Transmodeler_LC表中
            /////////

            OleDbCommand cmdLC = new OleDbCommand();
            OleDbDataReader readerLC;
            strSql = "select * from " + LaneConnector.ConnectorName + " order by " + LaneConnectorFeatureService.ConnectorIDNm;
            cmdLC.CommandText = strSql;
            cmdLC.Connection = Conn;
            readerLC = cmdLC.ExecuteReader();
            int lcid, ul, dl;
            string dirlc;
            while (readerLC.Read())
            {
                lcid = Convert.ToInt32(readerLC[LaneConnectorFeatureService.ConnectorIDNm]);
                ul = Convert.ToInt32(readerLC[LaneConnectorFeatureService.fromLaneIDNm]);
                dl = Convert.ToInt32(readerLC[LaneConnectorFeatureService.toLaneIDNm]);
                dirlc = Convert.ToString(readerLC[LaneConnectorFeatureService.TurningDirNm]);

                string Str_2 = "select ID from Transmodeler_Lanes where ReLaneID=" + ul;
                OleDbCommand Com_2 = new OleDbCommand(Str_2, Conn);
                ul = Convert.ToInt32(Com_2.ExecuteScalar());
                Str_2 = "select ID from Transmodeler_Lanes where ReLaneID=" + dl;
                Com_2 = new OleDbCommand(Str_2, Conn);
                dl = Convert.ToInt32(Com_2.ExecuteScalar());

                OleDbCommand insertCom = new OleDbCommand();
                insertCom.Connection = Conn;
                string str = "insert into Transmodeler_LC(ID,UL,DL,Dir) Values(" + lcid + "," + ul + "," + dl + ",'" + dirlc + "')";
                insertCom.CommandText = str;
                insertCom.ExecuteNonQuery();
            }
            readerLC.Close();
            cmdLC.Dispose();
        }

        private static void btnCreateFromMDB(string app)
        {
            try
            {
                string strSQL;

                OleDbCommand cmd = new OleDbCommand();
                OleDbDataReader reader_links;
                OleDbDataReader reader_lines;
                OleDbDataReader reader_nodes;
                //DataTable reader_segments;
                //DataTable reader_lanes;

                //////////////
                //创建路网文件
                //////////////
                string path = app + "SimRoadFile";
                string path1 = app + "SimRoadFile\\TM";
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

                TransmodelerNetworkBuilder builder = new TransmodelerNetworkBuilder("BasicNetwork", path1);
                builder.CreateNetworkXML();

                ////////////////
                //添加道路类型
                ////////////////
                builder.AddClass("0", "Undefind", "0", "9", "0", "100", "1200", "11.00", "16.0", "0.8", "6.0", "1.80,5.00");

                ////////////////
                //添加节点
                ////////////////
                strSQL = "SELECT * FROM Transmodeler_Nodes";
                cmd.CommandText = strSQL;
                cmd.Connection = Conn;
                reader_nodes = cmd.ExecuteReader();

                while (reader_nodes.Read())
                {
                    int node_id = (int)reader_nodes["ID"];

                    string type = reader_nodes["Control Type"].ToString();
                    if (type == "")
                        type = "2";
                    else
                        type = "";
                    builder.AddNode(node_id.ToString(), type);
                }
                reader_nodes.Close();

                ////////////////
                //添加Lines
                ////////////////
                strSQL = "SELECT * FROM Transmodeler_Lines";
                cmd.CommandText = strSQL;
                cmd.Connection = Conn;
                reader_lines = cmd.ExecuteReader();

                while (reader_lines.Read())
                {
                    string line_id = reader_lines["ID"].ToString();
                    string type = (string)reader_lines["Type"];
                    string pts = (string)reader_lines["Points"];

                    builder.AddLine(line_id, type, pts);
                }
                reader_lines.Close();

                ////////////////
                //添加Links
                ////////////////
                strSQL = "SELECT * FROM Transmodeler_Links";
                cmd.CommandText = strSQL;
                cmd.Connection = Conn;
                reader_links = cmd.ExecuteReader();

                while (reader_links.Read())
                {
                    string link_id = reader_links["ID"].ToString();
                    string dir = reader_links["Dir"].ToString();
                    string ups = reader_links["ANode"].ToString();
                    string dns = reader_links["BNode"].ToString();

                    string sl = "";

                    XmlNode link_node = builder.AddLink(link_id, ups, dns, sl);

                    string Str = "select ID from Transmodeler_Segments where Link=" + link_id;
                    OleDbCommand Com = new OleDbCommand(Str, Conn);
                    string segment_id = Convert.ToString(Com.ExecuteScalar());
                    Str = "select Link from Transmodeler_Segments where Link=" + link_id;
                    Com = new OleDbCommand(Str, Conn);
                    string line_belong_to = Convert.ToString(Com.ExecuteScalar());


                    XmlNode segment_node = builder.AddSegment(link_node, segment_id, line_belong_to);

                    //加入车道

                    strSQL = "SELECT * FROM Transmodeler_Lanes WHERE Segment=" + segment_id + " AND Dir=1";
                    OleDbCommand cmdDB = new OleDbCommand();
                    OleDbDataReader readDB;
                    cmdDB.CommandText = strSQL;
                    cmdDB.Connection = Conn;
                    readDB = cmdDB.ExecuteReader();
                    int index;
                    string lane_id, laneid;
                    ArrayList listLanes = new ArrayList();
                    while (readDB.Read())
                    {
                        laneid = Convert.ToString(readDB["ID"]);
                        listLanes.Add(laneid);
                    }
                    readDB.Close();
                    cmdDB.Dispose();
                    //                             
                    Dictionary<string, Connector_Entity> rs = new Dictionary<string, Connector_Entity>();
                    for (index = 1; index <= listLanes.Count; index++)
                    {
                        lane_id = listLanes[index - 1].ToString();
                        rs = get_connections(lane_id);
                        if (listLanes.Count == 1)
                        {
                            builder.AddLane(segment_node, "1", rs);
                        }
                        else if (index == 1)
                        {
                            builder.AddLane(segment_node, "1", rs);
                        }
                        else if (index != listLanes.Count)
                        {
                            builder.AddLane(segment_node, "3", rs);
                        }
                        else
                        {
                            builder.AddLane(segment_node, "2", rs);
                        }
                    }

                    //双向Link
                    if (dir.Trim() == "0")
                    {
                        link_node = builder.AddLink("-" + link_id, dns, ups, sl);
                        segment_node = builder.AddSegment(link_node, "-" + segment_id, line_belong_to);
                        //
                        strSQL = "SELECT * FROM Transmodeler_Lanes WHERE Segment=" + segment_id + " AND Dir=-1";
                        cmdDB.CommandText = strSQL;
                        cmdDB.Connection = Conn;
                        readDB = cmdDB.ExecuteReader();
                        int index2;
                        string lane_id2, laneid2;
                        ArrayList listLanes2 = new ArrayList();
                        while (readDB.Read())
                        {
                            laneid2 = Convert.ToString(readDB["ID"]);
                            listLanes2.Add(laneid2);
                        }
                        readDB.Close();
                        cmdDB.Dispose();
                        //      
                        for (index2 = 1; index2 <= listLanes2.Count; index2++)
                        {
                            //
                            lane_id2 = listLanes2[index2 - 1].ToString();
                            rs = get_connections(lane_id2);
                            if (listLanes2.Count == 1)
                            {
                                builder.AddLane(segment_node, "1", rs);
                            }
                            else if (index2 == 1)
                            {
                                builder.AddLane(segment_node, "1", rs);
                            }
                            else if (index2 != listLanes2.Count)
                            {
                                builder.AddLane(segment_node, "3", rs);
                            }
                            else
                            {
                                builder.AddLane(segment_node, "2", rs);
                            }
                        }
                        //reader_lanes.Clear();
                    }

                }
                reader_links.Close();

                //
                Conn.Close();
                builder.SaveNetworkXML();

                MessageBox.Show("已创建路网文件！" + path1 + "\\test.XML");
            }
            catch (Exception ex)
            {
                //
                MessageBox.Show(ex.Message);
            }
        }

        private static Dictionary<string, Connector_Entity> get_connections(string lane_id)
        {
            //
            Dictionary<string, Connector_Entity> d = new Dictionary<string, Connector_Entity>();

            string strSQL = "SELECT [ID],[Segment],[Position] FROM Transmodeler_Lanes WHERE [ID] in (SELECT [DL] FROM Transmodeler_LC WHERE [UL] =" + lane_id + ")";
            OleDbCommand cmdDB = new OleDbCommand();
            OleDbDataReader readDB;
            cmdDB.CommandText = strSQL;
            cmdDB.Connection = Conn;
            readDB = cmdDB.ExecuteReader();
            int key = 1;
            if (readDB.HasRows)
            {
                while (readDB.Read())
                {
                    Connector_Entity ety = new Connector_Entity();
                    ety.LINK = readDB["Segment"].ToString();
                    ety.LANE_POS = (int.Parse(readDB["Position"].ToString()) + 1).ToString();   //车道位置从左到右算，从1开始

                    d.Add(key.ToString(), ety);
                    key++;
                }
            }
            readDB.Close();
            cmdDB.Dispose();
            return d;
        }
    }
}
