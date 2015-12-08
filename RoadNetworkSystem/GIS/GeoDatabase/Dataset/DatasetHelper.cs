using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.GIS.GeoDatabase.Dataset
{
    class DatasetHelper
    {
        /// <summary>
        /// 查找是否有某张表
        /// </summary>
        /// <param name="pFeatureWorkspace"></param> 要素工作空间
        /// <param name="DatasetName"></param>数据集名
        /// <returns></returns>
        public static bool ExistDataset(IFeatureWorkspace pFeatureWorkspace,string DatasetName)
        {
            try
            {
                IWorkspace pWks = pFeatureWorkspace as IWorkspace;
                if (pWks.Type == esriWorkspaceType.esriRemoteDatabaseWorkspace && DatasetName.Contains(".") == false)
                {
                    DatasetName = pWks.ConnectionProperties.GetProperty("User").ToString() + "." + DatasetName;
                }
                if (pWks.Type == esriWorkspaceType.esriFileSystemWorkspace)
                {
                    if (System.IO.File.Exists(pWks.PathName + "\\" + DatasetName + ".shp"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    IWorkspace2 pWs2 = pFeatureWorkspace as IWorkspace2;
                    if (pWs2.get_NameExists(esriDatasetType.esriDTFeatureClass, DatasetName))
                        return true;
                    if (pWs2.get_NameExists(esriDatasetType.esriDTTable, DatasetName))
                        return true;
                    if (pWs2.get_NameExists(esriDatasetType.esriDTFeatureDataset, DatasetName))
                        return true;
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 创建要素数据集FeatureDataset
        /// </summary>
        /// <param name="workspace"></param>工作空间
        /// <param name="fdsName"></param>数据集名称
        /// <param name="fdsSR"></param>数据集参考系
        /// <returns></returns>
        public static IFeatureDataset CreateFeatureDataset(IWorkspace workspace, string fdsName, ISpatialReference fdsSR)
        {
            IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspace;
            IFeatureDataset feaDataset= featureWorkspace.CreateFeatureDataset(fdsName, fdsSR);
            return feaDataset;
        }

        public static ESRI.ArcGIS.Geometry.ISpatialReference CreateHighPrecisionSpatialReference(System.Int32 spatialRefEnum, 
            System.Boolean hasMs, System.Boolean hasZs)
        {

            // Create a Projected or Geographic Coordinate System interface then set it equal to a call to the method
            // CreateHighPrecisionSR cast as the respective interface you have created. 
            // Examples:
            //   ESRI.ArcGIS.Geometry.IProjectedCoordinateSystem projSRset = CreateHighPrecisionSR((System.Int32)ESRI.ArcGIS.Geometry.esriSRProjCS3Type.esriSRProjCS_Sphere_Aitoff, true, true) as ESRI.ArcGIS.Geometry.IProjectedCoordinateSystem;
            //   ESRI.ArcGIS.Geometry.IGeographicCoordinateSystem geoSRset = CreateHighPrecisionSR((System.int32)ESRI.ArcGIS.Geometry.esriSRGeoCS3Type.esriSRGeoCS_TheMoon, true, false) as ESRI.ArcGIS.Geometry.IGeographicCoordinateSystem;

            ESRI.ArcGIS.Geometry.ISpatialReferenceFactory3 spatialReferenceFactory3 = new ESRI.ArcGIS.Geometry.SpatialReferenceEnvironmentClass();
            ESRI.ArcGIS.Geometry.ISpatialReferenceResolution spatialReferenceResolution;
            ESRI.ArcGIS.Geometry.IControlPrecision2 controlPrecision2;

            try
            {
                // Create the spatial reference
                ESRI.ArcGIS.Geometry.IGeographicCoordinateSystem geographicCoordinateSystem = spatialReferenceFactory3.CreateGeographicCoordinateSystem(spatialRefEnum);

                controlPrecision2 = geographicCoordinateSystem as ESRI.ArcGIS.Geometry.IControlPrecision2; // Dynamic Cast

                // Make the spatial reference high precision
                controlPrecision2.IsHighPrecision = true;

                spatialReferenceResolution = geographicCoordinateSystem as ESRI.ArcGIS.Geometry.ISpatialReferenceResolution; // Dynamic Cast
                spatialReferenceResolution.ConstructFromHorizon();
                spatialReferenceResolution.SetDefaultXYResolution();

                if (hasMs)
                    spatialReferenceResolution.SetDefaultMResolution();

                if (hasZs)
                    spatialReferenceResolution.SetDefaultZResolution();

                return geographicCoordinateSystem;
            }

            catch (System.ArgumentException)
            {
                // Create the spatial reference
                ESRI.ArcGIS.Geometry.IProjectedCoordinateSystem projectedCoordinateSystem = spatialReferenceFactory3.CreateProjectedCoordinateSystem((System.Int32)spatialRefEnum); // Explict Cast

                controlPrecision2 = projectedCoordinateSystem as ESRI.ArcGIS.Geometry.IControlPrecision2; // Dynamic Cast

                // Make the spatial reference high precision
                controlPrecision2.IsHighPrecision = true;

                spatialReferenceResolution = projectedCoordinateSystem as ESRI.ArcGIS.Geometry.ISpatialReferenceResolution; // Dynamic Cast
                spatialReferenceResolution.ConstructFromHorizon();
                spatialReferenceResolution.SetDefaultXYResolution();

                if (hasMs)
                    spatialReferenceResolution.SetDefaultMResolution();

                if (hasZs)
                    spatialReferenceResolution.SetDefaultZResolution();

                return projectedCoordinateSystem;
            }

        }

    }
}
