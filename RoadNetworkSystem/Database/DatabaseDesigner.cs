using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.DataModel.GuideSignNetwork;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.RoadSign;
using RoadNetworkSystem.FileDirectory;
using RoadNetworkSystem.GIS.GeoDatabase.Dataset;
using RoadNetworkSystem.NetworkElement.GuideSignNetwork;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LaneLayer;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.LinkLayer;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.RoadSegmentLayer;
using System;
using System.Windows.Forms;

namespace RoadNetworkSystem.NetworkExtraction.LaneBasedNetwork.DatabaseManager
{
    /// <summary>
    /// 创建车道级路网地理数据库
    /// 前提，需要在数据库中新建要素集，并把Road放进去
    /// </summary>
    class DatabaseDesigner
    {

        /// <summary>
        /// 创建空数据库对话框
        /// </summary>
        /// <param name="MdbDircetion"></param>
        /// <param name="MdbName"></param>
        /// <param name="MdbPath"></param>
        /// <returns></returns>
        public static IWorkspace CreateGDGDialoge(ref string MdbDircetion, ref string MdbName, ref string MdbPath)
        {
            try
            {
                string rootPath = Application.StartupPath;
                FileHelper.SaveFile("personal geodatabase files (*.mdb)|*.mdb|All files (*.*)|*.*", ref MdbDircetion, ref MdbName);
                Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.AccessWorkspaceFactory");
                IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);
                IWorkspaceName workspaceName = workspaceFactory.Create(MdbDircetion, MdbName, null, 0);
                MdbPath = MdbDircetion + "\\" + MdbName;
                IWorkspace pWSP = workspaceFactory.OpenFromFile(MdbPath, 0);
                return pWSP;
                ////创建dataset
                //ISpatialReferenceFactory spatialReferenceFactory = new SpatialReferenceEnvironmentClass();
                //ISpatialReference spatialReference = spatialReferenceFactory.CreateProjectedCoordinateSystem((int)esriSRProjCS4Type.esriSRProjCS_Xian1980_3_Degree_GK_Zone_38);
                //DatasetHelper.CreateFeatureDataset(pWSP, "Ming", spatialReference);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 创建GIS-T仿真路网数据库
        /// </summary>
        /// <param name="pWs"></param>
        public static void CreateSimNetworkDb(IWorkspace pWs)
        {
            
            IFeatureDataset feaDS = pWs.get_Datasets(esriDatasetType.esriDTFeatureDataset).Next() as IFeatureDataset;
            #region ------------无要素集，创建----------------

            if (feaDS == null)
            {
                //是否存在Ming要素数据集
                bool flag = DatasetHelper.ExistDataset((pWs as IFeatureWorkspace), "Ming");
                if (flag == false)
                {
                    ISpatialReferenceFactory spatialReferenceFactory = new SpatialReferenceEnvironmentClass();
                    ISpatialReference spatialReference = spatialReferenceFactory.CreateProjectedCoordinateSystem((int)esriSRProjCS4Type.esriSRProjCS_Xian1980_3_Degree_GK_Zone_38);
                    feaDS = DatasetHelper.CreateFeatureDataset(pWs, "Ming", spatialReference);
                }
            }
            #endregion ------------无要素集，创建----------------

            //----------------Setp2 创建Link层-----------------------------
            DatabaseDesigner.CreateLinkClass(feaDS);
            DatabaseDesigner.CreateNodeClass(feaDS);
            DatabaseDesigner.CreateArcClass(feaDS);

            //----------------Setp3 创建Lane层-----------------------------
            DatabaseDesigner.CreateConnectorClass(feaDS);
            DatabaseDesigner.CreateLaneClass(feaDS);
        }

        /// <summary>
        /// 创建指路标志路网数据库
        /// </summary>
        /// <param name="pWs"></param>
        public void CreateGuideSignDb(IWorkspace pWs)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 创建车道级路网数据库
        /// </summary>
        /// <param name="pWs"></param>
        public void CreateBasicNetworkDb(IWorkspace pWs)
        {
            throw new System.NotImplementedException();
        }


        #region --------------创建RoadSegment图层-------------------------

        public static IFeatureClass CreateSegmentNodeClass(IFeatureDataset feaDS)
        {
            
            if (feaDS == null)
            {
                return null;
            }
            else
            {
                IFeatureWorkspace pFeatWorkspace = feaDS.Workspace as IFeatureWorkspace;
                if (DatasetHelper.ExistDataset(pFeatWorkspace, SegmentNodeEntity.RoadSegmentNodeName))
                {
                    return pFeatWorkspace.OpenFeatureClass(SegmentNodeEntity.RoadSegmentNodeName);
                }

                IFields roadSegNodeFields = new FieldsClass();
                IFieldsEdit pFieldsEdit = roadSegNodeFields as IFieldsEdit;

                try
                {
                    SegmentNode rdSegNode = new SegmentNode(null, 0, null);
                    IFieldEdit pFieldEdit = FeatureClassHelper.CreateField(rdSegNode.NodeIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(rdSegNode.CompositeTypeNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(rdSegNode.NodeTypeNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);


                    pFieldEdit = FeatureClassHelper.CreateField(rdSegNode.OtherNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(rdSegNode.AdjIDsNm, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(rdSegNode.NorthAnglesNm, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    IFeatureClass pNodeClass = FeatureClassHelper.CreateFeatureClass(feaDS, SegmentNodeEntity.RoadSegmentNodeName, esriGeometryType.esriGeometryPoint, roadSegNodeFields, rdSegNode.NodeIDNm);
                    return pNodeClass;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    return null;
                }
                
            }

        }


        public static IFeatureClass CreateSegmentClass(IFeatureDataset feaDS)
        {

            if (feaDS == null)
            {
                return null;
            }
            else
            {

                IFeatureWorkspace pFeatWorkspace = feaDS.Workspace as IFeatureWorkspace;
                if (DatasetHelper.ExistDataset(pFeatWorkspace, SegmentEntity.SegmentName))
                {
                    return pFeatWorkspace.OpenFeatureClass(SegmentEntity.SegmentName);
                }

                IFields fields = new FieldsClass();
                IFieldsEdit pFieldsEdit = fields as IFieldsEdit;

                Segment rdSeg = new Segment(null, 0);

                IFieldEdit pFieldEdit = FeatureClassHelper.CreateField(rdSeg.IDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(rdSeg.FNodeIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(rdSeg.TNodeIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);


                pFieldEdit = FeatureClassHelper.CreateField(rdSeg.RelIDNm, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(rdSeg.RoadTypeNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(rdSeg.FlowDirNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);


                pFieldEdit = FeatureClassHelper.CreateField(rdSeg.RoadNameNm, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(rdSeg.OtherNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                IFeatureClass pSegClass = FeatureClassHelper.CreateFeatureClass(feaDS, SegmentEntity.SegmentName, esriGeometryType.esriGeometryPolyline, fields, rdSeg.IDNm);
                return pSegClass;
            }
        }

        #endregion --------------创建RoadSegment图层-------------------------


        #region --------------创建Link图层-------------------------


        public static IFeatureClass CreateLinkClass(IFeatureDataset feaDS)
        {

            if (feaDS == null)
            {
                return null;
            }
            else
            {

                IFeatureWorkspace pFeatWorkspace = feaDS.Workspace as IFeatureWorkspace;
                if (DatasetHelper.ExistDataset(pFeatWorkspace, LinkEntity.LinkName))
                {
                    return pFeatWorkspace.OpenFeatureClass(LinkEntity.LinkName);
                }

                IFields fields = new FieldsClass();
                IFieldsEdit pFieldsEdit = fields as IFieldsEdit;

                Link link = new Link(null, 0);

                IFieldEdit pFieldEdit = FeatureClassHelper.CreateField(link.IDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(link.FNodeIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(link.TNodeIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);


                pFieldEdit = FeatureClassHelper.CreateField(link.RelIDNm, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(link.RoadTypeNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(link.FlowDirNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);


                pFieldEdit = FeatureClassHelper.CreateField(link.RoadNameNm, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(link.OtherNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                IFeatureClass pLinkClass = FeatureClassHelper.CreateFeatureClass(feaDS, LinkEntity.LinkName, esriGeometryType.esriGeometryPolyline, fields, link.IDNm);
                return pLinkClass;
            }
        }

        public static IFeatureClass CreateNodeClass(IFeatureDataset feaDS)
        {

            if (feaDS == null)
            {
                return null;
            }
            else
            {

                IFeatureWorkspace pFeatWorkspace = feaDS.Workspace as IFeatureWorkspace;
                if (DatasetHelper.ExistDataset(pFeatWorkspace, NodeEntity.NodeName))
                {
                    return pFeatWorkspace.OpenFeatureClass(NodeEntity.NodeName);
                }

                IFields roadSegNodeFields = new FieldsClass();
                IFieldsEdit pFieldsEdit = roadSegNodeFields as IFieldsEdit;

                Node node = new Node(null, 0, null);

                IFieldEdit pFieldEdit = FeatureClassHelper.CreateField(node.NodeIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(node.CompositeTypeNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(node.NodeTypeNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);


                pFieldEdit = FeatureClassHelper.CreateField(node.OtherNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(node.AdjIDsNm, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(node.NorthAnglesNm, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                IFeatureClass pNodeClass = FeatureClassHelper.CreateFeatureClass(feaDS, NodeEntity.NodeName, esriGeometryType.esriGeometryPoint, roadSegNodeFields, node.NodeIDNm);
                return pNodeClass;

            }

        }

        public static IFeatureClass CreateArcClass(IFeatureDataset feaDS)
        {

            if (feaDS == null)
            {
                return null;
            }
            else
            {

                IFeatureWorkspace pFeatWorkspace = feaDS.Workspace as IFeatureWorkspace;
                if (DatasetHelper.ExistDataset(pFeatWorkspace, ArcEntity.ArcFeatureName))
                {
                    return pFeatWorkspace.OpenFeatureClass(ArcEntity.ArcFeatureName);
                }

                IFields fields = new FieldsClass();
                IFieldsEdit pFieldsEdit = fields as IFieldsEdit;

                IFieldEdit pFieldEdit = FeatureClassHelper.CreateField(Arc.ArcIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(Arc.LaneNumNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(Arc.LinkIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);


                pFieldEdit = FeatureClassHelper.CreateField(Arc.FlowDirNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(Arc.OtherNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                IFeatureClass pArcClass = FeatureClassHelper.CreateFeatureClass(feaDS, ArcEntity.ArcFeatureName, esriGeometryType.esriGeometryPolyline, fields, Arc.ArcIDNm);
                return pArcClass;
            }
        }

        #endregion --------------创建Link图层-------------------------


        #region --------------创建Lane图层-------------------------
            #region --------------创建Lane图层,表格式-------------------------
                public static ITable CreateLaneTable(IWorkspace2 workspace)
                {

                    if (workspace == null)
                    {
                        return null;
                    }
                    else
                    {
                        IFeatureWorkspace pFeaWs = workspace as IFeatureWorkspace;
                
                        if (DatasetHelper.ExistDataset(pFeaWs,LaneEntity.LaneName))
                        {
                            return pFeaWs.OpenTable(LaneEntity.LaneName);
                        }

                        IFields fields = new FieldsClass();
                        IFieldsEdit pFieldsEdit = fields as IFieldsEdit;

                        IFieldEdit pFieldEdit = FeatureClassHelper.CreateField(LaneTable.ArcIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneTable.PositionNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneTable.SerialNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);


                        pFieldEdit = FeatureClassHelper.CreateField(LaneTable.ArcIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneTable.ChangeNm, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneTable.LeftBoundaryIDNm, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);


                        pFieldEdit = FeatureClassHelper.CreateField(LaneTable.RightBoundaryIDNm, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneTable.VehClassesNm, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneTable.LaneClosedNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);


                        pFieldEdit = FeatureClassHelper.CreateField(LaneTable.WidthNm, esriFieldType.esriFieldTypeDouble, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneTable.OtherNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        ITable pLaneTable = DataTableHelper.CreateTable(workspace, LaneEntity.LaneName, fields);
                        return pLaneTable;
                    }
                }

                public static ITable CreateConnectorTable(IWorkspace2 workspace)
            {

                if (workspace == null)
                {
                    return null;
                }
                else
                {

                    IFeatureWorkspace pFeaWs = workspace as IFeatureWorkspace;

                    if (DatasetHelper.ExistDataset(pFeaWs, LaneConnectorEntity.ConnectorName))
                    {
                        return pFeaWs.OpenTable(LaneConnectorEntity.ConnectorName);
                    }
                    else
                    {
                        IFields fields = new FieldsClass();
                        IFieldsEdit pFieldsEdit = fields as IFieldsEdit;

                        IFieldEdit pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTable.ConnectorIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTable.fromLaneIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTable.toLaneIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);


                        pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTable.fromArcIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTable.TurningDirNm, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTable.toArcIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);


                        pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTable.fromLinkIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTable.fromDirNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTable.toLinkIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);


                        pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTable.toDirNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTable.OtherNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        ITable pConnectorTable = DataTableHelper.CreateTable(workspace, LaneConnectorEntity.ConnectorName, fields);
                        return pConnectorTable;
                    }

               
                }
            }
            #endregion --------------创建Lane图层，表格式-------------------------

        #region --------------创建Lane图层,表格式-------------------------

                public static IFeatureClass CreateLaneClass(IFeatureDataset feaDS)
                {
                    if (feaDS == null)
                    {
                        return null;
                    }
                    else
                    {

                        IFeatureWorkspace pFeatWorkspace = feaDS.Workspace as IFeatureWorkspace;
                        if (DatasetHelper.ExistDataset(pFeatWorkspace, LaneEntity.LaneName))
                        {
                            return pFeatWorkspace.OpenFeatureClass(LaneEntity.LaneName);
                        }

                        IFields fields = new FieldsClass();
                        IFieldsEdit pFieldsEdit = fields as IFieldsEdit;

                        IFieldEdit pFieldEdit = FeatureClassHelper.CreateField(LaneFeature.ArcIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneFeature.PositionNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);


                        pFieldEdit = FeatureClassHelper.CreateField(LaneFeature.ArcIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneFeature.ChangeNm, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneFeature.LeftBoundaryIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);


                        pFieldEdit = FeatureClassHelper.CreateField(LaneFeature.RightBoundaryIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneFeature.VehClassesNm, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneFeature.LaneClosedNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);


                        pFieldEdit = FeatureClassHelper.CreateField(LaneFeature.WidthNm, esriFieldType.esriFieldTypeDouble, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneFeature.OtherNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        IFeatureClass pLaneClass = FeatureClassHelper.CreateFeatureClass(feaDS, LaneEntity.LaneName, esriGeometryType.esriGeometryPolyline, fields, LaneFeature.LaneIDNm);
                        return pLaneClass;

                    }
                }

                public static IFeatureClass CreateConnectorClass(IFeatureDataset feaDS)
                {
                    if (feaDS == null)
                    {
                        return null;
                    }
                    else
                    {

                        IFeatureWorkspace pFeatWorkspace = feaDS.Workspace as IFeatureWorkspace;
                        if (DatasetHelper.ExistDataset(pFeatWorkspace, LaneConnectorEntity.ConnectorName))
                        {
                            return pFeatWorkspace.OpenFeatureClass(LaneConnectorEntity.ConnectorName);
                        }

                        else
                        {
                            IFields fields = new FieldsClass();
                            IFieldsEdit pFieldsEdit = fields as IFieldsEdit;

                            IFieldEdit pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTable.ConnectorIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                            pFieldsEdit.AddField(pFieldEdit);

                            pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTable.fromLaneIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                            pFieldsEdit.AddField(pFieldEdit);

                            pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTable.toLaneIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                            pFieldsEdit.AddField(pFieldEdit);


                            pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTable.fromArcIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                            pFieldsEdit.AddField(pFieldEdit);

                            pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTable.TurningDirNm, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                            pFieldsEdit.AddField(pFieldEdit);

                            pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTable.toArcIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                            pFieldsEdit.AddField(pFieldEdit);


                            pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTable.fromLinkIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                            pFieldsEdit.AddField(pFieldEdit);

                            pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTable.fromDirNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                            pFieldsEdit.AddField(pFieldEdit);

                            pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTable.toLinkIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                            pFieldsEdit.AddField(pFieldEdit);


                            pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTable.toDirNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                            pFieldsEdit.AddField(pFieldEdit);

                            pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTable.OtherNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                            pFieldsEdit.AddField(pFieldEdit);

                            IFeatureClass pConnectorClass = FeatureClassHelper.CreateFeatureClass(feaDS, LaneConnectorEntity.ConnectorName, esriGeometryType.esriGeometryPolyline, fields, LaneConnectorTable.ConnectorIDNm);
                            return pConnectorClass;

                        }
                    }
                }

        #endregion --------------创建Lane图层，表格式-------------------------

                public static IFeatureClass CreateBoudaryClass(IFeatureDataset feaDS)
                {
                    if (feaDS == null)
                    {
                        return null;
                    }
                    else
                    {

                        IFeatureWorkspace pFeatWorkspace = feaDS.Workspace as IFeatureWorkspace;
                        if (DatasetHelper.ExistDataset(pFeatWorkspace, BoundaryEntity.BoundaryName))
                        {
                            return pFeatWorkspace.OpenFeatureClass(BoundaryEntity.BoundaryName);
                        }

                        IFields fields = new FieldsClass();
                        IFieldsEdit pFieldsEdit = fields as IFieldsEdit;

                        IFieldEdit pFieldEdit = FeatureClassHelper.CreateField(LaneFeature.ArcIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneFeature.PositionNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);


                        pFieldEdit = FeatureClassHelper.CreateField(LaneFeature.ArcIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneFeature.ChangeNm, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneFeature.LeftBoundaryIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);


                        pFieldEdit = FeatureClassHelper.CreateField(LaneFeature.RightBoundaryIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneFeature.VehClassesNm, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneFeature.LaneClosedNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);


                        pFieldEdit = FeatureClassHelper.CreateField(LaneFeature.WidthNm, esriFieldType.esriFieldTypeDouble, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneFeature.OtherNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        IFeatureClass pLaneClass = FeatureClassHelper.CreateFeatureClass(feaDS, LaneEntity.LaneName, esriGeometryType.esriGeometryPolyline, fields, LaneFeature.LaneIDNm);
                        return pLaneClass;

                    }
                }

        #endregion --------------创建Lane图层-------------------------

        #region -----------------------------创建标志标线--------------------------


        #endregion -----------------------------创建标志标线--------------------------

        #region --------------创建指路标志路网-------------------------

                public static IFeatureClass CreateNode1Class(IFeatureDataset feaDS)
        {

            if (feaDS == null)
            {
                return null;
            }
            else
            {
                IFeatureWorkspace pFeatWorkspace = feaDS.Workspace as IFeatureWorkspace;
                if (DatasetHelper.ExistDataset(pFeatWorkspace, Node1Entity.Node1Name))
                {
                    return pFeatWorkspace.OpenFeatureClass(Node1Entity.Node1Name);
                }

                IFields node1Fields = new FieldsClass();
                IFieldsEdit pFieldsEdit = node1Fields as IFieldsEdit;

                try
                {
                    Node1 node1 = new Node1(null, 0, null);
                    IFieldEdit pFieldEdit = FeatureClassHelper.CreateField(node1.NodeIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(node1.CompositeTypeNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(node1.NodeTypeNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);


                    pFieldEdit = FeatureClassHelper.CreateField(node1.OtherNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(node1.AdjIDsNm, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(node1.NorthAnglesNm, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    IFeatureClass pNodeClass = FeatureClassHelper.CreateFeatureClass(feaDS, Node1Entity.Node1Name, esriGeometryType.esriGeometryPoint, node1Fields, node1.NodeIDNm);
                    return pNodeClass;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    return null;
                }

            }

        }


        public static IFeatureClass CreateArc1Class(IFeatureDataset feaDS)
        {

            if (feaDS == null)
            {
                return null;
            }
            else
            {

                IFeatureWorkspace pFeatWorkspace = feaDS.Workspace as IFeatureWorkspace;
                if (DatasetHelper.ExistDataset(pFeatWorkspace, Arc1Entity.Arc1Name))
                {
                    return pFeatWorkspace.OpenFeatureClass(Arc1Entity.Arc1Name);
                }

                IFields fields = new FieldsClass();
                IFieldsEdit pFieldsEdit = fields as IFieldsEdit;

                Arc1 arc1 = new Arc1(null, 0);

                IFieldEdit pFieldEdit = FeatureClassHelper.CreateField(arc1.IDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(arc1.FNodeIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(arc1.TNodeIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);


                pFieldEdit = FeatureClassHelper.CreateField(arc1.RelIDNm, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(arc1.RoadTypeNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(arc1.FlowDirNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);


                pFieldEdit = FeatureClassHelper.CreateField(arc1.RoadNameNm, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(arc1.OtherNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                IFeatureClass pSegClass = FeatureClassHelper.CreateFeatureClass(feaDS, Arc1Entity.Arc1Name, esriGeometryType.esriGeometryPolyline, fields, arc1.IDNm);
                return pSegClass;
            }
        }

        #endregion --------------创建指路标志路网-------------------------

        

    }
}
