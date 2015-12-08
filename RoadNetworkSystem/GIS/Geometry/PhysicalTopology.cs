using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace RoadNetworkSystem.GIS.Geometry
{
    /// <summary>
    /// 用于描述地理空间上的物理几何关系，不涉及逻辑连通关系描述和推理
    /// Modified by niuzhm 
    /// 2104-10-8
    /// </summary>
    class PhysicalTopology
    {
        /// <summary>
        /// 判断点要素是否在面要素上
        /// </summary>
        /// <param name="feaGon"></param>面要素
        /// <param name="feaPoint"></param>点要素
        /// <returns></returns>返回布尔型，如果在面要素上则返回false，否则返回true
        public static bool IsPointOnGon(IFeature feaGon, IFeature feaPoint)
        {
            bool onFlag;
            IRelationalOperator relationOperator;
            relationOperator = feaGon.Shape as IRelationalOperator;
            onFlag = relationOperator.Contains(feaPoint.Shape);
            return onFlag;
        }

        /// <summary>
        /// 返回一个线段相对另一个线段的数字化方向，1为相同，-1标识两个方向相反
        /// </summary>
        /// <param name="targetBoundaryFea"></param>目标
        /// <param name="corrArcFeature"></param>相应的Arc
        /// <returns></returns>
        public static int GetCorrepondseDir(IPolyline targetLine, IPolyline corrLine)
        {
            int tarBoundaryDir = 0;

            if (corrLine.Length > targetLine.Length)
            {
                //目标线段的起点到相应线段起点的距离
                double disTFandCF = PointHelper.GetDistance2Point(targetLine.FromPoint, corrLine.FromPoint);

                //目标线段的终点到相应线段的起点的距离
                double disTTandCF = PointHelper.GetDistance2Point(targetLine.ToPoint, corrLine.FromPoint);
                if (disTTandCF > disTFandCF)
                {
                    tarBoundaryDir = 1;
                }
                else
                {
                    tarBoundaryDir = -1;
                }
            }

            else
            {
                //目标线段的起点到相应线段起点的距离
                double disCFandTF = PointHelper.GetDistance2Point(targetLine.FromPoint, corrLine.FromPoint);

                //目标线段的终点到相应线段的起点的距离
                double disCTandTF = PointHelper.GetDistance2Point(targetLine.FromPoint, corrLine.ToPoint);
                if (disCTandTF > disCFandTF)
                {
                    tarBoundaryDir = 1;
                }
                else
                {
                    tarBoundaryDir = -1;
                }
            }
            return tarBoundaryDir;
        }

        /// <summary>
        /// 在一个指定的要素图层上，获取距离一个点最近的要素和距离
        /// </summary>
        /// <param name="pFeaCls"></param>指定的要素类
        /// <param name="point"></param>指定的点
        /// <param name="targetFeature"></param>返回值，获取的最近要素
        /// <param name="distance"></param>返回值，指定点与最近要素的距离
        public static void GetClosestFeature(IFeatureClass pFeaCls, IPoint point, ref IFeature targetFeature, ref double distance)
        {
            try
            {
                IGeoDataset pGDS = (IGeoDataset)pFeaCls;
                IFeatureIndex featureIndex;
                IIndexQuery indexQuery;
                featureIndex = new FeatureIndexClass();
                featureIndex.FeatureClass = pFeaCls;
                featureIndex.Index(null, pGDS.Extent);
                indexQuery = (IIndexQuery)featureIndex;
                int nearestFeaObjectID;
                indexQuery.NearestFeature(point, out nearestFeaObjectID, out distance);
                targetFeature = pFeaCls.GetFeature(nearestFeaObjectID);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        /// <summary>
        /// 获取电到直线最近的点
        /// </summary>
        /// <param name="pnt"></param>
        /// <param name="polyLine"></param>
        /// <returns></returns>
        public static IPoint GetNearestPointOnLine(IPoint pnt, IPolyline polyLine)
        {
            try
            {
                IProximityOperator pProximity = (IProximityOperator)polyLine;
                IPoint pNearestPoint = pProximity.ReturnNearestPoint(pnt, esriSegmentExtension.esriExtendEmbedded);
                return pNearestPoint;
            }
            catch (Exception Err)
            {
                MessageBox.Show(Err.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }

        }
       
        /// <summary>
        /// 获取指定范围内的要素
        /// </summary>
        /// <param name="pFeaCls"></param> 指定的要素类（）对应到图层
        /// <param name="centerPnt"></param> 圆心
        /// <param name="radius"></param> 半径
        /// <returns></returns> 区域内的要素
        public static List<IFeature> GetFeaInCircle(IFeatureClass pFeaCls, IPoint centerPnt, double radius)
        {
            #region
            #endregion

            #region
            List<IFeature> feaList = new List<IFeature>();

            IConstructCircularArc pConstructCircularArc = new CircularArcClass();
            pConstructCircularArc.ConstructCircle(centerPnt, radius, false);
            ICircularArc pArc = pConstructCircularArc as ICircularArc;

            ISegment pSegment1 = pArc as ISegment;

            //通过ISegmentCollection构建Ring对象
            ISegmentCollection pSegCollection = new RingClass();
            object o = Type.Missing;

            //添加Segement对象即圆
            pSegCollection.AddSegment(pSegment1, ref o, ref o);

            //QI到IRing接口封闭Ring对象，使其有效
            IRing pRing = pSegCollection as IRing;
            pRing.SpatialReference = centerPnt.SpatialReference;
            pRing.Close();
            //通过Ring对象使用IGeometryCollection构建Polygon对象
            IGeometryCollection pGeometryColl = new PolygonClass();
            pGeometryColl.AddGeometry(pRing, ref o, ref o);

            IPolygon gon = pGeometryColl as IPolygon;
            gon.SpatialReference = centerPnt.SpatialReference;
            IGeometry geo = gon as IGeometry;



            ISpatialFilter spatialFilter = new SpatialFilterClass();

            spatialFilter.Geometry = geo;//指定几何体

            String shpFld = pFeaCls.ShapeFieldName;

            spatialFilter.GeometryField = shpFld;

            spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;

            IQueryFilter queryFilter = new QueryFilterClass();

            queryFilter = (IQueryFilter)spatialFilter;

            IFeatureCursor searchCursor = pFeaCls.Search(queryFilter, false);

            IFeature feature = searchCursor.NextFeature();

            while (feature != null)
            {
                feaList.Add(feature);
                feature = searchCursor.NextFeature();
            }
            #endregion

            return feaList;
        }

    }
}
