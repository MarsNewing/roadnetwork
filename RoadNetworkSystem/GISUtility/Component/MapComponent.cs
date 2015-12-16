using AxESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.SystemUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RoadNetworkSystem.GIS
{
    class MapComponent
    {
        public static IWorkspace OpenGeoDatabase(AxMapControl axMapControl1)
        {
            //清空所有的图层
            axMapControl1.ClearLayers();
            //把所有的要素类值置空

            ICommand pCommand = new ControlsAddDataCommandClass();
            pCommand.OnCreate(axMapControl1.Object);
            pCommand.OnClick();

            if (axMapControl1.LayerCount > 0)
            {
                ILayer layer = axMapControl1.Map.Layer[axMapControl1.Map.LayerCount - 1];
                return ((IDataset)(layer as IFeatureLayer).FeatureClass).Workspace;
            }
            else
            {
                return null;
            }
        }

        public static IWorkspace OpenArcMap(AxMapControl axMapControl1, string path)
        {
            OpenFileDialog dia = new OpenFileDialog();
            dia.Filter = "arcMap file(*.mxd)|*.mxd";
            dia.InitialDirectory = path;
            DialogResult dialogResult = dia.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                string mxdPath = dia.FileName;
                axMapControl1.LoadMxFile(mxdPath);
                if (axMapControl1.LayerCount > 0)
                {
                    ILayer layer = axMapControl1.get_Layer(axMapControl1.LayerCount - 1);
                    if ((layer as IFeatureLayer).FeatureClass == null)
                        return null;
                    return ((IDataset)(layer as IFeatureLayer).FeatureClass).Workspace;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }
}
