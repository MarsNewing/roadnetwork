using ESRI.ArcGIS.Geometry;
using System;
using System.Windows.Forms;

namespace RoadNetworkSystem.GIS.Geometry
{
    class PointHelper
    {

        /// <summary>
        /// 通过线性参考生成点
        /// </summary>
        /// <param name="line"></param>参考对象
        /// <param name="offset"></param>偏移距离
        /// <param name="measure"></param>距离起点的距离
        /// <returns></returns>
        public static IPoint CreatPointFromLineByLRS(IPolyline line, double offset, double measure)
        {
            IPolyline offsetline = LineHelper.CreatLineByOffset(line, offset);
            IPoint newPt = GetPointFromLine(offsetline, measure);
            return newPt;
        }


        /// <summary>
        /// 获取指定线上的点
        /// </summary>
        /// <param name="line"></param>参考线
        /// <param name="position"></param>距离
        /// <returns></returns>
        public static IPoint GetPointFromLine(IPolyline line, double position)
        {
            IPoint newPt = new Point();
            line.QueryPoint(esriSegmentExtension.esriExtendAtTo, position, false, newPt);
            return newPt;
        }


        /// <summary>
        /// 从屏幕坐标转为地理坐标
        /// </summary>
        /// <param name="screenPoint"></param>屏幕坐标
        /// <param name="activeView"></param>活动视图
        /// <returns></returns>
        public ESRI.ArcGIS.Geometry.IPoint GetMapCoordinatesFromScreenCoordinates(ESRI.ArcGIS.Geometry.IPoint screenPoint, ESRI.ArcGIS.Carto.IActiveView activeView)
        {

            if (screenPoint == null || screenPoint.IsEmpty || activeView == null)
            {
                return null;
            }
            ESRI.ArcGIS.Display.IScreenDisplay screenDisplay = activeView.ScreenDisplay;
            ESRI.ArcGIS.Display.IDisplayTransformation displayTransformation = screenDisplay.DisplayTransformation;

            return displayTransformation.ToMapPoint((System.Int32)screenPoint.X, (System.Int32)screenPoint.Y); // Explicit Cast
        }

        /// <summary>
        /// 从地图坐标转为屏幕坐标
        /// </summary>
        /// <param name="mapPnt"></param>地图坐标点
        /// <param name="activeView"></param>活动视图
        /// <returns></returns>
        public static System.Drawing.Point GetScreenCoordFromMap(IPoint mapPnt, ESRI.ArcGIS.Carto.IActiveView activeView)
        {
            if (mapPnt == null || mapPnt.IsEmpty || activeView == null)
            {
                return new System.Drawing.Point(0, 0);
            }
            ESRI.ArcGIS.Display.IScreenDisplay screenDisplay = activeView.ScreenDisplay;
            ESRI.ArcGIS.Display.IDisplayTransformation displayTransformation = screenDisplay.DisplayTransformation;
            int x, y;
            displayTransformation.FromMapPoint(mapPnt, out x, out y);
            return new System.Drawing.Point(x, y);
        }

        /// <summary>
        /// 获取两点间的直线距离
        /// </summary>
        /// <param name="pnt1"></param>第一个点
        /// <param name="pnt2"></param>第二个点
        /// <returns></returns>两点间距离
        public static double GetDistance2Point(IPoint pnt1, IPoint pnt2)
        {
            double distance = 0;
            distance = GetDistanceByLonAndLat(pnt1.X, pnt1.Y, pnt2.X, pnt2.Y);
            return distance;
        }

        /// <summary>
        /// 计算两个坐标间距离
        /// </summary>
        /// <param name="point1_X"></param>第一个点横坐标
        /// <param name="point1_Y"></param>第一个点纵坐标
        /// <param name="point2_X"></param>第二个点横坐标
        /// <param name="point2_Y"></param>第二个点纵坐标
        /// <returns></returns>返回两个点间距离
        public static double GetDistanceByLonAndLat(double point1_X, double point1_Y, double point2_X, double point2_Y)
        {
            double longitude = 0;
            double latitude = 0;

            longitude = point1_X - point2_X;
            latitude = point1_Y - point2_Y;
            double distance_meter = Math.Sqrt(longitude * longitude + latitude * latitude);

            return distance_meter;
        }
    }
}
