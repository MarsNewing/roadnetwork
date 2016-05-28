using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.DataModel.RoadSign;
using RoadNetworkSystem.ElementService.LaneBasedNetwork.NetworkBuilder;
using RoadNetworkSystem.NetworkEditor;
using RoadNetworkSystem.NetworkEditor.EditorFlow;
using System;
using System.Collections.Generic;

namespace RoadNetworkSystem.NetworkElement.RoadSignElement
{
    class SurfaceService
    {

        public IFeatureClass FeaClsSurface;
        public int SurfaceID;
        public SurfaceService(IFeatureClass pFeaClsSurface, int surfaceID)
        {
            FeaClsSurface = pFeaClsSurface;
            SurfaceID = surfaceID;
        }

        /// <summary>
        /// 删除Arc对应的Surface
        /// </summary>
        /// <param name="arcId"></param>
        public void DeleteSurfaceInArc(int arcId)
        {
            IFeatureCursor curseorSurface;
            IQueryFilter filterSurface = new QueryFilterClass();
            filterSurface.WhereClause = Surface.ArcIDNm + " = " + arcId;
            curseorSurface = FeaClsSurface.Search(filterSurface, false);
            IFeature pFeaSurface = curseorSurface.NextFeature();
            while (pFeaSurface != null)
            {
                pFeaSurface.Delete();
                pFeaSurface = curseorSurface.NextFeature();
            }
        }

        public Surface GetEntity(IFeature pFeature)
        {
            Surface surfaceEty = new Surface();
            if (pFeature != null)
            {
                if (FeaClsSurface.FindField(Surface.SurfaceIDNm) > 0)
                    surfaceEty.SurfaceID = Convert.ToInt32(pFeature.get_Value(FeaClsSurface.FindField(Surface.SurfaceIDNm)));

                if (FeaClsSurface.FindField(Surface.ArcIDNm) > 0)
                    surfaceEty.ArcID = Convert.ToInt32(pFeature.get_Value(FeaClsSurface.FindField(Surface.ArcIDNm)));
                if (FeaClsSurface.FindField(Surface.ControlIDsNm) > 0)
                    surfaceEty.ControlIDs = Convert.ToString(pFeature.get_Value(FeaClsSurface.FindField(Surface.ControlIDsNm)));

                if (FeaClsSurface.FindField(Surface.OtherNm) > 0)
                    surfaceEty.Other = Convert.ToInt32(pFeature.get_Value(FeaClsSurface.FindField(Surface.OtherNm)));
            }
            return surfaceEty;
        }

        public void CrateSurfaceShape(IFeatureClass pFeaClsKerb, NextNodeCutInfor nextArcCutInfor, IFeature nextNodeFea, IFeature preNodeFea, 
            ref IPolygon gon,ref string ctrlPntStrs)
        {
            
            Dictionary<int, IPoint> ctrlPnts = new Dictionary<int, IPoint>();
            Dictionary<int, int> ctrlIndexs = new Dictionary<int, int>();

            KerbService kerb=new KerbService(pFeaClsKerb,0);
            for (int i = 0; i < 4; i++)
            {
                IFeature pFeature = kerb.GetKerbByArcAndSerial(nextArcCutInfor.curArcID, i);
                if (pFeature == null)
                {
                    continue;
                }
                ctrlPnts.Add(i, pFeature.Shape as IPoint);
                if (i < 2)
                {
                    ctrlIndexs.Add(i, Convert.ToInt32(pFeature.get_Value(pFeaClsKerb.FindField(Kerb.KerbIDNm))));
                }
                else
                {
                    ctrlIndexs.Add(i + 1, Convert.ToInt32(pFeature.get_Value(pFeaClsKerb.FindField(Kerb.KerbIDNm))));
                }

            }

            if (ctrlPnts.Count == 0)
            {
                gon = null;
                ctrlPntStrs = null;
                return;
            }

            IPointCollection pntClt = new PolygonClass();
            pntClt.AddPoint(ctrlPnts[0]);
            pntClt.AddPoint(ctrlPnts[1]);
            pntClt.AddPoint(preNodeFea.Shape as IPoint);
            pntClt.AddPoint(ctrlPnts[2]);
            pntClt.AddPoint(ctrlPnts[3]);
            pntClt.AddPoint(nextNodeFea.Shape as IPoint);
            

            int preNodeIndex=Convert.ToInt32(preNodeFea.get_Value(preNodeFea.Fields.FindField("NodeID")));
            int nextNodeIndex=Convert.ToInt32(nextNodeFea.get_Value(preNodeFea.Fields.FindField("NodeID")));

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


        public IFeature CreateSurface(Surface surfaceEty, IPolygon gon)
        {
            IFeature surFeature = FeaClsSurface.CreateFeature();

            if (surfaceEty.SurfaceID > 0)
            {
                if (FeaClsSurface.FindField(Surface.SurfaceIDNm) >= 0)
                    surFeature.set_Value(FeaClsSurface.FindField(Surface.SurfaceIDNm), surfaceEty.SurfaceID);
            }
            else
            {
                if (FeaClsSurface.FindField(Surface.SurfaceIDNm) >= 0)
                    surFeature.set_Value(FeaClsSurface.FindField(Surface.SurfaceIDNm), surFeature.OID);
            }

            if (FeaClsSurface.FindField(Surface.ControlIDsNm) >= 0)
                surFeature.set_Value(FeaClsSurface.FindField(Surface.ControlIDsNm), surfaceEty.ControlIDs);


          
            if (FeaClsSurface.FindField(Surface.ArcIDNm) >= 0)
                surFeature.set_Value(FeaClsSurface.FindField(Surface.ArcIDNm), surfaceEty.ArcID);


            if (FeaClsSurface.FindField(Surface.OtherNm) >= 0)
                surFeature.set_Value(FeaClsSurface.FindField(Surface.OtherNm), surfaceEty.Other);

            surFeature.Shape = gon;
            surFeature.Store();
            return surFeature;
        }
    }
}
