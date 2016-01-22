using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.DataModel.Float;
using RoadNetworkSystem.DataModel.GuideSignNetwork;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.RoadSign;
using RoadNetworkSystem.DataModel.Sensor;
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
                if (DatasetHelper.ExistDataset(pFeatWorkspace, SegmentNode.RoadSegmentNodeName))
                {
                    return pFeatWorkspace.OpenFeatureClass(SegmentNode.RoadSegmentNodeName);
                }

                IFields roadSegNodeFields = new FieldsClass();
                IFieldsEdit pFieldsEdit = roadSegNodeFields as IFieldsEdit;

                try
                {
                    SegmentNodeService rdSegNode = new SegmentNodeService(null, 0, null);
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

                    IFeatureClass pNodeClass = FeatureClassHelper.CreateFeatureClass(feaDS, SegmentNode.RoadSegmentNodeName, esriGeometryType.esriGeometryPoint, roadSegNodeFields, rdSegNode.NodeIDNm);
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
                if (DatasetHelper.ExistDataset(pFeatWorkspace, Segment.SegmentName))
                {
                    return pFeatWorkspace.OpenFeatureClass(Segment.SegmentName);
                }

                IFields fields = new FieldsClass();
                IFieldsEdit pFieldsEdit = fields as IFieldsEdit;

                SegmentService rdSeg = new SegmentService(null, 0);

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

                IFeatureClass pSegClass = FeatureClassHelper.CreateFeatureClass(feaDS, Segment.SegmentName, esriGeometryType.esriGeometryPolyline, fields, rdSeg.IDNm);
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
                if (DatasetHelper.ExistDataset(pFeatWorkspace, Link.LinkName))
                {
                    return pFeatWorkspace.OpenFeatureClass(Link.LinkName);
                }

                IFields fields = new FieldsClass();
                IFieldsEdit pFieldsEdit = fields as IFieldsEdit;

                LinkService link = new LinkService(null, 0);

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

                IFeatureClass pLinkClass = FeatureClassHelper.CreateFeatureClass(feaDS, Link.LinkName, esriGeometryType.esriGeometryPolyline, fields, link.IDNm);
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
                if (DatasetHelper.ExistDataset(pFeatWorkspace, Node.NodeName))
                {
                    return pFeatWorkspace.OpenFeatureClass(Node.NodeName);
                }

                IFields roadSegNodeFields = new FieldsClass();
                IFieldsEdit pFieldsEdit = roadSegNodeFields as IFieldsEdit;

                NodeService node = new NodeService(null, 0, null);

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

                IFeatureClass pNodeClass = FeatureClassHelper.CreateFeatureClass(feaDS, Node.NodeName, esriGeometryType.esriGeometryPoint, roadSegNodeFields, node.NodeIDNm);
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
                if (DatasetHelper.ExistDataset(pFeatWorkspace, Arc.ArcFeatureName))
                {
                    return pFeatWorkspace.OpenFeatureClass(Arc.ArcFeatureName);
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

                IFeatureClass pArcClass = FeatureClassHelper.CreateFeatureClass(feaDS, Arc.ArcFeatureName, esriGeometryType.esriGeometryPolyline, fields, Arc.ArcIDNm);
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
                
                        if (DatasetHelper.ExistDataset(pFeaWs,Lane.LaneName))
                        {
                            return pFeaWs.OpenTable(Lane.LaneName);
                        }

                        IFields fields = new FieldsClass();
                        IFieldsEdit pFieldsEdit = fields as IFieldsEdit;

                        IFieldEdit pFieldEdit = FeatureClassHelper.CreateField(LaneTableService.ArcIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneTableService.PositionNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneTableService.SerialNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);


                        pFieldEdit = FeatureClassHelper.CreateField(LaneTableService.ArcIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneTableService.ChangeNm, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneTableService.LeftBoundaryIDNm, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);


                        pFieldEdit = FeatureClassHelper.CreateField(LaneTableService.RightBoundaryIDNm, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneTableService.VehClassesNm, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneTableService.LaneClosedNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);


                        pFieldEdit = FeatureClassHelper.CreateField(LaneTableService.WidthNm, esriFieldType.esriFieldTypeDouble, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneTableService.OtherNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        ITable pLaneTable = DataTableHelper.CreateTable(workspace, Lane.LaneName, fields);
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

                    if (DatasetHelper.ExistDataset(pFeaWs, LaneConnector.ConnectorName))
                    {
                        return pFeaWs.OpenTable(LaneConnector.ConnectorName);
                    }
                    else
                    {
                        IFields fields = new FieldsClass();
                        IFieldsEdit pFieldsEdit = fields as IFieldsEdit;

                        IFieldEdit pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTableService.ConnectorIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTableService.fromLaneIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTableService.toLaneIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);


                        pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTableService.fromArcIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTableService.TurningDirNm, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTableService.toArcIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);


                        pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTableService.fromLinkIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTableService.fromDirNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTableService.toLinkIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);


                        pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTableService.toDirNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTableService.OtherNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                        pFieldsEdit.AddField(pFieldEdit);

                        ITable pConnectorTable = DataTableHelper.CreateTable(workspace, LaneConnector.ConnectorName, fields);
                        return pConnectorTable;
                    }

               
                }
            }
            #endregion --------------创建Lane图层，表格式-------------------------

            #region --------------创建Lane图层,要素类-------------------------

            public static IFeatureClass CreateLaneClass(IFeatureDataset feaDS)
                    {
                        if (feaDS == null)
                        {
                            return null;
                        }
                        else
                        {

                            IFeatureWorkspace pFeatWorkspace = feaDS.Workspace as IFeatureWorkspace;
                            if (DatasetHelper.ExistDataset(pFeatWorkspace, Lane.LaneName))
                            {
                                return pFeatWorkspace.OpenFeatureClass(Lane.LaneName);
                            }

                            IFields fields = new FieldsClass();
                            IFieldsEdit pFieldsEdit = fields as IFieldsEdit;


                            IFieldEdit pFieldEdit = FeatureClassHelper.CreateField(LaneFeatureService.ArcIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                            pFieldsEdit.AddField(pFieldEdit);

                            pFieldEdit = FeatureClassHelper.CreateField(LaneFeatureService.LaneIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                            pFieldsEdit.AddField(pFieldEdit);


                            pFieldEdit = FeatureClassHelper.CreateField(LaneFeatureService.PositionNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                            pFieldsEdit.AddField(pFieldEdit);


                            pFieldEdit = FeatureClassHelper.CreateField(LaneFeatureService.ArcIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                            pFieldsEdit.AddField(pFieldEdit);

                            pFieldEdit = FeatureClassHelper.CreateField(LaneFeatureService.ChangeNm, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                            pFieldsEdit.AddField(pFieldEdit);

                            pFieldEdit = FeatureClassHelper.CreateField(LaneFeatureService.LeftBoundaryIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                            pFieldsEdit.AddField(pFieldEdit);


                            pFieldEdit = FeatureClassHelper.CreateField(LaneFeatureService.RightBoundaryIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                            pFieldsEdit.AddField(pFieldEdit);

                            pFieldEdit = FeatureClassHelper.CreateField(LaneFeatureService.VehClassesNm, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                            pFieldsEdit.AddField(pFieldEdit);

                            pFieldEdit = FeatureClassHelper.CreateField(LaneFeatureService.LaneClosedNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                            pFieldsEdit.AddField(pFieldEdit);


                            pFieldEdit = FeatureClassHelper.CreateField(LaneFeatureService.WidthNm, esriFieldType.esriFieldTypeDouble, 50, 0, "", true, true);
                            pFieldsEdit.AddField(pFieldEdit);

                            pFieldEdit = FeatureClassHelper.CreateField(LaneFeatureService.OtherNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                            pFieldsEdit.AddField(pFieldEdit);

                            IFeatureClass pLaneClass = FeatureClassHelper.CreateFeatureClass(feaDS, Lane.LaneName, esriGeometryType.esriGeometryPolyline, fields, LaneFeatureService.LaneIDNm);
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
                            if (DatasetHelper.ExistDataset(pFeatWorkspace, LaneConnector.ConnectorName))
                            {
                                return pFeatWorkspace.OpenFeatureClass(LaneConnector.ConnectorName);
                            }

                            else
                            {
                                IFields fields = new FieldsClass();
                                IFieldsEdit pFieldsEdit = fields as IFieldsEdit;

                                IFieldEdit pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTableService.ConnectorIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                                pFieldsEdit.AddField(pFieldEdit);

                                pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTableService.fromLaneIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                                pFieldsEdit.AddField(pFieldEdit);

                                pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTableService.toLaneIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                                pFieldsEdit.AddField(pFieldEdit);


                                pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTableService.fromArcIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                                pFieldsEdit.AddField(pFieldEdit);

                                pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTableService.TurningDirNm, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                                pFieldsEdit.AddField(pFieldEdit);

                                pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTableService.toArcIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                                pFieldsEdit.AddField(pFieldEdit);


                                pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTableService.fromLinkIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                                pFieldsEdit.AddField(pFieldEdit);

                                pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTableService.fromDirNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                                pFieldsEdit.AddField(pFieldEdit);

                                pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTableService.toLinkIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                                pFieldsEdit.AddField(pFieldEdit);


                                pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTableService.toDirNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                                pFieldsEdit.AddField(pFieldEdit);

                                pFieldEdit = FeatureClassHelper.CreateField(LaneConnectorTableService.OtherNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                                pFieldsEdit.AddField(pFieldEdit);

                                IFeatureClass pConnectorClass = FeatureClassHelper.CreateFeatureClass(feaDS, LaneConnector.ConnectorName, esriGeometryType.esriGeometryPolyline, fields, LaneConnectorTableService.ConnectorIDNm);
                                return pConnectorClass;

                            }
                        }
                    }

            #endregion --------------创建Lane图层，要素类-------------------------

        
        #endregion --------------创建Lane图层-------------------------

        #region -----------------------------创建标志标线--------------------------

            public static IFeatureClass CreateBoudaryClass(IFeatureDataset feaDS)
            {
                if (feaDS == null)
                {
                    return null;
                }
                else
                {

                    IFeatureWorkspace pFeatWorkspace = feaDS.Workspace as IFeatureWorkspace;
                    if (DatasetHelper.ExistDataset(pFeatWorkspace, Boundary.BoundaryName))
                    {
                        return pFeatWorkspace.OpenFeatureClass(Boundary.BoundaryName);
                    }

                    IFields fields = new FieldsClass();
                    IFieldsEdit pFieldsEdit = fields as IFieldsEdit;

                    IFieldEdit pFieldEdit = FeatureClassHelper.CreateField(Boundary.BOUNDARYID_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(Boundary.STYLEID_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);


                    pFieldEdit = FeatureClassHelper.CreateField(Boundary.DIR_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    IFeatureClass pLaneClass = FeatureClassHelper.CreateFeatureClass(feaDS, Boundary.BoundaryName,
                        esriGeometryType.esriGeometryPolyline, fields,Boundary.BOUNDARYID_NAME);
                    return pLaneClass;

                }
            }

            public static IFeatureClass CreateTurnArrowClass(IFeatureDataset feaDS)
            {
                if (feaDS == null)
                {
                    return null;
                }
                else
                {

                    IFeatureWorkspace pFeatWorkspace = feaDS.Workspace as IFeatureWorkspace;
                    if (DatasetHelper.ExistDataset(pFeatWorkspace, TurnArrow.TurnArrowName))
                    {
                        return pFeatWorkspace.OpenFeatureClass(TurnArrow.TurnArrowName);
                    }

                    IFields fields = new FieldsClass();
                    IFieldsEdit pFieldsEdit = fields as IFieldsEdit;

                    IFieldEdit pFieldEdit = FeatureClassHelper.CreateField(TurnArrow.ANGLENm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(TurnArrow.ArcIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);


                    pFieldEdit = FeatureClassHelper.CreateField(TurnArrow.ArrowIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);


                    pFieldEdit = FeatureClassHelper.CreateField(TurnArrow.ArrowTypeNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);


                    pFieldEdit = FeatureClassHelper.CreateField(TurnArrow.LaneIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(TurnArrow.PrecedeArrowsNm, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);


                    pFieldEdit = FeatureClassHelper.CreateField(TurnArrow.SerialNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);


                    pFieldEdit = FeatureClassHelper.CreateField(TurnArrow.STYLEID_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    IFeatureClass pLaneClass = FeatureClassHelper.CreateFeatureClass(feaDS, TurnArrow.TurnArrowName, 
                        esriGeometryType.esriGeometryPoint, fields, TurnArrow.ArrowIDNm);
                    return pLaneClass;

                }
            }


            public static IFeatureClass CreateStopLineClass(IFeatureDataset feaDS)
            {
                if (feaDS == null)
                {
                    return null;
                }
                else
                {
                    IFeatureWorkspace pFeatWorkspace = feaDS.Workspace as IFeatureWorkspace;
                    if (DatasetHelper.ExistDataset(pFeatWorkspace, StopLine.StopLineName))
                    {
                        return pFeatWorkspace.OpenFeatureClass(StopLine.StopLineName);
                    }

                    IFields fields = new FieldsClass();
                    IFieldsEdit pFieldsEdit = fields as IFieldsEdit;

                    IFieldEdit pFieldEdit = FeatureClassHelper.CreateField(StopLine.ArcIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(StopLine.LaneIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);


                    pFieldEdit = FeatureClassHelper.CreateField(StopLine.NodeIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);


                    pFieldEdit = FeatureClassHelper.CreateField(StopLine.OtherNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);


                    pFieldEdit = FeatureClassHelper.CreateField(StopLine.StopLineIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    IFeatureClass pLaneClass = FeatureClassHelper.CreateFeatureClass(feaDS, StopLine.StopLineName,
                        esriGeometryType.esriGeometryPolyline, fields, StopLine.StopLineIDNm);
                    return pLaneClass;
                }
            }


            public static IFeatureClass CreateSurfaceClass(IFeatureDataset feaDS)
            {
                if (feaDS == null)
                {
                    return null;
                }
                else
                {
                    IFeatureWorkspace pFeatWorkspace = feaDS.Workspace as IFeatureWorkspace;
                    if (DatasetHelper.ExistDataset(pFeatWorkspace, Surface.SurfaceName))
                    {
                        return pFeatWorkspace.OpenFeatureClass(Surface.SurfaceName);
                    }

                    IFields fields = new FieldsClass();
                    IFieldsEdit pFieldsEdit = fields as IFieldsEdit;

                    IFieldEdit pFieldEdit = FeatureClassHelper.CreateField(Surface.SurfaceIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(Surface.ArcIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);


                    pFieldEdit = FeatureClassHelper.CreateField(Surface.ControlIDsNm, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);


                    pFieldEdit = FeatureClassHelper.CreateField(Surface.OtherNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);


                    IFeatureClass pLaneClass = FeatureClassHelper.CreateFeatureClass(feaDS, Surface.SurfaceName,
                        esriGeometryType.esriGeometryPolygon, fields, Surface.SurfaceIDNm);
                    return pLaneClass;
                }
            }

            public static IFeatureClass CreateKerbClass(IFeatureDataset feaDS)
            {
                if (feaDS == null)
                {
                    return null;
                }
                else
                {
                    IFeatureWorkspace pFeatWorkspace = feaDS.Workspace as IFeatureWorkspace;
                    if (DatasetHelper.ExistDataset(pFeatWorkspace, Kerb.KerbName))
                    {
                        return pFeatWorkspace.OpenFeatureClass(Kerb.KerbName);
                    }

                    IFields fields = new FieldsClass();
                    IFieldsEdit pFieldsEdit = fields as IFieldsEdit;

                    IFieldEdit pFieldEdit = FeatureClassHelper.CreateField(Kerb.KerbIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(Kerb.ArcIDNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);


                    pFieldEdit = FeatureClassHelper.CreateField(Kerb.SerialNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);


                    pFieldEdit = FeatureClassHelper.CreateField(Surface.OtherNm, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);


                    IFeatureClass pLaneClass = FeatureClassHelper.CreateFeatureClass(feaDS, Kerb.KerbName,
                        esriGeometryType.esriGeometryPoint, fields, Kerb.KerbIDNm);
                    return pLaneClass;
                }
            }


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
                if (DatasetHelper.ExistDataset(pFeatWorkspace, Node1.Node1Name))
                {
                    return pFeatWorkspace.OpenFeatureClass(Node1.Node1Name);
                }

                IFields node1Fields = new FieldsClass();
                IFieldsEdit pFieldsEdit = node1Fields as IFieldsEdit;

                try
                {
                    Node1Service node1 = new Node1Service(null, 0, null);
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

                    IFeatureClass pNodeClass = FeatureClassHelper.CreateFeatureClass(feaDS, Node1.Node1Name, esriGeometryType.esriGeometryPoint, node1Fields, node1.NodeIDNm);
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
                if (DatasetHelper.ExistDataset(pFeatWorkspace, Arc1.Arc1Name))
                {
                    return pFeatWorkspace.OpenFeatureClass(Arc1.Arc1Name);
                }

                IFields fields = new FieldsClass();
                IFieldsEdit pFieldsEdit = fields as IFieldsEdit;

                Arc1Service arc1 = new Arc1Service(null, 0);

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

                IFeatureClass pSegClass = FeatureClassHelper.CreateFeatureClass(feaDS, Arc1.Arc1Name, esriGeometryType.esriGeometryPolyline, fields, arc1.IDNm);
                return pSegClass;
            }
        }

        #endregion --------------创建指路标志路网-------------------------


        #region -------------------- 检测器 -------------------------------
        public static IFeatureClass CreateBASE_FAC_LDT(IFeatureDataset feaDS)
        {
            if (feaDS == null)
            {
                return null;
            }
            else
            {
                IFeatureWorkspace pFeatWorkspace = feaDS.Workspace as IFeatureWorkspace;
                if (DatasetHelper.ExistDataset(pFeatWorkspace, BaseFacilityLDT.BaseFacilityLDT_Name))
                {
                    return pFeatWorkspace.OpenFeatureClass(BaseFacilityLDT.BaseFacilityLDT_Name);
                }

                IFields node1Fields = new FieldsClass();
                IFieldsEdit pFieldsEdit = node1Fields as IFieldsEdit;

                try
                {
                    IFieldEdit pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityLDT.LDTID_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityLDT.CREATE_DATE_NAME, esriFieldType.esriFieldTypeDate, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityLDT.DIR_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);


                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityLDT.LANEID_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityLDT.LINKID_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityLDT.MAINTAIN_DATE_NAME, esriFieldType.esriFieldTypeDate, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityLDT.OFFSET_NAME, esriFieldType.esriFieldTypeDouble, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityLDT.POS_NAME, esriFieldType.esriFieldTypeDouble, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityLDT.STATUS_NAME, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    IFeatureClass pNodeClass = FeatureClassHelper.CreateFeatureClass(feaDS, BaseFacilityLDT.BaseFacilityLDT_Name, esriGeometryType.esriGeometryPoint, node1Fields, BaseFacilityLDT.BaseFacilityLDT_Name);
                    return pNodeClass;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    return null;
                }
            }

        }
        public static ITable CreateBASE_Float_LDT(IWorkspace2 workspace)
        {
            if (workspace == null)
            {
                return null;
            }
            else
            {
                IFeatureWorkspace pFeaWs = workspace as IFeatureWorkspace;

                if (DatasetHelper.ExistDataset(pFeaWs, BaseFloatLDT.BaseFloatLDT_NAME))
                {
                    return pFeaWs.OpenTable(BaseFacilityLDT.BaseFacilityLDT_Name);
                }

                IFields fields = new FieldsClass();
                IFieldsEdit pFieldsEdit = fields as IFieldsEdit;

                IFieldEdit pFieldEdit = FeatureClassHelper.CreateField(BaseFloatLDT.DIR_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatLDT.LANEID_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatLDT.LDTID_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);


                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatLDT.LINKID_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatLDT.VELOCITY_NAME, esriFieldType.esriFieldTypeDouble, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatLDT.DENSITY_NAME, esriFieldType.esriFieldTypeDouble, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);


                ITable pLaneTable = DataTableHelper.CreateTable(workspace, BaseFloatLDT.BaseFloatLDT_NAME, fields);
                return pLaneTable;
            }
        }

        public static IFeatureClass CreateBASE_FAC_VDT(IFeatureDataset feaDS)
        {
            if (feaDS == null)
            {
                return null;
            }
            else
            {
                IFeatureWorkspace pFeatWorkspace = feaDS.Workspace as IFeatureWorkspace;
                if (DatasetHelper.ExistDataset(pFeatWorkspace, BaseFacilityVDT.BaseFacilityVDT_NAME))
                {
                    return pFeatWorkspace.OpenFeatureClass(BaseFacilityVDT.BaseFacilityVDT_NAME);
                }

                IFields node1Fields = new FieldsClass();
                IFieldsEdit pFieldsEdit = node1Fields as IFieldsEdit;

                try
                {
                    IFieldEdit pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityVDT.VDTID_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityVDT.CREATE_DATE_NAME, esriFieldType.esriFieldTypeDate, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityVDT.DIR_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);



                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityVDT.LINKID_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityVDT.MAINTAIN_DATE_NAME, esriFieldType.esriFieldTypeDate, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityVDT.OFFSET_NAME, esriFieldType.esriFieldTypeDouble, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityVDT.POS_NAME, esriFieldType.esriFieldTypeDouble, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityVDT.STATUS_NAME, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityVDT.LANE1_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityVDT.LANE2_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityVDT.LANE3_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityVDT.LANE4_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityVDT.LANE5_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityVDT.LANE6_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityVDT.LANE7_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityVDT.LANE8_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityVDT.VDT_TYPE_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    IFeatureClass pNodeClass = FeatureClassHelper.CreateFeatureClass(feaDS, BaseFacilityVDT.BaseFacilityVDT_NAME,
                        esriGeometryType.esriGeometryPoint, node1Fields, BaseFacilityVDT.BaseFacilityVDT_NAME);
                    return pNodeClass;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    return null;
                }
            }

        }

        public static ITable CreateBASE_Float_VDT(IWorkspace2 workspace)
        {
            if (workspace == null)
            {
                return null;
            }
            else
            {
                IFeatureWorkspace pFeaWs = workspace as IFeatureWorkspace;

                if (DatasetHelper.ExistDataset(pFeaWs, BaseFloatVDT.BaseFloatVDT_NAME))
                {
                    return pFeaWs.OpenTable(BaseFloatVDT.BaseFloatVDT_NAME);
                }

                IFields fields = new FieldsClass();
                IFieldsEdit pFieldsEdit = fields as IFieldsEdit;

                IFieldEdit pFieldEdit = FeatureClassHelper.CreateField(BaseFloatVDT.DIR_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatVDT.LANE1_QUANTITY_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatVDT.LANE2_QUANTITY_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);


                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatVDT.LANE3_QUANTITY_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);


                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatVDT.LANE4_QUANTITY_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatVDT.LANE5_QUANTITY_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatVDT.LANE6_QUANTITY_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);

                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatVDT.LANE7_QUANTITY_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);


                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatVDT.LANE8_QUANTITY_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatVDT.LANEID_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatVDT.LINKID_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);


                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatVDT.LONG_VEH_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatVDT.TTIME_NAME, esriFieldType.esriFieldTypeDate, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatVDT.FTIME_NAME, esriFieldType.esriFieldTypeDate, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);


                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatVDT.VDT_INDEX_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);


                ITable pLaneTable = DataTableHelper.CreateTable(workspace, BaseFloatVDT.BaseFloatVDT_NAME, fields);
                return pLaneTable;
            }
        }

        public static IFeatureClass CreateBASE_FAC_WDT(IFeatureDataset feaDS)
        {
            if (feaDS == null)
            {
                return null;
            }
            else
            {
                IFeatureWorkspace pFeatWorkspace = feaDS.Workspace as IFeatureWorkspace;
                if (DatasetHelper.ExistDataset(pFeatWorkspace, BaseFacilityWDT.BaseFacilityWDT_NAME))
                {
                    return pFeatWorkspace.OpenFeatureClass(BaseFacilityWDT.BaseFacilityWDT_NAME);
                }

                IFields node1Fields = new FieldsClass();
                IFieldsEdit pFieldsEdit = node1Fields as IFieldsEdit;

                try
                {
                    IFieldEdit pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityWDT.WDTID_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityWDT.CREATE_DATE_NAME, esriFieldType.esriFieldTypeDate, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityWDT.DIR_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityWDT.LINKID_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityWDT.MAINTAIN_DATE_NAME, esriFieldType.esriFieldTypeDate, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityWDT.OFFSET_NAME, esriFieldType.esriFieldTypeDouble, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityWDT.POS_NAME, esriFieldType.esriFieldTypeDouble, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityWDT.STATUS_NAME, esriFieldType.esriFieldTypeString, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityWDT.LANE1_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityWDT.LANE2_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityWDT.LANE3_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityWDT.LANE4_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityWDT.LANE5_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityWDT.LANE6_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityWDT.LANE7_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);

                    pFieldEdit = FeatureClassHelper.CreateField(BaseFacilityWDT.LANE8_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                    pFieldsEdit.AddField(pFieldEdit);


                    IFeatureClass pNodeClass = FeatureClassHelper.CreateFeatureClass(feaDS, BaseFacilityWDT.BaseFacilityWDT_NAME,
                        esriGeometryType.esriGeometryPoint, node1Fields, BaseFacilityWDT.WDTID_NAME);
                    return pNodeClass;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    return null;
                }
            }

        }
        public static ITable CreateBASE_Float_WDT(IWorkspace2 workspace)
        {
            if (workspace == null)
            {
                return null;
            }
            else
            {
                IFeatureWorkspace pFeaWs = workspace as IFeatureWorkspace;

                if (DatasetHelper.ExistDataset(pFeaWs, BaseFloatWDT.BaseFloatWDT_NAME))
                {
                    return pFeaWs.OpenTable(BaseFloatWDT.BaseFloatWDT_NAME);
                }

                IFields fields = new FieldsClass();
                IFieldsEdit pFieldsEdit = fields as IFieldsEdit;

                IFieldEdit pFieldEdit = FeatureClassHelper.CreateField(BaseFloatWDT.DIR_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatWDT.LANE1_QUANTITY_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatWDT.LANE2_QUANTITY_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatWDT.LANE3_QUANTITY_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);


                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatWDT.LANE4_QUANTITY_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatWDT.LANE5_QUANTITY_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatWDT.LANE6_QUANTITY_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);

                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatWDT.LANE7_QUANTITY_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);


                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatWDT.LANE8_QUANTITY_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatWDT.LANEID_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatWDT.LINKID_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);


                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatWDT.LONG_VEH_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatWDT.TTIME_NAME, esriFieldType.esriFieldTypeDate, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);

                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatWDT.FTIME_NAME, esriFieldType.esriFieldTypeDate, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);


                pFieldEdit = FeatureClassHelper.CreateField(BaseFloatWDT.VDT_INDEX_NAME, esriFieldType.esriFieldTypeInteger, 50, 0, "", true, true);
                pFieldsEdit.AddField(pFieldEdit);


                ITable pLaneTable = DataTableHelper.CreateTable(workspace, BaseFloatWDT.BaseFloatWDT_NAME, fields);
                return pLaneTable;
            }
        }

        #endregion -------------------- 检测器 -------------------------------

    }
}
