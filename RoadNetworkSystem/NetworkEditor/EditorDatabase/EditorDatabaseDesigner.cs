using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.GIS.GeoDatabase.Dataset;
using RoadNetworkSystem.NetworkExtraction.LaneBasedNetwork.DatabaseManager;

namespace RoadNetworkSystem.NetworkEditor.EditorDatabase
{
    class EditorDatabaseDesigner
    {
        private string _mdbPath;
        private IWorkspace _pWS2;
        private IFeatureDataset _feaDS;

        public EditorDatabaseDesigner(IWorkspace pWs)
        {


            _pWS2 = pWs;
            _feaDS = pWs.get_Datasets(esriDatasetType.esriDTFeatureDataset).Next() as IFeatureDataset;
            #region ------------无要素集，创建----------------
            
         
            if (_feaDS == null)
            {
                //是否存在Ming要素数据集
                bool flag = DatasetHelper.ExistDataset((_pWS2 as IFeatureWorkspace), "Ming");
                if (flag == false)
                {
                    ISpatialReferenceFactory spatialReferenceFactory = new SpatialReferenceEnvironmentClass();
                    ISpatialReference spatialReference = spatialReferenceFactory.CreateProjectedCoordinateSystem((int)esriSRProjCS4Type.esriSRProjCS_Xian1980_3_Degree_GK_Zone_38);
                    _feaDS = DatasetHelper.CreateFeatureDataset(_pWS2, "Ming", spatialReference);
                }
            }
            #endregion ------------无要素集，创建----------------
        }

        public void NewLaneBasedNetworkDb()
        {
            //----------------Setp2 创建Link层-----------------------------
            DatabaseDesigner.CreateLinkClass(_feaDS);
            DatabaseDesigner.CreateNodeClass(_feaDS);
            DatabaseDesigner.CreateArcClass(_feaDS);

            //----------------Setp3 创建Lane层-----------------------------

            DatabaseDesigner.CreateConnectorClass(_feaDS);
            DatabaseDesigner.CreateLaneClass(_feaDS);


        }
    }
}
