using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.DataModel.RoadSign;
using System;
using System.Collections.Generic;

namespace RoadNetworkSystem.NetworkElement.RoadSignElement
{
    class Surface
    {
        public const string SurfaceIDNm = "SurfaceID";
        public const string ArcIDNm = "ArcID";
        public const string ControlIDsNm = "ControlIDs";
        public const string OtherNm = "Other";

        public IFeatureClass FeaClsSurface;
        public int SurfaceID;
        public Surface(IFeatureClass pFeaClsSurface, int surfaceID)
        {
            FeaClsSurface = pFeaClsSurface;
            SurfaceID = surfaceID;
        }

        public SurfaceEntity GetEntity(IFeature pFeature)
        {
            SurfaceEntity surfaceEty = new SurfaceEntity();
            if (pFeature != null)
            {
                if (FeaClsSurface.FindField(SurfaceIDNm) > 0)
                    surfaceEty.SurfaceID = Convert.ToInt32(pFeature.get_Value(FeaClsSurface.FindField(SurfaceIDNm)));

                if (FeaClsSurface.FindField(ArcIDNm) > 0)
                    surfaceEty.ArcID = Convert.ToInt32(pFeature.get_Value(FeaClsSurface.FindField(ArcIDNm)));
                if (FeaClsSurface.FindField(ControlIDsNm) > 0)
                    surfaceEty.ControlIDs = Convert.ToString(pFeature.get_Value(FeaClsSurface.FindField(ControlIDsNm)));

                if (FeaClsSurface.FindField(OtherNm) > 0)
                    surfaceEty.Other = Convert.ToInt32(pFeature.get_Value(FeaClsSurface.FindField(OtherNm)));
            }
            return surfaceEty;
        }

        public void CrateSurfaceShape(IFeatureClass pFeaClsKerb, RoadNetworkSystem.NetworkEditor.EditorFlow.SegmentConstructor.NextNodeCutInfor nextArcCutInfor, IFeature nextFea, IFeature preFea, 
            ref IPolygon gon,ref string ctrlPntStrs)
        {
            
            Dictionary<int, IPoint> ctrlPnts = new Dictionary<int, IPoint>();
            Dictionary<int, int> ctrlIndexs = new Dictionary<int, int>();

            Kerb kerb=new Kerb(pFeaClsKerb,0);
            for (int i = 0; i < 4; i++)
            {
                IFeature pFeature = kerb.GetKerbByArcAndSerial(nextArcCutInfor.curArcID, i);
                ctrlPnts.Add(i, pFeature.Shape as IPoint);
                if (i < 2)
                {
                    ctrlIndexs.Add(i, Convert.ToInt32(pFeature.get_Value(pFeaClsKerb.FindField(Kerb.KerbIDNm))));
                }
                else
                {
                    ctrlIndexs.Add(i+1, Convert.ToInt32(pFeature.get_Value(pFeaClsKerb.FindField(Kerb.KerbIDNm))));
                }

            }

            IPointCollection pntClt = new PolygonClass();
            pntClt.AddPoint(ctrlPnts[0]);
            pntClt.AddPoint(ctrlPnts[1]);
            pntClt.AddPoint(preFea.Shape as IPoint);
            pntClt.AddPoint(ctrlPnts[2]);
            pntClt.AddPoint(ctrlPnts[3]);
            pntClt.AddPoint(nextFea.Shape as IPoint);
            

            int preNodeIndex=Convert.ToInt32(preFea.get_Value(preFea.Fields.FindField("NodeID")));
            int nextNodeIndex=Convert.ToInt32(nextFea.get_Value(preFea.Fields.FindField("NodeID")));

            ctrlIndexs.Add(2, preNodeIndex);
            ctrlIndexs.Add(5, nextNodeIndex);

            //下游Arc实体存在
            if (nextArcCutInfor.nextArcEty != null)
            {
                IFeature nextArc1Kerb = kerb.GetKerbByArcAndSerial(nextArcCutInfor.nextArcEty.ArcID, 1);
                if (nextArc1Kerb != null)
                {
                    pntClt.AddPoint(nextArc1Kerb.Shape as IPoint);
                    ctrlIndexs.Add(6, Convert.ToInt32(nextArc1Kerb.get_Value(pFeaClsKerb.FindField(Kerb.KerbIDNm))));
                }
            }

            //2015-01-08补充，由于形成的polygon非闭环的，所以需要加入第一个点。
            pntClt.AddPoint(ctrlPnts[0]);
            gon = pntClt as IPolygon;

            for (int j = 0; j < ctrlIndexs.Count - 1; j++)
            {
                ctrlPntStrs = ctrlPntStrs + ctrlIndexs[j].ToString() + "\\";
            }
            ctrlPntStrs = ctrlPntStrs + ctrlIndexs[ctrlIndexs.Count - 1].ToString();
        }


        public IFeature CreateSurface(SurfaceEntity surfaceEty, IPolygon gon)
        {
            IFeature surFeature = FeaClsSurface.CreateFeature();

            if (surfaceEty.SurfaceID > 0)
            {
                if (FeaClsSurface.FindField(SurfaceIDNm) >= 0)
                    surFeature.set_Value(FeaClsSurface.FindField(SurfaceIDNm), surfaceEty.SurfaceID);
            }
            else
            {
                if (FeaClsSurface.FindField(SurfaceIDNm) >= 0)
                    surFeature.set_Value(FeaClsSurface.FindField(SurfaceIDNm), surFeature.OID);
            }

            if (FeaClsSurface.FindField(ControlIDsNm) >= 0)
                surFeature.set_Value(FeaClsSurface.FindField(ControlIDsNm), surfaceEty.ControlIDs);


          
            if (FeaClsSurface.FindField(ArcIDNm) >= 0)
                surFeature.set_Value(FeaClsSurface.FindField(ArcIDNm), surfaceEty.ArcID);


            if (FeaClsSurface.FindField(OtherNm) >= 0)
                surFeature.set_Value(FeaClsSurface.FindField(OtherNm), surfaceEty.Other);

            surFeature.Shape = gon;
            surFeature.Store();
            return surFeature;
        }
    }
}
