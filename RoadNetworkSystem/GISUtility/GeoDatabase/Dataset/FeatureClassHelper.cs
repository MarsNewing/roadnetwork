using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.GIS.GeoDatabase.Dataset
{
    class FeatureClassHelper
    {
        public static List<IFeatureClass> GetFeaClsInAccess(string mdbPath, List<string> feaClsNames)
        {

            List<IFeatureClass> feaClsList = new List<IFeatureClass>();

            IAoInitialize m_AoInitialize = new AoInitialize();
            m_AoInitialize.Initialize(esriLicenseProductCode.esriLicenseProductCodeArcInfo);
            IWorkspaceFactory pWsFct = new AccessWorkspaceFactoryClass();
            IWorkspace pWks = pWsFct.OpenFromFile(mdbPath, 0);
            
            foreach(string item in feaClsNames)
            {
                 IFeatureClass pFeaCls = (pWks as IFeatureWorkspace).OpenFeatureClass(item);
                
                feaClsList.Add(pFeaCls);
            }
            return feaClsList;
        }



        public static List<IFeatureClass> GetFeaClsInSqlServer(IPropertySet propertySet, List<string> feaClsNames)
        {
            List<IFeatureClass> feaClsList = new List<IFeatureClass>();

            IAoInitialize m_AoInitialize = new AoInitialize();
            m_AoInitialize.Initialize(esriLicenseProductCode.esriLicenseProductCodeArcInfo);

            IWorkspaceFactory pWSF = new SdeWorkspaceFactoryClass();
            IWorkspace pWks = pWSF.Open(propertySet, 0);

            foreach (string item in feaClsNames)
            {
                IFeatureClass pFeaCls = (pWks as IFeatureWorkspace).OpenFeatureClass(item);
                feaClsList.Add(pFeaCls);
            }

            return feaClsList;
        }


        public static ITable GetTableByName(string mdbPath,string tableName)
        {

            IAoInitialize m_AoInitialize = new AoInitialize();
            m_AoInitialize.Initialize(esriLicenseProductCode.esriLicenseProductCodeArcInfo);
            IWorkspaceFactory pWsFct = new AccessWorkspaceFactoryClass();
            IWorkspace pWks = pWsFct.OpenFromFile(mdbPath, 0);
            ITable table = (pWks as IFeatureWorkspace).OpenTable(tableName);
            return table;
        }

        public static IFeatureClass CreateFeatureClass(IFeatureDataset pFeaDataSet, 
            string feaClsNm,
            esriGeometryType enuGeometryType,
            IFields pNewFields,
            string ConfigKeyWord)
        {

            IFeatureClass newFeaCls = null;

            IFeatureWorkspace pFeatWorkspace = pFeaDataSet.Workspace as IFeatureWorkspace;
            ISpatialReference pSpatialReference = (pFeaDataSet as IGeoDataset).SpatialReference;


            //判断设否有名为feaClsNm的数据集
            //有则返回要素类
            if (DatasetHelper.ExistDataset(pFeatWorkspace, feaClsNm))
            {
                newFeaCls = pFeatWorkspace.OpenFeatureClass(feaClsNm);
            }
            //无，则创建一个新的要素类
            else
            {
                IFieldEdit pFieldEdit;
                IField pField;

                //The IGeometryDefEdit interface is used when creating a GeometryDef object.
                //You would normally use this interface when defining a new feature class. 
                //创建几何类型的对象时使用
                //创建过程参考：http://resources.arcgis.com/en/help/arcobjects-net/componenthelp/index.html#//0025000003qq000000
                IGeometryDefEdit pGeomDefEdit;

                //The IObjectClassDescription interface provides configuration information for ArcCatalog 
                //and custom clients to use when creating a new object class or feature class.
                //用ArcCatalog或客户端 创建新的对象或要素类时，使用
                IObjectClassDescription pObjectClassDescription = new FeatureClassDescription();
                IFieldsEdit pFieldsEdit = pObjectClassDescription.RequiredFields as IFieldsEdit;


                //修改图形类型，默认是面
                IFields pFields = pFieldsEdit;
                for (int i = 0; i < pFields.FieldCount; i++)
                {
                    pField = pFields.get_Field(i);
                    if (pField.Name.ToUpper() == "SHAPE")
                    {
                        pGeomDefEdit = pField.GeometryDef as IGeometryDefEdit;
                        pFieldEdit = pField as IFieldEdit;
                        pGeomDefEdit.GeometryType_2 = enuGeometryType;
                        pGeomDefEdit.GridCount_2 = 1;
                        pGeomDefEdit.set_GridSize(0, 1000);
                        //pGeomDefEdit.HasZ_2 = true;
                        if (pSpatialReference != null)
                        {
                            pGeomDefEdit.SpatialReference_2 = pSpatialReference;
                            if (pGeomDefEdit.SpatialReference != null)
                            {
                                //pGeomDefEdit.SpatialReference.SetZDomain(-1000, 60000);
                            }
                        }

                        pFieldEdit.GeometryDef_2 = pGeomDefEdit;
                    }
                }

                //添加 自定义的字段
                for (int i = 0; i < pNewFields.FieldCount; i++)
                {
                    try
                    {
                        if (pFieldsEdit.FindField(pNewFields.get_Field(i).Name) < 0 && pNewFields.get_Field(i).Type != esriFieldType.esriFieldTypeOID)
                        {
                            IFieldEdit pNewFieldEdit = pNewFields.get_Field(i) as IFieldEdit;
                            pNewFieldEdit.IsNullable_2 = true;
                            pFieldsEdit.AddField(pNewFieldEdit);
                        }
                    }
                    catch { }
                }

                if (pFeaDataSet == null)
                {
                    newFeaCls = pFeatWorkspace.CreateFeatureClass(feaClsNm, pFieldsEdit, null, null, esriFeatureType.esriFTSimple, "Shape", ConfigKeyWord);
                }
                else
                {
                    newFeaCls = pFeaDataSet.CreateFeatureClass(feaClsNm, pFieldsEdit, null, null, esriFeatureType.esriFTSimple, "Shape", ConfigKeyWord);
                }
                IClassSchemaEdit pClassSchemaEdit = (IClassSchemaEdit)newFeaCls;
                pClassSchemaEdit.AlterAliasName(feaClsNm);
            }
            return newFeaCls;
        }



        /// <summary>
        /// 创建一个字段
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="FieldType"></param>
        /// <param name="Length">长度，实数包括小数位数</param>
        /// <param name="FieldScale">小数位数</param>
        /// <param name="AliasName"></param>
        /// <param name="blnEditable"></param>
        /// <param name="IsNullable"></param>
        /// <returns></returns>
        public static IFieldEdit2 CreateField(string FieldName,
                                                esriFieldType FieldType,
                                                int Length,
                                                int FieldScale,
                                                string AliasName,
                                                Boolean blnEditable,
                                                Boolean IsNullable)
        {
            IFieldEdit2 pFieldEdit = new ESRI.ArcGIS.Geodatabase.Field() as IFieldEdit2;
            pFieldEdit.Name_2 = FieldName;

            pFieldEdit.Type_2 = FieldType;
            if (FieldType == esriFieldType.esriFieldTypeString)
                pFieldEdit.Length_2 = Length;

            if (FieldType == esriFieldType.esriFieldTypeDouble || FieldType == esriFieldType.esriFieldTypeSingle)
            {
                //pFieldEdit.Precision_2 = Length;
                pFieldEdit.Scale_2 = FieldScale;
            }
            else
            {

            }

            if (AliasName != "")
                pFieldEdit.AliasName_2 = AliasName;


            pFieldEdit.Editable_2 = blnEditable;
            pFieldEdit.IsNullable_2 = IsNullable;

            return pFieldEdit;
        }
    }
}
