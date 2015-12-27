﻿using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.DataModel.RoadSign;
using System;
using System.Data.OleDb;

namespace RoadNetworkSystem.NetworkElement.RoadSignElement
{
    class KerbService
    {


        public IFeatureClass FeaClsKerb;
        public int KerbID;
        public KerbService(IFeatureClass pFeaClsKerb, int kerbID)
        {
            FeaClsKerb = pFeaClsKerb;
            KerbID = kerbID;
        }

        public Kerb GetEntity(IFeature pFeature)
        {
            Kerb bounEty = new Kerb();
            if (pFeature != null)
            {
                if (FeaClsKerb.FindField(Kerb.KerbIDNm) > 0)
                    bounEty.KerbID = Convert.ToInt32(pFeature.get_Value(FeaClsKerb.FindField(Kerb.KerbIDNm)));

                if (FeaClsKerb.FindField(Kerb.ArcIDNm) > 0)
                    bounEty.ArcID = Convert.ToInt32(pFeature.get_Value(FeaClsKerb.FindField(Kerb.ArcIDNm)));
                if (FeaClsKerb.FindField(Kerb.SerialNm) > 0)
                    bounEty.Serial = Convert.ToInt32(pFeature.get_Value(FeaClsKerb.FindField(Kerb.SerialNm)));

                if (FeaClsKerb.FindField(Kerb.OtherNm) > 0)
                    bounEty.Other = Convert.ToInt32(pFeature.get_Value(FeaClsKerb.FindField(Kerb.OtherNm)));
            }
            return bounEty;
        }

        public IFeature CreateKerb(Kerb kerbEty, IPoint pnt)
        {
            IFeature kerbFeature = FeaClsKerb.CreateFeature();
            int bounID = 0;

            if (kerbEty.KerbID > 0)
            {
                bounID = kerbEty.KerbID;
                if (FeaClsKerb.FindField(Kerb.KerbIDNm) >= 0)
                    kerbFeature.set_Value(FeaClsKerb.FindField(Kerb.KerbIDNm), kerbEty.KerbID);
            }
            else
            {

                if (FeaClsKerb.FindField(Kerb.KerbIDNm) >= 0)
                    kerbFeature.set_Value(FeaClsKerb.FindField(Kerb.KerbIDNm), kerbFeature.OID);
                bounID = kerbFeature.OID;
            }

            if (FeaClsKerb.FindField(Kerb.ArcIDNm) >= 0)
                kerbFeature.set_Value(FeaClsKerb.FindField(Kerb.ArcIDNm), kerbEty.ArcID);


            if (FeaClsKerb.FindField(Kerb.SerialNm) >= 0)
                kerbFeature.set_Value(FeaClsKerb.FindField(Kerb.SerialNm), kerbEty.Serial);



            if (FeaClsKerb.FindField(Kerb.OtherNm) >= 0)
                kerbFeature.set_Value(FeaClsKerb.FindField(Kerb.OtherNm), kerbEty.Other);

            kerbFeature.Shape = pnt;
            kerbFeature.Store();
            return kerbFeature;
        }


        
        public IFeature GetFeature()
        {
            IQueryFilter queryFilter = new QueryFilterClass();
            queryFilter.WhereClause = String.Format("{0}={1}", Kerb.KerbIDNm, KerbID);
            IFeatureCursor cursor = FeaClsKerb.Search(queryFilter, false);
            IFeature pFeature = cursor.NextFeature();
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
            if (pFeature != null)
            {
                return pFeature;
            }
            else
            {
                return null;
            }
            
        }

        public IFeature GetKerbByArcAndSerial(int arcID, int serial)
        {
            IWorkspace pWs = FeaClsKerb.FeatureDataset.Workspace;
            string mdbPath = pWs.PathName;

            OleDbConnection Conn = new OleDbConnection();
            string conn_str = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + mdbPath + "; Persist Security Info=False";
            Conn = new OleDbConnection(conn_str);
            Conn.Open();
            string readStr = String.Format("select * from {0} where {1} = {2} and {3} = {4}", Kerb.KerbName, Kerb.ArcIDNm, arcID, Kerb.SerialNm, serial);


            OleDbCommand readCmd = new OleDbCommand();
            readCmd.CommandText = readStr;
            readCmd.Connection = Conn;
            OleDbDataReader read;
            read = readCmd.ExecuteReader();
            IFeature featureKerb = null;
            while (read.Read())
            {
                KerbID = Convert.ToInt32(read[Kerb.KerbIDNm]);
                break;
            }
            read.Close();
            Conn.Close();
            Conn.Dispose();
            featureKerb = GetFeature();
            return featureKerb;
        }


    }
}
