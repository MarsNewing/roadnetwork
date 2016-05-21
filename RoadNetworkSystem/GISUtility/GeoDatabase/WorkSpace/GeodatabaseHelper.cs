using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using RoadNetworkSystem.FileDirectory;
using RoadNetworkSystem.GIS.GeoDatabase.Dataset;
using RoadNetworkSystem.WinForm;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace RoadNetworkSystem.GIS.GeoDatabase.WorkSpace
{
    class GeodatabaseHelper
    {
        public static IFeatureDataset CreateGeoDatabase(ISpatialReference spatialReference)
        {
            string mdbName = "";
            string mdbDircetion = "";
            IFeatureDataset feaDs;
            IWorkspace WSP;
            #region ------------------创建数据库--------------------
            string rootPath = Application.StartupPath;
            FileHelper.SaveFile("personal geodatabase files (*.mdb)|*.mdb|All files (*.*)|*.*", ref mdbDircetion, ref mdbName);
            Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.AccessWorkspaceFactory");
            IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);
            IWorkspaceName workspaceName = workspaceFactory.Create(mdbDircetion, mdbName, null, 0);
            string mdbPath = mdbDircetion + "\\" + mdbName;
            WSP = workspaceFactory.OpenFromFile(mdbPath, 0);

            try
            {
                //创建dataset
                ISpatialReferenceFactory spatialReferenceFactory = new SpatialReferenceEnvironmentClass();
                //ISpatialReference spatialReference = spatialReferenceFactory.CreateProjectedCoordinateSystem((int)esriSRProjCS4Type.esriSRProjCS_Xian1980_3_Degree_GK_Zone_38);
                feaDs = DatasetHelper.CreateFeatureDataset(WSP, "Ming", spatialReference);
            }
            catch (Exception ex)
            {
                feaDs = null;
                MessageBox.Show(ex.ToString());
            }
            #endregion ------------------创建数据库--------------------

            return feaDs;
        }


        /// <summary>
        /// 复制要素类到geodatabse中
        /// </summary>
        /// <param name="sourceWorkspace"></param>源要素类的位置
        /// <param name="targetWorkspace"></param>新要素类的位置
        /// <param name="nameOfSourceFeatureClass"></param>
        /// <param name="nameOfTargetFeatureClass"></param>
        public static void CopyFeaClsToDataset(IWorkspace sourceWorkspace, IFeatureDataset targetFeatureDs, string nameOfSourceFeatureClass, string nameOfTargetFeatureClass)
        {
            //create source workspace name
            IDataset sourceWorkspaceDataset = (IDataset)sourceWorkspace;
            IWorkspaceName sourceWorkspaceName = (IWorkspaceName)sourceWorkspaceDataset.FullName;


            //create source dataset name
            IFeatureClassName sourceFeatureClassName = new FeatureClassNameClass();
            IDatasetName sourceDatasetName = (IDatasetName)sourceFeatureClassName;
            sourceDatasetName.WorkspaceName = sourceWorkspaceName;
            sourceDatasetName.Name = nameOfSourceFeatureClass;


            IFeatureDatasetName feadsName = new FeatureDatasetNameClass();
            feadsName = (IFeatureDatasetName)targetFeatureDs.FullName;
            
            //create target workspace name
            IDataset targetWorkspaceDataset = (IDataset)targetFeatureDs;
            IWorkspaceName targetWorkspaceName = new WorkspaceNameClass();
            targetWorkspaceName.WorkspaceFactoryProgID = "esriDataSourcesGDB.AccessWorkspaceFactory";
            targetWorkspaceName.PathName = targetFeatureDs.Workspace.PathName;

            //create target dataset name
            IFeatureClassName targetFeatureClassName = new FeatureClassNameClass();
            IDatasetName targetDatasetName = (IDatasetName)targetFeatureClassName;
            targetDatasetName.WorkspaceName = targetWorkspaceName;
            targetDatasetName.Name = nameOfTargetFeatureClass;


            //Open input Featureclass to get field definitions.
            ESRI.ArcGIS.esriSystem.IName sourceName = (ESRI.ArcGIS.esriSystem.IName)sourceFeatureClassName;
            IFeatureClass sourceFeatureClass = (IFeatureClass)sourceName.Open();


            //Validate the field names because you are converting between different workspace types.
            IFieldChecker fieldChecker = new FieldCheckerClass();
            IFields targetFeatureClassFields;
            IFields sourceFeatureClassFields = sourceFeatureClass.Fields;
            IEnumFieldError enumFieldError;


            // Most importantly set the input and validate workspaces!
            fieldChecker.InputWorkspace = sourceWorkspace;
            fieldChecker.ValidateWorkspace = targetFeatureDs.Workspace;
            fieldChecker.Validate(sourceFeatureClassFields, out enumFieldError, out targetFeatureClassFields);


            // Loop through the output fields to find the geomerty field
            IField geometryField;
            for (int i = 0; i < targetFeatureClassFields.FieldCount; i++)
            {
                if (targetFeatureClassFields.get_Field(i).Type == esriFieldType.esriFieldTypeGeometry)
                {
                    geometryField = targetFeatureClassFields.get_Field(i);
                    // Get the geometry field's geometry defenition
                    IGeometryDef geometryDef = geometryField.GeometryDef;


                    //Give the geometry definition a spatial index grid count and grid size
                    IGeometryDefEdit targetFCGeoDefEdit = (IGeometryDefEdit)geometryDef;


                    targetFCGeoDefEdit.GridCount_2 = 1;
                    targetFCGeoDefEdit.set_GridSize(0, 0); //Allow ArcGIS to determine a valid grid size for the data loaded
                    targetFCGeoDefEdit.SpatialReference_2 = geometryField.GeometryDef.SpatialReference;


                    // we want to convert all of the features
                    IQueryFilter queryFilter = new QueryFilterClass();
                    queryFilter.WhereClause = "";


                    // Load the feature class
                    IFeatureDataConverter fctofc = new FeatureDataConverterClass();
                    try
                    {

                        IEnumInvalidObject enumErrors = fctofc.ConvertFeatureClass(sourceFeatureClassName, queryFilter,
                            feadsName, targetFeatureClassName, geometryDef, targetFeatureClassFields, "", 1000, 0);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    break;
                }
            }
        }

       

        /// <summary>
        /// 复制要素类到geodatabse中
        /// </summary>
        /// <param name="sourceWorkspace"></param>源要素类的位置
        /// <param name="targetWorkspace"></param>新要素类的位置
        /// <param name="nameOfSourceFeatureClass"></param>
        /// <param name="nameOfTargetFeatureClass"></param>
        public static void CopyFeaCls2Workspace(IWorkspace sourceWorkspace, IWorkspace targetWorkspace, 
            string nameOfSourceFeatureClass, string nameOfTargetFeatureClass)
        {
            //create source workspace name
            IDataset sourceWorkspaceDataset = (IDataset)sourceWorkspace;
            IWorkspaceName sourceWorkspaceName = (IWorkspaceName)sourceWorkspaceDataset.FullName;


            //create source dataset name
            IFeatureClassName sourceFeatureClassName = new FeatureClassNameClass();
            IDatasetName sourceDatasetName = (IDatasetName)sourceFeatureClassName;
            sourceDatasetName.WorkspaceName = sourceWorkspaceName;
            sourceDatasetName.Name = nameOfSourceFeatureClass;

            //create target workspace name
            IDataset targetWorkspaceDataset = (IDataset)targetWorkspace;
            IWorkspaceName targetWorkspaceName = (IWorkspaceName)targetWorkspaceDataset.FullName;


            //create target dataset name
            IFeatureClassName targetFeatureClassName = new FeatureClassNameClass();
            IDatasetName targetDatasetName = (IDatasetName)targetFeatureClassName;
            targetDatasetName.WorkspaceName = targetWorkspaceName;
            targetDatasetName.Name = nameOfTargetFeatureClass;


            //Open input Featureclass to get field definitions.
            ESRI.ArcGIS.esriSystem.IName sourceName = (ESRI.ArcGIS.esriSystem.IName)sourceFeatureClassName;
            IFeatureClass sourceFeatureClass = (IFeatureClass)sourceName.Open();


            //Validate the field names because you are converting between different workspace types.
            IFieldChecker fieldChecker = new FieldCheckerClass();
            IFields targetFeatureClassFields;
            IFields sourceFeatureClassFields = sourceFeatureClass.Fields;
            IEnumFieldError enumFieldError;


            // Most importantly set the input and validate workspaces!
            fieldChecker.InputWorkspace = sourceWorkspace;
            fieldChecker.ValidateWorkspace = targetWorkspace;
            fieldChecker.Validate(sourceFeatureClassFields, out enumFieldError, out targetFeatureClassFields);


            // Loop through the output fields to find the geomerty field
            IField geometryField;
            for (int i = 0; i < targetFeatureClassFields.FieldCount; i++)
            {
                if (targetFeatureClassFields.get_Field(i).Type == esriFieldType.esriFieldTypeGeometry)
                {
                    geometryField = targetFeatureClassFields.get_Field(i);
                    // Get the geometry field's geometry defenition
                    IGeometryDef geometryDef = geometryField.GeometryDef;


                    //Give the geometry definition a spatial index grid count and grid size
                    IGeometryDefEdit targetFCGeoDefEdit = (IGeometryDefEdit)geometryDef;


                    targetFCGeoDefEdit.GridCount_2 = 1;
                    targetFCGeoDefEdit.set_GridSize(0, 0); //Allow ArcGIS to determine a valid grid size for the data loaded
                    targetFCGeoDefEdit.SpatialReference_2 = geometryField.GeometryDef.SpatialReference;


                    // we want to convert all of the features
                    IQueryFilter queryFilter = new QueryFilterClass();
                    queryFilter.WhereClause = "";


                    // Load the feature class
                    IFeatureDataConverter fctofc = new FeatureDataConverterClass();
                    IEnumInvalidObject enumErrors = fctofc.ConvertFeatureClass(sourceFeatureClassName, queryFilter, null, targetFeatureClassName, geometryDef, targetFeatureClassFields, "", 1000, 0);
                    break;
                }
            }
        }



        /// <summary>
        /// 复制要素类到geodatabse中
        /// </summary>
        /// <param name="sourceWorkspace"></param>源要素类的位置
        /// <param name="targetWorkspace"></param>新要素类的位置
        /// <param name="nameOfSourceDataTable"></param>
        /// <param name="nameOfTargetDataTable"></param>
        public static void CopyDatatable2Workspace(IWorkspace sourceWorkspace, IWorkspace targetWorkspace,
            string nameOfSourceDataTable, string nameOfTargetDataTable)
        {
            //create source workspace name
            IDataset sourceWorkspaceDataset = (IDataset)sourceWorkspace;
            IWorkspaceName sourceWorkspaceName = (IWorkspaceName)sourceWorkspaceDataset.FullName;


            //create source dataset name
            ITableName sourceTableName = new TableNameClass();


            IDatasetName sourceDatasetName = (IDatasetName)sourceTableName;
            sourceDatasetName.WorkspaceName = sourceWorkspaceName;
            sourceDatasetName.Name = nameOfSourceDataTable;

            //create target workspace name
            IDataset targetWorkspaceDataset = (IDataset)targetWorkspace;
            IWorkspaceName targetWorkspaceName = (IWorkspaceName)targetWorkspaceDataset.FullName;


            //create target dataset name
            ITableName targetTableName = new TableNameClass();
            IDatasetName targetDatasetName = (IDatasetName)targetTableName;
            targetDatasetName.WorkspaceName = targetWorkspaceName;
            targetDatasetName.Name = nameOfTargetDataTable;


            //Open input Featureclass to get field definitions.
            ESRI.ArcGIS.esriSystem.IName sourceName = (ESRI.ArcGIS.esriSystem.IName)sourceTableName;
            ITable sourceTable = (ITable)sourceName.Open();


            //Validate the field names because you are converting between different workspace types.
            IFieldChecker fieldChecker = new FieldCheckerClass();
            IFields targetFeatureClassFields;
            IFields sourceFeatureClassFields = sourceTable.Fields;
            IEnumFieldError enumFieldError;


            // Most importantly set the input and validate workspaces!
            fieldChecker.InputWorkspace = sourceWorkspace;
            fieldChecker.ValidateWorkspace = targetWorkspace;
            fieldChecker.Validate(sourceFeatureClassFields, out enumFieldError, out targetFeatureClassFields);


            // Loop through the output fields to find the geomerty field
            IField geometryField;
            for (int i = 0; i < targetFeatureClassFields.FieldCount; i++)
            {
                geometryField = targetFeatureClassFields.get_Field(i);
                // Get the geometry field's geometry defenition
                //IGeometryDef geometryDef = geometryField.GeometryDef;


                ////Give the geometry definition a spatial index grid count and grid size
                //IGeometryDefEdit targetFCGeoDefEdit = (IGeometryDefEdit)geometryDef;


                //targetFCGeoDefEdit.GridCount_2 = 1;
                //targetFCGeoDefEdit.set_GridSize(0, 0); //Allow ArcGIS to determine a valid grid size for the data loaded
                //targetFCGeoDefEdit.SpatialReference_2 = geometryField.GeometryDef.SpatialReference;


                // we want to convert all of the features
                IQueryFilter queryFilter = new QueryFilterClass();
                queryFilter.WhereClause = "";


                // Load the feature class
                IFeatureDataConverter fctofc = new FeatureDataConverterClass();
                IEnumInvalidObject enumErrors = fctofc.ConvertTable(sourceDatasetName, queryFilter,
                    targetDatasetName, targetFeatureClassFields, "", 1000, 0);
                break;
            }
        }


        //http://resources.esri.com/help/9.3/arcgisengine/dotnet/c45379b5-fbf2-405c-9a36-ea6690f295b2.htm
        #region -----------------------------exist error please debug----------------------------------
        //public static void ConverterFeaClsToDataset(IFeatureClass pFeaCls, IFeatureDataset targetDataset)
        //{
        //    //input 
        //    IWorkspaceName pSourceWorkspaceName = new WorkspaceNameClass() as IWorkspaceName;
        //    pSourceWorkspaceName.PathName = pFeaCls.FeatureDataset.Workspace.PathName;
        //    pSourceWorkspaceName.WorkspaceFactoryProgID = "esriDataSourcesGDB.AccessWorkspaceFactory";

        //    IFeatureClassName pSourceFCName = new FeatureClassNameClass() as IFeatureClassName;
        //    IDatasetName pSourceDatasetName = pSourceFCName as IDatasetName;
        //    pSourceDatasetName.Name = pFeaCls.AliasName;//this is your input file 
        //    pSourceDatasetName.WorkspaceName = pSourceWorkspaceName;

        //    IName sourceName = (IName)pSourceFCName;
        //    //IFeatureClass sourceFeatureClass = (IFeatureClass)sourceName.Open(); //fails
        //    IFeatureClass sourceFeatureClass = pFeaCls;
        //    IFields sourceFields = sourceFeatureClass.Fields;
        //    IGeometryDef pGeomDefEdit = new GeometryDefClass();

        //    for (int i = 0; i < sourceFields.FieldCount; i++)
        //    {
        //        IField pField = sourceFields.get_Field(i);
        //        if (pField.Name.ToUpper() == "SHAPE")
        //        {
        //            pGeomDefEdit = pField.GeometryDef;
        //        }
        //    }

        //    IWorkspaceName pTargetWorkspaceName = new WorkspaceNameClass() as IWorkspaceName;
        //    pSourceWorkspaceName.WorkspaceFactoryProgID = "esriDataSourcesGDB.AccessWorkspaceFactory";
        //    pSourceWorkspaceName.PathName = targetDataset.Workspace.PathName; //output file 

        //    IFeatureClassName pOutputFCName = new FeatureClassNameClass();
        //    IDatasetName pDataSetName = pOutputFCName as IDatasetName;
        //    pDataSetName.WorkspaceName = pSourceWorkspaceName;
        //    pDataSetName.Name = pFeaCls.AliasName;

        //    IFeatureDataConverter feaDataCon = new FeatureDataConverterClass();

        //    GeometryDef geoDef = pGeomDefEdit as GeometryDef;
        //    feaDataCon.ConvertFeatureClass(pSourceFCName, null, pSourceDatasetName as IFeatureDatasetName, pOutputFCName, geoDef, sourceFields, "", 1000, 0);
        //}

            
        //public static void TransferFeaClsToDataset(IFeatureDataset sourceDataset, IFeatureDataset targetDataset)
        //{

        //    #region set name
        //    // Create workspace name objects.
        //    IWorkspaceName sourceWorkspaceName = new WorkspaceNameClass();
        //    IWorkspaceName targetWorkspaceName = new WorkspaceNameClass();
        //    IName targetName = (IName)targetWorkspaceName;

        //    // Set the workspace name properties.
        //    sourceWorkspaceName.PathName = sourceDataset.Workspace.PathName;
        //    sourceWorkspaceName.WorkspaceFactoryProgID = "esriDataSourcesGDB.AccessWorkspaceFactory";

        //    //string[] path = targetDataset.Workspace.PathName.Split('\\');
        //    //targetWorkspaceName.PathName = @path[path.Length - 1];

        //    targetWorkspaceName.PathName = targetDataset.Workspace.PathName;
        //    targetWorkspaceName.WorkspaceFactoryProgID = "esriDataSourcesGDB.AccessWorkspaceFactory";


        //    // Create a name object for the source feature class.
        //    IFeatureClassName featureClassName = new FeatureClassNameClass();

        //    // Set the featureClassName properties.
        //    IDatasetName sourceDatasetName = (IDatasetName)featureClassName;
        //    sourceDatasetName.WorkspaceName = sourceWorkspaceName;
        //    sourceDatasetName.Name = sourceDataset.Name;
        //    IName sourceName = (IName)sourceDatasetName;
            
        //    #endregion set name

        //    //IName sourceName = sourceDataset.FullName;
        //    //IName targetName = targetDataset.FullName;


        //    // Create an enumerator for source datasets.
        //    IEnumName sourceEnumName = new NamesEnumeratorClass();
        //    IEnumNameEdit sourceEnumNameEdit = (IEnumNameEdit)sourceEnumName;

        //    // Add the name object for the source class to the enumerator.
        //    sourceEnumNameEdit.Add(sourceName);

        //    // Create a GeoDBDataTransfer object and a null name mapping enumerator.
        //    IGeoDBDataTransfer geoDBDataTransfer = new GeoDBDataTransferClass();
        //    IEnumNameMapping enumNameMapping = null;

        //    System.GC.Collect();
        //    System.GC.WaitForPendingFinalizers(); 
        //    // Use the data transfer object to create a name mapping enumerator.
        //    Boolean conflictsFound = geoDBDataTransfer.GenerateNameMapping(sourceEnumName, targetName, out enumNameMapping);
        //    //System.Runtime.InteropServices.Marshal.ReleaseComObject(pSomeCOMObject)
        //    enumNameMapping.Reset();

        //    // Check for conflicts.
        //    if (conflictsFound)
        //    {
        //        // Iterate through each name mapping.
        //        INameMapping nameMapping = null;
        //        while ((nameMapping = enumNameMapping.Next()) != null)
        //        {
        //            // Resolve the mapping's conflict (if there is one).
        //            if (nameMapping.NameConflicts)
        //            {
        //                nameMapping.TargetName = nameMapping.GetSuggestedName(targetName);
        //            }

        //            // See if the mapping's children have conflicts.
        //            IEnumNameMapping childEnumNameMapping = nameMapping.Children;
        //            if (childEnumNameMapping != null)
        //            {
        //                childEnumNameMapping.Reset();

        //                // Iterate through each child mapping.
        //                INameMapping childNameMapping = null;
        //                while ((childNameMapping = childEnumNameMapping.Next()) != null)
        //                {
        //                    if (childNameMapping.NameConflicts)
        //                    {
        //                        childNameMapping.TargetName = childNameMapping.GetSuggestedName(targetName);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    // Start the transfer.
        //    geoDBDataTransfer.Transfer(enumNameMapping, targetName);
        //}
        #endregion -----------------------------exist error please debug----------------------------------
    }
}
