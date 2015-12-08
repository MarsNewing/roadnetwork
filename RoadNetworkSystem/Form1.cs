using AxESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SystemUI;
using IntersectionModel.GIS;
using RoadNetworkSystem.DataModel.LaneBasedNetwork;
using RoadNetworkSystem.DataModel.Road;
using RoadNetworkSystem.DataModel.RoadSign;
using RoadNetworkSystem.GIS;
using RoadNetworkSystem.GIS.Interactive;
using RoadNetworkSystem.NetworkEditor;
using RoadNetworkSystem.NetworkElement.LaneBasedNetwork.RoadSegmentLayer;
using RoadNetworkSystem.NetworkElement.RoadLayer;
using RoadNetworkSystem.NetworkExtraction.FreeWay;
using RoadNetworkSystem.NetworkExtraction.FreeWay.Service;
using RoadNetworkSystem.NetworkExtraction.LaneBasedNetwork.DatabaseManager;
using RoadNetworkSystem.ParamicsDataTransform;
using RoadNetworkSystem.VissimDataTransform;
using RoadNetworkSystem.WinForm.EditTool;
using RoadNetworkSystem.WinForm.NetworkEditor;
using RoadNetworkSystem.WinForm.NetworkExtraction;
using RoadNetworkSystem.WinForm.RuleSetting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RoadNetworkSystem
{
    public partial class Form1 : Form
    {
        #region ArcGIS地图控件

            private ESRI.ArcGIS.Controls.IMapControl3 _mapControl = null;
            private ESRI.ArcGIS.Controls.IPageLayoutControl2 _pageLayoutControl = null;
            private IMapDocument _mapDocument;
        
        #endregion ArcGIS地图控件
            
        #region ****************操作标志*******************

            /// <summary>
            /// 用来标识选用的功能
            /// 值域位于FunEnum中
            /// 如果需要添加其他功能，请在该处添加功能对应枚举项
            /// </summary>
            private int funFlag = -1;

            /// <summary>
            /// 枚举出系统所有的功能
            /// </summary>
            private enum FunEnum
            {
                /// <summary>
                /// 路网编辑
                /// </summary>
                NetworkEditor,

                /// <summary>
                /// 编辑工具
                /// </summary>
                EditorTool,

                /// <summary>
                /// 转换
                /// </summary>
                Transform,

                /// <summary>
                /// 校验
                /// </summary>
                DataRectify,

                /// <summary>
                /// 路网提取
                /// </summary>
                NetworkExtraction,

                /// <summary>
                /// 规则设定
                /// </summary>
                RuleSetting


            }

            /// <summary>
            /// 标识是否使用工具栏上的工具
            /// </summary>
            public bool ToolBarFlag = false;

            RuleSettingDesigner ruleSettingDesigner;

            #endregion ****************操作标志*******************

        #region ****************数据库*******************

        /// <summary>
        /// 数据库所在文件夹的目录
        /// </summary>
        public string solutionPath = "";

        /// <summary>
        /// 数据库的名称
        /// </summary>
        public string MdbName = "BasicNetwork.mdb";   //BasicNetwork

        /// <summary>
        /// 数据库的绝对路径
        /// </summary>
        private string _mdbPath;

        public string MdbPath
        {
            get { return _mdbPath; }
        }


        private string _rootPath;

        /// <summary>
        /// 数据库对应的工作空间,
        /// </summary>
        private IWorkspace _pWsp;

        /// <summary>
        /// 设置工作空间的时候， 更新文件路径，并跟更新所有的要素类
        /// </summary>
        public IWorkspace Wsp
        {
            set 
            {
                _pWsp = value;
                //更新文件路径
                _mdbPath = _pWsp.PathName;
                //更新要素类
                getAllFeaClses(_pWsp);
            }
            get { return _pWsp; }
        }

        /// <summary>
        /// 数据库中的要素数据集
        /// </summary>
        public IFeatureDataset FeaDs;


            #region 要素类
            public IFeatureClass FeaClsRoad;
            public IFeatureClass FeaClsSegment;
            public IFeatureClass FeaClsSegNode;

            public IFeatureClass FeaClsNode;
            public IFeatureClass FeaClsArc;
            public IFeatureClass FeaClsLink;

            public IFeatureClass FeaClsLane;
            public IFeatureClass FeaClsConnector;
            public IFeatureClass FeaClsBoundary;

            public IFeatureClass FeaClsKerb;
            public IFeatureClass FeaClsTurnArrow;
            public IFeatureClass FeaClsStopLine;

            public IFeatureClass FeaClsSurface;
            #endregion 要素类

        #endregion ****************geodatabase*******************

        #region *******************路网编辑创建几何*********************
            /// <summary>
            /// 创建的Node几何点
            /// </summary>
            public IPoint CrtPnt = null;

            /// <summary>
            /// 只保存两个点
            /// </summary>
            public int SlctPntIndex = 0;

            /// <summary>
            /// 连接两个Node，用于生成Link
            /// </summary>
            public IFeature FNodeFea = null;
            public IFeature TNodeFea = null;

            /// <summary>
            /// 新生成的Link
            /// </summary>
            public IFeature CrttLinkFea = null;

            /// <summary>
            /// 用于生成Link所需的线型几何
            /// 在两个地方更新和赋值，（1）生成，（2）鼠标选择
            /// </summary>
            public IPolyline CrtLine = null;

            /// <summary>
            /// Node相邻Link哈希表
            /// </summary>
            public Hashtable AdjSegmentHsTb;
            #endregion *******************路网编辑创建几何*********************

        #region ************************路网编辑工具*************************

            /// <summary>
            /// 选择Road的编号
            /// </summary>
            public int SlctRoadIndex_EditTool = 0;
            public int SlctSegmentIndex_EditTool = 0;

            /// <summary>
            /// 编辑工具选择第一个Road要素，选择数量多余2个时，那就更第一个
            /// </summary>
            public IFeature FirstRoadFea = null;

            /// <summary>
            /// 编辑工具选择第二个Road要素
            /// </summary>
            public IFeature SecondRoadFea = null;

            /// <summary>
            /// 
            /// </summary>
            public IFeature FirstSegFea = null;
            public IFeature SecondSegFea = null;


            /// <summary>
            /// 选择的SegmentNode的要素
            /// </summary>
            public IFeature SltSegmentNode = null;

            public Boolean NodeSelected = false;
            #endregion ************************编辑工具*************************

        #region *******************路网提取***********************

            /// <summary>
            /// 选取的road对集
            /// </summary>
            public List<FeaPair> RoadFeaPairs = new List<FeaPair>();

            /// <summary>
            /// 选择的road对
            /// </summary>
            private FeaPair _roadFeaPair = null;

            /// <summary>
            /// 选择road对标志，
            /// FeaPairSlctFlag==0，需要选择第一个，FeaPairSlctFlag==1，需要选择第二个
            /// 选择第一个后FeaPairSlctFlag置为1，选择第二个后置为-1
            /// </summary>
            public int FeaPairSlctFlag = 0;

            #endregion *******************路网提取***********************


        #region *********************** 规则 ***********************
            public IFeature SlctArc_Rule;
            public IFeature SlctLane_Rule;

        #endregion *********************** 规则 ***********************

        public Form1()
        {
            InitializeComponent();
            int i;
            _rootPath = Application.StartupPath;//一开始运行就寻找这个程序的路径
            string[] arrPath = _rootPath.Split('\\');//把路径按照“\\”分成几部分

            _mdbPath = _rootPath +"\\"+ MdbName;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.axTOCControl1.SetBuddyControl(this.axMapControl1);

            #region ------------------------------直接载入mxd文件----------------------------------------
            //从根目录中拿到地图文件
            string[] filePaths = Directory.GetFiles(_rootPath, "*.mxd");
            if (filePaths != null && filePaths.Length > 0)
            {
                string mapPath = filePaths[0];
                axMapControl1.LoadMxFile(mapPath);  //BasicNetwork
            }
            #endregion ------------------------------直接载入mxd文件----------------------------------------

            _mapControl = (IMapControl3)this.axMapControl1.Object;//取得axMapControl1的“控制”
            //_pageLayoutControl = (IPageLayoutControl2)this.axPageLayoutControl1.Object;//取得axPageLayoutControl1的“控制”


            #region ———————————————初始化要素类———————————————————————
            IAoInitialize m_AoInitialize = new AoInitialize();
            m_AoInitialize.Initialize(esriLicenseProductCode.esriLicenseProductCodeArcInfo);

            IWorkspaceFactory pWsf = new AccessWorkspaceFactoryClass();
            _pWsp = pWsf.OpenFromFile(_mdbPath, 0);
            getAllFeaClses(_pWsp);
            #endregion ———————————————初始化要素类———————————————————————


            #region ———————————————设置右侧折叠框———————————————————————
            int dis = splitContainer3.SplitterDistance;
            int spP2W = splitContainer4.Panel2.Width;
            //设置右侧边框
            button1.Tag = 0;
            button1.Text = "<<";
            splitContainer4.Panel2Collapsed = true;
            splitContainer3.SplitterDistance = dis + spP2W;
            #endregion ———————————————设置右侧折叠框———————————————————————
        }

        #region ******************菜单栏***********************
            #region ------------------文件-----------------------

            private void 打开mdbToolStripMenuItem_Click(object sender, EventArgs e)
            {
                IWorkspace temWorkSpace = this.Wsp = MapComponent.OpenGeoDatabase(this.axMapControl1);
                if (temWorkSpace == null)
                {
                    MessageBox.Show("选的mdb文件不存在要素类，请确认");
                }
                else
                {
                    this.Wsp = temWorkSpace;
                    getAllFeaClses(Wsp);
                }
            }


            private void 打开地图ToolStripMenuItem_Click(object sender, EventArgs e)
            {
                
                IWorkspace temWorkSpace =MapComponent.OpenArcMap(this.axMapControl1, _rootPath);
                if (temWorkSpace == null)
                {
                    MessageBox.Show("选的mdb文件不存在要素类，请确认");
                }
                else
                {
                    this.Wsp = temWorkSpace;
                    getAllFeaClses(Wsp);
                }
                
            }

            /// <summary>
            /// 新建一个空的地理数据库
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void 空数据库ToolStripMenuItem_Click(object sender, EventArgs e)
            {
                _pWsp = DatabaseDesigner.CreateGDGDialoge(ref solutionPath, ref MdbName, ref _mdbPath);
            }

            /// <summary>
            /// 新建一个基础路网数据库
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void 基础路网数据库ToolStripMenuItem_Click(object sender, EventArgs e)
            {

            }

            /// <summary>
            /// 新建一个仿真路网数据库
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void 仿真路网数据库ToolStripMenuItem_Click(object sender, EventArgs e)
            {
                _pWsp = DatabaseDesigner.CreateGDGDialoge(ref solutionPath, ref MdbName, ref _mdbPath);
                DatabaseDesigner.CreateSimNetworkDb(_pWsp);
                addMayLayers(_pWsp);
            }


            /// <summary>
            /// 新建一个指路标志路网数据库
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void 指路标志路网数据库ToolStripMenuItem_Click(object sender, EventArgs e)
            {
                _pWsp = DatabaseDesigner.CreateGDGDialoge(ref solutionPath, ref MdbName, ref _mdbPath);
            }


            private void 保存地图ToolStripMenuItem_Click(object sender, EventArgs e)
            {
                // 首先确认当前地图文档是否有效
                if (null != _pageLayoutControl.DocumentFilename && _mapControl.CheckMxFile(_pageLayoutControl.DocumentFilename))
                {
                    // 创建一个新的地图文档实例
                    IMapDocument mapDoc = new MapDocumentClass();
                    // 打开当前地图文档
                    mapDoc.Open(_pageLayoutControl.DocumentFilename, string.Empty);
                    // 用 PageLayout 中的文档替换当前文档中的 PageLayout 部分
                    mapDoc.ReplaceContents((IMxdContents)_pageLayoutControl.PageLayout);
                    // 保存地图文档
                    mapDoc.Save(mapDoc.UsesRelativePaths, false);
                    mapDoc.Close();
                }
            }


            private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
            {
                Application.Exit();
            }
            #endregion ------------------文件-----------------------

            #region ------------路网提取（指路标志路网，车道级路网路段层）------------

            private void 路网提取ToolStripMenuItem_Click(object sender, EventArgs e)
            {
                funFlag = Convert.ToInt32(FunEnum.NetworkExtraction);
                LayerHelper.ClearSelect(axMapControl1);
                int dis = splitContainer3.SplitterDistance;
                int spP2W = splitContainer4.Panel2.Width;
                button1.Text = ">>";
                splitContainer4.Panel2Collapsed = false;
                splitContainer3.SplitterDistance = this.Width * 3 / 5;
                //splitContainer4.Panel2.Controls.Clear();

                ExtractionDesigner extrctDsgnr = new ExtractionDesigner(this);
                extrctDsgnr.SetExtractionPlatte();

                if (FeaClsRoad == null)
                {
                    MessageBox.Show("请打开Road图层");
                    Wsp = MapComponent.OpenArcMap(this.axMapControl1,_mdbPath);
                    getAllFeaClses(Wsp);
                }
                else
                {
                    System.Collections.Generic.List<string> layerNames = new System.Collections.Generic.List<string>();
                    layerNames.Add(RoadEntity.RoadNm);
                    LayerHelper.LoadMapLayer(axMapControl1, layerNames);
                }
            }

            #endregion ------------路网提取（指路标志路网，车道级路网路段层）------------

            #region ------------路网转换(vissim,Paramics,TransModeler与车道级路网间转换)------------
            private void vissimToolStripMenuItem1_Click(object sender, EventArgs e)
            {
                Basic2Vissim.CreateVissimdata(solutionPath, _mdbPath);
            }

            private void paramicsToolStripMenuItem1_Click(object sender, EventArgs e)
            {
                Basic2Paramics.CreateParamicsdata(solutionPath, _mdbPath);
            }

            private void transModelerToolStripMenuItem_Click(object sender, EventArgs e)
            {
                Basic2Vissim.CreateVissimdata(solutionPath, _mdbPath);
            }
            #endregion ------------路网转换------------

            #region ------------路网构建(仿真路网编辑器)------------

            private void 仿真路网构建ToolStripMenuItem_Click(object sender, EventArgs e)
            {
                funFlag = Convert.ToInt32(FunEnum.NetworkEditor);
                LayerHelper.ClearSelect(axMapControl1);
                int dis = splitContainer3.SplitterDistance;
                int spP2W = splitContainer4.Panel2.Width;
                button1.Text = ">>";
                splitContainer4.Panel2Collapsed = false;
                splitContainer3.SplitterDistance = this.Width * 3 / 5;

                NetworkEditorDesigner.LayoutNetworkEditor(this);
            }

            #endregion ------------路网构建------------

            #region 路网编辑工具

            private void 编辑工具ToolStripMenuItem_Click(object sender, EventArgs e)
            {
                //赋值，操作是编辑
                object o = (object)0;
                funFlag = Convert.ToInt32(FunEnum.EditorTool);
                EditToolDesigner edtToolD = new EditToolDesigner(this);
                edtToolD.SetEditToolPattle();

                int dis = splitContainer3.SplitterDistance;
                int spP2W = splitContainer4.Panel2.Width;
                button1.Text = ">>";
                splitContainer4.Panel2Collapsed = false;
                splitContainer3.SplitterDistance = this.Width * 3 / 5;
            }
            #endregion 

        #endregion ******************菜单栏***********************

        #region **********************数据管理**************************

        /// <summary>
        /// 获取所有的要素类
        /// </summary>
        /// <param name="pWSP"></param>
        public void getAllFeaClses(IWorkspace pWSP)
        {
            //清除所有的要素类值
            clearFeatureClass();
            IFeatureWorkspace feaWs = pWSP as IFeatureWorkspace;

            #region ++++++++++++++++++无要素集，数据库中直接存的是要素类++++++++++++++++++++++
            IEnumDataset enumDs;
            enumDs = _pWsp.get_Datasets(esriDatasetType.esriDTFeatureClass);        //从工作空间获取数据集 }
            IFeatureClass pFeaCls = enumDs.Next() as IFeatureClass;
            while (pFeaCls != null)
            {
                updateFeaCls(feaWs, pFeaCls.AliasName);
                pFeaCls = enumDs.Next() as IFeatureClass;
            }
            #endregion ++++++++++++++++++无要素集，数据库中直接存的是要素类++++++++++++++++++++++

            #region ++++++++++++++++++存在要素集，获取要素集中的所有要素++++++++++++++++++++++
            enumDs = _pWsp.get_Datasets(esriDatasetType.esriDTFeatureDataset);        //从工作空间获取数据集 }
            IFeatureDataset feaDs = enumDs.Next() as IFeatureDataset;

            if (feaDs != null)
            {
                IFeatureClassContainer feaClsCtn = feaDs as IFeatureClassContainer;
                IEnumFeatureClass enumFeaCls = feaClsCtn.Classes;
                IFeatureClass featureClass = enumFeaCls.Next();

                while (featureClass != null)
                {
                    updateFeaCls(feaWs, featureClass.AliasName);
                    featureClass = enumFeaCls.Next();
                }
            }
            #endregion ++++++++++++++++++存在要素集，获取要素集中的所有要素++++++++++++++++++++++
        }

        /// <summary>
        /// 清除所有的要素类值
        /// </summary>
        private void clearFeatureClass()
        {
            FeaClsRoad = null;
            FeaClsSegment = null;
            FeaClsSegNode = null;

            FeaClsNode = null;
            FeaClsArc = null;
            FeaClsLink = null;

            FeaClsLane = null;
            FeaClsConnector = null;
            FeaClsBoundary = null;

            FeaClsKerb = null;
            FeaClsTurnArrow = null;
            FeaClsStopLine = null;

            FeaClsSurface = null;
        }

        /// <summary>
        /// 更新要素类
        /// </summary>
        /// <param name="feaWs"></param>
        /// <param name="feaClsName"></param>
        private void updateFeaCls(IFeatureWorkspace feaWs, string feaClsName)
        {
            try
            {
                switch (feaClsName)
                {
                    case SegmentNodeEntity.RoadSegmentNodeName:
                        {
                            FeaClsSegNode = feaWs.OpenFeatureClass(SegmentNodeEntity.RoadSegmentNodeName);
                            break;
                        }
                    case SegmentEntity.SegmentName:
                        {
                            FeaClsSegment = feaWs.OpenFeatureClass(SegmentEntity.SegmentName);
                            break;
                        }
                    case NodeEntity.NodeName:
                        {
                            FeaClsNode = feaWs.OpenFeatureClass(NodeEntity.NodeName);
                            break;
                        }


                    case LinkEntity.LinkName:
                        {
                            FeaClsLink = feaWs.OpenFeatureClass(LinkEntity.LinkName);
                            break;
                        }
                    case ArcEntity.ArcFeatureName:
                        {
                            FeaClsArc = feaWs.OpenFeatureClass(ArcEntity.ArcFeatureName);
                            break;

                        }
                    case LaneEntity.LaneName:
                        {
                            FeaClsLane = feaWs.OpenFeatureClass(LaneEntity.LaneName);
                            break;
                        }


                    case LaneConnectorEntity.ConnectorName:
                        {
                            FeaClsConnector = feaWs.OpenFeatureClass(LaneConnectorEntity.ConnectorName);
                            break;
                        }
                    case BoundaryEntity.BoundaryName:
                        {
                            FeaClsBoundary = feaWs.OpenFeatureClass(BoundaryEntity.BoundaryName);
                            break;
                        }
                    case KerbEntity.KerbName:
                        {
                            FeaClsKerb = feaWs.OpenFeatureClass(KerbEntity.KerbName);
                            break;
                        }


                    case StopLineEntity.StopLineName:
                        {
                            FeaClsStopLine = feaWs.OpenFeatureClass(StopLineEntity.StopLineName);
                            break;
                        }
                    case TurnArrowEntity.TurnArrowName:
                        {
                            FeaClsTurnArrow = feaWs.OpenFeatureClass(TurnArrowEntity.TurnArrowName);
                            break;
                        }
                    case SurfaceEntity.SurfaceName:
                        {
                            FeaClsSurface = feaWs.OpenFeatureClass(SurfaceEntity.SurfaceName);
                            break;
                        }
                    case RoadEntity.RoadNm:
                        {
                            FeaClsRoad = feaWs.OpenFeatureClass(RoadEntity.RoadNm);
                            break;
                        }
                    default:
                        {

                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion **********************数据管理**************************

        #region **********************图层管理**************************

        /// <summary>
        /// 把数据库中的数据加载到窗口中
        /// </summary>
        /// <param name="pWSP"></param>
        private void addMayLayers(IWorkspace pWSP)
        {
            //移除现有图层
            axMapControl1.Map.ClearLayers();

            //添加新建的图层
            IEnumDataset enumDs;
            enumDs = pWSP.get_Datasets(esriDatasetType.esriDTFeatureDataset);        //从工作空间获取数据集 }
            IFeatureDataset feaDs = enumDs.Next() as IFeatureDataset;
            IFeatureClassContainer feaClsCtn = feaDs as IFeatureClassContainer;
            IEnumFeatureClass enumFeaCls = feaClsCtn.Classes;

            IFeatureClass featureClass = enumFeaCls.Next();
            while (featureClass != null)
            {
                IFeatureLayer feaLayer = new FeatureLayerClass();
                feaLayer.FeatureClass = featureClass;
                feaLayer.Name = featureClass.AliasName;
                axMapControl1.Map.AddLayer(feaLayer as ILayer);
                featureClass = enumFeaCls.Next();
            }
        }

        #endregion **********************图层管理**************************

        private void button1_Click(object sender, EventArgs e)
        {
            int tag = Convert.ToInt32(button1.Tag);
            int dis = splitContainer3.SplitterDistance;
            int spP2W = splitContainer4.Panel2.Width;
            switch (tag)
            {
                case 0:
                    {
                        button1.Tag = 1;
                        button1.Text = ">>";
                        splitContainer4.Panel2Collapsed = false;

                        splitContainer3.SplitterDistance = this.Width * 3 / 5;
                        break;

                    }
                case 1:
                    {
                        button1.Tag = 0;
                        button1.Text = "<<";
                        splitContainer4.Panel2Collapsed = true;
                        splitContainer3.SplitterDistance = dis + spP2W;
                        break;
                    }
                default:
                    {
                        break;
                    }

            }
        }


        private int queryAdjFeaIndex(Hashtable adjSegHstb, IFeature pFeature)
        {
            int index = -1;
            foreach (DictionaryEntry fea in adjSegHstb)
            {
                if ((fea.Value as IFeature).OID == pFeature.OID)
                {
                    index = Convert.ToInt32(fea.Key);
                }
                else
                {
                    continue;
                }
            }
            return index;
        }

        private void axToolbarControl1_OnMouseDown(object sender, AxESRI.ArcGIS.Controls.IToolbarControlEvents_OnMouseDownEvent e)
        {
            ToolBarFlag = true;
        }

        private void axMapControl1_OnMouseUp(object sender, IMapControlEvents2_OnMouseUpEvent e)
        {
            if (ToolBarFlag == true)
            { return; }
            switch (funFlag)
            {
                case (int)FunEnum.EditorTool:
                    {
                        #region 选择是Road
                        if (comBox_Tool_Layer.SelectedIndex == 0)
                        {

                            IEnumFeature pEnumFeat;
                            IFeature pFeature;
                            pEnumFeat = axMapControl1.Map.FeatureSelection as IEnumFeature;
                            pFeature = pEnumFeat.Next();
                            if (pEnumFeat.Next() != null)
                            {
                                MessageBox.Show("选择多余1个Road");
                            }
                            if (pFeature != null)
                            {

                                if (SlctRoadIndex_EditTool == 0)
                                {
                                    FirstRoadFea = pFeature;
                                    Road road = new Road(FeaClsRoad, 0);
                                    RoadEntity roadEty = new RoadEntity();
                                    roadEty = road.GetEntity(pFeature);
                                    object o = roadEty.RoadType;
                                    string text = "路名:" + roadEty.RoadName + "\n" +
                                        "道路类型:" + System.Enum.GetName(typeof(LinkEntity.道路类型), o).ToString();

                                    SlctRoadIndex_EditTool += 1;
                                    richTextBox_Tool_FirstFea.Text = text;

                                    GeoDisplayHelper.HightLine(axMapControl1, FirstRoadFea.Shape as IPolyline, 255, 0, 0, 10, esriSimpleLineStyle.esriSLSSolid);
                                    
                                }
                                else if (SlctRoadIndex_EditTool == 1)
                                {
                                    SecondRoadFea = pFeature;
                                    Road road = new Road(FeaClsRoad, 0);
                                    RoadEntity roadEty = new RoadEntity();
                                    roadEty = road.GetEntity(pFeature);
                                    object o = roadEty.RoadType;
                                    string text = "路名:" + roadEty.RoadName + "\n" +
                                        "道路类型:" + System.Enum.GetName(typeof(LinkEntity.道路类型), o).ToString();
                                    richTextBox_Tool_SecondFea.Text = text;
                                    SlctRoadIndex_EditTool = 0;
                                    GeoDisplayHelper.HightLine(axMapControl1, SecondRoadFea.Shape as IPolyline, 0, 255, 0, 10, esriSimpleLineStyle.esriSLSSolid);
                                }
                            }

                        }
                        #endregion 选择是Road

                        #region 选择的是segment
                        if (comBox_Tool_Layer.SelectedIndex == 1)
                        {
   
                            IEnumFeature pEnumFeat;
                            IFeature pFeature;
                            pEnumFeat = axMapControl1.Map.FeatureSelection as IEnumFeature;
                            pFeature = pEnumFeat.Next();
                            if (pEnumFeat.Next() != null)
                            {
                                MessageBox.Show("选择多余1个segment");
                            }
                            if (pFeature != null)
                            {

                                if (SlctSegmentIndex_EditTool == 0)
                                {
                                    FirstSegFea = pFeature;
                                    Segment seg = new Segment(FeaClsSegment, 0);
                                    SegmentEntity segEty = new SegmentEntity();

                                    LinkMasterEntity linkMstrEty = seg.GetEntity(pFeature);
                                    segEty = segEty.Copy(linkMstrEty);
                                    object o = segEty.RoadType;


                                    string text = "路名:" + segEty.RoadName + "\n" +
                                        "道路类型:" + System.Enum.GetName(typeof(LinkEntity.道路类型), o).ToString();

                                    SlctSegmentIndex_EditTool += 1;
                                    richTextBox_Tool_FirstFea.Text = text;
                                    GeoDisplayHelper.HightLine(axMapControl1, FirstSegFea.Shape as IPolyline, 255, 0, 0, 10, esriSimpleLineStyle.esriSLSSolid);
                                }
                                else if (SlctSegmentIndex_EditTool == 1)
                                {
                                    SecondSegFea = pFeature;
                                    Segment seg = new Segment(FeaClsSegment, 0);
                                    SegmentEntity segEty = new SegmentEntity();

                                    LinkMasterEntity linkMstrEty = seg.GetEntity(pFeature);

                                    segEty = segEty.Copy(linkMstrEty);
                                    object o = segEty.RoadType;

                                    string text = "路名:" + segEty.RoadName + "\n" +
                                        "道路类型:" + System.Enum.GetName(typeof(LinkEntity.道路类型), o).ToString();

                                    richTextBox_Tool_SecondFea.Text = text;
                                    SlctSegmentIndex_EditTool = 0;
                                    GeoDisplayHelper.HightLine(axMapControl1, SecondSegFea.Shape as IPolyline, 0, 255, 0, 10, esriSimpleLineStyle.esriSLSSolid);
                                }
                            }

                        }
                        #endregion 选择的是segment

                        #region 当选了点后再选择的是Segment
                        else if (comBox_Tool_Layer.SelectedIndex == 2 && NodeSelected == true)
                        {
                            IEnumFeature pEnumFeat;
                            IFeature pFeature;
                            pEnumFeat = axMapControl1.Map.FeatureSelection as IEnumFeature;
                            pFeature = pEnumFeat.Next();
                            if (pEnumFeat.Next() != null)
                            {
                                MessageBox.Show("选择多余1个Segment");
                            }
                            if (pFeature != null)
                            {
                                if (SlctSegmentIndex_EditTool == 0)
                                {
                                    FirstSegFea = pFeature;

                                    int seleIndex = queryAdjFeaIndex(AdjSegmentHsTb, FirstSegFea);
                                    if (seleIndex == 0)
                                    {
                                        GeoDisplayHelper.HightLine(axMapControl1, FirstSegFea.Shape as IPolyline,
                                            255, 0, 0, 10, esriSimpleLineStyle.esriSLSDashDot);
                                        richTextBox_Tool_FirstFea.Font = new System.Drawing.Font(richTextBox_Tool_FirstFea.Font.Name, richTextBox_Tool_FirstFea.Font.Size + 3);
                                    }
                                    if (seleIndex == 1)
                                    {
                                        GeoDisplayHelper.HightLine(axMapControl1, FirstSegFea.Shape as IPolyline,
                                            0, 255, 0, 10, esriSimpleLineStyle.esriSLSDashDot);
                                        richTextBox_Tool_SecondFea.Font = new System.Drawing.Font(richTextBox_Tool_FirstFea.Font.Name, richTextBox_Tool_FirstFea.Font.Size + 3);
                                    }
                                    if (seleIndex == 2)
                                    {
                                        GeoDisplayHelper.HightLine(axMapControl1, FirstSegFea.Shape as IPolyline,
                                            System.Drawing.Color.Pink.R, System.Drawing.Color.Pink.G, System.Drawing.Color.Pink.B, 10, esriSimpleLineStyle.esriSLSDashDot);

                                        richTextBox_Tool_ThirdFea.Font = new System.Drawing.Font(richTextBox_Tool_FirstFea.Font.Name, richTextBox_Tool_FirstFea.Font.Size + 3);
                                    }
                                    if (seleIndex == 3)
                                    {
                                        GeoDisplayHelper.HightLine(axMapControl1, FirstSegFea.Shape as IPolyline,
                                            0, 0, 255, 10, esriSimpleLineStyle.esriSLSDashDot);

                                        richTextBox_Tool_ForthFea.Font = new System.Drawing.Font(richTextBox_Tool_FirstFea.Font.Name, richTextBox_Tool_FirstFea.Font.Size + 3);
                                    }
                                    SlctSegmentIndex_EditTool += 1;
                                }
                                else if (SlctSegmentIndex_EditTool == 1)
                                {
                                    SecondSegFea = pFeature;
                                    int seleIndex = queryAdjFeaIndex(AdjSegmentHsTb, SecondSegFea);
                                    if (seleIndex == 0)
                                    {
                                        GeoDisplayHelper.HightLine(axMapControl1, SecondSegFea.Shape as IPolyline,
                                            255, 0, 0, 10, esriSimpleLineStyle.esriSLSDashDot);
                                        richTextBox_Tool_FirstFea.Font = new System.Drawing.Font(richTextBox_Tool_FirstFea.Font.Name, richTextBox_Tool_FirstFea.Font.Size + 3);
                                    }
                                    if (seleIndex == 1)
                                    {
                                        GeoDisplayHelper.HightLine(axMapControl1, SecondSegFea.Shape as IPolyline,
                                            0, 255, 0, 10, esriSimpleLineStyle.esriSLSDashDot);
                                        richTextBox_Tool_SecondFea.Font = new System.Drawing.Font(richTextBox_Tool_FirstFea.Font.Name, richTextBox_Tool_FirstFea.Font.Size + 3);
                                    }
                                    if (seleIndex == 2)
                                    {
                                        GeoDisplayHelper.HightLine(axMapControl1, SecondSegFea.Shape as IPolyline,
                                            System.Drawing.Color.Pink.R, System.Drawing.Color.Pink.G, System.Drawing.Color.Pink.B, 10, esriSimpleLineStyle.esriSLSDashDot);

                                        richTextBox_Tool_ThirdFea.Font = new System.Drawing.Font(richTextBox_Tool_FirstFea.Font.Name, richTextBox_Tool_FirstFea.Font.Size + 3);
                                    }
                                    if (seleIndex == 3)
                                    {
                                        GeoDisplayHelper.HightLine(axMapControl1, SecondSegFea.Shape as IPolyline,
                                            0, 0, 255, 10, esriSimpleLineStyle.esriSLSDashDot);

                                        richTextBox_Tool_ForthFea.Font = new System.Drawing.Font(richTextBox_Tool_FirstFea.Font.Name, richTextBox_Tool_FirstFea.Font.Size + 3);
                                    }
                                    SlctSegmentIndex_EditTool = 0;
                                }
                            }
                        }

                        #endregion 当选了点后再选择的是Segment
                        #region 选择是SegmentNode
                        else if (comBox_Tool_Layer.SelectedIndex == 2 && NodeSelected == false)
                        {
                            AdjSegmentHsTb = new Hashtable();
                            //刷新所有的被选中的东东
                            //IGraphicsContainer pGraphicsContainer = axMapControl1.ActiveView as IGraphicsContainer;//把地图的当前view作为图片的容器
                            //pGraphicsContainer.DeleteAllElements();//显示储存在 IElement 中图形，这样就持久化了。
                            //axMapControl1.Refresh();

                            IEnumFeature pEnumFeat;
                            IFeature pFeature;
                            pEnumFeat = axMapControl1.Map.FeatureSelection as IEnumFeature;
                            pFeature = pEnumFeat.Next();
                            if (pEnumFeat.Next() != null)
                            {
                                MessageBox.Show("选择多余1个SegmentNode");
                            }
                            if (pFeature != null)
                            {
                                SltSegmentNode = pFeature;
                                SegmentNode segNode = new SegmentNode(FeaClsSegNode, 0, null);
                                SegmentNodeEntity segNodeEty = new SegmentNodeEntity();
                                NodeMasterEntity nodeMstEty = segNode.GetNodeMasterEty(pFeature);
                                segNodeEty = segNodeEty.Copy(nodeMstEty);

                                string[] northAngles = segNodeEty.NorthAngles.Split('\\');
                                string[] adjSegmentIDs = segNodeEty.AdjIDs.Split('\\');
                                for (int i = 0; i < northAngles.Length; i++)
                                {
                                    int temSegmentID = Convert.ToInt32(adjSegmentIDs[i]);
                                    Segment seg = new Segment(FeaClsSegment, temSegmentID);
                                    IFeature fea1 = seg.GetFeature();
                                    SegmentEntity segEty = new SegmentEntity();
                                    LinkMasterEntity linkMstrEty = seg.GetEntity(fea1);
                                    segEty = segEty.Copy(linkMstrEty);
                                    object o = segEty.RoadType;
                                    string text = "北偏角为：" + northAngles[i] + "\n" +
                                        "道路名为：" + segEty.RoadName + "\n" +
                                        "道路类型：" + System.Enum.GetName(typeof(LinkEntity.道路类型), o).ToString();
                                    if (i == 0)
                                    {
                                        GeoDisplayHelper.HightLine(axMapControl1, fea1.Shape as IPolyline, 255, 0, 0, 10, esriSimpleLineStyle.esriSLSSolid);
                                        richTextBox_Tool_FirstFea.Text = text;
                                        AdjSegmentHsTb.Add(0, fea1);
                                    }
                                    else if (i == 1)
                                    {
                                        GeoDisplayHelper.HightLine(axMapControl1, fea1.Shape as IPolyline, 0, 255, 0, 10, esriSimpleLineStyle.esriSLSSolid);
                                        richTextBox_Tool_SecondFea.Text = text;
                                        AdjSegmentHsTb.Add(1, fea1);
                                    }
                                    else if (i == 2)
                                    {
                                        GeoDisplayHelper.HightLine(axMapControl1, fea1.Shape as IPolyline, System.Drawing.Color.Pink.R, System.Drawing.Color.Pink.G,
                                            System.Drawing.Color.Pink.B, 10, esriSimpleLineStyle.esriSLSSolid);
                                        richTextBox_Tool_ThirdFea.Text = text;
                                        AdjSegmentHsTb.Add(2, fea1);
                                    }
                                    else if (i == 3)
                                    {
                                        GeoDisplayHelper.HightLine(axMapControl1, fea1.Shape as IPolyline, 0, 0, 255, 10, esriSimpleLineStyle.esriSLSSolid);
                                        richTextBox_Tool_ForthFea.Text = text;
                                        AdjSegmentHsTb.Add(3, fea1);
                                    }

                                }
                                if (adjSegmentIDs.Length == 2)
                                {
                                    richTextBox_Tool_ThirdFea.Visible = false;
                                    label_Tool_ThirdFea.Visible = false;

                                    richTextBox_Tool_ForthFea.Visible = false;
                                    label_Tool_ForthFea.Visible = false;
                                    groupBox_Tool_SltAtt.Height = richTextBox_Tool_SecondFea.Location.Y + richTextBox_Tool_SecondFea.Height + 2;
                                }
                                else if (adjSegmentIDs.Length == 3)
                                {
                                    richTextBox_Tool_ForthFea.Visible = false;
                                    label_Tool_ForthFea.Visible = false;
                                    groupBox_Tool_SltAtt.Height = richTextBox_Tool_ThirdFea.Location.Y + richTextBox_Tool_ThirdFea.Height + 2;
                                }

                                //这时选择图层设为Segment

                                LayerHelper.SelectLayer(axMapControl1, SegmentEntity.SegmentName);
                                NodeSelected = true;


                            }
                        }
                        #endregion 选择是SegmentNode
                        break;
                    }
                case (int)FunEnum.NetworkEditor:
                    {
                        #region 路网编辑
                        if (comboBox_Layer.SelectedItem.Equals("Node"))
                        {
                            //清除上一个点的显示，
                            IGraphicsContainer pGraphicsContainer = axMapControl1.ActiveView as IGraphicsContainer;//把地图的当前view作为图片的容器
                            pGraphicsContainer.DeleteAllElements();
                            //添加新的点
                            IElement pElement = GeoDrawingHelper.DrawPoint(axMapControl1);
                            CrtPnt = pElement.Geometry as IPoint;
                            textBox_node_x.Text = CrtPnt.X.ToString();
                            textBox_node_y.Text = CrtPnt.Y.ToString();
                            textBox_node_z.Text = Convert.ToString(0);
                        }
                        else if (comboBox_Layer.SelectedItem.Equals("Link"))
                        {
                            IEnumFeature pEnumFeat;
                            IFeature pFeature;
                            pEnumFeat = axMapControl1.Map.FeatureSelection as IEnumFeature;
                            pFeature = pEnumFeat.Next();
                            if (pEnumFeat.Next() != null)
                            {
                                MessageBox.Show("选择多于一个点，请重新选择");
                            }

                            //获取选择的图层
                            ILayer pLayer = LayerHelper.GetSelectedLayer(axMapControl1.Map);

                            if (pLayer.Name.Equals(NodeEntity.NodeName))
                            {
                                //确定是link的添加
                                if (Convert.ToInt32(button_Add.Tag) == 2)
                                {
                                    if (pFeature != null)
                                    {
                                        //选择的第一个点是起点
                                        if (SlctPntIndex == 0)
                                        {
                                            FNodeFea = pFeature;
                                            try
                                            {
                                                #region %%%%%%%%%%%%%%%%%%%%%%%%%%%%FNODE的用红色的圆形状%%%%%%%%%%%%%%%%%%%%%%%%%%

                                                GeoDisplayHelper.HightPoint(axMapControl1, FNodeFea.Shape as IPoint, 255, 0, 0, 8, esriSimpleMarkerStyle.esriSMSCircle);

                                                #endregion %%%%%%%%%%%%%%%%%%%%%%%%%%%%FNODE的用红色的圆形状%%%%%%%%%%%%%%%%%%%%%%%%%%
                                            }
                                            catch (Exception ex)
                                            {
                                                MessageBox.Show(ex.ToString());
                                            }

                                            //下一步选这到达的点
                                            SlctPntIndex = 1;
                                        }
                                        //选择的第一个点是终点
                                        else if (SlctPntIndex == 1)
                                        {
                                            #region %%%%%%%%%%%%%%%%%%%%%%%%%%%%TNODE的用红色的钻石形状%%%%%%%%%%%%%%%%%%%%%%%%%%
                                            TNodeFea = pFeature;

                                            GeoDisplayHelper.HightPoint(axMapControl1, TNodeFea.Shape as IPoint, 255, 0, 0, 8, esriSimpleMarkerStyle.esriSMSDiamond);
                                            #endregion %%%%%%%%%%%%%%%%%%%%%%%%%%%%TNODE的用红色的钻石形状%%%%%%%%%%%%%%%%%%%%%%%%%%
                                            //停止选择
                                            SlctPntIndex = 2;

                                            //生成线段
                                            IPointCollection pntCOl = new PolylineClass();

                                            pntCOl.AddPoint(FNodeFea.Shape as IPoint);
                                            pntCOl.AddPoint(TNodeFea.Shape as IPoint);

                                            CrtLine = pntCOl as IPolyline;

                                            #region %%%%%%%%%%%%%%%%%%%%%%%%%%%%之间的Link用蓝色线表示%%%%%%%%%%%%%%%%%%%%%%%%%%
                                            GeoDisplayHelper.HightLine(axMapControl1, CrtLine, 0, 160, 220, 3, esriSimpleLineStyle.esriSLSSolid);
                                            #endregion %%%%%%%%%%%%%%%%%%%%%%%%%%%%之间的Link用蓝色线表示%%%%%%%%%%%%%%%%%%%%%%%%%%
                                        }
                                    }
                                }

                            }
                            else if (pLayer.Name.Equals(LinkEntity.LinkName))
                            {
                                if (pFeature != null)
                                {
                                    CrttLinkFea = pFeature;
                                    CrtLine = pFeature.Shape as IPolyline;
                                }
                            }

                        }
                        break;
                        #endregion 路网编辑
                    }
                case (int)FunEnum.NetworkExtraction:
                    {
                        #region 路网提取
                        IEnumFeature pEnumFea = axMapControl1.Map.FeatureSelection as IEnumFeature;
                        IFeature pFeature = pEnumFea.Next();
                        if (pEnumFea.Next() != null)
                        {
                            MessageBox.Show("选择多于一个Road要素");
                        }
                        else
                        {
                            if (pFeature != null)
                            {
                                if (FeaPairSlctFlag == 0)
                                {
                                    FirstRoadFea = pFeature;
                                    try
                                    {
                                        #region %%%%%%%%%%%%%%%%%%%%%%%%%%%%用红色线表示第一条Road%%%%%%%%%%%%%%%%%%%%%%%%%%

                                        GeoDisplayHelper.HightLine(axMapControl1, FirstRoadFea.Shape as IPolyline, 255, 0, 0, 50, esriSimpleLineStyle.esriSLSDashDotDot);

                                        #endregion %%%%%%%%%%%%%%%%%%%%%%%%%%%%用红色线表示第一条Road%%%%%%%%%%%%%%%%%%%%%%%%%%
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show(ex.ToString());
                                    }

                                    //下一步选这到达的点
                                    button_extaction_AddRoadPair.Enabled = false;
                                    button_extaction_AddRoadFea.Enabled = true;
                                    LayerHelper.ClearSelect(axMapControl1);

                                    _roadFeaPair = new FeaPair(FirstRoadFea, null);

                                    string roadNm = Convert.ToString(FirstRoadFea.get_Value(FeaClsRoad.FindField(Road.RoadNameNm)));
                                    listBox_extraction_roadPair1.Items.Add(roadNm);

                                    FeaPairSlctFlag = -1;
                                }
                                //选择的第一个点是终点
                                else if (FeaPairSlctFlag == 1)
                                {
                                    #region %%%%%%%%%%%%%%%%%%%%%%%%%%%%用蓝色线表示第二条Road%%%%%%%%%%%%%%%%%%%%%%%%%%
                                    SecondRoadFea = pFeature;

                                    GeoDisplayHelper.HightLine(axMapControl1, SecondRoadFea.Shape as IPolyline, 0, 0, 255, 50, esriSimpleLineStyle.esriSLSSolid);




                                    _roadFeaPair = new FeaPair(FirstRoadFea, SecondRoadFea);

                                    //列表中没有，才加入
                                    if (_roadFeaPair.IsExistInFeaPair(RoadFeaPairs) == false)
                                    {
                                        RoadFeaPairs.Add(_roadFeaPair);
                                        string roadNm = Convert.ToString(SecondRoadFea.get_Value(FeaClsRoad.FindField(Road.RoadNameNm)));
                                        listBox_extraction_roadPair2.Items.Add(roadNm);
                                        //停止选择
                                        FeaPairSlctFlag = -1;
                                        button_extaction_AddRoadPair.Enabled = true;
                                        button_extaction_AddRoadFea.Enabled = false;
                                    }
                                    else
                                    {
                                        MessageBox.Show("您已经选择了该Road对，请选择其他Road");
                                        //从新选择
                                        FeaPairSlctFlag = 1;
                                    }
                                    //button_extaction_AddRoadFea.Enabled = true;
                                    //下一步选这到达的点

                                    LayerHelper.ClearSelect(axMapControl1);

                                    #endregion %%%%%%%%%%%%%%%%%%%%%%%%%%%%用蓝色线表示第二条Road%%%%%%%%%%%%%%%%%%%%%%%%%%
                                }
                            }

                        }
                        #endregion 路网提取
                        break;
                    }
                case (int)FunEnum.RuleSetting:
                    {
                        #region 规则设定
                        IEnumFeature pEnumFea = axMapControl1.Map.FeatureSelection as IEnumFeature;
                        IFeature pFeature = pEnumFea.Next();
                        if (pEnumFea.Next() != null)
                        {
                            MessageBox.Show("选择多于一个Road要素");
                        }
                        else
                        {
                            if (pFeature != null)
                            {

                                //刷新所有的被选中的东东
                                IGraphicsContainer pGraphicsContainer = axMapControl1.ActiveView as IGraphicsContainer;//把地图的当前view作为图片的容器
                                pGraphicsContainer.DeleteAllElements();//显示储存在 IElement 中图形，这样就持久化了。
                                axMapControl1.Refresh();

                                GeoDisplayHelper.HightLine(axMapControl1, pFeature.Shape as IPolyline, 255, 0, 0, 10, esriSimpleLineStyle.esriSLSDashDotDot);
                                if (comboBox_Layer.SelectedIndex == 
                                    (int)RoadNetworkSystem.WinForm.RuleSetting.RuleSettingDesigner.RuleRoadItem.车道)
                                {
                                    SlctLane_Rule = pFeature;
                                    ruleSettingDesigner.setLaternConnection();
                                }
                                else if (comboBox_Layer.SelectedIndex == 
                                    (int)RoadNetworkSystem.WinForm.RuleSetting.RuleSettingDesigner.RuleRoadItem.有向子路段)
                                {
                                    SlctArc_Rule = pFeature;
                                }
                            }
                        }
                        #endregion 规则设定
                        break;
                    }
                default:
                    break;
            }
        }

        private void 动态分段ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*
             * 高速公路动态分段在NetworkExtraction\FreeWay包中
             * 
             * 1.给每条高速公路Road添加一个SegNode，作为历程桩(在ArcMap中绘制)
             * 2.标记Road车辆行驶方向与里程增长的方向，相同1，相反-1(在ArcMap中标记)
             * 3.分别按照Road方向1和-1方向，打断Road，生成新的Segment（生成SegNode，给桩号）
             * 4.在Node处打断Segment
             * 5.在Segment的断点处，生成SegNode
            */


           
            Segmentation segmentation = new Segmentation(MdbPath);
            segmentation.SegmentRoad();

            MessageBox.Show("OK");

        }

        private void 新建车道ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //IFeatureCursor cursor = 
            LaneService laneService = new LaneService(MdbPath);
            laneService.CreateLane();
            MessageBox.Show("OK");
        }

        private void 车道连接器ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //先要构架在Node处的Arc连通关系

            ConnectorService conService = new ConnectorService(MdbPath);
            conService.CreateConnector();
            MessageBox.Show("OK");
        }

        private void 规则ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            funFlag = (int)FunEnum.RuleSetting;

            ruleSettingDesigner = new RuleSettingDesigner(this);
            ruleSettingDesigner.SetRuleSettingPlatte();

            int dis = splitContainer3.SplitterDistance;
            int spP2W = splitContainer4.Panel2.Width;
            button1.Text = ">>";
            splitContainer4.Panel2Collapsed = false;
            splitContainer3.SplitterDistance = this.Width * 3 / 5;
        }

    }
}
