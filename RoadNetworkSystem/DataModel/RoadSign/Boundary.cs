using System;
using System.Collections.Generic;
using System.Text;

namespace RoadNetworkSystem.DataModel.RoadSign
{
    class Boundary
    {
        public const int DASHBOUNSTYLE = -225;
        public const int SOLIDBOUNSTYLE = -227;
        public const int OUTSIDEBOUNSTYLE = 227;   //边界线
        //private const int TURNARROWSTYLE = 227;
        public const int CENTERLINESTYLE = 241;


        /// <summary>
        /// 规定了数据模型，请不要在其他类中直接读取数据
        /// </summary>
        public const string BoundaryName = "Boundary";

        public int BoundaryID { get; set; }
        public int StyleID { get; set; }
        public int Dir { get; set; }
        public int Other { get; set; }


        public const string BOUNDARYID_NAME = "BoundaryID";
        public const string STYLEID_NAME = "StyleID";
        public const string DIR_NAME = "Dir";
        public const string OTHER_NAME = "Other";
  
        
        /// <summary>
        /// 复制实体
        /// </summary>
        /// <returns></returns>
        public Boundary Copy()
        {
            Boundary boundary = new Boundary();

            boundary.BoundaryID = this.BoundaryID;           
            boundary.Dir = this.Dir;
            boundary.StyleID = this.StyleID;

            boundary.Other = this.Other;

            return boundary;
        }
    }
}
