using ESRI.ArcGIS.Geodatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadNetworkSystem.NetworkEditor
{
    /// <summary>
    /// 要素对
    /// </summary>
    public class FeaPair
    {
        private IFeature feature1;
        private IFeature feature2;

        public FeaPair(IFeature fea1, IFeature fea2)
        {
            feature1 = fea1;
            feature2 = fea2;
        }

        public IFeature Fea1
        {
            get { return feature1; }
            //set { feature1 = value; }
        }

        public IFeature Fea2
        {
            get { return feature2; }
            //set { feature2 = value; }
        }

        /// <summary>
        /// 检查List<FeaPair>是否含有某个FeaPair，有返回true，否则返回false
        /// </summary>
        /// <param name="feaPair"></param>指定的FeaPair
        /// <param name="feaPairs"></param>检查集
        /// <returns></returns>
        public bool IsExistInFeaPair(List<FeaPair> feaPairs)
        {
            bool checkFlag = false;
            foreach (FeaPair item in feaPairs)
            {
                int feaOID1 = item.feature1.OID;
                int feaOID2 = item.feature2.OID;
                if ((feaOID1 == feature1.OID) && (feaOID2 == feature2.OID))
                {
                    checkFlag = true;
                }
                else if (feaOID1 == feature2.OID && feaOID2 == feature1.OID)
                {
                    checkFlag = true;
                }
            }
            return checkFlag;
        }


    }
}
