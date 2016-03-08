using AxESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.GIS.Interactive
{
    class GeoDisplayHelper
    {
        public static void HightPoint(AxMapControl axMapControl1, IPoint pnt,int rColor,int gColor,
            int bColor,int size,esriSimpleMarkerStyle smapleStyle)
        {
            IElement _element;
            IMarkerElement pMarkerElement;//对于点，线，面的element定义这里都不一样，他是可实例化的类，而IElement是实例化的类，必须通过 IMarkerElement 初始化负值给 IElement 。
            pMarkerElement = new MarkerElementClass();

            ISimpleMarkerSymbol pSimpleMarkSymbol = new SimpleMarkerSymbolClass();
            RgbColor pColor = new RgbColor();
            pColor.Red = rColor;
            pColor.Green = gColor;
            pColor.Blue = bColor;
            pSimpleMarkSymbol.Color = pColor;
            pSimpleMarkSymbol.Size = size;
            pSimpleMarkSymbol.Style = smapleStyle;

            pMarkerElement.Symbol = pSimpleMarkSymbol;

            _element = pMarkerElement as IElement;
            _element.Geometry = pnt;//把你在屏幕中画好的图形付给 IElement 储存


            IMap pMap = axMapControl1.Map;
            IActiveView pActiveView = pMap as IActiveView;

            IGraphicsContainer pGraphicsContainer = pMap as IGraphicsContainer;
            pGraphicsContainer.AddElement(_element, 0);//显示储存在 IElement 中图形，这样就持久化了。

            pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);

        }


        public static IElement HightLine(AxMapControl axMapControl1, IPolyline line, int rColor, int gColor,
            int bColor, int size, esriSimpleLineStyle smapleStyle)
        {
            ILineElement pLineElement;
            IElement pLElement;

            RgbColor pColor = new RgbColor();
            pColor = new RgbColor();
            pColor.Red = rColor;
            pColor.Green = gColor;
            pColor.Blue = bColor;

            ISimpleLineSymbol pSimpleLineSymbol = new SimpleLineSymbolClass();
            pSimpleLineSymbol.Color = pColor;
            pSimpleLineSymbol.Width = size;
            pSimpleLineSymbol.Style = smapleStyle;

            pLineElement = new LineElementClass();
            pLineElement.Symbol = pSimpleLineSymbol;

            pLElement = pLineElement as IElement;
            pLElement.Geometry = line;

            IMap pMap = axMapControl1.Map;
            
            IGraphicsContainer pGraphicsContainer = pMap as IGraphicsContainer;
            pGraphicsContainer.AddElement(pLElement, 0);//把刚刚的element转到容器上  
            axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            return pLElement;
        }


        public static void Refresh(AxMapControl axMapControl1)
        {
            //刷新所有的被选中的东东
            IGraphicsContainer pGraphicsContainer = axMapControl1.ActiveView as IGraphicsContainer;//把地图的当前view作为图片的容器
            pGraphicsContainer.DeleteAllElements();//显示储存在 IElement 中图形，这样就持久化了。
            axMapControl1.Refresh();
        }

        public static void ClearElement(AxMapControl axMapControl1, IElement element)
        {
            if (null == element)
            {
                return;
            }
            IGraphicsContainer pGraphicsContainer = axMapControl1.ActiveView as IGraphicsContainer;//把地图的当前view作为图片的容器
            pGraphicsContainer.DeleteElement(element);
            axMapControl1.Refresh();
        }

        /// <summary>
        /// 把要素置于屏幕中心显示
        /// </summary>
        /// <param name="axMapControl"></param>
        /// <param name="targetGeo"></param>
        public static void DisplayAtCenter(AxMapControl axMapControl, IGeometry targetGeo)
        {
            axMapControl.ActiveView.Extent = targetGeo.Envelope;
            IPoint pnt = targetGeo as IPoint;
            axMapControl.CenterAt(pnt);
            axMapControl.ActiveView.Refresh();
        }

    }
}
