using AxESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.SystemUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RoadNetworkSystem.GIS
{
    class LayerHelper
    {
        /// <summary>
        /// 清除地图中被选

        /// </summary>
        /// <param name="axMapControl1"></param>
        public static void ClearSelect(AxMapControl axMapControl1)
        {
            IActiveView pActiveView = (IActiveView)(axMapControl1.Map);
            int i;
            for (i = 0; i <= axMapControl1.Map.LayerCount - 1; i++)     //除标志点外，其余图层不能被选择
            {
                pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, axMapControl1.get_Layer(i), null);
                axMapControl1.Map.ClearSelection();
                pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, axMapControl1.get_Layer(i), null);
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="axMapControl1"></param>
        /// <param name="str2"></param>
        public static void SelectLayer(AxMapControl axMapControl1, string str2)
        {
            ICommand cmd = new ControlsSelectFeaturesTool();       //Selects features by clicking or dragging a box. 
            cmd.OnCreate(axMapControl1.Object);
            axMapControl1.CurrentTool = cmd as ESRI.ArcGIS.SystemUI.ITool;

            int i;
            IFeatureLayer pFLayer;

            IFeatureLayer pFLayer0;
            IFeatureLayer pFLayer1;

            ICompositeLayer pComLayer0;
            ICompositeLayer pComLayer1;
            for (i = 0; i <= axMapControl1.Map.LayerCount - 1; i++)     //除标志点外，其余图层不能被选择
            {
                //单个图层
                if (axMapControl1.Map.get_Layer(i) is IFeatureLayer)
                {
                    pFLayer = axMapControl1.Map.get_Layer(i) as IFeatureLayer;
                    pFLayer.Selectable = true;
                    if (axMapControl1.Map.get_Layer(i).Name != str2)
                    {
                        pFLayer.Selectable = false;
                    }
                }
                //组合图层
                if (axMapControl1.Map.get_Layer(i) is ICompositeLayer)
                {
                    pComLayer0 = axMapControl1.Map.get_Layer(i) as ICompositeLayer;
                    for (int j = 0; j < pComLayer0.Count; j++)
                    {
                        //组合图层里的单个图层
                        if (pComLayer0.get_Layer(j) is IFeatureLayer)
                        {
                            pFLayer0 = pComLayer0.get_Layer(j) as IFeatureLayer;
                            pFLayer0.Selectable = true;
                            if (pFLayer0.Name != str2)
                            {
                                pFLayer0.Selectable = false;
                            }
                        }
                        //组合图层里仍有组合图层

                        if (pComLayer0.get_Layer(j) is ICompositeLayer)
                        {
                            pComLayer1 = pComLayer0.get_Layer(j) as ICompositeLayer;
                            for (int k = 0; k < pComLayer1.Count; k++)
                            {
                                pFLayer1 = pComLayer1.get_Layer(k) as IFeatureLayer;
                                pFLayer1.Selectable = true;
                                if (pFLayer1.Name != str2)
                                {
                                    pFLayer1.Selectable = false;
                                }
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="axMapControl1"></param>
        /// <param name="str2"></param>
        /// <returns></returns>
        public static ILayer GetLayerByName(AxMapControl axMapControl1, string str2)
        {
            int i;
            ILayer pLayer = axMapControl1.Map.get_Layer(0);
            ILayer temLayer = null;
            for (i = 0; i <= axMapControl1.Map.LayerCount - 1; i++)
            {
                if (axMapControl1.Map.get_Layer(i).Name == str2)
                {
                    pLayer = axMapControl1.Map.get_Layer(i);
                    temLayer = pLayer;
                }
            }
            if (temLayer == null)
            {
                string str1 = String.Format("找不到名为{0}的图层", str2);
                MessageBox.Show(str1);
            }
            return temLayer;

        }

        public static Hashtable GetLayerHashTb(AxMapControl axMapCtrl)
        {
            Hashtable layerHashTb = new Hashtable();

            ILayer pLayer = null;
            
            for (int i = 0; i <= axMapCtrl.Map.LayerCount - 1; i++)
            {
                string layName = "";

                layName=axMapCtrl.Map.get_Layer(i).Name;
                pLayer = axMapCtrl.Map.get_Layer(i);
                layerHashTb.Add(layName, pLayer);
            }
            return layerHashTb;
        }


        public static void LoadMapLayer(AxMapControl axMapCtrl, List<string> mapLayersName)
        {
            int layersCount = axMapCtrl.Map.LayerCount;
            List<int> corrLayerIndexs = new List<int>();
            List<string> layerNames = new List<string>();
            for (int i = 0; i < layersCount; i++)
            {
                string temLayerName = axMapCtrl.Map.Layer[i].Name;
                layerNames.Add(temLayerName);
                if (mapLayersName.Contains(temLayerName) == true)
                {
                    corrLayerIndexs.Add(i);
                }
                else
                {
                    continue;
                }
            }

            for (int j = 0; j < layersCount; j++)
            {
                if (corrLayerIndexs.Contains(j) == false)
                {
                    ILayer temLayer = GetLayerByName(axMapCtrl, layerNames[j]);
                    axMapCtrl.Map.DeleteLayer(temLayer);
                }
            }
        }

        public static ILayer GetSelectedLayer(IMap pMap)
        {

            ILayer pLayer = null;

            for (int i = 0; i <= pMap.LayerCount - 1; i++)
            {
                pLayer = pMap.get_Layer(i);
                IFeatureLayer feaLayer=pLayer as IFeatureLayer;
                if (feaLayer.Selectable == true)
                {
                   return pLayer;
                }

            }
            return null;
        }

        /// <summary>
        /// 所有图层不能被选中
        /// </summary>
        /// <param name="axMapControl1"></param>
        public static void LayerNotSelect(AxMapControl axMapControl1)
        {
            ICommand cmd = new ControlsSelectFeaturesTool();       //Selects features by clicking or dragging a box. 
            cmd.OnCreate(axMapControl1.Object);
            axMapControl1.CurrentTool = cmd as ESRI.ArcGIS.SystemUI.ITool;

            int i;
            IFeatureLayer pFLayer;

            IFeatureLayer pFLayer0;
            IFeatureLayer pFLayer1;

            ICompositeLayer pComLayer0;
            ICompositeLayer pComLayer1;
            for (i = 0; i <= axMapControl1.Map.LayerCount - 1; i++)     //除标志点外，其余图层不能被选择
            {
                //单个图层
                if (axMapControl1.Map.get_Layer(i) is IFeatureLayer)
                {
                    pFLayer = axMapControl1.Map.get_Layer(i) as IFeatureLayer;
                    pFLayer.Selectable = false;
                }

                //组合图层
                if (axMapControl1.Map.get_Layer(i) is ICompositeLayer)
                {
                    pComLayer0 = axMapControl1.Map.get_Layer(i) as ICompositeLayer;
                    for (int j = 0; j < pComLayer0.Count; j++)
                    {
                        //组合图层里的单个图层
                        if (pComLayer0.get_Layer(j) is IFeatureLayer)
                        {
                            pFLayer0 = pComLayer0.get_Layer(j) as IFeatureLayer;
                            pFLayer0.Selectable = false;

                        }
                        //组合图层里仍有组合图层

                        if (pComLayer0.get_Layer(j) is ICompositeLayer)
                        {
                            pComLayer1 = pComLayer0.get_Layer(j) as ICompositeLayer;
                            for (int k = 0; k < pComLayer1.Count; k++)
                            {
                                pFLayer1 = pComLayer1.get_Layer(k) as IFeatureLayer;
                                pFLayer1.Selectable = false;

                            }
                        }
                    }
                }
            }

        }
    }
}
