using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.DataModel.Road;
using RoadNetworkSystem.GIS;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace RoadNetworkSystem.NetworkElement.MasterLayer
{
    class LinkMasterService
    {

        #region ++++++++Segment字段名++++++++
        /// <summary>
        /// 规定了数据模型，请不要在其他类中直接读取数据
        /// </summary>
        public string IDNm;
        public string FNodeIDNm;
        public string TNodeIDNm;

        public string RelIDNm;
        public string RoadNameNm;
        public string RoadTypeNm;

        public string RoadLevelNm;
        public string FlowDirNm;
        public string LengthNm;
        public string OtherNm;

        #endregion ++++++++Segment字段名++++++++

        public IFeatureClass FeaClsLinkMaster;
        public int Id;
        public IFeature FeaLink;

        public LinkMasterService(IFeatureClass pFeaClsSegment,int id)
        {
            if (pFeaClsSegment != null)
            {
                FeaClsLinkMaster = pFeaClsSegment;
                Id = id;
                
            }
        }

        public IFeature Create(LinkMaster segEty, IPolyline line)
        {
            IFeature newFea = FeaClsLinkMaster.CreateFeature();
            if (Id > 0)
            {
                if (FeaClsLinkMaster.FindField(IDNm) >= 0)
                    newFea.set_Value(FeaClsLinkMaster.FindField(IDNm), segEty.ID);
            }
            else
            {
                if (FeaClsLinkMaster.FindField(IDNm) >= 0)
                    newFea.set_Value(FeaClsLinkMaster.FindField(IDNm), newFea.OID);
            }

            if (FeaClsLinkMaster.FindField(RoadNameNm) >= 0)
                newFea.set_Value(FeaClsLinkMaster.FindField(RoadNameNm), segEty.RoadName);

            if (FeaClsLinkMaster.FindField(RoadTypeNm) >= 0)
                newFea.set_Value(FeaClsLinkMaster.FindField(RoadTypeNm), segEty.RoadType);

            if (FeaClsLinkMaster.FindField(FlowDirNm) >= 0)
                newFea.set_Value(FeaClsLinkMaster.FindField(FlowDirNm), segEty.FlowDir);
            if (FeaClsLinkMaster.FindField(FNodeIDNm) >= 0)
                newFea.set_Value(FeaClsLinkMaster.FindField(FNodeIDNm), segEty.FNodeID);
            if (FeaClsLinkMaster.FindField(TNodeIDNm) >= 0)
                newFea.set_Value(FeaClsLinkMaster.FindField(TNodeIDNm), segEty.TNodeID);

            if (FeaClsLinkMaster.FindField(RelIDNm) >= 0)
                newFea.set_Value(FeaClsLinkMaster.FindField(RelIDNm), segEty.RelID);
            if (FeaClsLinkMaster.FindField(OtherNm) >= 0)
                newFea.set_Value(FeaClsLinkMaster.FindField(OtherNm), segEty.Other);

            newFea.Shape = line;

            newFea.Store();
            return newFea;
        }

        public IFeature GetFeature()
        {
            IFeatureCursor cursor;
            IQueryFilter filer = new QueryFilterClass();
            filer.WhereClause = String.Format("{0}={1}", IDNm, Id);
            cursor = FeaClsLinkMaster.Search(filer, false);
            IFeature nodeFeature = cursor.NextFeature();
            if (nodeFeature != null)
            {
                return nodeFeature;
            }
            else
            {
                return null;
            }
        }

        public LinkMaster GetEntity(IFeature feaLink)
        {
            LinkMaster segEty = new LinkMaster();
            try
            {
                if (feaLink.Fields.FindField(IDNm) > 0)
                    segEty.ID = Convert.ToInt32(feaLink.get_Value(feaLink.Fields.FindField(IDNm)));

                if (feaLink.Fields.FindField(RoadNameNm) >= 0)
                    segEty.RoadName = Convert.ToString(feaLink.get_Value(feaLink.Fields.FindField(RoadNameNm)));
                if (feaLink.Fields.FindField(RoadTypeNm) >= 0)
                {
                    object o=feaLink.get_Value(feaLink.Fields.FindField(RoadTypeNm));
                    if (!(o is DBNull))
                    {
                        segEty.RoadType = Convert.ToInt32(feaLink.get_Value(feaLink.Fields.FindField(RoadTypeNm)));
                    }
                    else
                    {
                        segEty.RoadType = -1;
                    }
                }

                if (feaLink.Fields.FindField(FNodeIDNm) >= 0)
                {
                    object o = feaLink.get_Value(feaLink.Fields.FindField(FNodeIDNm));
                    if (o is DBNull)
                    {
                        segEty.FNodeID = -1;
                        
                    }
                    else
                    {
                        segEty.FNodeID = Convert.ToInt32(o);
                    }
                }

                if (feaLink.Fields.FindField(TNodeIDNm) >= 0)
                    segEty.TNodeID = Convert.ToInt32(feaLink.get_Value(feaLink.Fields.FindField(TNodeIDNm)));
                if (feaLink.Fields.FindField(FlowDirNm) >= 0)
                    segEty.FlowDir = Convert.ToInt32(feaLink.get_Value(feaLink.Fields.FindField(FlowDirNm)));
                if (feaLink.Fields.FindField(RelIDNm) >= 0)
                    segEty.RelID = Convert.ToInt32(feaLink.get_Value(feaLink.Fields.FindField(RelIDNm)));

                if (feaLink.Fields.FindField(OtherNm) >= 0)
                {
                    {
                        object o = feaLink.get_Value(feaLink.Fields.FindField(OtherNm));
                        if (o is DBNull)
                        {
                            segEty.Other = -1;
                        }
                        else
                        {
                            segEty.Other = Convert.ToInt32(o);
                            
                        }
                    }
                }

                    

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return segEty;
        }



    }
}
