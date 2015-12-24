using ESRI.ArcGIS.Geodatabase;
using RoadNetworkSystem.GIS.GeoDatabase.WorkSpace;
using RoadNetworkSystem.WinForm.NetworkExtraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadNetworkSystem.NetworkExtraction.Road2BasicRoadNetwork
{
    class Road2BasicRoadNetwork
    {
        private Form1 _frm1;
        IFeatureClass _feaClsRoad;
        IFeatureClass _feaClsSegment;

        public Road2BasicRoadNetwork(Form1 frm1)
        {
            this._frm1 = frm1;
            this._feaClsRoad = frm1.FeaClsRoad;
            this._feaClsSegment = frm1.FeaClsSegment;
        }


        public void Convert2BasicRoadNetwork()
        {
            //处理的时候，均默认为双向的
            //多线段转为直线段

            /*
             * 1. Road -> Segment层
             *      利用“路网提取-中心线到路段路网”提取出Segment层
             * 2. Segment层 ->   Link层  
             *      交通组织中断处打断
             *      多线段转为直线段
             *      
             * 3. Link层 ->  Lane层
             * 
             */
            
            //1
            //把原始的mdb中的BreakPoint,LaneNumChange移到新的数据库中
            ExtractionDesigner extrctDsgnr = new ExtractionDesigner(_frm1);
            extrctDsgnr.CopyFlag = (int)ExtractionDesigner.CopyFeatureClassAndTable.CopyForRoad2BasicNetwork;
            _frm1.TriggerRoadNetworkExtractor(extrctDsgnr);

            //2
            breakSegmentInTrafficDisturb();
            breakSegmentInKinkPoint();
            
        }


        

        /// <summary>
        /// 在交通组织中断处打断
        /// </summary>
        private void breakSegmentInTrafficDisturb()
        {
            //BreakPoint,LaneNumChange
            IQueryFilter filter = new QueryFilterClass();
            filter.WhereClause = "";
            IFeatureCursor query = _feaClsRoad.Search(filter, false);
            IFeature pFeatureRoad = query.NextFeature();
            while (pFeatureRoad != null)
            {
                
                pFeatureRoad = query.NextFeature();
            }

        }

        /// <summary>
        /// 多线段转为直线段
        /// </summary>
        private void breakSegmentInKinkPoint()
        { 
        }
             

    }
}
