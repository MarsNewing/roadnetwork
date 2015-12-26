using AxESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace IntersectionModel.GIS
{
    class GeoDrawingHelper
    {
        /// <summary>
        /// 在AxMapControl上绘制一条多线段
        /// </summary>
        /// <param name="axMapControl1"></param>
        /// <returns></returns>
        public static IPolyline DrawLine(AxMapControl axMapControl1)
        {
            ILineElement pLineElement;
            IElement pLElement;
            IPolyline pLine;
            RgbColor pColor = new RgbColor();
            pColor.Red = 0;
            pColor.Green = 150;
            pColor.Blue = 255;

            ISimpleLineSymbol pSimpleLineSymbol = new SimpleLineSymbolClass();
            pSimpleLineSymbol.Color = pColor;
            pSimpleLineSymbol.Width = 3;

            pLineElement = new LineElementClass();
            pLineElement.Symbol = pSimpleLineSymbol;

            pLElement = pLineElement as IElement;

            IRubberBand pRubberBand;
            pRubberBand = new RubberLineClass();
            pLine = pRubberBand.TrackNew(axMapControl1.ActiveView.ScreenDisplay, null) as IPolyline;

            pLElement.Geometry = pLine;

            IGraphicsContainer pGraphicsContainer;
            pGraphicsContainer = axMapControl1.ActiveView as IGraphicsContainer;//把地图的当前view作为图片的容器  

            pGraphicsContainer.AddElement(pLElement, 0);//把刚刚的element转到容器上  
            axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);

            return pLine;
        }






        public static IElement DrawPoint(AxMapControl axMapControl1)
        {
            //http://resources.arcgis.com/en/help/arcobjects-net/componenthelp/index.html#//00490000009s000000
            IMarkerElement pMarkerElement;//对于点，线，面的element定义这里都不一样，他是可实例化的类，而IElement是实例化的类，必须通过 IMarkerElement 初始化负值给 IElement 。

            IElement pMElement;

            pMarkerElement = new MarkerElementClass();
            pMElement = pMarkerElement as IElement;

            RubberPointClass pRubberBand = new RubberPointClass(); //你的RUBBERBAND随着你的图形耳边
            IPoint pPoint = pRubberBand.TrackNew(axMapControl1.ActiveView.ScreenDisplay, null) as IPoint;

            pMElement.Geometry = pPoint;//把你在屏幕中画好的图形付给 IElement 储存
            IGraphicsContainer pGraphicsContainer = axMapControl1.ActiveView as IGraphicsContainer;//把地图的当前view作为图片的容器
            pGraphicsContainer.AddElement(pMElement, 0);//显示储存在 IElement 中图形，这样就持久化了。


            axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            return pMElement;
        }

        /// <summary>
        /// 捕捉控制点
        /// </summary>
        /// <param name="axMapControl1"></param>
        /// <param name="featureClass"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static IPoint Snapping(AxMapControl axMapControl1, IFeature pFeature, double x, double y)
        {
            IMap map = axMapControl1.Map;
            IActiveView activeView = axMapControl1.ActiveView;
            IPoint point = new PointClass();
            point.PutCoords(x, y);

            IPoint hitPoint1 = new PointClass();
            IPoint hitPoint2 = new PointClass();
            IHitTest hitTest = pFeature.Shape as IHitTest;
            double hitDist = 0;
            int partIndex = 0;
            int vertexIndex = 0;
            bool bVertexHit = false;

            double tol = ConvertPixelsToMapUnits(activeView, 8);
            if (hitTest.HitTest(point, tol, esriGeometryHitPartType.esriGeometryPartBoundary,
                hitPoint2, ref hitDist, ref partIndex, ref vertexIndex, ref bVertexHit))
            {
                hitPoint1 = hitPoint2;
            }
            axMapControl1.ActiveView.Refresh();
            return hitPoint1;
        }

        //转换像素到地图单位
        public static double ConvertPixelsToMapUnits(IActiveView activeView, double pixelUnits)
        {
            double realDisplayExtent;
            int pixelExtent;
            double sizeOfOnePixel;
            pixelExtent = activeView.ScreenDisplay.DisplayTransformation.get_DeviceFrame().right - activeView.ScreenDisplay.DisplayTransformation.get_DeviceFrame().left;
            realDisplayExtent = activeView.ScreenDisplay.DisplayTransformation.VisibleBounds.Width;
            sizeOfOnePixel = realDisplayExtent / pixelExtent;
            return pixelUnits * sizeOfOnePixel;
        }


        public static void CreateMarkerElement(AxMapControl axMapControl1, ref IMovePointFeedback movePointFeedback, IElement m_element, IPoint point)
        {
            IActiveView activeView = axMapControl1.ActiveView;
            IGraphicsContainer graphicsContainer = axMapControl1.Map as IGraphicsContainer;
            //建立一个marker元素
            IMarkerElement markerElement = new MarkerElement() as IMarkerElement;
            ISimpleMarkerSymbol simpleMarkerSymbol = new SimpleMarkerSymbol();
            //符号化元素
            IRgbColor rgbColor1 = new RgbColor();
            rgbColor1.Red = 255;
            rgbColor1.Blue = 0;
            rgbColor1.Green = 0;
            simpleMarkerSymbol.Color = rgbColor1;
            IRgbColor rgbColor2 = new RgbColor();
            rgbColor2.Red = 0;
            rgbColor2.Blue = 255;
            rgbColor2.Green = 0;
            simpleMarkerSymbol.Outline = true;
            simpleMarkerSymbol.OutlineColor = rgbColor2 as IColor;
            simpleMarkerSymbol.OutlineSize = 1;
            simpleMarkerSymbol.Size = 5;
            simpleMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSCircle;
            ISymbol symbol = simpleMarkerSymbol as ISymbol;
            symbol.ROP2 = esriRasterOpCode.esriROPNotXOrPen;
            markerElement.Symbol = simpleMarkerSymbol;
            m_element = markerElement as IElement;
            m_element.Geometry = point as IGeometry;
            graphicsContainer.AddElement(m_element, 0);
            activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, m_element, null);
            IGeometry geometry = m_element.Geometry;
            movePointFeedback.Display = activeView.ScreenDisplay;
            movePointFeedback.Symbol = simpleMarkerSymbol as ISymbol;
            movePointFeedback.Start(geometry as IPoint, point);
        }

        //移动元素到新的位置
        public static void ElementMoveTo(AxMapControl axMapControl1, IMovePointFeedback movePointFeedback, IElement m_element, IPoint point)
        {
            //移动元素
            movePointFeedback.MoveTo(point);
            IGeometry geometry1 = null;
            IGeometry geometry2 = null;
            if (m_element != null)
            {
                geometry1 = m_element.Geometry;
                geometry2 = movePointFeedback.Stop();
                m_element.Geometry = geometry2;
                //更新该元素的位置
                axMapControl1.ActiveView.GraphicsContainer.UpdateElement(m_element);
                //重新移动元素
                movePointFeedback.Stop();//(geometry1 as IPoint, nodePoint);
                axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
            }
        }
    }
}
